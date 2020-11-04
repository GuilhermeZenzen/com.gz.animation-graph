using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class BoolConditionEvaluator : IConditionEvaluator
    {
        public Bool ComparisonValue;

        public bool Evaluate(IValueProvider valueProvider) => ValueComparator.CompareBool(((BoolProvider)valueProvider).Value, ComparisonValue);
    }
}
