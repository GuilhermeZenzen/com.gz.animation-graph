using GZ.Tools.UnityUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public abstract class BaseStateNodeUI<T> : NamedStateMachineBaseNodeUI<T>, IStateNode where T : BaseStateNodeUI<T>, INamedItem<T>
    {

        protected Foldout _transitionsFoldout;
        public IndexedDictionary<(TransitionConnectionUI ui, TransitionInfo info), StateNodeUITransitionItem> ConnectionToTransitionItemMap { get; protected set; } = new IndexedDictionary<(TransitionConnectionUI ui, TransitionInfo info), StateNodeUITransitionItem>();

        private List<(TransitionConnectionUI connection, TransitionInfo info)> _transitions = new List<(TransitionConnectionUI connection, TransitionInfo info)>();
        private HashSet<StateNodeUI> _connectedNodes = new HashSet<StateNodeUI>();

        private ResizableListView _transitionList;

        public List<ConnectionUI> EntryConnections { get; protected set; } = new List<ConnectionUI>();
        public List<ConnectionUI> ExitConnections { get; protected set; } = new List<ConnectionUI>();

        public event Action<string> OnNameChanged;

        public virtual bool HasTwoWaysConnection => false;

        public BaseStateNodeUI() : base()
        {
            AddToClassList("base-state-node");

            _transitionsFoldout = new Foldout() { value = false, text = "Transitions" };
            extensionContainer.Add(_transitionsFoldout);

            VisualElement transitionListContainer = new VisualElement();
            transitionListContainer.RegisterCallback<MouseDownEvent>(e => e.StopPropagation());
            _transitionsFoldout.Add(transitionListContainer);

            _transitionList = new ResizableListView(_transitions, 20, () =>
            {
                VisualElement container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.Add(new Label());
                container.Add(new Button() { text = "X" });

                return container;
            }, (item, index) =>
            {
                void Remove()
                {
                    _transitions[index].connection.RemoveTransition(_transitions[index].info);
                }

                item.Q<Label>().text = ((StateNodeUI)_transitions[index].connection.Destination).Name;

                Button removeButton = item.Q<Button>();
                removeButton.clicked -= Remove;
                removeButton.clicked += Remove;

            });
            _transitionList.reorderable = true;
            _transitionList.selectionType = SelectionType.Single;
            _transitionList.onSelectionChange += selection =>
            {
                StateMachineEditor.Editor.TransitionInspector.Show(_transitions[_transitionList.selectedIndex].connection);
                StateMachineEditor.Editor.TransitionInspector.SelectTransition(_transitions[_transitionList.selectedIndex].info);
            };
            _transitionList.onItemsChosen += items =>
            {
                StateMachineEditor.Editor.GraphView.ClearSelection();
                StateMachineEditor.Editor.GraphView.AddToSelection((StateNodeUI)_transitions[_transitionList.selectedIndex].connection.Destination);
                StateMachineEditor.Editor.GraphView.FrameSelection();
            };
            _transitionList.AddToClassList("base-state__transition-list");
            transitionListContainer.Add(_transitionList);

            StateMachineEditor.Editor.States.OnItemRenamed += RenameTransitionItem;

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseEnterEvent>(e =>
            {
                StateMachineEditor.Editor.TargetConnectable(this);
            });
            RegisterCallback<MouseLeaveEvent>(e =>
            {
                StateMachineEditor.Editor.UntargetConnectable(this);
            });

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                EntryConnections.ForEach(c =>
                {
                    c.Refresh();
                });
                ExitConnections.ForEach(c =>
                {
                    c.Refresh();
                });
            });

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                StateMachineEditor.Editor.States.OnItemRenamed -= RenameTransitionItem;

                if (!StateMachineEditor.IsClosing)
                {
                    this.ClearConnections();
                }
            });
        }

        public void LoadData(State state)
        {
            Name = state.Name;
        }

        private void CreateTransitionItem(TransitionConnectionUI connection, TransitionInfo transitionInfo)
        {
            _transitions.Add((connection, transitionInfo));
            _transitionList.Refresh();
        }

        private void RenameTransitionItem(StateNodeUI stateNode, string previousName)
        {
            if (_connectedNodes.Contains(stateNode))
            {
                _transitionList.Refresh();
            }
        }

        private void RemoveTransitionItem(TransitionConnectionUI transitionConnection, TransitionInfo transitionInfo, int index = -1)
        {
            _transitions.Remove((transitionConnection, transitionInfo));
            _transitionList.Refresh();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Create Transition", a =>
            {
                StateMachineEditor.Editor.AddConnection(this);
            });
        }

        public (ConnectionUI connection, bool isNew) GetConnection(IConnectable target, bool isEnabled)
        {
            TransitionConnectionUI connection = ExitConnections.Find(conn => conn.Destination == target) as TransitionConnectionUI;

            if (connection != null)
            {
                connection.CreateTransition();
                return (connection, false);
            }

            connection = new TransitionConnectionUI(isEnabled);
            connection.CreateTransition();

            return (connection, true);
        }

        public bool CanConnect(IConnectable target) => target is StateNodeUI;

        public void OnEntryConnect(ConnectionUI connection)
        {

        }

        public void OnExitConnect(ConnectionUI connection)
        {
            var transitionConnection = (TransitionConnectionUI)connection;
            transitionConnection.OnCreatedTransition += CreateTransitionItem;
            transitionConnection.OnRemovedTransition += RemoveTransitionItem;
            _connectedNodes.Add((StateNodeUI)connection.Destination);
            CreateTransitionItem(transitionConnection, transitionConnection.Transitions[transitionConnection.Transitions.Count - 1]);
        }

        public void OnEntryConnectionDeleted(ConnectionUI connection)
        {

        }

        public void OnExitConnectionDeleted(ConnectionUI connection)
        {
            var transitionConnection = (TransitionConnectionUI)connection;

            foreach (var transitionInfo in transitionConnection.Transitions)
            {
                _transitions.Remove((transitionConnection, transitionInfo));
            }

            _transitionList.Refresh();

            _connectedNodes.Remove((StateNodeUI)transitionConnection.Destination);
        }
    }
}
