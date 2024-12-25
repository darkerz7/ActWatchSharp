using ActWatchSharpAPI;
using CounterStrikeSharp.API.Core;
using EntWatchSharpAPI;

namespace ActWatchSharp
{
	internal class AWAPI : IActWatchSharpAPI
	{
		public SAWAPI_Ban Native_ButtonWatch_IsClientBanned(string sSteamID)
		{
			if (!string.IsNullOrEmpty(sSteamID))
			{
				ActBanPlayer ban = ActBanDB.GetBan(sSteamID, AW.g_CFG.server_name, true);
				if (ban != null)
				{
					SAWAPI_Ban apiban = new SAWAPI_Ban();
					apiban.bBanned = ban.bBanned;
					apiban.sAdminName = ban.sAdminName;
					apiban.sAdminSteamID = ban.sAdminSteamID;
					apiban.iDuration = ban.iDuration;
					apiban.iTimeStamp_Issued = ban.iTimeStamp_Issued;
					apiban.sReason = ban.sReason;
					apiban.sClientName = ban.sClientName;
					apiban.sClientSteamID = ban.sClientSteamID;
					return apiban;
				}
			}
			return new SAWAPI_Ban();
		}
		public bool Native_ButtonWatch_BanClient(SAWAPI_Ban sawPlayer)
		{
			return ActBanDB.BanClient(sawPlayer.sClientName, sawPlayer.sClientSteamID, sawPlayer.sAdminName, sawPlayer.sAdminSteamID, AW.g_CFG.server_name, sawPlayer.iDuration, sawPlayer.iTimeStamp_Issued, sawPlayer.sReason, true);
		}
		public bool Native_ButtonWatch_UnbanClient(SAWAPI_Ban sawPlayer)
		{
			return ActBanDB.UnBanClient(sawPlayer.sClientSteamID, sawPlayer.sAdminName, sawPlayer.sAdminSteamID, AW.g_CFG.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sawPlayer.sReason, true);
		}
		public void Native_ButtonWatch_UpdateStatusBanClient(CCSPlayerController Player)
		{
			if (!AW.g_ButtonBannedPlayer[Player].GetBan(Player)) AW.g_ButtonBannedPlayer[Player].bBanned = false;
		}
		public SAWAPI_Ban Native_TriggerWatch_IsClientBanned(string sSteamID)
		{
			if (!string.IsNullOrEmpty(sSteamID))
			{
				ActBanPlayer ban = ActBanDB.GetBan(sSteamID, AW.g_CFG.server_name, false);
				if (ban != null)
				{
					SAWAPI_Ban apiban = new SAWAPI_Ban();
					apiban.bBanned = ban.bBanned;
					apiban.sAdminName = ban.sAdminName;
					apiban.sAdminSteamID = ban.sAdminSteamID;
					apiban.iDuration = ban.iDuration;
					apiban.iTimeStamp_Issued = ban.iTimeStamp_Issued;
					apiban.sReason = ban.sReason;
					apiban.sClientName = ban.sClientName;
					apiban.sClientSteamID = ban.sClientSteamID;
					return apiban;
				}
			}
			return new SAWAPI_Ban();
		}
		public bool Native_TriggerWatch_BanClient(SAWAPI_Ban sawPlayer)
		{
			return ActBanDB.BanClient(sawPlayer.sClientName, sawPlayer.sClientSteamID, sawPlayer.sAdminName, sawPlayer.sAdminSteamID, AW.g_CFG.server_name, sawPlayer.iDuration, sawPlayer.iTimeStamp_Issued, sawPlayer.sReason, false);
		}
		public bool Native_TriggerWatch_UnbanClient(SAWAPI_Ban sawPlayer)
		{
			return ActBanDB.UnBanClient(sawPlayer.sClientSteamID, sawPlayer.sAdminName, sawPlayer.sAdminSteamID, AW.g_CFG.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sawPlayer.sReason, false);
		}
		public void Native_TriggerWatch_UpdateStatusBanClient(CCSPlayerController Player)
		{
			if (!AW.g_TriggerBannedPlayer[Player].GetBan(Player)) AW.g_TriggerBannedPlayer[Player].bBanned = false;
		}
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_BW_OnClientBanned Forward_ButtonWatch_OnClientBanned;
		public void ButtonOnClientBanned(SAWAPI_Ban sawPlayer) => Forward_ButtonWatch_OnClientBanned?.Invoke(sawPlayer);
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_BW_OnClientUnbanned Forward_ButtonWatch_OnClientUnbanned;
		public void ButtonOnClientUnbanned(SAWAPI_Ban sawPlayer) => Forward_ButtonWatch_OnClientUnbanned?.Invoke(sawPlayer);
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_BW_OnButtonPressed Forward_ButtonWatch_OnButtonPressed;
		public void ButtonOnButtonPressed(CCSPlayerController Player, string sButtonName, uint uiButtonID) => Forward_ButtonWatch_OnButtonPressed?.Invoke(Player, sButtonName, uiButtonID);
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_TW_OnClientBanned Forward_TriggerWatch_OnClientBanned;
		public void TriggerOnClientBanned(SAWAPI_Ban sawPlayer) => Forward_TriggerWatch_OnClientBanned?.Invoke(sawPlayer);
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_TW_OnClientUnbanned Forward_TriggerWatch_OnClientUnbanned;
		public void TriggerOnClientUnbanned(SAWAPI_Ban sawPlayer) => Forward_TriggerWatch_OnClientUnbanned?.Invoke(sawPlayer);
		//===================================================================================================
		public event IActWatchSharpAPI.Forward_TW_OnTriggerTouch Forward_TriggerWatch_OnTriggerTouch;
		public void TriggerOnTriggerTouch(CCSPlayerController Player, string sTriggerName, uint uiTriggerID) => Forward_TriggerWatch_OnTriggerTouch?.Invoke(Player, sTriggerName, uiTriggerID);
		//===================================================================================================
	}
}
