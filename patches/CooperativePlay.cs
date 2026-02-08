using System;
using HarmonyLib;
using JetBrains.Annotations;
using RDLevelEditor;
using Steamworks;

namespace Multiplayer;

public class CooperativePlay : Global
{
	[HarmonyPatch(typeof(Row), nameof(Row.cpuControlled), MethodType.Getter)]
    private class CPUPatch
    {
        public static void Postfix(Row __instance, ref bool __result)
        {
            if (!InCoop)
				return;
			bool isOtherPlayer = __instance.GetCurrentPlayer() == MultiplayerState.OtherPlayer;
			if (isOtherPlayer)
			{
				__instance.ent.characterMarkerAnimation = null;
				__instance.cpuCharacter = Character.Beans;
				__instance.cpuCharacterToChangeInto = Character.Beans;
			}
			__result |= isOtherPlayer;
		}
    }

	[HarmonyPatch]
	private class HitstripPatch
    {
		static RDPlayer MostRecentPlayer;

        [HarmonyPrefix]
		[HarmonyPatch(typeof(HitStripManager), nameof(HitStripManager.ShowHitstrip))]
		public static void ShowHitstripPrefix(ref RDPlayer player)
        {
            if (player != RDPlayer.CPU || !InCoop)
				return;
			player = MostRecentPlayer;
        }

        [HarmonyPrefix]
		[HarmonyPatch(typeof(Beat), nameof(Beat.Update))]
		public static void BeatUpdatePrefix(Beat __instance)
        	=> MostRecentPlayer = __instance.row.GetCurrentPlayer();
    }	

	[HarmonyPatch]
	private class ArmPatch
    {
		[HarmonyPostfix]
		[HarmonyPatch(typeof(RDArm), nameof(RDArm.SetToCharacter))]
		public static void SetToCharacterPostfix(RDArm __instance, Character character)
        {
            if (character != Character.Player || !InCoop)
				return;
			__instance.playerCanUse = __instance.player == MultiplayerState.SelfPlayer;
			__instance.cpuCanUse = !__instance.playerCanUse;
			__instance.cpuOwner = Character.Beans; 
        }
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(scrHandController), nameof(scrHandController.cpuHandPress))]
		public static void CPUPrefix(scrHandController __instance, Character cpuRowOwner)
        {
			if (!InCoop || !__instance.gameObject.activeSelf)
				return;

			patchArm(__instance.leftArm);
			patchArm(__instance.rightArm);

			static void patchArm(RDArm arm)
			{
				if (!arm.playerCanUse || arm.player != MultiplayerState.OtherPlayer)
					return;

				arm.playerCanUse = false;
				arm.cpuCanUse = true;
				arm.cpuOwner = Character.Beans;
			}
		} 
    }	

	[HarmonyPatch]
	private class ChangeRowsPatch
    {
        [HarmonyPostfix]
		[HarmonyPatch(typeof(scnGame), "ImplementRowPropPlayerChange")]
		public static void Postfix(scnGame __instance, RDPlayer[] arrRowPlayers, int indexOffset)
			=> Do(__instance, arrRowPlayers, indexOffset, false);

        [HarmonyPostfix]
		[HarmonyPatch(typeof(scnGame), "ImplementRowPropPlayerOnBarStartChange")]
		public static void OnBarStartChangePostfix(scnGame __instance, RDPlayer[] arrRows, int indexOffset)
			=> Do(__instance, arrRows, indexOffset, true);
    
		public static void Do(scnGame game, RDPlayer[] playerRows, int indexOffset, bool onBarStartChange)
        {
			if (!InCoop)
				return;
            for (int i = 0; i < playerRows.Length; i++)
            {
                if (playerRows[i] != MultiplayerState.OtherPlayer)
					continue;

				Row row = game.rows[indexOffset + i];
				if (onBarStartChange)
					row.cpuCharacterToChangeInto = Character.Beans;
				else
					row.cpuCharacter = Character.Beans;
            }
        }
	}
}