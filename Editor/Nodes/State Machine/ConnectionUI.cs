using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public class ConnectionUI : GraphElement
    {
        private const float k_lineWidth = 1.5f;
        private const int k_arrowWidth = 16;
        private const int k_arrowHeight = 20;
        private const float k_sqrtThree = 1.7320508f;

        private static readonly Color _color = Color.white;
        private static readonly Color _selectedColor = new Color(0.2f, 0.5f, 1f);

        public IConnectable Source;
        public IConnectable Destination;

        public Vector3 start;
        public Vector3 end;

        private bool _isConnectionSelected;
        public bool IsConnectionSelected
        {
            get => _isConnectionSelected;
            set
            {
                _isConnectionSelected = value;
                MarkDirtyRepaint();
            }
        }

        protected virtual int ArrowCount => 1;

        public ConnectionUI(bool enableContextualMenu = true)
        {
            AddToClassList("transition-connection");

            generateVisualContent = mgc =>
            {
                int indexCount = 6 + ArrowCount * 3;

                Vertex[] vertices = new Vertex[4 + ArrowCount * 3];
                MeshWriteData mesh = mgc.Allocate(vertices.Length, indexCount);

                Vector3 dir = (end - start).normalized;
                Vector3 normal = new Vector3(-dir.y, dir.x);
                Vector3 z = Vector3.forward * Vertex.nearZ;

                Color color = IsConnectionSelected ? _selectedColor : _color;

                vertices[0].position = start + normal * k_lineWidth + z;
                vertices[1].position = start - normal * k_lineWidth + z;
                vertices[2].position = end + normal * k_lineWidth + z;
                vertices[3].position = end - normal * k_lineWidth + z;

                vertices[0].tint = color;
                vertices[1].tint = color;
                vertices[2].tint = color;
                vertices[3].tint = color;

                ushort[] indices = new ushort[indexCount];
                (indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]) = (0, 1, 2, 1, 3, 2);

                int arrowCount = ArrowCount;

                Vector3 lineCenter = (start + end) / 2f;
                Vector3 arrowTopDir = dir * (k_arrowHeight / 2f);
                Vector3 arrowBottomDir = -arrowTopDir;
                Vector3 startPoint = lineCenter + arrowTopDir * (arrowCount - 1) / 2f * 2f;

                for (int i = 0; i < arrowCount; i++)
                {
                    Vector3 arrowBottom = startPoint + arrowBottomDir;

                    int index = i * 3;
                    int v1 = 4 + index, v2 = 4 + index + 1, v3 = 4 + index + 2;
                    vertices[v1].position = arrowBottom + normal * (k_arrowWidth / 2f) + z;
                    vertices[v2].position = arrowBottom - normal * (k_arrowWidth / 2f) + z;
                    vertices[v3].position = startPoint + dir * (k_arrowHeight / 2f) + z;

                    vertices[v1].tint = color;
                    vertices[v2].tint = color;
                    vertices[v3].tint = color;

                    indices[6 + index] = (ushort)v1;
                    indices[6 + index + 1] = (ushort)v2;
                    indices[6 + index + 2] = (ushort)v3;

                    startPoint += arrowBottomDir * 2f;
                }

                // Arrow
                //Vector3 arrowBottom = lineCenter - dir * (k_arrowHeight / 2f);

                //vertices[4].position = arrowBottom + normal * (k_arrowWidth / 2f) + z;
                //vertices[5].position = arrowBottom - normal * (k_arrowWidth / 2f) + z;
                //vertices[6].position = lineCenter + dir * (k_arrowHeight / 2f) + z;

                //vertices[4].tint = _lineColor;
                //vertices[5].tint = _lineColor;
                //vertices[6].tint = _lineColor;

                mesh.SetAllVertices(vertices);
                mesh.SetAllIndices(indices);
                //mesh.SetAllIndices(new ushort[] { 0, 1, 2, 1, 3, 2, 4, 5, 6 });
            };

            style.position = Position.Absolute;

            if (enableContextualMenu)
            {
                this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
                this.AddManipulator(new Clickable(() => StateMachineEditor.Editor.SelectConnection(this)));
            }
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            Vector2 dir = (end - start).normalized;

            return Mathf.Abs(Vector2.Dot(localPoint - (Vector2)start, new Vector2(-dir.y, dir.x))) <= k_arrowWidth * 0.6f;
        }

        public void EnableContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        public void Refresh()
        {
            if (Destination == null) { return; }

            Vector2 topLeft = new Vector2(Source.resolvedStyle.left < Destination.resolvedStyle.left ? Source.resolvedStyle.left : Destination.resolvedStyle.left, Source.resolvedStyle.top < Destination.resolvedStyle.top ? Source.resolvedStyle.top : Destination.resolvedStyle.top);

            Vector2 sourceBottomRight = new Vector2(Source.resolvedStyle.left + Source.resolvedStyle.width, Source.resolvedStyle.top + Source.resolvedStyle.height);
            Vector2 destinationBottomRight = new Vector2(Destination.resolvedStyle.left + Destination.resolvedStyle.width, Destination.resolvedStyle.top + Destination.resolvedStyle.height);

            Vector2 bottomRight = new Vector2(sourceBottomRight.x > destinationBottomRight.x ? sourceBottomRight.x : destinationBottomRight.x, sourceBottomRight.y > destinationBottomRight.y ? sourceBottomRight.y : destinationBottomRight.y);

            style.left = topLeft.x;
            style.top = topLeft.y;
            style.width = bottomRight.x - topLeft.x;
            style.height = bottomRight.y - topLeft.y;

            Vector2 sourceCenter = new Vector2(Source.resolvedStyle.left + Source.resolvedStyle.width / 2, Source.resolvedStyle.top + Source.resolvedStyle.height / 2);
            Vector2 destinationCenter = new Vector2(Destination.resolvedStyle.left + Destination.resolvedStyle.width / 2, Destination.resolvedStyle.top + Destination.resolvedStyle.height / 2);

            if (Source.HasTwoWaysConnection)
            {
                Vector2 direction = (destinationCenter - sourceCenter).normalized;
                Vector2 normal = new Vector2(-direction.y, direction.x);
                Vector2 displacement = normal * (k_arrowWidth * 0.6f);

                sourceCenter += displacement;
                destinationCenter += displacement;
            }

            start = sourceCenter - topLeft;
            end = destinationCenter - topLeft;

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

            StateMachineEditor.Editor.RemoveConnection(this);
        }
    }
}