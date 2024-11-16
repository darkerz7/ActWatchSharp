using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;

namespace ActWatchSharp.Helpers
{
	public static class FindTarget
	{
		public enum MultipleFlags
		{
			NORMAL = 0,
			IGNORE_DEAD_PLAYERS,
			IGNORE_ALIVE_PLAYERS
		}

		public static (List<CCSPlayerController> players, string targetname) Find
			(
				CCSPlayerController player,
				CommandInfo command,
				int numArg,
				bool singletarget,
				bool immunitycheck,
				MultipleFlags flags,
				bool shownomatching = true
			)
		{
			if (command.ArgCount < numArg)
			{
				return ([], string.Empty);
			}

			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			TargetResult targetresult = command.GetArgTargetResult(numArg);
			if (targetresult.Players.Count == 0)
			{
				if (shownomatching) UI.ReplyToCommand(player, bConsole, "Reply.No_matching_client");

				return ([], string.Empty);
			}

			else if (singletarget && targetresult.Players.Count > 1)
			{
				UI.ReplyToCommand(player, bConsole, "Reply.More_than_one_client_matched");

				return ([], string.Empty);
			}

			if (immunitycheck)
			{
				targetresult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

				if (targetresult.Players.Count == 0)
				{
					UI.ReplyToCommand(player, bConsole, "Reply.You_cannot_target");

					return ([], string.Empty);
				}
			}

			if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
			{
				targetresult.Players.RemoveAll(target => !target.PawnIsAlive);

				if (targetresult.Players.Count == 0)
				{
					UI.ReplyToCommand(player, bConsole, "Reply.You_can_target_only_alive_players");

					return ([], string.Empty);
				}
			}
			else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
			{
				targetresult.Players.RemoveAll(target => target.PawnIsAlive);

				if (targetresult.Players.Count == 0)
				{
					UI.ReplyToCommand(player, bConsole, "Reply.You_can_target_only_dead_players");

					return ([], string.Empty);
				}
			}

			string targetname;

			if (targetresult.Players.Count == 1)
			{
				targetname = targetresult.Players.Single().PlayerName;
			}
			else
			{
				Target.TargetTypeMap.TryGetValue(command.GetArg(1), out TargetType type);

				targetname = type switch
				{
					TargetType.GroupAll => ActWatchSharp.Strlocalizer["all"],
					TargetType.GroupBots => ActWatchSharp.Strlocalizer["bots"],
					TargetType.GroupHumans => ActWatchSharp.Strlocalizer["humans"],
					TargetType.GroupAlive => ActWatchSharp.Strlocalizer["alive"],
					TargetType.GroupDead => ActWatchSharp.Strlocalizer["dead"],
					TargetType.GroupNotMe => ActWatchSharp.Strlocalizer["notme"],
					TargetType.PlayerMe => targetresult.Players.First().PlayerName,
					TargetType.TeamCt => ActWatchSharp.Strlocalizer["ct"],
					TargetType.TeamT => ActWatchSharp.Strlocalizer["t"],
					TargetType.TeamSpec => ActWatchSharp.Strlocalizer["spec"],
					_ => targetresult.Players.First().PlayerName
				};
			}

			return (targetresult.Players, targetname);
		}
	}
}
