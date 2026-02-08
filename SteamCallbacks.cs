using System;
using System.Collections.Generic;
using System.Linq;
using RDLevelEditor;
using UnityEngine.UI;

namespace Multiplayer;

public class SteamCallbacks : Global
{
	public static void Initialise()
	{
		Lobby.OnEnterLobby = OnEnterLobby;
		Lobby.OnOtherEnterLobby = OnOtherEnterLobby;
		Lobby.OnOtherLeaveLobby = OnOtherLeaveLobby;
		Lobby.OnPacketReceived = OnPacketReceived;
	}

	public static void OnEnterLobby()
	{
		if (Lobby.IsHost)
		{
			Log.LogMessage($"Created lobby {Lobby.CurrentLobbySteamID.m_SteamID} ({Lobby.IsHost})");
			return;
		}
		Log.LogMessage($"Joined lobby {Lobby.CurrentLobbySteamID.m_SteamID} ({Lobby.IsHost})");
	}

	public static void OnOtherEnterLobby()
	{
		if (!Lobby.IsHost)
		{
			Log.LogMessage("Other has joined.");
			return;
		}
		Log.LogMessage($"Player {Lobby.OtherUser.GetSteamID64()} has joined us ({Lobby.IsHost})");
		// Lobby.SendPacket(HostPackets.SendHandshake())
		HostPackets.SendHandshake();
	}

	public static void OnOtherLeaveLobby()
	{
		Log.LogMessage($"Player {Lobby.OtherUser.GetSteamID64()} has left us ({Lobby.IsHost})");
		if (scnGame.instance == null || scnEditor.instance != null)
			return;
		scnGame game = scnGame.instance;

		Text extraCredits = game.rankscreen.extraCredits;
		string left = LanguageMap.Get("multiplayer.otherLeft.preAck");
		if (!game.startTheGameCalled && !Lobby.OtherUserReady && !extraCredits.text.Contains(left))
        {
			string help = LanguageMap.Get("multiplayer.help");
			bool narrationNeeded = true;

			if (extraCredits.text.Contains("\n"))
			{
				help = "\n" + help;
				left = "\n" + left;
			}

			if (extraCredits.text.Contains(help))
            	extraCredits.text = extraCredits.text.Replace(help, left);
			else if (extraCredits.text.Length > 0)
				extraCredits.text += left;
			else
			{
				game.rankscreen.ShowExtraCredits(left);
				narrationNeeded = false;
			}

			if (narrationNeeded)
                Narration.Say(LanguageMap.Get("Multiplayer.otherLeft.preAck"), NarrationCategory.Notification, false);
			return;
        }
		
		if (!game.startTheGameCalled)
			return;


		if (RankscreenDisplay.GetTrueGameover() > 0 || RankscreenDisplay.GetExiting())
			return;
		if (RankscreenDisplay.WaitingForData)
        {
			MistakesManager mm = game.mistakesManager;
			List<float> hitTimes = [.. scnGame.p1HitTimes, ..scnGame.p2HitTimes];
			SharedPackets.ReadRankData(
				mm.mistakes, mm.earlyOffsetsSum, mm.lateOffsetsSum, 
				hitTimes.Count(h => h < 0.04f),
				hitTimes.Count(h => h >= 0.04f && h <= 0.08f),
				hitTimes.Count(h => h > 0.08f)
			);
			return;
        }
		if (game.bladesContainer.gameObject.activeSelf)
			return;

		if (InVersus)
		{
			LEDSign.status = LanguageMap.Get("multiplayer.otherLeft");
			return;
		}
		
		if (InCoop)
			game.Restart(false);
	}

	public static void OnPacketReceived(Packet packet)
	{
		Log.LogMessage($"Packet {packet.Type} received");

		SharedPackets.CheckPacket(packet);
		if (!Lobby.IsHost)
		{
			ClientPackets.CheckPacket(packet);
			return;
		}
		HostPackets.CheckPacket(packet);
	}	
}