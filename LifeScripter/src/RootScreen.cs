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
    object _timeLock = new();
    double timeStep = 0;
    TimeSpan tickStartTime;
    int realTicksPerSecond = 0;
    readonly Point screenCenter;

    Point startDragPosition;
    int dragSpeed = 1;

    readonly ControlsConsole timeControls;

    readonly ScreenSurface tickDisplaySurface;

    Thread tickWorker;
    AutoResetEvent frameUpdateEvent = new(false);
    volatile bool killTickWorker = false;

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
        RadioBoxButton pauseButton = new(4, 3)
        {
            GroupName = "TimeStep",
            Position = (0, 0),
            Text = "x0"
        };
        pauseButton.Click += (sender, e) => {
            SetTimeStep(0);
        };
        RadioBoxButton Buttonx1 = new(4, 3)
        {
            GroupName = "TimeStep",
            Position = (4, 0),
            Text = "x1"
        };
        Buttonx1.Click += (sender, e) => {
            SetTimeStep(1.0 / World.TICKS_PER_SECOND);
        };
        RadioBoxButton Buttonx10 = new(5, 3)
        {
            GroupName = "TimeStep",
            Position = (8, 0),
            Text = "x10"
        };
        Buttonx10.Click += (sender, e) => {
            SetTimeStep(0.1 / World.TICKS_PER_SECOND);
        };
        RadioBoxButton Buttonx25 = new(5, 3)
        {
            GroupName = "TimeStep",
            Position = (13, 0),
            Text = "x25"
        };
        Buttonx25.Click += (sender, e) => {
            SetTimeStep(0.04 / World.TICKS_PER_SECOND);
        };
        RadioBoxButton Buttonx50 = new(5, 3)
        {
            GroupName = "TimeStep",
            Position = (18, 0),
            Text = "x50"
        };
        Buttonx50.Click += (sender, e) => {
            SetTimeStep(0.02 / World.TICKS_PER_SECOND);
        };
        RadioBoxButton Buttonx100 = new(6, 3)
        {
            GroupName = "TimeStep",
            Position = (23, 0),
            Text = "x100"
        };
        Buttonx100.Click += (sender, e) => {
            SetTimeStep(0.01 / World.TICKS_PER_SECOND);
        };
        RadioBoxButton ButtonMax = new(5, 3)
        {
            GroupName = "TimeStep",
            Position = (29, 0),
            Text = "Max"
        };
        ButtonMax.Click += (sender, e) => {
            SetTimeStep(-1);
        };

        timeControls.Controls.Add(pauseButton);
        timeControls.Controls.Add(Buttonx1);
        timeControls.Controls.Add(Buttonx10);
        timeControls.Controls.Add(Buttonx25);
        timeControls.Controls.Add(Buttonx50);
        timeControls.Controls.Add(Buttonx100);
        timeControls.Controls.Add(ButtonMax);

        tickDisplaySurface = new ScreenSurface(20, 1);
        tickDisplaySurface.Position = (Game.Instance.ScreenCellsX - 20, Game.Instance.ScreenCellsY - 1);

        Children.Add(tickDisplaySurface);
        Children.Add(timeControls);

        Game.Instance.FrameUpdate += (sender, e) => {     
            UpdateTPSCounter();

            // System.Diagnostics.Debug.WriteLine("Frame Update");
            if (timeStep == 0) {
                return;
            }
            lock (_timeLock) {
                time += e.UpdateFrameDelta.TotalSeconds;
            }
            // TickWorker();
            frameUpdateEvent.Set();
        };
        tickWorker = new Thread(TickWorker);
        tickWorker.Name = "Tick Worker";
        tickWorker.Start();

        //Stop thread on application close
        Game.Instance.Ending += (sender, e) => {
            killTickWorker = true;
            tickWorker.Interrupt();
        };
    }

    void TickWorker() {
        while (!killTickWorker) {
            try {
                frameUpdateEvent.WaitOne();
            } catch (ThreadInterruptedException) {
                break;
            }
            
            while (time > timeStep && timeStep != 0 && !killTickWorker)
            {
                if (timeStep > 0) {
                    lock (_timeLock) {
                        time -= timeStep;
                    }
                }
                
                Interlocked.Increment(ref realTicksPerSecond);
                _map.Tick();
            }
        }
    }

    public void SetTimeStep(double newTimeStep) {
        timeStep = newTimeStep;
        lock (_timeLock) {
            time = 0;
        }
        realTicksPerSecond = 0;
        tickStartTime = DateTime.Now.TimeOfDay;
    }

    public void UpdateTPSCounter() {
        double timeSinceLastTPSUpdate = DateTime.Now.TimeOfDay.TotalSeconds - tickStartTime.TotalSeconds;
        int realTPSEstimate = (int)Math.Ceiling(realTicksPerSecond / timeSinceLastTPSUpdate);
        string tpsText = realTPSEstimate + " TPS";
        tickDisplaySurface.Clear();
        tickDisplaySurface.Print(20 - tpsText.Length, 0, tpsText, Color.Black);
            
        if (timeSinceLastTPSUpdate >= 1) {
            tickStartTime = DateTime.Now.TimeOfDay;
            realTicksPerSecond = 0;
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