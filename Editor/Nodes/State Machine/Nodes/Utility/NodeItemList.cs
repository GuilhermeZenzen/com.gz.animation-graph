using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class NodeItemList : Foldout
    {
        public ResizableListView List { get; private set; }

        public NodeItemList(string title, string buttonWord, IList list, int itemHeight, Action<VisualElement> makeItem, Action<VisualElement, int> bindItem, Func<object> itemCreationCallback)
        {
            text = title;
            AddToClassList("node-item-list");

            VisualElement buttonsContainer = new VisualElement();
            buttonsContainer.AddToClassList("node-item-list__buttons-container");
            Add(buttonsContainer);

            Button addButton = new Button(() =>
            {
                List.itemsSource.Add(itemCreationCallback());
                List.Refresh();
            }) { text = $"Add {buttonWord}" };
            addButton.AddToClassList("node-item-list__add-button");
            buttonsContainer.Add(addButton);

            Button removeButton = new Button(() =>
            {
                if (List.selectedItem == null) { return; }

                int firstIndex = List.selectedIndex;
                int indexFixer = 0;

                foreach (int index in List.selectedIndices)
                {
                    List.itemsSource.RemoveAt(index - indexFixer);
                    indexFixer++;
                }

                List.selectedIndex = Mathf.Min(firstIndex, List.itemsSource.Count - 1);
                List.Refresh();
            }) { text = $"Remove {buttonWord}" };
            removeButton.AddToClassList("node-item-list__remove-button");
            buttonsContainer.Add(removeButton);

            VisualElement listContainer = new VisualElement();
            listContainer.AddToClassList("node-item-list__list-container");
            listContainer.RegisterCallback<MouseDownEvent>(e => e.StopPropagation());
            Add(listContainer);

            List = new ResizableListView(list, itemHeight, () =>
            {
                VisualElement container = new VisualElement();
                container.AddToClassList("node-item-list__item");

                makeItem(container);

                return container;
            }, bindItem);
            List.selectionType = SelectionType.Multiple;
            List.reorderable = true;
            List.AddToClassList("node-item-list__list");
            listContainer.Add(List);
        }
    }
}
