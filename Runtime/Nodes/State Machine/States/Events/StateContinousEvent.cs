using System;

namespace GZ.AnimationGraph
{
    public class StateContinousEvent
    {
        public float StartTime;
        public float EndTime;

        public Action<State, StateContinousEvent> Callback;

        public bool JustStarted(State state) => state.PreviousNormalizedTime.Value <= StartTime && state.NormalizedTime.Value >= StartTime;
    }
}