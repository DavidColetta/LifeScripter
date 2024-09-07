using SadConsole.Components;
using SadConsole.Entities;

abstract class DisplayEntity : Entity
{
    public bool IsSubscribedToFrameUpdate = false;
    public bool InScene = false;
    public DisplayEntity(ColoredGlyph appearance, Point position)
        : base(new SingleCell(appearance), 1)
    {
        Position = position;
    }

    public virtual void Update(object? sender, GameHost e)
    {
        IsSubscribedToFrameUpdate = false;
        Position = GetWorldObject().Position;

        if (GetWorldObject().IsAlive && !InScene) {
            GetWorldObject().World.AddEntity(this);
            InScene = true;
        }
        
        if (GetWorldObject().IsDead && InScene) {
            InScene = false;
            GetWorldObject().World.RemoveEntity(this);
        }
    }

    public abstract WorldObject GetWorldObject();
}