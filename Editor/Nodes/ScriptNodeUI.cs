using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using System;
using System.Linq;

namespace GZ.AnimationGraph.Editor
{
    public class ScriptNodeUI : BaseNodeUI
    {
        private PopupField<Type> JobTypeField;

        public Port InputPort { get; private set; }

        protected override string DefaultName => "Script";

        public ScriptNodeUI() : base()
        {
            JobTypeField = AnimationGraphEditor.Editor.ScriptNodeJobs.Count > 0 ? new PopupField<Type>(AnimationGraphEditor.Editor.ScriptNodeJobs, 0,
                type => type.Name, type => type.Name)
                : new PopupField<Type>();

            extensionContainer.Add(JobTypeField);

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "";
            inputContainer.Add(InputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            if (!(nodeAsset.Data is ScriptNode))
            {
                JobTypeField.SetValueWithoutNotify(nodeAsset.Data.GetType().GetGenericArguments()[0]);
            }

            base.LoadData(graphView, nodeAsset, nodeMap);

            LoadDataWithCallback(graphView, nodeAsset, nodeMap, portAsset => InputPort);
        }

        public override NodeAsset GenerateData()
        {
            BaseNode data = AnimationGraphEditor.Editor.ScriptNodeJobs.Count > 0 ? (BaseNode)Activator.CreateInstance(typeof(ScriptNode<>).MakeGenericType(JobTypeField.value)) : new ScriptNode();

            if (data != null)
            {
                data.Name = _nameField.value;
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
