using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
#if !BPE5
using BepInEx.Unity.Mono;
#endif
using HarmonyLib;

namespace Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
// i don't know what
#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class Multiplayer : BaseUnityPlugin
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
	public static ManualLogSource Log;

	public static ConfigEntry<bool> Enabled;
	// public static ConfigEntry<bool> PrivateLobby;
	public static ConfigEntry<bool> CanPauseInVersus;
	public static ConfigEntry<bool> SharePause;
	public static ConfigEntry<bool> ShowHelpText;
	public static ConfigEntry<bool> RespectStartImmediately;

	private void Awake()
	{
		Global.Log = Log = Logger;
		Enabled = Config.Bind("", "Enabled", false, "If multiplayer should be enabled.");
		// PrivateLobby = Config.Bind("", "PrivateLobby", false, "If people should be prevented from joining you without an invite.");
		CanPauseInVersus = Config.Bind("", "CanPauseInVersus", false, "If either player should be allowed to pause whilst in versus.");
		SharePause = Config.Bind("", "SharePause", true, "If when one player pauses, the other should pause too. Shares unpausing as well.");
		ShowHelpText = Config.Bind("", "ShowHelpText", true, "If the help text in the prestart screen should be shown.");
		RespectStartImmediately = Config.Bind("", "RespectStartImmediately", false, 
			"If starting a level immediately (set by the level authors), should be done or not.\n" +
			"Do note that starting the level immediately makes it impossible for multiplayer to be done."
		);

		Log.LogMessage($"Multiplayer is {(!Enabled.Value ? "not " : "")}enabled.");
		Harmony instance = new("patcher");
		if (Enabled.Value)
		{
			instance.PatchAll();
		}
	}
}