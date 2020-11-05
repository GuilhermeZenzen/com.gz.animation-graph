using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animation Graph Overrider", menuName = "GZ/Animation/Animation Graph Overrider")]
public class AnimationGraphOverrider : ScriptableObject
{
    public AnimationGraphAsset SourceGraph;
    public AnimationGraphAsset TargetGraph;

    public OverrideList Overrides = new OverrideList();

    [System.Serializable]
    public class OverrideList : IndexedDictionary<string, AnimationClip> { }
}
