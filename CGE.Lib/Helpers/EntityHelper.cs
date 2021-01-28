using System;
using System.Drawing;
using Simple.CGE.Interfaces;

namespace Simple.CGE.Helpers
{
    public static class EntityHelper
    {
        public static void MoveTo(this IEntity entity, PointF newLocation)
        {
            entity.Position = newLocation;
        }
        public static void MoveBy(this IEntity entity, PointF Vector)
        {
            entity.Position = new PointF(entity.Position.X + Vector.X, entity.Position.Y + Vector.Y);
        }
        public static void MoveBy(this IEntity entity, PointF Vector, TimeSpan timeCompensation)
        {
            float time = (float)timeCompensation.TotalSeconds;
            entity.Position = new PointF(entity.Position.X + Vector.X * time, entity.Position.Y + Vector.Y * time);
        }

        public static bool IntersectWithRectanglePerimeters(RectangleF first, RectangleF second)
        {
            // left border
            if (first.Left < second.Left && first.Right > second.Left) return true;

            // right border
            if (first.Left < second.Right && first.Right > second.Right) return true;

            // top border
            if (first.Top < second.Top && first.Bottom > second.Top) return true;

            // bottom border
            if (first.Top < second.Bottom && first.Bottom > second.Bottom) return true;

            return false;
        }
    }
}
