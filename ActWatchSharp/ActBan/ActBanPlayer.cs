using ActWatchSharp.Helpers;
using ActWatchSharpAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ActWatchSharp.ActBan
{
	internal class ActBanPlayer(bool b)
	{
		public bool bBanned = false;

		public string sAdminName;
		public string sAdminSteamID;
		public int iDuration;
		public int iTimeStamp_Issued;
		public string sReason;

		public string sClientName;
		public string sClientSteamID;

		private bool bType = b;

		public bool SetBan(string sBanAdminName, string sBanAdminSteamID, string sBanClientName, string sBanClientSteamID, int iBanDuration, string sBanReason)
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
					SAWAPI_Ban apiBan = new()
					{
						bBanned = bBanned,
						sAdminName = sAdminName,
						sAdminSteamID = sAdminSteamID,
						iDuration = iDuration,
						iTimeStamp_Issued = iTimeStamp_Issued,
						sReason = sReason,
						sClientName = sClientName,
						sClientSteamID = sClientSteamID
					};
					if (bType) AW.g_cAWAPI.ButtonOnClientBanned(apiBan);
					else AW.g_cAWAPI.TriggerOnClientBanned(apiBan);
				}
				ActBanDB.BanClient(sBanClientName, sBanClientSteamID, sAdminName, sAdminSteamID, AW.g_CFG.server_name, iDuration, iTimeStamp_Issued, sReason, bType);
				return true;
			}
			return false;
		}

		public bool UnBan(string sUnBanAdminName, string sUnBanAdminSteamID, string sUnBanClientSteamID, string sUnbanReason)
		{
			if (!string.IsNullOrEmpty(sUnBanClientSteamID))
			{
				bBanned = false;
				if (string.IsNullOrEmpty(sUnbanReason)) sUnbanReason = "Amnesty";
				if (AW.g_cAWAPI != null)
				{
					SAWAPI_Ban apiBan = new()
					{
						bBanned = bBanned,
						sAdminName = sUnBanAdminName,
						sAdminSteamID = sUnBanAdminSteamID,
						iDuration = 0,
						iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
						sReason = sUnbanReason,
						sClientName = "",
						sClientSteamID = sUnBanClientSteamID
					};
					if (bType) AW.g_cAWAPI.ButtonOnClientUnbanned(apiBan);
					else AW.g_cAWAPI.TriggerOnClientUnbanned(apiBan);
				}
				ActBanDB.UnBanClient(sUnBanClientSteamID, sUnBanAdminName, sUnBanAdminSteamID, AW.g_CFG.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sUnbanReason, bType);
				return true;
			}
			return false;
		}

		public static void GetBan(CCSPlayerController player, bool bType, bool bShow = false)
		{
			ActBanDB.GetBan(player, AW.g_CFG.server_name, GetBanPlayer_Handler, bType, bShow);
		}
		static ActBanDB.GetBanPlayerFunc GetBanPlayer_Handler = (CCSPlayerController player, List<List<string>> DBQuery_Result, bool bType, bool bShow) =>
		{
			if (player.IsValid && AW.g_ButtonBannedPlayer.ContainsKey(player))
			{
				if (DBQuery_Result.Count > 0)
				{
					Dictionary<CCSPlayerController, ActBanPlayer> dActBan = bType ? AW.g_ButtonBannedPlayer : AW.g_TriggerBannedPlayer;
					if (dActBan.TryGetValue(player, out ActBanPlayer value))
					{
						value.bBanned = true;
						value.sAdminName = DBQuery_Result[0][0];
						value.sAdminSteamID = DBQuery_Result[0][1];
						value.iDuration = Convert.ToInt32(DBQuery_Result[0][2]);
						value.iTimeStamp_Issued = Convert.ToInt32(DBQuery_Result[0][3]);
						value.sReason = DBQuery_Result[0][4];
					}
					if (bShow)
						Server.NextFrame(() =>
						{
							if (dActBan[player].bBanned) UI.TranslatedPrintToConsole("Info.Ban.PlayerConnect", 4, UI.PlayerInfoFormat(player)[3], "Buttons Ban", dActBan[player].iDuration, dActBan[player].iTimeStamp_Issued, UI.PlayerInfoFormat(dActBan[player].sAdminName, dActBan[player].sAdminSteamID)[3], dActBan[player].sReason);
						});
				}
				else
				{
					Dictionary<CCSPlayerController, ActBanPlayer> dActBan = bType ? AW.g_ButtonBannedPlayer : AW.g_TriggerBannedPlayer;
					if (dActBan.TryGetValue(player, out ActBanPlayer value)) value.bBanned = false;
				}
			}
		};
#nullable enable
		public static void GetBan(string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, ActBanDB.GetBanCommFunc handler, bool bType)
#nullable disable
		{
			ActBanDB.GetBan(sClientSteamID, AW.g_CFG.server_name, admin, reason, bConsole, handler, bType);
		}
		public static void GetBan(string sClientSteamID, ActBanDB.GetBanAPIFunc handler, bool bType)
		{
			ActBanDB.GetBan(sClientSteamID, AW.g_CFG.server_name, handler, bType);
		}
	}
}
