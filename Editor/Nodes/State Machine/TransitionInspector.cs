using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionInspector : Foldout
    {
        private TransitionConnectionUI _connection;

        private ScrollView _scrollView;

        private ResizableListView _transitions;
        private TransitionInfo _selectedTransition;

        private EnumField DurationTypeField;
        private FloatField DurationField;
        private EnumField OffsetTypeField;
        private FloatField OffsetField;
        private EnumField InterruptionSourceField;
        private Toggle OrderedInterruptionToggle;
        private Toggle InterruptableByAnyStateToggle;
        private Toggle PlayAfterTransitionToggle;

        private ResizableListView _conditions;

        public TransitionInspector()
        {
            text = "Transition";

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("content");
            Add(_scrollView);

            Foldout transitionsFoldout = new Foldout() { value = true, text = "Transitions" };
            transitionsFoldout.RegisterValueChangedCallback(e => e.StopPropagation());
            _scrollView.Add(transitionsFoldout);

            _transitions = new ResizableListView();
            _transitions.name = "Transitions";
            _transitions.itemHeight = 20;
            _transitions.makeItem = () =>
            {
                VisualElement container = new VisualElement();

                Label label = new Label() { pickingMode = PickingMode.Ignore };
                Button removeButton = new Button() { text = "X" };

                container.Add(label);
                container.Add(removeButton);

                return container;
            };
            _transitions.bindItem = (item, index) =>
            {
                void Remove()
                {
                    _connection.RemoveTransition(index);
                }

                item.Q<Label>().text = $"{((IStateNode)_connection.Source).Name} >>> {((StateNodeUI)_connection.Destination).Name}";
                Button removeButton = item.Q<Button>();
                removeButton.clicked -= Remove;
                removeButton.clicked += Remove;
            };
            _transitions.onSelectionChange += selection =>
            {
                _selectedTransition = (TransitionInfo)selection.First();
                BindTransitionFields(_selectedTransition);
            };
            _transitions.selectionType = SelectionType.Single;
            _transitions.reorderable = true;

            transitionsFoldout.Add(_transitions);

            DurationTypeField = new EnumField("Duration Type", DurationType.Fixed);
            DurationTypeField.RegisterValueChangedCallback(e =>
            {
                _selectedTransition.DurationType = (DurationType)e.newValue;
                DurationField.label = (DurationType)e.newValue == DurationType.Fixed ? "Duration (s)" : "Duration (%)";
            });
            _scrollView.Add(DurationTypeField);

            DurationField = new FloatField("Duration (s)");
            DurationField.RegisterValueChangedCallback(e => _selectedTransition.Duration = e.newValue);
            _scrollView.Add(DurationField);

            OffsetTypeField = new EnumField("Offset Type", DurationType.Fixed);
            OffsetTypeField.RegisterValueChangedCallback(e =>
            {
                _selectedTransition.OffsetType = (DurationType)e.newValue;
                OffsetField.label = (DurationType)e.newValue == DurationType.Fixed ? "Offset (s)" : "Offset (%)";
            });
            _scrollView.Add(OffsetTypeField);

            OffsetField = new FloatField("Offset (s)");
            OffsetField.RegisterValueChangedCallback(e => _selectedTransition.Offset = e.newValue);
            _scrollView.Add(OffsetField);

            InterruptionSourceField = new EnumField("Interruption Source", TransitionInterruptionSource.None);
            InterruptionSourceField.RegisterValueChangedCallback(e =>
            {
                TransitionInterruptionSource interruptionSource = (TransitionInterruptionSource)e.newValue;
                _selectedTransition.InterruptionSource = interruptionSource;

                if (interruptionSource == TransitionInterruptionSource.None || interruptionSource == TransitionInterruptionSource.NextState)
                {
                    OrderedInterruptionToggle.style.display = DisplayStyle.None;
                }
                else
                {
                    OrderedInterruptionToggle.style.display = DisplayStyle.Flex;
                }
            });
            _scrollView.Add(InterruptionSourceField);

            OrderedInterruptionToggle = new Toggle("Ordered Interruption");
            OrderedInterruptionToggle.RegisterValueChangedCallback(e =>
            {
                _selectedTransition.OrderedInterruption = e.newValue;
                e.StopPropagation();
            });
            OrderedInterruptionToggle.style.display = DisplayStyle.None;
            _scrollView.Add(OrderedInterruptionToggle);

            InterruptableByAnyStateToggle = new Toggle("Interruptable By Any State");
            InterruptableByAnyStateToggle.RegisterValueChangedCallback(e =>
            {
                _selectedTransition.InterruptableByAnyState = e.newValue;
                e.StopPropagation();
            });
            _scrollView.Add(InterruptableByAnyStateToggle);

            PlayAfterTransitionToggle = new Toggle("Play After Transition");
            PlayAfterTransitionToggle.RegisterValueChangedCallback(e =>
            {
                _selectedTransition.PlayAfterTransition = e.newValue;
                e.StopPropagation();
            });
            _scrollView.Add(PlayAfterTransitionToggle);

            Foldout conditionsFoldout = new Foldout() { value = true, text = "Conditions" };
            conditionsFoldout.RegisterValueChangedCallback(e => e.StopPropagation());
            _scrollView.Add(conditionsFoldout);

            VisualElement conditionListButtons = new VisualElement();
            conditionListButtons.style.flexDirection = FlexDirection.Row;
            conditionsFoldout.Add(conditionListButtons);

            Button removeConditionButton = new Button(() =>
            {
                if (_conditions.selectedItem == null) { return; }

                int firstIndex = _conditions.selectedIndex;
                int indexFixer = 0;

                foreach (var index in _conditions.selectedIndices)
                {
                    _selectedTransition.Conditions.RemoveAt(index - indexFixer);
                    indexFixer++;
                }

                _conditions.Refresh();
                _conditions.selectedIndex = Mathf.Min(firstIndex, _selectedTransition.Conditions.Count - 1);
            })
            { text = "Remove Condition" };
            conditionListButtons.Add(removeConditionButton);

            Button addConditionButton = new Button(() =>
            {
                _selectedTransition.Conditions.Add(new TransitionInfoCondition());
                _conditions.Refresh();
            })
            { text = "Add Condition" };
            conditionListButtons.Add(addConditionButton);

            _conditions = new ResizableListView();
            _conditions.name = "Conditions";
            _conditions.itemHeight = 25;
            _conditions.makeItem = () =>
            {
                TransitionConditionUI transitionCondition = new TransitionConditionUI();
                
                return transitionCondition;
            };
            _conditions.bindItem = (item, index) =>
            {
                ((TransitionConditionUI)item).Bind(_selectedTransition.Conditions[index]);
            };
            _conditions.selectionType = SelectionType.Multiple;
            _conditions.reorderable = true;
            conditionsFoldout.Add(_conditions);

            RegisterCallback<ChangeEvent<bool>>(e => ToggleInClassList("expanded"));
        }

        public void Show(TransitionConnectionUI connection)
        {
            UnbindConnectionCallbacks(_connection);

            _connection = connection;
            _selectedTransition = _connection.Transitions[0];

            BindTransitionFields(_selectedTransition);
            BindConnectionCallbacks(_connection);

            _transitions.itemsSource = _connection.Transitions;
            _transitions.selectedIndex = 0;
            _transitions.Refresh();
            style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            UnbindConnectionCallbacks(_connection);

            _connection = null;
            _selectedTransition = null;
            style.display = DisplayStyle.None;
        }

        public void SelectTransition(TransitionInfo transitionInfo)
        {
            _transitions.selectedIndex = _connection.Transitions.IndexOf(transitionInfo);
        }

        private void BindTransitionFields(TransitionInfo transition)
        {
            DurationTypeField.SetValueWithoutNotify(transition.DurationType);
            DurationField.SetValueWithoutNotify(transition.Duration);
            OffsetTypeField.SetValueWithoutNotify(transition.OffsetType);
            OffsetField.SetValueWithoutNotify(transition.Offset);
            InterruptionSourceField.SetValueWithoutNotify(transition.InterruptionSource);
            OrderedInterruptionToggle.SetValueWithoutNotify(transition.OrderedInterruption);
            InterruptableByAnyStateToggle.SetValueWithoutNotify(transition.InterruptableByAnyState);
            PlayAfterTransitionToggle.SetValueWithoutNotify(transition.PlayAfterTransition);
            _conditions.itemsSource = transition.Conditions;
            _conditions.selectedIndex = -1;
            _conditions.Refresh();
        }
        private void BindConnectionCallbacks(TransitionConnectionUI connection)
        {
            if (connection != null)
            {
                connection.OnCreatedTransition += CreatedTransition;
                connection.OnRemovedTransition += RemovedTransition;
            }
        }

        private void UnbindConnectionCallbacks(TransitionConnectionUI connection)
        {
            if (connection != null)
            {
                connection.OnCreatedTransition -= CreatedTransition;
                connection.OnRemovedTransition -= RemovedTransition;
            }
        }

        void CreatedTransition(TransitionConnectionUI transitionConnection, TransitionInfo transitionInfo)
        {
            _transitions.Refresh();
            _transitions.selectedIndex = transitionConnection.Transitions.Count - 1;
        }

        void RemovedTransition(TransitionConnectionUI transitionConnection, TransitionInfo transitionInfo, int index)
        {
            _transitions.Refresh();
            _transitions.selectedIndex = index == transitionConnection.Transitions.Count ? index - 1 : index;
        }
    }
}
