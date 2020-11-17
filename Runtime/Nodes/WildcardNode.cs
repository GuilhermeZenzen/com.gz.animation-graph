using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    public class WildcardNode : BaseNode
    {
        public override BaseNode Copy() => new WildcardNode { Name = Name, Speed = Speed };

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => Playable.Create(playableGraph);
    }
}
