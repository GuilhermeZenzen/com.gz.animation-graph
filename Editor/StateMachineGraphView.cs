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
        public List<StateNodeUI> States { get; private set; } = new List<StateNodeUI>();
        public List<TransitionNodeUI> Transitions { get; private set; } = new List<TransitionNodeUI>();
        public List<ParameterNodeUI> Parameters { get; private set; } = new List<ParameterNodeUI>();
        public List<AnyStateNodeUI> AnyStates { get; private set; } = new List<AnyStateNodeUI>();
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
                        States.Remove(stateNode);
                        foreach (var edge in stateNode.AccessPort.connections)
                        {
                            edge.input.Disconnect(edge);
                            edge.RemoveFromHierarchy();
                        }
                        stateNode.AccessPort.DisconnectAll();
                        break;
                    case TransitionNodeUI transitionNode:
                        Transitions.Remove(transitionNode);
                        break;
                    case ParameterNodeUI parameterNode:
                        Parameters.Remove(parameterNode);
                        break;
                    case AnyStateNodeUI anyStateNode:
                        var edges = anyStateNode.PriorityPort.connections.ToList();
                        if (edges.Count > 0)
                        {
                            edges[0].input.Disconnect(edges[0]);
                            anyStateNode.PriorityPort.DisconnectAll();
                            edges[0].RemoveFromHierarchy();
                        }
                        AnyStates.Remove(anyStateNode);
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
                    States.Add(stateNode);
                    break;
                case TransitionNodeUI transitionNode:
                    Transitions.Add(transitionNode);
                    break;
                case ParameterNodeUI parameterNode:
                    Parameters.Add(parameterNode);
                    break;
                case AnyStateNodeUI anyStateNode:
                    AnyStates.Add(anyStateNode);
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
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    bool canConnect = true;

                    if (startPort is ValueProviderOutputNodePort valueProviderPort)
                    {
                        canConnect = port is TransitionConditionInputNodePort;
                    }
                    else if (startPort is TransitionConditionInputNodePort conditionPort)
                    {
                        canConnect = port is ValueProviderOutputNodePort;
                    }
                    else if (canConnect)
                    {
                        switch (startPort.node)
                        {
                            case StateNodeUI stateNode:
                                canConnect = StateCompatiblePorts(startPort, port);
                                break;
                            case AnyStateNodeUI anyStateNode:
                                canConnect = AnyStateCompatiblePorts(startPort, port);
                                break;
                            case AnyStatePriorityManagerNodeUI anyStatePriorityManagerNode:
                                canConnect = AnyStatePriorityManagerCompatiblePorts(startPort, port);
                                break;
                            case TransitionNodeUI transitionNode:
                                canConnect = TransitionCompatiblePorts(startPort, port);
                                break;
                            case EntryNodeUI entryNode:
                                canConnect = port.node is StateNodeUI stateNodeUI && port == stateNodeUI.AccessPort;
                                break;
                            default:
                                break;
                        }
                    }

                    if (canConnect)
                    {
                        compatiblePorts.Add(port);
                    }
                }
            });

            return compatiblePorts;
        }

        private bool StateCompatiblePorts(Port startPort, Port port)
        {
            if (startPort == ((StateNodeUI)startPort.node).AccessPort)
            {
                if (!(port.node is AnyStateNodeUI) && !(port.node is EntryNodeUI))
                {
                    return false;
                }

                return true;
            }
            else if (!(port.node is TransitionNodeUI))
            {
                return false;
            }

            if (startPort.direction == Direction.Output)
            {
                if (port.node.inputContainer.IndexOf(port) != 0)
                {
                    return false;
                }

                var edges = ((Port)port.node.outputContainer[0]).connections.ToList();

                return edges.Count == 0 || edges[0].input.node != startPort.node;
            }
            else
            {
                var edges = ((Port)port.node.inputContainer[0]).connections.ToList();

                return edges.Count == 0 || edges[0].output.node != startPort.node;
            }
        }

        private bool AnyStateCompatiblePorts(Port startPort, Port port)
        {
            if (startPort.direction == Direction.Input)
            {
                return port.node is StateNodeUI && port.node.extensionContainer.Contains(port);
            }
            else if (startPort.node.extensionContainer.Contains(startPort))
            {
                return port.node is AnyStatePriorityManagerNodeUI;
            }
            else
            {
                return port.node is TransitionNodeUI && port.node.inputContainer.IndexOf(port) == 0;
            }
        }

        private bool AnyStatePriorityManagerCompatiblePorts(Port startPort, Port port)
        {
            return port.node is AnyStateNodeUI && port.node.extensionContainer.Contains(port);
        }

        private bool TransitionCompatiblePorts(Port startPort, Port port)
        {
            if (startPort.direction == Direction.Output)
            {
                if (!(port.node is StateNodeUI))
                {
                    return false;
                }

                var edges = ((Port)startPort.node.inputContainer[0]).connections.ToList();

                return edges.Count == 0 || edges[0].output.node != port.node;
            }
            else
            {
                if (startPort.node.inputContainer.IndexOf(startPort) == 0)
                {
                    if (port.node is AnyStateNodeUI)
                    {
                        return true;
                    }

                    if (!(port.node is StateNodeUI))
                    {
                        return false;
                    }

                    var edges = ((Port)startPort.node.outputContainer[0]).connections.ToList();

                    return edges.Count == 0 || edges[0].input.node != port.node;
                }
                else
                {
                    return port.node is ParameterNodeUI;
                }
            }
        }
    }
}
