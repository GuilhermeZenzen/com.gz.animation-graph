using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Blendspace1DNode : BaseNode
    {
        public float Parameter { get; private set; }

        public void SetParameter(float parameter)
        {
            Parameter = parameter;
        }

        private void RecalculateWeights()
        {
            Blendspace1DNodeInputPort previousPort;
            Blendspace1DNodeInputPort currentPort = null;
            Blendspace1DNodeInputPort nextPort = (Blendspace1DNodeInputPort)InputPorts[0];

            for (int i = 0; i < InputPorts.Count; i++)
            {
                previousPort = currentPort;
                currentPort = nextPort;
                nextPort = (Blendspace1DNodeInputPort)(i == InputPorts.Count - 1 ? null : InputPorts[i + 1]);

                float parameterDistance = Parameter - currentPort.Threshold;

                if (parameterDistance == 0f)
                {
                    InputPorts[i].Weight = 1f;
                    break;
                }
                else if (parameterDistance < 0f)
                {
                    if (i == 0)
                    {
                        InputPorts[i].Weight = 1f;
                        break;
                    }
                    else
                    {
                        float previousDistance = previousPort.Threshold - currentPort.Threshold;

                        if (previousDistance == 0f)
                        {
                            InputPorts[i].Weight = 0f;
                            continue;
                        }

                        InputPorts[i].Weight = 1f - Mathf.Min(1f, parameterDistance / previousDistance);
                    }
                }
                else
                {
                    if (i == InputPorts.Count - 1)
                    {
                        InputPorts[i].Weight = 1f;
                        break;
                    }
                    else
                    {
                        float nextDistance = nextPort.Threshold - currentPort.Threshold;

                        if (nextDistance == 0f)
                        {
                            InputPorts[i].Weight = 0f;
                            continue;
                        }

                        InputPorts[i].Weight = 1f - Mathf.Min(1f, parameterDistance / nextDistance);
                    }
                }
            }
        }

        public Blendspace1DNodeInputPort CreateInputPort(float threshold)
        {
            Blendspace1DNodeInputPort port = new Blendspace1DNodeInputPort  { Node = this, Index = InputPorts.Count, Threshold = threshold };
            Playable.SetInputCount(Playable.GetInputCount() + 1);

            InputPorts.Add(port);

            RecalculateWeights();

            return port;
        }

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph) => AnimationMixerPlayable.Create(playableGraph);

        public override (float rawDuration, float duration) CalculateDuration()
        {
            float rawDuration = 0f, duration = 0f;

            InputPorts.ForEach(p =>
            {
                duration += p.Link.OutputPort.Node.Duration * p.Weight;
                rawDuration += p.Link.OutputPort.Node.RawDuration * p.Weight;
            });

            return (rawDuration, duration);
        }

        public NodeLink ConnectWithThreshold(BaseNode sourceNode, float threshold) => Connect(sourceNode.CreateOutputPort(), threshold);
        public NodeLink ConnectWithThreshold(NodeOutputPort outputPort, float threshold) => Connect(CreateInputPort(threshold), outputPort);

        public override BaseNode Copy() => new Blendspace1DNode { Name = this.Name, Speed = Speed, Parameter = this.Parameter };
    }
}
