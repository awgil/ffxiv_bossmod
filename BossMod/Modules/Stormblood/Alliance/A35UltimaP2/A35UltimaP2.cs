namespace BossMod.Stormblood.Alliance.A35UltimaP2;

class Redemption(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Redemption));
class Auralight1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Auralight1), new AOEShapeRect(50, 5));
class Auralight2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Auralight2), new AOEShapeRect(25, 5));
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Bombardment), 6);
class Embrace2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Embrace2), 3);
class GrandCrossAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandCrossAOE), new AOEShapeCross(60, 7.5f));
class Holy(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Holy), 2);
class HolyIVBait(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HolyIVBait), 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HolyIVSpread), 6);
class Plummet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Plummet), new AOEShapeRect(15, 7.5f));

class Cataclysm(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Stay;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -600), new ArenaBoundsSquare(30));
