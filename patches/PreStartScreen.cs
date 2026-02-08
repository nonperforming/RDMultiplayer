using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer;

public class PreStartScreen : Global
{
	public static bool Original2PMode = false;
	public static bool PreviousInMultiplayer = false;

	static GameObject P1Mark;
	static GameObject P2Mark;
	static GameObject SwitchPlayersKeys;

	static TweenerCore<Color, Color, ColorOptions> P1MarkColorTween;
	static TweenerCore<Color, Color, ColorOptions> P2MarkColorTween;

	[HarmonyPatch(typeof(scnGame), "Awake")]
	private class CreateLobbyPatch
    {
        public static void Prefix(scnGame __instance)
        {
			if (!__instance.editorMode && scnGame.levelToLoadSource != LevelSource.CutscenesPath)
			{
				// Lobby.CreateLobby(Multiplayer.PrivateLobby.Value);
				if (!Lobby.InLobby)
				{
					Lobby.CreateLobby(false);
					MultiplayerState.CanPauseInVersus = Multiplayer.CanPauseInVersus.Value;
					MultiplayerState.SharePause = Multiplayer.SharePause.Value;
				}
				scnGame.attemptToLoadTutorial = false;
				PreviousInMultiplayer = false;
				
				MultiplayerState.Versus = !GC.twoPlayerMode;
				MultiplayerState.GotOtherPlayerRankInformation = false;
				MultiplayerState.OtherReady = false;
				MultiplayerState.SwappedPlayers = false;

				RankscreenDisplay.WaitingForData = false;
				RankscreenDisplay.IntendedRankSequence = 0;
				RankscreenDisplay.SentData = false;

				P1Mark = __instance.p1MarkImage.gameObject;
				P2Mark = __instance.p2MarkImage.gameObject;
				SwitchPlayersKeys = __instance.switchPlayersContainer.transform.Find("SwitchPlayers").gameObject;
			}
		}
    }

	[HarmonyPatch]
    private class Faux2PPatch
    {

		[HarmonyPrefix]
		[HarmonyPatch(typeof(scnGame), "Update")]
		[HarmonyPatch(typeof(RDArm), "Update")]
		[HarmonyPatch(typeof(scrHandController), nameof(scrHandController.ShowHandButtons))]
		[HarmonyPatch(typeof(scrHandController), nameof(scrHandController.SlideIn))]
		public static void UpdatePrefix(RDBase __instance)
        {
			Original2PMode = GC.twoPlayerMode;
			if (__instance.game == null || __instance.game.startTheGameCalled || !Lobby.InMultiplayer)
				return;
			GC.twoPlayerMode = true;
        }

		[HarmonyPostfix]
		[HarmonyPatch(typeof(scnGame), "Update")]
		[HarmonyPatch(typeof(RDArm), "Update")]
		[HarmonyPatch(typeof(scrHandController), nameof(scrHandController.ShowHandButtons))]
		[HarmonyPatch(typeof(scrHandController), nameof(scrHandController.SlideIn))]
		public static void UpdatePostfix(RDBase __instance)
        {
			// We don't dare touch anything if it's in the editor.
			if (__instance.editorMode)
				return;

			if (__instance is scnGame game && !game.startTheGameCalled)
			{
				scrHandController hands = game.GetHandControllerFromInt(4);
				bool leftArmActive = hands.leftArm.button.gameObject.activeSelf;
				if (!Lobby.InMultiplayer)
                {
					if (PreviousInMultiplayer)
                    {
						game.EnsureSwitchPlayersState();
						MultiplayerState.SwappedPlayers = false;
						game.statusText.SetStatusText((string)AccessTools.Field(typeof(scnGame), "beginLevel").GetValue(game));
						game.statusText.hideTextAfterSomeTime = false;
                    }

					game.switchPlayersContainer.SetActive(GC.twoPlayerMode);
					P1Mark.SetActive(GC.twoPlayerMode);
					P2Mark.SetActive(GC.twoPlayerMode);
					SwitchPlayersKeys.SetActive(GC.twoPlayerMode);

					if (GC.twoPlayerMode)
                    {
						bool isTweeningP1 = DOTween.IsTweening(game.p1MarkImage);
						if (isTweeningP1 && (P1MarkColorTween?.IsPlaying() ?? false))
                        {
                            P1MarkColorTween = null;
                            game.p1MarkImage.DOKill();
							isTweeningP1 = false;
                        }
						if (!isTweeningP1)
							game.p1MarkImage.color = game.gc.player1StripColor;

						bool isTweeningP2 = DOTween.IsTweening(game.p2MarkImage);
						if (isTweeningP2 && (P2MarkColorTween?.IsPlaying() ?? false))
                        {
                            P2MarkColorTween = null;
                            game.p2MarkImage.DOKill();
							isTweeningP2 = false;
                        }
						if (!isTweeningP2)
							game.p2MarkImage.color = game.gc.player2StripColor;
                    }
					else if (leftArmActive)
                    {
                        hands.leftArm.moveButtonWithArm = hands.rightArm.moveButtonWithArm = true;
						hands.leftArm.player = RDPlayer.P2;
						hands.rightArm.player = RDPlayer.P1;
						hands.ShowHandButtons();
						hands.UpdateHandsPositions(HandUpdateType.RightHand);
                    }
                    goto EndIf;
                }

				Text extraCredits = game.rankscreen.extraCredits;
				string help = LanguageMap.Get("multiplayer.help");
				string left = LanguageMap.Get("multiplayer.otherLeft.preAck");

				if (extraCredits.text.Contains("\n"))
					extraCredits.text = extraCredits.text.Replace("\n" + help, "").Replace("\n" + left, "");
				extraCredits.text = extraCredits.text.Replace(help, "").Replace(left, "");

				string status = LanguageMap.Get("multiplayer.status.hold");
				if (game.statusText.message.text != status)
				{
                    game.statusText.SetStatusText(status);
					game.statusText.hideTextAfterSomeTime = false;
				}
				
				game.switchPlayersContainer.SetActive(true);
				P1Mark.SetActive(true);
				P2Mark.SetActive(InCoop);
				SwitchPlayersKeys.SetActive(InCoop);

				if (InCoop)
                {
                    if (!DOTween.IsTweening(game.p1MarkImage))
						game.p1MarkImage.color = MultiplayerState.SelfPlayer == RDPlayer.P1 ? game.gc.player1StripColor : Color.grey;
                    if (!DOTween.IsTweening(game.p2MarkImage))
						game.p2MarkImage.color = MultiplayerState.SelfPlayer == RDPlayer.P2 ? game.gc.player2StripColor : Color.grey;
                }
				else 
				{
					game.p1Mark.anchoredPosition = game.rightMarkPos;
					if (leftArmActive)
						goto EndIf;
					hands.leftArm.moveButtonWithArm = hands.rightArm.moveButtonWithArm = true;
					hands.leftArm.player = RDPlayer.P2;
					hands.rightArm.player = RDPlayer.P1;
					hands.ShowHandButtons();
					hands.UpdateHandsPositions(HandUpdateType.BothHands);
				}
			}

		EndIf:
			GC.twoPlayerMode = Original2PMode;
			PreviousInMultiplayer = Lobby.InMultiplayer;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(scnGame), nameof(scnGame.StartTheGame))]
		public static void StartPrefix(scnGame __instance)
		{
			GC.twoPlayerMode = Original2PMode;
			if (!Lobby.InMultiplayer)
				return;
				
			__instance.switchPlayersContainer.SetActive(value: false);
			if (__instance.paused)
			{
				MultiplayerPauseChanges.PauseCalledFromPacket = true;
				__instance.TogglePauseGame();
			}

			if (Lobby.InMultiplayer)
			{
				if (Lobby.IsHost)
					SteamMatchmaking.SetLobbyJoinable(Lobby.CurrentLobbySteamID, false);
				Lobby.SendPacket(new(PacketType.StartLevel));
			}
			else if (Lobby.InLobby)
				Lobby.LeaveLobby();
		}
	}

	[HarmonyPatch(typeof(Persistence), nameof(Persistence.SetSwapP1AndP2Controls))]
	private class SaveSwapPatch
    {
        public static bool Prefix()
			=> !InCoop;
    }

	[HarmonyPatch(typeof(scnGame), nameof(scnGame.SwitchPlayers))]
	private class GreyTweenPatch
    {
        public static void Postfix(scnGame __instance)
        {
            if (!InCoop)
				return;
			if (MultiplayerState.OtherPlayer == RDPlayer.P1)
            {
                __instance.p1MarkImage.DOKill();
				P1MarkColorTween = __instance.p1MarkImage.DOColor(Color.grey, 0.3f);
            }
			else
            {
                __instance.p2MarkImage.DOKill();
				P2MarkColorTween = __instance.p2MarkImage.DOColor(Color.grey, 0.3f);
            }
        }
    }

	[HarmonyPatch]
	private class ExtraCreditsHelpPatch
    {
		static bool ExtraCreditsTextShown = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(scnGame), "LoadingRoutine")]
        public static IEnumerator LoadingPostfix(IEnumerator __result, scnGame __instance)
        {
			ExtraCreditsTextShown = false;

            while (__result.MoveNext())
                yield return __result.Current;  

			if (Multiplayer.ShowHelpText.Value && !__instance.editorMode)
			{
				if (ExtraCreditsTextShown)
				{
					__instance.rankscreen.extraCredits.text += "\n" + LanguageMap.Get("multiplayer.help");
					Narration.Say(LanguageMap.Get("Multiplayer.help"), NarrationCategory.Notification, false);
				}
				else
					__instance.rankscreen.ShowExtraCredits(LanguageMap.Get("multiplayer.help"));
			}
        }

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowExtraCredits))]
		public static void ExtraCreditsPostfix()
			=> ExtraCreditsTextShown = true;
    }
}