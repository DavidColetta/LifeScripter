using System.Numerics;

class ChartDisplay
{

    public ScreenSurface SurfaceObject;
    public static readonly int BORDER_MARGIN = 2;
    private readonly World _world;

    int PopulationScale = 5;

    int chartWidth;
    int chartHeight;
    int maxChartIncrement = 6;
    int maxQuantity = 1;
    TimeSpan lastUpdate = DateTime.Now.TimeOfDay;
    public ChartDisplay(int _width, int _height, World world)
    {
        _world = world;
        chartWidth = _width;
        chartHeight = _height;
        SurfaceObject = new ScreenSurface(chartWidth+BORDER_MARGIN, chartHeight+BORDER_MARGIN);
        // SurfaceObject.Fill(Color.Black, Color.LightBlue, 0);
        SurfaceObject.Print(0, 0, "Chart Display");

        SurfaceObject.DrawBox(new Rectangle(BORDER_MARGIN, BORDER_MARGIN, chartWidth, chartHeight), 
            ShapeParameters.CreateFilled(new ColoredGlyph(Color.White, Color.Black, 0), new ColoredGlyph(Color.Black, Color.LightBlue, 0)));

        SurfaceObject.Print(1, 1, "Chart Data");
        
        world.OnStep += Update;
    }

    public void Update()
    {
        if (DateTime.Now.TimeOfDay - lastUpdate < TimeSpan.FromMilliseconds(25)) {
            return;
        }
        lastUpdate = DateTime.Now.TimeOfDay;

        //Clear chart
        SurfaceObject.DrawBox(new Rectangle(BORDER_MARGIN, BORDER_MARGIN, chartWidth, chartHeight), 
            ShapeParameters.CreateFilled(new ColoredGlyph(Color.White, Color.Black, 0), new ColoredGlyph(Color.Black, Color.LightBlue, 0)));
        
        // if (_world.currentStepData.FoodQuantity > maxQuantity) {
        //     maxQuantity = _world.currentStepData.FoodQuantity;
        // }
        
        int chartDataCount = _world.Data.Count;
        int increment = chartDataCount / (chartWidth-2) + 1;
        
        int chartDataStart = 0;
        if (increment > maxChartIncrement) {
            increment = maxChartIncrement;
            chartDataStart = chartDataCount - ((chartWidth-2) * increment);
        }
    
        int _maxFoodQuantity = 1;
        for (int i = 0; (i*increment)+chartDataStart < chartDataCount;  i++) {
            int maxHeight = chartHeight - 2;
            int foodQuantity = _world.Data[(i*increment)+chartDataStart].FoodQuantity;
            if (foodQuantity > _maxFoodQuantity) {
                _maxFoodQuantity = foodQuantity;
            }
            int height = (int)(foodQuantity * maxHeight / maxQuantity);
            SurfaceObject.Fill(new Rectangle(BORDER_MARGIN+i+1, BORDER_MARGIN+1+(maxHeight-height), 1, height), Color.Black, Color.Green, 0);
        }

        int _maxTotalPopulation = 1;
        for (int i = 0; (i*increment)+chartDataStart < chartDataCount;  i++) {
            int maxHeight = chartHeight - 2;
            int populationQuantity = _world.Data[(i*increment)+chartDataStart].TotalPopulation * PopulationScale;
            if (populationQuantity > _maxTotalPopulation) {
                _maxTotalPopulation = populationQuantity;
            }
            int height = (int)(populationQuantity * maxHeight / maxQuantity);
            SurfaceObject.Fill(new Rectangle(BORDER_MARGIN+i+1, BORDER_MARGIN+1+(maxHeight-height), 1, height), Color.Black, Color.Black, 0);
        }

        // int maxPopulationsCount = 1;
        // foreach (var population in _world.Data[chartDataCount-1].Populations) {
        //     int _maxPopulation = 1;
        //     for (int i = 0; (i*increment)+chartDataStart < chartDataCount;  i++) {
        //         int maxHeight = chartHeight - 2;
        //         int populationValue = _world.Data[(i*increment)+chartDataStart].Populations[population.Key] * 4;
        //         if (populationValue > _maxPopulation) {
        //             _maxPopulation = populationValue;
        //         }
        //         int height = (int)(populationValue * maxHeight / maxQuantity);
        //         Color color = Cell.GetHashedAppearance(population.Key).Foreground;
        //         // color = new Color(color, 50);
        //         SurfaceObject.Fill(new Rectangle(BORDER_MARGIN+i+1, BORDER_MARGIN+1+(maxHeight-height), 1, height), Color.Black, color, 0);
        //     }
        //     if (_maxPopulation > maxPopulationsCount)
        //         maxPopulationsCount = _maxPopulation;
        // }

        maxQuantity = _maxFoodQuantity;
        if (_maxTotalPopulation > maxQuantity) {
            maxQuantity = _maxTotalPopulation;
        }
    }
}