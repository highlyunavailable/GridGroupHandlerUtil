using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace GridGroupHandlerUtil
{

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Decoy), false)]
    public class ExampleEntityComponent : MyGameLogicComponent
    {
        private ExampleEventHandler handler;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            var decoy = Entity as IMyDecoy;
            handler = new ExampleEventHandler(Entity as IMyDecoy, this);
        }

        public override void MarkForClose()
        {
            base.MarkForClose();
            handler?.Close();
            handler = null;
        }

        public void OnChangedToNewGridGroup()
        {
            MyLog.Default.WriteLineAndConsole("ExampleEntityComponent: I changed grid group! How exciting!");
        }
    }
}
