using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GZ.AnimationGraph
{
    public class StateEvent
    {
        public Action<float, State> Callback;
        public float NormalizedTime;
    }
}
