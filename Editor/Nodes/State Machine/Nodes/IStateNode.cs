using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public interface IStateNode : IConnectable
    {
        string Name { get; }
        IndexedDictionary<(TransitionConnectionUI ui, TransitionInfo info), StateNodeUITransitionItem> ConnectionToTransitionItemMap { get; }
    }
}
