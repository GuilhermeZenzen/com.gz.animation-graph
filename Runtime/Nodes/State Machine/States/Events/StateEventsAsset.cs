using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class StateEventsAsset
    {
        public string Name;

        public List<StateEventAsset> Events = new List<StateEventAsset>();

        public bool IsExpanded = true;
    }
}
