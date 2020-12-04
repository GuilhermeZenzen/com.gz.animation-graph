using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GZ.AnimationGraph
{
    [CreateAssetMenu(fileName = "State Events Collection", menuName = "GZ/Animation/State Events Collection")]
    public class StateEventsCollectionAsset : ScriptableObject
    {
        public List<StateEventsAsset> StatesEvents = new List<StateEventsAsset>();

        public void LoadEvents(StateMachineNode stateMachine, IReadOnlyDictionary<string, Action<State>> callbacks)
        {
            StatesEvents.ForEach(stateEvents =>
            {
                Assert.IsTrue(stateMachine.States.ContainsKey(stateEvents.Name), $"The state machine {stateMachine.Name} from the graph {stateMachine.Graph.Name} doesn't contain the state {stateEvents.Name} required by the asset {name}");

                State state = stateMachine.States[stateEvents.Name];

                stateEvents.Events.ForEach(evt =>
                {
                    Assert.IsTrue(callbacks.ContainsKey(evt.Name), $"The passed callbacks doesn't contain the callback required by the event {evt.Name} from the asset {name}");

                    state.AddEvent(evt.NormalizedTime, callbacks[evt.Name]);
                });
            });
        }
    }
}
