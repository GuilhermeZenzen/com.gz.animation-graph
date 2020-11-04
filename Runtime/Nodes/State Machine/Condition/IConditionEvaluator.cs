namespace GZ.AnimationGraph
{
    public interface IConditionEvaluator
    {
        bool Evaluate(IValueProvider valueProvider);
    }
}
