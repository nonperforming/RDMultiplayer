namespace Multiplayer;

public enum PacketType : byte
{
	Handshake, // Responds to Handshake if.. does have level, otherwise leaves the lobby.
	Acknowledge,
	SetReadyStatus,
	SetSwappedStatus,
	StartLevel, // PRevents race conditions
	SetPaused,
	ShareRank,
	Unknown = 0xff
}