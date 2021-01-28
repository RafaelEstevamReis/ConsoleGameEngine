namespace Simple.CGE.Interfaces
{
    public interface IDrawable : IEntity
    {
        bool DrawOnPaused { get; }
        DrawLayers Layer { get; }

        void DoDraw(FrameData data);
    }
}
