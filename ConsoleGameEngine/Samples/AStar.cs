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
            CGEngine cg = new CGEngine(new FastDraw(new Size(300, 300), new Size(2, 2)));
            cg.OnSetup += (e, a) =>
            {
                cg.TargetDrawFps = 0;
                cg.AddEntities(new Maze(new Size(100, 100)));
                cg.ShowDataOnTitle = true; 
            };
            cg.OnPosFrame += Cg_OnPosFrame;
            cg.Setup();
            cg.Run();
        }
        private static void Cg_OnPosFrame(object sender, FrameData e)
        {
            if (e.Engine.TotalFrames >= 1000)
            {
                e.Engine.Stop();
            }
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
            solver = new PathSearch(size, new Point(1, 1), new Point(98, 98));
            solver.Setup();

            for (int i = 0; i < 97; i++) solver.SetNotWalkable(new Point(i, 20));
            for (int i = 3; i < 100; i++) solver.SetNotWalkable(new Point(i, 40));

            for (int i = 0; i < 97; i++) solver.SetNotWalkable(new Point(i, 60));
            for (int i = 3; i < 100; i++) solver.SetNotWalkable(new Point(i, 70));

            //solver.SetWalkable('X',
            //    "----------" +
            //    "-S--X-----" +
            //    "----X-----" +
            //    "XXXXX-----" +
            //    "----------" +
            //    "----XXXXXX" +
            //    "----------" +
            //    "----X--XXX" +
            //    "----X---F-" +
            //    "----X-----");
        }

        public void DoDraw(FrameData data)
        {
            int nodeSize = 2;
            int clearance = 3;

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
                    else if (node.SearchState == PathSearch.SearchNode.NodeState.OpenSet) box = box2;
                    else if (node.SearchState == PathSearch.SearchNode.NodeState.ClosedSet) box = box3;

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
