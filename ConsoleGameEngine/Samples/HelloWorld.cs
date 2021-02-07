using System.Diagnostics;
using System.Drawing;
using Simple.CGE;
using Simple.CGE.DrawEngines;
using Simple.CGE.Helpers;
using Simple.CGE.Prefab.PrefabDrawable;

namespace CGE.Tests.Samples
{
    public class HelloWorld
    {

        static int vSpeedSign = 1;
        static int hSpeedSign = 1;

        public static void run()
        {
            CGEngine cg = new CGEngine(new FastDraw(new Size(80, 40), new Size(5, 8)));
            cg.OnSetup += (e, a) =>
            {
                cg.TargetDrawFps = 0;
                cg.AddEntities(new UIText(new PointF(),
                                          true,
                                          "Hello World",
                                          update));
            };
            cg.OnPosFrame += posFrame;
            cg.Setup();
            cg.Run();

            System.Console.WriteLine($"Benchmark ended: {cg.TotalGameTime}, {cg.TotalFrames} was drawn");
        }

        static void posFrame(object sender, FrameData data)
        {
            // benchmark modes
            if (data.TotalGameTime.TotalSeconds > 20)
            {
                Debug.WriteLine($"In {data.TotalGameTime}, {data.Engine.TotalFrames} was drawn");
                data.Engine.Stop();
            }
        }

        static void update(FrameData data, UIText text)
        {
            text.Text = $"Hello World at {data.Engine.CurrentFPS:0000} fps";
            text.MoveBy(new PointF(1 * hSpeedSign, 1 * vSpeedSign), data.LastFrameTime);

            if (text.Position.X > data.DrawEngine.GameBorder.Width - text.Text.Length) hSpeedSign = -1;
            if (text.Position.X < 0) hSpeedSign = 1;

            if (text.Position.Y > data.DrawEngine.GameBorder.Height) vSpeedSign = -1;
            if (text.Position.Y < 0) vSpeedSign = 1;
        }

    }
}
