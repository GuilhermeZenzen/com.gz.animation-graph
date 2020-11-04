using GZ.AnimationGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Animation Graph", menuName = "GZ/Animation Graph Asset")]
public class AnimationGraphAsset : ScriptableObject
{
    [SerializeReference] public NodeAsset OutputNode;
    [SerializeReference] public NodeAsset OutputIndicatorNode;

    [SerializeReference] public List<NodeAsset> Nodes = new List<NodeAsset>();
}
