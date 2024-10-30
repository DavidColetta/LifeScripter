class Food : WorldObject
{
    public int Nutrition = 10;

    public Food(int nutrition, Point position, World world) : base(position, world)
    {
        Nutrition = nutrition;
        IsAlive = true;

        world.currentStepData.FoodQuantity++;
    }

    public override void Die() {
        if (!IsDead) {
            World.currentStepData.FoodQuantity--;
        }
        base.Die();
    }
}