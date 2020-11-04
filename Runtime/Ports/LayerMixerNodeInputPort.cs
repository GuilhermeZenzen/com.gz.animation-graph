using System;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class LayerMixerNodeInputPort : NodeInputPort
    {
        public LayerBlendMode BlendMode;
        public AvatarMask AvatarMask;
    }
}
