using LifeScripter.Backend;
using SadConsole.Input;

namespace LifeScripter.Scenes;

internal class RootScreen: ScreenObject
{
    private readonly World _map;
    double time;
    double timeStep = 0.01;

    readonly Point screenCenter;

    Point startDragPosition;
    int dragSpeed = 1;

    public RootScreen()
    {
        _map = new World(360, 240);
        Children.Add(_map.SurfaceObject);

        screenCenter = new Point(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY) * _map.SurfaceObject.FontSize / 2;
        startDragPosition = screenCenter;

        Game.Instance.FrameUpdate += (sender, e) => {
            time += e.UpdateFrameDelta.TotalSeconds;
            while (time > timeStep)
            {
                time -= timeStep;
                _map.Tick();
            }
        };
    }
    
    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        bool handled = false;
        if (state.Mouse.ScrollWheelValueChange != 0)
        {
            // dragSpeed -= state.Mouse.ScrollWheelValueChange / 120;
            // dragSpeed = Math.Max(1, dragSpeed);

            Point newFontSize = _map.SurfaceObject.FontSize - state.Mouse.ScrollWheelValueChange / 120;
            if (newFontSize.X > 0 && newFontSize.Y > 0) {
                Point positionBefore = state.Mouse.ScreenPosition / _map.SurfaceObject.FontSize;
                _map.SurfaceObject.FontSize -= state.Mouse.ScrollWheelValueChange / 120;
                Point positionAfter = state.Mouse.ScreenPosition / _map.SurfaceObject.FontSize;

                //Move the screen to keep the mouse in the same position
                _map.MoveScreen(positionAfter - positionBefore);

                handled = true;
            }
        }

        //Move with mouse drag
        if (state.Mouse.RightButtonDown)
        {
            if (state.Mouse.RightButtonDownDuration == TimeSpan.Zero) {
                startDragPosition = state.Mouse.ScreenPosition;
                // Game.Instance.MonoGameInstance.IsMouseVisible = false;
            }

            Point positionChange = state.Mouse.ScreenPosition - startDragPosition;
            Point screenPositionChange = positionChange / _map.SurfaceObject.FontSize;
            if (screenPositionChange != Point.Zero) {
                _map.MoveScreen(screenPositionChange * dragSpeed);

                //Move the mouse to center of screen using MonoGame
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(startDragPosition.X, startDragPosition.Y);
                
                handled = true;
            }
        }
        // else {
        //     Game.Instance.MonoGameInstance.IsMouseVisible = true;
        // }
        return handled;
    }


    public override bool ProcessKeyboard(Keyboard keyboard)
    {   
        bool handled = false;

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _map.MoveScreen((0, 1));
            handled = true;
        }
        else if (keyboard.IsKeyPressed(Keys.Down))
        {
            _map.MoveScreen((0, -1));
            handled = true;
        }

        if (keyboard.IsKeyPressed(Keys.Left))
        {
            _map.MoveScreen((1, 0));
            handled = true;
        }
        else if (keyboard.IsKeyPressed(Keys.Right))
        {
            _map.MoveScreen((-1, 0));
            handled = true;
        }

        return handled;
    }
}