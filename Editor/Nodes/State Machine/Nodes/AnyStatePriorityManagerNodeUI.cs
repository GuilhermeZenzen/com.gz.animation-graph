using GZ.AnimationGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnyStatePriorityManagerNodeUI : StateMachineBaseNodeUI
    {
        public override string Title => "Any State Priority Manager";

        private static readonly Color _portColor = Color.white;

        public List<AnyStateNodeUI> AnyStates { get; private set; } = new List<AnyStateNodeUI>();
        private NodeItemList _anyStateList;

        public AnyStatePriorityManagerNodeUI() : base()
        {
            _anyStateList = new NodeItemList(
                "Any States", 
                "Any State", 
                AnyStates, 
                25, 
                container => container.Add(new NamedItemFinder<AnyStateNodeUI>(StateMachineEditor.Editor.AnyStates)),
                (container, index) =>
                {
                    void SelectAnyState(AnyStateNodeUI previousAnyState, AnyStateNodeUI newAnyState, int selectedIndex)
                    {
                        AnyStates[index] = newAnyState;
                    }

                    var anyStateFinder = container.Q<NamedItemFinder<AnyStateNodeUI>>();
                    anyStateFinder.OnItemSelected -= SelectAnyState;
                    anyStateFinder.OnItemSelected += SelectAnyState;
                    anyStateFinder.SelectItemWithoutNotify(AnyStates[index], true);
                },
                () => null);
            extensionContainer.Add(_anyStateList);

            Button createAnyStatePriorityPortButton = new Button(() => CreateAnyStatePriorityPort()) { text = "+ Any State" };

            //inputContainer.Add(createAnyStatePriorityPortButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port CreateAnyStatePriorityPort()
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            port.portName = string.Empty;
            port.portColor = _portColor;

            MakePortRemovable(port);

            inputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        public void LoadData(GraphView graphView, List<AnyState> anyStates, Dictionary<AnyState, AnyStateNodeUI> anyStateMap)
        {
            anyStates.ForEach(anyState =>
            {
                AnyStates.Add(anyStateMap[anyState]);
            });

            _anyStateList.List.Refresh();

            //anyStates.ForEach(s =>
            //{
            //    Port port = CreateAnyStatePriorityPort();

            //    if (s == null) { return; }

            //    var anyStateNode = (AnyStateNodeUI)anyStateMap[s];

            //    Edge edge = new Edge { output = anyStateNode.PriorityPort, input = port };

            //    edge.output.Connect(edge);
            //    edge.input.Connect(edge);

            //    graphView.AddElement(edge);
            //});
        }
    }
}
