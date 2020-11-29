using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class EntryNodeUI : StateMachineBaseNodeUI, IConnectable
    {
        public override string Title => "Entry";

        private static readonly Color _portColor = new Color(252 / 255f, 231 / 255f, 3 / 255f);

        public List<ConnectionUI> EntryConnections { get; private set; } = new List<ConnectionUI>();

        public List<ConnectionUI> ExitConnections { get; private set; } = new List<ConnectionUI>();

        public bool HasTwoWaysConnection => false;

        public EntryNodeUI() : base()
        {
            capabilities &= ~Capabilities.Deletable;

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<GeometryChangedEvent>(e => ExitConnections.ForEach(c => c.Refresh()));
        }

        public void LoadData(GraphView graphView, State entryState, Dictionary<State, StateNodeUI> map)
        {
            if (entryState != null)
            {
                var connection = StateMachineEditor.Editor.CreateConnection(this, map[entryState], false);
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
                StateMachineEditor.Editor.AddConnection(this);
            });
        }

        public (ConnectionUI connection, bool isNew) GetConnection(IConnectable target, bool isEnabled)
        {
            return (new ConnectionUI(isEnabled), true);
        }

        public bool CanConnect(IConnectable target) => target is StateNodeUI;

        public void OnEntryConnect(ConnectionUI connection)
        {

        }

        public void OnExitConnect(ConnectionUI connection)
        {
            if (ExitConnections.Count > 1)
            {
                ExitConnections[0].Delete();
            }
        }

        public void OnEntryConnectionDeleted(ConnectionUI connection)
        {

        }

        public void OnExitConnectionDeleted(ConnectionUI connection)
        {

        }
    }
}
