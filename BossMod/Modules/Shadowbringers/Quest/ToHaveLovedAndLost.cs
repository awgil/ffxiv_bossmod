namespace BossMod.Shadowbringers.Quest.ToHaveLovedAndLost;

public enum OID : uint
{
    Boss = 0x2927,
    Helper = 0x233C,
}

public enum AID : uint
{
    Bloodstain = 4747, // Boss->self, 2.5s cast, range 5 circle
    BrandOfSin = 16132, // Boss->self, 3.0s cast, range 80 circle
    BladeOfJustice = 16134, // Boss->players, 8.0s cast, range 6 circle
    SanctifiedHolyII = 17427, // Boss->self, 3.0s cast, range 5 circle
    SanctifiedHolyIII = 17430, // 2AB3/2AB2->location, 3.0s cast, range 6 circle
    HereticsFork = 17552, // 2779->self, 4.0s cast, range 40 width 6 cross
    SpiritsWithout = 4746, // Boss->self, 2.5s cast, range 3+R width 3 rect
    SeraphBlade = 16131, // Boss->self, 5.0s cast, range 40+R ?-degree cone
    Fracture = 15576, // 2612->location, 5.0s cast, range 3 circle
    Fracture1 = 13208, // 2612->location, 5.0s cast, range 3 circle
    Fracture2 = 13207, // 2612->location, 5.0s cast, range 3 circle
    Fracture3 = 15374, // 2612->location, 5.0s cast, range 3 circle
    Fracture4 = 16612, // 2612->location, 5.0s cast, range 3 circle
    Fracture5 = 13209, // 2612->location, 5.0s cast, range 3 circle
    HereticsQuoit = 17470, // 2968->self, 5.0s cast, range -15 donut
    SanctifiedHoly1 = 17431, // 2AB3/2AB2->players/2928, 5.0s cast, range 6 circle
}

class HereticsFork(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HereticsFork), new AOEShapeCross(40, 3));
class SpiritsWithout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpiritsWithout), new AOEShapeRect(3.5f, 1.5f));
class SeraphBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeraphBlade), new AOEShapeCone(40, 90.Degrees()));
class HereticsQuoit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HereticsQuoit), new AOEShapeDonut(5, 15));
class SanctifiedHoly(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SanctifiedHoly1), 6);

class Fracture(BossModule module) : Components.GenericTowers(module)
{
    private readonly AID[] TowerCasts = [AID.Fracture, AID.Fracture1, AID.Fracture2, AID.Fracture3, AID.Fracture4, AID.Fracture5];

    private bool IsTower(ActionID act) => TowerCasts.Contains((AID)act.ID);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (IsTower(spell.Action))
            Towers.Add(new(spell.LocXZ, 3, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (IsTower(spell.Action))
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.LocXZ, 1));
    }
}
class Bloodstain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bloodstain), new AOEShapeCircle(5));
class BrandOfSin(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BrandOfSin), 10);
class BladeOfJustice(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BladeOfJustice), 6, minStackSize: 1);
class SanctifiedHolyII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SanctifiedHolyII), new AOEShapeCircle(5));
class SanctifiedHolyIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SanctifiedHolyIII), 6);

class DikaiosyneStates : StateMachineBuilder
{
    public DikaiosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<BrandOfSin>()
            .ActivateOnEnter<BladeOfJustice>()
            .ActivateOnEnter<SanctifiedHoly>()
            .ActivateOnEnter<SanctifiedHolyII>()
            .ActivateOnEnter<SanctifiedHolyIII>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<HereticsQuoit>()
            .ActivateOnEnter<SpiritsWithout>()
            .ActivateOnEnter<SeraphBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68784, NameID = 8922)]
public class Dikaiosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(-798.6f, -40.49f), new ArenaBoundsCircle(20));
