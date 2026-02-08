using System;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer;

public class VersusRankscreenDisplay : Global
{
	public static Text OtherRank;
	public static Text OtherRankDescription;
	public static bool RankscreenInVersus => RankscreenDisplay.MultiplayerRankscreen && !GC.twoPlayerMode;

	[HarmonyPatch]
	private class RankPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowHeaderRankText))]
		public static void ShowHeaderPostfix(Rankscreen __instance)
		{ 
			if (!RankscreenInVersus)
				return;
			__instance.header.text = LanguageMap.Get("multiplayer.rankscreen.winnerSuspense");
			Narration.Say(LanguageMap.Get("multiplayer.narration.versusRank"), NarrationCategory.Notification, false);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowAndSaveRank))]
		public static void ShowRankPostfix(Rankscreen __instance)
		{
			if (!RankscreenInVersus)
				return;
			OtherRank = UnityEngine.Object.Instantiate(__instance.rank, __instance.rank.gameObject.transform.position, Quaternion.identity, __instance.rank.gameObject.transform.parent);

			RectTransform baseRankRT = __instance.rank.GetComponent<RectTransform>();
			RectTransform otherRankRT = OtherRank.GetComponent<RectTransform>();

			otherRankRT.anchoredPosition = baseRankRT.anchoredPosition3D.AddX(352 / 4);
			baseRankRT.anchoredPosition = baseRankRT.anchoredPosition3D.AddX(-352 / 4);
			OtherRank.text = GetOtherRank().ToString();
			Narration.Say(RDString.Get("narration.rank." + __instance.game.currentLevel.GetRankFromMistakes().ToString()), NarrationCategory.Notification, false);
			Narration.Say(RDString.Get("narration.rank." + GetOtherRank().ToString().ToString()), NarrationCategory.Notification, false);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowRankDescription))]
		public static void ShowRankDescriptionPostfix(Rankscreen __instance)
		{
			if (!RankscreenInVersus)
				return;
			scnGame game = __instance.game;
			MistakesManager mistakesManager = game.mistakesManager;

			__instance.descriptionLayoutGroup.enabled = false;
			OtherRankDescription = UnityEngine.Object.Instantiate(__instance.description, __instance.description.gameObject.transform.position, Quaternion.identity, __instance.description.gameObject.transform.parent);

			RectTransform baseDescriptionRT = __instance.description.GetComponent<RectTransform>();
			RectTransform otherDescriptionRT = OtherRankDescription.GetComponent<RectTransform>();

			otherDescriptionRT.anchoredPosition = baseDescriptionRT.anchoredPosition3D.AddX(352 / 4).AddY(-12f);
			baseDescriptionRT.anchoredPosition = baseDescriptionRT.anchoredPosition3D.AddX(-352 / 4).AddY(-12f);
			baseDescriptionRT.SizeDeltaX(176f);
			otherDescriptionRT.SizeDeltaX(176f);
			OtherRankDescription.text = __instance.game.currentLevel.rankDescriptions[GetOtherRank().ToNormal()];

			__instance.resultsSingleplayer.gameObject.SetActive(false);
			__instance.multiplayerResultsLayoutGroup.gameObject.SetActive(true);
			__instance.multiplayerResultsLayoutGroup.transform.position = __instance.multiplayerResultsLayoutGroup.transform.position.AddY(-12f);
			__instance.resultsP1.gameObject.SetActive(true);
			__instance.resultsP2.gameObject.SetActive(true);

			string selfName = SteamFriends.GetPersonaName();
			string otherName = SteamFriends.GetFriendPersonaName(Lobby.OtherUser.GetSteamID());
			string baseText = $"[name]:\n{RDString.Get("rankscreen.mistakes")}\n{RDString.Get("rankscreen.frameOffsetsDescriptionShort")}";
			int otherTotalOffset = MultiplayerState.OtherEarlyOffset + MultiplayerState.OtherLateOffset;

			__instance.resultsP2.text = baseText.Replace("[name]", selfName)
												.Replace("[mistakes]", Math.Round(mistakesManager.mistakes, 4).ToString())
												.Replace("[earlyAmount]", mistakesManager.earlyOffsetsSum.ToString())
												.Replace("[lateAmount]", mistakesManager.lateOffsetsSum.ToString())
												.Replace("[totalAmount]", mistakesManager.totalOffsetsSum.ToString());

			__instance.resultsP1.text = baseText.Replace("[name]", otherName)
												.Replace("[mistakes]", Math.Round(MultiplayerState.OtherMistakes, 4).ToString())
												.Replace("[earlyAmount]", MultiplayerState.OtherEarlyOffset.ToString())
												.Replace("[lateAmount]", MultiplayerState.OtherLateOffset.ToString())
												.Replace("[totalAmount]", otherTotalOffset.ToString());

			bool tie = mistakesManager.mistakes == MultiplayerState.OtherMistakes
					&& mistakesManager.totalOffsetsSum == otherTotalOffset;
			bool selfWin = mistakesManager.mistakes < MultiplayerState.OtherMistakes;
			if (mistakesManager.mistakes == MultiplayerState.OtherMistakes)
				selfWin = mistakesManager.totalOffsetsSum < otherTotalOffset;
			if (tie)
				__instance.header.text = LanguageMap.Get("multiplayer.rankscreen.tie");
			else
				__instance.header.text = LanguageMap.Get("multiplayer.rankscreen.winner").Replace("[name]", selfWin ? selfName : otherName);

			Narration.Say(
			string.Join("\n",
			__instance.resultsP2.text,
			__instance.description.text,
			__instance.resultsP1.text,
			OtherRankDescription.text,
			 __instance.header.text),

			 NarrationCategory.Notification, false, NarrationActionName.ToContinue);
		}
	}

	[HarmonyPatch]
	private class NarrationPatch
	{
		public static bool NarrationDisabled = false;

		[HarmonyPatch(typeof(Narration), nameof(Narration.Say))]
		public static bool NarrationPrefix()
			=> !NarrationDisabled;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowHeaderRankText))]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowAndSaveRank))]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowRankDescription))]
		public static void Prefix()
			=> NarrationDisabled = RankscreenInVersus;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowHeaderRankText))]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowAndSaveRank))]
		[HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowRankDescription))]
		public static void Postfix()
			=> NarrationDisabled = false;
	}

	public static Rank GetOtherRank()
	{
		scnGame game = scnGame.instance;
		if (game == null)
			return Rank.NotAvailable;

		float mistakes = MultiplayerState.OtherMistakes;
		float[] bounds = game.currentLevel.rankLowerBounds;
		Rank rank = Rank.F;
		for (int i = 1; i < 5; i++)
			if (mistakes <= bounds[i])
				rank++;
		if (mistakes == 0)
			rank = Rank.S;

		int totalHits = MultiplayerState.OtherHitsInPlusZone +
						MultiplayerState.OtherHitsInNormalZone +
						MultiplayerState.OtherHitsInMinusZone;
		int halfHits = totalHits / 2;
		if (totalHits == 0 || MultiplayerState.OtherHitsInPlusZone > halfHits)
			rank = rank.ToPlus();
		else if (MultiplayerState.OtherHitsInMinusZone > halfHits)
			rank = rank.ToMinus();

		return rank;
	}
}