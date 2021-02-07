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

        public static bool IsInside(this Point point, Rectangle rectangle)
        {
            if (point.X < rectangle.Left) return false;
            if (point.Y < rectangle.Top) return false;

            if (point.X > rectangle.Right) return false;
            if (point.Y > rectangle.Bottom) return false;

            return true;
        }
        public static bool IsInside(this PointF point, RectangleF rectangle)
        {
            if (point.X < rectangle.Left) return false;
            if (point.Y < rectangle.Top) return false;

            if (point.X > rectangle.Right) return false;
            if (point.Y > rectangle.Bottom) return false;

            return true;
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

        public static float Distance(Point c1, Point c2)
        {
            return Distance(new PointF(c1.X, c1.Y), new PointF(c2.X, c2.Y));
        }
        public static float Distance(PointF c1, PointF c2)
        {
            return (float)Math.Sqrt((c1.X - c2.X) * (c1.X - c2.X) + (c1.Y - c2.Y) * (c1.Y - c2.Y));
        }
    }
}
