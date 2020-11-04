using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class AnimationGraphView : GraphView
    {
        public AnimationGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            nodeCreationRequest = ctx =>
            {
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition, 0f, 0f), AnimationGraphEditor.Editor);
            };
        }

        public void AddNode(Node node)
        {
            AddElement(node);
        }
        public void AddNode(Node node, Vector2 screenMousePosition)
        {
            AddNode(node);
            var windowRoot = AnimationGraphEditor.Editor.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, screenMousePosition - AnimationGraphEditor.Editor.position.position);
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            node.SetPosition(new Rect(graphMousePosition.x, graphMousePosition.y, 0, 0));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }
    }
}
