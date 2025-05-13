using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Cvars;
using ActWatchSharp.Helpers;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		public FakeConVar<int> FakeCvar_Button_bantime = new("awc_bbantime", "Default button press ban time. 0 - Permanent", 0, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 43200));
		public FakeConVar<int> FakeCvar_Button_banlong = new("awc_bbanlong", "Max button press ban time with once @css/bw_ban privilege", 720, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1440000));
		public FakeConVar<string> FakeCvar_Button_banreason = new("awc_bbanreason", "Default button press ban reason", "Trolling", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<string> FakeCvar_Button_unbanreason = new("awc_bunbanreason", "Default button press unban reason", "Giving another chance", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<bool> FakeCvar_Button_keepexpiredban = new("awc_bkeep_expired_ban", "Enable/Disable keep expired button press bans", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));

		public FakeConVar<bool> FakeCvar_Button_enable = new("awc_benable", "Enable/Disable button press functionality", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_show_button = new("awc_bshow_button", "Enable/Disable display of func_[rot_]button presses", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_show_door = new("awc_bshow_door", "Enable/Disable display of func_door[_rotating] presses", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_show_physbox = new("awc_bshow_physbox", "Enable/Disable display of func_physbox presses", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_watch_button = new("awc_bwatch_button", "Enable/Disable watch of func_[rot_]button presses. Do bans affect", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_watch_door = new("awc_bwatch_door", "Enable/Disable watch of func_door[_rotating] presses. Do bans affect", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Button_watch_physbox = new("awc_bwatch_physbox", "Enable/Disable watch of func_physbox presses. Do bans affect", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));

		public FakeConVar<int> FakeCvar_Trigger_bantime = new("awc_tbantime", "Default trigger touch ban time. 0 - Permanent", 0, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 43200));
		public FakeConVar<int> FakeCvar_Trigger_banlong = new("awc_tbanlong", "Max trigger touch ban time with once @css/tw_ban privilege", 720, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1440000));
		public FakeConVar<string> FakeCvar_Trigger_banreason = new("awc_tbanreason", "Default trigger touch ban reason", "Trolling", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<string> FakeCvar_Trigger_unbanreason = new("awc_tunbanreason", "Default trigger touch unban reason", "Giving another chance", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<bool> FakeCvar_Trigger_keepexpiredban = new("awc_tkeep_expired_ban", "Enable/Disable keep expired trigger touch bans", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));

		public FakeConVar<bool> FakeCvar_Trigger_enable = new("awc_tenable", "Enable/Disable trigger touch functionality", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Trigger_show_once = new("awc_tshow_once", "Enable/Disable display of trigger_once touching", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Trigger_show_multiple = new("awc_tshow_multiple", "Enable/Disable display of trigger_multiple touching", false, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Trigger_watch_once = new("awc_twatch_once", "Enable/Disable watch of trigger_once touching. Do bans affect.[BUG] When touched by a banned trigger disappears", false, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_Trigger_watch_multiple = new("awc_twatch_multiple", "Enable/Disable watch of trigger_multiple touching. Do bans affect", false, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));

		public FakeConVar<int> FakeCvar_offline_clear_time = new("awc_offline_clear_time", "Time during which data is stored (1-240)", 30, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 240));
		public FakeConVar<byte> FakeCvar_playerformat = new("awc_player_format", "Changes the way player information is displayed by default (0 - Only Nickname, 1 - Nickname and UserID, 2 - Nickname and SteamID, 3 - Nickname, UserID and SteamID)", 3, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<byte>(0, 3));

		private void RegisterCVARS()
		{
			FakeCvar_Button_bantime.ValueChanged += (sender, value) =>
			{
				if (value >= 0 && value <= 43200) Cvar.ButtonBanTime = value;
				else Cvar.ButtonBanTime = 0;
				UI.CvarChangeNotify(FakeCvar_Button_bantime.Name, value.ToString(), FakeCvar_Button_bantime.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_banlong.ValueChanged += (sender, value) =>
			{
				if (value >= 1 && value <= 1440000) Cvar.ButtonBanLong = value;
				else Cvar.ButtonBanLong = 720;
				UI.CvarChangeNotify(FakeCvar_Button_banlong.Name, value.ToString(), FakeCvar_Button_banlong.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_banreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.ButtonBanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_Button_banreason.Name, Cvar.ButtonBanReason.ToString(), FakeCvar_Button_banreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_Button_unbanreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.ButtonUnBanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_Button_unbanreason.Name, Cvar.ButtonUnBanReason.ToString(), FakeCvar_Button_unbanreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_Button_keepexpiredban.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonKeepExpiredBan = value;
				UI.CvarChangeNotify(FakeCvar_Button_keepexpiredban.Name, value.ToString(), FakeCvar_Button_keepexpiredban.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_Button_enable.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonGlobalEnable = value;
				UI.CvarChangeNotify(FakeCvar_Button_enable.Name, value.ToString(), FakeCvar_Button_enable.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_show_button.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonShowButton = value;
				UI.CvarChangeNotify(FakeCvar_Button_show_button.Name, value.ToString(), FakeCvar_Button_show_button.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_show_door.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonShowDoor = value;
				UI.CvarChangeNotify(FakeCvar_Button_show_door.Name, value.ToString(), FakeCvar_Button_show_door.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_show_physbox.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonShowPhysbox = value;
				UI.CvarChangeNotify(FakeCvar_Button_show_physbox.Name, value.ToString(), FakeCvar_Button_show_physbox.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_watch_button.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonWatchButton = value;
				UI.CvarChangeNotify(FakeCvar_Button_watch_button.Name, value.ToString(), FakeCvar_Button_watch_button.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_watch_door.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonWatchDoor = value;
				UI.CvarChangeNotify(FakeCvar_Button_watch_door.Name, value.ToString(), FakeCvar_Button_watch_door.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Button_watch_physbox.ValueChanged += (sender, value) =>
			{
				Cvar.ButtonWatchPhysbox = value;
				UI.CvarChangeNotify(FakeCvar_Button_watch_physbox.Name, value.ToString(), FakeCvar_Button_watch_physbox.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};


			FakeCvar_Trigger_bantime.ValueChanged += (sender, value) =>
			{
				if (value >= 0 && value <= 43200) Cvar.TriggerBanTime = value;
				else Cvar.TriggerBanTime = 0;
				UI.CvarChangeNotify(FakeCvar_Trigger_bantime.Name, value.ToString(), FakeCvar_Trigger_bantime.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_banlong.ValueChanged += (sender, value) =>
			{
				if (value >= 1 && value <= 1440000) Cvar.TriggerBanLong = value;
				else Cvar.TriggerBanLong = 720;
				UI.CvarChangeNotify(FakeCvar_Trigger_banlong.Name, value.ToString(), FakeCvar_Trigger_banlong.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_banreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.TriggerBanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_Trigger_banreason.Name, Cvar.TriggerBanReason.ToString(), FakeCvar_Trigger_banreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_Trigger_unbanreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.TriggerUnBanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_Trigger_unbanreason.Name, Cvar.TriggerUnBanReason.ToString(), FakeCvar_Trigger_unbanreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_Trigger_keepexpiredban.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerKeepExpiredBan = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_keepexpiredban.Name, value.ToString(), FakeCvar_Trigger_keepexpiredban.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_Trigger_enable.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerGlobalEnable = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_enable.Name, value.ToString(), FakeCvar_Trigger_enable.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_show_once.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerShowOnce = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_show_once.Name, value.ToString(), FakeCvar_Trigger_show_once.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_show_multiple.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerShowMultiple = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_show_multiple.Name, value.ToString(), FakeCvar_Trigger_show_multiple.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_watch_once.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerWatchOnce = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_watch_once.Name, value.ToString(), FakeCvar_Trigger_watch_once.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_Trigger_watch_multiple.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerWatchMultiple = value;
				UI.CvarChangeNotify(FakeCvar_Trigger_watch_multiple.Name, value.ToString(), FakeCvar_Trigger_watch_multiple.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_offline_clear_time.ValueChanged += (sender, value) =>
			{
				if (value >= 1 && value <= 240) Cvar.OfflineClearTime = value;
				else Cvar.OfflineClearTime = 30;
				UI.CvarChangeNotify(FakeCvar_offline_clear_time.Name, value.ToString(), FakeCvar_offline_clear_time.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_playerformat.ValueChanged += (sender, value) =>
			{
				Cvar.PlayerFormat = value;
				UI.CvarChangeNotify(FakeCvar_playerformat.Name, value.ToString(), FakeCvar_playerformat.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			RegisterFakeConVars(typeof(ConVar));
		}
	}

	static class Cvar
	{
		public static int ButtonBanTime = 0;
		public static int ButtonBanLong = 720;
		public static string ButtonBanReason = "Trolling";
		public static string ButtonUnBanReason = "Giving another chance";
		public static bool ButtonKeepExpiredBan = true;

		public static bool ButtonGlobalEnable = true;
		public static bool ButtonShowButton = true;
		public static bool ButtonShowDoor = true;
		public static bool ButtonShowPhysbox = true;
		public static bool ButtonWatchButton = true;
		public static bool ButtonWatchDoor = true;
		public static bool ButtonWatchPhysbox = true;

		public static int TriggerBanTime = 0;
		public static int TriggerBanLong = 720;
		public static string TriggerBanReason = "Trolling";
		public static string TriggerUnBanReason = "Giving another chance";
		public static bool TriggerKeepExpiredBan = true;

		public static bool TriggerGlobalEnable = true;
		public static bool TriggerShowOnce = true;
		public static bool TriggerShowMultiple = false;
		public static bool TriggerWatchOnce = false;
		public static bool TriggerWatchMultiple = false;

		public static int OfflineClearTime = 30;
		public static byte PlayerFormat = 3;
	}
}
