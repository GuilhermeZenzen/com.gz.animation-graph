using System;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class TransitionCondition
    {
        [field: SerializeReference] public IValueProvider ValueProvider { get; private set; }

        [SerializeReference] public IConditionEvaluator Evaluator;

        public bool IsTrue() => Evaluator.Evaluate(ValueProvider);

        public void SetValueProvider(IValueProvider valueProvider)
        {
            ValueProvider = valueProvider;

            switch (valueProvider)
            {
                case BoolProvider boolProvider:
                    Evaluator = new BoolConditionEvaluator();
                    break;
                case IntProvider intProvider:
                    Evaluator = new IntConditionEvaluator();
                    break;
                case FloatProvider floatProvider:
                    Evaluator = new FloatConditionEvaluator();
                    break;
                case TriggerProvider triggerProvider:
                    Evaluator = new TriggerConditionEvaluator();
                    break;
            }
        }

        public void SetValueProviderWithoutUpdate(IValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
        }

        public TransitionCondition Copy(Dictionary<IValueProvider, IValueProvider> valueProviderCopyMap)
        {
            var copy = new TransitionCondition();

            if (valueProviderCopyMap.ContainsKey(ValueProvider))
            {
                copy.SetValueProvider(valueProviderCopyMap[ValueProvider]);
            }
            else
            {
                copy.SetValueProvider(ValueProvider.Copy());
                valueProviderCopyMap.Add(ValueProvider, copy.ValueProvider);
            }

            switch (copy.Evaluator)
            {
                case BoolConditionEvaluator boolEvaluator:
                    boolEvaluator.ComparisonValue = ((BoolConditionEvaluator)Evaluator).ComparisonValue;
                    break;
                case IntConditionEvaluator intEvaluator:
                    var originalIntEvaluator = (IntConditionEvaluator)Evaluator;
                    intEvaluator.Comparison = originalIntEvaluator.Comparison;
                    intEvaluator.ComparisonValue = originalIntEvaluator.ComparisonValue;
                    break;
                case FloatConditionEvaluator floatEvaluator:
                    var originalFloatEvaluator = (FloatConditionEvaluator)Evaluator;
                    floatEvaluator.Comparison = originalFloatEvaluator.Comparison;
                    floatEvaluator.ComparisonValue = originalFloatEvaluator.ComparisonValue;
                    break;
                default:
                    break;
            }

            return copy;
        }
    }
}
