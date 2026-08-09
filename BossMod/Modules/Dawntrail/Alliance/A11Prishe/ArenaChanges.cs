namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module, (uint)AID.Thornbite)
{
    public bool Active => _aoe.Length != 0 || Arena.Bounds is not ArenaBoundsSquare;
    private AOEInstance[] _aoe = [];
    public uint Curstate;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    private static Square[] GetDefaultSquare() => [new(new(800f, 400f), 35f)];
    public static Square[] GetMiddleENVC00020001() => [new(new(795f, 405f), 10f), new(new(805f, 395f), 10f)];
    private static Rectangle[] GetDifferenceENVC00020001() => [.. GetMiddleENVC00020001(), new Rectangle(new(810f, 430f), 15f, 5f),
    new Rectangle(new(830f, 420f), 5f, 15f), new Rectangle(new(790f, 370f), 15f, 5f), new Rectangle(new(770f, 380f), 5f, 15f)];
    public static Square[] GetMiddleENVC02000100() => [new(new(795f, 395f), 10f), new(new(805f, 405f), 10f)];
    private static Shape[] GetDifferenceENVC02000100() => [.. GetMiddleENVC02000100(), new Rectangle(new(820f, 370f), 15f, 5f),
    new Rectangle(new(830f, 390f), 5f, 15f), new Rectangle(new(780f, 430f), 15f, 5f), new Rectangle(new(770f, 410f), 5f, 15f)];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                SetAOE(new(Arena.Center, GetDefaultSquare(), GetDifferenceENVC00020001()));
                break;
            case 0x02000100u:
                SetAOE(new(Arena.Center, GetDefaultSquare(), GetDifferenceENVC02000100()));
                break;
            case 0x00200010u:
                SetArena(new(GetDifferenceENVC00020001()), state);
                break;
            case 0x08000400u:
                SetArena(new(GetDifferenceENVC02000100()), state);
                break;
            case 0x00080004u or 0x00800004u:
                Arena.Bounds = new ArenaBoundsSquare(35f);
                break;
        }

        void SetArena(ArenaBoundsCustom bounds, uint state)
        {
            Arena.Bounds = bounds;
            Curstate = state;
            _aoe = [];
        }

        void SetAOE(AOEShapeCustom shape) => _aoe = [new(shape, Arena.Center, default, WorldState.FutureTime(5d), shapeDistance: shape.Distance(Arena.Center, default))];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // no need to generate a hint here, we generate a special hint in CrystallineThornsHint
    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

sealed class CrystallineThornsHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                SetAOE(new(Arena.Center, ArenaChanges.GetMiddleENVC00020001(), invertForbiddenZone: true));
                break;
            case 0x02000100u:
                SetAOE(new(Arena.Center, ArenaChanges.GetMiddleENVC02000100(), invertForbiddenZone: true));
                break;
            case 0x00200010u:
            case 0x08000400u:
                _aoe = [];
                break;
        }
        void SetAOE(AOEShapeCustom shape) => _aoe = [new(shape, Arena.Center, default, WorldState.FutureTime(5d), Colors.SafeFromAOE, shapeDistance: shape.Distance(Arena.Center, default))];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        if (!aoe.Check(actor.Position))
        {
            hints.Add("Go into middle to prepare for knockback!");
        }
    }
}
