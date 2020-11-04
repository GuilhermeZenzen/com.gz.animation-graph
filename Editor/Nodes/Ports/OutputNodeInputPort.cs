using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class OutputNodeInputPort : Port
    {
        public OutputNodeInputPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());

            this.AddManipulator(m_EdgeConnector);
        }

        public override void Connect(Edge edge)
        {
            foreach (var connection in edge.output.connections)
            {
                connection.input.Disconnect(connection);
                connection.RemoveFromHierarchy();
            }

            edge.output.DisconnectAll();

            AnimationGraphEditor.Editor.GraphView.OutputNode = (BaseNodeUI)edge.output.node;

            base.Connect(edge);
        }

        public override void Disconnect(Edge edge)
        {
            AnimationGraphEditor.Editor.GraphView.OutputNode = null;

            base.Disconnect(edge);
        }

        public override void DisconnectAll()
        {
            AnimationGraphEditor.Editor.GraphView.OutputNode = null;

            base.DisconnectAll();
        }
    }
}
