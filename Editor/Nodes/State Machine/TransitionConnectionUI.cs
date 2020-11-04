using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class TransitionConnectionUI : GraphElement
    {
        private const float k_lineWidth = 1.5f;
        private const int k_arrowWidth = 16;
        private const int k_arrowHeight = 20;
        private const float k_sqrtThree = 1.7320508f;

        private static readonly Color _lineColor = Color.white;

        public ITransitionConnectable Source;
        public ITransitionConnectable Destination;

        public Vector3 start;
        public Vector3 end;

        public TransitionConnectionUI(bool enableContextualMenu = true)
        {
            AddToClassList("transition-connection");

            generateVisualContent = mgc =>
            {
                Vertex[] vertices = new Vertex[7];
                MeshWriteData mesh = mgc.Allocate(vertices.Length, 9);

                Vector3 dir = (end - start).normalized;
                Vector3 normal = new Vector3(-dir.y, dir.x);
                Vector3 z = Vector3.forward * Vertex.nearZ;

                vertices[0].position = start + normal * k_lineWidth + z;
                vertices[1].position = start - normal * k_lineWidth + z;
                vertices[2].position = end + normal * k_lineWidth + z;
                vertices[3].position = end - normal * k_lineWidth + z;

                vertices[0].tint = _lineColor;
                vertices[1].tint = _lineColor;
                vertices[2].tint = _lineColor;
                vertices[3].tint = _lineColor;

                // Arrow
                Vector3 lineCenter = (start + end) / 2f;
                Vector3 arrowBottom = lineCenter - dir * (k_arrowHeight / 2f);
                //float arrowHeight = k_arrowSideSize * k_sqrtThree / 2f;
                //Vector3 arrowBottom = lineCenter - dir * (arrowHeight / 2f);
                vertices[4].position = arrowBottom + normal * (k_arrowWidth / 2f) + z;
                vertices[5].position = arrowBottom - normal * (k_arrowWidth / 2f) + z;
                vertices[6].position = lineCenter + dir * (k_arrowHeight / 2f) + z;

                vertices[4].tint = _lineColor;
                vertices[5].tint = _lineColor;
                vertices[6].tint = _lineColor;

                mesh.SetAllVertices(vertices);
                mesh.SetAllIndices(new ushort[] { 0, 1, 2, 1, 3, 2, 4, 5, 6 });
            };

            style.position = Position.Absolute;

            if (enableContextualMenu)
            {
                this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            }
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            Vector2 dir = (end - start).normalized;
            Debug.Log($"Dir: {dir} | Normal: {new Vector2(-dir.y, dir.x)} | Width: {k_arrowWidth} | Dist: {Mathf.Abs(Vector2.Dot(localPoint, new Vector2(-dir.y, dir.x)))}");
            return Mathf.Abs(Vector2.Dot(localPoint - (Vector2)start, new Vector2(-dir.y, dir.x))) <= k_arrowWidth;
        }

        public void EnableContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        public void Refresh()
        {
            float sourceXCenter = Source.resolvedStyle.left + Source.resolvedStyle.width / 2;
            float sourceYCenter = Source.resolvedStyle.top + Source.resolvedStyle.height / 2;
            float destinationXCenter = Destination.resolvedStyle.left + Destination.resolvedStyle.width / 2;
            float destinationYCenter = Destination.resolvedStyle.top + Destination.resolvedStyle.height / 2;

            var mostTopCenter = sourceYCenter < destinationYCenter ? sourceYCenter : destinationYCenter;
            var mostLeftCenter = sourceXCenter < destinationXCenter ? sourceXCenter : destinationXCenter;

            style.top = mostTopCenter;
            style.left = mostLeftCenter;

            if (Destination != null)
            {
                end = new Vector3(destinationXCenter < sourceXCenter ? 0f : destinationXCenter - sourceXCenter, destinationYCenter < sourceYCenter ? 0f : destinationYCenter - sourceYCenter);

                style.width = Mathf.Abs(sourceXCenter - destinationXCenter);
                style.height = Mathf.Abs(sourceYCenter - destinationYCenter);
            }

            if (destinationYCenter < sourceYCenter)
            {
                start = new Vector3(start.x, style.height.value.value);
            }

            if (destinationXCenter < sourceXCenter)
            {
                start = new Vector3(style.width.value.value, start.y);
            }

            MarkDirtyRepaint();
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent menuBuilder)
        {
            menuBuilder.menu.AppendAction("Delete", a => Delete());
        }

        public void Delete()
        {
            Source.OnExitConnectionDeleted(this);
            Source.ExitConnections.Remove(this);

            Destination.OnEntryConnectionDeleted(this);
            Destination.EntryConnections.Remove(this);

            StateMachineEditor.Editor.GraphView.RemoveElement(this);
        }
    }
}