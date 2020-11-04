using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class EntryNodeUI : StateMachineBaseNodeUI, ITransitionConnectable
    {
        private static readonly Color _portColor = new Color(252 / 255f, 231 / 255f, 3 / 255f);

        public List<TransitionConnectionUI> EntryConnections { get; private set; } = new List<TransitionConnectionUI>();

        public List<TransitionConnectionUI> ExitConnections { get; private set; } = new List<TransitionConnectionUI>();

        public EntryNodeUI()
        {
            title = "Entry";

            capabilities &= ~Capabilities.Deletable;

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<GeometryChangedEvent>(e => ExitConnections.ForEach(c => c.Refresh()));
        }

        public void LoadData(GraphView graphView, State entryState, Dictionary<State, StateNodeUI> map)
        {
            if (entryState != null)
            {
                var connection = StateMachineEditor.Editor.CreateTransitionConnection(this, map[entryState]);
                ExitConnections.Add(connection);
                map[entryState].EntryConnections.Add(connection);
                connection.schedule.Execute(() => connection.Refresh());
            }
        }

        public void GenerateData(StateMachineNode stateMachineNode, Dictionary<StateNodeUI, State> stateMap)
        {
            if (ExitConnections.Count > 0)
            {
                stateMachineNode.EntryState = stateMap[(StateNodeUI)ExitConnections[0].Destination];
            }
            else if (stateMap.Count > 0)
            {
                stateMachineNode.EntryState = stateMap.Values.ElementAt(0);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Set Entry State", a =>
            {
                StateMachineEditor.Editor.AddTransitionConnection(this);
            });
        }

        public void OnEntryConnect(TransitionConnectionUI connection)
        {

        }

        public void OnExitConnect(TransitionConnectionUI connection)
        {
            if (ExitConnections.Count > 1)
            {
                ExitConnections[0].Delete();
                ExitConnections.RemoveAt(0);
            }
        }

        public void OnEntryConnectionDeleted(TransitionConnectionUI connection)
        {

        }

        public void OnExitConnectionDeleted(TransitionConnectionUI connection)
        {

        }
    }
}
