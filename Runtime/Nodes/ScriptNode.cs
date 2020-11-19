using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [System.Serializable]
    public class ScriptNode : BaseNode
    {
        [field: SerializeReference] public IScriptNodeJob Job { get; private set; }

        public ScriptNode() { }
        public ScriptNode(IScriptNodeJob job) => Job = job;

        public void SetJob<T>(T job) where T: struct, IScriptNodeJob
        {
            Job = job;

            if (!Playable.IsNull())
            {
                ((AnimationScriptPlayable)Playable).SetJobData(job);
            }
        }
        public void SetJob<T>() where T: struct, IScriptNodeJob
        {
            if (!Playable.IsNull())
            {
                ((AnimationScriptPlayable)Playable).SetJobData((T)Job);
            }
        }

        public override BaseNode Copy() => new ScriptNode { Job = Job };

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => Playable.Null;

        public Playable CreateScriptPlayable<T>(PlayableGraph playableGraph) where T: struct, IScriptNodeJob
        {
            if (Playable.IsNull())
            {
                Playable = AnimationScriptPlayable.Create(playableGraph, Job != null ? (T)Job : default);
                Playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
                Playable.SetOutputCount(0);
                Playable.SetSpeed(Speed);
                UpdateDuration();
            }

            return Playable;
        }
    }

    [System.Serializable]
    public class ScriptNode<T> : BaseNode where T: struct, IScriptNodeJob
    {
        [SerializeReference] private T _job;
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

        public ScriptNode() { }
        public ScriptNode(T job) => _job = job;

        public override BaseNode Copy() => new ScriptNode<T>(_job);

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationScriptPlayable.Create(playableGraph, _job);
    }
}
