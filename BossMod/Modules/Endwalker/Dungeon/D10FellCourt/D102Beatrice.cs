namespace BossMod.Endwalker.Dungeon.D10FellCourt.D102Beatrice;

public enum OID : uint
{
    Boss = 0x396D, // R=4.95
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
    Helper = 0x233C, // R0.500, x26, 523 type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BeatificScorn1 = 29811, // Boss->self, 4.0s cast, single-target //Visual
    BeatificScorn2 = 29814, // Boss->self, no cast, single-target //Visual
    BeatificScorn3 = 29815, // Boss->self, no cast, single-target //Visual
    BeatificScorn4 = 29816, // Boss->self, no cast, single-target //Visual

    BeatificScorn5 = 29817, // Helper->self, 10.0s cast, range 9 circle
    DeathForeseen1 = 29821, // Helper->self, 5.0s cast, range 40 circle
    DeathForeseen2 = 29828, // Helper->self, 8.0s cast, range 40 circle

    EyeOfTroia = 29818, // Boss->self, 4.0s cast, range 40 circle //Raidwide

    Hush = 29824, // Boss->player, 5.0s cast, single-target //Tankbuster
    VoidNail = 29823, // Helper->player, 5.0s cast, range 6 circle //spread

    Voidshaker = 29822, // Boss->self, 5.0s cast, range 20 120-degree cone
    UnknownAbility1 = 29819, // Boss->self, no cast, single-target
    UnknownAbility2 = 29820, // Boss->location, no cast, single-target //Likely Toric Void: A ring AoE around the outside of the area, used at the same time as Eye of Troia.
    // Missing Antipreasure stack marker
}

public enum IconID : uint
{
    Icon_218 = 218, // player
    Icon_139 = 139, // player
}

class BeatificScorn5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BeatificScorn5), new AOEShapeCircle(9));

class DeathForeseen1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseen1));
class DeathForeseen2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseen2));

class Voidshaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Voidshaker), new AOEShapeCone(20, 60.Degrees()));

class VoidNail(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VoidNail), 6);
class Hush(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Hush));
class EyeOfTroia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EyeOfTroia));

class D102BeatriceStates : StateMachineBuilder
{
    public D102BeatriceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeatificScorn5>()
            .ActivateOnEnter<DeathForeseen1>()
            .ActivateOnEnter<DeathForeseen2>()
            .ActivateOnEnter<Voidshaker>()
            .ActivateOnEnter<VoidNail>()
            .ActivateOnEnter<Hush>()
            .ActivateOnEnter<EyeOfTroia>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11384)]
public class D102Beatrice(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(20));
