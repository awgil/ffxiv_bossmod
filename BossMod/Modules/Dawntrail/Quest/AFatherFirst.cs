namespace BossMod.Dawntrail.Quest.AFatherFirst;

public enum OID : uint
{
    Boss = 0x4176,
    Shade = 0x4177,
    Helper = 0x233C,
    BurningSunPuddle = 0x1E8D9B
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    FancyBladework = 36413, // Boss->self, 5.0s cast, range 60 circle

    DualBlowsR = 36395, // Boss->self, 7.0s cast, single-target
    DualBlowsR1 = 35423, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualBlowsR2 = 35424, // Helper->self, 10.5s cast, range 30 180-degree cone
    DualBlowsL = 36393, // Boss->self, 7.0s cast, single-target
    DualBlowsL1 = 35421, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualBlowsL2 = 35422, // Helper->self, 10.5s cast, range 30 180-degree cone
    DualBlowsVisualL = 36394, // Boss->self, no cast, single-target
    DualBlowsVisualR = 36396, // Boss->self, no cast, single-target

    SteeledStrikeCast = 36389, // Boss->location, 4.0s cast, single-target
    SteeledStrikeAOE = 36390, // Helper->self, 5.2s cast, range 30 width 8 cross
    SteeledStrikeIDK = 37062, // Helper->self, 5.2s cast, range 30 width 8 cross, dunno what the fuck this is

    CoiledStrikeCW = 36405, // Boss->self, 5.0+1.0s cast, single-target
    CoiledStrikeCCW = 36406, // Boss->self, 5.0+1.0s cast, single-target
    CoiledStrikeAOE = 36407, // Helper->self, 6.0s cast, range 30 150-degree cone

    BattleBreaker = 36414, // Boss->self, 5.0s cast, range 40 width 30 rect

    MorningStarsLines = 39135, // Helper->location, 1.5s cast, range 4 circle
    MorningStarsChase = 38819, // Helper->location, 3.0s cast, range 4 circle

    BurningSunCast = 36408, // Boss->self, 6.3+0.7s cast, single-target
    BurningSunVisualSmall = 36409, // Helper->location, 1.5s cast, range 4 circle
    BurningSunVisualLarge = 36410, // Helper->location, 1.5s cast, range 6 circle
    BurningSunSmall = 36411, // Helper->location, 1.0s cast, range 4 circle
    BurningSunLarge = 36412, // Helper->location, 1.0s cast, range 6 circle

    BrawlEnderCast = 36397, // Boss->self, 5.0s cast, range 50 circle
    BrawlEnderKB = 36398, // Boss->player, no cast, single-target

    Doubling1 = 38475, // Boss->self, 3.0s cast, single-target
    GloryBlaze = 36417, // 4177->self, 8.0s cast, range 40 width 6 rect

    Doubling2 = 36424, // Boss->self, 3.0s cast, single-target

    ShadeThrillCast = 38815, // 4177->location, 5.0+1.5s cast, single-target
    ShadeThrill = 38817, // Helper->self, 6.2s cast, range 3 circle
    BossThrillCast = 36418, // Boss->location, 5.0+1.5s cast, single-target
    BossThrill = 36420, // Helper->self, 6.2s cast, range 3 circle

    ShadeSteeledStrikeCast = 36391, // Shade->location, 4.0s cast, single-target
    ShadeSteeledStrike = 36392, // Helper->self, 5.2s cast, range 30 width 8 cross
    ShadeSteeledStrikeIDK = 37063, // Helper->self, 5.2s cast, range 30 width 8 cross
}

class FancyBladework(BossModule module) : Components.RaidwideCast(module, AID.FancyBladework);

class Thrill(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ShadeThrill or AID.BossThrill)
            Towers.Add(new(caster.Position, 3, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShadeThrill or AID.BossThrill)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

class BurningSun(BossModule module) : Components.GenericAOEs(module)
{
    record Sun(WPos Origin, DateTime Activation, float Size);

    private readonly List<Sun> Suns = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in Suns)
            yield return new AOEInstance(new AOEShapeCircle(s.Size), s.Origin, Activation: s.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BurningSunVisualLarge:
                Suns.Add(new(spell.LocXZ, WorldState.FutureTime(6.1f), 6));
                break;
            case AID.BurningSunVisualSmall:
                Suns.Add(new(spell.LocXZ, WorldState.FutureTime(6.1f), 4));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BurningSunLarge:
            case AID.BurningSunSmall:
                Suns.RemoveAll(x => x.Origin.AlmostEqual(spell.TargetXZ, 0.1f));
                break;
        }
    }
}

class CoiledStrike(BossModule module) : Components.StandardAOEs(module, AID.CoiledStrikeAOE, new AOEShapeCone(30, 75.Degrees()));
class SteeledStrike(BossModule module) : Components.StandardAOEs(module, AID.SteeledStrikeAOE, new AOEShapeCross(30, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var isShadeActive = Module.FindComponent<SteeledStrikeShade>()?.ActiveCasters.Any() ?? false;
        if (!isShadeActive)
            foreach (var e in base.ActiveAOEs(slot, actor))
                yield return e;
    }
}
class SteeledStrikeShade(BossModule module) : Components.StandardAOEs(module, AID.ShadeSteeledStrike, new AOEShapeCross(30, 4));
class MorningStarsLines(BossModule module) : Components.StandardAOEs(module, AID.MorningStarsLines, 4);
class MorningStarsChase(BossModule module) : Components.StandardAOEs(module, AID.MorningStarsChase, 4);

class DualBlows(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> AOEs = [];
    static readonly AOEShape BladeRight = new AOEShapeCone(30, 90.Degrees(), -90.Degrees());
    static readonly AOEShape BladeLeft = new AOEShapeCone(30, 90.Degrees(), 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DualBlowsR1)
        {
            AOEs.Add(new(BladeRight, caster.Position, caster.Rotation, WorldState.FutureTime(8)));
            AOEs.Add(new(BladeLeft, caster.Position, caster.Rotation, WorldState.FutureTime(10.5f)));
        }
        if ((AID)spell.Action.ID == AID.DualBlowsL1)
        {
            AOEs.Add(new(BladeLeft, caster.Position, caster.Rotation, WorldState.FutureTime(8)));
            AOEs.Add(new(BladeRight, caster.Position, caster.Rotation, WorldState.FutureTime(10.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DualBlowsR1 or AID.DualBlowsR2 or AID.DualBlowsL1 or AID.DualBlowsL2)
        {
            NumCasts++;
            AOEs.RemoveAt(0);
        }
    }
}

class BurningSunPuddle(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.BurningSunPuddle).Where(x => x.EventState != 7));
class BrawlEnder(BossModule module) : Components.Knockback(module, AID.BrawlEnderCast, stopAtWall: true)
{
    private DateTime? Activation;
    private Angle? FixedDirection;
    public int NumKBs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Activation = Module.CastFinishAt(spell).AddSeconds(2.6);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrawlEnderKB)
            FixedDirection = WorldState.Actors.Find(spell.MainTargetID)?.Rotation;
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (Activation != null && actor.OID == (uint)OID.Boss && modelState == 0 && animState1 == 0 && animState2 == 0)
        {
            Activation = null;
            FixedDirection = null;
            NumKBs++;
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(Activation).Select(a => new Source(actor.Position, 20, a, null, FixedDirection ?? actor.Rotation, Kind.DirForward));
}

class GloryBlaze(BossModule module) : Components.StandardAOEs(module, AID.GloryBlaze, new AOEShapeRect(40, 3));

class GuloolJaJaStates : StateMachineBuilder
{
    public GuloolJaJaStates(GuloolJaJa module) : base(module)
    {
        DeathPhase(0, id =>
        {
            P1(id);
            P2(id + 0x100000, 27.8f);
        });
    }

    private void CoiledStrike(uint id, float delay, bool bothCasts = true)
    {
        CastMulti(id, [AID.CoiledStrikeCCW, AID.CoiledStrikeCW], delay, 4.95f).ActivateOnEnter<CoiledStrike>();
        var cast1 = ComponentCondition<CoiledStrike>(id + 0x02, 1, b => b.NumCasts > 1, "Coil 1");
        if (bothCasts)
        {
            CastMulti(id + 0x10, [AID.CoiledStrikeCCW, AID.CoiledStrikeCW], 3.2f, 4.95f);
            ComponentCondition<CoiledStrike>(id + 0x12, 1, b => b.NumCasts > 3, "Coil 2")
                .DeactivateOnExit<CoiledStrike>();
        }
        else
        {
            cast1.DeactivateOnExit<CoiledStrike>();
        }
    }

    private void SteeledStrike(uint id, float delay)
    {
        Cast(id, AID.SteeledStrikeCast, delay, 4)
            .ActivateOnEnter<SteeledStrike>();
        ComponentCondition<SteeledStrike>(id + 0x02, 1.2f, b => b.NumCasts > 0, "Cross")
            .DeactivateOnExit<SteeledStrike>();
    }

    private State DualBlows(uint id, float delay)
    {
        CastMulti(id, [AID.DualBlowsL, AID.DualBlowsR], delay, 7)
            .ActivateOnEnter<DualBlows>();
        ComponentCondition<DualBlows>(id + 0x02, 1, b => b.NumCasts > 0, "Left/right 1");
        return ComponentCondition<DualBlows>(id + 0x04, 2.5f, b => b.NumCasts > 1, "Left/right 2")
            .DeactivateOnExit<DualBlows>();
    }

    private void BurningSun(uint id, float delay, bool exit = true)
    {
        Cast(id, AID.BurningSunCast, delay, 6.2f, "Puddles")
            .ActivateOnEnter<BurningSun>()
            .ActivateOnEnter<BurningSunPuddle>();
        Cast(id + 0x10, AID.BrawlEnderCast, 7.3f, 5).ActivateOnEnter<BrawlEnder>();
        ComponentCondition<BrawlEnder>(id + 0x12, 2.6f, b => b.NumKBs > 0, "Knockback")
            .DeactivateOnExit<BrawlEnder>()
            .SetHint(StateMachine.StateHint.Knockback);
        if (exit)
            BurningSunExit(id + 0x14, 4);
    }

    private void BurningSunExit(uint id, float delay)
    {
        ComponentCondition<BurningSunPuddle>(id, delay, b => !b.Sources(Module).Any(), "Puddles disappear")
            .DeactivateOnExit<BurningSun>()
            .DeactivateOnExit<BurningSunPuddle>();
    }

    private void P1(uint id)
    {
        Cast(id, AID.FancyBladework, 8.2f, 5, "Raidwide").ActivateOnEnter<FancyBladework>();
        DualBlows(id + 0x10, 6.2f);
        SteeledStrike(id + 0x10000, 7.5f);
        CoiledStrike(id + 0x20000, 12);
        Targetable(id + 0x30000, false, 14.7f, "Stun + cutscene");
    }

    private void P2(uint id, float downtime)
    {
        Targetable(id, true, downtime, "Zigzags + chasing AOE")
            .ActivateOnEnter<MorningStarsLines>()
            .ActivateOnEnter<MorningStarsChase>();
        ComponentCondition<MorningStarsLines>(id + 0x10, 12.3f, m => m.NumCasts >= 32, "AOEs end")
            .DeactivateOnExit<MorningStarsLines>()
            .DeactivateOnExit<MorningStarsChase>();

        id += 0x10000;
        BurningSun(id, 10.2f);

        id += 0x10000;
        Cast(id + 0x40, AID.Doubling1, 10.4f, 3).ActivateOnEnter<GloryBlaze>();
        DualBlows(id + 0x50, 11.9f).DeactivateOnExit<GloryBlaze>();

        id += 0x10000;
        Cast(id + 0x60, AID.Doubling2, 7.7f, 3).ActivateOnEnter<Thrill>();
        ComponentCondition<Thrill>(id + 0x70, 10.4f, t => t.NumCasts > 0, "Tower 1");
        ComponentCondition<Thrill>(id + 0x72, 3.1f, t => t.NumCasts > 1, "Tower 2")
            .DeactivateOnExit<Thrill>();

        id += 0x10000;
        Cast(id + 0x80, AID.FancyBladework, 7, 5, "Raidwide");

        id += 0x10000;
        Cast(id + 0x90, AID.Doubling2, 9.4f, 3)
            .ActivateOnEnter<SteeledStrikeShade>()
            .ActivateOnEnter<SteeledStrike>();
        CoiledStrike(id + 0xA0, 6.6f, bothCasts: false);
        ComponentCondition<SteeledStrikeShade>(id + 0xB0, 5, s => s.NumCasts > 0, "Cross 1")
            .DeactivateOnExit<SteeledStrikeShade>();
        ComponentCondition<SteeledStrike>(id + 0xB2, 3.6f, b => b.NumCasts > 0, "Cross 2")
            .DeactivateOnExit<SteeledStrike>();

        id += 0x10000;
        BurningSun(id, 9.3f, false);

        id += 0x10000;
        DualBlows(id, 2.4f)
            .DeactivateOnExit<BurningSun>()
            .DeactivateOnExit<BurningSunPuddle>();

        id += 0x10000;
        Cast(id, AID.Doubling1, 8.7f, 3)
            .ActivateOnEnter<GloryBlaze>()
            .ActivateOnEnter<SteeledStrikeShade>();
        CoiledStrike(id + 0x10, 17.6f, bothCasts: false);
        ComponentCondition<SteeledStrikeShade>(id + 0x20, 4.9f, s => s.NumCasts > 0, "Cross 1")
            .DeactivateOnExit<SteeledStrikeShade>();

        id += 0x10000;
        Timeout(id, 9000f, "Towers + victory")
            .ActivateOnEnter<Thrill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70419, NameID = 12675, PlanLevel = 94)]
public class GuloolJaJa(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 49), new ArenaBoundsRect(15, 20));
