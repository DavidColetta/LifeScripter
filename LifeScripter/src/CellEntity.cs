
class CellEntity : DisplayEntity {

    Cell cell;
    public CellEntity(Cell cell)
        : base(cell.Appearance, cell.Position) 
    {
        this.cell = cell;
        Name = cell.script.GetSourceCode(0).Name;

        cell.OnUpdate += () => {
            if (IsSubscribedToFrameUpdate) return;
            IsSubscribedToFrameUpdate = true;
            cell.World.AddEntityToUpdate(this);
        };
        cell.World.AddEntityToUpdate(this);
    }

    public override WorldObject GetWorldObject() {
        return cell;
    }
}

