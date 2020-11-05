using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    public class AnimationGraphBehaviour : MonoBehaviour
    {
        public AnimationGraphNode Graph { get; private set; }

        public PlayableGraph PlayableGraph { get; private set; }
        public AnimationPlayableOutput PlayableOutput { get; private set; }

        public BaseNode this[string key] => Graph[key];

        #region Lifecycle

        private void Awake()
        {
            Graph = new AnimationGraphNode(this);

            PlayableGraph = PlayableGraph.Create("Animation Graph");

            Graph.CreatePlayable(PlayableGraph);

            PlayableOutput = AnimationPlayableOutput.Create(PlayableGraph, "Animation Output", GetComponent<Animator>());
            PlayableOutput.SetSourcePlayable(Graph.Playable);
            PlayableOutput.SetAnimationStreamSource(AnimationStreamSource.DefaultValues);

            PlayableGraph.Play();
        }

        private void LateUpdate()
        {
            Graph.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            PlayableGraph.Destroy();
        }

        #endregion Lifecycle

        #region I/O

        public T AddNode<T>(T node, string name = null) where T : BaseNode => Graph.AddNode(node, name);

        public bool RemoveNode(string nodeName) => Graph.RemoveNode(nodeName);

        public void Clear() => Graph.Clear();

        #endregion

        public void SetOutput(BaseNode node) => Graph.SetOutput(node);

        #region Asset

        public void LoadAsset(AnimationGraphAsset asset) => Graph.LoadAsset(asset);

        public void AppendAsset(AnimationGraphAsset asset) => Graph.AppendAsset(asset);

        #endregion

        #region Access

        public ClipNode Clip(string nodeName) => Graph.Clip(nodeName);

        public Blendspace1DNode Blend1D(string nodeName) => Graph.Blend1D(nodeName);

        public Blendspace2DNode Blend2D(string nodeName) => Graph.Blend2D(nodeName);

        public MixerNode Mixer(string nodeName) => Graph.Mixer(nodeName);

        public LayerMixerNode LayerMixer(string nodeName) => Graph.LayerMixer(nodeName);

        public StateMachineNode StateMachine(string nodeName) => Graph.StateMachine(nodeName);

        #endregion
    }
}
