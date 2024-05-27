namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D032Batsquatch;

public enum OID : uint
{
    Boss = 0x27B0, //R=3.2
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 870, // 27B0->player, no cast, single-target
    RipperFang = 15505, // 27B0->player, 4.0s cast, single-target
    Soundwave = 15506, // 27B0->self, 3.0s cast, range 40 circle
    Subsonics = 15507, // 27B0->self, 6.0s cast, single-target
    Subsonics2 = 15508, // 233C->self, no cast, range 40 circle
    FallingRock = 15510, // 233C->self, 2.0s cast, range 3 circle
    FallingRock2 = 15509, // 233C->self, 2.0s cast, range 2 circle
    FallingBoulder = 15511, // 233C->self, 2.0s cast, range 4 circle
    Towerfall = 15512, // 233C->self, 3.0s cast, range 15 30-degree cone
}

class Towerfall(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Towerfall), new AOEShapeCone(15, 15.Degrees()));
class Soundwave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Soundwave), "Raidwide + towers fall");
class Subsonics(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Subsonics), "Raidwide x11");
class RipperFang(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.RipperFang));
class FallingBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingBoulder), new AOEShapeCircle(4));
class FallingRock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3));
class FallingRock2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock2), new AOEShapeCircle(2));

class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
            if (!Module.FindComponent<Towerfall>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<FallingBoulder>()!.ActiveAOEs(slot, actor).Any() &&
            !Module.FindComponent<FallingRock>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<FallingRock2>()!.ActiveAOEs(slot, actor).Any())
                if (actor.Role is Role.Melee or Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
    }
}

class D032BatsquatchStates : StateMachineBuilder
{
    public D032BatsquatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<Soundwave>()
            .ActivateOnEnter<Subsonics>()
            .ActivateOnEnter<RipperFang>()
            .ActivateOnEnter<FallingBoulder>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<FallingRock2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8232)]
public class D032Batsquatch(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(62, -35.2f), 14.7f)];
    private static readonly List<Shape> difference = [new Rectangle(new(61.9f, -20), 20, 2), new Rectangle(new(61.9f, -50), 20, 2)];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}
