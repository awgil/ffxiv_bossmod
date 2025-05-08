namespace BossMod.Stormblood.Dungeon.D07AlaMhigo.D073ZenosYaeGalvus;

public enum OID : uint
{
    Boss = 0x1BAA,
    Helper = 0x233C,
    ZenosYaeGalvus = 0x18D6, // R0.500, x12
    ZenosYaeGalvus1 = 0x1BAB, // R0.960, x4
    B = 0x1DD3, // R0.500, x0 (spawn during fight)
    AmeNoHabakiri = 0x1BAE, // R3.000, x0 (spawn during fight)
    TheSwell = 0x1BAC, // R3.000, x0 (spawn during fight)
    TheStorm = 0x1BAD, // R3.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Weaponskill = 8287, // Boss->self, no cast, single-target
    Ability = 8292, // Boss->location, no cast, ???
    Weaponskill1 = 8295, // _Gen_ZenosYaeGalvus->self, 1.0s cast, range 15 circle
    Ability1 = 9119, // _Gen_ZenosYaeGalvus1->location, no cast, ???
    WeaponskillFastBlade = 717, // 1DD3->1DCB, no cast, single-target
    Weaponskill2 = 8302, // Boss->self, no cast, single-target

    ArtOfTheStorm = 8294, // Boss->self, 6.0s cast, range 8 circle
    ArtOfTheSwell = 8293, // Boss->self, 6.0s cast, range 33 circle
    ArtOfTheSword = 8296, // Boss->self, 5.5s cast, single-target
    ArtOfTheSword1 = 8297, // _Gen_ZenosYaeGalvus->self, no cast, range 40+R width 6 rect
    UnmovingTroika = 8288, // Boss->self, no cast, range 9+R ?-degree cone
    UnmovingTroika1 = 8289, // _Gen_ZenosYaeGalvus->self, 1.7s cast, range 9+R ?-degree cone
    UnmovingTroika2 = 8290, // _Gen_ZenosYaeGalvus->self, 2.1s cast, range 9+R ?-degree cone
    VeinSplitter = 9398, // Boss->self, 3.5s cast, range 10 circle
    VeinSplitter1 = 8300, // _Gen_ZenosYaeGalvus1->self, 3.0s cast, range 10 circle
    LightlessSpark = 8299, // Boss->self, 3.0s cast, range 40+R 90-degree cone
    Concentrativity = 8301, // Boss->self, 3.0s cast, range 40 circle
    ArtOfTheSword2 = 9608, // _Gen_ZenosYaeGalvus1->self, 5.5s cast, single-target
    ArtOfTheSword3 = 9609, // _Gen_ZenosYaeGalvus->self, no cast, range 40+R width 6 rect

    ArtOfTheStorm1 = 9607, // _Gen_ZenosYaeGalvus1->self, 8.0s cast, range 8 circle
    ArtOfTheSwell1 = 9606, // _Gen_ZenosYaeGalvus1->self, 6.0s cast, range 33 circle
    StormSwellSword = 8303, // Boss->self, no cast, single-target
    Weaponskill3 = 9118, // 1BAC/1BAD/1BAE->self, no cast, single-target
    StormSwellSword1 = 8304, // _Gen_ZenosYaeGalvus->self, 7.0s cast, range 40 circle
}

public enum TetherID : uint
{
    Tether41 = 41, // Boss->player
}

class LightlessSparkTether(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 41 && WorldState.Actors.Find(tether.Target) is Actor tar)
            CurrentBaits.Add(new(source, tar, new AOEShapeCone(40.5f, 45.Degrees()), WorldState.FutureTime(8.1f)));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 41)
            CurrentBaits.Clear();
    }
}
class LightlessSpark(BossModule module) : Components.StandardAOEs(module, AID.LightlessSpark, new AOEShapeCone(40.5f, 45.Degrees()));
class VeinSplitter(BossModule module) : Components.StandardAOEs(module, AID.VeinSplitter, new AOEShapeCircle(10));
class VeinSplitter1(BossModule module) : Components.StandardAOEs(module, AID.VeinSplitter1, new AOEShapeCircle(10));
class UnmovingTroika1(BossModule module) : Components.StandardAOEs(module, AID.UnmovingTroika1, new AOEShapeCone(9.5f, 45.Degrees()));
class UnmovingTroika2(BossModule module) : Components.StandardAOEs(module, AID.UnmovingTroika2, new AOEShapeCone(9.5f, 45.Degrees()));
class ArtOfTheStorm(BossModule module) : Components.StandardAOEs(module, AID.ArtOfTheStorm, new AOEShapeCircle(8));
class ArtOfTheSwell(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ArtOfTheSwell, 15);
class ArtOfTheSword(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArtOfTheSword or AID.ArtOfTheSword2)
            foreach (var player in Raid.WithoutSlot())
                CurrentBaits.Add(new(caster, player, new AOEShapeRect(40, 3), Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ArtOfTheSword1 or AID.ArtOfTheSword3)
            CurrentBaits.Clear();
    }
}
class Concentrativity(BossModule module) : Components.RaidwideCast(module, AID.Concentrativity);
class ArtOfTheStorm1(BossModule module) : Components.StandardAOEs(module, AID.ArtOfTheStorm1, new AOEShapeCircle(8));
class ArtOfTheSwell1(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ArtOfTheSwell1, 15)
{
    private ArtOfTheStorm1? storm;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        storm ??= Module.FindComponent<ArtOfTheStorm1>();

        var caster = Casters.FirstOrDefault();
        if (caster == null)
            return;

        List<Func<WPos, bool>> funcs = [
            ShapeContains.InvertedCircle(Module.Center, 20),
        ];

        if (storm?.ActiveCasters.FirstOrDefault() is Actor st)
            funcs.Add(storm.Shape.CheckFn(st.Position, st.Rotation));

        bool inbounds(WPos pos)
        {
            var dir = (pos - caster.Position).Normalized();
            var proj = pos + 15 * dir;
            return funcs.Any(f => f(proj));
        }

        hints.AddForbiddenZone(inbounds, Module.CastFinishAt(caster.CastInfo));
    }
}

class StormSwellSword(BossModule module) : Components.RaidwideCast(module, AID.StormSwellSword1);

class Adds(BossModule module) : Components.AddsMulti(module, [OID.AmeNoHabakiri, OID.TheStorm, OID.TheSwell]);

class ZenosYaeGalvusStates : StateMachineBuilder
{
    public ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArtOfTheStorm>()
            .ActivateOnEnter<ArtOfTheSwell>()
            .ActivateOnEnter<ArtOfTheSword>()
            .ActivateOnEnter<UnmovingTroika1>()
            .ActivateOnEnter<UnmovingTroika2>()
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<VeinSplitter1>()
            .ActivateOnEnter<LightlessSparkTether>()
            .ActivateOnEnter<LightlessSpark>()
            .ActivateOnEnter<Concentrativity>()
            .ActivateOnEnter<ArtOfTheStorm1>()
            .ActivateOnEnter<ArtOfTheSwell1>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6039)]
public class ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -353), new ArenaBoundsCircle(20));
