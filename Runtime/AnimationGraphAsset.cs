using GZ.AnimationGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Animation Graph", menuName = "GZ/Animation Graph Asset")]
public class AnimationGraphAsset : ScriptableObject
{
    [SerializeReference]
    public List<NodeAsset> Nodes = new List<NodeAsset>();

#if UNITY_EDITOR
    private void OnEnable()
    {
        Nodes.ForEach(n =>
        {
            switch (n.Data)
            {
                case StateMachineNode stateMachineNode:
                    //foreach (var state in stateMachineNode.States.Values)
                    //{
                    //    state.EntryTransitions.ForEach(t =>
                    //    {
                    //        if (t != null) { t.DestinationState = state; }
                    //    });
                    //    state.ExitTransitions.ForEach(t =>
                    //    {
                    //        if (t != null) { t.SourceState = state; }
                    //    });
                    //}
                    //foreach (var anyState in stateMachineNode.AnyStates)
                    //{
                    //    anyState.ExitTransitions.ForEach(t =>
                    //    {
                    //        if (t != null) { t.SourceState = anyState; }
                    //    });
                    //    anyState.StateFilter.ForEach(f =>
                    //    {
                    //        if (f.State != null) { f.State = stateMachineNode.States[f.State.Name]; }
                    //    });
                    //}
                    //foreach (var transition in stateMachineNode.Transitions)
                    //{
                    //    transition.Conditions.ForEach(c =>
                    //    {
                    //        if (c.ValueProvider != null)
                    //        {
                    //            c.ValueProvider(stateMachineNode.Parameters[c.ValueProvider.Name]);
                    //        }
                    //    });
                    //}
                    break;
                default:
                    break;
            }
        });
    }
#endif
}
