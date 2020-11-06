using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionNodeUI : StateMachineBaseNodeUI, ITransitionConnectable
    {
        public EnumField DurationTypeField { get; private set; }
        public FloatField DurationField { get; private set; }
        public EnumField OffsetTypeField { get; private set; }
        public FloatField OffsetField { get; private set; }
        public EnumField InterruptionSourceField { get; private set; }
        public Toggle OrderedInterruptionToggle { get; private set; }
        public Toggle InterruptableByAnyStateToggle { get; private set; }
        public Toggle PlayAfterTransitionToggle { get; private set; }

        public List<TransitionConditionInputNodePort> ConditionPorts { get; private set; } = new List<TransitionConditionInputNodePort>();

        private static readonly Color _portColor = new Color(252 / 255f, 65 / 255f, 3 / 255f);

        public List<TransitionConnectionUI> EntryConnections { get; private set; } = new List<TransitionConnectionUI>();
        public List<TransitionConnectionUI> ExitConnections { get; private set; } = new List<TransitionConnectionUI>();

        public event Action<StateNodeUI, StateNodeUI> OnDestinationChanged;

        public TransitionNodeUI()
        {
            title = "Transition";

            Button addConditionButton = new Button(() =>
            {
                AddCondition();
            })
            { text = "+ Condition" };
            inputContainer.Add(addConditionButton);

            DurationTypeField = new EnumField("Duration Type", DurationType.Fixed);
            DurationTypeField.RegisterValueChangedCallback(e => DurationField.label = (DurationType)e.newValue == DurationType.Fixed ? "Duration (s)" : "Duration (%)");
            extensionContainer.Add(DurationTypeField);

            DurationField = new FloatField("Duration (s)");
            extensionContainer.Add(DurationField);

            OffsetTypeField = new EnumField("Offset Type", DurationType.Fixed);
            OffsetTypeField.RegisterValueChangedCallback(e => OffsetField.label = (DurationType)e.newValue == DurationType.Fixed ? "Offset (s)" : "Offset (%)");
            extensionContainer.Add(OffsetTypeField);

            OffsetField = new FloatField("Offset (s)");
            extensionContainer.Add(OffsetField);

            InterruptionSourceField = new EnumField("Interruption Source", TransitionInterruptionSource.None);
            InterruptionSourceField.RegisterValueChangedCallback(e =>
            {
                TransitionInterruptionSource interruptionSource = (TransitionInterruptionSource)e.newValue;

                if (interruptionSource == TransitionInterruptionSource.None || interruptionSource == TransitionInterruptionSource.NextState)
                {
                    OrderedInterruptionToggle.style.display = DisplayStyle.None;
                }
                else
                {
                    OrderedInterruptionToggle.style.display = DisplayStyle.Flex;
                }
            });
            extensionContainer.Add(InterruptionSourceField);

            OrderedInterruptionToggle = new Toggle("Ordered Interruption");
            OrderedInterruptionToggle.style.display = DisplayStyle.None;
            extensionContainer.Add(OrderedInterruptionToggle);

            InterruptableByAnyStateToggle = new Toggle("Interruptable By Any State");
            extensionContainer.Add(InterruptableByAnyStateToggle);

            PlayAfterTransitionToggle = new Toggle("Play After Transition");
            extensionContainer.Add(PlayAfterTransitionToggle);

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseEnterEvent>(e =>
            {
                if (StateMachineEditor.Editor.TransitionConnection != null && StateMachineEditor.Editor.TransitionConnectionSource is IStateNode stateNode && (EntryConnections.Count == 0 || stateNode != EntryConnections[0].Source) && (ExitConnections.Count == 0 || stateNode != ExitConnections[0].Destination))
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
                var refreshedConnections = new HashSet<TransitionConnectionUI>();

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

        private TransitionConditionInputNodePort AddCondition()
        {
            TransitionConditionInputNodePort conditionPort = new TransitionConditionInputNodePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float)) { portColor = _portColor };

            MakePortRemovable(conditionPort, () => ConditionPorts.Remove(conditionPort));

            inputContainer.Add(conditionPort);

            ConditionPorts.Add(conditionPort);

            RefreshExpandedState();
            RefreshPorts();

            return conditionPort;
        }

        public void LoadData(GraphView graphView, Transition transition, Dictionary<State, StateNodeUI> stateMap, Dictionary<AnyState, AnyStateNodeUI> anyStateMap, Dictionary<IValueProvider, ValueProviderOutputNodePort> valueProviderMap)
        {
            if (transition.SourceState != null)
            {
                ITransitionConnectable source = transition.SourceState is AnyState anyState ? (ITransitionConnectable)anyStateMap[anyState] : stateMap[transition.SourceState];
                var connection = StateMachineEditor.Editor.CreateTransitionConnection(source, this);
                source.ExitConnections.Add(connection);
                EntryConnections.Add(connection);
                connection.schedule.Execute(() => connection.Refresh());
            }

            if (transition.DestinationState != null)
            {
                var connection = StateMachineEditor.Editor.CreateTransitionConnection(this, stateMap[transition.DestinationState]);
                stateMap[transition.DestinationState].EntryConnections.Add(connection);
                ExitConnections.Add(connection);
                connection.schedule.Execute(() => connection.Refresh());
            }

            if (EntryConnections.Count > 0)
            {
                EntryConnections[0].Source.OnExitConnect(EntryConnections[0]);
            }

            DurationTypeField.value = transition.DurationType;
            DurationField.SetValueWithoutNotify(transition.Duration);
            OffsetTypeField.value = transition.OffsetType;
            OffsetField.SetValueWithoutNotify(transition.Offset);
            InterruptionSourceField.SetValueWithoutNotify(transition.InterruptionSource);
            OrderedInterruptionToggle.value = transition.OrderedInterruption;
            InterruptableByAnyStateToggle.SetValueWithoutNotify(transition.InterruptableByAnyState);
            PlayAfterTransitionToggle.SetValueWithoutNotify(transition.PlayAfterTransition);

            transition.Conditions.ForEach(c =>
            {
                var conditionPort = AddCondition();

                if (c.ValueProvider != null)
                {
                    Edge edge = new Edge { output = valueProviderMap[c.ValueProvider], input = conditionPort };
                    edge.output.Connect(edge);
                    edge.input.Connect(edge);
                    graphView.AddElement(edge);

                    switch (c.ValueProvider)
                    {
                        case BoolProvider boolProvider:
                            conditionPort.BoolComparisonValueField.SetValueWithoutNotify(((BoolConditionEvaluator)c.Evaluator).ComparisonValue);
                            break;
                        case IntProvider intProvider:
                            var intEvaluator = (IntConditionEvaluator)c.Evaluator;
                            conditionPort.IntComparisonField.SetValueWithoutNotify(intEvaluator.Comparison);
                            conditionPort.IntComparisonValueField.SetValueWithoutNotify(intEvaluator.ComparisonValue);
                            break;
                        case FloatProvider floatProvider:
                            var floatEvaluator = (FloatConditionEvaluator)c.Evaluator;
                            conditionPort.FloatComparisonField.SetValueWithoutNotify(floatEvaluator.Comparison);
                            conditionPort.FloatComparisonValueField.SetValueWithoutNotify(floatEvaluator.ComparisonValue);
                            break;
                        default:
                            break;
                    }
                }
            });
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
            if (EntryConnections.Count > 1)
            {
                EntryConnections[0].Delete();
                EntryConnections.RemoveAt(0);
            }
        }

        public void OnExitConnect(TransitionConnectionUI connection)
        {
            var previousDestination = (StateNodeUI)ExitConnections[0].Destination;

            if (ExitConnections.Count > 1)
            {
                ExitConnections[0].Delete();
                ExitConnections.RemoveAt(0);
            }

            OnDestinationChanged?.Invoke(previousDestination, (StateNodeUI)connection.Destination);
        }

        public void OnEntryConnectionDeleted(TransitionConnectionUI connection)
        {

        }

        public void OnExitConnectionDeleted(TransitionConnectionUI connection)
        {
            OnDestinationChanged?.Invoke(ExitConnections.Count > 0 ? (StateNodeUI)ExitConnections[0].Destination : null, null);
        }
    }
}
