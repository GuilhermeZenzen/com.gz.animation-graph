using GZ.Tools.UnityUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateNodeUI : BaseStateNodeUI<StateNodeUI>
    {
        public override string Title => "State";

        public override bool HasTwoWaysConnection => true;
    }
}
