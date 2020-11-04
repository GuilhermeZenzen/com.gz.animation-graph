namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class IntProvider : IValueProvider
    {
        public int Value;

        public IValueProvider Copy() => new IntProvider { Value = Value };
    }
}
