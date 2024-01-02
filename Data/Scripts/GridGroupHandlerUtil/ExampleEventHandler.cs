using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace GridGroupHandlerUtil
{
    internal class ExampleEventHandler : BlockCentricGridGroupNotifyingEventHandlerBase
    {
        public readonly IMyDecoy decoyBlock;
        private readonly ExampleEntityComponent component;
        private static readonly Guid makeYourOwnGuid = new Guid("0e72d3d4-35cc-45d4-b6ba-691877bdc829");
        protected override Guid GetDataKey() => makeYourOwnGuid;

        public ExampleEventHandler(IMyDecoy decoy, ExampleEntityComponent entityComponent) : base(decoy, GridLinkTypeEnum.Physical)
        {
            decoyBlock = decoy;
            component = entityComponent;
        }

        protected override void OnGridAddedToGroup(IMyCubeGrid grid, IMyGridGroupData oldGroup)
        {
            base.OnGridAddedToGroup(grid, oldGroup);
            MyLog.Default.WriteLineAndConsole($"ExampleEventHandler ({decoyBlock?.DisplayNameText}): Grid {grid?.DisplayName} added to grid group");
        }
        protected override void OnGridGroupChanged(IMyGridGroupData oldGroup)
        {
            base.OnGridGroupChanged(oldGroup);
            MyLog.Default.WriteLineAndConsole($"ExampleEventHandler ({decoyBlock?.DisplayNameText}): changed to a new grid group");
            component.OnChangedToNewGridGroup();
        }
        protected override void OnGridRemovedFromGroup(IMyCubeGrid grid, IMyGridGroupData newGroup)
        {
            base.OnGridRemovedFromGroup(grid, newGroup);
            MyLog.Default.WriteLineAndConsole($"ExampleEventHandler ({decoyBlock?.DisplayNameText}): Grid {grid?.DisplayName} removed from this grid group");
        }

        public override void OnBlockEnteredGroup(IMyCubeBlock otherBlock)
        {
            MyLog.Default.WriteLineAndConsole($"ExampleEventHandler ({decoyBlock?.DisplayNameText}): block {otherBlock?.DisplayNameText} has entered the grid group");
        }

        public override void OnBlockLeftGroup(IMyCubeBlock otherBlock)
        {
            MyLog.Default.WriteLineAndConsole($"ExampleEventHandler ({decoyBlock?.DisplayNameText}): block {otherBlock?.DisplayNameText} has left the grid group");
        }
    }
}
