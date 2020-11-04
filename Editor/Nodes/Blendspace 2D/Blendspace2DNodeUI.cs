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
    public class Blendspace2DNodeUI : BaseNodeUI
    {
        private List<(Port inputPort, FloatField x, FloatField y)> _portMap = new List<(Port inputPort, FloatField x, FloatField y)>();

        private EnumField _blendingModeField;

        private static readonly Color _portColor = new Color(252 / 255f, 231 / 255f, 3 / 255f);

        protected override string DefaultName => "2D Blendspace";

        public Blendspace2DNodeUI() : base()
        {
            Button addLayerButton = new Button(() => AddPort()) { text = "Add Port" };
            titleContainer.Add(addLayerButton);

            _blendingModeField = new EnumField(default(Blendspace2DBlendingMode));
            extensionContainer.Add(_blendingModeField);

            GenerateOutputPort(_portColor);
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset =>
            {
                Blendspace2DNodeInputPortAsset inputPort = (Blendspace2DNodeInputPortAsset)portAsset;

                return AddPort(inputPort.X, inputPort.Y, false);
            });
        }

        public Port AddPort(float x = 0f, float y = 0f, bool refresh = true)
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = string.Empty;
            inputPort.portColor = _portColor;

            FloatField xField = new FloatField() { value = x };
            xField.style.minWidth = 40;
            inputPort.Add(xField);

            FloatField yField = new FloatField() { value = y };
            yField.style.minWidth = 40;
            inputPort.Add(yField);

            MakePortRemovable(inputPort, index => _portMap.RemoveAt(index));

            inputContainer.Add(inputPort);

            _portMap.Add((inputPort, xField, yField));

            if (refresh)
            {
                RefreshExpandedState();
                RefreshPorts();
            }

            return inputPort;
        }

        public override NodeAsset GenerateData() => new NodeAsset { Data = new Blendspace2DNode { Name = _nameField.value, Speed = _speedField.value, BlendingMode = (Blendspace2DBlendingMode)_blendingModeField.value } };

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            _portMap.ForEach(p =>
            {
                NodeAsset outputNodeAsset = p.inputPort.connections.Count() > 0 ? nodeMap[p.inputPort.connections.First().output.node] : null;
                nodeAsset.InputPorts.Add(new Blendspace2DNodeInputPortAsset { SourceNodeAsset = outputNodeAsset, X = p.x.value, Y = p.y.value });
            });
        }
    }
}
