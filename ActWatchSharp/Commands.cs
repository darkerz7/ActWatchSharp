using ActWatchSharp.ActBan;
using ActWatchSharp.Helpers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using static ActWatchSharp.ActBan.ActBanDB;
using static ActWatchSharp.Helpers.FindTarget;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		void UnRegCommands()
		{
			RemoveCommand("aw_reload", OnAWReload);
			RemoveCommand("css_areload", OnAWReload);
			RemoveCommand("bw_ban", OnBWBan);
			RemoveCommand("css_bban", OnBWBan);
			RemoveCommand("bw_unban", OnBWUnBan);
			RemoveCommand("css_bunban", OnBWUnBan);
			RemoveCommand("bw_status", OnBWStatus);
			RemoveCommand("css_bstatus", OnBWStatus);
			RemoveCommand("bw_banlist", OnBWBanList);
			RemoveCommand("css_bbanlist", OnBWBanList);

			RemoveCommand("tw_ban", OnTWBan);
			RemoveCommand("css_trban", OnTWBan);
			RemoveCommand("tw_unban", OnTWUnBan);
			RemoveCommand("css_trunban", OnTWUnBan);
			RemoveCommand("tw_status", OnTWStatus);
			RemoveCommand("css_trstatus", OnTWStatus);
			RemoveCommand("tw_banlist", OnTWBanList);
			RemoveCommand("css_trbanlist", OnTWBanList);

			RemoveCommand("aw_list", OnAWList);
			RemoveCommand("css_alist", OnAWList);

			RemoveCommand("css_buttons", OnAWButtons);
			RemoveCommand("css_triggers", OnAWTriggers);
		}

		[ConsoleCommand("aw_reload", "Reload plugin config")]
		[ConsoleCommand("css_areload", "Reload plugin config")]
		[RequiresPermissions("@css/aw_reload")]
#nullable enable
		public void OnAWReload(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			AW.LoadCFG(ModuleDirectory);
			UI.ReplyToCommand(player, bConsole, "Reply.Reload_configs");
		}

		//Bban Start
		[ConsoleCommand("bw_ban", "Allows the admin to ban players from pressing buttons")]
		[ConsoleCommand("css_bban", "Allows the admin to ban players from pressing buttons")]
		[RequiresPermissions("@css/bw_ban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<time>] [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnBWBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.ButtonGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			OfflineBan target = null;

			if (players.Count > 0)
			{
				CCSPlayerController targetOnline = players.Single();

				if (!AdminManager.CanPlayerTarget(admin, targetOnline))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.You_cannot_target");
					return;
				}

				if (!(AW.g_ButtonBannedPlayer.ContainsKey(targetOnline) || AW.g_ButtonBannedPlayer.TryAdd(targetOnline, new ActBanPlayer(true))))
				{
					UI.ReplyToCommand(admin, bConsole, "Info.Error", "Player not found in dictionary");
					return;
				}

				if (AW.g_ButtonBannedPlayer[targetOnline].bBanned)
				{
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(targetOnline)} {Strlocalizer["Reply.Buttons.Has_a_ban"]}");
					return;
				}

				foreach (OfflineBan OfflineTest in AW.g_OfflinePlayer.ToList())
				{
					if (OfflineTest.UserID == targetOnline.UserId)
					{
						target = OfflineTest;
						break;
					}
				}
			}
			else
			{
				target = OfflineFunc.FindTarget(admin, command.GetArg(1), bConsole);
			}

			if (target == null)
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
				return;
			}

			int time = Cvar.ButtonBanTime;
			if (command.ArgCount >= 2)
			{
				if (!int.TryParse(command.GetArg(2), out int timeparse))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.Must_be_an_integer");
					return;
				}
				time = timeparse;
			}

			if (time == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/bw_ban_perm"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Permanent");
				return;
			}

			if (time > Cvar.ButtonBanLong && !AdminManager.PlayerHasPermissions(admin, "@css/bw_ban_long"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Long", Cvar.ButtonBanLong);
				return;
			}

			string reason = command.GetArg(3);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.ButtonBanReason;

			UI.PrintToAllAdminAction("Chat.Admin.Buttons.Banned", AW.g_CFG.color_warning, UI.PlayerInfo(admin), AW.g_CFG.color_disabled, target.Online ? UI.PlayerInfo(target.Player) : UI.PlayerInfo(target.Name, target.SteamID));
			UI.PrintToAllAdminAction("Chat.Admin.Reason", AW.g_CFG.color_warning, reason);
			Server.NextFrame(() =>
			{
				ActBanPlayer bbanPlayer = target.Online ? AW.g_ButtonBannedPlayer[target.Player] : new ActBanPlayer(true);
				if (bbanPlayer.SetBan(admin != null ? admin.PlayerName : "Console", admin != null ? AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.Name, target.SteamID, time, reason))
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Success"); //admin.PrintToChat("Success");
						UI.TranslatedPrintToConsole("Reply.Ban.Success", 6);
						LogManager.SystemAction("Reply.Ban.Success");
					});
				}
				else
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Failed"); //admin.PrintToChat("Failed");
						UI.TranslatedPrintToConsole("Reply.Ban.Failed", 15);
						LogManager.SystemAction("Reply.Ban.Failed");
					});
				}
			});
		}

		[ConsoleCommand("bw_unban", "Allows the admin to remove the ban on pressing buttons")]
		[ConsoleCommand("css_bunban", "Allows the admin to remove the ban on pressing buttons")]
		[RequiresPermissions("@css/bw_unban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnBWUnBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.ButtonGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			ActBanPlayer target = new(true);
			string sTarget = command.GetArg(1);

			bool bOnline = players.Count > 0;

			if (bOnline)
			{
				CCSPlayerController targetController = players.Single();
				if (!AdminManager.CanPlayerTarget(admin, targetController))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.You_cannot_target");
					return;
				}
				if (!(AW.g_ButtonBannedPlayer.ContainsKey(targetController) || AW.g_ButtonBannedPlayer.TryAdd(targetController, new ActBanPlayer(true))))
				{
					UI.ReplyToCommand(admin, bConsole, "Info.Error", "Player not found in dictionary");
					return;
				}
				target.bBanned = AW.g_ButtonBannedPlayer[targetController].bBanned;
				target.sAdminName = AW.g_ButtonBannedPlayer[targetController].sAdminSteamID;
				target.sAdminSteamID = AW.g_ButtonBannedPlayer[targetController].sAdminSteamID;
				target.iDuration = AW.g_ButtonBannedPlayer[targetController].iDuration;
				target.iTimeStamp_Issued = AW.g_ButtonBannedPlayer[targetController].iTimeStamp_Issued;
				target.sReason = AW.g_ButtonBannedPlayer[targetController].sReason;
				target.sClientName = targetController.PlayerName;
				target.sClientSteamID = AW.ConvertSteamID64ToSteamID(targetController.SteamID.ToString());
			}

			string reason = command.GetArg(2);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.ButtonUnBanReason;
			if (bOnline) UnBanButtonsComm(admin, players.Single(), target, reason, bConsole);
			else if (sTarget.StartsWith("#steam_", StringComparison.OrdinalIgnoreCase))
			{
				ActBanPlayer.GetBan(sTarget[1..], admin, reason, bConsole, GetBanComm_Handler, true);
			}
			else UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
		}
#nullable enable
		GetBanCommFunc GetBanComm_Handler = (string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, List<List<string>> DBQuery_Result, bool bType) =>
#nullable disable
		{
			if (DBQuery_Result.Count > 0)
			{
				ActBanPlayer target = new(bType)
				{
					bBanned = true,
					sAdminName = DBQuery_Result[0][0],
					sAdminSteamID = DBQuery_Result[0][1],
					iDuration = Convert.ToInt32(DBQuery_Result[0][2]),
					iTimeStamp_Issued = Convert.ToInt32(DBQuery_Result[0][3]),
					sReason = DBQuery_Result[0][4],
					sClientName = DBQuery_Result[0][5],
					sClientSteamID = sClientSteamID
				};
				if (bType) UnBanButtonsComm(admin, null, target, reason, bConsole);
				else UnBanTriggersComm(admin, null, target, reason, bConsole);
				return;
			}
			if (bType) UnBanButtonsComm(admin, null, null, reason, bConsole);
			else UnBanTriggersComm(admin, null, null, reason, bConsole);
		};
#nullable enable
		static void UnBanButtonsComm(CCSPlayerController? admin, CCSPlayerController? player, ActBanPlayer? target, string reason, bool bConsole)
#nullable disable
		{
			if (target == null)
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
				return;
			}

			if (!target.bBanned)
			{
				UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target.sClientName, target.sClientSteamID)} {Strlocalizer["Reply.Buttons.Can_use"]}");
				return;
			}

			if (target.iDuration == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/bw_unban_perm"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.UnPermanent");
				return;
			}

			if (admin != null && !string.Equals(target.sAdminSteamID, AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString())) && !AdminManager.PlayerHasPermissions(admin, "@css/bw_unban_other"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Other");
				return;
			}

			if (target.UnBan(admin != null ? admin.PlayerName : "Console", admin != null ? AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.sClientSteamID, reason))
			{
				if (player != null) AW.g_ButtonBannedPlayer[player].bBanned = false;
				Server.NextFrame(() =>
				{
					if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.UnBan.Success"); //admin.PrintToChat("Success");
					UI.TranslatedPrintToConsole("Reply.Ban.UnBan.Success", 6);
					LogManager.SystemAction("Reply.Ban.UnBan.Success");
				});
			}
			else
			{
				Server.NextFrame(() =>
				{
					if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.UnBan.Failed"); //admin.PrintToChat("Failed");
					UI.TranslatedPrintToConsole("Reply.Ban.UnBan.Failed", 15);
					LogManager.SystemAction("Reply.Ban.UnBan.Failed");
				});
			}

			Server.NextFrame(() =>
			{
				UI.PrintToAllAdminAction("Chat.Admin.Buttons.Unrestricted", AW.g_CFG.color_warning, UI.PlayerInfo(admin), AW.g_CFG.color_enabled, UI.PlayerInfo(target.sClientName, target.sClientSteamID));
				UI.PrintToAllAdminAction("Chat.Admin.Reason", AW.g_CFG.color_warning, reason);
			});
		}


		[ConsoleCommand("bw_status", "Allows the player to view the buttons ban")]
		[ConsoleCommand("css_bstatus", "Allows the player to view the buttons ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnBWStatus(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!Cvar.ButtonGlobalEnable) return;
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			CCSPlayerController target = player;
			if (command.ArgCount > 1)
			{
				(List<CCSPlayerController> players, _) = Find(player, command, 1, true, false, MultipleFlags.NORMAL);

				if (players.Count == 0) return;

				target = players.Single();
			}
			if (target == null || !(AW.g_ButtonBannedPlayer.ContainsKey(target) || AW.g_ButtonBannedPlayer.TryAdd(target, new ActBanPlayer(true))))
			{
				UI.ReplyToCommand(player, bConsole, "Info.Error", "Player not found in dictionary");
				return;
			}
			if (AW.g_ButtonBannedPlayer[target].bBanned)
			{
				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Buttons.Has_a_ban"]}");

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Admin"]}: {AW.g_CFG.color_name}{UI.PlayerInfo(AW.g_ButtonBannedPlayer[target].sAdminName, AW.g_ButtonBannedPlayer[target].sAdminSteamID)}");

				switch (AW.g_ButtonBannedPlayer[target].iDuration)
				{
					case -1: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_enabled}{Strlocalizer["Reply.Ban.Temporary"]}"); break;
					case 0: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{Strlocalizer["Reply.Ban.Permanently"]}"); break;
					default: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{AW.g_ButtonBannedPlayer[target].iDuration} {Strlocalizer["Reply.Ban.Minutes"]}"); break;
				}

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Expires"]}: {AW.g_CFG.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(AW.g_ButtonBannedPlayer[target].iTimeStamp_Issued)}");

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Reason"]}: {AW.g_CFG.color_disabled}{AW.g_ButtonBannedPlayer[target].sReason}");
			}
			else
			{
				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Buttons.Can_use"]}");
			}
		}
		[ConsoleCommand("bw_banlist", "Displays a list of buttons ban")]
		[ConsoleCommand("css_bbanlist", "Displays a list of buttons ban")]
		[RequiresPermissions("@css/bw_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnBWBanList(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.ButtonGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			UI.ReplyToCommand(admin, bConsole, "Reply.Ban.List");
			int iCount = 0;
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(target =>
			{
				if ((AW.g_ButtonBannedPlayer.ContainsKey(target) || AW.g_ButtonBannedPlayer.TryAdd(target, new ActBanPlayer(true))) && AW.g_ButtonBannedPlayer[target].bBanned)
				{
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}***{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)}{AW.g_CFG.color_warning}***");
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Admin"]} {AW.g_CFG.color_name}{UI.PlayerInfo(AW.g_ButtonBannedPlayer[target].sAdminName, AW.g_ButtonBannedPlayer[target].sAdminSteamID)}");
					switch (AW.g_ButtonBannedPlayer[target].iDuration)
					{
						case -1: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_enabled}{Strlocalizer["Reply.Ban.Temporary"]}"); break;
						case 0: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{Strlocalizer["Reply.Ban.Permanently"]}"); break;
						default: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{AW.g_ButtonBannedPlayer[target].iDuration} {Strlocalizer["Reply.Ban.Minutes"]}"); break;
					}
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Expires"]}: {AW.g_CFG.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(AW.g_ButtonBannedPlayer[target].iTimeStamp_Issued)}");

					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Reason"]}: {AW.g_CFG.color_disabled}{AW.g_ButtonBannedPlayer[target].sReason}");

					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|__________________________________________");
					iCount++;
				}
			});
			if (iCount == 0) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.NoPlayers");
		}
		//Bban End

		//Tban Start
		[ConsoleCommand("tw_ban", "Allows the admin to issue a ban on touching triggers")]
		[ConsoleCommand("css_trban", "Allows the admin to issue a ban on touching triggers")]
		[RequiresPermissions("@css/tw_ban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<time>] [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnTWBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.TriggerGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			OfflineBan target = null;

			if (players.Count > 0)
			{
				CCSPlayerController targetOnline = players.Single();

				if (!AdminManager.CanPlayerTarget(admin, targetOnline))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.You_cannot_target");
					return;
				}

				if (!(AW.g_TriggerBannedPlayer.ContainsKey(targetOnline) || AW.g_TriggerBannedPlayer.TryAdd(targetOnline, new ActBanPlayer(true))))
				{
					UI.ReplyToCommand(admin, bConsole, "Info.Error", "Player not found in dictionary");
					return;
				}

				if (AW.g_TriggerBannedPlayer[targetOnline].bBanned)
				{
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(targetOnline)} {Strlocalizer["Reply.Triggers.Has_a_ban"]}");
					return;
				}

				foreach (OfflineBan OfflineTest in AW.g_OfflinePlayer.ToList())
				{
					if (OfflineTest.UserID == targetOnline.UserId)
					{
						target = OfflineTest;
						break;
					}
				}
			}
			else
			{
				target = OfflineFunc.FindTarget(admin, command.GetArg(1), bConsole);
			}

			if (target == null)
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
				return;
			}

			int time = Cvar.TriggerBanTime;
			if (command.ArgCount >= 2)
			{
				if (!int.TryParse(command.GetArg(2), out int timeparse))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.Must_be_an_integer");
					return;
				}
				time = timeparse;
			}

			if (time == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/tw_ban_perm"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Permanent");
				return;
			}

			if (time > Cvar.TriggerBanLong && !AdminManager.PlayerHasPermissions(admin, "@css/tw_ban_long"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Long", Cvar.TriggerBanLong);
				return;
			}

			string reason = command.GetArg(3);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.TriggerBanReason;

			UI.PrintToAllAdminAction("Chat.Admin.Triggers.Banned", AW.g_CFG.color_warning, UI.PlayerInfo(admin), AW.g_CFG.color_disabled, target.Online ? UI.PlayerInfo(target.Player) : UI.PlayerInfo(target.Name, target.SteamID));
			UI.PrintToAllAdminAction("Chat.Admin.Reason", AW.g_CFG.color_warning, reason);
			Server.NextFrame(() =>
			{
				ActBanPlayer actbanPlayer = target.Online ? AW.g_TriggerBannedPlayer[target.Player] : new ActBanPlayer(false);
				if (actbanPlayer.SetBan(admin != null ? admin.PlayerName : "Console", admin != null ? AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.Name, target.SteamID, time, reason))
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Success"); //admin.PrintToChat("Success");
						UI.TranslatedPrintToConsole("Reply.Ban.Success", 6);
						LogManager.SystemAction("Reply.Ban.Success");
					});
				}
				else
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Failed"); //admin.PrintToChat("Failed");
						UI.TranslatedPrintToConsole("Reply.Ban.Failed", 15);
						LogManager.SystemAction("Reply.Ban.Failed");
					});
				}
			});
		}

		[ConsoleCommand("tw_unban", "Allows the admin to remove the ban on touching triggers")]
		[ConsoleCommand("css_trunban", "Allows the admin to remove the ban on touching triggers")]
		[RequiresPermissions("@css/tw_unban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnTWUnBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.TriggerGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			ActBanPlayer target = new(false);
			string sTarget = command.GetArg(1);

			bool bOnline = players.Count > 0;

			if (bOnline)
			{
				CCSPlayerController targetController = players.Single();
				if (!AdminManager.CanPlayerTarget(admin, targetController))
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.You_cannot_target");
					return;
				}
				if (!(AW.g_TriggerBannedPlayer.ContainsKey(targetController) || AW.g_TriggerBannedPlayer.TryAdd(targetController, new ActBanPlayer(true))))
				{
					UI.ReplyToCommand(admin, bConsole, "Info.Error", "Player not found in dictionary");
					return;
				}
				target.bBanned = AW.g_TriggerBannedPlayer[targetController].bBanned;
				target.sAdminName = AW.g_TriggerBannedPlayer[targetController].sAdminSteamID;
				target.sAdminSteamID = AW.g_TriggerBannedPlayer[targetController].sAdminSteamID;
				target.iDuration = AW.g_TriggerBannedPlayer[targetController].iDuration;
				target.iTimeStamp_Issued = AW.g_TriggerBannedPlayer[targetController].iTimeStamp_Issued;
				target.sReason = AW.g_TriggerBannedPlayer[targetController].sReason;
				target.sClientName = targetController.PlayerName;
				target.sClientSteamID = AW.ConvertSteamID64ToSteamID(targetController.SteamID.ToString());
			}

			string reason = command.GetArg(2);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.TriggerUnBanReason;
			if (bOnline) UnBanTriggersComm(admin, players.Single(), target, reason, bConsole);
			else if (sTarget.StartsWith("#steam_", StringComparison.OrdinalIgnoreCase))
			{
				ActBanPlayer.GetBan(sTarget[1..], admin, reason, bConsole, GetBanComm_Handler, false);
			}
			else UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
		}
#nullable enable
		static void UnBanTriggersComm(CCSPlayerController? admin, CCSPlayerController? player, ActBanPlayer? target, string reason, bool bConsole)
#nullable disable
		{
			if (target == null)
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.No_matching_client");
				return;
			}

			if (!target.bBanned)
			{
				UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target.sClientName, target.sClientSteamID)} {Strlocalizer["Reply.Triggers.Can_touch"]}");
				return;
			}

			if (target.iDuration == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/tw_unban_perm"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.UnPermanent");
				return;
			}

			if (admin != null && !string.Equals(target.sAdminSteamID, AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString())) && !AdminManager.PlayerHasPermissions(admin, "@css/tw_unban_other"))
			{
				UI.ReplyToCommand(admin, bConsole, "Reply.Ban.Access.Other");
				return;
			}

			if (target.UnBan(admin != null ? admin.PlayerName : "Console", admin != null ? AW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.sClientSteamID, reason))
			{
				if (player != null) AW.g_TriggerBannedPlayer[player].bBanned = false;
				Server.NextFrame(() =>
				{
					if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.UnBan.Success"); //admin.PrintToChat("Success");
					UI.TranslatedPrintToConsole("Reply.Ban.UnBan.Success", 6);
					LogManager.SystemAction("Reply.Ban.UnBan.Success");
				});
			}
			else
			{
				Server.NextFrame(() =>
				{
					if (admin != null && admin.IsValid) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.UnBan.Failed"); //admin.PrintToChat("Failed");
					UI.TranslatedPrintToConsole("Reply.Ban.UnBan.Failed", 15);
					LogManager.SystemAction("Reply.Ban.UnBan.Failed");
				});
			}

			Server.NextFrame(() =>
			{
				UI.PrintToAllAdminAction("Chat.Admin.Triggers.Unrestricted", AW.g_CFG.color_warning, UI.PlayerInfo(admin), AW.g_CFG.color_enabled, UI.PlayerInfo(target.sClientName, target.sClientSteamID));
				UI.PrintToAllAdminAction("Chat.Admin.Reason", AW.g_CFG.color_warning, reason);
			});
		}

		[ConsoleCommand("tw_status", "Allows players to view bans for touching triggers")]
		[ConsoleCommand("css_trstatus", "Allows players to view bans for touching triggers")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnTWStatus(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!Cvar.TriggerGlobalEnable) return;
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			CCSPlayerController target = player;
			if (command.ArgCount > 1)
			{
				(List<CCSPlayerController> players, _) = Find(player, command, 1, true, false, MultipleFlags.NORMAL);

				if (players.Count == 0) return;

				target = players.Single();
			}
			if (target == null || !(AW.g_TriggerBannedPlayer.ContainsKey(target) || AW.g_TriggerBannedPlayer.TryAdd(target, new ActBanPlayer(true))))
			{
				UI.ReplyToCommand(player, bConsole, "Info.Error", "Player not found in dictionary");
				return;
			}
			if (AW.g_TriggerBannedPlayer[target].bBanned)
			{
				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Triggers.Has_a_ban"]}");

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Admin"]}: {AW.g_CFG.color_name}{UI.PlayerInfo(AW.g_TriggerBannedPlayer[target].sAdminName, AW.g_TriggerBannedPlayer[target].sAdminSteamID)}");

				switch (AW.g_TriggerBannedPlayer[target].iDuration)
				{
					case -1: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_enabled}{Strlocalizer["Reply.Ban.Temporary"]}"); break;
					case 0: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{Strlocalizer["Reply.Ban.Permanently"]}"); break;
					default: UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{AW.g_TriggerBannedPlayer[target].iDuration} {Strlocalizer["Reply.Ban.Minutes"]}"); break;
				}

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Expires"]}: {AW.g_CFG.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(AW.g_TriggerBannedPlayer[target].iTimeStamp_Issued)}");

				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Ban.Reason"]}: {AW.g_CFG.color_disabled}{AW.g_TriggerBannedPlayer[target].sReason}");
			}
			else
			{
				UI.ReplyToCommandMessage(player, bConsole, $"{AW.g_CFG.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Triggers.Can_touch"]}");
			}
		}
		[ConsoleCommand("tw_banlist", "Displays a list of bans for touching triggers")]
		[ConsoleCommand("css_trbanlist", "Displays a list of bans for touching triggers")]
		[RequiresPermissions("@css/tw_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnTWBanList(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (!Cvar.TriggerGlobalEnable) return;
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			UI.ReplyToCommand(admin, bConsole, "Reply.Ban.List");
			int iCount = 0;
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(target =>
			{
				if ((AW.g_TriggerBannedPlayer.ContainsKey(target) || AW.g_TriggerBannedPlayer.TryAdd(target, new ActBanPlayer(true))) && AW.g_TriggerBannedPlayer[target].bBanned)
				{
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}***{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)}{AW.g_CFG.color_warning}***");
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Admin"]} {AW.g_CFG.color_name}{UI.PlayerInfo(AW.g_TriggerBannedPlayer[target].sAdminName, AW.g_TriggerBannedPlayer[target].sAdminSteamID)}");
					switch (AW.g_TriggerBannedPlayer[target].iDuration)
					{
						case -1: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_enabled}{Strlocalizer["Reply.Ban.Temporary"]}"); break;
						case 0: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{Strlocalizer["Reply.Ban.Permanently"]}"); break;
						default: UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Duration"]}: {AW.g_CFG.color_disabled}{AW.g_TriggerBannedPlayer[target].iDuration} {Strlocalizer["Reply.Ban.Minutes"]}"); break;
					}
					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Expires"]}: {AW.g_CFG.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(AW.g_TriggerBannedPlayer[target].iTimeStamp_Issued)}");

					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|{Strlocalizer["Reply.Ban.Reason"]}: {AW.g_CFG.color_disabled}{AW.g_TriggerBannedPlayer[target].sReason}");

					UI.ReplyToCommandMessage(admin, bConsole, $"{AW.g_CFG.color_warning}|__________________________________________");
					iCount++;
				}
			});
			if (iCount == 0) UI.ReplyToCommand(admin, bConsole, "Reply.Ban.NoPlayers");
		}
		//Tban End
		[ConsoleCommand("aw_list", "Shows a list of players including those who have disconnected")]
		[ConsoleCommand("css_alist", "Shows a list of players including those who have disconnected")]
		[RequiresPermissions("@css/aw_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnAWList(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			UI.ReplyToCommand(admin, bConsole, "Reply.Offline.Info");

			int iCount = 0;
			double CurrentTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			foreach (OfflineBan OfflineTest in AW.g_OfflinePlayer.ToList())
			{
				iCount++;
				if (OfflineTest.Online)
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.Offline.OnServer", iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID);
				}
				else
				{
					UI.ReplyToCommand(admin, bConsole, "Reply.Offline.Leave", iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID, (int)((CurrentTime - OfflineTest.TimeStamp_Start) / 60));
				}
			}
		}

		[ConsoleCommand("css_buttons", "Disables or enables display of button activity")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnAWButtons(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!Cvar.ButtonGlobalEnable) return;
			if (player == null || !player.IsValid) return;
			if (AW.g_bButton[player.Slot])
			{
				AW.g_bButton[player.Slot] = false;
				AW.SetValueButton(player);
				UI.ReplyToCommand(player, command.CallingContext == CommandCallingContext.Console, "Reply.Buttons.Player.Disable", AW.g_CFG.color_disabled);
			}
			else
			{
				AW.g_bButton[player.Slot] = true;
				AW.SetValueButton(player);
				UI.ReplyToCommand(player, command.CallingContext == CommandCallingContext.Console, "Reply.Buttons.Player.Enable", AW.g_CFG.color_enabled);
			}
		}

		[ConsoleCommand("css_triggers", "Disables or enables display of button activity")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnAWTriggers(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!Cvar.TriggerGlobalEnable) return;
			if (player == null || !player.IsValid) return;
			if (AW.g_bTrigger[player.Slot])
			{
				AW.g_bTrigger[player.Slot] = false;
				AW.SetValueTrigger(player);
				UI.ReplyToCommand(player, command.CallingContext == CommandCallingContext.Console, "Reply.Triggers.Player.Disable", AW.g_CFG.color_disabled);
			}
			else
			{
				AW.g_bTrigger[player.Slot] = true;
				AW.SetValueTrigger(player);
				UI.ReplyToCommand(player, command.CallingContext == CommandCallingContext.Console, "Reply.Triggers.Player.Enable", AW.g_CFG.color_enabled);
			}
		}
	}
}
