
class CellEntity : EntityObject {

    Cell cell;
    public CellEntity(Cell cell)
        : base(cell.Appearance, cell.Position) 
    {
        this.cell = cell;
        Name = cell.script.GetSourceCode(0).Name;

        if (!cell.World.IsInBounds(cell.Position) || !cell.World.IsEmpty(cell.Position)) {
            return;
        }
    }

    public override WorldObject GetWorldObject() {
        return cell;
    }
}

