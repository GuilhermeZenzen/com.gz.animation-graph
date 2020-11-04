using System;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class NodeInputPortAsset
    {
        [SerializeReference]
        public NodeAsset SourceNodeAsset;

        public float Weight;

        public virtual NodeInputPort CreatePort(BaseNode node)
        {
            return node.CreateBaseInputPort(Weight);
        }
    }
}
