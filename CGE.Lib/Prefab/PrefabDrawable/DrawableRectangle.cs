using System;
using System.Drawing;
using Simple.CGE.Interfaces;

namespace Simple.CGE.Prefab.PrefabDrawable
{
    public class DrawableRectangle : IDrawable
    {
        public bool DrawOnPaused { get; set; }
        public DrawLayers Layer { get; set; }
        public char[] Tiles { get; set; }

        public Action<DrawableRectangle, FrameData> OnDraw { get; set; }

        public PointF Position
        {
            get { return Rectangle.Location; }
            set { Rectangle = new RectangleF(value, Rectangle.Size); }
        }

        public RectangleF Rectangle { get; set; }

        public DrawableRectangle(RectangleF rectangle, bool drawOnPaused, DrawLayers layer, char[] tiles)
        {
            Rectangle = rectangle;
            DrawOnPaused = drawOnPaused;
            Layer = layer;
            Tiles = tiles;
        }

        public void DoDraw(FrameData data)
        {
            OnDraw?.Invoke(this, data);

            data.DrawEngine.DrawRectangle(Rectangle, Tiles);
        }
    }
}
