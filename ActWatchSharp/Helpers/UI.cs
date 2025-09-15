using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Globalization;

namespace ActWatchSharp.Helpers
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

			Task.Run(() =>
			{
				LogManager.CvarAction(sCvarName, sCvarValue);
			});

			if (bClientNotify)
			{
				Task.Run(() =>
				{
					Parallel.ForEach(AW.g_OfflinePlayer, (pl) =>
					{
						if (pl.Player != null && pl.Player is { IsValid: true, IsBot: false, IsHLTV: false })
						{
							ReplyToCommand(pl.Player, false, "Cvar.Notify", sCvarName, sCvarValue);
						}
					});
				});
				/*Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
				{
					ReplyToCommand(pl, false, "Cvar.Notify", sCvarName, sCvarValue);
				});*/
			}
		}
		public static void PrintToAllAdminBan(string sMessage, string[] sPIF_admin, string[] sPIF_player, string sReason, string sColorMessage)
		{
			Server.NextFrame(() =>
			{
				if (ActWatchSharp.Strlocalizer == null) return;
				using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
				{
					PrintToConsole(ActWatchSharp.Strlocalizer[sMessage, AW.g_CFG.color_warning, sPIF_admin[3], sColorMessage, sPIF_player[3]], 1);
					PrintToConsole(ActWatchSharp.Strlocalizer["Chat.Admin.Reason", AW.g_CFG.color_warning, sReason], 1);
				}

				Task.Run(() =>
				{
					LogManager.AdminAction(sMessage, AW.g_CFG.color_warning, sPIF_admin[3], sColorMessage, sPIF_player[3]);
					LogManager.AdminAction("Chat.Admin.Reason", AW.g_CFG.color_warning, sReason);
				});

				Task.Run(() =>
				{
					Parallel.ForEach(AW.g_OfflinePlayer, (pl) =>
					{
						if (pl.Player != null && pl.Player is { IsValid: true, IsBot: false, IsHLTV: false })
						{
							ReplyToCommand(pl.Player, false, sMessage, AW.g_CFG.color_warning, PlayerInfo(pl.Player, sPIF_admin), sColorMessage, PlayerInfo(pl.Player, sPIF_player));
							ReplyToCommand(pl.Player, false, "Chat.Admin.Reason", AW.g_CFG.color_warning, sReason);
						}
					});
				});

				/*Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
				{
					ReplyToCommand(pl, false, sMessage, AW.g_CFG.color_warning, PlayerInfo(pl, sPIF_admin), sColorMessage, PlayerInfo(pl, sPIF_player));
					ReplyToCommand(pl, false, "Chat.Admin.Reason", AW.g_CFG.color_warning, sReason);
				});*/
			});
		}
		public static void PrintToAllActAction(string sMessage, string[] sPlayerInfoFormat, string sActName, uint iIndex, bool bType)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(ClearcolorReplacements(ActWatchSharp.Strlocalizer[sMessage, sPlayerInfoFormat[3], sActName, iIndex]), 1);
			}

			Task.Run(() =>
			{
				LogManager.ActAction(sMessage, sPlayerInfoFormat[3], sActName, iIndex);
			});

			Task.Run(() =>
			{
				Parallel.ForEach(AW.g_OfflinePlayer, (pl) =>
				{
					if (pl.Player != null && pl.Player is { IsValid: true, IsBot: false, IsHLTV: false })
					{
						if (bType)
						{
							if (AW.g_bButton[pl.Player.Slot]) ReplyToCommand(pl.Player, false, sMessage, PlayerInfo(pl.Player, sPlayerInfoFormat), sActName, iIndex);
						}
						else
						{
							if (AW.g_bTrigger[pl.Player.Slot]) ReplyToCommand(pl.Player, false, sMessage, PlayerInfo(pl.Player, sPlayerInfoFormat), sActName, iIndex);
						}
					}
				});
			});

			/*Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
			{
				if (bType)
				{
					if (AW.g_bButton[pl.Slot]) ReplyToCommand(pl, false, sMessage, PlayerInfo(pl, sPlayerInfoFormat), sActName, iIndex);
				} else
				{
					if (AW.g_bTrigger[pl.Slot]) ReplyToCommand(pl, false, sMessage, PlayerInfo(pl, sPlayerInfoFormat), sActName, iIndex);
				}
			});*/
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
				else
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
					{
						TranslatedPrintToConsole(sMessage, 13, arg);
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
				PrintToConsole(ActWatchSharp.Strlocalizer[sMessage, arg], iColor);
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
			Console.WriteLine(ReplaceColorTags(sMessage, false), arg);
			Console.ResetColor();
			/* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
		}

#nullable enable
		public static string PlayerInfo(CCSPlayerController? player, string[] sPlayerInfoFormat)
#nullable disable
		{
			if (player != null)
			{
				if (AW.g_iFormatPlayer[player.Slot] < 0 || AW.g_iFormatPlayer[player.Slot] > 3) return sPlayerInfoFormat[Cvar.PlayerFormat];
				return sPlayerInfoFormat[AW.g_iFormatPlayer[player.Slot]];
			}
			return sPlayerInfoFormat[3];
		}
		public static string[] PlayerInfoFormat(CCSPlayerController player)
		{
			if (player != null)
			{
				string[] sResult = new string[4];
				sResult[0] = $"{AW.g_CFG.color_name}{player.PlayerName}{AW.g_CFG.color_warning}";
				sResult[1] = $"{sResult[0]}[{AW.g_CFG.color_steamid}#{player.UserId}{AW.g_CFG.color_warning}]";
				sResult[2] = $"{sResult[0]}[{AW.g_CFG.color_steamid}#{AW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{AW.g_CFG.color_warning}]";
				sResult[3] = $"{sResult[0]}[{AW.g_CFG.color_steamid}#{player.UserId}{AW.g_CFG.color_warning}|{AW.g_CFG.color_steamid}#{AW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{AW.g_CFG.color_warning}]";
				return sResult;
			}
			return PlayerInfoFormat("Console", "Server");
		}
		public static string[] PlayerInfoFormat(string sName, string sSteamID)
		{
			string[] sResult = new string[4];
			sResult[0] = $"{AW.g_CFG.color_name}{sName}{AW.g_CFG.color_warning}";
			sResult[1] = sResult[0];
			sResult[2] = $"{AW.g_CFG.color_name}{sName}{AW.g_CFG.color_warning}[{AW.g_CFG.color_steamid}{sSteamID}{AW.g_CFG.color_warning}]";
			sResult[3] = sResult[2];
			return sResult;
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

		readonly static string[] colorPatterns =
	   [
		   "{default}", "{darkred}", "{purple}", "{green}", "{lightgreen}", "{lime}", "{red}", "{grey}",
			"{olive}", "{a}", "{lightblue}", "{blue}", "{d}", "{pink}", "{darkorange}", "{orange}",
			"{white}", "{yellow}", "{magenta}", "{silver}", "{bluegrey}", "{lightred}", "{cyan}", "{gray}"
	   ];
		readonly static string[] colorReplacements =
		[
			"\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08",
			"\x09", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10",
			"\x01", "\x09", "\x0E", "\x0A", "\x0D", "\x0F", "\x03", "\x08"
		];
	}
}
