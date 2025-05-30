# [Core]ActWatchSharp for CounterStrikeSharp
Notify players about button and trigger(Activator) interactions. Beta version of the plugin

## Features:
1. Async functions
2. SQLite/MySQL/PostgreSQL support
3. Language setting for players
4. Allows you to set up individual access for admins
5. Keeps logs to a file and discord
6. Online/Offline ban/unban of button press/trigger touch
7. API for interaction with other plugins
8. Allows you to select the player display format

## Required packages:
1. [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) (Min version: 285)
2. [AnyBaseLibCS2](https://github.com/NickFox007/AnyBaseLibCS2) (0.9.3)
3. [PlayerSettingsCS2](https://github.com/NickFox007/PlayerSettingsCS2) (0.9.3)
4. Of necessity [EntWatchSharp](https://github.com/darkerz7/EntWatchSharp/) (DefineConstants `USE_ENTWATCH`)
5. Recomended [CSSharp-Fixes](https://github.com/darkerz7/CSSharp-Fixes)

## Installation:
1. Install `AnyBaseLibCS2`, `PlayerSettingsCS2` and `CSSharp-Fixes`
2. Compile or copy ActWatchSharp to `counterstrikesharp/plugins/ActWatchSharp` folger. To compile without EntWatch remove DefineConstants `USE_ENTWATCH` (Project Properties -> Build -> Conditional compilation symbols)
3. Copy and configure the configuration file `db_config.json`, `plugin_config.json` and `log_config.json` to `counterstrikesharp/plugins/ActWatchSharp` folger
4. Copy `lang` folger to `counterstrikesharp/plugins/ActWatchSharp/lang` folger
5. Compile or copy ActWatchSharp to `counterstrikesharp/shared/ActWatchSharp` folger
6. Add CVARs to server.cfg
7. Restart server

## Admin privileges
Privilege | Description
--- | ---
`@css/aw_reload` | Allows you to reload the plugin config
`@css/bw_ban` | Allows access to button press bans (Command)
`@css/bw_ban_perm` | Allows access to permanent button press bans (Duration 0)
`@css/bw_ban_long` | Allows access to long button press bans (Cvar awc_bbanlong)
`@css/bw_unban` | Allows access to button press unbans (Command)
`@css/bw_unban_perm` | Allows access to permanent button press unbans (Duration 0)
`@css/bw_unban_other` | Allows access to button press unbans from other admins
`@css/tw_ban` | Allows access to trigger touch bans (Command)
`@css/tw_ban_perm` | Allows access to permanent trigger touch bans (Duration 0)
`@css/tw_ban_long` | Allows access to long trigger touch bans (Cvar awc_tbanlong)
`@css/tw_unban` | Allows access to trigger touch unbans (Command)
`@css/tw_unban_perm` | Allows access to permanent trigger touch unbans (Duration 0)
`@css/tw_unban_other` | Allows access to trigger touch unbans from other admins

## CVARs
Cvar | Parameters | Description
--- | --- | ---
`awc_bbantime` | `<0-43200>` | Default button press ban time. 0 - Permanent. (Default 0)
`awc_bbanlong` | `<1-1440000>` | Max button press ban time with once @css/bw_ban privilege. (Default 720)
`awc_bbanreason` | `<string>` | Default button press ban reason. (Default Trolling)
`awc_bunbanreason` | `<string>` | Default button press unban reason. (Default Giving another chance)
`awc_bkeep_expired_ban` | `<false-true>` | Enable/Disable keep expired button press bans. (Default true)
`awc_benable` | `<false-true>` | Enable/Disable button press functionality. (Default true)
`awc_bshow_button` | `<false-true>` | Enable/Disable display of func_(rot_)button presses. (Default true)
`awc_bshow_door` | `<false-true>` | Enable/Disable display of func_door(_rotating) presses. (Default true)
`awc_bshow_physbox` | `<false-true>` | Enable/Disable display of func_physbox presses. (Default true)
`awc_bwatch_button` | `<false-true>` | Enable/Disable watch of func_(rot_)button presses. Do bans affect. (Default true)
`awc_bwatch_door` | `<false-true>` | Enable/Disable watch of func_door(_rotating) presses. Do bans affect. (Default true)
`awc_bwatch_physbox` | `<false-true>` | Enable/Disable watch of func_physbox presses. Do bans affect. (Default true)
`awc_tbantime` | `<0-43200>` | Default trigger touch ban time. 0 - Permanent. (Default 0)
`awc_tbanlong` | `<1-1440000>` | Max trigger touch ban time with once @css/tw_ban privilege. (Default 720)
`awc_tbanreason` | `<string>` | Default trigger touch ban reason. (Default Trolling)
`awc_tunbanreason` | `<string>` | Default trigger touch unban reason. (Default Giving another chance)
`awc_tkeep_expired_ban` | `<false-true>` | Enable/Disable keep expired trigger touch bans. (Default true)
`awc_tenable` | `<false-true>` | Enable/Disable trigger touch functionality. (Default true)
`awc_tshow_once` | `<false-true>` | Enable/Disable display of trigger_once touching. (Default true)
`awc_tshow_multiple` | `<false-true>` | Enable/Disable display of trigger_multiple touching. (Default false)
`awc_twatch_once` | `<false-true>` | Enable/Disable watch of trigger_once touching. Do bans affect.(BUG) When touched by a banned trigger disappears. (Default false)
`awc_twatch_multiple` | `<false-true>` | Enable/Disable watch of trigger_multiple touching. Do bans affect. (Default false)
`awc_offline_clear_time` | `<1-240>` | Time during which data is stored. (Default 30)
`awc_player_format` | `<0-3>` | Changes the way player information is displayed by default (0 - Only Nickname, 1 - Nickname and UserID, 2 - Nickname and SteamID, 3 - Nickname, UserID and SteamID). (Default 3)

## Commands
Client Command | Description
--- | ---
`css_buttons` | Allows players to toggle the button press display
`css_triggers` | Allows players to toggle the trigger touch display
`css_apf` | Allows the player to change the player display format (0 - Only Nickname, 1 - Nickname and UserID, 2 - Nickname and SteamID, 3 - Nickname, UserID and SteamID)
`bw_status`<br>`css_bstatus` | Allows the player to view the button press ban {null/target}
`tw_status`<br>`css_trstatus` | Allows the player to view the trigger touch ban {null/target}

## Admin's commands
Admin Command | Privilege | Description
--- | --- | ---
`aw_reload`<br>`css_areload` | `@css/aw_reload` | Reloads plugin config
`bw_ban`<br>`css_bban` | `@css/bw_ban`+`@css/bw_ban_perm`+`@css/bw_ban_long` | Allows the admin to button press bans for the player `<#userid/name/#steamid> [<time>] [<reason>]`
`bw_unban`<br>`css_bunban` | `@css/bw_unban`+`@css/bw_unban_perm`+`@css/bw_unban_other` | Allows the admin to remove button press ban for a player `<#userid/name/#steamid> [<reason>]`
`bw_banlist`<br>`css_bbanlist` | `@css/bw_ban` | Displays a list of button press bans
`tw_ban`<br>`css_trban` | `@css/tw_ban`+`@css/tw_ban_perm`+`@css/tw_ban_long` | Allows the admin to trigger touch bans for the player `<#userid/name/#steamid> [<time>] [<reason>]`
`tw_unban`<br>`css_trunban` | `@css/tw_unban`+`@css/tw_unban_perm`+`@css/tw_unban_other` | Allows the admin to remove trigger touch ban for a player `<#userid/name/#steamid> [<reason>]`
`tw_banlist`<br>`css_trbanlist` | `@css/tw_ban` | Displays a list of trigger touch bans
