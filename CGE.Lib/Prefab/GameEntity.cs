using System;
using System.Drawing;
using Simple.CGE.Interfaces;

namespace Simple.CGE.Prefab
{
    public class GameEntity : IDrawable, IPhysicsable, IColisionable
    {
        public bool ColidesWithBorders { get; set; }
        public Action BorderColision { get; set; }
        public bool ColidesWithOthers { get; set; }
        public Action<IColisionable[]> OthersColision { get; set; }

        public bool DrawOnPaused { get; set; }
        public bool PhysicsOnPaused { get; set; }
        public DrawLayers Layer { get; set; }
        public char[] Tiles { get; set; }

        public PointF Position
        {
            get { return Rectangle.Location; }
            set { Rectangle = new RectangleF(value, Rectangle.Size); }
        }

        public RectangleF Rectangle { get; set; }

        public Action<GameEntity, FrameData> PhysicsAction { get; }

        public GameEntity(RectangleF rectangle, 
                          bool drawOnPaused, DrawLayers layer, char[] tiles, 
                          bool physicsOnPaused, Action<GameEntity, FrameData> physicsAction,
                          Action borderColision = null, Action<IColisionable[]> othersColision = null)
        {
            Rectangle = rectangle;
            DrawOnPaused = drawOnPaused;
            Layer = layer;
            Tiles = tiles;
            PhysicsOnPaused = physicsOnPaused;
            PhysicsAction = physicsAction;
            BorderColision = borderColision;
            OthersColision = othersColision;

            ColidesWithBorders = BorderColision != null;
            ColidesWithOthers = OthersColision != null;
        }

        public void DoDraw(FrameData data)
        {
            data.DrawEngine.DrawRectangle(Rectangle, Tiles);
        }

        public void DoPhysics(FrameData data)
        {
            PhysicsAction?.Invoke(this, data);
        }

        public void ColidedWith(IColisionable[] entities)
        {
            
        }
        public void ColidedWithBorder()
        {
            
        }
    }
}
