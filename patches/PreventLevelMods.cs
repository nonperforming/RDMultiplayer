using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using RDLevelEditor;
using UnityEngine;

namespace Multiplayer;

public class PreventLevelMods : Global
{
	public static bool MontageDisplayed = false;

	[HarmonyPatch]
    private class PreventFunctionsPatch
    {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevel))]
		[HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevelLite))]
		[HarmonyPatch(typeof(scnGame), nameof(scnGame.LevelFailSequence))]
        public static bool PreventFunction()
			=> !Lobby.InMultiplayer;
    }

	[HarmonyPatch(typeof(scnGame), nameof(scnGame.StartTheGame))]
	private class BossPatch
    {
        public static void Prefix(scnGame __instance)
        {
            if (!Lobby.InMultiplayer || __instance.currentLevel.levelType != LevelType.Boss)
				return;
			__instance.currentLevel.levelType = LevelType.Regular;
			MontageDisplayed = false;
		}
	}

	[HarmonyPatch]
	private class HPPatch
    {
		[HarmonyPostfix]
		[HarmonyPatch(typeof(LevelBase), nameof(LevelBase.Is1HP))]
		[HarmonyPatch(typeof(Level_OrientalInsomniac), nameof(Level_OrientalInsomniac.Is1HP))]
        public static void Is1HPPostfix(ref bool __result)
			=> __result &= !Lobby.InMultiplayer;
    }

	[HarmonyPatch]
	private class MontagePatch
    {

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Level_Montage), nameof(Level_Montage.PrepNextLevel))]
        public static bool PrepPrefix(Level_Montage __instance)
        {
            if (!Lobby.InMultiplayer)
				return true;
			if (MontageDisplayed)
				return false;
			__instance.game.StartCoroutine(DisplayRankscreen(__instance.game.rankscreen));
			return false;
        }

		static IEnumerator DisplayRankscreen(Rankscreen rankscreen)
        {
			MontageDisplayed = true;

            rankscreen.AdvanceGameover();
			yield return new WaitForSeconds(5f);
			rankscreen.AdvanceGameover();
			yield return new WaitForSeconds(5f);
			rankscreen.AdvanceGameover();
        }

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Level_Montage), nameof(Level_Montage.FailBoss))]
		[HarmonyPatch(typeof(Level_Montage2), nameof(Level_Montage2.FailBoss))]
		public static bool MPrefix(LevelBase __instance)
        {
			if (!Lobby.InMultiplayer)
				return true;
			if (__instance is Level_Montage montage && montage.b9)
                montage.PrepNextLevel();

            __instance.UpdateAllHearts();
			return false;
        }
	}

	[HarmonyPatch(typeof(LevelBase), MethodType.Constructor, [typeof(RDLevelData)])]
	private class StartImmediatelyPatch
    {
        public static void Postfix(LevelBase __instance)
			=> __instance.startImmediately &= Multiplayer.RespectStartImmediately.Value;
    }

	[HarmonyPatch(typeof(LevelBase), MethodType.Constructor, [])]
	private class DefaultRankMarginTextPatch
    {
        public static void Postfix(LevelBase __instance)
			=> __instance.rankDescriptions = [
                RDString.Get("editor.LevelSettings.defaultF"),
                RDString.Get("editor.LevelSettings.defaultD"),
                RDString.Get("editor.LevelSettings.defaultC"),
                RDString.Get("editor.LevelSettings.defaultB"),
                RDString.Get("editor.LevelSettings.defaultA"),
                RDString.Get("editor.LevelSettings.defaultS")
            ];
    }

	[HarmonyPatch(typeof(Level_Lesmis), nameof(Level_Lesmis.preactions))]
	private class LesmisPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
			MethodInfo getMistakesMethod = AccessTools.PropertyGetter(typeof(MistakesManager), nameof(MistakesManager.mistakes));

			foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.OperandIs(getMistakesMethod))
                {
					instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(LesmisPatch), nameof(GetMistakes));
                }
				yield return instruction;
            }
        }

		public static float GetMistakes(MistakesManager mistakesManager)
        {
            if (!Lobby.InMultiplayer)
				return mistakesManager.mistakes;
			return 0f;
        }
    }

}