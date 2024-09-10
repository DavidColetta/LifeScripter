using System.Diagnostics;
using System.Text;
using MoonSharp.Interpreter;
partial class Cell : WorldObject
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