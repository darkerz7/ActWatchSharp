using ActWatchSharp.ActBan;
using CounterStrikeSharp.API.Core;
#if (USE_ENTWATCH)
using EntWatchSharpAPI;
#endif
using System.Text.Json;
using ActWatchSharpAPI;
using CounterStrikeSharp.API.Core.Capabilities;
using PlayerSettings;

namespace ActWatchSharp
{
	static class AW
	{
		public static IActWatchSharpAPI _AW_api;
		public static AWAPI g_cAWAPI = null;
#if (USE_ENTWATCH)
		public static IEntWatchSharpAPI _EW_api;
#endif
#nullable enable
		public static ISettingsApi? _PlayerSettingsAPI;
		public static readonly PluginCapability<ISettingsApi?> _PlayerSettingsAPICapability = new("settings:nfcore");
#nullable disable

		public static PluginConfig g_CFG = new();
		public static Dictionary<CCSPlayerController, ActBanPlayer> g_ButtonBannedPlayer = [];
		public static Dictionary<CCSPlayerController, ActBanPlayer> g_TriggerBannedPlayer = [];
		public static List<OfflineBan> g_OfflinePlayer = [];
		public static bool[] g_bButton = new bool[65];
		public static bool[] g_bTrigger = new bool[65];
		public static int[] g_iFormatPlayer = new int[65];

		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerRetryDB = null;
		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerUnban = null;

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
		public static void LoadCFG(string ModuleDirectory)
		{
			string sConfig = $"{Path.Join(ModuleDirectory, "plugin_config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				sData = File.ReadAllText(sConfig);
				g_CFG = JsonSerializer.Deserialize<PluginConfig>(sData);
				g_CFG ??= new PluginConfig();
			}
			else g_CFG = new PluginConfig();
		}
#nullable enable
		public static void LoadClientPrefs(CCSPlayerController? player)
#nullable disable
		{
			GetValueButton(player);
			GetValueTrigger(player);
			GetValuePIFormmat(player);
		}
#nullable enable
		public static void GetValueButton(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				string sValue = _PlayerSettingsAPI.GetPlayerSettingsValue(player, "AW_Button", "0");
				if (string.IsNullOrEmpty(sValue) || !Int32.TryParse(sValue, out int iValue)) iValue = 0;
				if (iValue == 0) g_bButton[player.Slot] = false;
				else g_bButton[player.Slot] = true;
			}
		}
#nullable enable
		public static void SetValueButton(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				if (g_bButton[player.Slot]) _PlayerSettingsAPI.SetPlayerSettingsValue(player, "AW_Button", "1");
				else _PlayerSettingsAPI.SetPlayerSettingsValue(player, "AW_Button", "0");
			}
		}
#nullable enable
		public static void GetValueTrigger(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				string sValue = _PlayerSettingsAPI.GetPlayerSettingsValue(player, "AW_Trigger", "0");
				if (string.IsNullOrEmpty(sValue) || !Int32.TryParse(sValue, out int iValue)) iValue = 0;
				if (iValue == 0) g_bTrigger[player.Slot] = false;
				else g_bTrigger[player.Slot] = true;
			}
		}
#nullable enable
		public static void SetValueTrigger(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				if (g_bTrigger[player.Slot]) _PlayerSettingsAPI.SetPlayerSettingsValue(player, "AW_Trigger", "1");
				else _PlayerSettingsAPI.SetPlayerSettingsValue(player, "AW_Trigger", "0");
			}
		}

#nullable enable
		public static void GetValuePIFormmat(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				string sValue = _PlayerSettingsAPI.GetPlayerSettingsValue(player, "AW_PInfo_Format", $"{Cvar.PlayerFormat}");
				if (string.IsNullOrEmpty(sValue) || !Int32.TryParse(sValue, out int iValue)) iValue = Cvar.PlayerFormat;
				g_iFormatPlayer[player.Slot] = iValue;
			}
		}
#nullable enable
		public static void SetValuePIFormmat(CCSPlayerController? player)
#nullable disable
		{
			if (player == null || !player.IsValid) return;
			if (_PlayerSettingsAPI != null)
			{
				_PlayerSettingsAPI.SetPlayerSettingsValue(player, "AW_PInfo_Format", $"{g_iFormatPlayer[player.Slot]}");
			}
		}
#nullable enable
		public static CCSPlayerController? EntityIsPlayer(CEntityInstance? entity)
#nullable disable
		{
			if (entity != null && entity.IsValid && string.Equals(entity.DesignerName, "player"))
			{
				var pawn = new CCSPlayerPawn(entity.Handle);
				if (pawn.Controller.Value != null && pawn.Controller.Value.IsValid)
				{
					var player = new CCSPlayerController(pawn.Controller.Value.Handle);
					if (player != null && player.IsValid) return player;
				}
			}
			return null;
		}
	}

	internal class PluginConfig
	{
		public string server_name { get; set; }
		public string color_enabled { get; set; }
		public string color_disabled { get; set; }
		public string color_warning { get; set; }
		public string color_name { get; set; }
		public string color_steamid { get; set; }
		public PluginConfig()
		{
			server_name = "Server";
			color_enabled = "{green}";
			color_disabled = "{red}";
			color_warning = "{orange}";
			color_name = "{default}";
			color_steamid = "{grey}";
		}
	}
}
