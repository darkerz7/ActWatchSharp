using ActWatchSharp.ActBan;
using ActWatchSharp.Helpers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static CounterStrikeSharp.API.Core.Listeners;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		public void RegEvents()
		{
			RegisterListener<OnMapStart>(OnMapStart_Listener);
			RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			RegisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);
			HookEntityOutput("func_button", "OnPressed", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller, 0)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_rot_button", "OnPressed", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller, 0)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_door", "OnOpen", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller, 1)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_door_rotating", "OnOpen", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller, 1)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_physbox", "OnPlayerUse", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller, 2)) return HookResult.Handled;
				return HookResult.Continue;
			});
		}

		public void UnRegEvents()
		{
			RemoveListener<OnMapStart>(OnMapStart_Listener);
			DeregisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			DeregisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);
		}

		private void OnMapStart_Listener(string sMapName)
		{
			LogManager.SystemAction("Info.ChangeMap", sMapName);
		}

		[GameEventHandler]
		private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
		{
			if (@event.Userid == null) return HookResult.Continue;
			OfflineFunc.PlayerConnectFull(@event.Userid);

			CCSPlayerController pl = new(@event.Userid.Handle);
			if (pl.IsValid)
			{
				if (AW.g_ButtonBannedPlayer.ContainsKey(pl) || AW.g_ButtonBannedPlayer.TryAdd(pl, new ActBanPlayer(true)))  //Add Bban
				{
					if (Cvar.ButtonGlobalEnable) ActBanPlayer.GetBan(pl, true, true); //Set Bban
				}
				if (AW.g_TriggerBannedPlayer.ContainsKey(pl) || AW.g_TriggerBannedPlayer.TryAdd(pl, new ActBanPlayer(false)))  //Add Tban
				{
					if (Cvar.TriggerGlobalEnable) ActBanPlayer.GetBan(pl, false, true); //Set Tban
				}
				AW.g_bButton[pl.Slot] = true;
				AW.g_bTrigger[pl.Slot] = true;
				AW.LoadClientPrefs(pl);
			}

			return HookResult.Continue;
		}

		[GameEventHandler]
		private HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
		{
			if (@event.Userid == null) return HookResult.Continue;

			AW.g_bButton[@event.Userid.Slot] = false;
			AW.g_bTrigger[@event.Userid.Slot] = false;

			OfflineFunc.PlayerDisconnect(@event.Userid);

			if (AW.g_ButtonBannedPlayer.ContainsKey(@event.Userid))
				AW.g_ButtonBannedPlayer.Remove(@event.Userid);   //Remove Bban

			if (AW.g_TriggerBannedPlayer.ContainsKey(@event.Userid))
				AW.g_TriggerBannedPlayer.Remove(@event.Userid);   //Remove Tban

			return HookResult.Continue;
		}

		private void TimerRetry()
		{
			//Reban after reload plugin
			if (ActBanDB.db.bDBReady)
			{
				Utilities.GetPlayers().ForEach(player =>
				{
					if (player.IsValid)
					{
						if (AW.g_ButtonBannedPlayer.ContainsKey(player) || AW.g_ButtonBannedPlayer.TryAdd(player, new ActBanPlayer(true)))
						{
							if (Cvar.ButtonGlobalEnable) ActBanPlayer.GetBan(player, true, false);
						}
						if (AW.g_TriggerBannedPlayer.ContainsKey(player) || AW.g_TriggerBannedPlayer.TryAdd(player, new ActBanPlayer(false)))
						{
							if (Cvar.TriggerGlobalEnable) ActBanPlayer.GetBan(player, false, false);
						}
					}
				});
				if (AW.g_TimerRetryDB != null)
				{
					AW.g_TimerRetryDB.Kill();
					AW.g_TimerRetryDB = null;
				}
			}
		}

		private void TimerUnban()
		{
			string sServerName = AW.g_CFG.server_name;
			if (!string.IsNullOrEmpty(sServerName)) { sServerName = "Server"; }

			if (Cvar.ButtonGlobalEnable) ActBanDB.OfflineUnban(sServerName, true);
			if (Cvar.TriggerGlobalEnable) ActBanDB.OfflineUnban(sServerName, false);

			Task.Run(() =>
			{
				ActBan.OfflineFunc.TimeToClear();
			});

			//Update (Un)Bans
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid)
				{
					if (AW.g_ButtonBannedPlayer.ContainsKey(player) || AW.g_ButtonBannedPlayer.TryAdd(player, new ActBanPlayer(true)))
					{
						if (Cvar.ButtonGlobalEnable) ActBanPlayer.GetBan(player, true, false);
					}
					if (AW.g_TriggerBannedPlayer.ContainsKey(player) || AW.g_TriggerBannedPlayer.TryAdd(player, new ActBanPlayer(false)))
					{
						if (Cvar.TriggerGlobalEnable) ActBanPlayer.GetBan(player, false, false);
					}
				}
			});
		}

		private static bool OnButtonPressed(CEntityInstance activator, CEntityInstance caller, byte iType)
		{
			if (!Cvar.ButtonGlobalEnable) return true;

			CCSPlayerController player = AW.EntityIsPlayer(activator);

			if (player == null || caller == null || !caller.IsValid) return true;

			//restrict + check EW
#if (USE_ENTWATCH)
			if (AW._EW_api != null && AW._EW_api.Native_EntWatch_IsButtonSpecialItem(caller)) return true;
#endif
			switch (iType)
			{
				case 0: if (Cvar.ButtonWatchButton && AW.g_ButtonBannedPlayer[player].bBanned) return false; break;
				case 1: if (Cvar.ButtonWatchDoor && AW.g_ButtonBannedPlayer[player].bBanned) return false; break;
				case 2: if (Cvar.ButtonWatchPhysbox && AW.g_ButtonBannedPlayer[player].bBanned) return false; break;
			}

			string sButtonName = string.IsNullOrEmpty(caller.Entity?.Name) ? "" : caller.Entity?.Name;

			//api
			AW.g_cAWAPI?.ButtonOnButtonPressed(player, sButtonName, caller.Index);

			//show
			bool bShow = false;
			switch(iType)
			{
				case 0: if (Cvar.ButtonShowButton) bShow = true; break;
				case 1: if (Cvar.ButtonShowDoor) bShow = true; break;
				case 2: if (Cvar.ButtonShowPhysbox) bShow = true; break;
			}
			if (bShow) UI.PrintToAllActAction("Reply.Buttons.Activate", UI.PlayerInfoFormat(player), sButtonName, caller.Index, true);

			return true;
		}

		private HookResult OnTriggerStartTouch(DynamicHook hook)
		{
			if (!Cvar.TriggerGlobalEnable) return HookResult.Continue;
			try
			{
				var entity = hook.GetParam<CBaseEntity>(1);
				CCSPlayerController player = AW.EntityIsPlayer(entity);
				if (player == null) return HookResult.Continue;

				var trigger = hook.GetParam<CBaseTrigger>(0);

				if (trigger == null || !trigger.IsValid) return HookResult.Continue;

				if (string.Equals(trigger.DesignerName, "trigger_once"))
				{
					if (Cvar.TriggerWatchOnce && AW.g_TriggerBannedPlayer[player].bBanned) return HookResult.Handled;
					
					string sTriggerName = string.IsNullOrEmpty(trigger.Entity?.Name) ? "" : trigger.Entity?.Name;

					AW.g_cAWAPI?.TriggerOnTriggerTouch(player, sTriggerName, trigger.Index);

					if (Cvar.TriggerShowOnce) UI.PrintToAllActAction("Reply.Triggers.StartTouch", UI.PlayerInfoFormat(player), sTriggerName, trigger.Index, false);
				} else if (string.Equals(trigger.DesignerName, "trigger_multiple"))
				{
					if (Cvar.TriggerWatchMultiple && AW.g_TriggerBannedPlayer[player].bBanned) return HookResult.Handled;

					string sTriggerName = string.IsNullOrEmpty(trigger.Entity?.Name) ? "" : trigger.Entity?.Name;

					AW.g_cAWAPI?.TriggerOnTriggerTouch(player, sTriggerName, trigger.Index);

					if (Cvar.TriggerShowMultiple) UI.PrintToAllActAction("Reply.Triggers.StartTouch", UI.PlayerInfoFormat(player), sTriggerName, trigger.Index, false);
				}			
			}
			catch (Exception) { }
			return HookResult.Continue;
		}
	}
}
