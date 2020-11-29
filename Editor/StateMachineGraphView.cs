using GZ.Tools.UnityUtility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateMachineGraphView : GraphView
    {
        public AnyStatePriorityManagerNodeUI AnyStatePriorityManager { get; private set; }
        public EntryNodeUI EntryNode { get; set; }

        public StateMachineGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            nodeCreationRequest = ctx =>
            {
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition, 0f, 0f), StateMachineEditor.Editor);
            };
        }
        
        public override EventPropagation DeleteSelection()
        {
            selection.ForEach(s =>
            {
                switch (s)
                {
                    case StateNodeUI stateNode:
                        StateMachineEditor.Editor.States.RemoveItem(stateNode);
                        break;
                    case ParameterNodeUI parameterNode:
                        StateMachineEditor.Editor.Parameters.RemoveItem(parameterNode);
                        break;
                    case AnyStateNodeUI anyStateNode:
                        var edges = anyStateNode.PriorityPort.connections.ToList();
                        if (edges.Count > 0)
                        {
                            edges[0].input.Disconnect(edges[0]);
                            anyStateNode.PriorityPort.DisconnectAll();
                            edges[0].RemoveFromHierarchy();
                        }

                        StateMachineEditor.Editor.AnyStates.RemoveItem(anyStateNode);
                        break;
                    case AnyStatePriorityManagerNodeUI anyStatePriorityManagerNode:
                        AnyStatePriorityManager = null;
                        break;
                    default:
                        break;
                }
            });

            return base.DeleteSelection();
        }

        public void AddNode(StateMachineBaseNodeUI node)
        {
            AddElement(node);

            switch (node)
            {
                case StateNodeUI stateNode:
                    StateMachineEditor.Editor.States.AddItem(stateNode);
                    break;
                case ParameterNodeUI parameterNode:
                    StateMachineEditor.Editor.Parameters.AddItem(parameterNode);
                    break;
                case AnyStateNodeUI anyStateNode:
                    StateMachineEditor.Editor.AnyStates.AddItem(anyStateNode);

                    break;
                case AnyStatePriorityManagerNodeUI anyStatePriorityManagerNode:
                    AnyStatePriorityManager = anyStatePriorityManagerNode;
                    break;
            }

            node.GraphView = this;
        }
        public void AddNode(StateMachineBaseNodeUI node, Vector2 screenMousePosition)
        {
            AddNode(node);
            var windowRoot = StateMachineEditor.Editor.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, screenMousePosition - StateMachineEditor.Editor.position.position);
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            node.SetPosition(new Rect(graphMousePosition.x, graphMousePosition.y, 0, 0));
        }
    }
}
