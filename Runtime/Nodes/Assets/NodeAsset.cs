using GZ.AnimationGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class NodeAsset
    {
        public string Name;
        public string ID;

        public Vector2 Position;
        public bool IsExpanded;

        [SerializeReference]
        public List<NodeInputPortAsset> InputPorts = new List<NodeInputPortAsset>();

        [SerializeReference]
        public BaseNode Data;
    }
}
