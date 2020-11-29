using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnyStateNodeUI : BaseStateNodeUI<AnyStateNodeUI>
    {
        public override string Title => "Any State";

        public Port PriorityPort { get; private set; }

        private static readonly Color _portColor = new Color(180 / 255f, 40 / 255f, 255 / 255f);

        private List<AnyStateFilterItem> _filters = new List<AnyStateFilterItem>();

        //private ResizableListView _filterList;
        private NodeItemList _filterList;

        public AnyStateNodeUI() : base()
        {
            _filterList = new NodeItemList(
                "Filters",
                "Filter",
                _filters,
                20,
                container =>
                {
                    container.Add(new AnyStateFilterVisualItem());
                    container.AddToClassList("any-state__filters__item");
                },
                (container, index) => container.Q<AnyStateFilterVisualItem>().Bind(_filters[index]),
                () => new AnyStateFilterItem());
            _filterList.List.AddToClassList("any-state__filters");
            extensionContainer.Add(_filterList);
            //Foldout filtersFoldout = new Foldout() { value = false, text = "Filters" };
            //extensionContainer.Add(filtersFoldout);

            //VisualElement filterButtonsContainer = new VisualElement();
            //filterButtonsContainer.AddToClassList("any-state__filter-list-buttons");
            //filterButtonsContainer.style.flexDirection = FlexDirection.RowReverse;
            //filtersFoldout.Add(filterButtonsContainer);

            //Button addFilterButton = new Button(() =>
            //{
            //    _filters.Add(new AnyStateFilterItem());
            //    _filterList.Refresh();
            //}) { text = "Add Filter" };
            //filterButtonsContainer.Add(addFilterButton);

            //Button removeFilterButton = new Button(() =>
            //{
            //    if (_filterList.selectedItem == null) { return; }

            //    int firstIndex = _filterList.selectedIndex;
            //    int indexFixer = 0;

            //    foreach (int index in _filterList.selectedIndices)
            //    {
            //        _filters.RemoveAt(index - indexFixer);
            //        indexFixer++;
            //    }

            //    _filterList.selectedIndex = Mathf.Min(firstIndex, _filters.Count - 1);
            //    _filterList.Refresh();
            //}) { text = "Remove Filter" };
            //filterButtonsContainer.Add(removeFilterButton);

            //VisualElement filterListContainer = new VisualElement();
            //filterListContainer.RegisterCallback<MouseDownEvent>(e => e.StopPropagation());
            //filtersFoldout.Add(filterListContainer);

            //_filterList = new ResizableListView(_filters, 20, () => new AnyStateFilterVisualItem(), (filter, index) =>
            //{
            //    ((AnyStateFilterVisualItem)filter).Bind(_filters[index]);
            //})
            //{ reorderable = true, selectionType = SelectionType.Multiple };
            //_filterList.AddToClassList("any-state__filters");
            //filterListContainer.Add(_filterList);

            PriorityPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            PriorityPort.portName = "Priority";
            PriorityPort.portColor = _portColor;

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<GeometryChangedEvent>(e => ExitConnections.ForEach(c => c.Refresh()));

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (!StateMachineEditor.IsClosing)
                {
                    this.ClearConnections();
                }
            });
        }

        public void LoadData(GraphView graphView, AnyState anyState, Dictionary<State, StateNodeUI> map)
        {
            anyState.StateFilters.ForEach(filter =>
            {
                _filters.Add(new AnyStateFilterItem { State = filter.State == null ? null : map[filter.State], Mode = filter.Mode });
            });

            _filterList.List.Refresh();
        }

        public AnyState GenerateData(Dictionary<StateNodeUI, State> stateMap)
        {
            var anyState = new AnyState() { Name = Name };

            foreach (var filter in _filters)
            {
                anyState.StateFilters.Add(new AnyStateFilter { State = filter.State == null ? null : stateMap[filter.State], Mode = filter.Mode });
            }

            foreach (var element in outputContainer.Children())
            {
                if (!(element is Port port)) { continue; }

                anyState.ExitTransitions.Add(null);
            }

            return anyState;
        }
    }
}
