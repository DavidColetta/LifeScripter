using SadConsole.Components;
using SadConsole.Entities;

internal class GameObject : Entity
{
    public GameObject(ColoredGlyph appearance, Point position)
        : base(new SingleCell(appearance), 1)
    {
        Position = position;
    }

    public void Move(Point newPosition)
    { 
        Position = newPosition;
    }
}