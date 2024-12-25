using CounterStrikeSharp.API.Core;
using System.Data;
using System.Text.Json;
using ActWatchSharp.Helpers;
using System.Reflection.PortableExecutable;

namespace ActWatchSharp
{
	static class ActBanDB
	{
		public static Database db;
		private static DBConfig dbConfig;

		public static void Init_DB(string ModuleDirectory)
		{
			string sConfig = $"{Path.Join(ModuleDirectory, "db_config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				sData = File.ReadAllText(sConfig);
				dbConfig = JsonSerializer.Deserialize<DBConfig>(sData);
				if (dbConfig == null) dbConfig = new DBConfig();
			}
			else dbConfig = new DBConfig();
			if (dbConfig.TypeDB == "mysql") db = new DB_Mysql(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
			else if (dbConfig.TypeDB == "postgre") db = new DB_PosgreSQL(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
			else
			{
				dbConfig.TypeDB = "sqlite";
				string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
				db = new DB_SQLite(sDBFile);
			}
			if (db.bSuccess)
			{
				UI.TranslatedPrintToConsole("Info.DB.Success", 6, dbConfig.TypeDB);
				LogManager.SystemAction("Info.DB.Success", dbConfig.TypeDB);
				#pragma warning disable CS8625
				if (dbConfig.TypeDB == "sqlite")
				{
					db.AnyDB.QueryAsync(CreateTableSQL_SQLite(TablePrefix(true) + TablePostfix(true)) + CreateTableSQL_SQLite(TablePrefix(true) + TablePostfix(false)) + CreateTableSQL_SQLite(TablePrefix(false) + TablePostfix(true)) + CreateTableSQL_SQLite(TablePrefix(false) + TablePostfix(false)), null, (_) =>
					{
						db.bDBReady = true;
					}, true);
				}
				else if (dbConfig.TypeDB == "mysql")
				{
					db.AnyDB.QueryAsync(CreateTableSQL_MySQL(TablePrefix(true) + TablePostfix(true)) + CreateTableSQL_MySQL(TablePrefix(true) + TablePostfix(false)) + CreateTableSQL_MySQL(TablePrefix(false) + TablePostfix(true)) + CreateTableSQL_MySQL(TablePrefix(false) + TablePostfix(false)), null, (_) =>
					{
						db.bDBReady = true;
					}, true);
				}
				else if (dbConfig.TypeDB == "postgre")
				{
					db.AnyDB.QueryAsync(CreateTableSQL_PostgreSQL(TablePrefix(true) + TablePostfix(true)) + CreateTableSQL_PostgreSQL(TablePrefix(true) + TablePostfix(false)) + CreateTableSQL_PostgreSQL(TablePrefix(false) + TablePostfix(true)) + CreateTableSQL_PostgreSQL(TablePrefix(false) + TablePostfix(false)), null, (_) =>
					{
						db.bDBReady = true;
					}, true);
				}
				#pragma warning restore CS8625
			}
			else
			{
				UI.TranslatedPrintToConsole("Info.DB.Failed", 15, dbConfig.TypeDB);
				LogManager.SystemAction("Info.DB.Failed", dbConfig.TypeDB);
			}
		}

		public static bool BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				#pragma warning disable CS8625
				db.AnyDB.QueryAsync("INSERT INTO " + TablePrefix(bType) + TablePostfix(true) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES ('{ARG}', '{ARG}', '{ARG}', '{ARG}', '{ARG}', {ARG}, {ARG}, '{ARG}');", new List<string>([sClientName, sClientSteamID, sAdminName, sAdminSteamID, sServer, iDuration.ToString(), iTimeStamp.ToString(), sReason]), null, true);
				#pragma warning restore CS8625
				return true;
			}
			return false;
		}

		public static bool UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				#pragma warning disable CS8625
				if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
					db.AnyDB.QueryAsync("UPDATE " + TablePrefix(bType) + TablePostfix(true) + " SET reason_unban = '{ARG}', admin_name_unban = '{ARG}', admin_steamid_unban = '{ARG}', timestamp_unban = {ARG} " +
											"WHERE client_steamid='{ARG}' and server='{ARG}' and admin_steamid_unban IS NULL;", new List<string>([sReason, sAdminName, sAdminSteamID, iTimeStamp.ToString(), sClientSteamID, sServer]), (_) =>
					{
						db.AnyDB.QueryAsync("INSERT INTO " + TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
												"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM " + TablePrefix(bType) + TablePostfix(true) +
													" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (_) =>
						{
							db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) +
													" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), null, true);
						}, true);
					}, true);
				else db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), null, true);
				#pragma warning restore CS8625
				return true;
			}
			return false;
		}

		public static bool GetBan(CCSPlayerController player, string sServer, bool bType)
		{
			try
			{
				if (player.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					var res = db.AnyDB.Query("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM " + TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([AW.ConvertSteamID64ToSteamID(player.SteamID.ToString()), sServer]));
					if (res.Count > 0)
					{
						Dictionary<CCSPlayerController, ActBanPlayer> dActBan = bType ? AW.g_ButtonBannedPlayer : AW.g_TriggerBannedPlayer;
						if (dActBan.ContainsKey(player))
						{
							dActBan[player].bBanned = true;
							dActBan[player].sAdminName = res[0][0];
							dActBan[player].sAdminSteamID = res[0][1];
							dActBan[player].iDuration = Convert.ToInt32(res[0][2]);
							dActBan[player].iTimeStamp_Issued = Convert.ToInt32(res[0][3]);
							dActBan[player].sReason = res[0][4];

							return true;
						}
						else
						{
							dActBan.TryAdd(player, new ActBanPlayer(bType));
							return GetBan(player, sServer, bType);
						}
					}
					return false;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return false;
		}

		public static ActBanPlayer GetBan(string SteamID, string sServer, bool bType)
		{
			try
			{
				if (!string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					var res = db.AnyDB.Query("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM " + TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([SteamID, sServer]));
					if (res.Count > 0)
					{
						ActBanPlayer player = new ActBanPlayer(bType);
						player.bBanned = true;
						player.sAdminName = res[0][0];
						player.sAdminSteamID = res[0][1];
						player.iDuration = Convert.ToInt32(res[0][2]);
						player.iTimeStamp_Issued = Convert.ToInt32(res[0][3]);
						player.sReason = res[0][4];
						player.sClientName = res[0][5];
						player.sClientSteamID = SteamID;
						return player;
					}
					return null;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return null;
		}

		public static bool OfflineUnban(string sServer, int iTime, bool bType)
		{
			if (db.bDBReady)
			{
				var res = db.AnyDB.Query("SELECT id FROM " + TablePrefix(bType) + TablePostfix(true) +
											" WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG};", new List<string>([sServer, iTime.ToString()]));

				if (res.Count > 0)
				{
					#pragma warning disable CS8625
					for (int i = 0; i < res.Count; i++)
					{
						string sID = res[i][0];
						if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
							db.AnyDB.QueryAsync("UPDATE " + TablePrefix(bType) + TablePostfix(true) + " SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban={ARG} WHERE id={ARG};", new List<string>([iTime.ToString(), sID]), (_) =>
							{
								db.AnyDB.QueryAsync("INSERT INTO " + TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
														"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM " + TablePrefix(bType) + TablePostfix(true) +
															" WHERE id={ARG};", new List<string>([sID]), (_) =>
								{
									db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE id={ARG};", new List<string>([sID]), null, true);
								}, true);
							}, true);
						else db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE id={ARG};", new List<string>([sID]), null, true);
					}
					#pragma warning restore CS8625
				}
				return true;
			}
			return false;
		}

		private static string CreateTableSQL_SQLite(string sTableName)
		{
			return "CREATE TABLE IF NOT EXISTS "+ sTableName + "(	id INTEGER PRIMARY KEY AUTOINCREMENT, " +
																								"client_name varchar(32) NOT NULL, " +
																								"client_steamid varchar(64) NOT NULL, " +
																								"admin_name varchar(32) NOT NULL, " +
																								"admin_steamid varchar(64) NOT NULL, " +
																								"server varchar(64), " +
																								"duration INTEGER NOT NULL, " +
																								"timestamp_issued INTEGER NOT NULL, " +
																								"reason varchar(64), " +
																								"reason_unban varchar(64), " +
																								"admin_name_unban varchar(32), " +
																								"admin_steamid_unban varchar(64), " +
																								"timestamp_unban INTEGER);";
		}

		private static string CreateTableSQL_MySQL(string sTableName)
		{
			return "CREATE TABLE IF NOT EXISTS " + sTableName + "(	id int(10) unsigned NOT NULL auto_increment, " +
																								"client_name varchar(32) NOT NULL, " +
																								"client_steamid varchar(64) NOT NULL, " +
																								"admin_name varchar(32) NOT NULL, " +
																								"admin_steamid varchar(64) NOT NULL, " +
																								"server varchar(64), " +
																								"duration int unsigned NOT NULL, " +
																								"timestamp_issued int NOT NULL, " +
																								"reason varchar(64), " +
																								"reason_unban varchar(64), " +
																								"admin_name_unban varchar(32), " +
																								"admin_steamid_unban varchar(64), " +
																								"timestamp_unban int, " +
																								"PRIMARY KEY(id));";
		}

		private static string CreateTableSQL_PostgreSQL(string sTableName)
		{
			return "CREATE TABLE IF NOT EXISTS " + sTableName + "(	id serial, " +
																								"client_name varchar(32) NOT NULL, " +
																								"client_steamid varchar(64) NOT NULL, " +
																								"admin_name varchar(32) NOT NULL, " +
																								"admin_steamid varchar(64) NOT NULL, " +
																								"server varchar(64), " +
																								"duration integer NOT NULL, " +
																								"timestamp_issued integer NOT NULL, " +
																								"reason varchar(64), " +
																								"reason_unban varchar(64), " +
																								"admin_name_unban varchar(32), " +
																								"admin_steamid_unban varchar(64), " +
																								"timestamp_unban integer, " +
																								"PRIMARY KEY(id));";
		}

		private static string TablePrefix(bool bType)
		{
			return bType ? "ButtonWatch" : "TriggerWatch";
		}

		private static string TablePostfix(bool bCurrent)
		{
			return bCurrent ? "_Current_ban" : "_Old_ban";
		}
	}
}
