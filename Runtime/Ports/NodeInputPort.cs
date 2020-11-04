using GZ.AnimationGraph;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class NodeInputPort
    {
        [SerializeReference] public BaseNode Node;
        [SerializeReference] public NodeLink Link;

        [SerializeField] private float _weight = 1f;
        public float Weight
        {
            get => _weight;
            set
            {
                _weight = value;

                if (Node != null && !Node.Playable.IsNull() && Index > -1)
                {
                    Node.Playable.SetInputWeight(Index, _weight);
                }
            }
        }

        public int Index = -1;
    }
}
