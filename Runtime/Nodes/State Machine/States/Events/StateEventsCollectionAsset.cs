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

        public void LoadEvents(StateMachineNode stateMachine, IReadOnlyDictionary<string, Action<float, State>> eventCallbacks, IReadOnlyDictionary<string, Action<State, StateContinousEvent>> continousEventCallbacks)
        {
            StatesEvents.ForEach(eventState =>
            {
                //Assert.IsTrue(stateMachine.States.ContainsKey(stateEvents.Name), $"The state machine {stateMachine.Name} from the graph {stateMachine.Graph.Name} doesn't contain the state {stateEvents.Name} required by the asset {name}");

                if (!stateMachine.States.TryGetValue(eventState.Name, out var state)) { return; }

                eventState.Events.ForEach(evt =>
                {
                    //Assert.IsTrue(callbacks.ContainsKey(evt.Name), $"The passed callbacks doesn't contain the callback required by the event {evt.Name} from the asset {name}");

                    if (evt.Type == EventType.Trigger)
                    {
                        if (eventCallbacks.TryGetValue(evt.Name, out var callback))
                        {
                            state.AddEvent(evt.TriggerTime, eventCallbacks[evt.Name]);
                        }
                    }
                    else
                    {
                        if (continousEventCallbacks.TryGetValue(evt.Name, out var continousCallback))
                        {
                            state.AddContinousEvent(evt.StartTime, evt.EndTime, continousCallback);
                        }
                    }
                });
            });
        }
    }
}
