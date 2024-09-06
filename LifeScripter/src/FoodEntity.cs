class FoodEntity : EntityObject
{
    public Food food;
    public FoodEntity(int nutrition, Point position, World world)
        : this(new Food(nutrition, position, world)) {}

    public FoodEntity(Food food)
        : base(new ColoredGlyph(Color.Green, Color.Transparent, 6), food.Position)
    {
        this.food = food;
    }

    public override WorldObject GetWorldObject()
    {
        return food;
    }
}