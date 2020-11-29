using GZ.Tools.UnityUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class State : BaseState, INamedItem<State, StateGroup>
    {
        public StateGroup Group { get; set; }

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

        [SerializeReference]
        public List<Transition> EntryTransitions = new List<Transition>();

        public override BaseState Copy(Func<Transition, Transition> transitionCopyCallback, Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap = null)
        {
            State copy = (State)base.Copy(transitionCopyCallback, valueProviderCopyMap);

            copy.Name = Name;
            EntryTransitions.ForEach(t => copy.EntryTransitions.Add(transitionCopyCallback(t)));

            return copy;
        }

        public void SetNameWithoutNotify(string newName) => _name = newName;

        protected override BaseState GetCopyInstance() => new State() { Name = Name };
    }
}
