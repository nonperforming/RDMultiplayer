using System.IO;
using System.Text;
using UnityEngine;

namespace Multiplayer;

public class Packet(PacketType type)
{
	public PacketType Type = type;

	public bool CanPauseInVersus = false;
	public bool SharePause = false;
	public bool IsStoryMode = false;
	public bool TwoPlayer = false;
	public bool DogMode = false;
	public Random.State Seed = new();
	public float LevelSpeed = 1f;
	public string Level = "";

	public bool IsReady = false;
	
	public bool IsSwapped = false;

	public bool IsPaused = false;

	public float Mistakes = 0;
	public int EarlyOffset = 0;
	public int LateOffset = 0;
	public int HitsInPlusZone = 0;
	public int HitsInNormalZone = 0;
	public int HitsInMinusZone = 0;

	public byte[] UnknownData;

	public byte[] Encode()
    {
        using MemoryStream output = new();
		using BinaryWriter writer = new(output);
		
		writer.Write((byte)Type);
		switch (Type)
        {
            case PacketType.Handshake:
				writer.Write(CanPauseInVersus);
				writer.Write(SharePause);
				writer.Write(IsStoryMode);
				writer.Write(TwoPlayer);
				writer.Write(DogMode);

				int[] rng = RandomSeeding.GetState(Seed);
				foreach (int s in rng)
					writer.Write(s);

				writer.Write(LevelSpeed);
				writer.Write(Encoding.UTF8.GetBytes(Level));
				break;

			case PacketType.Acknowledge:
			case PacketType.StartLevel:
				break;
			case PacketType.SetReadyStatus:
				writer.Write(IsReady);
				break;
			case PacketType.SetSwappedStatus:
				writer.Write(IsSwapped);
				break;
			case PacketType.SetPaused:
				writer.Write(IsPaused);
				break;

			case PacketType.ShareRank:
				writer.Write(Mistakes);
				writer.Write(EarlyOffset);
				writer.Write(LateOffset);
				writer.Write(HitsInPlusZone);
				writer.Write(HitsInNormalZone);
				writer.Write(HitsInMinusZone);
				break;

			default:
				writer.Write(UnknownData);
				break;
        }

		return output.GetBuffer();
    }

	public static Packet Decode(byte[] buffer)
    {
		// purely for floats...
		using MemoryStream stream = new(buffer);
		using BinaryReader reader = new(stream);

		byte type = reader.ReadByte();
        switch (type)
        {
			case (byte)PacketType.Handshake:
				Packet hasLevel = new(PacketType.Handshake)
                {
					CanPauseInVersus = reader.ReadBoolean(),
					SharePause = reader.ReadBoolean(),
					IsStoryMode = reader.ReadBoolean(),
					TwoPlayer = reader.ReadBoolean(),
					DogMode = reader.ReadBoolean()
                };

				int s0 = reader.ReadInt32();
				int s1 = reader.ReadInt32();
				int s2 = reader.ReadInt32();
				int s3 = reader.ReadInt32();

				hasLevel.Seed = RandomSeeding.CreateState(s0, s1, s2, s3);
				hasLevel.LevelSpeed = reader.ReadSingle();
				hasLevel.Level = TrimNull(Encoding.UTF8.GetString(buffer[26..]));

				return hasLevel;
				
			case (byte)PacketType.Acknowledge:
			case (byte)PacketType.StartLevel:
				Packet noData = new((PacketType)type);
				return noData;

			case (byte)PacketType.SetReadyStatus:
				Packet setReadyStatus = new(PacketType.SetReadyStatus)
                {
					IsReady = reader.ReadBoolean()
                };
				return setReadyStatus;

			case (byte)PacketType.SetSwappedStatus:
				Packet setSwappedStatus = new(PacketType.SetSwappedStatus)
                {
					IsSwapped = reader.ReadBoolean()
                };
				return setSwappedStatus;

			case (byte)PacketType.SetPaused:
				Packet setPaused = new(PacketType.SetPaused)
                {
					IsPaused = reader.ReadBoolean()
                };
				return setPaused;

			case (byte)PacketType.ShareRank:
				Packet shareRank = new(PacketType.ShareRank)
                {
                    Mistakes = reader.ReadSingle(),
					EarlyOffset = reader.ReadInt32(),
					LateOffset = reader.ReadInt32(),
					HitsInPlusZone = reader.ReadInt32(),
					HitsInNormalZone = reader.ReadInt32(),
					HitsInMinusZone = reader.ReadInt32(),
                };
				return shareRank;

            default:
				Packet unknown = new(PacketType.Unknown)
				{
					UnknownData = buffer[1..]
				};
				return unknown;
        }
    }

	private static string TrimNull(string str)
    {
        string output = "";
		foreach (char c in str)
        {
            if (c == 0x00)
				break;
			output += c;
        }
		return output;
    }
}