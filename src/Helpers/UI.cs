using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Globalization;
using ActWatchSharp.Helpers;
using System;

namespace ActWatchSharp
{
	static class UI
	{
		public static void CvarChangeNotify(string sCvarName, string sCvarValue, bool bClientNotify)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ActWatchSharp.Strlocalizer["Cvar.Notify", sCvarName, sCvarValue], 3);
			}

			LogManager.CvarAction(sCvarName, sCvarValue);

			if (bClientNotify)
			{
				Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
				{
					ReplyToCommand(pl, false, "Cvar.Notify", sCvarName, sCvarValue);
				});
			}
		}
		public static void PrintToAllAdminAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ActWatchSharp.Strlocalizer[sMessage, arg], 1);
			}
			LogManager.AdminAction(sMessage, arg);
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
			{
				ReplyToCommand(pl, false, sMessage, arg);
			});
		}
		public static void PrintToAllButtonAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ActWatchSharp.Strlocalizer[sMessage, arg], 1);
			}
			LogManager.ActAction(sMessage, arg);
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
			{
				if (AW.g_bButton[pl.Slot]) ReplyToCommand(pl, false, sMessage, arg);
			});
		}
		public static void PrintToAllTriggerAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ActWatchSharp.Strlocalizer[sMessage, arg], 1);
			}
			LogManager.ActAction(sMessage, arg);
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
			{
				if (AW.g_bTrigger[pl.Slot]) ReplyToCommand(pl, false, sMessage, arg);
			});
		}
		public static void ReplyToCommand(CCSPlayerController player, bool bConsole, string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			Server.NextFrame(() =>
			{
				if (player is { IsValid: true, IsBot: false, IsHLTV: false })
				{
					using (new WithTemporaryCulture(player.GetLanguage()))
					{
						if (!bConsole) player.PrintToChat(ReplaceColorTags(" {lightblue}[{green}ActWatch{lightblue}]{default} ") + ReplaceColorTags(ActWatchSharp.Strlocalizer[sMessage, arg]));
						else player.PrintToConsole($"[ActWatch] {ReplaceColorTags(ActWatchSharp.Strlocalizer[sMessage, arg], false)}");
					}
				}
			});
		}

		public static void ReplyToCommandMessage(CCSPlayerController player, bool bConsole, string sMessage)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			Server.NextFrame(() =>
			{
				if (player is { IsValid: true, IsBot: false, IsHLTV: false })
				{
					using (new WithTemporaryCulture(player.GetLanguage()))
					{
						if (!bConsole) player.PrintToChat(ReplaceColorTags(" {lightblue}[{green}ActWatch{lightblue}]{default} ") + ReplaceColorTags(sMessage));
						else player.PrintToConsole($"[ActWatch] {ReplaceColorTags(sMessage, false)}");
					}
				}
			});
		}

		public static void TranslatedPrintToConsole(string sMessage, int iColor = 1, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ReplaceColorTags(ActWatchSharp.Strlocalizer[sMessage, arg], false), iColor);
			}
		}

		public static void PrintToConsole(string sMessage, int iColor = 1, params object[] arg)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("ActWatch");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)iColor;
			Console.WriteLine(ClearcolorReplacements(ReplaceColorTags(sMessage, false)), arg);
			Console.ResetColor();
			/* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
		}
		public static string PlayerInfo(CCSPlayerController player)
		{
			return player != null ? $"{AW.g_CFG.color_name}{player.PlayerName}{AW.g_CFG.color_warning}[{AW.g_CFG.color_steamid}#{player.UserId}{AW.g_CFG.color_warning}|{AW.g_CFG.color_steamid}#{AW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{AW.g_CFG.color_warning}]" : PlayerInfo("Console", "Server");
		}
		public static string PlayerInfo(string sName, string sSteamID)
		{
			return $"{AW.g_CFG.color_name}{sName} {AW.g_CFG.color_warning}[{AW.g_CFG.color_steamid}{sSteamID}{AW.g_CFG.color_warning}]";
		}
		public static string ReplaceColorTags(string input, bool bChat = true)
		{
			for (var i = 0; i < colorPatterns.Length; i++)
				input = input.Replace(colorPatterns[i], bChat ? colorReplacements[i] : "");

			return input;
		}
		static string ClearcolorReplacements(string input)
		{
			for (var i = 0; i < colorReplacements.Length; i++)
				input = input.Replace(colorReplacements[i], "");

			return input;
		}
		static string[] colorPatterns =
		{
			"{default}", "{darkred}", "{purple}", "{green}", "{lightgreen}", "{lime}", "{red}", "{grey}",
			"{olive}", "{a}", "{lightblue}", "{blue}", "{d}", "{pink}", "{darkorange}", "{orange}",
			"{white}", "{yellow}", "{magenta}", "{silver}", "{bluegrey}", "{lightred}", "{cyan}", "{gray}"
		};
		static string[] colorReplacements =
		{
			"\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08",
			"\x09", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10",
			"\x01", "\x09", "\x0E", "\x0A", "\x0D", "\x0F", "\x03", "\x08"
		};
	}
}
