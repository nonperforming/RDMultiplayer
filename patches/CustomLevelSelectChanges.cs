using System.Collections;
using HarmonyLib;

namespace Multiplayer;

public class CustomLevelSelectChanges : Global
{
	public static string LastPlayedCLSLevel = null;
	public static string ToDownload = null;

	[HarmonyPatch]
    private class MainPatch
    {
		[HarmonyPostfix]
		[HarmonyPatch(typeof(scnCLS), "Start")]
		public static void StartPostfix(scnCLS __instance)
		{
			if (LastPlayedCLSLevel == null)
			{
				if (ToDownload == null)
					return;

				LevelImporter levelImporter = __instance.levelImporter;
				levelImporter.Showing = true;
				levelImporter.ToggleInsertUrlContainer(true, true, false);
				levelImporter.urlInput.text = ToDownload;
				levelImporter.ValidateUrl();
				ToDownload = null;
				return;
			}
			
			int index = __instance.levelsData.FindIndex((data) => data.path == LastPlayedCLSLevel);
			if (index >= 0 && index < __instance.levelsData.Count)
				__instance.StartCoroutine(__instance.LoadLevelsData());
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(scnCLS), nameof(scnCLS.LoadLevelsData))]
		public static IEnumerator LoadLevelsDataPatch(IEnumerator __result, scnCLS __instance)
        {
			while (__result.MoveNext())
				yield return __result.Current;
				
			int index = __instance.levelsData.FindIndex((data) => data.path == LastPlayedCLSLevel);
			if (index >= 0 && index < __instance.levelsData.Count)
			{
            	AccessTools.Method(typeof(scnCLS), "ShowSyringes").Invoke(__instance, [__instance.levelsData, index, true]);
				__instance.sendLevelDataToLevelDetailCoroutine = __instance.SendLevelDataToLevelDetail(true, -1f);
				__instance.StartCoroutine(__instance.sendLevelDataToLevelDetailCoroutine);
				LastPlayedCLSLevel = null;
			}
		}
	}
}