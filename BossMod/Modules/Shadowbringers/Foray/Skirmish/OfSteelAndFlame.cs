namespace BossMod.Shadowbringers.Foray.Skirmish.OfSteelAndFlame;

public enum OID : uint
{
    Boss = 0x2FAE,
    Helper = 0x233C,
    SartauvoirTheInferno = 0x2FAD,
    SartHelper = 0x2EA1
}

public enum AID : uint
{
    Exhaust = 21556, // Boss->self, 4.0s cast, range 40 width 10 rect
    MagitekRay = 21558, // Boss->location, 3.5s cast, range 6 circle
    ThermalShock = 21022, // SartHelper->self, 7.0s cast, range 30 circle
    GrandSword = 21553, // Boss->self, 9.0s cast, range 27 120-degree cone
    GrandSwordInstant = 21555, // Boss->self, 0.5s cast, range 27 ?-degree cone
    TridirectionalFlame = 20927, // SartHelper->self, 3.0s cast, range 60 width 8 rect
    PyreticEruption = 20929, // SartHelper->location, 3.0s cast, range 8 circle
    Pyroscatter = 20930, // SartHelper->location, 3.0s cast, range 8 circle
    Pyroburst = 20931, // SartHelper->self, 4.0s cast, range 10 circle
    GrandCrossflame1 = 20928, // SartHelper->self, 4.5s cast, range 40 width 18 cross
    AtomicRay = 21559, // Boss->self, 8.0s cast, range 40 circle
}

class TridirectionalFlame(BossModule module) : Components.StandardAOEs(module, AID.TridirectionalFlame, new AOEShapeRect(60, 4));
class Exhaust(BossModule module) : Components.StandardAOEs(module, AID.Exhaust, new AOEShapeRect(40, 5));
class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, 6);
class ThermalShock(BossModule module) : Components.StandardAOEs(module, AID.ThermalShock, 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().InRadius(Casters[0].CastInfo!.LocXZ, 30).Mask(), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
class PyreticEruption(BossModule module) : Components.StandardAOEs(module, AID.PyreticEruption, 8);
class Pyroscatter(BossModule module) : Components.StandardAOEs(module, AID.Pyroscatter, 8);
class Pyroburst(BossModule module) : Components.StandardAOEs(module, AID.Pyroburst, new AOEShapeCircle(10));
class GrandCrossflame(BossModule module) : Components.StandardAOEs(module, AID.GrandCrossflame1, new AOEShapeCross(40, 9));
class AtomicRay(BossModule module) : Components.RaidwideCast(module, AID.AtomicRay);

class GrandSword(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GrandSword)
            Sequences.Add(new(new AOEShapeCone(27, 60.Degrees()), caster.Position, caster.Rotation, -120.Degrees(), Module.CastFinishAt(spell), 2.8f, NumCasts > 0 ? 6 : 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GrandSword or AID.GrandSwordInstant)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}

class CacusStates : StateMachineBuilder
{
    public CacusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ThermalShock>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<TridirectionalFlame>()
            .ActivateOnEnter<PyreticEruption>()
            .ActivateOnEnter<Pyroscatter>()
            .ActivateOnEnter<Pyroburst>()
            .ActivateOnEnter<GrandCrossflame>()
            .ActivateOnEnter<AtomicRay>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && Module.Enemies(OID.SartauvoirTheInferno).All(e => e.HPRatio < 1 && !e.IsTargetable);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1626, NameID = 9634)]
public class Cacus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-234.1f, 260.1f), new ArenaBoundsCircle(50))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SartauvoirTheInferno), ArenaColor.Enemy);
    }
}

