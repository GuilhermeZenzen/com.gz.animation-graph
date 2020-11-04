using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class StateMachineNodeAsset : NodeAsset
    {
        [SerializeReference] public ParameterAssetMap ParameterMap = new ParameterAssetMap();
        [SerializeReference] public StateAssetMap StateMap = new StateAssetMap();
        [SerializeReference] public AnyStateAssetMap AnyStateMap = new AnyStateAssetMap();
        [SerializeReference] public TransitionAssetMap TransitionMap = new TransitionAssetMap();
        [SerializeReference] public NodeVisualInfo AnyStatePriorityManager;
        [SerializeReference] public NodeVisualInfo EntryState = new NodeVisualInfo(Vector2.zero, true);
    }

    [Serializable]
    public class ParameterAssetMap : IndexedDictionary<string, NodeVisualInfo> { }

    [Serializable]
    public class StateAssetMap : IndexedDictionary<string, NodeVisualInfo> { }

    [Serializable]
    public class AnyStateAssetMap : IndexedDictionary<string, NodeVisualInfo> { }

    [Serializable]
    public class TransitionAssetMap : IndexedDictionary<string, NodeVisualInfo> { }

    [Serializable]
    public class NodeVisualInfo
    {
        public Vector2 Position;
        public bool IsExpanded;

        public NodeVisualInfo(Vector2 position, bool isExpanded) => (Position, IsExpanded) = (position, isExpanded);
    }
}
