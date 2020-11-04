using System;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class LayerMixerNodeInputPortAsset : NodeInputPortAsset
    {
        public string Name;
        public LayerBlendMode BlendMode;
        public AvatarMask AvatarMask;

        public override NodeInputPort CreatePort(BaseNode node)
        {
            return ((LayerMixerNode)node).CreateInputPort(BlendMode, Weight, AvatarMask);
        }
    }
}
