using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class LayerMixerNodeUI : BaseNodeUI
    {
        private List<(Port inputPort, TextField name, FloatField weight, EnumField blendMode, ObjectField avatarMask)> _portMap = new List<(Port inputPort, TextField name, FloatField weight, EnumField blendMode, ObjectField avatarMask)>();

        private static Color _portColor = new Color(3 / 255f, 190 / 255f, 252 / 255f);

        protected override string DefaultName => "Layer Mixer";

        public LayerMixerNodeUI() : base()
        {
            Button addLayerButton = new Button(() => AddLayer("New Layer")) { text = "Add Layer" };
            titleButtonContainer.Add(addLayerButton);

            GenerateOutputPort(_portColor);
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset =>
            {
                LayerMixerNodeInputPortAsset inputPort = (LayerMixerNodeInputPortAsset)portAsset;

                return AddLayer(inputPort.Name, inputPort.Weight, inputPort.BlendMode, inputPort.AvatarMask, false);
            });
        }

        private Port AddLayer(string name = null, float weight = 1f, LayerBlendMode blendMode = default, AvatarMask avatarMask = null, bool refresh = true)
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = string.Empty;
            inputPort.portColor = _portColor;

            TextField nameField = new TextField() { value = name };
            nameField.style.minWidth = 40;
            inputPort.Add(nameField);

            FloatField weightField = new FloatField() { value = weight };
            weightField.style.minWidth = 40;
            inputPort.Add(weightField);

            EnumField blendModeField = new EnumField(blendMode);
            inputPort.Add(blendModeField);

            ObjectField maskField = new ObjectField()
            {
                objectType = typeof(AvatarMask),
                value = avatarMask
            };

            inputPort.Add(maskField);

            MakePortRemovable(inputPort, index => _portMap.RemoveAt(index));

            inputContainer.Add(inputPort);

            _portMap.Add((inputPort, nameField, weightField, blendModeField, maskField));

            if (refresh)
            {
                RefreshExpandedState();
                RefreshPorts();
            }

            return inputPort;
        }

        public override NodeAsset GenerateData() => new NodeAsset { Data = new LayerMixerNode { Name = _nameField.value, Speed = _speedField.value } };

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            _portMap.ForEach(p =>
            {
                NodeAsset outputNodeAsset = p.inputPort.connections.Count() > 0 ? nodeMap[p.inputPort.connections.First().output.node] : null;
                nodeAsset.InputPorts.Add(new LayerMixerNodeInputPortAsset { SourceNodeAsset = outputNodeAsset, Name = p.name.value, Weight = p.weight.value, BlendMode = (LayerBlendMode)p.blendMode.value, AvatarMask = (AvatarMask)p.avatarMask.value });
            });
        }
    }
}
