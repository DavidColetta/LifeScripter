using System.Diagnostics;
using System.Numerics;
using MoonSharp.Interpreter;
using SadConsole.Entities;

class World
{
    private readonly ScreenSurface _screenSurface;

    private readonly EntityManager entities;

    public ScreenSurface SurfaceObject => _screenSurface;
    public DisplayEntity? UserControlledObject { get; set; }

    public WorldObject?[,] grid;

    public int Width => _screenSurface.Surface.Width;
    public int Height => _screenSurface.Surface.Height;
    public TickHandler[] TickHandlers = new TickHandler[TICKS_PER_SECOND];
    public const int TICKS_PER_SECOND = 60;
    public int tickNumber{get; private set;} = 0;
    public List<WorldStepData> Data;
    public event EventHandler? OnScreenUpdate;
    public delegate void StepHandler();
    public event StepHandler? OnStep; 
    public readonly WorldStepData currentStepData;

    public World(int mapWidth, int mapHeight)
    {
        _screenSurface = new ScreenSurface(mapWidth, mapHeight);
        // _mapSurface.FontSize = (15, 15);
        entities = new EntityManager();
        _screenSurface.UseMouse = false;

        FillBackground();

        grid = new WorldObject[mapWidth, mapHeight];
        Data = new List<WorldStepData>();
        currentStepData = new WorldStepData(tickNumber, 0, 0, new Dictionary<string, int>());

        for (int i = 0; i < TICKS_PER_SECOND; i++)
        {
            TickHandlers[i] = new TickHandler();
        }

        // Script testScript = new Script();
        // try {
        //     testScript.LoadFile("scripts\\findFood.lua", null, "slowHai");
        // } catch (SyntaxErrorException e) {
        //     Debug.WriteLine(e.DecoratedMessage);
        // }
        // Cell testCell = new Cell(testScript, _screenSurface.Surface.Area.Center + (0, -10), this);
        // SpawnCellEntity(testCell);

        Script scriptCharge = new Script();
        try {
            scriptCharge.LoadFile("scripts\\findFoodCharge.lua", null, "Charge");
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        SpawnCellEntity(new Cell(scriptCharge, _screenSurface.Surface.Area.Center + (0, 10), this));

        // Script scriptCharge2 = new Script();
        // try {
        //     scriptCharge2.LoadFile("scripts\\findFoodCharge2.lua", null);
        // } catch (SyntaxErrorException e) {
        //     Debug.WriteLine(e.DecoratedMessage);
        // }
        // SpawnCellEntity(new Cell(scriptCharge2, _screenSurface.Surface.Area.Center + (10, 0), this));

        //Spawn food
        PopulateFood(0.01);
        TickHandlers[0].OnTick += PopulateFood;

        Game.Instance.FrameUpdate += (sender, e) => {
            EventHandler? OSU = OnScreenUpdate;
            OnScreenUpdate = null;
            OSU?.Invoke(this, System.EventArgs.Empty);
        };

        _screenSurface.SadComponents.Add(entities);
    }

    public void AddEntityToUpdate(DisplayEntity entity) {
        OnScreenUpdate += (sender, e) => {
            entity.Update(sender, GameHost.Instance);
        };
    }

    public void AddSpawnCellEntityToUpdate(Cell cell) {
        OnScreenUpdate += (sender, e) => {
            if (cell.IsAlive) {
                SpawnCellEntity(cell);
            }
        };
    }

    public void AddSpawnFoodEntityToUpdate(Food food) {
        OnScreenUpdate += (sender, e) => {
            if (food.IsAlive) {
                _ = new FoodEntity(food);
            }
        };
    }

    public void Tick() {
        int tickIndex = tickNumber % TICKS_PER_SECOND;
        TickHandlers[tickIndex].FireTick();

        tickNumber++;
        if (tickIndex == 0) {
            Data.Add(new WorldStepData(this));
            OnStep?.Invoke();
        }
    }

    public void PopulateFood() {
        PopulateFood(0.0001);
    }

    public void PopulateFood(double density) {
        int mapArea = Width * Height;
        int numFoodToSpawn = (int)(mapArea * density);
        for (int i = 0; i < numFoodToSpawn; i++) {
            Point pos = GetRandomPosition(Width, Height);
            if (IsEmpty(pos)) {
                AddSpawnFoodEntityToUpdate(new Food(30, pos, this));
            }
        }
    }

    public bool IsInBounds(Point position) {
        return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
    }

    public bool IsEmpty(Point position) {
        return grid[position.X, position.Y] == null;
    }

    public void SpawnCellEntity(Cell cell) {
        _ = new CellEntity(cell);
    }

    public void AddEntity(DisplayEntity entity) {
        if (entity.InScene) {
            throw new Exception("Entity " + entity.Name + " already in scene");
        }
        entities.Add(entity);
    }

    public void RemoveEntity(DisplayEntity entity) {
        if (!entities.Remove(entity)) {
            throw new Exception("Entity " + entity.Name + " not found in entities");
        }
    }

    public static Point GetRandomPosition(int width, int height)
    {
        return (Game.Instance.Random.Next(0, width), Game.Instance.Random.Next(0, height));
    }

    public void MoveScreen(Point delta) {
        _screenSurface.Position += delta;
    }

    private void FillBackground()
    {
        Color[] colors = new[] {Color.LightBlue, Color.LightGray};
        float[] colorStops = new[] { 0f, 1f };

        Algorithms.GradientFill(_screenSurface.FontSize,
                                _screenSurface.Surface.Area.Center,
                                _screenSurface.Surface.Width / 3,
                                15,
                                _screenSurface.Surface.Area,
                                new Gradient(colors, colorStops),
                                (x, y, color) => _screenSurface.Surface[x, y].Background = color);
    }
    
}