using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    public class AnimationGraphNode : BaseNode, IUpdatableNode
    {
        public AnimationGraphBehaviour Behaviour { get; }

        [field: SerializeReference] public Dictionary<string, BaseNode> Nodes { get; private set; } = new Dictionary<string, BaseNode>();
        [field: SerializeReference] public List<IUpdatableNode> UpdatableNodes { get; private set; } = new List<IUpdatableNode>();

        public BaseNode OutputNode { get; private set; }

        public event Action<BaseNode> OnNodeAdded;
        public event Action<NodeLink> OnNodeConnected;
        public event Action<NodeLink> OnNodeDisconnected;
        public event Action<BaseNode> OnNodeRemoved;

        public BaseNode this[string key] => Nodes.TryGetValue(key, out BaseNode node) ? node : null;

        public AnimationGraphNode(AnimationGraphBehaviour behaviour)
        {
            Behaviour = behaviour;
        }

        #region Lifecycle

        public void Update(float deltaTime)
        {
            UpdatableNodes.ForEach(node => node.Update(deltaTime));
        }

        public override BaseNode Copy()
        {
            return new AnimationGraphNode(Behaviour);
        }

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationMixerPlayable.Create(Behaviour.PlayableGraph, 1);

        #endregion

        #region I/O

        public T AddNode<T>(T node, string name = null) where T : BaseNode
        {
            Assert.IsFalse(node is ScriptNode, $"Can't add node {name} because it's of type ScriptNode. Use AddScriptNode instead.");

            node.CreatePlayable(Behaviour.PlayableGraph);
            node.Playable.SetPropagateSetTime(true);

            SetAddedNodeName(node, name);
            node.Graph = this;
            Nodes.Add(node.Name, node);

            if (node is IUpdatableNode updatableNode)
            {
                UpdatableNodes.Add(updatableNode);
            }

            OnNodeAdded?.Invoke(node);

            return node;
        }

        public ScriptNode AddScriptNode<T>(IScriptNodeJob job, string name) where T : struct, IScriptNodeJob => AddScriptNode<T>(new ScriptNode(job), name);
        public ScriptNode AddScriptNode<T>(ScriptNode node, string name = null) where T: struct, IScriptNodeJob
        {
            node.CreateScriptPlayable<T>(Behaviour.PlayableGraph);
            node.Playable.SetPropagateSetTime(true);

            SetAddedNodeName(node, name);
            node.Graph = this;
            Nodes.Add(node.Name, node);

            OnNodeAdded?.Invoke(node);

            return node;
        }

        public T ReplaceNode<T>(string wildcardName) where T : BaseNode, new() => ReplaceNode(wildcardName, new T());
        public T ReplaceNode<T>(string wildcardName, T node) where T : BaseNode => ReplaceNode((WildcardNode)Nodes[wildcardName], node);
        public T ReplaceNode<T>(WildcardNode wildcard, T node) where T: BaseNode
        {
            Assert.IsFalse(node is ScriptNode, $"Can't replace node {wildcard.Name} because it's of type ScriptNode. Use ReplaceScriptNode instead.");

            Nodes.Remove(wildcard.Name);

            AddNode(node, wildcard.Name);
            node.Speed = wildcard.Speed;

            for (int i = 0; i < wildcard.InputPorts.Count; i++)
            {
                var inputPort = wildcard.InputPorts[i];

                wildcard.Disconnect(inputPort.Link);
                var newInputPort = node.CreateBaseInputPort(inputPort.Weight);
                node.Connect(newInputPort, inputPort.Link.OutputPort);
            }

            for (int i = 0; i < wildcard.OutputPorts.Count; i++)
            {
                var outputPort = wildcard.OutputPorts[i];

                outputPort.Link.InputPort.Node.Disconnect(outputPort.Link);
                outputPort.Link.InputPort.Node.Connect(outputPort.Link.InputPort, node);
            }

            Behaviour.PlayableGraph.DestroyPlayable(wildcard.Playable);

            return node;
        }

        public ScriptNode ReplaceScriptNode<T>(string wildcardName, T job) where T: struct, IScriptNodeJob
        {
            return ReplaceScriptNode<T>(wildcardName, new ScriptNode(job));
        }
        public ScriptNode ReplaceScriptNode<T>(string wildcardName, ScriptNode scriptNode) where T : struct, IScriptNodeJob
        {
            return ReplaceScriptNode<T>((WildcardNode)Nodes[wildcardName], scriptNode);
        }
        public ScriptNode ReplaceScriptNode<T>(WildcardNode wildcard, ScriptNode scriptNode) where T : struct, IScriptNodeJob
        {
            Nodes.Remove(wildcard.Name);

            AddScriptNode<T>(scriptNode, wildcard.Name);
            scriptNode.Speed = wildcard.Speed;

            for (int i = 0; i < wildcard.InputPorts.Count; i++)
            {
                var inputPort = wildcard.InputPorts[i];
                NodeLink link = inputPort.Link;

                wildcard.Disconnect(link);
                var newInputPort = scriptNode.CreateBaseInputPort(inputPort.Weight);
                scriptNode.Connect(newInputPort, link.OutputPort);
            }

            for (int i = 0; i < wildcard.OutputPorts.Count; i++)
            {
                var outputPort = wildcard.OutputPorts[i];
                NodeLink link = outputPort.Link;

                link.InputPort.Node.Disconnect(link);
                link.InputPort.Node.Connect(link.InputPort, scriptNode);
            }

            Behaviour.PlayableGraph.DestroyPlayable(wildcard.Playable);

            return scriptNode;
        }

        public bool RemoveNode(string nodeName)
        {
            if (!Nodes.TryGetValue(nodeName, out BaseNode node)) { return false; }

            Nodes.Remove(nodeName);
            Behaviour.PlayableGraph.DestroyPlayable(node.Playable);

            if (node is IUpdatableNode updatableNode)
            {
                UpdatableNodes.Remove(updatableNode);
            }

            OnNodeRemoved?.Invoke(node);

            return true;
        }

        private void SetAddedNodeName(BaseNode node, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(node.Name))
                {
                    name = "Node";
                }
                else
                {
                    name = node.Name;
                }
            }

            node.Name = ValidateNodeName(name);
        }

        public void Clear()
        {
            foreach (var node in Nodes.Values)
            {
                node.Playable.Destroy();
            }

            Nodes.Clear();
            UpdatableNodes.Clear();
            OutputNode = null;
        }

        #endregion

        public void SetOutput(BaseNode node)
        {
            OutputNode = node;

            if (node.Playable.GetOutputCount() == 0)
            {
                node.Playable.SetOutputCount(1);
            }

            if (Playable.GetInputCount() > 0)
            {
                if (!Playable.GetInput(0).IsNull())
                {
                    Playable.DisconnectInput(0);
                }

                Playable.ConnectInput(0, node.Playable, 0, 1f);
            }
            else
            {
                Playable.AddInput(node.Playable, 0, 1f);
            }
        }

        #region Connection

        public void ConnectedNode(NodeLink link) => OnNodeConnected?.Invoke(link);

        public void DisconnectedNode(NodeLink link) => OnNodeDisconnected?.Invoke(link);

        #endregion Connection

        #region Asset

        public void LoadAsset(AnimationGraphAsset asset)
        {
            Clear();

            Dictionary<NodeAsset, BaseNode> nodeMap = new Dictionary<NodeAsset, BaseNode>();

            asset.Nodes.ForEach(n =>
            {
                nodeMap.Add(n, AddNode(n.Data.Copy()));
            });

            foreach (var entry in nodeMap)
            {
                entry.Key.InputPorts.ForEach(p =>
                {
                    if (p.SourceNodeAsset != null)
                    {
                        entry.Value.Connect(p.CreatePort(entry.Value), nodeMap[p.SourceNodeAsset]);
                    }
                });
            }

            if (asset.OutputNode != null)
            {
                SetOutput(nodeMap[asset.OutputNode]);
            }
        }

        public void AppendAsset(AnimationGraphAsset asset)
        {
            Dictionary<NodeAsset, BaseNode> nodeMap = new Dictionary<NodeAsset, BaseNode>();

            asset.Nodes.ForEach(n =>
            {
                nodeMap.Add(n, AddNode(n.Data.Copy()));
            });

            foreach (var entry in nodeMap)
            {
                entry.Key.InputPorts.ForEach(p =>
                {
                    if (p.SourceNodeAsset != null)
                    {
                        entry.Value.Connect(p.CreatePort(entry.Value), nodeMap[p.SourceNodeAsset]);
                    }
                });
            }
        }

        #endregion Asset

        #region Access

        public ClipNode Clip(string nodeName) => this[nodeName] as ClipNode;

        public Blendspace1DNode Blend1D(string nodeName) => this[nodeName] as Blendspace1DNode;

        public Blendspace2DNode Blend2D(string nodeName) => this[nodeName] as Blendspace2DNode;

        public MixerNode Mixer(string nodeName) => this[nodeName] as MixerNode;

        public LayerMixerNode LayerMixer(string nodeName) => this[nodeName] as LayerMixerNode;

        public StateMachineNode StateMachine(string nodeName) => this[nodeName] as StateMachineNode;

        public AnimationGraphNode AnimationGraph(string nodeName) => this[nodeName] as AnimationGraphNode;

        public ScriptNode Script(string nodeName) => this[nodeName] as ScriptNode;
        //public ScriptNode<T> Script<T>(string nodeName) where T: struct, IScriptNodeJob => this[nodeName] as ScriptNode<T>;

        #endregion Access

        #region Utility

        public string ValidateNodeName(string name) => NameValidation.ValidateName(name, validationName => !Nodes.ContainsKey(validationName));

        #endregion Utility
    }
}
