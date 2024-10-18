class WorldStepData {
    public int Frame { get; set; }
    public int FoodQuantity { get; set; }
    public readonly Dictionary<string, int> Populations = new Dictionary<string, int>();

    public WorldStepData(World world) {
        Frame = world.tickNumber / World.TICKS_PER_SECOND;
        FoodQuantity = world.FoodQuantity;
    }
}