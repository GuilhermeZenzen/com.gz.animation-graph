namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class StateEventAsset
    {
        public string Name = "Event";
        public float NormalizedTime;

        public bool IsExpanded = true;
    }

    public enum TimeType
    {
        Frame,
        NormalizedTime
    }
}

