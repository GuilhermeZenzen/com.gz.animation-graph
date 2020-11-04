using GZ.AnimationGraph;
using GZ.AnimationGraph.Editor;
using GZ.Tools.UnityUtility;
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

        public TransitionConnectionUI TransitionConnection { get; set; }
        public ITransitionConnectable TransitionConnectionSource { get; set; }
        public ITransitionConnectable TransitionConnectionTarget { get; set; }

        public static void OpenEditor()
        {
            Editor = GetWindow<StateMachineEditor>();
            Editor.titleContent = new GUIContent("State Machine");
        }

        private void OnEnable()
        {
            string resourcesPath = $"{AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)).Replace($"/{nameof(StateMachineEditor)}.cs", "")}/Resources";
            _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{resourcesPath}/{_ussPath}");

            _toolbar = new Toolbar();

            _saveButton = new ToolbarButton(SaveStateMachine) { text = "Save" };

            _toolbar.Add(_saveButton);

            rootVisualElement.Add(_toolbar);

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

        private void OnDisable()
        {
            //Editor = null;
            //IsClosing = true;

            //rootVisualElement.Remove(GraphView);
            //rootVisualElement.Remove(_toolbar);
            //_styleSheet = null;

            //IsClosing = false;
        }

        private void CreateGraphView()
        {
            if (GraphView != null) { rootVisualElement.Remove(GraphView); }

            GraphView = new StateMachineGraphView
            {
                name = "State Machine Graph"
            };
            GraphView.styleSheets.Add(_styleSheet);

            GraphView.StretchToParentSize();

            rootVisualElement.Insert(0, GraphView);

            GraphView.RegisterCallback<MouseMoveEvent>(UpdateTransitionConnection);
            GraphView.RegisterCallback<MouseUpEvent>(e =>
            {
                FinishTransitionConnection(e.button == 0);
            });
            GraphView.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    FinishTransitionConnection();
                }
            });
        }

        public TransitionConnectionUI CreateTransitionConnection(ITransitionConnectable source, ITransitionConnectable destination = null, bool enableContextualMenu = true)
        {
            var connection = new TransitionConnectionUI(enableContextualMenu) { Source = source, Destination = destination };
            GraphView.AddElement(connection);
            connection.SendToBack();

            return connection;
        }

        public void AddTransitionConnection(ITransitionConnectable connectable)
        {
            var connection = CreateTransitionConnection(connectable, null, false);
            connection.style.top = connectable.resolvedStyle.top + connectable.resolvedStyle.height / 2;
            connection.style.left = connectable.resolvedStyle.left + connectable.resolvedStyle.width / 2;
            TransitionConnection = connection;
            TransitionConnectionSource = connectable;

            if (_panSchedule == null)
            {
                _panSchedule = GraphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
                _panSchedule.Pause();
            }
        }

        private void UpdateTransitionConnection(MouseMoveEvent e)
        {
            if (TransitionConnection == null) { return; }

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
            UpdateTransitionConnectionVisual(e.localMousePosition);
        }

        private void UpdateTransitionConnectionVisual(Vector2 localMousePosition)
        {
            TransitionConnection.end = TransitionConnectionTarget != null
                ? new Vector3(TransitionConnectionTarget.resolvedStyle.left + TransitionConnectionTarget.resolvedStyle.width / 2 - TransitionConnection.resolvedStyle.left, TransitionConnectionTarget.resolvedStyle.top + TransitionConnectionTarget.resolvedStyle.height / 2 - TransitionConnection.resolvedStyle.top)
                : new Vector3(localMousePosition.x - TransitionConnection.worldBound.x, localMousePosition.y - TransitionConnection.worldBound.y + _toolbar.resolvedStyle.height) / GraphView.scale;
            TransitionConnection.style.width = TransitionConnection.end.x;
            TransitionConnection.style.height = TransitionConnection.end.y;
            TransitionConnection.MarkDirtyRepaint();
        }

        private void FinishTransitionConnection(bool canConnect = true)
        {
            if (TransitionConnection == null) { return; }

            _panSchedule.Pause();

            if (TransitionConnectionTarget == null || !canConnect)
            {
                GraphView.RemoveElement(TransitionConnection);
            }
            else
            {
                TransitionConnection.EnableContextualMenu();

                TransitionConnection.Source = TransitionConnectionSource;
                TransitionConnection.Destination = TransitionConnectionTarget;
                TransitionConnectionSource.ExitConnections.Add(TransitionConnection);
                TransitionConnectionTarget.EntryConnections.Add(TransitionConnection);

                TransitionConnectionSource.OnExitConnect(TransitionConnection);
                TransitionConnectionTarget.OnEntryConnect(TransitionConnection);

                TransitionConnection.Refresh();
            }

            TransitionConnection = null;
            TransitionConnectionSource = null;
            TransitionConnectionTarget = null;
        }

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

            UpdateTransitionConnectionVisual(GraphView.ChangeCoordinatesTo(GraphView.contentContainer, _localMousePosition));
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

        private void SaveStateMachine()
        {
            var stateMachine = new StateMachineNode();
            NodeUI.StateMachineNodeAsset = new StateMachineNodeAsset { Data = stateMachine };

            var parameterMap = new Dictionary<ParameterNodeUI, Parameter>();
            var valueProviderMap = new Dictionary<ValueProviderOutputNodePort, IValueProvider>();
            var stateMap = new Dictionary<StateNodeUI, State>();
            var anyStateMap = new Dictionary<AnyStateNodeUI, AnyState>();
            var transitionMap = new Dictionary<TransitionNodeUI, Transition>();

            GraphView.Parameters.ForEach(p =>
            {
                var parameter = stateMachine.AddParameter(p.NameField.value);
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
                valueProviderMap.Add(p.OutputPort, parameter.ValueProvider);
            });

            GraphView.Transitions.ForEach(t =>
            {
                var transition = new Transition() { Duration = t.DurationField.value, Offset = t.OffsetField.value, InterruptionSource = (TransitionInterruptionSource)t.InterruptionSourceField.value, OrderedInterruption = t.OrderedInterruptionToggle.value, InterruptableByAnyState = t.InterruptableByAnyStateToggle.value, PlayAfterTransition = t.PlayAfterTransitionToggle.value };

                //t.ConditionPorts.ForEach(c =>
                //{
                //    TransitionCondition condition = new TransitionCondition();

                //    var edges = c.connections.ToList();

                //    if (edges.Count > 0)
                //    {
                //        condition.SetValueProvider(valueProviderMap[(ValueProviderOutputNodePort)edges[0].output]);

                //        switch (condition.ValueProvider)
                //        {
                //            case BoolProvider boolProvider:
                //                ((BoolConditionEvaluator)condition.Evaluator).ComparisonValue = (Bool)c.BoolComparisonValueField.value;
                //                break;
                //            case IntProvider intProvider:
                //                var intEvaluator = (IntConditionEvaluator)condition.Evaluator;
                //                intEvaluator.Comparison = (IntComparison)c.IntComparisonField.value;
                //                intEvaluator.ComparisonValue = c.IntComparisonValueField.value;
                //                break;
                //            case FloatProvider floatProvider:
                //                var floatEvaluator = (FloatConditionEvaluator)condition.Evaluator;
                //                floatEvaluator.Comparison = (FloatComparison)c.FloatComparisonField.value;
                //                floatEvaluator.ComparisonValue = c.FloatComparisonValueField.value;
                //                break;
                //            default:
                //                break;
                //        }
                //    }

                //    transition.Conditions.Add(condition);
                //});

                stateMachine.Transitions.Add(transition);
                NodeUI.StateMachineNodeAsset.TransitionMap.Add(transition.Id, new NodeVisualInfo(t.GetPosition().position, t.expanded));
                transitionMap.Add(t, transition);
            });

            GraphView.States.ForEach(s =>
            {
                var state = stateMachine.AddState(s.NameField.value);

                s.EntryConnections.ForEach(c =>
                {
                    if (!(c.Source is TransitionNodeUI transitionNode)) { return; }

                    Transition transition = transitionMap[transitionNode];
                    transition.DestinationState = state;
                    state.EntryTransitions.Add(transition);
                });

                s.ExitConnections.ForEach(c =>
                {
                    Transition transition = transitionMap[(TransitionNodeUI)c.Destination];
                    transition.SourceState = state;
                    state.ExitTransitions.Add(transition);
                });

                NodeUI.StateMachineNodeAsset.StateMap.Add(state.Id, new NodeVisualInfo(s.GetPosition().position, s.expanded));
                stateMap.Add(s, state);

                valueProviderMap.Add(s.TimeOutputPort, state.Time);
                valueProviderMap.Add(s.PreviousTimeOutputPort, state.PreviousTime);
                valueProviderMap.Add(s.NormalizedTimeOutputPort, state.NormalizedTime);
                valueProviderMap.Add(s.PreviousNormalizedTimeOutputPort, state.PreviousNormalizedTime);
            });

            GraphView.Transitions.ForEach(t =>
            {
                t.ConditionPorts.ForEach(c =>
                {
                    TransitionCondition condition = new TransitionCondition();

                    var edges = c.connections.ToList();

                    if (edges.Count > 0)
                    {
                        condition.SetValueProvider(valueProviderMap[(ValueProviderOutputNodePort)edges[0].output]);

                        switch (condition.ValueProvider)
                        {
                            case BoolProvider boolProvider:
                                ((BoolConditionEvaluator)condition.Evaluator).ComparisonValue = (Bool)c.BoolComparisonValueField.value;
                                break;
                            case IntProvider intProvider:
                                var intEvaluator = (IntConditionEvaluator)condition.Evaluator;
                                intEvaluator.Comparison = (IntComparison)c.IntComparisonField.value;
                                intEvaluator.ComparisonValue = c.IntComparisonValueField.value;
                                break;
                            case FloatProvider floatProvider:
                                var floatEvaluator = (FloatConditionEvaluator)condition.Evaluator;
                                floatEvaluator.Comparison = (FloatComparison)c.FloatComparisonField.value;
                                floatEvaluator.ComparisonValue = c.FloatComparisonValueField.value;
                                break;
                            default:
                                break;
                        }
                    }

                    transitionMap[t].Conditions.Add(condition);
                });
            });

            HashSet<AnyStateNodeUI> alreadyAddedAnyStates = new HashSet<AnyStateNodeUI>();

            if (GraphView.AnyStatePriorityManager == null)
            {
                NodeUI.StateMachineNodeAsset.AnyStatePriorityManager = null;
            }
            else
            {
                NodeUI.StateMachineNodeAsset.AnyStatePriorityManager = new NodeVisualInfo(GraphView.AnyStatePriorityManager.GetPosition().position, GraphView.AnyStatePriorityManager.expanded);

                foreach (var element in GraphView.AnyStatePriorityManager.inputContainer.Children())
                {
                    if (!(element is Port port)) { continue; }

                    var edges = port.connections.ToList();

                    if (edges.Count > 0)
                    {
                        var anyStateNode = (AnyStateNodeUI)edges[0].output.node;
                        var anyState = anyStateNode.GenerateData(stateMap);

                        anyStateNode.ExitConnections.ForEach(c =>
                        {
                            Transition transition = transitionMap[(TransitionNodeUI)c.Destination];
                            transition.SourceState = anyState;
                            anyState.ExitTransitions.Add(transition);
                        });

                        stateMachine.AnyStates.Add(anyState);
                        NodeUI.StateMachineNodeAsset.AnyStateMap.Add(anyState.Id, new NodeVisualInfo(anyStateNode.GetPosition().position, anyStateNode.expanded));
                        alreadyAddedAnyStates.Add(anyStateNode);
                        anyStateMap.Add(anyStateNode, anyState);
                    }
                    else
                    {
                        stateMachine.AnyStates.Add(null);
                    }
                }
            }

            GraphView.AnyStates.ForEach(s =>
            {
                if (alreadyAddedAnyStates.Contains(s)) { return; }

                var anyState = s.GenerateData(stateMap);

                s.ExitConnections.ForEach(c =>
                {
                    Transition transition = transitionMap[(TransitionNodeUI)c.Destination];
                    transition.SourceState = anyState;
                    anyState.ExitTransitions.Add(transition);
                });

                stateMachine.AnyStates.Add(anyState);
                NodeUI.StateMachineNodeAsset.AnyStateMap.Add(anyState.Id, new NodeVisualInfo(s.GetPosition().position, s.expanded));
                anyStateMap.Add(s, anyState);
            });

            GraphView.EntryNode.GenerateData(stateMachine, stateMap);
            NodeUI.StateMachineNodeAsset.EntryState = new NodeVisualInfo(GraphView.EntryNode.GetPosition().position, GraphView.EntryNode.expanded);

            NodeUI.UpdateStatePorts();
        }

        public void LoadData(StateMachineNodeUI stateMachineUI)
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
            var valueProviderMap = new Dictionary<IValueProvider, ValueProviderOutputNodePort>();

            foreach (var parameter in stateMachine.Parameters.Values)
            {
                var parameterNode = new ParameterNodeUI();
                GraphView.AddNode(parameterNode);
                SetNodePositionAndExpansion(parameterNode, stateMachineUI.StateMachineNodeAsset.ParameterMap[parameter.Id]);
                parameterNode.LoadData(parameter);
                parameterMap.Add(parameter, parameterNode);
                valueProviderMap.Add(parameter.ValueProvider, parameterNode.OutputPort);
            }

            var stateMap = new Dictionary<State, StateNodeUI>();

            foreach (var state in stateMachine.States.Values)
            {
                var stateNode = new StateNodeUI();
                GraphView.AddNode(stateNode);
                SetNodePositionAndExpansion(stateNode, stateMachineUI.StateMachineNodeAsset.StateMap[state.Id]);
                stateNode.LoadData(state);
                stateMap.Add(state, stateNode);
                valueProviderMap.Add(state.Time, stateNode.TimeOutputPort);
                valueProviderMap.Add(state.PreviousTime, stateNode.PreviousTimeOutputPort);
                valueProviderMap.Add(state.NormalizedTime, stateNode.NormalizedTimeOutputPort);
                valueProviderMap.Add(state.PreviousNormalizedTime, stateNode.PreviousNormalizedTimeOutputPort);
            }

            var anyStateMap = new Dictionary<AnyState, AnyStateNodeUI>();

            foreach (var anyState in stateMachine.AnyStates)
            {
                if (anyState == null) { continue; }

                var anyStateNode = new AnyStateNodeUI();
                GraphView.AddNode(anyStateNode);
                SetNodePositionAndExpansion(anyStateNode, stateMachineUI.StateMachineNodeAsset.AnyStateMap[anyState.Id]);
                anyStateNode.LoadData(GraphView, anyState, stateMap);
                anyStateMap.Add(anyState, anyStateNode);
            }

            if (NodeUI.StateMachineNodeAsset.AnyStatePriorityManager != null)
            {
                var anyStatePriorityManagerNode = new AnyStatePriorityManagerNodeUI();
                GraphView.AddNode(anyStatePriorityManagerNode);
                SetNodePositionAndExpansion(anyStatePriorityManagerNode, stateMachineUI.StateMachineNodeAsset.AnyStatePriorityManager);
                anyStatePriorityManagerNode.LoadData(GraphView, stateMachine.AnyStates, anyStateMap);
            }

            var transitionMap = new Dictionary<Transition, TransitionNodeUI>();

            foreach (var transition in stateMachine.Transitions)
            {
                var transitionNode = new TransitionNodeUI();
                GraphView.AddNode(transitionNode);
                SetNodePositionAndExpansion(transitionNode, stateMachineUI.StateMachineNodeAsset.TransitionMap[transition.Id]);
                transitionNode.LoadData(GraphView, transition, stateMap, anyStateMap, valueProviderMap);
                transitionMap.Add(transition, transitionNode);
            }

            EntryNodeUI entryStateNode = new EntryNodeUI();
            GraphView.AddNode(entryStateNode);
            GraphView.EntryNode = entryStateNode;
            if (NodeUI.StateMachineNodeAsset.EntryState != null)
            {
                SetNodePositionAndExpansion(entryStateNode, NodeUI.StateMachineNodeAsset.EntryState);
            }
            entryStateNode.LoadData(GraphView, stateMachine.EntryState, stateMap);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeEntry(new GUIContent("State")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Any State")) { level = 1 },
                new SearchTreeEntry(new GUIContent("Transition")) { level = 1 },
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
                case "Transition":
                    node = new TransitionNodeUI();
                    break;
                case "Parameter":
                    node = new ParameterNodeUI();
                    break;
                default:
                    break;
            }

            GraphView.AddNode(node, context.screenMousePosition);

            return true;
        }
    }
}
