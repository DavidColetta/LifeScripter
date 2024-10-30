class WorldStepData {
    public int Frame { get; set; }
    public int FoodQuantity { get; set; }
    public int TotalPopulation { get; set; }
    public readonly Dictionary<string, int> Populations;
    public WorldStepData(int frame, int foodQuantity, int totalPopulation, Dictionary<string, int> populations) {
        Frame = frame;
        FoodQuantity = foodQuantity;
        TotalPopulation = totalPopulation;
        Populations = populations;
    }

    public WorldStepData(World world) {
        Frame = world.tickNumber / World.TICKS_PER_SECOND;
        FoodQuantity = world.currentStepData.FoodQuantity;
        TotalPopulation = world.currentStepData.TotalPopulation;
        Populations = world.currentStepData.Populations.ToDictionary(entry => entry.Key, entry => entry.Value);
    }
}