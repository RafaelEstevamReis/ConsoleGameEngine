using System;
using System.Drawing;
using Simple.CGE.Interfaces;

namespace Simple.CGE.Prefab.PrefabDrawable
{
    public class UIText : IDrawable
    {
        private readonly Action<FrameData, UIText> updateAction;

        public DrawLayers Layer { get; } = DrawLayers.HUD;

        public bool DrawOnPaused { get; }
        public PointF Position { get; set; }

        public string Text { get; set; }

        public UIText(PointF position, bool drawOnPaused, string text, Action<FrameData, UIText> updateAction)
        {
            Position = position;
            DrawOnPaused = drawOnPaused;
            Text = text;
            this.updateAction = updateAction;
        }

        public void DoDraw(FrameData data)
        {
            updateAction?.Invoke(data, this);
            data.DrawEngine.DrawLine((int)Position.X, (int)Position.Y, Text);
        }
    }
}
