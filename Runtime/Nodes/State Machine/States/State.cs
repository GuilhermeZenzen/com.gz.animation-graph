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

        public bool ManualTime { get; set; } = false;

        [SerializeReference]
        public List<Transition> EntryTransitions = new List<Transition>();

        [NonSerialized] public List<StateEvent> Events;
        [NonSerialized] public List<StateContinousEvent> ContinousEvents;

        public void SetTime(float time)
        {
            PreviousTime.Value = Time.Value;
            Time.Value = time;
        }

        public void PassTime(float timeDelta)
        {
            PreviousTime.Value = Time.Value;
            Time.Value += timeDelta;
        }

        public void SetNormalizedTime(float normalizedTime)
        {
            PreviousNormalizedTime.Value = NormalizedTime.Value;
            NormalizedTime.Value = normalizedTime;
        }

        public void PassNormalizedTime(float normalizedTimeDelta)
        {
            PreviousNormalizedTime.Value = NormalizedTime.Value;
            NormalizedTime.Value += normalizedTimeDelta;
        }

        public void AddEvent(float normalizedTime, Action<float, State> callback)
        {
            Events ??= new List<StateEvent>();

            Events.Add(new StateEvent { Callback = callback, NormalizedTime = normalizedTime });
        }

        public void AddContinousEvent(float startTime, float endTime, Action<State, StateContinousEvent> callback)
        {
            ContinousEvents ??= new List<StateContinousEvent>();

            ContinousEvents.Add(new StateContinousEvent { StartTime = startTime, EndTime = endTime, Callback = callback });
        }

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
