using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateNodeUITransitionItem : VisualElement
    {
        private TransitionNodeUI _transition;

        public Label DestinationLabel { get; private set; }

        public StateNodeUITransitionItem(TransitionNodeUI transition)
        {
            DestinationLabel = new Label(transition.ExitConnections.Count > 0 ? ((StateNodeUI)transition.ExitConnections[0].Destination).NameField.value : "---");
            DestinationLabel.pickingMode = PickingMode.Ignore;

            var moveUpButton = new Button(MoveUp) { text = "▲" };
            moveUpButton.AddToClassList("move-up");

            var moveDownButton = new Button(MoveDown) { text = "▼" };
            moveDownButton.AddToClassList("move-down");

            Add(DestinationLabel);
            Add(moveUpButton);
            Add(moveDownButton);

            _transition = transition;
            _transition.OnDestinationChanged += ChangeDestination;

            if (_transition.ExitConnections.Count > 0)
            {
                ((StateNodeUI)_transition.ExitConnections[0].Destination).OnNameChanged += ChangeDestinationLabel;
            }

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount < 2) { return; }

                schedule.Execute(() =>
                {
                    StateMachineEditor.Editor.GraphView.ClearSelection();
                    StateMachineEditor.Editor.GraphView.AddToSelection(_transition);
                    StateMachineEditor.Editor.GraphView.FrameSelection();
                });
            });
        }

        private void ChangeDestination(StateNodeUI previousDestination, StateNodeUI newDestination)
        {
            if (previousDestination != null) { previousDestination.OnNameChanged -= ChangeDestinationLabel; }

            DestinationLabel.text = newDestination != null ? newDestination.NameField.value : "---";

            if (newDestination != null) { newDestination.OnNameChanged += ChangeDestinationLabel; }
        }

        private void ChangeDestinationLabel(string newDestinationName) => DestinationLabel.text = newDestinationName;

        public void RemoveChangeDestinationLabelListener()
        {
            if (_transition.ExitConnections.Count > 0)
            {
                ((StateNodeUI)_transition.ExitConnections[0].Destination).OnNameChanged -= ChangeDestinationLabel;
            }

            _transition.OnDestinationChanged -= ChangeDestination;
        }

        private void MoveUp()
        {
            RemoveFromClassList("last");

            int newIndex = parent.IndexOf(this) - 1;
            var state = _transition.EntryConnections[0].Source;
            var connection = state.ExitConnections[newIndex + 1];
            state.ExitConnections.RemoveAt(newIndex + 1);
            state.ExitConnections.Insert(newIndex, connection);

            if (newIndex == 0)
            {
                parent.ElementAt(0).RemoveFromClassList("first");

                AddToClassList("first");
            }

            parent.Insert(newIndex, this);

            if (newIndex + 1 == parent.childCount - 1)
            {
                parent.ElementAt(parent.childCount - 1).AddToClassList("last");
            }
        }

        private void MoveDown()
        {
            RemoveFromClassList("first");

            int newIndex = parent.IndexOf(this) + 1;
            var state = _transition.EntryConnections[0].Source;
            var connection = state.ExitConnections[newIndex - 1];
            state.ExitConnections.RemoveAt(newIndex - 1);

            if (newIndex == parent.childCount - 1)
            {
                parent.ElementAt(newIndex).RemoveFromClassList("last");

                parent.Add(this);
                state.ExitConnections.Add(connection);

                AddToClassList("last");
            }
            else
            {
                parent.Insert(newIndex, this);
                state.ExitConnections.Insert(newIndex, connection);
            }

            if (newIndex - 1 == 0)
            {
                parent.ElementAt(0).AddToClassList("first");
            }
        }
    }
}
