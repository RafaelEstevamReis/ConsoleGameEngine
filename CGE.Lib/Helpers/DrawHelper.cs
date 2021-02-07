using System.Drawing;
using Simple.CGE.Interfaces;

namespace Simple.CGE.Helpers
{
    public static class DrawHelper
    {
        public static void Fill(this IDrawEngine engine, Rectangle rectangle, char content)
        {
            int size = rectangle.Height * rectangle.Width;
            var arr = new char[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = content;
            }
            engine.DrawRectangle(rectangle, arr);
        }

    }
}
