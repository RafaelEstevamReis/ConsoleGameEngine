using System.Drawing;

namespace Simple.CGE.Interfaces
{
    public interface IDrawEngine
    {
        void Setup();
        void PreFrame();
        void PosFrame();

        RectangleF GameBorder { get; }

        void DrawLine(int left, int top, string Text);
        void DrawRectangle(RectangleF rectangle, char[] data);
        void DrawStart(FrameData data);
        void DrawFinish(FrameData data);
        void StartFrame(FrameData data, DrawLayers layer);
        void EndFrame(FrameData data, DrawLayers layer);
    }
}
