using System.Diagnostics;
using MoonSharp.Interpreter;
using SadConsole.Entities;

namespace LifeScripter.Backend;

class World
{
    private readonly ScreenSurface _screenSurface;

    private readonly EntityManager entities;

    public ScreenSurface SurfaceObject => _screenSurface;
    public GameObject? UserControlledObject { get; set; }

    public GameObject?[,] grid;

    public int Width => _screenSurface.Surface.Width;
    public int Height => _screenSurface.Surface.Height;
    public TickHandler[] TickHandlers = new TickHandler[TICKS_PER_SECOND];
    public const int TICKS_PER_SECOND = 100;
    int tickNumber = 0;

    public World(int mapWidth, int mapHeight)
    {
        _screenSurface = new ScreenSurface(mapWidth, mapHeight);
        // _mapSurface.FontSize = (15, 15);
        entities = new EntityManager();
        _screenSurface.UseMouse = false;

        FillBackground();

        grid = new GameObject[mapWidth, mapHeight];

        for (int i = 0; i < TICKS_PER_SECOND; i++)
        {
            TickHandlers[i] = new TickHandler();
        }

        Script testScript = new Script();
        try {
            testScript.LoadFile("scripts\\findFood.lua", null, "slowHai");
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell c = new Cell(testScript, _screenSurface.Surface.Area.Center + (-10, 0), this);
        c.ticksPerSecond = 1;
        AddEntity(new CellObject(c, this));


        Script script2 = new Script();
        try {
            script2.LoadFile("scripts\\findFood.lua", null, "hai");
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell c2 = new Cell(script2, _screenSurface.Surface.Area.Center + (10, 0), this);
        c2.ticksPerSecond = 2;
        AddEntity(new CellObject(c2, this));

        // Script scriptNoReproduce = new Script();
        // try {
        //     scriptNoReproduce.LoadFile("scripts\\findFoodNoReproduce.lua", null);
        // } catch (SyntaxErrorException e) {
        //     Debug.WriteLine(e.DecoratedMessage);
        // }
        // Cell cNoReproduce = new Cell(scriptNoReproduce, _screenSurface.Surface.Area.Center + (0, 10), this);
        // AddEntity(new CellObject(cNoReproduce, this));

        Script scriptHibernate = new Script();
        try {
            scriptHibernate.LoadFile("scripts\\findFoodHibernate.lua", null);
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell cHibernate = new Cell(scriptHibernate, _screenSurface.Surface.Area.Center + (0, -10), this);
        AddEntity(new CellObject(cHibernate, this));

        //Spawn food
        int mapArea = Width * Height;
        int numFoodToSpawn = (int)(mapArea * 0.01);
        for (int i = 0; i < numFoodToSpawn; i++) {
            Point pos = GetRandomPosition(Width, Height);
            if (IsEmpty(pos)) {
                AddEntity(new FoodObject(30, pos, this));
            }
        }
        TickHandlers[0].OnTick += SpawnFood;
        
        // UserControlledObject = new GameObject(new ColoredGlyph(Color.Black, Color.Transparent, 2), _screenSurface.Surface.Area.Center);
        // entities.Add(UserControlledObject);


        _screenSurface.SadComponents.Add(entities);        
    }

    public void Tick() {
        TickHandlers[tickNumber % TICKS_PER_SECOND].FireTick();

        tickNumber++;
    }

    public void SpawnFood() {
        int mapArea = Width * Height;
        int numFoodToSpawn = (int)(mapArea * 0.0001);
        for (int i = 0; i < numFoodToSpawn; i++) {
            Point pos = GetRandomPosition(Width, Height);
            if (IsEmpty(pos)) {
                AddEntity(new FoodObject(30, pos, this));
            }
        }
    }

    public bool IsInBounds(Point position) {
        return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
    }

    public bool IsEmpty(Point position) {
        return grid[position.X, position.Y] == null;
    }

    public bool AddEntity(GameObject entity) {
        if (!IsInBounds(entity.Position) || !IsEmpty(entity.Position)) {
            return false;
        }
        entities.Add(entity);
        grid[entity.Position.X, entity.Position.Y] = entity;
        return true;
    }

    public void RemoveEntity(GameObject entity) {
        if (grid[entity.Position.X, entity.Position.Y] != entity) {
            throw new Exception("Entity " + entity.Name + " not found in grid");
        }
        if (!entities.Remove(entity)) {
            throw new Exception("Entity " + entity.Name + " not found in entities");
        }
        grid[entity.Position.X, entity.Position.Y] = null;
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