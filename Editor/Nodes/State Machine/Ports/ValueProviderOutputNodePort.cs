using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using GZ.AnimationGraph;

namespace GZ.AnimationGraph.Editor
{
    public class ValueProviderOutputNodePort : Port
    {
        public ValueProviderType ValueProviderType;

        public ValueProviderOutputNodePort(Orientation orientation, Direction portDirection, Capacity portCapacity, Type type) : base(orientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());

            this.AddManipulator(m_EdgeConnector);
        }

        public void ChangeValueProviderType(ValueProviderType valueProviderType)
        {
            ValueProviderType = valueProviderType;

            foreach (var edge in connections)
            {
                ((TransitionConditionInputNodePort)edge.input).SetValueProviderType(valueProviderType);
            }
        }
    }
}
