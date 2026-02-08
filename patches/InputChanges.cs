using HarmonyLib;

namespace Multiplayer;

public class InputChanges : Global
{
	public static bool SwapCalledFromPacket = false;

	[HarmonyPatch]
	private class InputPatch
    {
		static bool PreviousReadyState = false;
		static bool SelfPreviousReadyState = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(RDInput), nameof(RDInput.Update))]
		public static void UpdatePostfix()
        {
            if (scnBase.instance is not scnGame || !Lobby.InMultiplayer)
			{
				SelfPreviousReadyState = false;
				return;
			}

			bool gameStarted = scnGame.instance.startTheGameCalled;
			bool otherReady = !gameStarted && MultiplayerState.OtherReady;
			bool change = !gameStarted && MultiplayerState.OtherReady != PreviousReadyState;

			RDInput.p1IsPressed = RDInput.anyPlayerIsPressed;
			RDInput.p1Press = RDInput.anyPlayerPress;
			RDInput.p1Release = RDInput.anyPlayerRelease;

			RDInput.p2IsPressed = otherReady;
			RDInput.p2Press = change && otherReady;
			RDInput.p2Release = change && !otherReady;

			if (!gameStarted && SelfPreviousReadyState != RDInput.p1IsPressed)
			{
				SelfPreviousReadyState = RDInput.p1IsPressed;
                Lobby.SendPacket(new(PacketType.SetReadyStatus)
                {
					IsReady = RDInput.p1IsPressed
                });
			}

			if (MultiplayerState.SelfPlayer == RDPlayer.P2 && !MultiplayerState.Versus)
            {
                (RDInput.p1IsPressed, RDInput.p2IsPressed) = (RDInput.p2IsPressed, RDInput.p1IsPressed);
                (RDInput.p1Press, RDInput.p2Press) = (RDInput.p2Press, RDInput.p1Press);
                (RDInput.p1Release, RDInput.p2Release) = (RDInput.p2Release, RDInput.p1Release);
            }

			PreviousReadyState = MultiplayerState.OtherReady;
        }

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RDInput), nameof(RDInput.SwapP1AndP2Controls))]
		public static bool SwapPrefix()
		{
			if (scnBase.instance is not scnGame || scnGame.instance.startTheGameCalled)
				return true;
			if (InVersus)
				return false;
			if (Lobby.InMultiplayer && !SwapCalledFromPacket)
			{
				MultiplayerState.SwappedPlayers = !MultiplayerState.SwappedPlayers;
				Lobby.SendPacket(new(PacketType.SetSwappedStatus)
				{
					IsSwapped = MultiplayerState.SwappedPlayers
				});
			}
			SwapCalledFromPacket = false;
			return true;
		}
    }
}