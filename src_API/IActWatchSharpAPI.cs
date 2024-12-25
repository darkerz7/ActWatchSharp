using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;

//VersionAPI: 0.DZ.3

namespace ActWatchSharpAPI
{
	public struct SAWAPI_Ban
	{
		public bool bBanned;                //True if user is banned, false otherwise

		public string sAdminName;           //Nickname admin who issued the ban
		public string sAdminSteamID;        //SteamID admin who issued the ban
		public int iDuration;               //Duration of the ban -1 - Temporary, 0 - Permamently, Positive value - time in minutes
		public int iTimeStamp_Issued;       //Pass an integer variable by reference and it will contain the UNIX timestamp when the player will be unbanned/ when a player was banned if ban = Permamently/Temporary
		public string sReason;              //The reason why the player was banned

		public string sClientName;          //Nickname of the player who got banned
		public string sClientSteamID;       //SteamID of the player who got banned

		public SAWAPI_Ban()
		{
			bBanned = false;
			sAdminName = "Console";
			sAdminSteamID = "SERVER";
			iDuration = 0;
			iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			sReason = "No Reason";
			sClientName = "";
			sClientSteamID = "";
		}
	}
	public interface IActWatchSharpAPI
	{
		public static PluginCapability<IActWatchSharpAPI> Capability { get; } = new("actwatch:api");

		/**
			* Checks if a player is currently button banned, if an integer variable is referenced the time of unban will be assigned to it.
			*
			* @param sSteamID		SteamID of the player to check for ban
			* @return				SAWAPI_Ban struct
			*
			*/
		SAWAPI_Ban Native_ButtonWatch_IsClientBanned(string sSteamID);

		/**
			* Bans a player from button pressed.
			*
			* @param sewPlayer		SAWAPI_Ban struct to ban
			* @return				True on success, false otherwsie
			*
			* On error/errors:		Invalid player
			*/
		bool Native_ButtonWatch_BanClient(SAWAPI_Ban sawPlayer);

		/**
			* Unbans a previously button banned player.
			*
			* @param sewPlayer		SAWAPI_Ban struct to unban
			* @return				True on success, false otherwsie
			*
			* On error/errors:		Invalid player
			*/
		bool Native_ButtonWatch_UnbanClient(SAWAPI_Ban sawPlayer);

		/**
			* Forces a button ban status update.
			*
			* @param Player			CCSPlayerController for forced update
			*
			* On error/errors:		Invalid player
			*/
		void Native_ButtonWatch_UpdateStatusBanClient(CCSPlayerController Player);

		/**
			* Checks if a player is currently trigger banned, if an integer variable is referenced the time of unban will be assigned to it.
			*
			* @param sSteamID		SteamID of the player to check for ban
			* @return				SAWAPI_Ban struct
			*
			*/
		SAWAPI_Ban Native_TriggerWatch_IsClientBanned(string sSteamID);

		/**
			* Bans a player from trigger touching.
			*
			* @param sewPlayer		SAWAPI_Ban struct to ban
			* @return				True on success, false otherwsie
			*
			* On error/errors:		Invalid player
			*/
		bool Native_TriggerWatch_BanClient(SAWAPI_Ban sewPlayer);

		/**
			* Unbans a previously trigger banned player.
			*
			* @param sewPlayer		SAWAPI_Ban struct to unban
			* @return				True on success, false otherwsie
			*
			* On error/errors:		Invalid player
			*/
		bool Native_TriggerWatch_UnbanClient(SAWAPI_Ban sawPlayer);

		/**
			* Forces a trigger ban status update.
			*
			* @param Player			CCSPlayerController for forced update
			*
			* On error/errors:		Invalid player
			*/
		void Native_TriggerWatch_UpdateStatusBanClient(CCSPlayerController Player);

		/**
			* Called when a player is button-banned by any means
			*
			* @param sewPlayer		Full information about ban in SEWAPI_Ban struct
			*
			* @return				None
			*/
		public delegate void Forward_BW_OnClientBanned(SAWAPI_Ban sawPlayer);
		public event Forward_BW_OnClientBanned Forward_ButtonWatch_OnClientBanned;

		/**
			* Called when a player is button-unbanned by any means
			*
			* @param sewPlayer		Full information about unban in SEWAPI_Ban struct
			* @return				None
			*/
		public delegate void Forward_BW_OnClientUnbanned(SAWAPI_Ban sewPlayer);
		public event Forward_BW_OnClientUnbanned Forward_ButtonWatch_OnClientUnbanned;

		/**
			* Сalled when a player presses the button
			*
			* @param Player			CCSPlayerController that was used item
			* @param sButtonName	Name of the button that was pressed
			* @param uiButtonID		ID of the button that was pressed
			* @return				None
			*/
		public delegate void Forward_BW_OnButtonPressed(CCSPlayerController Player, string sButtonName, uint uiButtonID);
		public event Forward_BW_OnButtonPressed Forward_ButtonWatch_OnButtonPressed;

		/**
			* Called when a player is trigger-banned by any means
			*
			* @param sewPlayer		Full information about ban in SEWAPI_Ban struct
			*
			* @return				None
			*/
		public delegate void Forward_TW_OnClientBanned(SAWAPI_Ban sawPlayer);
		public event Forward_TW_OnClientBanned Forward_TriggerWatch_OnClientBanned;

		/**
			* Called when a player is trigger-unbanned by any means
			*
			* @param sewPlayer		Full information about unban in SEWAPI_Ban struct
			* @return				None
			*/
		public delegate void Forward_TW_OnClientUnbanned(SAWAPI_Ban sewPlayer);
		public event Forward_TW_OnClientUnbanned Forward_TriggerWatch_OnClientUnbanned;

		/**
			* Сalled when a player touching the trigger
			*
			* @param Player			CCSPlayerController that was used item
			* @param sTriggerName	Name of the trigger that was tounching
			* @param uiTriggerID	ID of the trigger that was tounching
			* @return				None
			*/
		public delegate void Forward_TW_OnTriggerTouch(CCSPlayerController Player, string sTriggerName, uint uiTriggerID);
		public event Forward_TW_OnTriggerTouch Forward_TriggerWatch_OnTriggerTouch;
	}
}
