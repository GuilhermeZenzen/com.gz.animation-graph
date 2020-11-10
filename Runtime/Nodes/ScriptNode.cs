using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    public class ScriptNode<T> : BaseNode where T: struct, IAnimationJob
    {
        private T _job;
        public T Job
        {
            get => _job;
            set
            {
                _job = value;

                if (!Playable.IsNull())
                {
                    ((AnimationScriptPlayable)Playable).SetJobData(_job);
                }
            }
        }

        public ScriptNode(T job) => _job = job;

        public override BaseNode Copy() => new ScriptNode<T>(_job);

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationScriptPlayable.Create(playableGraph, _job);
    }
}
