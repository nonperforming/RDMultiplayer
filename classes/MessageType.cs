using System;

namespace Multiplayer;

// https://partner.steamgames.com/doc/api/steamnetworkingtypes#message_sending_flags
[Flags]
public enum MessageFlags : int
{
    Unreliable = 0x00,
	NoNagle = 0x01,
	NoDelay = 0x04,
	Reliable = 0x08,

	UnreliableNoNagle = Unreliable | NoNagle,
	UnreliableNoDelay = Unreliable | NoNagle | NoDelay,
	ReliableNoNagle = Reliable | NoNagle
}