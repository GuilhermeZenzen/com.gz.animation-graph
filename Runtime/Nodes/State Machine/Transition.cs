using System;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Transition
    {
        public string Id;

        [SerializeReference] public BaseState SourceState;
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

        public Transition Copy(Dictionary<BaseState, BaseState> copiedStates, Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap)
        {
            var copy = new Transition 
            { 
                Id = Id,
                DurationType = DurationType,
                Duration = Duration,
                OffsetType = OffsetType,
                Offset = Offset, 
                InterruptionSource = InterruptionSource,
                OrderedInterruption = OrderedInterruption,
                InterruptableByAnyState = InterruptableByAnyState, 
                PlayAfterTransition = PlayAfterTransition 
            };

            if (copiedStates.ContainsKey(SourceState)) { copy.SourceState = copiedStates[SourceState]; }

            if (copiedStates.ContainsKey(DestinationState)) { copy.DestinationState = (State)copiedStates[DestinationState]; }

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
