using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnyStateNodeUI : StateMachineBaseNodeUI, ITransitionConnectable, IStateNode
    {
        public List<(Port port, EnumField filterMode)> StateFilterPortMap { get; private set; } = new List<(Port port, EnumField filterMode)>();

        private Foldout _transitionsFoldout;
        private Dictionary<TransitionConnectionUI, StateNodeUITransitionItem> _connectionToTransitionItemMap = new Dictionary<TransitionConnectionUI, StateNodeUITransitionItem>();

        public Port PriorityPort { get; private set; }

        private static readonly Color _portColor = new Color(180 / 255f, 40 / 255f, 255 / 255f);

        public List<TransitionConnectionUI> EntryConnections { get; private set; } = new List<TransitionConnectionUI>();
        public List<TransitionConnectionUI> ExitConnections { get; private set; } = new List<TransitionConnectionUI>();

        public AnyStateNodeUI()
        {
            title = "Any State";

            _transitionsFoldout = new Foldout() { text = "Transitions" };
            extensionContainer.Add(_transitionsFoldout);

            PriorityPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            PriorityPort.portName = "Priority";
            PriorityPort.portColor = _portColor;
            extensionContainer.Add(PriorityPort);

            Button createStateFilterPortButton = new Button(() => CreateStateFilterPort()) { text = "+ State Filter" };

            inputContainer.Add(createStateFilterPortButton);

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<GeometryChangedEvent>(e => ExitConnections.ForEach(c => c.Refresh()));

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (!StateMachineEditor.IsClosing)
                {
                    this.ClearConnections();
                }
            });
        }

        private void CreateTransitionItem(TransitionConnectionUI connection)
        {
            var transitionItem = new StateNodeUITransitionItem((TransitionNodeUI)connection.Destination);

            if (_connectionToTransitionItemMap.Count == 0)
            {
                transitionItem.AddToClassList("first");
            }
            else
            {
                _transitionsFoldout.ElementAt(_connectionToTransitionItemMap.Count - 1).RemoveFromClassList("last");
            }

            transitionItem.AddToClassList("last");

            _transitionsFoldout.Add(transitionItem);
            _connectionToTransitionItemMap.Add(connection, transitionItem);
        }

        private Port CreateStateFilterPort(AnyStateFilterMode filterMode = AnyStateFilterMode.CurrentAndNextState)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            port.portName = string.Empty;
            port.portColor = _portColor;

            EnumField stateFilterModeField = new EnumField(filterMode);
            port.Add(stateFilterModeField);

            StateFilterPortMap.Add((port, stateFilterModeField));

            MakePortRemovable(port, () => StateFilterPortMap.RemoveAt(StateFilterPortMap.Count - 1));

            inputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        public void LoadData(GraphView graphView, AnyState anyState, Dictionary<State, StateNodeUI> map)
        {
            anyState.StateFilter.ForEach(f =>
            {
                Port port = CreateStateFilterPort(f.Mode);

                if (f.State == null) { return; }

                var stateNode = map[f.State];

                Edge edge = new Edge { output = stateNode.AccessPort, input = port };
                edge.output.Connect(edge);
                edge.input.Connect(edge);

                graphView.AddElement(edge);
            });
        }

        public AnyState GenerateData(Dictionary<StateNodeUI, State> stateMap)
        {
            var anyState = new AnyState();

            foreach (var port in StateFilterPortMap)
            {
                State state = null;

                var edges = port.port.connections.ToList();

                if (edges.Count > 0)
                {
                    state = stateMap[(StateNodeUI)edges[0].output.node];
                }

                anyState.StateFilter.Add(new StateFilterItem { State = state, Mode = (AnyStateFilterMode)port.filterMode.value });
            }

            foreach (var element in outputContainer.Children())
            {
                if (!(element is Port port)) { continue; }

                anyState.ExitTransitions.Add(null);
            }

            return anyState;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Create Transition Connection", a =>
            {
                StateMachineEditor.Editor.AddTransitionConnection(this);
            });
        }

        public void OnEntryConnect(TransitionConnectionUI connection)
        {

        }

        public void OnExitConnect(TransitionConnectionUI connection)
        {
            CreateTransitionItem(connection);
        }

        public void OnEntryConnectionDeleted(TransitionConnectionUI connection)
        {

        }

        public void OnExitConnectionDeleted(TransitionConnectionUI connection)
        {
            if (!_connectionToTransitionItemMap.ContainsKey(connection)) { return; }

            var item = _connectionToTransitionItemMap[connection];
            int index = item.parent.IndexOf(item);

            if (item.parent.childCount > 1)
            {
                if (index == 0)
                {
                    item.parent.ElementAt(1).AddToClassList("first");
                }
                else if (index == item.parent.childCount - 1)
                {
                    item.parent.ElementAt(index - 1).AddToClassList("last");
                }
            }

            item.RemoveChangeDestinationLabelListener();
            item.RemoveFromHierarchy();
            _connectionToTransitionItemMap.Remove(connection);
        }
    }
}
