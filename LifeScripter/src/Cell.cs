using System.Diagnostics;
using System.Text;
using MoonSharp.Interpreter;
class Cell : WorldObject
{
    //MoonSharp Script variables
    public readonly Script script;
    readonly Closure? behavior;
    readonly Closure? onTick;
    readonly Table memory;
    //SadConsole variables
    public readonly ColoredGlyph Appearance;
    //Cell variables
    static readonly int SIGHT_DISTANCE = 14;
    static readonly int MAX_ENERGY = 300;
    bool exhausted = true;
    int energy = MAX_ENERGY;

    public int TicksPerSecond{get; private set;} = 4;

    public Cell(Cell parent, Point position) : this(parent.script, position, parent.World, parent.Appearance) {
        ChangeSpeed(parent.TicksPerSecond);
    }
    
    public Cell(Script behaviorScript, Point position, World world, ColoredGlyph defaultAppearance)
        : base(position, world) {
        this.script = behaviorScript;
        Appearance = defaultAppearance;
        memory = new Table(script);
        memory.RegisterConstants();
        memory.RegisterCoreModules(CoreModules.Preset_SoftSandbox);//This could cause problems with Reload
        RegisterCellFunctions();

        SubscribeToTicks();

        DynValue scriptCallback;
        try {
            scriptCallback = script.Reload(memory);
        } catch (SyntaxErrorException e) {
            Debug.WriteLine("Syntax error in script reload: " + e.Source + " " + e.DecoratedMessage);
            return;
        }
        if (scriptCallback.Type != DataType.Function) {
            Debug.WriteLine("Script reload did not return a closure (this should never happen)");
            return;
        }
        behavior = scriptCallback.Function;

        //Call the initial setup.
        DynValue tickCallback;
        try {
            tickCallback = behavior.Call();
        } catch (ScriptRuntimeException e) {
            Debug.WriteLine(e.DecoratedMessage);
            return;
        }
        IsAlive = true;
        if (scriptCallback.Type != DataType.Function) {
            Debug.WriteLine("Script did not return a function");
            return;
        }
        onTick = tickCallback.Function;
    }
    
    public Cell(Script behaviorScript, Point position, World world) : this(behaviorScript, position, world, GetAppearanceFromHashedScript(behaviorScript)) {}

    private void RegisterCellFunctions() {
        memory["changeGlyph"] = (Action<int>)ChangeGlyph;
        memory["changeColor"] = (Action<int, int, int>)ChangeColor;

        memory["getEnergy"] = (Func<int>)GetEnergy;

        memory["directions"] = new Table(script) {
            ["north"] = Direction.NORTH,
            ["east"] = Direction.EAST,
            ["south"] = Direction.SOUTH,
            ["west"] = Direction.WEST
        };
        memory["look"] = (Func<Direction, Table>)Look;
        memory["move"] = (Func<Direction, bool>)Move;
        memory["eat"] = (Func<Direction, bool>)Eat;
        memory["reproduce"] = (Func<Direction, bool>)Reproduce;
        memory["changeSpeed"] = (Action<int>)ChangeSpeed;
        memory["getMaxEnergy"] = () => MAX_ENERGY;
    }

    public void Tick() {
        if (energy <= 0) {
            Die();
        }
        if (!IsAlive) {
            return;
        }
        exhausted = false;
        try {
            onTick?.Call();
        } catch (ScriptRuntimeException e) {
            Debug.WriteLine("Error in script: " + e.DecoratedMessage);
            Die();
            return;
        }
        energy--;
        FireOnUpdate();
    }
    
    public void ChangeGlyph(int glyph) {
        Appearance.Glyph = glyph;
    }

    public void ChangeColor(int r, int g, int b) {
        Appearance.Foreground = new Color(r, g, b);
    }

    public int GetEnergy() {
        return energy;
    }

    public Table Look(Direction direction) {
        Table lookResult = new Table(script);
        lookResult["type"] = "empty";
        Point lookPositioin = Position;
        for (int i = 0; i < SIGHT_DISTANCE; i++) {
            switch (direction) {
                case Direction.NORTH:
                    lookPositioin += (0, -1);
                    break;
                case Direction.EAST:
                    lookPositioin += (1, 0);
                    break;
                case Direction.SOUTH:
                    lookPositioin += (0, 1);
                    break;
                case Direction.WEST:
                    lookPositioin += (-1, 0);
                    break;
                default:
                    throw new ScriptRuntimeException("Invalid direction");
            }
            if (lookPositioin.X < 0 || lookPositioin.X >= World.Width || lookPositioin.Y < 0 || lookPositioin.Y >= World.Height) {
                lookResult["type"] = "wall";
                lookResult["distance"] = i;
                return lookResult;
            }
            WorldObject? entity = World.grid[lookPositioin.X, lookPositioin.Y];
            if (entity != null) {
                if (entity is Food) {
                    lookResult["type"] = "food";
                    lookResult["distance"] = i;
                    return lookResult;
                }
                lookResult["type"] = "entity";
                lookResult["distance"] = i;
                // lookResult["glyph"] = entity.AppearanceSingle?.Appearance.Glyph;
                // lookResult["color"] = entity.AppearanceSingle?.Appearance.Foreground;
                return lookResult;
            }
        }
        return lookResult;
    }

    public bool Move(Direction direction) {
        if (exhausted) {
            return false;
        }
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            if (!Eat(direction)) {
                return false;
            }
        }
        Reposition(newPosition);
        energy--;
        exhausted = true;
        return true;
    }

    public bool Eat(Direction direction) {
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            Food? food = World.grid[newPosition.X, newPosition.Y] as Food;
            if (food != null) {//Eat the food
                food.Die();
                energy += food.Nutrition;
                if (energy > MAX_ENERGY) {
                    energy = MAX_ENERGY;
                }
                return true;
            } else {//if there is something else in the way, don't eat
                return false;
            }
        }
        return false;
    }

    public bool Reproduce(Direction direction) {
        if (energy < 2) {
            return false;
        }
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            return false;
        }
        Cell newCell = new Cell(this, newPosition);
        newCell.energy = energy / 2;
        World.AddSpawnCellEntityToUpdate(newCell);
        energy = energy / 2;
        return true;
    }

    public void ChangeSpeed(int newSpeed) {
        if (TicksPerSecond == newSpeed) {
            return;
        }
        int oldSpeed = TicksPerSecond;
        TicksPerSecond = newSpeed;
        UnsubscribeFromTicks(oldSpeed);
        SubscribeToTicks();
    }

    private void SubscribeToTicks() {
        double tickInterval = (double)World.TICKS_PER_SECOND / TicksPerSecond;
        for (double i = 0; i < World.TICKS_PER_SECOND; i += tickInterval) {
            World.TickHandlers[(int)i] += Tick;
        }
    }

    private void UnsubscribeFromTicks(int speed) {
        double tickInterval = (double)World.TICKS_PER_SECOND / speed;
        for (double i = 0; i < World.TICKS_PER_SECOND; i += tickInterval) {
            World.TickHandlers[(int)i] -= Tick;
        }
    }

    public override void Die() {
        if (!IsDead) {
            UnsubscribeFromTicks(TicksPerSecond);
        }
        base.Die();
    }

    public enum Direction {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }

    public static ColoredGlyph GetAppearanceFromHashedScript(Script s) {
        uint hashCode = SemiStableHashCode(s.GetSourceCode(1).Name);
        //uint hashCode = SemiStableHashCode(s.GetSourceCode(1).Code);
        // System.Console.WriteLine(hashCode);
        int glyph = (int)((hashCode % 143) + 1);
        if (glyph == 45)
            glyph = 157;
        else if (glyph == 46)
            glyph = 208;
        else if (glyph == 32)
            glyph = 219;
        else if (glyph == 95)
            glyph = 146;
        else if (glyph > 128) {
            glyph += 96;
        }
        // System.Console.WriteLine(glyph);
        return new ColoredGlyph(new Color(new Color(hashCode), 1.0f), Color.Transparent, glyph);
    }

    public static uint SemiStableHashCode(string input)
    {
        unchecked
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input.Normalize(NormalizationForm.FormKD));
            uint result = 17;
            foreach (byte b in bytes)
                result = result * 23 + b;
            return result;
        }
    }
}