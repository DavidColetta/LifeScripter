using LifeScripter.Backend;
using SadConsole.Input;
using SadConsole.Configuration;

namespace LifeScripter.Scenes;

internal class RootScreen: ScreenObject
{
    private World _map;
    double time;
    double timeStep = 0.1;
    public RootScreen()
    {
        _map = new World(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        Children.Add(_map.SurfaceObject);

        Game.Instance.FrameUpdate += (sender, e) => {
            time += e.UpdateFrameDelta.TotalSeconds;
            if (time > timeStep)
            {
                time -= timeStep;
                _map.Tick();
            }
        };
    }
    public override bool ProcessKeyboard(Keyboard keyboard)
    {   
        if (_map.UserControlledObject == null) 
            return false;
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