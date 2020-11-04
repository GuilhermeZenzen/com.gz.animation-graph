using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class AnyState : State
    {
        public const string AnyStateName = "Any State";

        [SerializeReference] public List<StateFilterItem> StateFilter = new List<StateFilterItem>();

        public AnyState() : base()
        {
            Name = AnyStateName;
        }

        protected override State GetCopyInstance() => new AnyState();
    }

    [Serializable]
    public class StateFilterItem
    {
        [SerializeReference] public State State;
        public AnyStateFilterMode Mode;
    }

    [Serializable]
    public class StateFilter : IndexedDictionary<State, AnyStateFilterMode> { }

    public enum AnyStateFilterMode
    {
        CurrentState,
        NextState,
        CurrentAndNextState
    }
}
