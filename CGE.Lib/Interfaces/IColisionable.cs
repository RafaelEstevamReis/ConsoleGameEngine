using System.Drawing;

namespace Simple.CGE.Interfaces
{
    public interface IColisionable
    {
        bool ColidesWithBorders { get; }
        bool ColidesWithOthers { get; }
        RectangleF Rectangle { get; }
        void ColidedWith(IColisionable[] entities);
        void ColidedWithBorder();
    }
}
