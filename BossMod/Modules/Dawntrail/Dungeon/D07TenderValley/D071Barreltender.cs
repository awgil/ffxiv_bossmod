namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D071Barreltender;

public enum OID : uint
{
    Boss = 0x4234, // R5.000, x1
    Helper = 0x233C, // R0.500, x63, 523 type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1ebbf1 = 0x1EBBF1, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ebbf0 = 0x1EBBF0, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BarbedBellow = 37392, // Boss->self, 5.0s cast, range 50 circle // Raidwide

    UnknownAbility = 37393, // Boss->location, no cast, single-target

    HeavyweightNeedles1 = 37384, // Boss->self, 6.0s cast, single-target
    HeavyweightNeedles2 = 37386, // Helper->self, 6.5s cast, range 36 ?-degree cone

    TenderDrop = 37387, // Boss->self, 3.0s cast, single-target // spawns cacti
    BarrelBreaker = 37390, // Boss->location, 6.0s cast, range 50 circle // knockback 20

    NeedleSuperstorm = 37389, // Helper->self, 5.0s cast, range 11 circle
    NeedleStorm = 37388, // Helper->self, 5.0s cast, range 6 circle

    SucculentStomp = 37391, // Boss->players, 5.0s cast, range 6 circle
    PricklyRight = 39154, // Boss->self, 7.0s cast, range 36 ?-degree cone
    //PricklyLeft = ???, // Boss->self, 7.0s cast, range 36 ?-degree cone
    TenderFury = 39242, // Boss->player, 5.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/Boss->player, extra=0x2/0x3/0x1/0x4/0x5
    StabWound1 = 3061, // none->player, extra=0x0
    StabWound2 = 3062, // none->player, extra=0x0
}

public enum IconID : uint
{
    Stackmarker = 161, // player
    Tankbuster = 218, // player
}

class HeavyweightNeedles2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeavyweightNeedles2), new AOEShapeCone(36, 22.5f.Degrees()));
class PricklyRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PricklyRight), new AOEShapeCone(36, 135f.Degrees()));
class NeedleSuperstorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NeedleSuperstorm), new AOEShapeCircle(11));
class NeedleStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NeedleStorm), new AOEShapeCircle(6));

class BarrelBreaker(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BarrelBreaker), 20, stopAtWall: true);
class TenderFury(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TenderFury));
class BarbedBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BarbedBellow));

class D071BarreltenderStates : StateMachineBuilder
{
    public D071BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavyweightNeedles2>()
            .ActivateOnEnter<PricklyRight>()
            .ActivateOnEnter<NeedleSuperstorm>()
            .ActivateOnEnter<NeedleStorm>()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<TenderFury>()
            .ActivateOnEnter<BarbedBellow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12889)]
public class D071Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-65, 470), new ArenaBoundsSquare(20));

