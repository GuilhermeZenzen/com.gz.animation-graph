using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public interface IConnectable
    {
        IResolvedStyle resolvedStyle { get; }

        List<ConnectionUI> EntryConnections { get; }
        List<ConnectionUI> ExitConnections { get; }

        bool HasTwoWaysConnection { get; }

        void OnEntryConnect(ConnectionUI connection);
        void OnExitConnect(ConnectionUI connection);

        void OnEntryConnectionDeleted(ConnectionUI connection);
        void OnExitConnectionDeleted(ConnectionUI connection);

        bool CanConnect(IConnectable target);

        (ConnectionUI connection, bool isNew) GetConnection(IConnectable target, bool isEnabled);
    }

    public static class TransitionConnectable
    {
        public static ConnectionUI CreateDefaultConnection(bool isEnabled) => new ConnectionUI(isEnabled);

        public static void ClearConnections(this IConnectable connectable)
        {
            connectable.EntryConnections.ForEach(c =>
            {
                c.Source.OnExitConnectionDeleted(c);
                c.Destination.OnEntryConnectionDeleted(c);
                c.RemoveFromHierarchy();
            });
            connectable.EntryConnections.Clear();

            connectable.ExitConnections.ForEach(c =>
            {
                c.Source.OnExitConnectionDeleted(c);
                c.Destination.OnEntryConnectionDeleted(c);
                c.RemoveFromHierarchy();
            });
            connectable.ExitConnections.Clear();
        }
    }
}