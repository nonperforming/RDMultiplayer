using System;
using System.Collections.Generic;

namespace Multiplayer;

public class SharedPackets : Global
{
    public static void CheckPacket(Packet packet)
    {
		scnGame game = scnGame.instance;
		if (game == null)
			return;
        switch (packet.Type)
        {
            case PacketType.ShareRank:
				ReadRankData(packet.Mistakes, packet.EarlyOffset, packet.LateOffset, 
					packet.HitsInPlusZone, packet.HitsInNormalZone, packet.HitsInMinusZone);
				break;

			case PacketType.SetReadyStatus:
				MultiplayerState.OtherReady = packet.IsReady;
				break;

			case PacketType.SetSwappedStatus:
				if (MultiplayerState.SwappedPlayers != packet.IsSwapped)
				{
					MultiplayerState.SwappedPlayers = packet.IsSwapped;
					InputChanges.SwapCalledFromPacket = true;
					RDInput.SwapP1AndP2Controls(true);
				}
				break;

			case PacketType.StartLevel:
				if (game.startTheGameCalled)
					return;
				game.StartCoroutine(game.StartTheGame(scnGame.levelSpeed));
				break;
				
			case PacketType.SetPaused:
				if (game.paused == packet.IsPaused)
					return;
				MultiplayerPauseChanges.PauseCalledFromPacket = true;
				game.TogglePauseGame();
				break;
        }
    }

	public static void ReadRankData(float mistakes, int earlyOffset, int lateOffset, int hitsInPlusZone, int hitsInNormalZone, int hitsInMinusZone)
    {
		scnGame game = scnGame.instance;
		if (game == null)
			return;
				
		RDPlayer otherPlayer = MultiplayerState.OtherPlayer;
		if (GC.twoPlayerMode)
		{
			game.mistakesManager.AddMistake(otherPlayer, 0, mistakes);
			game.mistakesManager.AddAbsoluteMistake(otherPlayer, -earlyOffset);
			game.mistakesManager.AddAbsoluteMistake(otherPlayer, lateOffset);
			
			List<float> hitTimes = otherPlayer == RDPlayer.P2 ? scnGame.p2HitTimes : scnGame.p1HitTimes;
			for (int i = 0; i < hitsInPlusZone; i++)
				hitTimes.Add(0f);
			for (int i = 0; i < hitsInNormalZone; i++)
				hitTimes.Add(0.06f);
			for (int i = 0; i < hitsInMinusZone; i++)
				hitTimes.Add(0.1f);
		}
		else
		{
			MultiplayerState.OtherMistakes = mistakes;
			MultiplayerState.OtherEarlyOffset = earlyOffset;
			MultiplayerState.OtherLateOffset = lateOffset;
			MultiplayerState.OtherHitsInPlusZone = hitsInPlusZone;
			MultiplayerState.OtherHitsInNormalZone = hitsInNormalZone;
			MultiplayerState.OtherHitsInMinusZone = hitsInMinusZone;
		}

		MultiplayerState.GotOtherPlayerRankInformation = true;
		if (RankscreenDisplay.WaitingForData)
		{
			RankscreenDisplay.IntendedRankSequence = Math.Min(RankscreenDisplay.IntendedRankSequence, 2);
			game.rankscreen.AdvanceGameover();
		}
    }
}