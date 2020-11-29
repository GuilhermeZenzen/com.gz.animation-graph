using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class ClipNodeUI : BaseNodeUI
    {
        private ObjectField _clipField;

        private static readonly Color _portColor = new Color(3 / 255f, 252 / 255f, 173 / 255f);

        protected override string DefaultName => "Clip";

        public ClipNodeUI() : base()
        {
            _clipField = new ObjectField();
            _clipField.objectType = typeof(AnimationClip);

            extensionContainer.Add(_clipField);

            GenerateOutputPort(_portColor);
        }

        public override void LoadData(AnimationGraphView graphView, NodeAsset nodeAsset, Dictionary<NodeAsset, BaseNodeUI> nodeMap)
        {
            base.LoadData(graphView, nodeAsset, nodeMap);

            _clipField.SetValueWithoutNotify(((ClipNode)nodeAsset.Data).Clip);
        }

        public override NodeAsset GenerateData() => new NodeAsset { Data = new ClipNode((AnimationClip)_clipField.value) { Name = NameField.value, Speed = _speedField.value } };

        public override void GenerateLinkData(NodeAsset nodeAsset, Dictionary<Node, NodeAsset> nodeMap) { }
    }
}
