using System;
using Simple.CGE.Interfaces;

namespace Simple.CGE
{
    public class FrameData
    {
        public CGEngine Engine { get; internal set; }
        public TimeSpan LastFrameTime { get; internal set; }
        public IDrawEngine DrawEngine { get; internal set; }
        public TimeSpan TotalGameTime { get; internal set; }
    }
}
