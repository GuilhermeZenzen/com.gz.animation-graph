using System;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class MixerNode : BaseNode
    {
        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationMixerPlayable.Create(playableGraph);

        public NodeInputPort CreateInputPort(float weight = 1f)
        {
            NodeInputPort port = new NodeInputPort { Node = this, Index = InputPorts.Count, Weight = weight };
            Playable.SetInputCount(Playable.GetInputCount() + 1);
            Playable.SetInputWeight(port.Index, weight);

            InputPorts.Add(port);

            return port;
        }

        public override BaseNode Copy() => new MixerNode() { Name = this.Name, Speed = Speed };
    }
}
