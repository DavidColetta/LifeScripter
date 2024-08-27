using System.Diagnostics.CodeAnalysis;
using SadConsole.Entities;

namespace LifeScripter.Backend;

class World
{
    private ScreenSurface _screenSurface;

    private EntityManager entities;

    public ScreenSurface SurfaceObject => _screenSurface;
    public GameObject UserControlledObject { get; set; }

    public World(int mapWidth, int mapHeight)
    {
        _screenSurface = new ScreenSurface(mapWidth, mapHeight);
        // _mapSurface.FontSize = (15, 15);
        entities = new EntityManager();
        _screenSurface.UseMouse = false;

        FillBackground();

        UserControlledObject = new GameObject(new ColoredGlyph(Color.Black, Color.Transparent, 2), _screenSurface.Surface.Area.Center);
        entities.Add(UserControlledObject);


        _screenSurface.SadComponents.Add(entities);
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