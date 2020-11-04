using ICSharpCode.NRefactory.Ast;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnimationGraphEditor : GraphViewEditorWindow, ISearchWindowProvider
    {
        public static AnimationGraphEditor Editor;

        private Toolbar _toolbar;
        private ToolbarButton _closeButton;
        private Label _assetName;
        private ToolbarButton _saveButton;
        private ToolbarSpacer _closeToAssetNameSpacer;
        private ToolbarSpacer _assetNameToSaveSpacer;

        public AnimationGraphView GraphView { get; private set; }

        public AnimationGraphAsset AnimationGraphAsset;

        private const string _ussPath = "AnimationGraph.uss";
        private static StyleSheet _styleSheet;

        [MenuItem("GZ/Animation Graph")]
        public static void ShowEditor()
        {
            Editor = GetWindow<AnimationGraphEditor>();
            Editor.titleContent = new GUIContent("Animation Graph");
        }

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is AnimationGraphAsset animationGraphAsset)
            {
                if (Editor != null)
                {
                    if (animationGraphAsset != Editor.AnimationGraphAsset)
                    {
                        Editor.OpenAnimationGraphAsset(animationGraphAsset);
                    }
                    else { return false; }
                }
                else
                {
                    ShowEditor();
                    Editor.OpenAnimationGraphAsset(animationGraphAsset);
                }

                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            string resourcesPath = $"{AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)).Replace($"/{nameof(AnimationGraphEditor)}.cs", "")}/Resources";
            _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{resourcesPath}/{_ussPath}");

            _toolbar = new Toolbar();

            _closeButton = new ToolbarButton(CloseAnimationGraph) { text = "Close" };
            _closeButton.style.display = DisplayStyle.None;

            _assetName = new Label();
            _assetName.style.unityTextAlign = TextAnchor.MiddleLeft;
            _assetName.style.display = DisplayStyle.None;

            _saveButton = new ToolbarButton(SaveAnimationGraph) { text = "Save" };

            _closeToAssetNameSpacer = new ToolbarSpacer();
            _closeToAssetNameSpacer.style.display = DisplayStyle.None;

            _assetNameToSaveSpacer = new ToolbarSpacer();
            _assetNameToSaveSpacer.style.display = DisplayStyle.None;

            _toolbar.Add(_closeButton);
            _toolbar.Add(_closeToAssetNameSpacer);
            _toolbar.Add(_assetName);
            _toolbar.Add(_assetNameToSaveSpacer);
            _toolbar.Add(_saveButton);
            
            CreateGraphView();

            StateMachineEditor.Editor?.Close();

            rootVisualElement.Add(_toolbar);
        }

        private void OnDisable()
        {
            Editor = null;

            StateMachineEditor.Editor?.Close();

            rootVisualElement.Remove(GraphView);

            _styleSheet = null;
        }

        private void CreateGraphView()
        {
            GraphView = new AnimationGraphView
            {
                name = "Animation Graph"
            };
            GraphView.styleSheets.Add(_styleSheet);

            GraphView.StretchToParentSize();

            rootVisualElement.Insert(0, GraphView);

            StateMachineEditor.Editor?.Close();
        }

        private void OpenAnimationGraphAsset(AnimationGraphAsset asset)
        {
            AnimationGraphAsset = asset;

            _closeButton.style.display = DisplayStyle.Flex;

            _assetName.style.display = DisplayStyle.Flex;
            _assetName.text = asset.name;

            _closeToAssetNameSpacer.style.display = DisplayStyle.Flex;
            _assetNameToSaveSpacer.style.display = DisplayStyle.Flex;

            rootVisualElement.Remove(GraphView);
            CreateGraphView();

            Dictionary<NodeAsset, BaseNodeUI> nodeMap = new Dictionary<NodeAsset, BaseNodeUI>();

            asset.Nodes.ForEach(n =>
            {
                BaseNodeUI nodeUI = null;

                switch (n.Data)
                {
                    case ClipNode clipNode:
                        nodeUI = new ClipNodeUI();
                        break;
                    case MixerNode mixerNode:
                        nodeUI = new MixerNodeUI();
                        break;
                    case LayerMixerNode layerMixerNode:
                        nodeUI = new LayerMixerNodeUI();
                        break;
                    case Blendspace1DNode blendspace1DNode:
                        nodeUI = new Blendspace1DNodeUI();
                        break;
                    case Blendspace2DNode blendspace2DNode:
                        nodeUI = new Blendspace2DNodeUI();
                        break;
                    case StateMachineNode stateMachineNode:
                        nodeUI = new StateMachineNodeUI();
                        break;
                    default:
                        break;
                }

                if (nodeUI != null)
                {
                    GraphView.AddNode(nodeUI);
                    nodeUI.SetPosition(new Rect(n.Position, Vector2.zero));
                    nodeUI.expanded = n.IsExpanded;
                    nodeMap.Add(n, nodeUI);
                }
            });

            foreach (var entry in nodeMap)
            {
                entry.Value.LoadData(GraphView, entry.Key, nodeMap);
            }
        }

        private void CloseAnimationGraph()
        {
            AnimationGraphAsset = null;
            _closeButton.style.display = DisplayStyle.None;
            _assetName.style.display = DisplayStyle.None;
            _assetName.text = string.Empty;

            _closeToAssetNameSpacer.style.display = DisplayStyle.None;
            _assetNameToSaveSpacer.style.display = DisplayStyle.None;

            rootVisualElement.Remove(GraphView);

            CreateGraphView();
        }

        private void SaveAnimationGraph()
        {
            AnimationGraphAsset.Nodes.Clear();

            Dictionary<Node, NodeAsset> nodeUIMap = new Dictionary<Node, NodeAsset>();

            GraphView.nodes.ForEach(n =>
            {
                BaseNodeUI nodeUI = (BaseNodeUI)n;
                NodeAsset nodeAsset = nodeUI.GenerateData();
                nodeAsset.Position = n.GetPosition().position;
                nodeAsset.IsExpanded = n.expanded;
                AnimationGraphAsset.Nodes.Add(nodeAsset);
                nodeUIMap.Add(n, nodeAsset);
            });

            GraphView.nodes.ForEach(n =>
            {
                ((BaseNodeUI)n).GenerateLinkData(nodeUIMap[n], nodeUIMap);
            });

            EditorUtility.SetDirty(AnimationGraphAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry> 
            { 
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeEntry(new GUIContent("Clip")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Mixer")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Layer Mixer")) { level = 1 },
                new SearchTreeEntry(new GUIContent("1D Blendspace")) { level = 1 },
                new SearchTreeEntry(new GUIContent("2D Blendspace")) { level = 1 },
                new SearchTreeEntry(new GUIContent("State Machine")) { level = 1 },
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Node node = null;

            switch (SearchTreeEntry.name)
            {
                case "Clip":
                    node = new ClipNodeUI();

                    break;
                case "Layer Mixer":
                    node = new LayerMixerNodeUI();

                    break;
                case "Mixer":
                    node = new MixerNodeUI();

                    break;
                case "1D Blendspace":
                    node = new Blendspace1DNodeUI();

                    break;
                case "2D Blendspace":
                    node = new Blendspace2DNodeUI();

                    break;
                case "State Machine":
                    node = new StateMachineNodeUI();

                    break;
                default:
                    break;
            }

            GraphView.AddNode(node, context.screenMousePosition);

            return true;
        }
    }
}
