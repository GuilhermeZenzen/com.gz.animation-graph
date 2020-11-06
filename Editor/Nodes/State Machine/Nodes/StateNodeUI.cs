using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateNodeUI : StateMachineBaseNodeUI, ITransitionConnectable, IStateNode
    {
        public TextField NameField { get; private set; }

        private Foldout _transitionsFoldout;
        private Dictionary<TransitionConnectionUI, StateNodeUITransitionItem> _connectionToTransitionItemMap = new Dictionary<TransitionConnectionUI, StateNodeUITransitionItem>();

        public Port AccessPort { get; private set; }

        public ValueProviderOutputNodePort TimeOutputPort;
        public ValueProviderOutputNodePort PreviousTimeOutputPort;
        public ValueProviderOutputNodePort NormalizedTimeOutputPort;
        public ValueProviderOutputNodePort PreviousNormalizedTimeOutputPort;

        private static readonly Color _portColor = new Color(66 / 255f, 135 / 255f, 245 / 255f);

        public List<TransitionConnectionUI> EntryConnections { get; private set; } = new List<TransitionConnectionUI>();
        public List<TransitionConnectionUI> ExitConnections { get; private set; } = new List<TransitionConnectionUI>();

        public event Action<string> OnNameChanged;

        public StateNodeUI()
        {
            VisualElement container = new VisualElement();
            title = "State";
            Label titleLabel = (Label)titleContainer[0];
            titleContainer.RemoveAt(0);

            NameField = new TextField() { value = "New State", isDelayed = false };
            NameField.RegisterValueChangedCallback(e => OnNameChanged?.Invoke(e.newValue));
            NameField.style.flexGrow = 1f;

            container.Add(titleLabel);
            container.Add(NameField);

            titleContainer.Insert(0, container);

            _transitionsFoldout = new Foldout() { text = "Transitions" };
            extensionContainer.Add(_transitionsFoldout);

            AccessPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            AccessPort.portName = "Node";
            AccessPort.portColor = _portColor;
            extensionContainer.Add(AccessPort);

            TimeOutputPort = new ValueProviderOutputNodePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float)) { portName = "Time", portColor = _portColor,  ValueProviderType = ValueProviderType.Float };
            extensionContainer.Add(TimeOutputPort);

            PreviousTimeOutputPort = new ValueProviderOutputNodePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float)) { portName = "Previous Time", portColor = _portColor, ValueProviderType = ValueProviderType.Float };
            extensionContainer.Add(PreviousTimeOutputPort);

            NormalizedTimeOutputPort = new ValueProviderOutputNodePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float)) { portName = "Normalized Time", portColor = _portColor, ValueProviderType = ValueProviderType.Float };
            extensionContainer.Add(NormalizedTimeOutputPort);

            PreviousNormalizedTimeOutputPort = new ValueProviderOutputNodePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float)) { portName = "Previous Normalized Time", portColor = _portColor, ValueProviderType = ValueProviderType.Float };
            extensionContainer.Add(PreviousNormalizedTimeOutputPort);

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseEnterEvent>(e =>
            {
                if (StateMachineEditor.Editor.TransitionConnection != null && (StateMachineEditor.Editor.TransitionConnectionSource is StateNodeUI || StateMachineEditor.Editor.TransitionConnectionSource is AnyStateNodeUI || (StateMachineEditor.Editor.TransitionConnectionSource is TransitionNodeUI transitionNode && (transitionNode.EntryConnections.Count == 0 || transitionNode.EntryConnections[0].Source != this) && (transitionNode.ExitConnections.Count == 0 || transitionNode.ExitConnections[0].Destination != this)) || StateMachineEditor.Editor.TransitionConnectionSource is EntryNodeUI))
                {
                    StateMachineEditor.Editor.TransitionConnectionTarget = this;
                }
            });
            RegisterCallback<MouseLeaveEvent>(e =>
            {
                if (StateMachineEditor.Editor.TransitionConnection != null && StateMachineEditor.Editor.TransitionConnectionTarget == this)
                {
                    StateMachineEditor.Editor.TransitionConnectionTarget = null;
                }
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
                if (!StateMachineEditor.IsClosing)
                {
                    this.ClearConnections();
                }
            });
        }

        public void LoadData(State state)
        {
            NameField.SetValueWithoutNotify(state.Name);
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
