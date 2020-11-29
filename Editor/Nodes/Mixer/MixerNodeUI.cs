using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class MixerNodeUI : BaseNodeUI
    {
        private List<(Port inputPort, FloatField weight)> _portMap = new List<(Port inputPort, FloatField weight)>();

        private static Color _portColor = new Color(252 / 255f, 65 / 255f, 3 / 255f);

        protected override string DefaultName => "Mixer";

        public MixerNodeUI() : base()
        {
            Button addPortButton = new Button(() => AddPort()) { text = "Add Port" };
            titleContainer.Add(addPortButton);

            GenerateOutputPort(_portColor);
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset => AddPort(portAsset.Weight, false));
        }

        public Port AddPort(float weight = 1f, bool refresh = true)
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = string.Empty;
            inputPort.portColor = _portColor;

            FloatField weightField = new FloatField() { value = weight };
            weightField.style.minWidth = 40;
            inputPort.Add(weightField);

            MakePortRemovable(inputPort, index => _portMap.RemoveAt(index));

            inputContainer.Add(inputPort);

            _portMap.Add((inputPort, weightField));

            if (refresh)
            {
                RefreshExpandedState();
                RefreshPorts();
            }

            return inputPort;
        }

        public override NodeAsset GenerateData() => new NodeAsset { Data = new MixerNode { Name = NameField.value, Speed = _speedField.value } };

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            _portMap.ForEach(p =>
            {
                NodeAsset outputNodeAsset = p.inputPort.connections.Count() > 0 ? nodeMap[p.inputPort.connections.First().output.node] : null;
                nodeAsset.InputPorts.Add(new NodeInputPortAsset { SourceNodeAsset = outputNodeAsset, Weight = p.weight.value });
            });
        }
    }
}
