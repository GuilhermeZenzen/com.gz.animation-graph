using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public abstract class BaseNode
    {
        public string Name;

        [SerializeReference] public List<NodeInputPort> InputPorts = new List<NodeInputPort>();

        [SerializeReference] public List<NodeOutputPort> OutputPorts = new List<NodeOutputPort>();

        [NonSerialized] public Playable Playable;

        [SerializeField] private float _speed = 1f;
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;

                if (!Playable.IsNull())
                {
                    Playable.SetSpeed(_speed);
                    UpdateDuration();
                }
            }
        }

        public float Duration { get; private set; }
        public float RawDuration { get; private set; }
        
        public abstract BaseNode Copy();

        public Playable CreatePlayable(PlayableGraph playableGraph)
        {
            Playable = OnCreatePlayable(playableGraph);
            Playable.SetOutputCount(0);
            Playable.SetSpeed(Speed);
            UpdateDuration();

            return Playable;
        }

        protected abstract Playable OnCreatePlayable(PlayableGraph playableGraph);

        public void UpdateDuration()
        {
            (var rawDuration, var duration) = CalculateDuration();
            RawDuration = rawDuration;
            Duration = duration / Mathf.Abs(Speed);

            OutputPorts.ForEach(p => p.Link?.InputPort.Node.UpdateDuration());
        }

        public virtual (float rawDuration, float duration) CalculateDuration()
        {
            float rawDuration = 0f, duration = 0f;
            InputPorts.ForEach(p =>
            {
                duration = Mathf.Max(duration, p.Link.OutputPort.Node.Duration);
                rawDuration = Mathf.Max(rawDuration, p.Link.OutputPort.Node.RawDuration);
            });

            return (rawDuration, duration);
        }

        public void SetupInputPort(NodeInputPort inputPort, float weight)
        {
            inputPort.Node = this;
            inputPort.Index = InputPorts.Count;
            Playable.SetInputCount(Playable.GetInputCount() + 1);
            Playable.SetInputWeight(inputPort.Index, inputPort.Weight);

            InputPorts.Add(inputPort);

            OnSetupBaseInputPort(inputPort, weight);
        }

        public NodeInputPort CreateBaseInputPort(float weight)
        {
            NodeInputPort port = OnCreateBaseInputPort();
            SetupInputPort(port, weight);

            return port;
        }

        public virtual NodeInputPort OnCreateBaseInputPort() => new NodeInputPort();

        public virtual void OnSetupBaseInputPort(NodeInputPort inputPort, float weight) => inputPort.Weight = weight;

        public NodeOutputPort CreateOutputPort()
        {
            NodeOutputPort port = new NodeOutputPort { Node = this, Index = OutputPorts.Count };
            Playable.SetOutputCount(Playable.GetOutputCount() + 1);
            OutputPorts.Add(port);

            return port;
        }

        public void DestroyInputPort(NodeInputPort inputPort) => DestroyInputPort(inputPort.Index);
        public virtual void DestroyInputPort(int inputPortIndex) => DestroyInputPortWithCallback(inputPortIndex);

        public void DestroyInputPortWithCallback(NodeInputPort inputPort, Action<NodeInputPort> callback = null) => DestroyInputPortWithCallback(inputPort.Index, callback);
        public void DestroyInputPortWithCallback(int inputPortIndex, Action<NodeInputPort> callback = null)
        {
            if (InputPorts[inputPortIndex].Link != null)
            {
                Disconnect(InputPorts[inputPortIndex].Link);
            }

            InputPorts.RemoveAt(inputPortIndex);

            int newInputCount = Playable.GetInputCount() - 1;

            Playable.DisconnectInput(inputPortIndex);

            for (int i = inputPortIndex; i < newInputCount; i++)
            {
                InputPorts[i].Index--;
                Playable playable = Playable.GetInput(i + 1);
                Playable.DisconnectInput(i + 1);
                Playable.ConnectInput(i, playable, InputPorts[i].Link.OutputPort.Index, InputPorts[i].Weight);
                callback?.Invoke(InputPorts[i]);
            }

            Playable.SetInputCount(newInputCount);
        }

        public void DestroyOutputPort(int portIndex)
        {
            if (OutputPorts[portIndex].Link != null)
            {
                OutputPorts[portIndex].Link.OutputPort.Node.Disconnect(OutputPorts[portIndex].Link.InputPort.Link);
            }

            OutputPorts.RemoveAt(portIndex);

            int newOutputCount = Playable.GetOutputCount() - 1;

            for (int i = portIndex; i < newOutputCount; i++)
            {
                NodeOutputPort outputPort = OutputPorts[i];
                outputPort.Index--;
                outputPort.Link.InputPort.Node.UpdateConnection(outputPort.Link);
            }

            Playable.SetOutputCount(newOutputCount);
        }

        public NodeLink Connect(BaseNode sourceNode, float weight = 1f) => Connect(sourceNode.CreateOutputPort(), weight);
        public NodeLink Connect(NodeOutputPort outputPort, float weight = 1f) => Connect(CreateBaseInputPort(weight), outputPort);
        public NodeLink Connect(NodeInputPort inputPort, BaseNode sourceNode)
        {
            return Connect(inputPort, sourceNode.CreateOutputPort());
        }
        public virtual NodeLink Connect(NodeInputPort inputPort, NodeOutputPort outputPort)
        {
            if (inputPort.Link != null)
            {
                Disconnect(inputPort.Link);
            }
            if (outputPort.Link != null)
            {
                outputPort.Link.InputPort.Node.Disconnect(outputPort.Link);
            }

            NodeLink nodeLink = new NodeLink { InputPort = inputPort, OutputPort = outputPort };
            inputPort.Link = nodeLink;
            outputPort.Link = nodeLink;

            Playable.ConnectInput(inputPort.Index, outputPort.Node.Playable, outputPort.Index, inputPort.Weight);

            return nodeLink;
        }

        public void UpdateConnection(NodeLink nodeLink)
        {
            Playable.DisconnectInput(nodeLink.InputPort.Index);
            Playable.ConnectInput(nodeLink.InputPort.Index, nodeLink.OutputPort.Node.Playable, nodeLink.OutputPort.Index);
        }

        public virtual void Disconnect(NodeLink nodeLink)
        {
            Playable.DisconnectInput(nodeLink.InputPort.Index);
            nodeLink.InputPort.Link = null;
            nodeLink.OutputPort.Link = null;
        }
    }
}
