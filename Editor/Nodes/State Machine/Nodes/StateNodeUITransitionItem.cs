using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class StateNodeUITransitionItem : VisualElement
    {
        private TransitionConnectionUI _transition;

        public Label DestinationLabel { get; private set; }

        public StateNodeUITransitionItem(TransitionConnectionUI transition)
        {
            StateNodeUI destinationState = (StateNodeUI)transition.Destination;

            DestinationLabel = new Label(destinationState.Name);
            DestinationLabel.pickingMode = PickingMode.Ignore;

            var moveUpButton = new Button(MoveUp) { text = "▲" };
            moveUpButton.AddToClassList("move-up");

            var moveDownButton = new Button(MoveDown) { text = "▼" };
            moveDownButton.AddToClassList("move-down");

            Add(DestinationLabel);
            Add(moveUpButton);
            Add(moveDownButton);

            _transition = transition;

            destinationState.OnNameChanged += ChangeDestinationLabel;

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount < 2) { return; }

                schedule.Execute(() =>
                {
                    StateMachineEditor.Editor.GraphView.ClearSelection();
                    StateMachineEditor.Editor.GraphView.AddToSelection((StateNodeUI)_transition.Destination);
                    StateMachineEditor.Editor.GraphView.FrameSelection();
                });
            });
        }

        private void ChangeDestinationLabel(string newDestinationName) => DestinationLabel.text = newDestinationName;

        public void RemoveChangeDestinationLabelListener()
        {
            ((StateNodeUI)_transition.Destination).OnNameChanged -= ChangeDestinationLabel;
        }

        private void MoveUp()
        {
            RemoveFromClassList("last");
            
            int newIndex = parent.IndexOf(this) - 1;
            var state = (IStateNode)_transition.Source;
            state.ConnectionToTransitionItemMap.Move(newIndex + 1, newIndex);
            //var connection = state.ExitConnections[newIndex + 1];
            //state.ExitConnections.RemoveAt(newIndex + 1);
            //state.ExitConnections.Insert(newIndex, connection);

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
            var state = (IStateNode)_transition.Source;
            //var connection = state.ExitConnections[newIndex - 1];
            //state.ExitConnections.RemoveAt(newIndex - 1);

            if (newIndex == parent.childCount - 1)
            {
                parent.ElementAt(newIndex).RemoveFromClassList("last");

                parent.Add(this);
                //state.ExitConnections.Add(connection);

                AddToClassList("last");
            }
            else
            {
                parent.Insert(newIndex, this);
                //state.ExitConnections.Insert(newIndex, connection);
            }

            state.ConnectionToTransitionItemMap.Move(newIndex - 1, newIndex);

            if (newIndex - 1 == 0)
            {
                parent.ElementAt(0).AddToClassList("first");
            }
        }
    }
}
