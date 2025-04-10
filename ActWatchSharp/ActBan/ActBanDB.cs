﻿using CounterStrikeSharp.API.Core;
using System.Text.Json;
using ActWatchSharp.Helpers;

namespace ActWatchSharp.ActBan
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
				dbConfig ??= new DBConfig();
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
				Task.Run(() => {
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
				});
			}
			else
			{
				UI.TranslatedPrintToConsole("Info.DB.Failed", 15, dbConfig.TypeDB);
				LogManager.SystemAction("Info.DB.Failed", dbConfig.TypeDB);
			}
		}

		public static void BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("INSERT INTO " + TablePrefix(bType) + TablePostfix(true) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES ('{ARG}', '{ARG}', '{ARG}', '{ARG}', '{ARG}', {ARG}, {ARG}, '{ARG}');", new List<string>([sClientName, sClientSteamID, sAdminName, sAdminSteamID, sServer, iDuration.ToString(), iTimeStamp.ToString(), sReason]), (_) => { }, true);
				});
			}
		}

		public static void UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				Task.Run(() =>
				{
					if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
						db.AnyDB.QueryAsync("UPDATE " + TablePrefix(bType) + TablePostfix(true) + " SET reason_unban = '{ARG}', admin_name_unban = '{ARG}', admin_steamid_unban = '{ARG}', timestamp_unban = {ARG} " +
												"WHERE client_steamid='{ARG}' and server='{ARG}' and admin_steamid_unban IS NULL;" +
											"INSERT INTO " + TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
												"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM " + TablePrefix(bType) + TablePostfix(true) +
													" WHERE client_steamid='{ARG}' and server='{ARG}';" +
											"DELETE FROM " + TablePrefix(bType) + TablePostfix(true) +
													" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sReason, sAdminName, sAdminSteamID, iTimeStamp.ToString(), sClientSteamID, sServer, sClientSteamID, sServer, sClientSteamID, sServer]), (_) => { }, true);
					else
						db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) +
							" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (_) => { }, true);
				});
			}
		}

#nullable enable
		public delegate void GetBanCommFunc(string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, List<List<string>> DBQuery_Result, bool bType);
		public static void GetBan(string sClientSteamID, string sServer, CCSPlayerController? admin, string reason, bool bConsole, GetBanCommFunc getbanfunc, bool bType)
#nullable disable
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM " + TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (res) =>
											{
												getbanfunc(sClientSteamID, admin, reason, bConsole, res, bType);
											});
				});
			}
		}
#nullable enable
		public delegate void GetBanAPIFunc(string sClientSteamID, List<List<string>> DBQuery_Result, bool bType);
		public static void GetBan(string sClientSteamID, string sServer, GetBanAPIFunc getbanfunc, bool bType)
#nullable disable
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM " + TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (res) =>
											{
												getbanfunc(sClientSteamID, res, bType);
											});
				});
			}
		}
		public delegate void GetBanPlayerFunc(CCSPlayerController player, List<List<string>> DBQuery_Result, bool bType, bool bShow);
		public static void GetBan(CCSPlayerController pl, string sServer, GetBanPlayerFunc getbanfunc, bool bType, bool bShow)
		{
			if (pl.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM " + TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([AW.ConvertSteamID64ToSteamID(pl.SteamID.ToString()), sServer]), (res) =>
											{
												getbanfunc(pl, res, bType, bShow);
											});
				});
			}
		}

		public static void OfflineUnban(string sServer, bool bType)
		{
			if (db.bDBReady)
			{
				int iTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Task.Run(() =>
				{
					if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
					{
						db.AnyDB.QueryAsync("SELECT id FROM " + TablePrefix(bType) + TablePostfix(true) +
									" WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG};", new List<string>([sServer, iTime.ToString()]), (res) =>
									{
										if (res.Count > 0)
										{
											string sIDs = "";
											for (int i = 0; i < res.Count; i++)
											{
												sIDs = sIDs + ", " + res[i][0];
											}
											sIDs = sIDs[2..];
											db.AnyDB.QueryAsync("UPDATE " + TablePrefix(bType) + TablePostfix(true) + " SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban={ARG} WHERE id IN ({ARG});" +
												"INSERT INTO " + TablePrefix(bType) + TablePostfix(false) + "(client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
													"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE id IN ({ARG});" +
												"DELETE FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE id IN ({ARG});", new List<string>([iTime.ToString(), sIDs, sIDs, sIDs]), (_) => { }, true);
										}
									});
					}
					else
					{
						db.AnyDB.QueryAsync("DELETE FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE id IN (SELECT p.id FROM (" +
								"SELECT id FROM " + TablePrefix(bType) + TablePostfix(true) + " WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG}) AS p);", new List<string>([sServer, iTime.ToString()]), (_) => { }, true);
					}
				});
			}
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
