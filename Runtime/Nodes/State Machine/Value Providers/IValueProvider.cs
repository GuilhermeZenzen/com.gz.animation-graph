namespace GZ.AnimationGraph
{
    public interface IValueProvider
    {
        IValueProvider Copy();
    }

    public enum ValueProviderType
    {
        Bool, Int, Float, Trigger
    }
}