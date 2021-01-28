namespace Simple.CGE.Interfaces
{
    public interface IPhysicsable : IEntity
    {
        bool PhysicsOnPaused { get; }
        void DoPhysics(FrameData data);
    }
}
