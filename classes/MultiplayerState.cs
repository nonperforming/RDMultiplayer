namespace Multiplayer;

public class MultiplayerState
{
	public static bool CanPauseInVersus = false;
	public static bool SharePause = true;
    public static bool OtherReady = false;
	public static bool Versus = false;
	public static RDPlayer OtherPlayer => Lobby.IsHost != SwappedPlayers ? RDPlayer.P2 : RDPlayer.P1;
	public static RDPlayer SelfPlayer => Lobby.IsHost != SwappedPlayers ? RDPlayer.P1 : RDPlayer.P2;
	public static bool SwappedPlayers = false;
	public static string Hash = "";

	public static bool GotOtherPlayerRankInformation = false;
	public static float OtherMistakes = 0;
	public static int OtherEarlyOffset = 0;
	public static int OtherLateOffset = 0;
	public static int OtherHitsInPlusZone = 0;
	public static int OtherHitsInNormalZone = 0;
	public static int OtherHitsInMinusZone = 0;
}