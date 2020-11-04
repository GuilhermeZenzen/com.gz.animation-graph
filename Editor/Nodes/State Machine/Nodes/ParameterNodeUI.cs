using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class ParameterNodeUI : StateMachineBaseNodeUI
    {
        public TextField NameField { get; private set; }
        public EnumField ParameterTypeField { get; private set; }

        public Toggle BoolField;
        public IntegerField IntField;
        public FloatField FloatField;

        public ValueProviderOutputNodePort OutputPort { get; private set; }

        private static readonly Color _portColor = new Color(3 / 255f, 252 / 255f, 173 / 255f);

        public ParameterNodeUI()
        {
            VisualElement container = new VisualElement();

            title = "Parameter";
            Label titleLabel = (Label)titleContainer[0];
            titleContainer.RemoveAt(0);

            NameField = new TextField() { value = "New Parameter" };
            NameField.style.flexGrow = 1f;

            container.Add(titleLabel);
            container.Add(NameField);

            titleContainer.Insert(0, container);

            OutputPort = new ValueProviderOutputNodePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.portName = string.Empty;
            OutputPort.portColor = _portColor;

            ParameterTypeField = new EnumField(default(ValueProviderType));
            ParameterTypeField.RegisterValueChangedCallback(e =>
            {
                ChangeParameterType((ValueProviderType)e.newValue);
            });

            BoolField = new Toggle();
            OutputPort.Add(BoolField);

            IntField = new IntegerField();
            OutputPort.Add(IntField);

            FloatField = new FloatField();
            OutputPort.Add(FloatField);

            OutputPort.Add(ParameterTypeField);

            outputContainer.Add(OutputPort);

            ChangeParameterType(ValueProviderType.Bool);

            RefreshExpandedState();
            RefreshPorts();
        }

        public void ChangeParameterType(ValueProviderType parameterType)
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

            OutputPort.ChangeValueProviderType(parameterType);
        }

        public void LoadData(Parameter parameter)
        {
            NameField.SetValueWithoutNotify(parameter.Name);

            ParameterTypeField.SetValueWithoutNotify(parameter.Type);
            ChangeParameterType(parameter.Type);

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