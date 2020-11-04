using System;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Blendspace1DNodeInputPortAsset : NodeInputPortAsset
    {
        public float Threshold;

        public override NodeInputPort CreatePort(BaseNode node)
        {
            return ((Blendspace1DNode)node).CreateInputPort(Threshold);
        }
    }
}
