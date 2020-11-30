using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GZ.AnimationGraph
{
    [CreateAssetMenu(fileName = "State Events", menuName = "GZ/Animation/State Events")]
    public class StateEventsAsset : ScriptableObject
    {
        public List<StateEventAsset> Events;

        public void LoadEvents(StateMachineNode stateMachine, Dictionary<string, Action<State>> callbacks)
        {
            Events.ForEach(evt =>
            {
                Assert.IsTrue(stateMachine.States.ContainsKey(evt.StateName), $"The state machine {stateMachine.Name} from the graph {stateMachine.Graph.Name} doesn't contain the state {evt.StateName} required by the event {evt.Name} from the asset {name}");

                State state = stateMachine.States[evt.StateName];

                Assert.IsTrue(callbacks.ContainsKey(evt.Name), $"The passed callbacks doesn't contain the callback required by the event {evt.Name} from the asset {name}");

                state.AddEvent(evt.NormalizedTime, callbacks[evt.Name]);
            });
        }
    }
}
