using GZ.AnimationGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Transition
    {
        public string Id;

        [SerializeReference] public State SourceState;
        [SerializeReference] public State DestinationState;

        public DurationType DurationType;
        public float Duration;
        public DurationType OffsetType;
        public float Offset;
        public TransitionInterruptionSource InterruptionSource;
        public bool OrderedInterruption;
        public bool InterruptableByAnyState;
        public bool PlayAfterTransition;

        [SerializeReference] public List<TransitionCondition> Conditions = new List<TransitionCondition>();

        public Transition() => Id = Guid.NewGuid().ToString();

        public Transition Copy(Dictionary<State, State> copiedStates, Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap)
        {
            var copy = new Transition 
            { 
                Duration = Duration, 
                Offset = Offset, 
                InterruptionSource = InterruptionSource, 
                InterruptableByAnyState = InterruptableByAnyState, 
                PlayAfterTransition = PlayAfterTransition 
            };

            if (copiedStates.ContainsKey(SourceState)) { copy.SourceState = copiedStates[SourceState]; }

            if (copiedStates.ContainsKey(DestinationState)) { copy.DestinationState = copiedStates[DestinationState]; }

            Conditions.ForEach(c =>
            {
                copy.Conditions.Add(c.Copy(valueProviderCopyMap));
            });

            return copy;
        }
    }

    public enum DurationType
    {
        Fixed,
        SourcePercentage,
        DestinationPercentage
    }

    public enum TransitionInterruptionSource
    {
        None,
        CurrentState,
        NextState,
        CurrentThenNextState,
        NextThenCurrentState
    }
}
