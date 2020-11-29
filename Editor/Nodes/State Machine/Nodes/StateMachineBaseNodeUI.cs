using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public abstract class StateMachineBaseNodeUI : Node
    {
        public StateMachineGraphView GraphView;

        public abstract string Title { get; }

        public StateMachineBaseNodeUI()
        {
            title = Title;
        }

        protected void MakePortRemovable(Port port, Action callback = null)
        {
            port.Add(new Button(() =>
            {
                callback?.Invoke();

                port.RemoveFromHierarchy();

                foreach (var edge in port.connections)
                {
                    if (port.direction == Direction.Input)
                    {
                        edge.output.Disconnect(edge);
                    }
                    else
                    {
                        edge.input.Disconnect(edge);
                    }
                    edge.RemoveFromHierarchy();
                }

                port.DisconnectAll();
            })
            { text = "-" });
        }
    }
}
