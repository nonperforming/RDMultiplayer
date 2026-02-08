using System.Collections.Generic;
using RDLevelEditor;
using UnityEngine;

namespace Multiplayer;

public class LanguageMap
{
    public static Dictionary<string, Dictionary<SystemLanguage, string>> KeyMap = new()
    {
		/// NORMAL ///
		#region 
		["multiplayer.help"] = new()
        {
			[SystemLanguage.English] = "invite people to game via steam, or have them join you!",
			[SystemLanguage.Spanish] = "¡invita a jugadores al juego a través de steam, o haz que se unan a ti!"
        },
		["multiplayer.help.private"] = new()
        {
			[SystemLanguage.English] = "invite people to game via steam!",
			[SystemLanguage.Spanish] = "¡invita a jugadores al juego a través de steam!"
        },

		["multiplayer.status.hold"] = new()
        {
            [SystemLanguage.English] = "hold space bar to ready up!",
            [SystemLanguage.Spanish] = "¡Mantén Espacio para prepararte!"
        },

        ["multiplayer.otherLeft.preAck"] = new()
        {
            [SystemLanguage.English] = "other player may not have the level, please wait before reinviting them.",
			[SystemLanguage.Spanish] = "el otro jugador puede no tener el nivel. espera antes de volver a invitarle."
        },
        ["multiplayer.otherLeft"] = new()
        {
            [SystemLanguage.English] = "other player disconnected!",
			[SystemLanguage.Spanish] = "¡El otro jugador se ha desconectado!"
        },
		["multiplayer.pausingDisabled"] = new()
        {
			[SystemLanguage.English] = "pausing is disabled!",
			[SystemLanguage.Spanish] = "¡Pausar está deshabilitado!"
        },

		["multiplayer.rankscreen.tie"] = new()
        {
            [SystemLanguage.English] = "It's a tie!",
			[SystemLanguage.Spanish] = "¡Es un empate!"
        },
		["multiplayer.rankscreen.winnerSuspense"] = new()
        {
            [SystemLanguage.English] = "The winner is...",
			[SystemLanguage.Spanish] = "El ganador es..."
        },
		["multiplayer.rankscreen.winner"] = new()
        {
            [SystemLanguage.English] = "The winner is [name]!",
			[SystemLanguage.Spanish] = "¡El ganador es [name]!"
        },

		["multiplayer.narration.versusRank"] = new()
        {
			[SystemLanguage.English] = "Your rank and opponent's rank",
			[SystemLanguage.Spanish] = "Tu calificación y la del oponente"
        },
		#endregion
		
		/// RHYTHM DOGTOR ///
		/// Credits to others for some of these lines ///
		#region 
		["rhythmDogtor.multiplayer.help"] = new()
        {
			[SystemLanguage.English] = "invite doggies to play via steam, or have them chase after you!",
			[SystemLanguage.Spanish] = "¡invita a perritos para jugar a través de steam, o haz que te persigan!"
        },
		["rhythmDogtor.multiplayer.help.private"] = new()
        {
			[SystemLanguage.English] = "invite doggies to play via steam!",
			[SystemLanguage.Spanish] = "¡invita a perritos para jugar a través de steam!"
        },

		["rhythmDogtor.multiplayer.status.hold"] = new()
        {
            [SystemLanguage.English] = "howl to ready up!",
            [SystemLanguage.Spanish] = "¡aúlla para prepararte!"
        },

        ["rhythmDogtor.multiplayer.otherLeft"] = new()
        {
            [SystemLanguage.English] = "other doggie went outside!", 
			[SystemLanguage.Spanish] = "¡el otro perro ha salido!"
        },
		["rhythmDogtor.multiplayer.pausingDisabled"] = new()
        {
			[SystemLanguage.English] = "pawsing disabled!",
			[SystemLanguage.Spanish] = "¡no te puedes sentar!"
        },

		["rhythmDogtor.multiplayer.rankscreen.tie"] = new()
        {
            [SystemLanguage.English] = "same toy fetched!",
			[SystemLanguage.Spanish] = "¡habeis buscado el mismo juguete!"
        },
		["rhythmDogtor.multiplayer.rankscreen.winnerSuspense"] = new()
        {
            [SystemLanguage.English] = "the goodest boy is...",
			[SystemLanguage.Spanish] = "el mejor chico es..."
        },
		["rhythmDogtor.multiplayer.rankscreen.winner"] = new()
        {
            [SystemLanguage.English] = "the goodest boy is [name]!",
			[SystemLanguage.Spanish] = "¡el mejor chico es [name]!"
        },

		["rhythmDogtor.multiplayer.narration.versusRank"] = new()
        {
			[SystemLanguage.English] = "Your toy and the opponent's toy",
			[SystemLanguage.Spanish] = "Tu juguete y el del oponente"
        },
		#endregion
    };

	public static string Get(string key)
    {
		if (scnGame.instance != null && scnGame.instance.currentLevel.dogMode && KeyMap.ContainsKey("rhythmDogtor." + key))
			key = "rhythmDogtor." + key;
        if (!KeyMap.TryGetValue(key, out Dictionary<SystemLanguage, string> strings))
			return key;

		if (!strings.TryGetValue(RDString.language, out string value))
			return strings[SystemLanguage.English];

		return value;
    }
}