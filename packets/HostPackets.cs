using System.IO;
using UnityEngine;

namespace Multiplayer;

public class HostPackets : Global
{
	public static void CheckPacket(Packet packet)
	{
		// switch (packet.Type)
		// {
		// 	case PacketType.StartSessionAccept:
		// 		SendHandshake();
		// 		break;
		// }
	}

	public static void SendHandshake()
    {
        bool storyMode = scnGame.levelToLoadSource == LevelSource.InternalPath;
		RandomSeeding.Seed = Random.state;
		Lobby.SendPacket(new(PacketType.Handshake)
		{
			CanPauseInVersus = Multiplayer.CanPauseInVersus.Value, // Settings
			SharePause = Multiplayer.SharePause.Value, // Settings
			IsStoryMode = storyMode,
			TwoPlayer = PreStartScreen.Original2PMode,
			DogMode = scnGame.instance.currentLevel.dogMode,
			Seed = Random.state,
			LevelSpeed = scnGame.levelSpeed,
			Level = Path.GetFileNameWithoutExtension(storyMode ? scnGame.internalIdentifier : Path.GetDirectoryName(scnGame.currentLevelPath))
		});
    }
}