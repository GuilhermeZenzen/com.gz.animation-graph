namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class TriggerConditionEvaluator : IConditionEvaluator
    {
        public bool Evaluate(IValueProvider valueProvider) => ((TriggerProvider)valueProvider).Value;
    }
}
