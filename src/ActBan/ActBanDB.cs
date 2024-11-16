using CounterStrikeSharp.API.Core;
using MySqlConnector;
using System.Data.SQLite;
using System.Data;
using System.Text.Json;
using ActWatchSharp.Helpers;

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
			if (dbConfig.TypeDB == "mysql") db = new DB_Mysql($"server={dbConfig.Mysql_Server};port={dbConfig.Mysql_Port};user={dbConfig.Mysql_User};database={dbConfig.Mysql_NameDatabase};password={dbConfig.Mysql_Password};");
			else
			{
				string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
				if (!File.Exists(sDBFile)) File.WriteAllBytes(sDBFile, Array.Empty<byte>());
				db = new DB_SQLite($"Data Source={sDBFile}");
			}
			Task.Run(async () =>
			{
				string sExceptionMessage = await db.TestConnection();
				if (db.bSuccess)
				{
					UI.TranslatedPrintToConsole("Info.DB.Success", 6, db.TypeDB);
					LogManager.SystemAction("Info.DB.Success", db.TypeDB);

					if (db.TypeDB == "sqlite")
					{
						using (SQLiteCommand cmd = new SQLiteCommand())
						{
							cmd.CommandText = CreateTableSQL_SQLite(TablePrefix(true) + TablePostfix(true)) + CreateTableSQL_SQLite(TablePrefix(true) + TablePostfix(false)) + CreateTableSQL_SQLite(TablePrefix(false) + TablePostfix(true)) + CreateTableSQL_SQLite(TablePrefix(false) + TablePostfix(false));
							if (await ((DB_SQLite)db).Execute(cmd) > -1) db.bDBReady = true;
						}
					}
					else if (db.TypeDB == "mysql")
					{
						using (MySqlCommand cmd = new MySqlCommand())
						{
							cmd.CommandText = CreateTableSQL_MySQL(TablePrefix(true) + TablePostfix(true)) + CreateTableSQL_MySQL(TablePrefix(true) + TablePostfix(false)) + CreateTableSQL_MySQL(TablePrefix(false) + TablePostfix(true)) + CreateTableSQL_MySQL(TablePrefix(false) + TablePostfix(false));
							if (await ((DB_Mysql)db).Execute(cmd) > -1) db.bDBReady = true;
						}
					}
				}
				else
				{
					UI.TranslatedPrintToConsole("Info.DB.Failed", 15, db.TypeDB, sExceptionMessage);
					LogManager.SystemAction("Info.DB.Failed", db.TypeDB, sExceptionMessage);
				}
			});
		}

		public static async Task<bool> BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				if (db.TypeDB == "sqlite")
				{
					using (SQLiteCommand cmd = new SQLiteCommand())
					{
						cmd.CommandText = "INSERT INTO "+ TablePrefix(bType) + TablePostfix(true) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES (@clientname, @clientsteamid, @adminname, @adminsteamid, @server, @duration, @timeissued, @reason);";
						cmd.Parameters.Add(new SQLiteParameter("@clientname", sClientName));
						cmd.Parameters.Add(new SQLiteParameter("@clientsteamid", sClientSteamID));
						cmd.Parameters.Add(new SQLiteParameter("@adminname", sAdminName));
						cmd.Parameters.Add(new SQLiteParameter("@adminsteamid", sAdminSteamID));
						cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
						cmd.Parameters.Add(new SQLiteParameter("@duration", iDuration));
						cmd.Parameters.Add(new SQLiteParameter("@timeissued", iTimeStamp));
						cmd.Parameters.Add(new SQLiteParameter("@reason", sReason));
						await ((DB_SQLite)db).Execute(cmd);
					}
				}
				else if (db.TypeDB == "mysql")
				{
					using (MySqlCommand cmd = new MySqlCommand())
					{
						cmd.CommandText = "INSERT INTO "+ TablePrefix(bType) + TablePostfix(true) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES (@clientname, @clientsteamid, @adminname, @adminsteamid, @server, @duration, @timeissued, @reason);";
						cmd.Parameters.Add(new MySqlParameter("@clientname", sClientName));
						cmd.Parameters.Add(new MySqlParameter("@clientsteamid", sClientSteamID));
						cmd.Parameters.Add(new MySqlParameter("@adminname", sAdminName));
						cmd.Parameters.Add(new MySqlParameter("@adminsteamid", sAdminSteamID));
						cmd.Parameters.Add(new MySqlParameter("@server", sServer));
						cmd.Parameters.Add(new MySqlParameter("@duration", iDuration));
						cmd.Parameters.Add(new MySqlParameter("@timeissued", iTimeStamp));
						cmd.Parameters.Add(new MySqlParameter("@reason", sReason));
						await ((DB_Mysql)db).Execute(cmd);
					}
				}
				return true;
			}
			return false;
		}

		public static async Task<bool> UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason, bool bType)
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				if (db.TypeDB == "sqlite")
				{
					SQLiteCommand cmd = new SQLiteCommand();
					if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
						cmd.CommandText = "UPDATE "+ TablePrefix(bType) + TablePostfix(true) + " SET reason_unban=@reason, admin_name_unban=@adminname, admin_steamid_unban=@adminsteamid, timestamp_unban=@timeunban " +
											"WHERE client_steamid=@clientsteamid and server=@server and admin_steamid_unban IS NULL; " +

										"INSERT INTO "+ TablePrefix(bType) + TablePostfix(true) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
											"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM "+ TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid=@clientsteamid and server=@server; ";

					cmd.CommandText += "DELETE FROM "+ TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid=@clientsteamid and server=@server;";
					cmd.Parameters.Add(new SQLiteParameter("@clientsteamid", sClientSteamID));
					cmd.Parameters.Add(new SQLiteParameter("@adminname", sAdminName));
					cmd.Parameters.Add(new SQLiteParameter("@adminsteamid", sAdminSteamID));
					cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
					cmd.Parameters.Add(new SQLiteParameter("@timeunban", iTimeStamp));
					cmd.Parameters.Add(new SQLiteParameter("@reason", sReason));
					await ((DB_SQLite)db).Execute(cmd);
				}
				else if (db.TypeDB == "mysql")
				{
					using (MySqlCommand cmd = new MySqlCommand())
					{
						if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
							cmd.CommandText = "UPDATE "+ TablePrefix(bType) + TablePostfix(true) + " SET reason_unban=@reason, admin_name_unban=@adminname, admin_steamid_unban=@adminsteamid, timestamp_unban=@timeunban " +
												"WHERE client_steamid=@clientsteamid and server=@server and admin_steamid_unban IS NULL; " +

											"INSERT INTO "+ TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
												"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM "+ TablePrefix(bType) + TablePostfix(true) +
													" WHERE client_steamid=@clientsteamid and server=@server; ";

						cmd.CommandText += "DELETE FROM "+ TablePrefix(bType) + TablePostfix(true) +
											" WHERE client_steamid=@clientsteamid and server=@server;";
						cmd.Parameters.Add(new MySqlParameter("@clientsteamid", sClientSteamID));
						cmd.Parameters.Add(new MySqlParameter("@adminname", sAdminName));
						cmd.Parameters.Add(new MySqlParameter("@adminsteamid", sAdminSteamID));
						cmd.Parameters.Add(new MySqlParameter("@server", sServer));
						cmd.Parameters.Add(new MySqlParameter("@timeunban", iTimeStamp));
						cmd.Parameters.Add(new MySqlParameter("@reason", sReason));
						await ((DB_Mysql)db).Execute(cmd);
					}
				}
				return true;
			}
			return false;
		}

		public static async Task<bool> GetBan(CCSPlayerController player, string sServer, bool bType)
		{
			try
			{
				if (player.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					if (db.TypeDB == "sqlite")
					{
						using (SQLiteCommand cmd = new SQLiteCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM "+ TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new SQLiteParameter("@steam", AW.ConvertSteamID64ToSteamID(player.SteamID.ToString())));
							cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									Dictionary<CCSPlayerController, ActBanPlayer> dActBan = bType ? AW.g_ButtonBannedPlayer : AW.g_TriggerBannedPlayer;
									if (dActBan.ContainsKey(player))
									{
										await reader.ReadAsync();
										dActBan[player].bBanned = true;
										dActBan[player].sAdminName = await reader.GetFieldValueAsync<string>(0);
										dActBan[player].sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
										dActBan[player].iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<long>(2));
										dActBan[player].iTimeStamp_Issued = Convert.ToInt32(await reader.GetFieldValueAsync<long>(3));
										dActBan[player].sReason = await reader.GetFieldValueAsync<string>(4);

										return true;
									}
									else
									{
										dActBan.TryAdd(player, new ActBanPlayer(bType));
										return await GetBan(player, sServer, bType);
									}
								}
							}
						}
					}
					else if (db.TypeDB == "mysql")
					{
						using (MySqlCommand cmd = new MySqlCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM "+ TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new MySqlParameter("@steam", AW.ConvertSteamID64ToSteamID(player.SteamID.ToString())));
							cmd.Parameters.Add(new MySqlParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									Dictionary<CCSPlayerController, ActBanPlayer> dActBan = bType ? AW.g_ButtonBannedPlayer : AW.g_TriggerBannedPlayer;
									if (dActBan.ContainsKey(player))
									{
										await reader.ReadAsync();
										dActBan[player].bBanned = true;
										dActBan[player].sAdminName = await reader.GetFieldValueAsync<string>(0);
										dActBan[player].sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
										dActBan[player].iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
										dActBan[player].iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
										dActBan[player].sReason = await reader.GetFieldValueAsync<string>(4);
										return true;
									}
									else
									{
										dActBan.TryAdd(player, new ActBanPlayer(bType));
										return await GetBan(player, sServer, bType);
									}
								}
							}
						}
					}
					return false;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return false;
		}

		public static async Task<ActBanPlayer> GetBan(string SteamID, string sServer, bool bType)
		{
			try
			{
				if (!string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					if (db.TypeDB == "sqlite")
					{
						using (SQLiteCommand cmd = new SQLiteCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM "+ TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new SQLiteParameter("@steam", SteamID));
							cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									await reader.ReadAsync();
									ActBanPlayer player = new ActBanPlayer(bType);
									player.bBanned = true;
									player.sAdminName = await reader.GetFieldValueAsync<string>(0);
									player.sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
									player.iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<long>(2));
									player.iTimeStamp_Issued = Convert.ToInt32(await reader.GetFieldValueAsync<long>(3));
									player.sReason = await reader.GetFieldValueAsync<string>(4);
									player.sClientName = await reader.GetFieldValueAsync<string>(5);
									player.sClientSteamID = SteamID;
									return player;
								}
							}
						}
					}
					else if (db.TypeDB == "mysql")
					{
						using (MySqlCommand cmd = new MySqlCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM "+ TablePrefix(bType) + TablePostfix(true) +
												" WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new MySqlParameter("@steam", SteamID));
							cmd.Parameters.Add(new MySqlParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									await reader.ReadAsync();
									ActBanPlayer player = new ActBanPlayer(bType);
									player.bBanned = true;
									player.sAdminName = await reader.GetFieldValueAsync<string>(0);
									player.sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
									player.iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
									player.iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
									player.sReason = await reader.GetFieldValueAsync<string>(4);
									player.sClientName = await reader.GetFieldValueAsync<string>(5);
									player.sClientSteamID = SteamID;
									return player;
								}
							}
						}
					}
					return null;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return null;
		}

		public static async Task<bool> OfflineUnban(string sServer, int iTime, bool bType)
		{
			if (db.bDBReady)
			{
				if (db.TypeDB == "sqlite")
				{
					using (SQLiteCommand cmd = new SQLiteCommand())
					{
						cmd.CommandText = "SELECT id FROM "+ TablePrefix(bType) + TablePostfix(true) +
											" WHERE server=@server and duration>0 and timestamp_issued<@time;";
						cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
						cmd.Parameters.Add(new SQLiteParameter("@time", iTime));
						using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
						{
							if (reader != null && reader.HasRows)
							{
								while (await reader.ReadAsync())
								{
									long iID = await reader.GetFieldValueAsync<long>(0);

									SQLiteCommand cmd1 = new SQLiteCommand();
									if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
										cmd1.CommandText = "UPDATE "+ TablePrefix(bType) + TablePostfix(true) + " SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban=@time WHERE id=@id; " +

														"INSERT INTO "+ TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
															"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM "+ TablePrefix(bType) + TablePostfix(true) +
																" WHERE id=@id; ";

									cmd1.CommandText += "DELETE FROM "+ TablePrefix(bType) + TablePostfix(true) + " WHERE id=@id;";
									cmd1.Parameters.Add(new SQLiteParameter("@time", iTime));
									cmd1.Parameters.Add(new SQLiteParameter("@id", iID));
									await ((DB_SQLite)db).Execute(cmd1);
								}
							}
						}
					}
				}
				else if (db.TypeDB == "mysql")
				{
					using (MySqlCommand cmd = new MySqlCommand())
					{
						cmd.CommandText = "SELECT id FROM "+ TablePrefix(bType) + TablePostfix(true) +
											" WHERE server=@server and duration>0 and timestamp_issued<@time;";
						cmd.Parameters.Add(new MySqlParameter("@server", sServer));
						cmd.Parameters.Add(new MySqlParameter("@time", iTime));
						using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
						{
							if (reader != null && reader.HasRows)
							{
								while (await reader.ReadAsync())
								{
									int iID = await reader.GetFieldValueAsync<int>(0);

									MySqlCommand cmd1 = new MySqlCommand();
									if (bType && Cvar.ButtonKeepExpiredBan || !bType && Cvar.TriggerKeepExpiredBan)
										cmd1.CommandText = "UPDATE "+ TablePrefix(bType) + TablePostfix(true) + " SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban=@time WHERE id=@id; " +

															"INSERT INTO "+ TablePrefix(bType) + TablePostfix(false) + " (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
																"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM "+ TablePrefix(bType) + TablePostfix(true) +
																	" WHERE id=@id; ";

									cmd1.CommandText += "DELETE FROM "+ TablePrefix(bType) + TablePostfix(true) + " WHERE id=@id;";
									cmd1.Parameters.Add(new MySqlParameter("@time", iTime));
									cmd1.Parameters.Add(new MySqlParameter("@id", iID));
									await ((DB_Mysql)db).Execute(cmd1);
								}
							}
						}
					}
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
