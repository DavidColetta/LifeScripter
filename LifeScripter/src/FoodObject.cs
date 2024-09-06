using LifeScripter.Backend;

class FoodObject : GameObject
{
    public int Nutrition = 10;
    public FoodObject(int nutrition, Point position, World world)
        : base(new ColoredGlyph(Color.Green, Color.Transparent, 6), position, world)
    {
        isAlive = true;
        Nutrition = nutrition;
    }
}