namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D051ForgivenCruelty;

public enum OID : uint
{
    Boss = 0x27CA, //R=6.89
    Helper = 0x233C, //R=0.5
    Helper2 = 0x27CB, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 872, // 27CA->player, no cast, single-target
    Rake = 15611, // 27CA->player, 3.0s cast, single-target
    LumenInfinitum = 16818, // 27CA->self, 3.7s cast, range 40 width 5 rect
    TyphoonWingA = 15615, // 27CA->self, 5.0s cast, single-target
    TyphoonWingB = 15614, // 27CA->self, 5.0s cast, single-target
    TyphoonWingC = 15617, // 27CA->self, 7.0s cast, single-target
    TyphoonWingD = 15618, // 27CA->self, 7.0s cast, single-target
    TyphoonWing = 15616, // 233C->self, 5.0s cast, range 25 60-degree cone
    TyphoonWing2 = 17153, // 233C->self, 7.0s cast, range 25 60-degree cone
    CycloneWing = 15612, // 27CA->self, 3.0s cast, single-target
    CycloneWing2 = 15613, // 233C->self, 4.0s cast, range 40 circle
    HurricaneWing = 15619, // 233C->self, 5.0s cast, range 10 circle
}

class Rake(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Rake));
class CycloneWing(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CycloneWing2));
class LumenInfinitum(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LumenInfinitum), new AOEShapeRect(40, 2.5f));
class HurricaneWing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HurricaneWing), new AOEShapeCircle(10));
class TyphoonWing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TyphoonWing), new AOEShapeCone(25, 30.Degrees()));
class TyphoonWing2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TyphoonWing2), new AOEShapeCone(25, 30.Degrees()));
class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
            if (!Module.FindComponent<HurricaneWing>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<TyphoonWing>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<TyphoonWing2>()!.ActiveAOEs(slot, actor).Any())
                if (actor.Role is Role.Melee or Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
    }
}

class D051ForgivenCrueltyStates : StateMachineBuilder
{
    public D051ForgivenCrueltyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<Rake>()
            .ActivateOnEnter<HurricaneWing>()
            .ActivateOnEnter<TyphoonWing>()
            .ActivateOnEnter<TyphoonWing2>()
            .ActivateOnEnter<CycloneWing>()
            .ActivateOnEnter<LumenInfinitum>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8260)]
public class D051ForgivenCruelty(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(188, -170), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(168.25f, -170), 20, 1, 90.Degrees()), new Rectangle(new(207.75f, -170), 20, 1, 90.Degrees())];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}
