﻿using GZ.AnimationGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnyStatePriorityManagerNodeUI : StateMachineBaseNodeUI
    {
        private static readonly Color _portColor = Color.white;

        public AnyStatePriorityManagerNodeUI()
        {
            title = "Any State Priority Manager";

            Button createAnyStatePriorityPortButton = new Button(() => CreateAnyStatePriorityPort()) { text = "+ Any State" };

            inputContainer.Add(createAnyStatePriorityPortButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port CreateAnyStatePriorityPort()
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            port.portName = string.Empty;
            port.portColor = _portColor;

            MakePortRemovable(port);

            inputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        public void LoadData(GraphView graphView, List<AnyState> anyStates, Dictionary<AnyState, AnyStateNodeUI> map)
        {
            anyStates.ForEach(s =>
            {
                Port port = CreateAnyStatePriorityPort();

                if (s == null) { return; }

                var anyStateNode = (AnyStateNodeUI)map[s];

                Edge edge = new Edge { output = anyStateNode.PriorityPort, input = port };

                edge.output.Connect(edge);
                edge.input.Connect(edge);

                graphView.AddElement(edge);

            });
        }
    }
}
