using GZ.AnimationGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class LayerMixerNode : BaseNode
    {
        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationLayerMixerPlayable.Create(playableGraph);

        public LayerMixerNodeInputPort CreateInputPort(LayerBlendMode blendMode, float weight = 1f, AvatarMask avatarMask = null)
        {
            LayerMixerNodeInputPort port = new LayerMixerNodeInputPort { Node = this, Weight = weight, BlendMode = blendMode, AvatarMask = avatarMask };
            SetupInputPort(port);
            
            SetBlendMode(port, blendMode);
            SetAvatarMask(port, avatarMask);

            return port;
        }

        public override NodeInputPort OnCreateBaseInputPort(float weight) => new LayerMixerNodeInputPort() { Weight = weight };

        public override void DestroyInputPort(int inputPortIndex)
        {
            DestroyInputPortWithCallback(inputPortIndex, p =>
            {
                LayerMixerNodeInputPort port = (LayerMixerNodeInputPort)p;
                SetBlendMode(port.Index, port.BlendMode);
                SetAvatarMask(port, port.AvatarMask);
            });
        }

        public NodeLink Connect(BaseNode sourceNode, LayerBlendMode blendMode, float weight = 1f, AvatarMask avatarMask = null) => Connect(sourceNode.CreateOutputPort(), blendMode, weight, avatarMask);
        public NodeLink Connect(NodeOutputPort outputPort, LayerBlendMode blendMode, float weight = 1f, AvatarMask mask = null) => Connect(CreateInputPort(blendMode, weight, mask), outputPort);

        public void SetBlendMode(LayerMixerNodeInputPort inputPort, LayerBlendMode blendMode)
        {
            inputPort.BlendMode = blendMode;
            SetBlendMode(inputPort.Index, blendMode);
        }
        public void SetBlendMode(int inputPortIndex, LayerBlendMode blendMode)
        {
            ((AnimationLayerMixerPlayable)Playable).SetLayerAdditive((uint)inputPortIndex, blendMode == LayerBlendMode.Additive);
        }

        public void SetAvatarMask(LayerMixerNodeInputPort inputPort, AvatarMask mask)
        {
            if (inputPort.AvatarMask == null && mask == null) { return; }

            inputPort.AvatarMask = mask;
            ((AnimationLayerMixerPlayable)Playable).SetLayerMaskFromAvatarMask((uint)inputPort.Index, mask);
        }

        public override BaseNode Copy() => new LayerMixerNode() { Name = this.Name, Speed = Speed };
    }
}
