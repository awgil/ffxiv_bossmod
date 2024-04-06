namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SurgingWavesArenaChange : BossComponent
{
    public enum Arena { Normal, ExtendWest, ExtendEast }
    public Arena Shape { get; private set; }
    private bool Shockwave;

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state is 0x00200001 or 0x02000001 && index == 0x49)
        {
            module.Arena.Bounds = new ArenaBoundsRect(new(0, -900), 19, 29);
            Shape = Arena.Normal;
        }
        if (state == 0x00800040 && index == 0x49)
            Shape = Arena.ExtendWest;
        if (state == 0x08000400 && index == 0x49)
            Shape = Arena.ExtendEast;
    }

    public override void Update(BossModule module)
    {

        if (Shockwave && Shape == Arena.Normal)
            Shockwave = false;
        if (Shockwave && Shape == Arena.ExtendWest)
            module.Arena.Bounds = new ArenaBoundsRect(new(-40, -900), 40, 10);
        if (Shockwave && Shape == Arena.ExtendEast)
            module.Arena.Bounds = new ArenaBoundsRect(new(40, -900), 40, 10);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Shockwave)
            Shockwave = true;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var c = Shape == Arena.ExtendWest ? 1 : -1;
        if (Shape != Arena.Normal)
        {
            arena.PathLineTo(new(c * -19, -890));
            arena.PathLineTo(new(c * -19, -871));
            arena.PathLineTo(new(c * 19, -871));
            arena.PathLineTo(new(c * 19, -929));
            arena.PathLineTo(new(c * -19, -929));
            arena.PathLineTo(new(c * -19, -910));
            arena.PathStroke(false, ArenaColor.Border, 2);
            if (module.FindComponent<Shockwave>()!.Sources(module, pcSlot, pc).Any())
                arena.ZoneCone(new(-6 * c, -900), 0, 6, c * -90.Degrees(), 22.5f.Degrees(), ArenaColor.SafeFromAOE);
        }
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var c = Shape == Arena.ExtendWest ? -1 : 1;
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (module.FindComponent<Shockwave>()!.Sources(module, slot, actor).Any())
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(new(4 * c, -900), 6, c * 90.Degrees(), 22.5f.Degrees())); //for some reason I need to use different coords and angle for this than when drawing the ZoneCone, otherwise it wont appear in the correct spot?
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var c = Shape == Arena.ExtendWest ? -1 : 1;
        if (module.FindComponent<Shockwave>()!.Sources(module, slot, actor).Any())
        {
            if (!actor.Position.InCircleCone(new(4 * c, -900), 6, c * 90.Degrees(), 22.5f.Degrees())) //for some reason I need to use different coords and angle for this than when drawing the ZoneCone, otherwise it wont appear in the correct spot?
                hints.Add("Wait in cone for knockback!");
            else
                hints.Add("Stay in cone!", false);
        }
    }
}

class SeaFoam : Components.PersistentVoidzone
{
    public SeaFoam() : base(1.5f, m => m.Enemies(OID.SeaFoam).Where(x => !x.IsDead)) { }
}

class SurgingWave : Components.SelfTargetedAOEs
{
    public SurgingWave() : base(ActionID.MakeSpell(AID.SurgingWaveAOE), new AOEShapeCircle(6)) { }
}

class Shockwave : Components.KnockbackFromCastTarget
{
    public Shockwave() : base(ActionID.MakeSpell(AID.Shockwave), 68, true, kind: Kind.AwayFromOrigin) { }
}

class ShockwaveRaidwide : Components.RaidwideCast
{
    public ShockwaveRaidwide() : base(ActionID.MakeSpell(AID.Shockwave)) { }
}
