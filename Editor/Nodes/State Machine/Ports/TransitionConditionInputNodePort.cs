using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.UIElements;
using UnityEngine;
using GZ.Tools.UnityUtility;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionConditionInputNodePort : Port
    {
        public bool HasValueProvider;

        public ValueProviderType ValueProviderType;
        public EnumField BoolComparisonValueField;
        public EnumField IntComparisonField;
        public IntegerField IntComparisonValueField;
        public EnumField FloatComparisonField;
        public FloatField FloatComparisonValueField;

        public TransitionConditionInputNodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());

            this.AddManipulator(m_EdgeConnector);

            SetValueProviderType(null);

            BoolComparisonValueField = new EnumField(Bool.True);
            BoolComparisonValueField.AddToClassList("bool-comparison-value");
            Add(BoolComparisonValueField);

            IntComparisonField = new EnumField(Tools.UnityUtility.IntComparison.Equal);
            IntComparisonField.AddToClassList("int-comparison");
            Add(IntComparisonField);

            IntComparisonValueField = new IntegerField();
            Add(IntComparisonValueField);

            FloatComparisonField = new EnumField(Tools.UnityUtility.FloatComparison.BiggerOrEqual);
            FloatComparisonField.AddToClassList("float-comparison");
            Add(FloatComparisonField);

            FloatComparisonValueField = new FloatField();
            Add(FloatComparisonValueField);
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);

            SetValueProviderType(((ValueProviderOutputNodePort)edge.output).ValueProviderType);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);

            SetValueProviderType(null);
        }

        public void SetValueProviderType(ValueProviderType? valueProviderType)
        {
            portName = string.Empty;
            RemoveFromClassList("trigger-or-no-provider");
            RemoveFromClassList("bool-provider");
            RemoveFromClassList("int-provider");
            RemoveFromClassList("float-provider");

            if (valueProviderType == null)
            {
                HasValueProvider = false;

                AddToClassList("trigger-or-no-provider");
                return;
            }

            HasValueProvider = true;
            ValueProviderType = valueProviderType.Value;

            switch (ValueProviderType)
            {
                case ValueProviderType.Bool:
                    AddToClassList("bool-provider");
                    break;
                case ValueProviderType.Int:
                    AddToClassList("int-provider");
                    break;
                case ValueProviderType.Float:
                    AddToClassList("float-provider");
                    break;
                case ValueProviderType.Trigger:
                    portName = "Trigger";
                    AddToClassList("trigger-or-no-provider");
                    break;
            }
        }
    }
}
