using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace GridGroupHandlerUtil
{
    public abstract class BlockCentricGridGroupNotifyingEventHandlerBase : BlockCentricGridGroupEventHandlerBase
    {
        private readonly Guid dataKey;

        public BlockCentricGridGroupNotifyingEventHandlerBase(IMyCubeBlock cubeBlock, GridLinkTypeEnum linkType, Guid gridGroupDataKey) : base(cubeBlock, linkType)
        {
            dataKey = gridGroupDataKey;

            AddToGridGroupDataAndNotify(gridGroup);
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
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
            if (groupToNotify.TryGetVariable(dataKey, out data))
            {
                data.Add(this);
                foreach (var otherHandler in data)
                {
                    if (otherHandler == this)
                    {
                        continue;
                    }
                    otherHandler.OnBlockEnteredGroup(block);
                }
            }
            else
            {
                groupToNotify.SetVariable(dataKey, new HashSet<BlockCentricGridGroupNotifyingEventHandlerBase>() { this });
            }
        }

        private void RemoveFromGridGroupDataAndNotify(IMyGridGroupData groupToNotify)
        {
            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
            if (groupToNotify.TryGetVariable(dataKey, out data))
            {
                data.Remove(this);
                if (block == null)
                {
                    return;
                }
                foreach (var otherHandler in data)
                {
                    otherHandler.OnBlockLeftGroup(block);
                }
            }
        }

        public abstract void OnBlockEnteredGroup(IMyCubeBlock otherBlock);
        public abstract void OnBlockLeftGroup(IMyCubeBlock otherBlock);

        protected override void OnGridGroupChanged(IMyGridGroupData oldGroup)
        {
            base.OnGridGroupChanged(oldGroup);
            if (oldGroup != null)
            {
                RemoveFromGridGroupDataAndNotify(oldGroup);
            }

            if (gridGroup != null)
            {
                AddToGridGroupDataAndNotify(gridGroup);
            }
        }
    }
}
