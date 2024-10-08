abstract class WorldObject
{
    public Point Position { get; protected set; }
    public World World {get; private set;}
    public bool IsAlive { get; protected set; } = false;
    public bool IsDead { get; protected set; } = false;

    public delegate void EmptyHandler();
    public event EmptyHandler? OnUpdate;
    public WorldObject(Point position, World world)
    {
        Position = position;
        World = world;
        
        if (!World.IsInBounds(Position) || !World.IsEmpty(Position)) {
            throw new System.Exception("WorldObject cannot be placed at this position");
        }
        World.grid[Position.X, Position.Y] = this;
    }

    public void Reposition(Point newPosition) {
        World.grid[Position.X, Position.Y] = null;
        Position = newPosition;
        World.grid[Position.X, Position.Y] = this;
    }

    public virtual void Die() {
        IsAlive = false;
        if (IsDead) {
            return;
        }
        IsDead = true;
        World.grid[Position.X, Position.Y] = null;

        OnUpdate?.Invoke();
    }

    public void FireOnUpdate() {
        OnUpdate?.Invoke();
    }
}