using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class ParameterNodeUI : NamedStateMachineBaseNodeUI<ParameterNodeUI>
    {
        public override string Title => "Parameter";

        public ValueProviderType ParameterType => (ValueProviderType)ParameterTypeField.value;

        public EnumField ParameterTypeField { get; private set; }

        public Toggle BoolField;
        public IntegerField IntField;
        public FloatField FloatField;

        public event Action<ParameterNodeUI, ValueProviderType, ValueProviderType> OnTypeChanged;

        public ParameterNodeUI() : base()
        {
            ParameterTypeField = new EnumField(default(ValueProviderType));
            ParameterTypeField.RegisterValueChangedCallback(e =>
            {
                ChangeParameterType((ValueProviderType)e.newValue, (ValueProviderType)e.previousValue);
            });

            extensionContainer.style.flexDirection = FlexDirection.Row;

            extensionContainer.Add(ParameterTypeField);

            BoolField = new Toggle();
            extensionContainer.Add(BoolField);

            IntField = new IntegerField();
            extensionContainer.Add(IntField);

            FloatField = new FloatField();
            extensionContainer.Add(FloatField);

            ChangeParameterType(ValueProviderType.Bool, default);

            RefreshExpandedState();
            RefreshPorts();
        }

        public void ChangeParameterType(ValueProviderType parameterType, ValueProviderType previousParameterType)
        {
            RemoveFromClassList("bool");
            RemoveFromClassList("int");
            RemoveFromClassList("float");
            RemoveFromClassList("trigger");

            switch (parameterType)
            {
                case ValueProviderType.Bool:
                    AddToClassList("bool");
                    break;
                case ValueProviderType.Int:
                    AddToClassList("int");
                    break;
                case ValueProviderType.Float:
                    AddToClassList("float");
                    break;
                case ValueProviderType.Trigger:
                    AddToClassList("trigger");
                    break;
                default:
                    break;
            }

            OnTypeChanged?.Invoke(this, previousParameterType, parameterType);
            //OutputPort.ChangeValueProviderType(parameterType);
        }

        public void LoadData(Parameter parameter)
        {
            Name = parameter.Name;

            ParameterTypeField.SetValueWithoutNotify(parameter.Type);
            ChangeParameterType(parameter.Type, default);

            switch (parameter.ValueProvider)
            {
                case BoolProvider boolProvider:
                    BoolField.SetValueWithoutNotify(boolProvider.Value);
                    break;
                case IntProvider intProvider:
                    IntField.SetValueWithoutNotify(intProvider.Value);
                    break;
                case FloatProvider floatProvider:
                    FloatField.SetValueWithoutNotify(floatProvider.Value);
                    break;
                default:
                    break;
            }
        }
    }
}