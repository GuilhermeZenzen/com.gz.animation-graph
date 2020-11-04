using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class FloatConditionEvaluator : IConditionEvaluator
    {
        public FloatComparison Comparison;
        public float ComparisonValue;

        public bool Evaluate(IValueProvider valueProvider)
        {
            return ValueComparator.CompareFloat(((FloatProvider)valueProvider).Value, ComparisonValue, Comparison);
        }
    }
}
