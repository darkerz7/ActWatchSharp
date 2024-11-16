using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API;
using Serilog;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace ActWatchSharp.Helpers
{
	class LogCfg
	{
		public string Type { get; set; }        //File or Discord
		public string Send { get; set; }    //FileName or WebHook
		public string Lang { get; set; }       //Language to translate
		public bool ActInfo { get; set; }
		public bool AdminInfo { get; set; }
		public bool SystemInfo { get; set; }
		public bool CvarInfo { get; set; }

		public Serilog.Core.Logger LWritter;

		public LogCfg()
		{
			Type = "File";
			Send = "ActWatch";
			Lang = "en";
			ActInfo = true;
			AdminInfo = true;
			SystemInfo = true;
			CvarInfo = true;
			LWritter = null;
		}
	}
	public static class LogManager
	{
		static List<LogCfg> LM_CFG = new List<LogCfg>();
		static readonly Regex _invalidRegex = new Regex(@"[^\w\(\s!@\#\$%\^&\*\(\)_\+=\-'\\:\|/`~\.,\{}\)]+");
		static readonly HttpClient _httpClient = new HttpClient();
		static string ReplaceInvalid(string str) => _invalidRegex.Replace(str, "");

		public static void LoadConfig(string ModuleDirectory)
		{
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.LWritter != null) cfg.LWritter.Dispose();
				cfg.LWritter = null;
			}
			LM_CFG.Clear();
			string sConfig = $"{Path.Join(ModuleDirectory, "log_config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				sData = File.ReadAllText(sConfig);
				List<LogCfg> CFGBuffer = JsonSerializer.Deserialize<List<LogCfg>>(sData);
				if (CFGBuffer == null) return;
				foreach (LogCfg cfg in CFGBuffer.ToList())
				{
					ValidateCFG(cfg, ModuleDirectory);
				}
			}
		}

		public static void UnInit()
		{
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.LWritter != null) cfg.LWritter.Dispose();
				cfg.LWritter = null;
			}
		}

		static void ValidateCFG(LogCfg CfgTest, string ModuleDirectory)
		{
			if (!(CfgTest.ActInfo || CfgTest.AdminInfo || CfgTest.SystemInfo || CfgTest.CvarInfo)) return;

			if (string.IsNullOrEmpty(CfgTest.Type) || string.IsNullOrEmpty(CfgTest.Send) || string.IsNullOrEmpty(CfgTest.Lang)) return;

			byte iSend = 0;
			if (CfgTest.Type.ToLower().CompareTo("file") == 0) { iSend = 1; }
			if (CfgTest.Type.ToLower().CompareTo("discord") == 0) { iSend = 2; }

			if (iSend == 1) CfgTest.Send = ReplaceInvalid(CfgTest.Send);
			else if (iSend == 2) { if (!CfgTest.Send.StartsWith("https://discord.com/api/webhooks/")) return; }
			else return;

			if (string.IsNullOrEmpty(CfgTest.Send)) return;

			bool bNotFound = true;

			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (CfgTest.Type.ToLower().CompareTo(cfg.Type.ToLower()) == 0 && CfgTest.Send.CompareTo(cfg.Send) == 0)
				{
					if (iSend == 2 && CultureInfo.GetCultureInfo(CfgTest.Lang) != CultureInfo.GetCultureInfo(cfg.Lang)) continue;
					if (CfgTest.ActInfo) cfg.ActInfo = true;
					if (CfgTest.AdminInfo) cfg.AdminInfo = true;
					if (CfgTest.SystemInfo) cfg.SystemInfo = true;
					if (CfgTest.CvarInfo) cfg.CvarInfo = true;
					bNotFound = false;
					break;
				}
			}
			if (bNotFound)
			{
				LM_CFG.Add(CfgTest);
				if (iSend == 1)
				{
					CfgTest.LWritter = new LoggerConfiguration()
						.WriteTo.File($"{ModuleDirectory}/../../logs/ActWatch/{CfgTest.Send}-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff(zzz)} [{Level:w3}] {Message:l}{NewLine}{Exception}")
						.CreateLogger();
				}
			}
		}

		static async Task SendToDistord(string sWebHook, string sMessage)
		{
			try
			{
				var body = JsonSerializer.Serialize(new { content = $"*{Server.MapName} - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}* ```{sMessage}```" });
				var content = new StringContent(body, Encoding.UTF8, "application/json");
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage res = (await _httpClient.PostAsync($"{sWebHook}", content)).EnsureSuccessStatusCode();
			}
			catch (Exception) { }
		}

		public static void ActAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.ActInfo)
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(cfg.Lang)))
					{
						string sMsg = ActWatchSharp.Strlocalizer[sMessage, arg];
						if (cfg.Type.ToLower().CompareTo("file") == 0 && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
						else if (cfg.Type.ToLower().CompareTo("discord") == 0)
						{
							Server.NextFrame(async () => { await SendToDistord(cfg.Send, sMsg); });
						}
					}
				}
			}
		}

		public static void AdminAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.AdminInfo)
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(cfg.Lang)))
					{
						string sMsg = UI.ReplaceColorTags(ActWatchSharp.Strlocalizer[sMessage, arg], false);
						if (cfg.Type.ToLower().CompareTo("file") == 0 && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
						else if (cfg.Type.ToLower().CompareTo("discord") == 0)
						{
							Server.NextFrame(async () => { await SendToDistord(cfg.Send, sMsg); });
						}
					}
				}
			}
		}

		public static void SystemAction(string sMessage, params object[] arg)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.SystemInfo)
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(cfg.Lang)))
					{
						string sMsg = ActWatchSharp.Strlocalizer[sMessage, arg];
						if (cfg.Type.ToLower().CompareTo("file") == 0 && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
						else if (cfg.Type.ToLower().CompareTo("discord") == 0)
						{
							Server.NextFrame(async () => { await SendToDistord(cfg.Send, sMsg); });
						}
					}
				}
			}
		}

		public static void CvarAction(string sCvarName, string sCvarValue)
		{
			if (ActWatchSharp.Strlocalizer == null) return;
			foreach (LogCfg cfg in LM_CFG.ToList())
			{
				if (cfg.CvarInfo)
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(cfg.Lang)))
					{
						string sMsg = ActWatchSharp.Strlocalizer["Cvar.Notify", sCvarName, sCvarValue];
						if (cfg.Type.ToLower().CompareTo("file") == 0 && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
						else if (cfg.Type.ToLower().CompareTo("discord") == 0)
						{
							Server.NextFrame(async () => { await SendToDistord(cfg.Send, sMsg); });
						}
					}
				}
			}
		}
	}
}
