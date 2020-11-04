using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [RequireComponent(typeof(Animator))]
    public class AnimationGraph : MonoBehaviour
    {
        public PlayableGraph PlayableGraph { get; private set; }
        public AnimationPlayableOutput PlayableOutput { get; private set; }

        [field: SerializeReference] public MixerNode RootNode { get; private set; }

        [field: SerializeReference] public Dictionary<string, BaseNode> Nodes { get; private set; } = new Dictionary<string, BaseNode>();
        [field: SerializeReference] public List<IUpdatableNode> UpdatableNodes { get; private set; } = new List<IUpdatableNode>();

        public BaseNode this[string nodeName]
        {
            get
            {
                Assert.IsTrue(Nodes.ContainsKey(nodeName), $"The graph doesn't contain a node with the name {nodeName}");
                return Nodes[nodeName];
            }
            set
            {
                Assert.IsTrue(Nodes.ContainsKey(nodeName), $"The graph doesn't contain a node with the name {nodeName}");
                Nodes[nodeName] = value;
            }
        }

        private void Awake()
        {
            PlayableGraph = PlayableGraph.Create("Animation Graph");

            RootNode = new MixerNode();
            RootNode.CreatePlayable(PlayableGraph);

            PlayableOutput = AnimationPlayableOutput.Create(PlayableGraph, "Animation Output", GetComponent<Animator>());
            PlayableOutput.SetSourcePlayable(RootNode.Playable);
            PlayableOutput.SetAnimationStreamSource(AnimationStreamSource.DefaultValues);

            PlayableGraph.Play();
        }

        private void OnDestroy()
        {
            PlayableGraph.Destroy();
        }

        private void LateUpdate()
        {
            UpdatableNodes.ForEach(n => n.Update(Time.deltaTime));
        }

        public void LoadGraph(AnimationGraphAsset asset)
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

        public T AddNode<T>(T node, string name = null) where T: BaseNode
        {
            node.CreatePlayable(PlayableGraph);
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

        public LayerMixerNode LayerMixer(string nodeName)
        {
            Assert.IsTrue(Nodes.ContainsKey(nodeName), $"The graph doesn't contain a node with the name {nodeName}");
            Assert.IsTrue(Nodes[nodeName] is LayerMixerNode, $"The node {nodeName} isn't a Layer Mixer Node");

            return (LayerMixerNode)Nodes[nodeName];
        }

        public StateMachineNode StateMachine(string nodeName)
        {
            Assert.IsTrue(Nodes.ContainsKey(nodeName), $"The graph doesn't contain a node with the name {nodeName}");
            Assert.IsTrue(Nodes[nodeName] is StateMachineNode, $"The node {nodeName} isn't a State Machine Node");

            return (StateMachineNode)Nodes[nodeName];
        }

        public Blendspace2DNode Blend2D(string nodeName)
        {
            Assert.IsTrue(Nodes.ContainsKey(nodeName), $"The graph doesn't contain a node with the name {nodeName}");
            Assert.IsTrue(Nodes[nodeName] is Blendspace2DNode, $"The node {nodeName} isn't a 2d Blendspace Node");

            return (Blendspace2DNode)Nodes[nodeName];
        }

        public string ValidateNodeName(string name)
        {
            return NameValidation.ValidateName(name, validationName => !Nodes.ContainsKey(validationName));
        }

        public void RemoveNode(BaseNode node)
        {
            PlayableGraph.DestroyPlayable(node.Playable);
            Nodes.Remove(node.Name);
        }
    }
}
