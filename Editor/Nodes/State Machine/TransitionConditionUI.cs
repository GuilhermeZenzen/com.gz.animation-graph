using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GZ.AnimationGraph;
using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionConditionUI : VisualElement
    {
        private TransitionInfoCondition _condition;

        private ValueProviderType _providerType = ValueProviderType.Bool;

        private Button _parameterStateSwitch;

        private NamedItemFinder<ParameterNodeUI> _parameterFinder;

        private NamedItemFinder<StateNodeUI> _stateFinder;
        private EnumField _stateValueProvider;

        private EnumField _boolComparisonValueField;

        private EnumField _intComparisonField;
        private IntegerField _intComparisonValueField;

        private EnumField _floatComparisonField;
        private FloatField _floatComparisonValueField;

        public TransitionConditionUI()
        {
            AddToClassList("transition-condition");

            style.flexDirection = FlexDirection.Row;

            _parameterStateSwitch = new Button(() =>
            {
                if (_condition.ProviderSourceType == ValueProviderSourceType.Parameter)
                {
                    _condition.ProviderSourceType = ValueProviderSourceType.State;
                }
                else
                {
                    _condition.ProviderSourceType = ValueProviderSourceType.Parameter;
                }

                SetProviderSourceType(_condition.ProviderSourceType);
            }) { text = "P" };
            Add(_parameterStateSwitch);

            _parameterFinder = new NamedItemFinder<ParameterNodeUI>(StateMachineEditor.Editor.Parameters);
            _parameterFinder.OnItemSelected += (previousParameter, parameter, index) =>
            {
                _condition.Parameter = parameter;

                if (previousParameter != null)
                {
                    previousParameter.OnTypeChanged -= ParameterTypeChanged;
                }

                if (parameter != null)
                {
                    SetProviderType(parameter.ParameterType);
                    parameter.OnTypeChanged += ParameterTypeChanged;
                }
                else
                {
                    ComparisonElementsEnabling(_providerType, false);
                }
            };
            Add(_parameterFinder);

            _stateFinder = new NamedItemFinder<StateNodeUI>(StateMachineEditor.Editor.States);
            _stateFinder.OnItemSelected += (previousState, state, index) =>
            {
                _condition.State = state;

                if (state != null)
                {
                    _stateValueProvider.style.display = DisplayStyle.Flex;
                    SetProviderType(ValueProviderType.Float);
                }
                else
                {
                    _stateValueProvider.style.display = DisplayStyle.None;
                    ComparisonElementsEnabling(_providerType, false);
                }
            };
            _stateFinder.style.display = DisplayStyle.None;
            Add(_stateFinder);

            _stateValueProvider = new EnumField(StateValueProviders.Time);
            _stateValueProvider.style.display = DisplayStyle.None;
            _stateValueProvider.RegisterValueChangedCallback(e => _condition.StateValueProvider = (StateValueProviders)e.newValue);
            Add(_stateValueProvider);

            _boolComparisonValueField = new EnumField(Bool.True);
            _boolComparisonValueField.style.display = DisplayStyle.None;
            _boolComparisonValueField.RegisterValueChangedCallback(e => _condition.BoolComparisonValue = (Bool)e.newValue == Bool.True);
            Add(_boolComparisonValueField);

            _intComparisonField = new EnumField(IntComparison.Equal);
            _intComparisonField.style.display = DisplayStyle.None;
            _intComparisonField.RegisterValueChangedCallback(e => _condition.IntComparison = (IntComparison)e.newValue);
            Add(_intComparisonField);

            _intComparisonValueField = new IntegerField();
            _intComparisonValueField.style.display = DisplayStyle.None;
            _intComparisonValueField.RegisterValueChangedCallback(e => _condition.IntComparisonValue = e.newValue);
            Add(_intComparisonValueField);

            _floatComparisonField = new EnumField(FloatComparison.BiggerOrEqual);
            _floatComparisonField.style.display = DisplayStyle.None;
            _floatComparisonField.RegisterValueChangedCallback(e => _condition.FloatComparison = (FloatComparison)e.newValue);
            Add(_floatComparisonField);

            _floatComparisonValueField = new FloatField();
            _floatComparisonValueField.style.display = DisplayStyle.None;
            _floatComparisonValueField.RegisterValueChangedCallback(e => _condition.FloatComparisonValue = e.newValue);
            Add(_floatComparisonValueField);

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (_parameterFinder.Item != null)
                {
                    _parameterFinder.Item.OnTypeChanged -= ParameterTypeChanged;
                }
            });
        }

        public void Bind(TransitionInfoCondition condition)
        {
            _condition = condition;

            if (_parameterFinder.Item != null)
            {
                _parameterFinder.Item.OnTypeChanged -= ParameterTypeChanged;
            }

            _parameterFinder.SelectItemWithoutNotify(_condition.Parameter, true);

            if (_parameterFinder.Item != null)
            {
                _parameterFinder.Item.OnTypeChanged += ParameterTypeChanged;
            }

            _stateFinder.SelectItemWithoutNotify(_condition.State, true);
            _stateValueProvider.SetValueWithoutNotify(_condition.StateValueProvider);

            SetProviderSourceType(_condition.ProviderSourceType);

            _boolComparisonValueField.SetValueWithoutNotify(_condition.BoolComparisonValue ? Bool.True : Bool.False);

            _intComparisonField.SetValueWithoutNotify(_condition.IntComparison);
            _intComparisonValueField.SetValueWithoutNotify(_condition.IntComparisonValue);

            _floatComparisonField.SetValueWithoutNotify(_condition.FloatComparison);
            _floatComparisonValueField.SetValueWithoutNotify(_condition.FloatComparisonValue);
        }

        private void SetProviderSourceType(ValueProviderSourceType providerSourceType)
        {
            if (providerSourceType == ValueProviderSourceType.State)
            {
                _parameterStateSwitch.text = "S";
                _parameterFinder.style.display = DisplayStyle.None;
                _stateFinder.style.display = DisplayStyle.Flex;
                _stateValueProvider.style.display = _stateFinder.Item != null ? DisplayStyle.Flex : DisplayStyle.None;

                if (_stateFinder.Item != null)
                {
                    SetProviderType(ValueProviderType.Float);
                }
                else
                {
                    ComparisonElementsEnabling(_providerType, false);
                }
            }
            else
            {
                _parameterStateSwitch.text = "P";
                _stateFinder.style.display = _stateValueProvider.style.display = DisplayStyle.None;
                _parameterFinder.style.display = DisplayStyle.Flex;

                if (_parameterFinder.Item != null)
                {
                    SetProviderType(_parameterFinder.Item.ParameterType);
                }
                else
                {
                    ComparisonElementsEnabling(_providerType, false);
                }
            }
        }

        private void SetProviderType(ValueProviderType providerType)
        {
            ComparisonElementsEnabling(_providerType, false);

            _providerType = providerType;

            ComparisonElementsEnabling(_providerType, true);
        }

        private void ComparisonElementsEnabling(ValueProviderType providerType, bool enable)
        {
            switch (providerType)
            {
                case ValueProviderType.Bool:
                    _boolComparisonValueField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
                    break;
                case ValueProviderType.Int:
                    _intComparisonField.style.display = _intComparisonValueField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
                    break;
                case ValueProviderType.Float:
                    _floatComparisonField.style.display = _floatComparisonValueField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
                    break;
            }
        }

        private void ParameterTypeChanged(ParameterNodeUI parameter, ValueProviderType previousType, ValueProviderType newType)
        {
            if (_condition.ProviderSourceType == ValueProviderSourceType.Parameter)
            {
                SetProviderType(newType);
            }
        }
    }

    public enum ValueProviderSourceType
    {
        Parameter,
        State
    }

    public enum StateValueProviders
    {
        PreviousTime, Time, PreviousNormalizedTime, NormalizedTime
    }
}
