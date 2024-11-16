using ActWatchSharpAPI;
using CounterStrikeSharp.API.Core;

namespace ActWatchSharp
{
	internal class ActBanPlayer
	{
		public bool bBanned;

		public string sAdminName;
		public string sAdminSteamID;
		public int iDuration;
		public int iTimeStamp_Issued;
		public string sReason;

		public string sClientName;
		public string sClientSteamID;

		private bool bType;

		public async Task<bool> SetBan(string sBanAdminName, string sBanAdminSteamID, string sBanClientName, string sBanClientSteamID, int iBanDuration, string sBanReason)
		{
			if (!string.IsNullOrEmpty(sBanClientSteamID))
			{
				bBanned = true;
				sAdminName = sBanAdminName;
				sAdminSteamID = sBanAdminSteamID;
				sReason = sBanReason;
				if (iBanDuration < -1)
				{
					iDuration = -1;
					iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					return true;
				}
				else if (iBanDuration == 0)
				{
					iDuration = 0;
					iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				}
				else
				{
					iDuration = iBanDuration;
					iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + iDuration * 60;
				}
				if (AW.g_cAWAPI != null)
				{
					SAWAPI_Ban apiBan = new SAWAPI_Ban();
					apiBan.bBanned = bBanned;
					apiBan.sAdminName = sAdminName;
					apiBan.sAdminSteamID = sAdminSteamID;
					apiBan.iDuration = iDuration;
					apiBan.iTimeStamp_Issued = iTimeStamp_Issued;
					apiBan.sReason = sReason;
					apiBan.sClientName = sClientName;
					apiBan.sClientSteamID = sClientSteamID;
					if (bType) AW.g_cAWAPI.ButtonOnClientBanned(apiBan);
					else AW.g_cAWAPI.TriggerOnClientBanned(apiBan);
				}
				return await ActBanDB.BanClient(sBanClientName, sBanClientSteamID, sAdminName, sAdminSteamID, AW.g_CFG.server_name, iDuration, iTimeStamp_Issued, sReason, bType);
			}
			return false;
		}

		public async Task<bool> UnBan(string sUnBanAdminName, string sUnBanAdminSteamID, string sUnBanClientSteamID, string sUnbanReason)
		{
			if (!string.IsNullOrEmpty(sUnBanClientSteamID))
			{
				bBanned = false;
				if (string.IsNullOrEmpty(sUnbanReason)) sUnbanReason = "Amnesty";
				if (AW.g_cAWAPI != null)
				{
					SAWAPI_Ban apiBan = new SAWAPI_Ban();
					apiBan.bBanned = bBanned;
					apiBan.sAdminName = sUnBanAdminName;
					apiBan.sAdminSteamID = sUnBanAdminSteamID;
					apiBan.iDuration = 0;
					apiBan.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					apiBan.sReason = sUnbanReason;
					apiBan.sClientName = "";
					apiBan.sClientSteamID = sUnBanClientSteamID;
					if (bType) AW.g_cAWAPI.ButtonOnClientUnbanned(apiBan);
					else AW.g_cAWAPI.TriggerOnClientUnbanned(apiBan);
				}
				return await ActBanDB.UnBanClient(sUnBanClientSteamID, sUnBanAdminName, sUnBanAdminSteamID, AW.g_CFG.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sUnbanReason, bType);
			}
			return false;
		}

		public async Task<bool> GetBan(CCSPlayerController player)
		{
			if (player.IsValid)
			{
				return await ActBanDB.GetBan(player, AW.g_CFG.server_name, bType);
			}
			else if (bType)
			{
				if (AW.g_ButtonBannedPlayer.ContainsKey(player))
					AW.g_ButtonBannedPlayer.Remove(player);
			} else
			{
				if (AW.g_TriggerBannedPlayer.ContainsKey(player))
					AW.g_TriggerBannedPlayer.Remove(player);
			}
			return false;
		}

		public ActBanPlayer(bool b)
		{
			bBanned = false;
			bType = b;
		}
	}
}
