using SadConsole.Components;
using SadConsole.Entities;

abstract class EntityObject : Entity
{
    public bool InScene = false;
    public EntityObject(ColoredGlyph appearance, Point position)
        : base(new SingleCell(appearance), 1)
    {
        Position = position;

        Game.Instance.FrameUpdate += Update;
    }

    public virtual void Update(object? sender, GameHost e)
    {
        Position = GetWorldObject().Position;

        if (GetWorldObject().IsDead && InScene) {
            Game.Instance.FrameUpdate -= Update;
            GetWorldObject().World.RemoveEntity(this);
        }
    }

    public abstract WorldObject GetWorldObject();
}