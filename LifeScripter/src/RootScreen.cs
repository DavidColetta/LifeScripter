using LifeScripter.Backend;
using SadConsole.Input;
using SadConsole.Configuration;

namespace LifeScripter.Scenes;

internal class RootScreen: ScreenObject
{
    private World _map;

    public RootScreen()
    {
        _map = new World(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        Children.Add(_map.SurfaceObject);
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        bool handled = false;

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _map.UserControlledObject.Move(_map.UserControlledObject.Position + Direction.Up);
            handled = true;
        }
        else if (keyboard.IsKeyPressed(Keys.Down))
        {
            _map.UserControlledObject.Move(_map.UserControlledObject.Position + Direction.Down);
            handled = true;
        }

        if (keyboard.IsKeyPressed(Keys.Left))
        {
            _map.UserControlledObject.Move(_map.UserControlledObject.Position + Direction.Left);
            handled = true;
        }
        else if (keyboard.IsKeyPressed(Keys.Right))
        {
            _map.UserControlledObject.Move(_map.UserControlledObject.Position + Direction.Right);
            handled = true;
        }

        return handled;
    }
}