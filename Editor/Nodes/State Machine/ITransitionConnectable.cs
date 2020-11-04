using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GZ.AnimationGraph.Editor
{
    public interface ITransitionConnectable
    {
        IResolvedStyle resolvedStyle { get; }

        List<TransitionConnectionUI> EntryConnections { get; }
        List<TransitionConnectionUI> ExitConnections { get; }

        void OnEntryConnect(TransitionConnectionUI connection);
        void OnExitConnect(TransitionConnectionUI connection);

        void OnEntryConnectionDeleted(TransitionConnectionUI connection);
        void OnExitConnectionDeleted(TransitionConnectionUI connection);
    }

    public static class TransitionConnectable
    {
        public static void ClearConnections(this ITransitionConnectable connectable)
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