using SadConsole.Input;
using SadConsole.Entities;
using SadConsole.UI.Controls;
using SadConsole.UI;
using System.Runtime.InteropServices;

namespace LifeScripter.Scenes;

internal class RootScreen: ScreenObject
{
    private readonly World _map;
    double time;
    double timeStep = 0;

    readonly Point screenCenter;

    Point startDragPosition;
    int dragSpeed = 1;

    readonly ControlsConsole timeControls;

    // Thread tickWorker;

    public RootScreen()
    {
        //Map
        _map = new World(360, 240);
        Children.Add(_map.SurfaceObject);

        screenCenter = new Point(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY) * _map.SurfaceObject.FontSize / 2;
        startDragPosition = screenCenter;

        //Time Controls
        timeControls = new ControlsConsole(34, 3);
        timeControls.Position = ((Game.Instance.ScreenCellsX / 2) - ((timeControls.Width+1) / 2), Game.Instance.ScreenCellsY - 3);
        ButtonBox pauseButton = new(4, 3)
        {
            Position = (0, 0),
            Text = "x0"
        };
        pauseButton.Click += (sender, e) => {
            timeStep = 0;
        };
        ButtonBox Buttonx1 = new(4, 3)
        {
            Position = (4, 0),
            Text = "x1"
        };
        Buttonx1.Click += (sender, e) => {
            timeStep = 1.0 / World.TICKS_PER_SECOND;
        };
        ButtonBox Buttonx10 = new(5, 3)
        {
            Position = (8, 0),
            Text = "x10"
        };
        Buttonx10.Click += (sender, e) => {
            timeStep = 0.1 / World.TICKS_PER_SECOND;
        };
        ButtonBox Buttonx25 = new(5, 3)
        {
            Position = (13, 0),
            Text = "x25"
        };
        Buttonx25.Click += (sender, e) => {
            timeStep = 0.04 / World.TICKS_PER_SECOND;
        };
        ButtonBox Buttonx50 = new(5, 3)
        {
            Position = (18, 0),
            Text = "x50"
        };
        Buttonx50.Click += (sender, e) => {
            timeStep = 0.02 / World.TICKS_PER_SECOND;
        };
        ButtonBox Buttonx100 = new(6, 3)
        {
            Position = (23, 0),
            Text = "x100"
        };
        Buttonx100.Click += (sender, e) => {
            timeStep = 0.01 / World.TICKS_PER_SECOND;
        };
        ButtonBox ButtonMax = new(5, 3)
        {
            Position = (29, 0),
            Text = "Max"
        };
        ButtonMax.Click += (sender, e) => {
            timeStep = -1;
        };

        timeControls.Controls.Add(pauseButton);
        timeControls.Controls.Add(Buttonx1);
        timeControls.Controls.Add(Buttonx10);
        timeControls.Controls.Add(Buttonx25);
        timeControls.Controls.Add(Buttonx50);
        timeControls.Controls.Add(Buttonx100);
        timeControls.Controls.Add(ButtonMax);

        Children.Add(timeControls);

        Game.Instance.FrameUpdate += (sender, e) => {
            System.Diagnostics.Debug.WriteLine("Frame Update");
            if (timeStep == 0) {
                return;
            }
            time += e.UpdateFrameDelta.TotalSeconds;
            TickWorker();
        };
        // tickWorker = new Thread(TickWorker);
        // tickWorker.Start();
    }

    void TickWorker() {
        while (time > timeStep)
        {
            time -= timeStep;
            _map.Tick();
        }
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