using System.Drawing;
using Simple.CGE;
using Simple.CGE.Helpers;
using Simple.CGE.Prefab.PrefabDrawable;

int vSpeedSign = 1;
int hSpeedSign = 1;

CGEngine cg = new CGEngine();
cg.OnSetup += (e, a) =>
{
    cg.TargetDrawFps = 0;
    cg.AddEntities(new UIText(new PointF(),
                              true,
                              "Hello World",
                              update));
};
cg.Setup();
cg.Run();

void update(FrameData data, UIText text)
{
    text.Text = $"Hello World at {data.Engine.CurrentFPS:0000} fps";
    text.MoveBy(new PointF(1 * hSpeedSign, 1 * vSpeedSign), data.LastFrameTime);

    if (text.Position.X > data.DrawEngine.GameBorder.Width - text.Text.Length) hSpeedSign = -1;
    if (text.Position.X < 0) hSpeedSign = 1;

    if (text.Position.Y > data.DrawEngine.GameBorder.Height) vSpeedSign = -1;
    if (text.Position.Y < 0) vSpeedSign = 1;
}
