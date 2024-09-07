class FoodEntity : DisplayEntity
{
    public Food food;
    public FoodEntity(int nutrition, Point position, World world)
        : this(new Food(nutrition, position, world)) {}

    public FoodEntity(Food food)
        : base(new ColoredGlyph(Color.Green, Color.Transparent, 6), food.Position)
    {
        this.food = food;

        food.OnUpdate += () => {
            if (IsSubscribedToFrameUpdate) return;
            IsSubscribedToFrameUpdate = true;
            food.World.AddEntityToUpdate(this);
        };
        food.World.AddEntityToUpdate(this);
    }

    public override WorldObject GetWorldObject()
    {
        return food;
    }
}