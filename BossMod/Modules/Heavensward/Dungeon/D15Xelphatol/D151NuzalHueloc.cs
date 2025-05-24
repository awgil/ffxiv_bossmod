namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D151NuzalHueloc;

public enum OID : uint
{
    Boss = 0x179B, // R1.500, x1
    FallenRock = 0xD25, // R0.500, x1
    IxaliStitcher = 0x179C, // R1.080, x0 (spawn during fight)
    FloatingTurret = 0x179E, // R1.000, x0 (spawn during fight)
    Airstone = 0x179D, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    ShortBurst = 6598, // Boss->player, no cast, single-target
    WindBlast = 6599, // Boss->self, 3.0s cast, range 60+R width 8 rect
    Lift = 6601, // Boss->self, 3.0s cast, single-target
    AirRaid = 6602, // Boss->location, no cast, range 50 circle
    HotBlast = 6604, // 179E->self, 6.0s cast, range 25 circle
    LongBurst = 6600, // Boss->player, 3.0s cast, single-target
    ShortBurstAdds = 6603, // 179E->player, 3.0s cast, single-target
}

public enum SID : uint
{
    Hover = 412, // Boss->Boss, extra=0x0
    Invincibility = 775, // none->Boss/FloatingTurret, extra=0x0
    DamageUp = 443, // none->Boss, extra=0x1/0x2
    Windburn = 269, // Boss/FloatingTurret->player, extra=0x0
}

class WindBlast(BossModule module) : Components.StandardAOEs(module, AID.WindBlast, new AOEShapeRect(60, 4));
class HotBlast(BossModule module) : Components.CastCounter(module, AID.HotBlast)
{
    private readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0)
            return;

        Arena.ZoneCircle(Module.PrimaryActor.Position, 4, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count == 0)
            return;

        hints.Add("Stand under boss!", !actor.Position.InCircle(Module.PrimaryActor.Position, 4));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        hints.AddForbiddenZone(new AOEShapeDonut(4, 40), Module.PrimaryActor.Position, activation: Module.CastFinishAt(Casters[0].CastInfo));
        hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class NuzalHuelocStates : StateMachineBuilder
{
    public NuzalHuelocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindBlast>()
            .ActivateOnEnter<HotBlast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5265, Contributors = "xan")]
public class NuzalHueloc(WorldState ws, Actor primary) : BossModule(ws, primary, new(-74.5f, -70.25f), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var actor in WorldState.Actors.Where(x => !x.IsAlly))
            Arena.Actor(actor, (OID)actor.OID == OID.FloatingTurret && actor.HPMP.CurHP == 1 ? ArenaColor.PlayerGeneric : ArenaColor.Enemy);
    }
}
