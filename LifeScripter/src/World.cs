using System.Diagnostics;
using MoonSharp.Interpreter;
using SadConsole.Entities;

namespace LifeScripter.Backend;

class World
{
    private ScreenSurface _screenSurface;

    private EntityManager entities;

    public ScreenSurface SurfaceObject => _screenSurface;
    public GameObject? UserControlledObject { get; set; }

    public GameObject?[,] grid;

    public int Width => _screenSurface.Surface.Width;
    public int Height => _screenSurface.Surface.Height;

    public delegate void TickHandler();
    public event TickHandler? OnTick;

    public World(int mapWidth, int mapHeight)
    {
        _screenSurface = new ScreenSurface(mapWidth, mapHeight);
        // _mapSurface.FontSize = (15, 15);
        entities = new EntityManager();
        _screenSurface.UseMouse = false;

        FillBackground();

        grid = new GameObject[mapWidth, mapHeight];

        Script testScript = new Script();
        try {
            testScript.LoadFile("scripts\\findFood.lua", null);
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell c = new Cell(testScript, _screenSurface.Surface.Area.Center + (-10, 0), this);
        AddEntity(new CellObject(c, this));


        Script script2 = new Script();
        try {
            script2.LoadFile("scripts\\findFood2.lua", null);
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell c2 = new Cell(script2, _screenSurface.Surface.Area.Center + (10, 0), this);
        AddEntity(new CellObject(c2, this));

        Script scriptNoReproduce = new Script();
        try {
            scriptNoReproduce.LoadFile("scripts\\findFoodNoReproduce.lua", null);
        } catch (SyntaxErrorException e) {
            Debug.WriteLine(e.DecoratedMessage);
        }
        Cell cNoReproduce = new Cell(scriptNoReproduce, _screenSurface.Surface.Area.Center + (0, 10), this);
        AddEntity(new CellObject(cNoReproduce, this));

        for (int i = 0; i < 100; i++) {
            Point pos = GetRandomPosition(mapWidth, mapHeight);
            if (IsEmpty(pos)) {
                AddEntity(new FoodObject(30, pos, this));
            }
        }
        
        // UserControlledObject = new GameObject(new ColoredGlyph(Color.Black, Color.Transparent, 2), _screenSurface.Surface.Area.Center);
        // entities.Add(UserControlledObject);


        _screenSurface.SadComponents.Add(entities);
    }

    public void Tick() {
        OnTick?.Invoke();

        // if (Game.Instance.Random.NextDouble() < 0.5) {
            AddEntity(new FoodObject(30, GetRandomPosition(Width, Height), this));
        // }
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
            throw new Exception("Entity not found in grid");
        }
        if (!entities.Remove(entity)) {
            throw new Exception("Entity not found in entities");
        }
        grid[entity.Position.X, entity.Position.Y] = null;
    }

    public static Point GetRandomPosition(int width, int height)
    {
        return (Game.Instance.Random.Next(0, width), Game.Instance.Random.Next(0, height));
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