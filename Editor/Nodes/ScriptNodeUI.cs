using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using System;
using System.Linq;

namespace GZ.AnimationGraph.Editor
{
    public class WildcardNodeUI : BaseNodeUI
    {
        public Port InputPort { get; private set; }

        protected override string DefaultName => "Wildcard";

        public WildcardNodeUI() : base()
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "";
            inputContainer.Add(InputPort);

            GenerateOutputPort(Color.white);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset => InputPort);
        }

        public override NodeAsset GenerateData()
        {
            WildcardNode data = new WildcardNode();

            if (data != null)
            {
                data.Name = NameField.value;
                data.Speed = _speedField.value;
            }

            return new NodeAsset { Data = data };
        }

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            NodeAsset outputNodeAsset = InputPort.connections.Count() > 0 ? nodeMap[InputPort.connections.First().output.node] : null;
            nodeAsset.InputPorts.Add(new NodeInputPortAsset { SourceNodeAsset = outputNodeAsset, Weight = 1f });
        }
    }
}
