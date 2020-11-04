namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class TriggerProvider : IValueProvider
    {
        public bool Value;

        public IValueProvider Copy() => new TriggerProvider { Value = Value };
    }
}
