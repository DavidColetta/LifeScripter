using LifeScripter.Backend;

class CellObject : GameObject {

    Cell cell;
    public CellObject(Cell cell, World world)
        : base(cell.Appearance, cell.Position, world) 
    {
        this.cell = cell;

        if (!world.IsInBounds(cell.Position) || !world.IsEmpty(cell.Position)) {
            return;
        }

        world.OnTick += Tick;
    }

    public void Tick() {
        cell.Tick();

        Move(cell.Position);
        if (!cell.IsAlive) {
            world.OnTick -= Tick;
            world.RemoveEntity(this);
        }
    }
}

