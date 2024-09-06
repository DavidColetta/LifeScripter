using LifeScripter.Backend;

class CellObject : GameObject {

    Cell cell;
    public CellObject(Cell cell, World world)
        : base(cell.Appearance, cell.Position, world) 
    {
        this.cell = cell;
        Name = cell.script.GetSourceCode(0).Name;

        if (!world.IsInBounds(cell.Position) || !world.IsEmpty(cell.Position)) {
            return;
        }

        cell.Initialize(world);

        SubscribeToTicks();

        cell.onSpeedChanged += (int prevSpeed) => {
            UnsubscribeFromTicks(prevSpeed);
            SubscribeToTicks();
        };
    }

    public void Tick() {
        bool wasAlive = cell.IsAlive;
        cell.Tick();

        Move(cell.Position);
        if (!cell.IsAlive && wasAlive) {
            UnsubscribeFromTicks(cell.ticksPerSecond);
            world.RemoveEntity(this);
        }
    }

    private void SubscribeToTicks() {
        double tickInterval = (double)World.TICKS_PER_SECOND / cell.ticksPerSecond;
        for (double i = 0; i < World.TICKS_PER_SECOND; i += tickInterval) {
            world.TickHandlers[(int)i] += Tick;
        }
    }

    private void UnsubscribeFromTicks(int speed) {
        double tickInterval = (double)World.TICKS_PER_SECOND / speed;
        for (double i = 0; i < World.TICKS_PER_SECOND; i += tickInterval) {
            world.TickHandlers[(int)i] -= Tick;
        }
    }
}

