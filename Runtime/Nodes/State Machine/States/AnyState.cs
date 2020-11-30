using GZ.Tools.UnityUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class AnyState : BaseState, INamedItem<AnyState>
    {
        public NamedItemsGroup<AnyState> Group { get; set; }

        [SerializeField] protected string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (Group == null)
                {
                    _name = value;
                }
                else
                {
                    Group.RenameItem(this, value);
                }
            }
        }

        [SerializeReference] public List<AnyStateFilter> StateFilters = new List<AnyStateFilter>();

        public void SetNameWithoutNotify(string newName) => _name = newName;

        protected override BaseState GetCopyInstance() => new AnyState();

    }

    [Serializable]
    public class AnyStateFilter
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
