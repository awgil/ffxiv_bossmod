namespace BossMod.Stormblood.Trial.T08Suzaku;

class ScarletFever(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScarletFever)
            Module.Arena.Bounds = T08Suzaku.phase2Arena;
    }
}

class ScreamsOfTheDamned(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScreamsOfTheDamned));
class SouthronStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SouthronStar));
class AshesToAshes(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AshesToAshes));
class ScarletFeverAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScarletFever));

class RuthlessRefrain(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RuthlessRefrain), 8);
class Cremate(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Cremate));
class PhantomFlurryTankbuster(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PhantomFlurryTankbuster));
class PhantomFlurryAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomFlurryAOE), new AOEShapeCone(41, 90.Degrees()));
class FleetingSummer(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FleetingSummer), new AOEShapeCone(40, 45.Degrees()));
class Hotspot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hotspot), new AOEShapeCone(21, 60.Degrees()));
class Swoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Swoop), new AOEShapeRect(55, 3));
class WellOfFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WellOfFlame), new AOEShapeRect(41, 10));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 596, NameID = 6221)]
public class T08Suzaku(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(100, 100), 20)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union);
    public static readonly ArenaBounds phase2Arena = new ArenaBoundsComplex([new Donut(new(100, 100), 4, 20)]);
}
