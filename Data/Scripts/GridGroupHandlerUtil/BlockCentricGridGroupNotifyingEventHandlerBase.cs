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

            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
            if (gridGroup.TryGetVariable(dataKey, out data))
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
                gridGroup.SetVariable(dataKey, new HashSet<BlockCentricGridGroupNotifyingEventHandlerBase>() { this });
            }
        }

        protected override void RemoveBlockEvents(IMyCubeBlock block)
        {
            base.RemoveBlockEvents(block);

            if (gridGroup == null)
            {
                return;
            }

            HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
            if (gridGroup.TryGetVariable(dataKey, out data))
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
                HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
                if (oldGroup.TryGetVariable(dataKey, out data))
                {
                    data.Remove(this);
                    foreach (var otherHandler in data)
                    {
                        otherHandler.OnBlockLeftGroup(block);
                    }
                }
            }

            if (gridGroup != null)
            {
                HashSet<BlockCentricGridGroupNotifyingEventHandlerBase> data;
                if (gridGroup.TryGetVariable(dataKey, out data))
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
                    gridGroup.SetVariable(dataKey, new HashSet<BlockCentricGridGroupNotifyingEventHandlerBase>() { this });
                }
            }
        }
    }
}
