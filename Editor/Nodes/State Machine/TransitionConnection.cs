using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using GZ.AnimationGraph;
using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionConnectionUI : ConnectionUI
    {
        public List<TransitionInfo> Transitions = new List<TransitionInfo>();

        private int _arrowCount = 3;
        protected override int ArrowCount => Transitions.Count < 2 ? 1 : 3;

        public event Action<TransitionConnectionUI, TransitionInfo> OnCreatedTransition;
        public event Action<TransitionConnectionUI, TransitionInfo, int> OnRemovedTransition;

        public TransitionConnectionUI(bool isEnabled) : base(isEnabled) { }

        public void CreateTransition()
        {
            TransitionInfo transition = new TransitionInfo();
            Transitions.Add(transition);
            OnCreatedTransition?.Invoke(this, transition);

            MarkDirtyRepaint();
        }

        public void RemoveTransition(TransitionInfo transition)
        {
            if (Transitions.Count == 1)
            {
                Delete();
                return;
            }

            int index = Transitions.IndexOf(transition);
            Transitions.RemoveAt(index);

            MarkDirtyRepaint();
            OnRemovedTransition?.Invoke(this, transition, index);
        }
        public void RemoveTransition(int index)
        {
            if (Transitions.Count == 1)
            {
                Delete();
                return;
            }

            TransitionInfo transition = Transitions[index];
            Transitions.RemoveAt(index);

            MarkDirtyRepaint();
            OnRemovedTransition?.Invoke(this, transition, index);
        }
    }

    public class TransitionInfo
    {
        public DurationType DurationType;
        public float Duration;
        public DurationType OffsetType;
        public float Offset;
        public TransitionInterruptionSource InterruptionSource;
        public bool OrderedInterruption;
        public bool InterruptableByAnyState;
        public bool PlayAfterTransition;
        public List<TransitionInfoCondition> Conditions = new List<TransitionInfoCondition>();
    }

    public class TransitionInfoCondition
    {
        public ValueProviderSourceType ProviderSourceType = ValueProviderSourceType.Parameter;

        public ParameterNodeUI Parameter;
        public StateNodeUI State;
        public StateValueProviders StateValueProvider = StateValueProviders.Time;

        public bool BoolComparisonValue = true;

        public IntComparison IntComparison = IntComparison.Equal;
        public int IntComparisonValue;

        public FloatComparison FloatComparison = FloatComparison.BiggerOrEqual;
        public float FloatComparisonValue;
    }
}
