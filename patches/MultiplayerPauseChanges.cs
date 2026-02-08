using HarmonyLib;
using UnityEngine;

namespace Multiplayer;

public class MultiplayerPauseChanges : Global
{
	public static bool PauseCalledFromPacket = false;

	[HarmonyPatch(typeof(scnGame), nameof(scnGame.TogglePauseGame))]
    private class PausePatch
    {
		public static bool Prefix(scnGame __instance)
        {
			if (scnGame.pauseBlocked)
				return false;
			if (!Lobby.InMultiplayer)
				return true;
			if (PauseCalledFromPacket)
				return !(PauseCalledFromPacket = false);
            if (InVersus && !MultiplayerState.CanPauseInVersus && __instance.startTheGameCalled)
			{
				LEDSign.status = LanguageMap.Get("multiplayer.pausingDisabled");
				return false;
			}

			if (!MultiplayerState.SharePause || !__instance.startTheGameCalled)
				return true;
			// Pause will switch after this so our packet must be the opposite of the current state
			Lobby.SendPacket(new(PacketType.SetPaused)
            {
				IsPaused = !__instance.paused
            });
			return true;
        }
    }

	[HarmonyPatch(typeof(PauseMenuMode), "CheckButtonsVisibility")]
	private class RestartFromCheckpointPatch
    {
        public static void Postfix(PauseMenuMode __instance, GameObject ___restartFromCheckpoint)
        {
            if (!Lobby.InMultiplayer || !(___restartFromCheckpoint?.activeSelf ?? false))
				return;
			__instance.hiddenContent--;
			___restartFromCheckpoint.SetActive(false);
        }
    }
}