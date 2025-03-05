namespace BossMod.Endwalker.Dungeon.D10Troia.D102Beatrice;

public enum OID : uint
{
    Boss = 0x396D,
    Helper = 0x233C,
}

public enum AID : uint
{
    EyeOfTroia = 29818, // Boss->self, 4.0s cast, range 40 circle
    DeathForeseen = 29821, // Helper->self, 5.0s cast, range 40 circle
    BeatificScorn = 29817, // Helper->self, 10.0s cast, range 9 circle
    Hush = 29824, // Boss->player, 5.0s cast, single-target
    Voidshaker = 29822, // Boss->self, 5.0s cast, range 20 120-degree cone
    VoidNail = 29823, // Helper->player, 5.0s cast, range 6 circle
    DeathForeseenRing = 29828, // Helper->self, 8.0s cast, range 40 circle
    ToricVoid = 31207, // Helper->self, 4.0s cast, range 10-20 donut
    Antipressure = 31208, // Helper->players, 7.0s cast, range 6 circle
}

class EyeOfTroia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EyeOfTroia));
class DeathForeseen(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseen));
class DeathForeseen1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseenRing))
{
    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => base.ActiveEyes(slot, actor).Take(2);
}
class BeatificScorn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BeatificScorn), new AOEShapeCircle(9), maxCasts: 5);
class Hush(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Hush));
class Voidshaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Voidshaker), new AOEShapeCone(20, 60.Degrees()));
class VoidNail(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VoidNail), 6);
class ToricVoid(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ToricVoid), new AOEShapeDonut(10, 20));
class Antipressure(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Antipressure), 6);

class BeatriceStates : StateMachineBuilder
{
    public BeatriceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyeOfTroia>()
            .ActivateOnEnter<DeathForeseen>()
            .ActivateOnEnter<DeathForeseen1>()
            .ActivateOnEnter<BeatificScorn>()
            .ActivateOnEnter<Hush>()
            .ActivateOnEnter<Voidshaker>()
            .ActivateOnEnter<VoidNail>()
            .ActivateOnEnter<ToricVoid>()
            .ActivateOnEnter<Antipressure>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11384)]
public class Beatrice(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(20));

