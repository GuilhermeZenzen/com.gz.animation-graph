namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class StateEventAsset
    {
        public string Name = "Event";
        public EventType Type;

        public float TriggerTime;

        public float StartTime;
        public float EndTime;
    }

    public enum EventType
    {
        Trigger,
        Continous
    }

    public enum TimeType
    {
        Frame,
        NormalizedTime
    }
}

