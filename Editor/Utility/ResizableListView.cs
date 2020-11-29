using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class ResizableListView : ListView
    {
        public ResizableListView() : base() { }
        public ResizableListView(IList itemsSource, int itemHeight, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem) 
            : base(itemsSource, itemHeight, makeItem, bindItem) { }

        public new void Refresh()
        {
            style.height = itemsSource.Count * itemHeight;
            base.Refresh();
        }
    }
}

