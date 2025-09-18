using ActWatchSharp.Helpers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Localization;
#if (USE_ENTWATCH)
using EntWatchSharpAPI;
#endif
using ActWatchSharpAPI;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		public static IStringLocalizer Strlocalizer;
		public override string ModuleName => "[Core]ActWatchSharp";
		public override string ModuleDescription => "Notify players about activator of buttons/triggers";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.1.1";

		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IActWatchSharpAPI> CapabilityAW = new("actwatch:api");
				AW._AW_api = IActWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				AW._AW_api = null;
				UI.TranslatedPrintToConsole("Info.Error", 15, "ActWatch API Loading Failed!");
				LogManager.SystemAction("Info.Error", "ActWatch API Loading Failed!");
			}
			AW._PlayerSettingsAPI = AW._PlayerSettingsAPICapability.Get();
			if (AW._PlayerSettingsAPI == null)
			{
				UI.TranslatedPrintToConsole("Info.Error", 15, "PlayerSettings API Failed!");
				LogManager.SystemAction("Info.Error", "PlayerSettings API Failed!");
			}
#if (USE_ENTWATCH)
			try
			{
				PluginCapability<IEntWatchSharpAPI> CapabilityEW = new("entwatch:api");
				AW._EW_api = IEntWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				AW._EW_api = null;
			}
#endif
			if (hotReload)
			{
				Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(player =>
				{
					AW.LoadClientPrefs(player);
				});
			}
		}

		public override void Load(bool hotReload)
		{
			Strlocalizer = Localizer;

			AW.LoadCFG(ModuleDirectory);

			RegisterCVARS();

			try
			{
				AW.g_cAWAPI = new AWAPI();
				Capabilities.RegisterPluginCapability(IActWatchSharpAPI.Capability, () => AW.g_cAWAPI);
			}
			catch (Exception)
			{
				AW.g_cAWAPI = null;
				UI.TranslatedPrintToConsole("Info.Error", 15, "ActWatch API Register Failed!");
				LogManager.SystemAction("Info.Error", "ActWatch API Register Failed!");
			}

			if (hotReload)
			{
				Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(player =>
				{
					if (!AW.g_ButtonBannedPlayer.ContainsKey(player))
						AW.g_ButtonBannedPlayer.TryAdd(player, new ActBan.ActBanPlayer(true));

					if (!AW.g_TriggerBannedPlayer.ContainsKey(player))
						AW.g_TriggerBannedPlayer.TryAdd(player, new ActBan.ActBanPlayer(false));

					ActBan.OfflineFunc.PlayerConnectFull(player);
				});
				AW.g_TimerRetryDB = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerRetry, TimerFlags.REPEAT);
			}
			AW.g_TimerUnban = new CounterStrikeSharp.API.Modules.Timers.Timer(60.0f, TimerUnban, TimerFlags.REPEAT);

			RegEvents();
			RegVirtualFunctions();
			ActBan.ActBanDB.Init_DB(ModuleDirectory);
			LogManager.LoadConfig(ModuleDirectory);
			UI.TranslatedPrintToConsole("Info.AWLoaded");
			LogManager.SystemAction("Info.AWLoaded");
		}

		public override void Unload(bool hotReload)
		{
			UnRegVirtualFunctions();
			UnRegEvents();
			UnRegCommands();
			if (AW.g_TimerRetryDB != null)
			{
				AW.g_TimerRetryDB.Kill();
				AW.g_TimerRetryDB = null;
			}
			if (AW.g_TimerUnban != null)
			{
				AW.g_TimerUnban.Kill();
				AW.g_TimerUnban = null;
			}
			LogManager.UnInit();
		}
	}
}
