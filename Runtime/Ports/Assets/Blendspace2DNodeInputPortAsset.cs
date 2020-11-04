namespace GZ.AnimationGraph
{
    public class Blendspace2DNodeInputPortAsset : NodeInputPortAsset
    {
        public float X;
        public float Y;

        public override NodeInputPort CreatePort(BaseNode node)
        {
            return ((Blendspace2DNode)node).CreateInputPort(X, Y);
        }
    }
}
