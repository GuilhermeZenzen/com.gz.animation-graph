﻿using GZ.AnimationGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public abstract class BaseNodeUI : Node
    {
        public string ID;

        public TextField NameField { get; private set; }
        protected FloatField _speedField;

        public Port OutputPort { get; private set; }

        protected virtual string DefaultName => "Node";

        public BaseNodeUI()
        {
            VisualElement container = new VisualElement();

            title = DefaultName;
            Label titleLabel = (Label)titleContainer[0];
            titleContainer.RemoveAt(0);

            NameField = new TextField();
            NameField.style.flexGrow = 1f;

            container.Add(titleLabel);
            container.Add(NameField);

            titleContainer.Insert(0, container);

            _speedField = new FloatField("Speed");
            _speedField.SetValueWithoutNotify(1f);
            _speedField.AddToClassList("speed-field");
            mainContainer.Insert(1, _speedField);

            RefreshExpandedState();
            RefreshPorts();
        }

        protected void MakePortRemovable(Port port, Action<int> callback)
        {
            Button removePortButton = new Button(() =>
            {
                int portIndex = inputContainer.IndexOf(port);

                foreach (var edge in port.connections)
                {
                    edge.output.Disconnect(edge);
                    edge.RemoveFromHierarchy();
                }
                
                port.DisconnectAll();

                inputContainer.RemoveAt(portIndex);

                callback(portIndex);

                RefreshExpandedState();
                RefreshPorts();
            })
            { text = "-" };

            port.Add(removePortButton);
        }

        protected virtual void GenerateOutputPort(Color portColor)
        {
            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(BaseNode));
            OutputPort.portName = "";
            OutputPort.portColor = portColor;
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public abstract NodeAsset GenerateData();

        public abstract void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap);

        public virtual void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            ID = string.IsNullOrEmpty(nodeAsset.ID) ? Guid.NewGuid().ToString() : nodeAsset.ID;

            NameField.SetValueWithoutNotify(string.IsNullOrEmpty(nodeAsset.Data.Name) ? DefaultName : nodeAsset.Data.Name);
            _speedField.SetValueWithoutNotify(nodeAsset.Data.Speed);
        }

        public void LoadDataWithCallback(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap, Func<NodeInputPortAsset, Port> callback)
        {
            for (int i = 0; i < nodeAsset.InputPorts.Count; i++)
            {
                Port inputPortUI = callback(nodeAsset.InputPorts[i]);

                if (nodeAsset.InputPorts[i].SourceNodeAsset != null)
                {
                    Edge edge = new Edge { input = inputPortUI, output = (Port)nodeMap[nodeAsset.InputPorts[i].SourceNodeAsset].outputContainer[0] };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);

                    graphView.AddElement(edge);
                }
            }

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
