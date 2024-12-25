using ActWatchSharpAPI;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace AWSTestAPI
{
    public class AWSTest : BasePlugin
	{
		public static IActWatchSharpAPI? _AW_api;
		public override string ModuleName => "ActWatchSharp Test API";
		public override string ModuleDescription => "";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "API.0.DZ.3";
		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IActWatchSharpAPI> Capability = new("actwatch:api");
				_AW_api = IActWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				_AW_api = null;
				PrintToConsole("ActWatch API Failed!");
			}

			if (_AW_api != null)
			{
				_AW_api.Forward_ButtonWatch_OnClientBanned += DisplayButtonBan;
				_AW_api.Forward_ButtonWatch_OnClientUnbanned += DisplayButtonUnBan;
				_AW_api.Forward_ButtonWatch_OnButtonPressed += DisplayButtonPressed;
				_AW_api.Forward_TriggerWatch_OnClientBanned += DisplayTriggerBan;
				_AW_api.Forward_TriggerWatch_OnClientUnbanned += DisplayTriggerUnBan;
				_AW_api.Forward_TriggerWatch_OnTriggerTouch += DisplayTriggerTouch;
			}
		}

		void DisplayButtonBan(SAWAPI_Ban sawPlayer) { PrintToConsole($"Player {sawPlayer.sClientName} was button-banned {sawPlayer.sAdminName}"); }
		void DisplayButtonUnBan(SAWAPI_Ban sawPlayer) { PrintToConsole($"Player {sawPlayer.sClientName} was button-unbanned {sawPlayer.sAdminName}"); }
		void DisplayButtonPressed(CCSPlayerController Player, string sButtonName, uint uiButtonID) { PrintToConsole($"Player {Player.PlayerName} used button {sButtonName}({uiButtonID})"); }
		void DisplayTriggerBan(SAWAPI_Ban sawPlayer) { PrintToConsole($"Player {sawPlayer.sClientName} was trigger-banned {sawPlayer.sAdminName}"); }
		void DisplayTriggerUnBan(SAWAPI_Ban sawPlayer) { PrintToConsole($"Player {sawPlayer.sClientName} was trigger-unbanned {sawPlayer.sAdminName}"); }
		void DisplayTriggerTouch(CCSPlayerController Player, string sTriggerName, uint uiTriggerID) { PrintToConsole($"Player {Player.PlayerName} touch the trigger {sTriggerName}({uiTriggerID})"); }

		[ConsoleCommand("bwt_1", "")]
		[RequiresPermissions("@css/bw_ban")]
		public void OnBWT1(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = _AW_api.Native_ButtonWatch_IsClientBanned(ConvertSteamID64ToSteamID(player.SteamID.ToString()));
			if (ban.bBanned) PrintToConsole($"You {ban.sClientName}({ban.sClientSteamID}) have a bban. Duration: {ban.iDuration}");
			else PrintToConsole($"You have NOT a bban");
		}

		[ConsoleCommand("bwt_2", "")]
		[RequiresPermissions("@css/bw_ban")]
		public void OnBWT2(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = new SAWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iDuration = 5;
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api Ban";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (_AW_api.Native_ButtonWatch_BanClient(ban)) PrintToConsole("Successfully bbanned");
			else PrintToConsole("Failed to bban");
		}

		[ConsoleCommand("bwt_3", "")]
		[RequiresPermissions("@css/bw_unban")]
		public void OnBWT3(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = new SAWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api UnBan";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (_AW_api.Native_ButtonWatch_UnbanClient(ban)) PrintToConsole("Successfully unbbanned");
			else PrintToConsole("Failed to unbban");
		}
		[ConsoleCommand("bwt_4", "")]
		[RequiresPermissions("@css/bw_unban")]
		public void OnEWT4(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			_AW_api.Native_ButtonWatch_UpdateStatusBanClient(player);
		}
		[ConsoleCommand("twt_1", "")]
		[RequiresPermissions("@css/tw_ban")]
		public void OnTWT1(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = _AW_api.Native_TriggerWatch_IsClientBanned(ConvertSteamID64ToSteamID(player.SteamID.ToString()));
			if (ban.bBanned) PrintToConsole($"You {ban.sClientName}({ban.sClientSteamID}) have a trban. Duration: {ban.iDuration}");
			else PrintToConsole($"You have NOT a tban");
		}

		[ConsoleCommand("twt_2", "")]
		[RequiresPermissions("@css/tw_ban")]
		public void OnTWT2(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = new SAWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iDuration = 5;
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api Ban";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (_AW_api.Native_TriggerWatch_BanClient(ban)) PrintToConsole("Successfully trbanned");
			else PrintToConsole("Failed to trban");
		}

		[ConsoleCommand("twt_3", "")]
		[RequiresPermissions("@css/tw_unban")]
		public void OnTWT3(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			SAWAPI_Ban ban = new SAWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api UnBan";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (_AW_api.Native_TriggerWatch_UnbanClient(ban)) PrintToConsole("Successfully untrbanned");
			else PrintToConsole("Failed to untrban");
		}

		[ConsoleCommand("twt_4", "")]
		[RequiresPermissions("@css/tw_unban")]
		public void OnTWT4(CCSPlayerController? player, CommandInfo command)
		{
			if (_AW_api == null || player == null || !player.IsValid) return;
			_AW_api.Native_TriggerWatch_UpdateStatusBanClient(player);
		}

		public static void PrintToConsole(string sMessage)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("ActWatch:TestAPI");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)13;
			Console.WriteLine(sMessage);
			Console.ResetColor();
		}
		public static string ConvertSteamID64ToSteamID(string steamId64)
		{
			if (ulong.TryParse(steamId64, out var communityId) && communityId > 76561197960265728)
			{
				var authServer = (communityId - 76561197960265728) % 2;
				var authId = (communityId - 76561197960265728 - authServer) / 2;
				return $"STEAM_0:{authServer}:{authId}";
			}
			return null;
		}
	}
}
