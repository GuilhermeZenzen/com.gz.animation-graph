using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class State
    {
        public string Id;

        public string Name;

        public NodeInputPort InputPort;

        [SerializeReference]
        public List<Transition> EntryTransitions = new List<Transition>();

        [SerializeReference]
        public List<Transition> ExitTransitions = new List<Transition>();

        [SerializeReference] public FloatProvider Time = new FloatProvider();
        [SerializeReference] public FloatProvider NormalizedTime = new FloatProvider();
        [SerializeReference] public FloatProvider PreviousTime = new FloatProvider();
        [SerializeReference] public FloatProvider PreviousNormalizedTime = new FloatProvider();

        public State() => Id = Guid.NewGuid().ToString();

        public State Copy(Func<Transition, Transition> transitionCopyCallback, Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap = null)
        {
            var copy = GetCopyInstance();

            FloatProvider CopyProvider(FloatProvider originalProvider)
            {
                if (valueProviderCopyMap != null && valueProviderCopyMap.ContainsKey(originalProvider))
                {
                    return (FloatProvider)valueProviderCopyMap[originalProvider];
                }
                else
                {
                    var providerCopy = (FloatProvider)originalProvider.Copy();

                    if (valueProviderCopyMap != null)
                    {
                        valueProviderCopyMap.Add(originalProvider, providerCopy);
                    }

                    return providerCopy;
                }
            }

            copy.Time = CopyProvider(Time);
            copy.PreviousTime = CopyProvider(PreviousTime);
            copy.NormalizedTime = CopyProvider(NormalizedTime);
            copy.PreviousNormalizedTime = CopyProvider(PreviousNormalizedTime);

            EntryTransitions.ForEach(t => copy.EntryTransitions.Add(transitionCopyCallback(t)));
            ExitTransitions.ForEach(t => copy.ExitTransitions.Add(transitionCopyCallback(t)));

            return copy;
        }

        protected virtual State GetCopyInstance() => new State() { Name = Name };
    }
}
