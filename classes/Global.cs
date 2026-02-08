using BepInEx.Logging;

namespace Multiplayer;

public class Global
{
    public static ManualLogSource Log;
	public static bool InCoop => Lobby.InMultiplayer && !MultiplayerState.Versus;
	public static bool InVersus => Lobby.InMultiplayer && MultiplayerState.Versus;
}