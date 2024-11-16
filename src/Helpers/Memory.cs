using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace ActWatchSharp
{
	public partial class ActWatchSharp : BasePlugin
	{
		public void RegVirtualFunctions()
		{
			VirtualFunctions.CBaseTrigger_StartTouchFunc.Hook(OnTriggerStartTouch, HookMode.Pre);
		}

		public void UnRegVirtualFunctions()
		{
			VirtualFunctions.CBaseTrigger_StartTouchFunc.Unhook(OnTriggerStartTouch, HookMode.Pre);
		}
	}
}
