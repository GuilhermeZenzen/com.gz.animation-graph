using System;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class NodeOutputPort
    {
        [SerializeReference] public BaseNode Node;
        [SerializeReference] public NodeLink Link;
        [NonSerialized] public int Index;
    }
}
