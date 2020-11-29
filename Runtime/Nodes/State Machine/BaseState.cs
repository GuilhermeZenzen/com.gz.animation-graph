using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph
{
    [Serializable]
    public abstract class BaseState
    {
        public string Id;

        public NodeInputPort InputPort;

        [SerializeReference]
        public List<Transition> ExitTransitions = new List<Transition>();

        [SerializeReference] public FloatProvider Time = new FloatProvider();
        [SerializeReference] public FloatProvider NormalizedTime = new FloatProvider();
        [SerializeReference] public FloatProvider PreviousTime = new FloatProvider();
        [SerializeReference] public FloatProvider PreviousNormalizedTime = new FloatProvider();

        public BaseState() => Id = Guid.NewGuid().ToString();

        public virtual BaseState Copy(Func<Transition, Transition> transitionCopyCallback, Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap = null)
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

            ExitTransitions.ForEach(t => copy.ExitTransitions.Add(transitionCopyCallback(t)));

            return copy;
        }

        protected abstract BaseState GetCopyInstance();
    }
}
