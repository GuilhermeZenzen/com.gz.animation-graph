using GZ.Tools.UnityUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class NamedItemFinder<T> : ToolbarMenu where T: class, INamedItem<T>
    {
        public NamedItemsGroup<T> Group { get; private set; }

        public T Item { get; private set; }

        public event Action<T, T, int> OnItemSelected; 

        public NamedItemFinder(NamedItemsGroup<T> group)
        {
            Group = group;

            Group.OnItemAdded += ItemAdded;
            Group.OnItemRenamed += ItemRenamed;
            Group.OnItemRemoved += ItemRemoved;

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                Group.OnItemAdded -= ItemAdded;
                Group.OnItemRenamed -= ItemRenamed;
                Group.OnItemRemoved -= ItemRemoved;
            });

            if (Group.Items.Count > 0)
            {
                Item = Group.Items.At(0);

                UpdateSelectedItem();
                UpdateItems();
            }
        }

        public void SelectItemWithoutNotify(T item, bool selectDefaultIfNull = false)
        {
            if (selectDefaultIfNull && item == null && Group.Items.Count > 0)
            {
                SelectItem(Group.Items.At(0));
                return;
            }

            Item = item;
            UpdateSelectedItem();
            UpdateItems();
        }

        public void SelectItem(T item)
        {
            T previousItem = Item;
            Item = item;
            UpdateSelectedItem();
            UpdateItems();
            OnItemSelected?.Invoke(previousItem, item, Group.Items.Values.IndexOf(item));
        }

        public void SelectItem(T item, int index)
        {
            T previousItem = Item;
            Item = item;
            UpdateSelectedItem();
            UpdateItems();
            OnItemSelected?.Invoke(previousItem, item, index);
        }

        private void UpdateSelectedItem()
        {
            text = Item?.Name;
        }

        private void UpdateItems()
        {
            menu.MenuItems().Clear();

            for (int i = 0; i < Group.Items.Count; i++)
            {
                T item = Group.Items.At(i);
                int index = i;

                menu.AppendAction(item.Name, action =>
                {
                    SelectItem(item, index);
                }, item.Equals(Item) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
        }

        private void ItemAdded(T item)
        {
            if (Item == null)
            {
                Item = item;
                UpdateSelectedItem();
                OnItemSelected?.Invoke(default, Item, Group.Items.Count - 1);
            }

            UpdateItems();
        }

        private void ItemRenamed(T item, string previousName)
        {
            if (Item.Equals(item))
            {
                UpdateSelectedItem();
            }

            UpdateItems();
        }

        private void ItemRemoved(T item, int parameterIndex)
        {
            if (Item.Equals(item))
            {
                if (Group.Items.Count > 0)
                {
                    T previousItem = Item;
                    Item = Group.Items.At(Mathf.Min(parameterIndex, Group.Items.Count - 1));
                    OnItemSelected?.Invoke(previousItem, Item, parameterIndex);
                }
                else
                {
                    T previousItem = Item;
                    Item = default;
                    OnItemSelected?.Invoke(previousItem, Item, parameterIndex);
                }

                UpdateSelectedItem();
            }

            UpdateItems();
        }
    }
}
