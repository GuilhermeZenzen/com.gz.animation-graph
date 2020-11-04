using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class IntConditionEvaluator : IConditionEvaluator
    {
        public IntComparison Comparison;
        public int ComparisonValue;

        public bool Evaluate(IValueProvider valueProvider) => ValueComparator.CompareInt(((IntProvider)valueProvider).Value, ComparisonValue, Comparison);
    }
}
