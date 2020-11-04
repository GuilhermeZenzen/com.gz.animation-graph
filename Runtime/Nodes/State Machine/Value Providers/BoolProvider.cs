namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class BoolProvider : IValueProvider
    {
        public bool Value;

        public IValueProvider Copy() => new BoolProvider { Value = Value };
    }
}

