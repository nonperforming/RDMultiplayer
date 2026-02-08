using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Multiplayer;

public class RandomSeeding : Global
{
	public static Random.State Seed;

	[HarmonyPatch(typeof(LevelBase), nameof(LevelBase.EvalStringWithVariables))]
	private class RandomPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo originalRandomMethod = AccessTools.Method(typeof(Random), nameof(Random.Range), [typeof(int), typeof(int)]);
			MethodInfo customRandomMethod = AccessTools.Method(typeof(RandomPatch), nameof(RandomSeeded), [typeof(int), typeof(int)]);

			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.OperandIs(originalRandomMethod))
					instruction.operand = customRandomMethod;
				yield return instruction;
			}
		}

		public static int RandomSeeded(int min, int max)
		{
			if (!Lobby.InMultiplayer)
				return Random.Range(min, max);
				
			Random.State temp = Random.state;
			Random.state = Seed;

			int ret = Random.Range(min, max);
			Seed = Random.state;

			Random.state = temp;
			return ret;
		}
	}

	public static Random.State CreateState(int s0, int s1, int s2, int s3)
	{
		// Bad code apparently. Idc though because either this or nothing
		Random.State state = new();
		TypedReference stateRef = __makeref(state);
		AccessTools.Field(typeof(Random.State), "s0").SetValueDirect(stateRef, s0);
		AccessTools.Field(typeof(Random.State), "s1").SetValueDirect(stateRef, s1);
		AccessTools.Field(typeof(Random.State), "s2").SetValueDirect(stateRef, s2);
		AccessTools.Field(typeof(Random.State), "s3").SetValueDirect(stateRef, s3);
		return state;
	}

	public static int[] GetState(Random.State state)
		=> [
			(int)AccessTools.Field(typeof(Random.State), "s0").GetValue(state),
			(int)AccessTools.Field(typeof(Random.State), "s1").GetValue(state),
			(int)AccessTools.Field(typeof(Random.State), "s2").GetValue(state),
			(int)AccessTools.Field(typeof(Random.State), "s3").GetValue(state)
		];
}