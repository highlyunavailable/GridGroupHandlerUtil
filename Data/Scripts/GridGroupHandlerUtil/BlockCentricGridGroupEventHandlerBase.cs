using VRage.Game.ModAPI;
    
namespace GridGroupHandlerUtil
{
    public abstract class BlockCentricGridGroupEventHandlerBase
    {
        protected readonly IMyCubeBlock block;
        protected readonly GridLinkTypeEnum groupType;

        protected IMyCubeGrid cubeGrid;
        protected IMyGridGroupData gridGroup;

        public IMyCubeBlock Block => block;
        public IMyCubeGrid Grid => cubeGrid;
        public IMyGridGroupData GridGroup => gridGroup;

        public BlockCentricGridGroupEventHandlerBase(IMyCubeBlock cubeBlock, GridLinkTypeEnum linkType)
        {
            block = cubeBlock;
            groupType = linkType;
            cubeGrid = block.CubeGrid;
            gridGroup = block.CubeGrid.GetGridGroup(linkType);
            AddBlockEvents(block);
            AddGridEvents(cubeGrid);
            AddGroupEvents(gridGroup);
        }

        public void Close()
        {
            RemoveBlockEvents(block);
            RemoveGridEvents(cubeGrid);
            RemoveGroupEvents(gridGroup);
        }

        protected virtual void AddBlockEvents(IMyCubeBlock block)
        {
            block.OnMarkForClose += Block_OnMarkForClose;
        }
        protected virtual void RemoveBlockEvents(IMyCubeBlock block)
        {
            block.OnMarkForClose -= Block_OnMarkForClose;
        }

        private void Block_OnMarkForClose(VRage.ModAPI.IMyEntity obj)
        {
            if (obj != block || block == null)
            {
                return;
            }
            RemoveBlockEvents(block);
            RemoveGridEvents(cubeGrid);
            RemoveGroupEvents(gridGroup);
        }

        protected virtual void AddGridEvents(IMyCubeGrid grid)
        {
            if (grid == null) { return; }
            RemoveGridEvents(grid);
            grid.OnGridSplit += Grid_OnGridSplit;
            grid.OnGridMerge += Grid_OnGridMerge;
        }

        protected virtual void RemoveGridEvents(IMyCubeGrid grid)
        {
            if (grid == null) { return; }
            grid.OnGridSplit -= Grid_OnGridSplit;
            grid.OnGridMerge -= Grid_OnGridMerge;
        }

        private void Grid_OnGridMerge(IMyCubeGrid kept, IMyCubeGrid lost)
        {
            if (block?.CubeGrid == cubeGrid || block?.CubeGrid == lost)
            {
                // Nothing to do, the block is already on the kept grid or is going to be lost anyway
                return;
            }

            // Remap to the new gridgroup data
            RemoveGridEvents(cubeGrid);
            RemoveGroupEvents(gridGroup);
            var oldGroup = gridGroup;

            cubeGrid = kept;
            gridGroup = kept.GetGridGroup(groupType);
            AddGridEvents(kept);
            AddGroupEvents(gridGroup);
            OnGridGroupChanged(oldGroup);
        }

        private void Grid_OnGridSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
        {
            if (block?.CubeGrid == cubeGrid)
            {
                // Nothing to do, the block did not change grids
                return;
            }

            // Remap to the new gridgroup data
            RemoveGridEvents(cubeGrid);
            RemoveGroupEvents(gridGroup);
            var oldGroup = gridGroup;

            cubeGrid = newGrid;
            gridGroup = newGrid.GetGridGroup(groupType);
            AddGridEvents(newGrid);
            AddGroupEvents(gridGroup);
            OnGridGroupChanged(oldGroup);
        }

        protected virtual void OnGridAddedToGroup(IMyCubeGrid grid, IMyGridGroupData oldGroup) { }

        private void GridGroup_OnGridAdded(IMyGridGroupData newGroup, IMyCubeGrid grid, IMyGridGroupData oldGroup)
        {
            if (grid == cubeGrid)
            {
                OnGridGroupChanged(oldGroup);
            }
            else
            {
                OnGridAddedToGroup(grid, oldGroup);
            }
        }

        protected virtual void OnGridRemovedFromGroup(IMyCubeGrid grid, IMyGridGroupData newGroup) { }
        protected virtual void OnGridGroupChanged(IMyGridGroupData oldGroup) { }

        private void GridGroup_OnGridRemoved(IMyGridGroupData oldGroup, IMyCubeGrid grid, IMyGridGroupData newGroup)
        {
            if (grid == cubeGrid)
            {
                RemoveGroupEvents(oldGroup);
                gridGroup = newGroup;
                AddGroupEvents(newGroup);
            }
            else
            {
                OnGridRemovedFromGroup(grid, newGroup);
            }
        }

        protected virtual void AddGroupEvents(IMyGridGroupData group)
        {
            if (group == null)
            {
                return;
            }
            RemoveGroupEvents(group);
            group.OnGridAdded += GridGroup_OnGridAdded;
            group.OnGridRemoved += GridGroup_OnGridRemoved;
            group.OnReleased += GridGroup_OnReleased;
        }

        protected virtual void RemoveGroupEvents(IMyGridGroupData group)
        {
            if (group == null)
            {
                return;
            }
            group.OnGridAdded -= GridGroup_OnGridAdded;
            group.OnGridRemoved -= GridGroup_OnGridRemoved;
            group.OnReleased -= GridGroup_OnReleased;
        }

        private void GridGroup_OnReleased(IMyGridGroupData group)
        {
            RemoveGroupEvents(group);
            gridGroup = null;
        }
    }
}
