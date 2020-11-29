using GZ.Tools.UnityUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public abstract class NamedStateMachineBaseNodeUI<T> : StateMachineBaseNodeUI, INamedItem<T> where T: NamedStateMachineBaseNodeUI<T>, INamedItem<T>
    {
        public NamedItemsGroup<T> Group { get; set; }

        public string Name
        {
            get => NameLabel.text;
            set
            {
                if (Group == null)
                {
                    NameLabel.text = value;
                }
                else
                {
                    Group.RenameItem((T)this, value);
                }
            }
        }

        public Label NameLabel { get; private set; }
        public Button EditNameButton { get; private set; }

        public NamedStateMachineBaseNodeUI() : base()
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("named-node__title-container");

            Label titleLabel = (Label)titleContainer[0];

            titleContainer.RemoveAt(0);
            titleContainer.Insert(0, container);

            container.Add(titleLabel);

            VisualElement nameContainer = new VisualElement();
            nameContainer.AddToClassList("named-node__name-container");
            nameContainer.style.flexDirection = FlexDirection.Row;
            nameContainer.style.flexGrow = 1f;

            container.Add(nameContainer);

            NameLabel = new Label($"New {Title}");
            NameLabel.AddToClassList("named-node__name");
            nameContainer.Add(NameLabel);

            EditNameButton = new Button(() => RenameEditor.Open(Name, newName => Name = newName))
            {
                text = "Edit"
            };
            EditNameButton.AddToClassList("named-node__edit-name-button");
            nameContainer.Add(EditNameButton);
        }

        public void SetNameWithoutNotify(string newName) => NameLabel.text = newName;
    }
}
