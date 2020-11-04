using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GZ.AnimationGraph.Editor
{
    public class OutputNodeUI : Node
    {
        public OutputNodeInputPort InputPort { get; private set; }

        public OutputNodeUI()
        {
            title = "Output";

            InputPort = new OutputNodeInputPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "";
            inputContainer.Add(InputPort);

            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
