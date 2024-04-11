namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SurgingWavesArenaChange(BossModule module) : BossComponent(module)
{
    public enum ArenaShape { Normal, ExtendWest, ExtendEast }
    public ArenaShape Shape { get; private set; }
    private bool Shockwave;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state is 0x00200001 or 0x02000001 && index == 0x49)
        {
            Module.Arena.Bounds = new ArenaBoundsRect(new(0, -900), 19, 29);
            Shape = ArenaShape.Normal;
        }
        if (state == 0x00800040 && index == 0x49)
            Shape = ArenaShape.ExtendWest;
        if (state == 0x08000400 && index == 0x49)
            Shape = ArenaShape.ExtendEast;
    }

    public override void Update()
    {
        if (Shockwave && Shape == ArenaShape.Normal)
            Shockwave = false;
        if (Shockwave && Shape == ArenaShape.ExtendWest)
            Module.Arena.Bounds = new ArenaBoundsRect(new(-40, -900), 40, 10);
        if (Shockwave && Shape == ArenaShape.ExtendEast)
            Module.Arena.Bounds = new ArenaBoundsRect(new(40, -900), 40, 10);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Shockwave)
            Shockwave = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var c = Shape == ArenaShape.ExtendWest ? 1 : -1;
        if (Shape != ArenaShape.Normal)
        {
            Arena.PathLineTo(new(c * -19, -890));
            Arena.PathLineTo(new(c * -19, -871));
            Arena.PathLineTo(new(c * 19, -871));
            Arena.PathLineTo(new(c * 19, -929));
            Arena.PathLineTo(new(c * -19, -929));
            Arena.PathLineTo(new(c * -19, -910));
            Arena.PathStroke(false, ArenaColor.Border, 2);
            if (Module.FindComponent<Shockwave>()!.Sources(pcSlot, pc).Any())
                Arena.ZoneCone(new(-6 * c, -900), 0, 6, c * -90.Degrees(), 22.5f.Degrees(), ArenaColor.SafeFromAOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var c = Shape == ArenaShape.ExtendWest ? -1 : 1;
        base.AddAIHints(slot, actor, assignment, hints);
        if (Module.FindComponent<Shockwave>()!.Sources(slot, actor).Any())
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(new(4 * c, -900), 6, c * 90.Degrees(), 22.5f.Degrees())); //for some reason I need to use different coords and angle for this than when drawing the ZoneCone, otherwise it wont appear in the correct spot?
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var c = Shape == ArenaShape.ExtendWest ? -1 : 1;
        if (Module.FindComponent<Shockwave>()!.Sources(slot, actor).Any())
        {
            if (!actor.Position.InCircleCone(new(4 * c, -900), 6, c * 90.Degrees(), 22.5f.Degrees())) //for some reason I need to use different coords and angle for this than when drawing the ZoneCone, otherwise it wont appear in the correct spot?
                hints.Add("Wait in cone for knockback!");
            else
                hints.Add("Stay in cone!", false);
        }
    }
}

class SeaFoam(BossModule module) : Components.PersistentVoidzone(module, 1.5f, m => m.Enemies(OID.SeaFoam).Where(x => !x.IsDead));
class SurgingWave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SurgingWaveAOE), new AOEShapeCircle(6));
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Shockwave), 68, true, kind: Kind.AwayFromOrigin);
class ShockwaveRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Shockwave));
