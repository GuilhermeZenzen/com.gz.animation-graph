using GZ.AnimationGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationGraphOverrider))]
public class AnimationGraphOverriderInspector : Editor
{
    public OverrideDisplayList OverrideDisplays = new OverrideDisplayList();

    public bool IsShowingOverrides = true;

    [System.Serializable]
    public class OverrideDisplay
    {
        public string ID;
        public string NodeName;
        public AnimationClip SourceClip;
    }

    [System.Serializable]
    public class OverrideDisplayList : List<OverrideDisplay> { }

    private void OnEnable()
    {
        var overrider = (AnimationGraphOverrider)target;
        overrider.Overrides.LoadDictionary();

        UpdateOverrides(overrider);
    }

    public override void OnInspectorGUI()
    {
        var overrider = (AnimationGraphOverrider)target;

        var previousSourceGraph = overrider.SourceGraph;

        overrider.SourceGraph = (AnimationGraphAsset)EditorGUILayout.ObjectField(new GUIContent("Source Graph"), overrider.SourceGraph, typeof(AnimationGraphAsset), false);

        if (previousSourceGraph != overrider.SourceGraph)
        {
            UpdateOverrides(overrider);
        }

        overrider.TargetGraph = (AnimationGraphAsset)EditorGUILayout.ObjectField(new GUIContent("Target Graph"), overrider.TargetGraph, typeof(AnimationGraphAsset), false);

        EditorGUILayout.Space(15);

        IsShowingOverrides = EditorGUILayout.BeginFoldoutHeaderGroup(IsShowingOverrides, "Overrides");

        if (IsShowingOverrides)
        {
            OverrideDisplays.ForEach(display =>
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(display.NodeName);

                GUI.enabled = false;
                EditorGUILayout.ObjectField(GUIContent.none, display.SourceClip, typeof(AnimationClip), false, GUILayout.MaxWidth(100));
                GUI.enabled = true;

                overrider.Overrides[display.ID] = (AnimationClip)EditorGUILayout.ObjectField(GUIContent.none, overrider.Overrides[display.ID], typeof(AnimationClip), false, GUILayout.MaxWidth(100));

                EditorGUILayout.EndHorizontal();
            });
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (overrider.SourceGraph != null && overrider.TargetGraph != null && GUILayout.Button(new GUIContent("Generate Override")))
        {
            var transferAsset = Instantiate(overrider.SourceGraph) as AnimationGraphAsset;

            overrider.TargetGraph.Nodes = transferAsset.Nodes;
            overrider.TargetGraph.OutputIndicatorNode = transferAsset.OutputIndicatorNode;
            overrider.TargetGraph.OutputNode = transferAsset.OutputNode;

            overrider.TargetGraph.Nodes.ForEach(node =>
            {
                if (!(node.Data is ClipNode clipNode)) { return; }

                if (overrider.Overrides[node.ID] != null)
                {
                    clipNode.Clip = overrider.Overrides[node.ID];
                }
            });

            EditorUtility.SetDirty(overrider.TargetGraph);
        }

        EditorUtility.SetDirty(target);
    }

    private void UpdateOverrides(AnimationGraphOverrider overrider)
    {
        AnimationGraphAsset sourceGraph = overrider.SourceGraph;

        OverrideDisplays.Clear();

        if (sourceGraph == null) { return; }

        HashSet<string> nodesToRemove = new HashSet<string>(overrider.Overrides.Keys);

        foreach (var nodeAsset in sourceGraph.Nodes)
        {
            if (!(nodeAsset.Data is ClipNode clipNode)) { continue; }

            if (nodesToRemove.Contains(nodeAsset.ID))
            {
                nodesToRemove.Remove(nodeAsset.ID);
                OverrideDisplays.Add(new OverrideDisplay { ID = nodeAsset.ID, NodeName = clipNode.Name, SourceClip = clipNode.Clip });
            }
            else
            {
                overrider.Overrides.Add(nodeAsset.ID, null);
                OverrideDisplays.Add(new OverrideDisplay { ID = nodeAsset.ID, NodeName = clipNode.Name, SourceClip = clipNode.Clip });
            }
        }

        foreach (var nodeToRemove in nodesToRemove)
        {
            overrider.Overrides.Remove(nodeToRemove);
        }
    }
}
