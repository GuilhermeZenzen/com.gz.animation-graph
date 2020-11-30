namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class StateEventAsset
    {
        public string Name = "Event";
        public string StateName;

        public TimeType TimeType;

        public float Frame;
        public float NormalizedTime;

        public bool IsExpanded = true;
    }

    public enum TimeType
    {
        Frame,
        NormalizedTime
    }
}

