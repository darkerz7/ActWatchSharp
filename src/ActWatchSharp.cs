using ActWatchSharp.Helpers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Localization;
using EntWatchSharpAPI;
using ClientPrefsAPI;
using ActWatchSharpAPI;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		public static IStringLocalizer Strlocalizer;
		public override string ModuleName => "[Core]ActWatchSharp";
		public override string ModuleDescription => "Notify players about activator of buttons/triggers";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "0.DZ.1.beta";

		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IActWatchSharpAPI> CapabilityEW = new("actwatch:api");
				AW._AW_api = IActWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				AW._AW_api = null;
				UI.TranslatedPrintToConsole("Info.Error", 15, "ActWatch API Loading Failed!");
				LogManager.SystemAction("Info.Error", "ActWatch API Loading Failed!");
			}
			try
			{
				PluginCapability<IClientPrefsAPI> CapabilityCP = new("clientprefs:api");
				AW._CP_api = IClientPrefsAPI.Capability.Get();
			}
			catch (Exception)
			{
				AW._CP_api = null;
				UI.TranslatedPrintToConsole("Info.Error", 15, "ClientPrefs API Failed!");
				LogManager.SystemAction("Info.Error", "ClientPrefs API Failed!");
			}
			try
			{
				PluginCapability<IEntWatchSharpAPI> CapabilityEW = new("entwatch:api");
				AW._EW_api = IEntWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				AW._EW_api = null;
			}
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
						AW.g_ButtonBannedPlayer.TryAdd(player, new ActBanPlayer(true));

					if (!AW.g_TriggerBannedPlayer.ContainsKey(player))
						AW.g_TriggerBannedPlayer.TryAdd(player, new ActBanPlayer(false));

					ActBan.OfflineFunc.PlayerConnectFull(player);
				});
				AW.g_TimerRetryDB = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerRetry, TimerFlags.REPEAT);
			}
			AW.g_TimerUnban = new CounterStrikeSharp.API.Modules.Timers.Timer(60.0f, TimerUnban, TimerFlags.REPEAT);

			RegEvents();
			RegVirtualFunctions();
			ActBanDB.Init_DB(ModuleDirectory);
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
