using GZ.AnimationGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class Blendspace1DNodeUI : BaseNodeUI
    {
        private List<(Port inputPort, FloatField threshold)> _portMap = new List<(Port inputPort, FloatField threshold)>();

        private static Color _portColor = Color.white;

        protected override string DefaultName => "1D Blendspace";

        public Blendspace1DNodeUI() : base()
        {
            Button addLayerButton = new Button(() => AddPort()) { text = "Add Port" };
            titleContainer.Add(addLayerButton);

            GenerateOutputPort(_portColor);
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset =>
            {
                Blendspace1DNodeInputPortAsset inputPort = (Blendspace1DNodeInputPortAsset)portAsset;

                return AddPort(inputPort.Threshold, false);
            });
        }

        public Port AddPort(float threshold = 1f, bool refresh = true)
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = string.Empty;
            inputPort.portColor = _portColor;

            FloatField thresholdField = new FloatField() { value = threshold };
            thresholdField.style.minWidth = 40;
            inputPort.Add(thresholdField);

            MakePortRemovable(inputPort, index => _portMap.RemoveAt(index));

            inputContainer.Add(inputPort);

            _portMap.Add((inputPort, thresholdField));

            if (refresh)
            {
                RefreshExpandedState();
                RefreshPorts();
            }

            return inputPort;
        }

        public override NodeAsset GenerateData() => new NodeAsset { Data = new Blendspace1DNode { Name = NameField.value, Speed = _speedField.value } };

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            _portMap.ForEach(p =>
            {
                NodeAsset outputNodeAsset = p.inputPort.connections.Count() > 0 ? nodeMap[p.inputPort.connections.First().output.node] : null;
                nodeAsset.InputPorts.Add(new Blendspace1DNodeInputPortAsset { SourceNodeAsset = outputNodeAsset, Threshold = p.threshold.value });
            });
        }
    }
}
