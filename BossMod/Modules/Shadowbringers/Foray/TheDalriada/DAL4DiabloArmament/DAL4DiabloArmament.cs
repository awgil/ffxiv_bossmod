namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

class AdvancedDeathIVAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AdvancedDeathIVAOE), 1);

class AdvancedNox : Components.StandardChasingAOEs
{
    public AdvancedNox(BossModule module) : base(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.AdvancedNoxAOEFirst), ActionID.MakeSpell(AID.AdvancedNoxAOERest), 5.5f, 1.6f, 5) //float moveDistance, float secondsBetweenActivations, int maxCasts
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class AssaultCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AssaultCannon), new AOEShapeRect(100, 3));
class DeadlyDealingAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DeadlyDealingAOE), 6);

class Explosion1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion1), new AOEShapeRect(60, 11));
class Explosion2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion2), new AOEShapeRect(60, 11));
class Explosion3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion3), new AOEShapeRect(60, 11));
class Explosion4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion4), new AOEShapeRect(60, 11));
class Explosion5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion5), new AOEShapeRect(60, 11));
class Explosion6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion6), new AOEShapeRect(60, 11));
class Explosion7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion7), new AOEShapeRect(60, 11));
class Explosion8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion8), new AOEShapeRect(60, 11));
class Explosion9(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion9), new AOEShapeRect(60, 11));

class LightPseudopillarAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LightPseudopillarAOE), 10);

class PillarOfShamash1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarOfShamash1), new AOEShapeCone(70, 10.Degrees()));
class PillarOfShamash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarOfShamash2), new AOEShapeCone(70, 10.Degrees()));
class PillarOfShamash3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarOfShamash3), new AOEShapeCone(70, 10.Degrees()));

class UltimatePseudoterror(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UltimatePseudoterror), new AOEShapeDonut(15, 70));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 5)] //BossNameID = 10007
public class DAL4DiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720, -760), new ArenaBoundsCircle(30));
