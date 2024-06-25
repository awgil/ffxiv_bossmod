namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

class CentrifugalSlice(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CentrifugalSlice));
class PrimeBladeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PrimeBladeAOE), new AOEShapeCircle(20));
class PrimeBladeFront(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PrimeBladeFront), new AOEShapeRect(85, 10));
class PrimeBladeDonut1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PrimeBladeDonut1), new AOEShapeDonut(7, 43));
class PrimeBladeDonut2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PrimeBladeDonut2), new AOEShapeDonut(7, 43));
class RelentlessSpiralLocAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RelentlessSpiralLocAOE), 8);
class RelentlessSpiralAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RelentlessSpiralAOE), new AOEShapeCircle(8));
class ThreePartsDisdainStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ThreePartsDisdainStack), 6, 8);
class R012LaserLoc(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.R012LaserLoc), 6);
class R012LaserSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.R012LaserSpread), 6);
class R012LaserTankBuster(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.R012LaserTankBuster), 6);
class R011LaserLine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.R011LaserLine), new AOEShapeRect(70, 7.5f));

class EnergyCompression(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.TowersMaybe)
            Towers.Add(new(actor.Position, 5, 1, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnergyCompression or AID.Explosion && Towers.Count > 0)
            Towers.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 5));
        if (Towers.Count > 1)
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9644)]
public class A25Compound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsSquare(29.5f));
