using System.Drawing;
using Simple.CGE;
using Simple.CGE.DrawEngines;
using Simple.CGE.Helpers;
using Simple.CGE.Interfaces;

namespace CGE.Tests.Samples
{
    class AStar
    {
        internal static void run()
        {
            CGEngine cg = new CGEngine(new FastDraw(new Size(65, 65), new Size(7, 7)));
            cg.OnSetup += (e, a) =>
            {
                cg.TargetDrawFps = 10;
                cg.AddEntities(new Maze(new Size(10, 10)));
                cg.ShowDataOnTitle = true; 
            };
            cg.Setup();
            cg.Run();
        }
    }
    class Maze : IDrawable, IPhysicsable
    {
        public Size Size { get; }

        public bool DrawOnPaused => false;

        public DrawLayers Layer =>  DrawLayers.Foreground;

        public PointF Position { get; set; } = new PointF();

        public bool PhysicsOnPaused => false;

        PathSearch solver { get; }

        public Maze(Size size)
        {
            Size = size;
            solver = new PathSearch(size, new Point(1, 1), new Point(8, 8));
            solver.Setup();

            solver.SetWalkable('X',
                "----------" +
                "-S--X-----" +
                "----X-----" +
                "XXXXX-----" +
                "----------" +
                "----XXXXXX" +
                "----------" +
                "----X--XXX" +
                "----X---F-" +
                "----X-----");
        }

        public void DoDraw(FrameData data)
        {
            int nodeSize = 4;
            int clearance = 6;

            char box1 = '-';

            char box2 = '░';

            char box3 = '▓';

            char box4 = '█';
            char box5 = '#';

            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    var rect = new Rectangle(2 + clearance * x, 2 + clearance * y, nodeSize, nodeSize);

                    var node = solver.Map[x, y];
                    if (node == null) continue;

                    char box = box1;
                    if (node.Obstruction) box = box4;
                    else if (node.IsFinishedPath) box = box5;
                    else if (node.IsOpenSet) box = box2;
                    else if (node.IsClosedSet) box = box3;

                    if (node.Coordinates == solver.StartPoint) box = 'S';
                    if (node.Coordinates == solver.FinishPoint) box = 'F';

                    data.DrawEngine.Fill(rect, box);
                }
            }
        }

        public void DoPhysics(FrameData data)
        {
            solver.DoStep();
        }
    }
}
