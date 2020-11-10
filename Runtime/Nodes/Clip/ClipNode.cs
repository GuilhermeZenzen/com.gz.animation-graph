using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class ClipNode : BaseNode
    {
        [SerializeField] private AnimationClip _clip;
        public AnimationClip Clip
        {
            get => _clip;
            set
            {
                _clip = value;

                if (!Playable.IsNull())
                {
                    CreatePlayable(Playable.GetGraph());

                    foreach (var outputPort in OutputPorts)
                    {
                        outputPort.Link.InputPort.Node.UpdateConnection(outputPort.Link);
                    }
                }
            }
        }

        public ClipNode() { }
        public ClipNode(AnimationClip clip) => _clip = clip;

        public override BaseNode Copy() => new ClipNode(_clip) { Name = this.Name, Speed = Speed };

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationClipPlayable.Create(playableGraph, _clip);

        public override (float rawDuration, float duration) CalculateDuration()
        {
            return _clip != null ? (_clip.length, _clip.length) : (float.PositiveInfinity, float.PositiveInfinity);
        }
    }
}
