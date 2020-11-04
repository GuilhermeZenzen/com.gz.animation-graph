namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class FloatProvider : IValueProvider
    {
        public float Value;

        public IValueProvider Copy() => new FloatProvider { Value = Value };
    }
}