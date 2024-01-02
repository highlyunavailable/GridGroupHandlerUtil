using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace GridGroupHandlerUtil
{
    /// <summary>
    /// A handler that keeps track of other blocks in the grid group and will notify them when other tracked blocks
    /// with the same data key have entered the grid group.
    /// </summary>
    /// <typeparam name="TData">The data bag type that is persisted between grid groups</typeparam>
    public abstract class BlockCentricGridGroupNotifyingEventHandlerBase<TData> : BlockCentricGridGroupEventHandlerBase
    {
        private ulong generation = 0;
        private TData data;
        public BlockCentricGridGroupNotifyingEventHandlerBase(IMyCubeBlock cubeBlock, GridLinkTypeEnum linkType) : base(cubeBlock, linkType)
        {
            AddToGridGroupDataAndNotify(gridGroup);
        }

        /// <summary>
        /// Must be a GUID that is the same between all instances of this handler, used to key the grid group data.
        /// </summary>
        /// <returns>The Grid Group data key</returns>
        protected abstract Guid GetDataKey();

        /// <summary>
        /// The data bag for the handler. This is persisted between grid groups.
        /// To clear it during grid group movement, override ResetData in your handler.
        /// </summary>
        public TData Data
        {
            get { return data; }
            set
            {
                data = value;
                generation++;
            }
        }

        /// <summary>
        /// Track the Generation in your own types to know if the grid group data has changed or not
        /// </summary>
        public ulong Generation => generation;

        protected IReadOnlyCollection<BlockCentricGridGroupNotifyingEventHandlerBase<TData>> GetGroupBlocks()
        {
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase<TData>> groupHandlers;
            if (gridGroup.TryGetVariable(GetDataKey(), out groupHandlers))
            {
                return groupHandlers;
            }
            return Array.Empty<BlockCentricGridGroupNotifyingEventHandlerBase<TData>>();
        }

        protected override void RemoveBlockEvents(IMyCubeBlock block)
        {
            base.RemoveBlockEvents(block);

            if (gridGroup == null)
            {
                return;
            }

            RemoveFromGridGroupDataAndNotify(gridGroup);
        }

        private void AddToGridGroupDataAndNotify(IMyGridGroupData groupToNotify)
        {
            if (groupToNotify == null)
            {
                return;
            }
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase<TData>> groupHandlers;
            if (groupToNotify.TryGetVariable(GetDataKey(), out groupHandlers))
            {
                if (groupHandlers.Add(this))
                {
                    generation++;
                    foreach (var otherHandler in groupHandlers)
                    {
                        if (otherHandler == this)
                        {
                            continue;
                        }
                        otherHandler.OnBlockEnteredGroup(block);
                    }
                }
            }
            else // There were no registered handlers
            {
                generation++;
                groupToNotify.SetVariable(GetDataKey(), new HashSet<BlockCentricGridGroupNotifyingEventHandlerBase<TData>>() { this });
            }
        }

        private void RemoveFromGridGroupDataAndNotify(IMyGridGroupData groupToNotify)
        {
            if (groupToNotify == null)
            {
                return;
            }
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase<TData>> groupHandlers;
            if (groupToNotify.TryGetVariable(GetDataKey(), out groupHandlers))
            {
                if (groupHandlers.Remove(this))
                {
                    generation++;
                    if (block == null)
                    {
                        return;
                    }
                    foreach (var otherHandler in groupHandlers)
                    {
                        otherHandler.OnBlockLeftGroup(block);
                    }
                }
            }
        }

        public virtual void OnBlockEnteredGroup(IMyCubeBlock otherBlock) { }
        public virtual void OnBlockLeftGroup(IMyCubeBlock otherBlock) { }

        protected override void OnGridGroupChanged(IMyGridGroupData oldGroup)
        {
            base.OnGridGroupChanged(oldGroup);
            if (oldGroup != null)
            {
                RemoveFromGridGroupDataAndNotify(oldGroup);
            }

            ResetData();

            if (gridGroup != null)
            {
                AddToGridGroupDataAndNotify(gridGroup);
            }
        }

        /// <summary>
        /// Optional. Override this method to do something to reset the data. This is called after the old grid group has been notified
        /// the block has left, but before the new grid group is notified that the block has joined during OnGridGroupChanged.
        /// </summary>
        protected virtual void ResetData() { generation++; }

        /// <summary>
        /// Manually called to notify all group members with this data key that something has changed.
        /// Used for something like an async scan task finishing that you want all members to take notice of.
        /// </summary>
        /// <param name="groupToNotify">The grid group to notify (can be used to notify an old grid group)</param>
        private void NotifyGridGroupOfDataChange(IMyGridGroupData groupToNotify)
        {
            if (groupToNotify == null)
            {
                return;
            }
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase<TData>> groupHandlers;
            if (groupToNotify.TryGetVariable(GetDataKey(), out groupHandlers))
            {
                foreach (var otherHandler in groupHandlers)
                {
                    if (otherHandler == this)
                    {
                        continue;
                    }
                    otherHandler.OnDataChange(block);
                }
            }
        }
        public virtual void OnDataChange(IMyCubeBlock changeSource) { }
    }
}
