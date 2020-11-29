using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnyStateFilterVisualItem : VisualElement
    {
        private AnyStateFilterItem _filter;

        private NamedItemFinder<StateNodeUI> _stateFinder;
        private EnumField _filterModeField;

        public AnyStateFilterVisualItem()
        {
            style.flexDirection = FlexDirection.Row;
            AddToClassList("any-state__filters__item");

            _stateFinder = new NamedItemFinder<StateNodeUI>(StateMachineEditor.Editor.States);
            _stateFinder.OnItemSelected += (previousState, state, index) =>
            {
                _filter.State = state;
            };

            _filterModeField = new EnumField(AnyStateFilterMode.CurrentAndNextState);
            _filterModeField.RegisterValueChangedCallback(e => _filter.Mode = (AnyStateFilterMode)e.newValue);

            Add(_stateFinder);
            Add(_filterModeField);
        }

        public void Bind(AnyStateFilterItem filter)
        {
            _filter = filter;

            _stateFinder.SelectItemWithoutNotify(filter.State, true);
            _filterModeField.SetValueWithoutNotify(filter.Mode);
        }
    }
}
