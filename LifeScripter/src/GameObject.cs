using LifeScripter.Backend;
using SadConsole.Components;
using SadConsole.Entities;

internal class GameObject : Entity
{
    public World world;
    public bool isAlive = false;
    public GameObject(ColoredGlyph appearance, Point position, World world)
        : base(new SingleCell(appearance), 1)
    {
        Position = position;
        this.world = world;

        isAlive = true;
    }

    public virtual void Move(Point newPosition)
    {
        world.grid[Position.X, Position.Y] = null;

        Position = newPosition;

        world.grid[Position.X, Position.Y] = this;
    }
}