using GZ.AnimationGraph;
using GZ.AnimationGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateMachineNodeUI : BaseNodeUI
    {
        public StateMachineNodeAsset StateMachineNodeAsset { get; set; }

        public List<Port> StatePorts = new List<Port>();

        protected override string DefaultName => "State Machine";

        private static Color _portColor = new Color(180 / 255f, 40 / 255f, 255 / 255f);

        public StateMachineNodeUI() : base()
        {
            StateMachineNodeAsset = new StateMachineNodeAsset { Data = new StateMachineNode() };

            Button openEditorButton = new Button(() =>
            {
                StateMachineEditor.OpenEditor();
                StateMachineEditor.Editor.LoadStateMachine(this);
            }) { text = "Edit" };

            mainContainer.Insert(1, openEditorButton);

            GenerateOutputPort(_portColor);
        }

        public void UpdateStatePorts()
        {
            var stateMachine = (StateMachineNode)StateMachineNodeAsset.Data;

            for (int i = 0; i < stateMachine.States.Count; i++)
            {
                if (i < StatePorts.Count)
                {
                    StatePorts[i].portName = stateMachine.States.At(i).Name;
                }
                else
                {
                    Port port = GenerateStatePort(stateMachine.States.At(i).Name);
                }
            }

            for (int i = stateMachine.States.Count; i < StatePorts.Count; i++)
            {
                inputContainer.RemoveAt(i);
                StatePorts.RemoveAt(i);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port GenerateStatePort(string stateName)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            port.portColor = _portColor;
            port.portName = stateName;
            StatePorts.Add(port);
            inputContainer.Add(port);

            return port;
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            StateMachineNodeAsset = (StateMachineNodeAsset)nodeAsset;

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset => GenerateStatePort(((StateMachineNodeInputPortAsset)portAsset).StateName));
        }

        public override NodeAsset GenerateData()
        {
            StateMachineNodeAsset.Data.Name = NameField.value;
            StateMachineNodeAsset.Data.Speed = _speedField.value;

            return StateMachineNodeAsset;
        }

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap)
        {
            nodeAsset.InputPorts.Clear();

            StatePorts.ForEach(p =>
            {
                NodeAsset outputNodeAsset = p.connections.Count() > 0 ? nodeMap[p.connections.First().output.node] : null;
                nodeAsset.InputPorts.Add(new StateMachineNodeInputPortAsset { SourceNodeAsset = outputNodeAsset, Weight = 0f, StateName = p.portName });
            });
        }
    }
}
