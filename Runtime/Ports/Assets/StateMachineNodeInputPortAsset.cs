using System;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class StateMachineNodeInputPortAsset : NodeInputPortAsset
    {
        public string StateName;

        public override NodeInputPort CreatePort(BaseNode node)
        {
            var stateMachine = (StateMachineNode)node;
            return stateMachine.States[StateName].InputPort;
        }
    }
}
