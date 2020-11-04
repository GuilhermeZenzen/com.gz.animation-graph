using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    public class AnimationGraphNode : BaseNode, IUpdatableNode
    {
        public AnimationGraphBehaviour Behaviour { get; }

        [field: SerializeReference] public Dictionary<string, BaseNode> Nodes { get; private set; } = new Dictionary<string, BaseNode>();
        [field: SerializeReference] public List<IUpdatableNode> UpdatableNodes { get; private set; } = new List<IUpdatableNode>();

        public BaseNode OutputNode { get; private set; }

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

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => Playable.Create(Behaviour.PlayableGraph, 1);

        #endregion

        #region I/O

        public T AddNode<T>(T node, string name = null) where T : BaseNode
        {
            node.CreatePlayable(Behaviour.PlayableGraph);
            node.Playable.SetPropagateSetTime(true);

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
            Nodes.Add(node.Name, node);

            if (node is IUpdatableNode updatableNode)
            {
                UpdatableNodes.Add(updatableNode);
            }

            return node;
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

            return true;
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
                if (!Playable.GetInput(0).Equals(Playable.Null))
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

        #endregion Access

        #region Utility

        public string ValidateNodeName(string name) => NameValidation.ValidateName(name, validationName => !Nodes.ContainsKey(validationName));

        #endregion Utility
    }
}
