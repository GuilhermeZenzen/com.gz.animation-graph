using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Blendspace2DNode : BaseNode
    {
        private const float k_DirectionalBlendScale = 2f;

        public float XParameter { get; private set; }
        public float YParameter { get; private set; }

        public Blendspace2DBlendingMode BlendingMode;

        public void SetXParameter(float value)
        {
            XParameter = value;
            RecalculateWeights();
        }

        public void SetYParameter(float value)
        {
            YParameter = value;
            RecalculateWeights();
        }

        public void SetParameters(float xParameter, float yParameter)
        {
            XParameter = xParameter;
            YParameter = yParameter;
            RecalculateWeights();
        }

        private void RecalculateWeights()
        {
            if (BlendingMode == Blendspace2DBlendingMode.Cartesian)
            {
                RecalculateWeightsAsCartesian();
            }
            else
            {
                RecalculateWeightsAsDirectional();
            }
        }

        private void RecalculateWeightsAsCartesian()
        {
            Vector2 samplePoint = new Vector2(XParameter, YParameter);
            Vector2[] points = new Vector2[InputPorts.Count];

            float[] weights = new float[InputPorts.Count];
            float totalWeight = 0f;

            for (int i = 0; i < points.Length; i++)
            {
                Blendspace2DNodeInputPort port = (Blendspace2DNodeInputPort)InputPorts[i];
                points[i] = new Vector2(port.X, port.Y);
            }

            for (int i = 0; i < InputPorts.Count; i++)
            {
                float weight = 1f;
                Vector2 iPointToSamplePoint = samplePoint - points[i];

                for (int j = 0; j < InputPorts.Count; j++)
                {
                    if (i == j) continue;

                    Vector2 iPointToJPoint = points[j] - points[i];
                    float newWeight = Mathf.Clamp01(1f - (Vector2.Dot(iPointToSamplePoint, iPointToJPoint) / Vector2.Dot(iPointToJPoint, iPointToJPoint)));
                    weight = Mathf.Min(weight, newWeight);
                }
                weights[i] = weight;
                totalWeight += weight;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                InputPorts[i].Weight = weights[i] / totalWeight;
            }
        }

        private void RecalculateWeightsAsDirectional()
        {
            Vector2 samplePoint = new Vector2(XParameter, YParameter);
            float sampleMagnitude = samplePoint.magnitude;

            (Vector2 point, float magnitude)[] motionsValues = new (Vector2, float)[InputPorts.Count];

            float[] weights = new float[InputPorts.Count];
            float totalWeight = 0f;

            for (int i = 0; i < motionsValues.Length; i++)
            {
                var pointMotion = (Blendspace2DNodeInputPort)InputPorts[i];
                motionsValues[i].point = new Vector2(pointMotion.X, pointMotion.Y);
                motionsValues[i].magnitude = motionsValues[i].point.magnitude;
            }

            for (int i = 0; i < InputPorts.Count; i++)
            {
                float weight = 1f;
                float sampleMinusIMagnitude = sampleMagnitude - motionsValues[i].magnitude;
                float iAngleSample = Vector2.SignedAngle(motionsValues[i].point, samplePoint);

                for (int j = 0; j < InputPorts.Count; j++)
                {
                    if (i == j) continue;

                    float ijAverageMagnitude = (motionsValues[i].magnitude + motionsValues[j].magnitude) / 2;
                    float jMinusIMagnitude = motionsValues[j].magnitude - motionsValues[i].magnitude;
                    float iAngleJ = Vector2.SignedAngle(motionsValues[i].point, motionsValues[j].point);

                    Vector2 iPointToSamplePoint = new Vector2(sampleMinusIMagnitude / ijAverageMagnitude, iAngleSample * k_DirectionalBlendScale);
                    Vector2 iPointToJPoint = new Vector2(jMinusIMagnitude / ijAverageMagnitude, iAngleJ * k_DirectionalBlendScale);

                    float newWeight = Mathf.Clamp01(1f - (Vector2.Dot(iPointToSamplePoint, iPointToJPoint) / Vector2.Dot(iPointToJPoint, iPointToJPoint)));
                    weight = Mathf.Min(weight, newWeight);
                }

                weights[i] = weight;
                totalWeight += weight;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                InputPorts[i].Weight = weights[i] / totalWeight;
            }
        }

        public Blendspace2DNodeInputPort CreateInputPort(float x, float y)
        {
            Blendspace2DNodeInputPort port = new Blendspace2DNodeInputPort { Node = this, Index = InputPorts.Count, X = x, Y = y };
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

        public NodeLink Connect(BaseNode sourceNode, float x, float y) => Connect(sourceNode.CreateOutputPort(), x, y);

        public NodeLink Connect(NodeOutputPort outputPort, float x, float y) => Connect(CreateInputPort(x, y), outputPort);

        public override BaseNode Copy() => new Blendspace2DNode { Name = this.Name, Speed = Speed, XParameter = this.XParameter, YParameter = this.YParameter };
    }
}
