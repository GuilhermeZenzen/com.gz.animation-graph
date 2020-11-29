using GZ.AnimationGraph;
using GZ.AnimationGraph.Editor;
using GZ.Tools.UnityUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.WSA.Input;

namespace GZ.AnimationGraph.Editor
{
    public class StateMachineEditor : EditorWindow, ISearchWindowProvider
    {
        public static StateMachineEditor Editor;

        public static bool IsClosing;

        public StateMachineNodeUI NodeUI;

        public StateMachineGraphView GraphView { get; private set; }

        private Toolbar _toolbar;
        private ToolbarButton _saveButton;

        private const string _ussPath = "StateMachine.uss";
        private static StyleSheet _styleSheet;

        public ConnectionUI Connection { get; set; }
        public IConnectable ConnectionSource { get; set; }
        public IConnectable ConnectionTarget { get; set; }

        public ConnectionUI SelectedConnection { get; private set; }

        public List<TransitionConnectionUI> TransitionConnections { get; private set; } = new List<TransitionConnectionUI>();

        public TransitionInspector TransitionInspector { get; private set; }

        public List<ParameterNodeUI> ParametersTemp { get; private set; } = new List<ParameterNodeUI>();
        public NamedItemsGroup<ParameterNodeUI> Parameters { get; private set; } = new NamedItemsGroup<ParameterNodeUI>();

        public NamedItemsGroup<StateNodeUI> States { get; private set; } = new NamedItemsGroup<StateNodeUI>();

        public NamedItemsGroup<AnyStateNodeUI> AnyStates { get; private set; } = new NamedItemsGroup<AnyStateNodeUI>();

        #region Lifecycle

        public static void OpenEditor()
        {
            GetWindow<StateMachineEditor>();
        }

        private void OnEnable()
        {
            Editor = this;
            titleContent = new GUIContent("State Machine");

            string resourcesPath = $"{AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)).Replace($"/{nameof(StateMachineEditor)}.cs", "")}/Resources";
            _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{resourcesPath}/{_ussPath}");

            _toolbar = new Toolbar();

            _saveButton = new ToolbarButton(SaveStateMachine) { text = "Save" };

            _toolbar.Add(_saveButton);

            TransitionInspector = new TransitionInspector();
            TransitionInspector.SetValueWithoutNotify(false);
            TransitionInspector.Hide();

            rootVisualElement.Add(_toolbar);
            rootVisualElement.Add(TransitionInspector);

            rootVisualElement.styleSheets.Add(_styleSheet);

            this.SetAntiAliasing(4);
        }

        private void OnDestroy()
        {
            Editor = null;
            IsClosing = true;

            rootVisualElement.Remove(GraphView);
            rootVisualElement.Remove(_toolbar);
            _styleSheet = null;

            IsClosing = false;
        }

        #endregion Lifecycle

        private void CreateGraphView()
        {
            if (GraphView != null) { rootVisualElement.Remove(GraphView); }

            GraphView = new StateMachineGraphView
            {
                name = "State Machine Graph"
            };
            //GraphView.styleSheets.Add(_styleSheet);

            GraphView.StretchToParentSize();

            rootVisualElement.Insert(0, GraphView);

            GraphView.RegisterCallback<MouseMoveEvent>(UpdateConnection);
            GraphView.RegisterCallback<MouseUpEvent>(e =>
            {
                FinishConnection(e.button == 0);
            });
            GraphView.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    FinishConnection();
                }
            });
        }

        #region Connection

        public void SelectConnection(ConnectionUI connection)
        {
            if (SelectedConnection != null)
            {
                SelectedConnection.IsConnectionSelected = false;
            }

            SelectedConnection = connection;

            if (SelectedConnection != null)
            {
                SelectedConnection.IsConnectionSelected = true;

                if (SelectedConnection is TransitionConnectionUI transitionConnection)
                {
                    TransitionInspector.Show(transitionConnection);
                }
                else
                {
                    TransitionInspector.Hide();
                }
            }
            else
            {
                TransitionInspector.Hide();
            }
        }

        public ConnectionUI CreateConnection(IConnectable source, IConnectable destination = null, bool isTemporary = true)
        {
            ConnectionUI connection = null;

            if (isTemporary)
            {
                connection = new ConnectionUI(false) { Source = source };
                GraphView.AddElement(connection);
                connection.SendToBack();
            }
            else
            {
                bool isNew;
                (connection, isNew) = source.GetConnection(destination, true);
                connection.Source = source;
                connection.Destination = destination;

                if (isNew)
                {
                    GraphView.AddElement(connection);
                    connection.SendToBack();

                    connection.Source = source;
                    connection.Destination = destination;
                    source.ExitConnections.Add(connection);
                    destination.EntryConnections.Add(connection);

                    source.OnExitConnect(connection);
                    destination.OnEntryConnect(connection);

                    connection.schedule.Execute(() => connection.Refresh());

                    if (connection is TransitionConnectionUI transitionConnection)
                    {
                        TransitionConnections.Add(transitionConnection);
                    }
                }
            }

            return connection;
        }

        public void RemoveConnection(ConnectionUI connection)
        {
            GraphView.RemoveElement(connection);

            if (connection is TransitionConnectionUI transitionConnection)
            {
                TransitionConnections.Remove(transitionConnection);
            }

            SelectConnection(null);
        }

        public void AddConnection(IConnectable connectable)
        {
            var connection = CreateConnection(connectable, null, true);
            connection.style.top = connectable.resolvedStyle.top + connectable.resolvedStyle.height / 2;
            connection.style.left = connectable.resolvedStyle.left + connectable.resolvedStyle.width / 2;
            Connection = connection;
            ConnectionSource = connectable;

            if (_panSchedule == null)
            {
                _panSchedule = GraphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
                _panSchedule.Pause();
            }
        }

        private void UpdateConnection(MouseMoveEvent e)
        {
            if (Connection == null) { return; }

            _panDiff = GetEffectivePanSpeed(GraphView.ChangeCoordinatesTo(GraphView.contentContainer, e.localMousePosition));

            if (_panDiff != Vector3.zero)
            {
                _panSchedule.Resume();
            }
            else
            {
                _panSchedule.Pause();
            }

            _localMousePosition = e.localMousePosition;
            UpdateConnectionVisual(e.localMousePosition);
        }

        private void UpdateConnectionVisual(Vector2 localMousePosition)
        {
            Connection.end = ConnectionTarget != null
                ? new Vector3(ConnectionTarget.resolvedStyle.left + ConnectionTarget.resolvedStyle.width / 2 - Connection.resolvedStyle.left, ConnectionTarget.resolvedStyle.top + ConnectionTarget.resolvedStyle.height / 2 - Connection.resolvedStyle.top)
                : new Vector3(localMousePosition.x - Connection.worldBound.x, localMousePosition.y - Connection.worldBound.y + _toolbar.resolvedStyle.height) / GraphView.scale;
            Connection.style.width = Connection.end.x;
            Connection.style.height = Connection.end.y;
            Connection.MarkDirtyRepaint();
        }

        public void TargetConnectable(IConnectable target)
        {
            if (ConnectionSource != null && ConnectionSource != target && ConnectionSource.CanConnect(target))
            {
                ConnectionTarget = target;
            }
        }

        public void UntargetConnectable(IConnectable target)
        {
            if (ConnectionSource != null && ConnectionTarget == target)
            {
                ConnectionTarget = null;
            }
        }

        private void FinishConnection(bool canConnect = true)
        {
            if (Connection == null) { return; }

            _panSchedule.Pause();

            if (ConnectionTarget == null || !canConnect)
            {
                GraphView.RemoveElement(Connection);
            }
            else
            {
                GraphView.RemoveElement(Connection);
                Connection = CreateConnection(ConnectionSource, ConnectionTarget, false);
            }

            Connection = null;
            ConnectionSource = null;
            ConnectionTarget = null;
        }

        #endregion Connection

        #region Parameter

        public void AddParameter(ParameterNodeUI parameter)
        {
            ParametersTemp.Add(parameter);
        }

        public void RemoveParameter(ParameterNodeUI parameter)
        {
            int index = ParametersTemp.IndexOf(parameter);
            ParametersTemp.RemoveAt(index);
        }

        #endregion Parameter

        #region Pan

        private const int k_PanAreaWidth = 100;
        private const int k_PanSpeed = 4;
        private const int k_PanInterval = 10;
        private const float k_MinSpeedFactor = 0.5f;
        private const float k_MaxSpeedFactor = 2.5f;
        private const float k_MaxPanSpeed = k_MaxSpeedFactor * k_PanSpeed;

        private IVisualElementScheduledItem _panSchedule;
        private Vector3 _panDiff;

        private Vector2 _localMousePosition;

        private void Pan(TimerState ts)
        {
            GraphView.viewTransform.position -= _panDiff;

            UpdateConnectionVisual(GraphView.ChangeCoordinatesTo(GraphView.contentContainer, _localMousePosition));
        }

        private Vector2 GetEffectivePanSpeed(Vector2 mousePos)
        {
            Vector2 effectiveSpeed = Vector2.zero;

            if (mousePos.x <= k_PanAreaWidth)
                effectiveSpeed.x = -(((k_PanAreaWidth - mousePos.x) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (mousePos.x >= GraphView.contentContainer.layout.width - k_PanAreaWidth)
                effectiveSpeed.x = (((mousePos.x - (GraphView.contentContainer.layout.width - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            if (mousePos.y <= k_PanAreaWidth)
                effectiveSpeed.y = -(((k_PanAreaWidth - mousePos.y) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (mousePos.y >= GraphView.contentContainer.layout.height - k_PanAreaWidth)
                effectiveSpeed.y = (((mousePos.y - (GraphView.contentContainer.layout.height - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            effectiveSpeed = Vector2.ClampMagnitude(effectiveSpeed, k_MaxPanSpeed);

            return effectiveSpeed;
        }

        #endregion Pan

        #region Load & Save

        private void SaveStateMachine()
        {
            var stateMachine = new StateMachineNode();
            NodeUI.StateMachineNodeAsset = new StateMachineNodeAsset { Data = stateMachine };

            var parameterMap = new Dictionary<ParameterNodeUI, Parameter>();
            var stateMap = new Dictionary<StateNodeUI, State>();
            var anyStateMap = new Dictionary<AnyStateNodeUI, AnyState>();
            var transitionMap = new Dictionary<TransitionInfo, Transition>();

            Parameters.Items.Values.ForEach(p =>
            {
                var parameter = stateMachine.AddParameter(p.Name);
                parameter.Type = (ValueProviderType)p.ParameterTypeField.value;
                NodeUI.StateMachineNodeAsset.ParameterMap.Add(parameter.Id, new NodeVisualInfo(p.GetPosition().position, p.expanded));

                switch (parameter.ValueProvider)
                {
                    case BoolProvider boolProvider:
                        boolProvider.Value = p.BoolField.value;
                        break;
                    case IntProvider intProvider:
                        intProvider.Value = p.IntField.value;
                        break;
                    case FloatProvider floatProvider:
                        floatProvider.Value = p.FloatField.value;
                        break;
                    default:
                        break;
                }

                parameterMap.Add(p, parameter);
            });

            TransitionConnections.ForEach(transitionConnection =>
            {
                transitionConnection.Transitions.ForEach(transitionInfo =>
                {
                    Transition transition = new Transition()
                    {
                        DurationType = transitionInfo.DurationType,
                        Duration = transitionInfo.Duration,
                        OffsetType = transitionInfo.OffsetType,
                        Offset = transitionInfo.Offset,
                        InterruptionSource = transitionInfo.InterruptionSource,
                        OrderedInterruption = transitionInfo.OrderedInterruption,
                        InterruptableByAnyState = transitionInfo.InterruptableByAnyState,
                        PlayAfterTransition = transitionInfo.PlayAfterTransition
                    };

                    stateMachine.Transitions.Add(transition);
                    transitionMap.Add(transitionInfo, transition);
                });
            });

            States.Items.Values.ForEach(stateNode =>
            {
                var state = stateMachine.AddState(stateNode.Name);

                stateNode.ExitConnections.ForEach(connection =>
                {
                    ((TransitionConnectionUI)connection).Transitions.ForEach(transitionInfo =>
                    {
                        Transition transition = transitionMap[transitionInfo];
                        transition.SourceState = state;
                        state.ExitTransitions.Add(transition);
                    });
                });

                stateNode.EntryConnections.ForEach(connection =>
                {
                    if (!(connection is TransitionConnectionUI transitionConnection)) { return; }

                    transitionConnection.Transitions.ForEach(transitionInfo =>
                    {
                        Transition transition = transitionMap[transitionInfo];
                        transition.DestinationState = state;
                        state.EntryTransitions.Add(transition);
                    });
                });

                NodeUI.StateMachineNodeAsset.StateMap.Add(state.Id, new NodeVisualInfo(stateNode.GetPosition().position, stateNode.expanded));
                stateMap.Add(stateNode, state);
            });

            HashSet<AnyStateNodeUI> alreadyAddedAnyStates = new HashSet<AnyStateNodeUI>();

            if (GraphView.AnyStatePriorityManager == null)
            {
                NodeUI.StateMachineNodeAsset.AnyStatePriorityManager = null;
            }
            else
            {
                NodeUI.StateMachineNodeAsset.AnyStatePriorityManager = new NodeVisualInfo(GraphView.AnyStatePriorityManager.GetPosition().position, GraphView.AnyStatePriorityManager.expanded);

                foreach (var anyStateNode in GraphView.AnyStatePriorityManager.AnyStates)
                {
                    if (anyStateNode == null) { continue; }

                    AnyState anyState = anyStateNode.GenerateData(stateMap);

                    anyStateNode.ExitConnections.ForEach(connection =>
                    {
                        ((TransitionConnectionUI)connection).Transitions.ForEach(transitionInfo =>
                        {
                            Transition transition = transitionMap[transitionInfo];
                            transition.SourceState = anyState;
                            anyState.ExitTransitions.Add(transition);
                        });
                    });

                    stateMachine.AnyStates.AddItem(anyState);
                    NodeUI.StateMachineNodeAsset.AnyStateMap.Add(anyState.Id, new NodeVisualInfo(anyStateNode.GetPosition().position, anyStateNode.expanded));
                    alreadyAddedAnyStates.Add(anyStateNode);
                    anyStateMap.Add(anyStateNode, anyState);
                }
            }

            AnyStates.Items.Values.ForEach(anyStateNode =>
            {
                if (alreadyAddedAnyStates.Contains(anyStateNode)) { return; }

                AnyState anyState = anyStateNode.GenerateData(stateMap);

                anyStateNode.ExitConnections.ForEach(connection =>
                {
                    ((TransitionConnectionUI)connection).Transitions.ForEach(transitionInfo =>
                    {
                        Transition transition = transitionMap[transitionInfo];
                        transition.SourceState = anyState;
                        anyState.ExitTransitions.Add(transition);
                    });
                });

                stateMachine.AnyStates.AddItem(anyState);
                NodeUI.StateMachineNodeAsset.AnyStateMap.Add(anyState.Id, new NodeVisualInfo(anyStateNode.GetPosition().position, anyStateNode.expanded));
                anyStateMap.Add(anyStateNode, anyState);
            });

            TransitionConnections.ForEach(transitionConnection =>
            {
                transitionConnection.Transitions.ForEach(transitionInfo =>
                {
                    Transition transition = transitionMap[transitionInfo];
                    transition.Conditions.Capacity = transitionInfo.Conditions.Count;

                    transitionInfo.Conditions.ForEach(infoCondition =>
                    {
                        TransitionCondition condition = new TransitionCondition();

                        if (infoCondition.ProviderSourceType == ValueProviderSourceType.State)
                        {
                            if (infoCondition.State != null)
                            {
                                State state = stateMap[infoCondition.State];

                                switch (infoCondition.StateValueProvider)
                                {
                                    case StateValueProviders.PreviousTime:
                                        condition.SetValueProvider(state.PreviousTime);
                                        break;
                                    case StateValueProviders.Time:
                                        condition.SetValueProvider(state.Time);
                                        break;
                                    case StateValueProviders.PreviousNormalizedTime:
                                        condition.SetValueProvider(state.PreviousNormalizedTime);
                                        break;
                                    case StateValueProviders.NormalizedTime:
                                        condition.SetValueProvider(state.NormalizedTime);
                                        break;
                                }

                                FloatConditionEvaluator floatEvaluator = (FloatConditionEvaluator)condition.Evaluator;
                                floatEvaluator.Comparison = infoCondition.FloatComparison;
                                floatEvaluator.ComparisonValue = infoCondition.FloatComparisonValue;
                            }
                        }
                        else if (infoCondition.Parameter != null)
                        {
                            condition.SetValueProvider(parameterMap[infoCondition.Parameter].ValueProvider);

                            switch (condition.Evaluator)
                            {
                                case BoolConditionEvaluator boolEvaluator:
                                    boolEvaluator.ComparisonValue = infoCondition.BoolComparisonValue ? Bool.True : Bool.False;
                                    break;
                                case IntConditionEvaluator intEvaluator:
                                    intEvaluator.Comparison = infoCondition.IntComparison;
                                    intEvaluator.ComparisonValue = infoCondition.IntComparisonValue;
                                    break;
                                case FloatConditionEvaluator floatEvaluator:
                                    floatEvaluator.Comparison = infoCondition.FloatComparison;
                                    floatEvaluator.ComparisonValue = infoCondition.FloatComparisonValue;
                                    break;
                                default:
                                    break;
                            }
                        }

                        transition.Conditions.Add(condition);
                    });
                });
            });

            GraphView.EntryNode.GenerateData(stateMachine, stateMap);
            NodeUI.StateMachineNodeAsset.EntryState = new NodeVisualInfo(GraphView.EntryNode.GetPosition().position, GraphView.EntryNode.expanded);

            NodeUI.UpdateStatePorts();
        }

        public void LoadStateMachine(StateMachineNodeUI stateMachineUI)
        {
            CreateGraphView();

            void SetNodePositionAndExpansion(Node node, NodeVisualInfo info)
            {
                node.SetPosition(new Rect(info.Position, Vector2.zero));
                node.expanded = info.IsExpanded;
            }
            
            NodeUI = stateMachineUI;

            var stateMachine = (StateMachineNode)stateMachineUI.StateMachineNodeAsset.Data;

            var parameterMap = new Dictionary<Parameter, ParameterNodeUI>();
            var valueProviderMap = new Dictionary<IValueProvider, StateMachineBaseNodeUI>();

            foreach (var parameter in stateMachine.Parameters.Values)
            {
                var parameterNode = new ParameterNodeUI();
                GraphView.AddNode(parameterNode);
                SetNodePositionAndExpansion(parameterNode, stateMachineUI.StateMachineNodeAsset.ParameterMap[parameter.Id]);
                parameterNode.LoadData(parameter);
                parameterMap.Add(parameter, parameterNode);
                valueProviderMap.Add(parameter.ValueProvider, parameterNode);
            }

            var stateToNodeMap = new Dictionary<State, StateNodeUI>();
            var nodeToStateMap = new Dictionary<StateNodeUI, State>();

            for (int i = 0; i < stateMachine.States.Count; i++)
            {
                string stateName = stateMachine.States.KeyAt(i);
                State state = stateMachine.States.At(i);
            
                var stateNode = new StateNodeUI();
                stateNode.Name = stateName;
                GraphView.AddNode(stateNode);
                SetNodePositionAndExpansion(stateNode, stateMachineUI.StateMachineNodeAsset.StateMap[state.Id]);
                stateToNodeMap.Add(state, stateNode);
                nodeToStateMap.Add(stateNode, state);
                valueProviderMap.Add(state.Time, stateNode);
                valueProviderMap.Add(state.PreviousTime, stateNode);
                valueProviderMap.Add(state.NormalizedTime, stateNode);
                valueProviderMap.Add(state.PreviousNormalizedTime, stateNode);
            }

            var anyStateMap = new Dictionary<AnyState, AnyStateNodeUI>();
            
            for (int i = 0; i < stateMachine.AnyStates.Items.Count; i++)
            {
                string anyStateName = stateMachine.AnyStates.Items.KeyAt(i);
                AnyState anyState = stateMachine.AnyStates.Items.At(i);

                //if (anyState == null) { continue; }

                var anyStateNode = new AnyStateNodeUI();
                anyStateNode.Name = anyStateName;
                GraphView.AddNode(anyStateNode);
                SetNodePositionAndExpansion(anyStateNode, stateMachineUI.StateMachineNodeAsset.AnyStateMap[anyState.Id]);
                anyStateNode.LoadData(GraphView, anyState, stateToNodeMap);
                anyStateMap.Add(anyState, anyStateNode);

                foreach (var transition in anyState.ExitTransitions)
                {
                    var transitionConnection = (TransitionConnectionUI)CreateConnection(anyStateNode, stateToNodeMap[transition.DestinationState], false);
                    LoadTransitionInfo(transitionConnection.Transitions[transitionConnection.Transitions.Count - 1], transition);
                }
            }

            if (NodeUI.StateMachineNodeAsset.AnyStatePriorityManager != null)
            {
                var anyStatePriorityManagerNode = new AnyStatePriorityManagerNodeUI();
                GraphView.AddNode(anyStatePriorityManagerNode);
                SetNodePositionAndExpansion(anyStatePriorityManagerNode, stateMachineUI.StateMachineNodeAsset.AnyStatePriorityManager);
                anyStatePriorityManagerNode.LoadData(GraphView, stateMachine.AnyStates.Items.Values, anyStateMap);
            }

            foreach (var state in stateMachine.States.Values)
            {
                foreach (var transition in state.ExitTransitions)
                {
                    StateNodeUI stateNode = stateToNodeMap[(State)transition.SourceState];
                    var transitionConnection = (TransitionConnectionUI)CreateConnection(stateNode, stateToNodeMap[transition.DestinationState], false);
                    LoadTransitionInfo(transitionConnection.Transitions[transitionConnection.Transitions.Count - 1], transition);
                }
            }

            void LoadTransitionInfo(TransitionInfo transitionInfo, Transition transition)
            {
                transitionInfo.DurationType = transition.DurationType;
                transitionInfo.Duration = transition.Duration;
                transitionInfo.OffsetType = transition.OffsetType;
                transitionInfo.Offset = transition.Offset;
                transitionInfo.InterruptionSource = transition.InterruptionSource;
                transitionInfo.OrderedInterruption = transition.OrderedInterruption;
                transitionInfo.InterruptableByAnyState = transition.InterruptableByAnyState;
                transitionInfo.PlayAfterTransition = transition.PlayAfterTransition;

                transition.Conditions.ForEach(condition =>
                {
                    TransitionInfoCondition infoCondition = new TransitionInfoCondition();
                    transitionInfo.Conditions.Add(infoCondition);

                    if (condition.ValueProvider == null) { return; }

                    StateMachineBaseNodeUI node = valueProviderMap[condition.ValueProvider];

                    if (node is StateNodeUI stateNode)
                    {
                        State state = nodeToStateMap[stateNode];
                        infoCondition.ProviderSourceType = ValueProviderSourceType.State;
                        infoCondition.State = stateNode;
                        infoCondition.StateValueProvider = condition.ValueProvider == state.PreviousTime ? StateValueProviders.PreviousTime
                                                         : condition.ValueProvider == state.Time ? StateValueProviders.Time
                                                         : condition.ValueProvider == state.NormalizedTime ? StateValueProviders.NormalizedTime
                                                         : StateValueProviders.PreviousNormalizedTime;
                        FloatConditionEvaluator floatEvaluator = (FloatConditionEvaluator)condition.Evaluator;
                        infoCondition.FloatComparison = floatEvaluator.Comparison;
                        infoCondition.FloatComparisonValue = floatEvaluator.ComparisonValue;
                    }
                    else
                    {
                        ParameterNodeUI parameterNode = (ParameterNodeUI)node;
                        infoCondition.ProviderSourceType = ValueProviderSourceType.Parameter;
                        infoCondition.Parameter = parameterNode;

                        switch (condition.Evaluator)
                        {
                            case BoolConditionEvaluator boolEvaluator:
                                infoCondition.BoolComparisonValue = boolEvaluator.ComparisonValue == Bool.True;
                                break;
                            case IntConditionEvaluator intEvaluator:
                                infoCondition.IntComparison = intEvaluator.Comparison;
                                infoCondition.IntComparisonValue = intEvaluator.ComparisonValue;
                                break;
                            case FloatConditionEvaluator floatEvaluator:
                                infoCondition.FloatComparison = floatEvaluator.Comparison;
                                infoCondition.FloatComparisonValue = floatEvaluator.ComparisonValue;
                                break;
                        }
                    }
                });
            }

            EntryNodeUI entryStateNode = new EntryNodeUI();
            GraphView.AddNode(entryStateNode);
            GraphView.EntryNode = entryStateNode;
            if (NodeUI.StateMachineNodeAsset.EntryState != null)
            {
                SetNodePositionAndExpansion(entryStateNode, NodeUI.StateMachineNodeAsset.EntryState);
            }
            entryStateNode.LoadData(GraphView, stateMachine.EntryState, stateToNodeMap);
        }

        #endregion Load & Save

        #region Search Tree

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeEntry(new GUIContent("State")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Any State")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Parameter")) { level = 1 },
            };

            if (!GraphView.nodes.ToList().Exists(n => n is AnyStatePriorityManagerNodeUI))
            {
                entries.Insert(3, new SearchTreeEntry(new GUIContent("Any State Priority Manager")) { level = 1 });
            }

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            StateMachineBaseNodeUI node = null;

            switch (SearchTreeEntry.name)
            {
                case "State":
                    node = new StateNodeUI();
                    break;
                case "Any State":
                    node = new AnyStateNodeUI();
                    break;
                case "Any State Priority Manager":
                    node = new AnyStatePriorityManagerNodeUI();
                    break;
                case "Parameter":
                    node = new ParameterNodeUI();
                    break;
                default:
                    break;
            }

            GraphView.AddNode(node, context.screenMousePosition);

            if (node is StateNodeUI stateNode && States.Items.Count == 1)
            {
                CreateConnection(GraphView.EntryNode, stateNode, false);
            }

            if (node is INamedItem namedNode)
            {
                RenameEditor.Open(namedNode.Name, newName => namedNode.Name = newName);
            }

            return true;
        }

        #endregion Search Tree
    }
}
