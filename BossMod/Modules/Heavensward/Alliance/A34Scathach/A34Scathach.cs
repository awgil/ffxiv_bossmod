namespace BossMod.Heavensward.Alliance.A34Scathach;

class ThirtyCries(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ThirtyCries), new AOEShapeCircle(12));
class ThirtyThorns4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThirtyThorns4), new AOEShapeCircle(8));
class ThirtySouls(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ThirtySouls));
class ThirtyArrows2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThirtyArrows2), new AOEShapeRect(35, 4));
class ThirtyArrows1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ThirtyArrows1), 8);
class TheDragonsVoice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeCircle(30));

class Shadespin2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shadespin2), new AOEShapeCone(30, 45.Degrees()));

class Shadesmite1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shadesmite1), new AOEShapeCircle(15));
class Shadesmite2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shadesmite2), new AOEShapeCircle(3));
class Shadesmite3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shadesmite3), new AOEShapeCircle(3));

class Pitfall(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Pitfall));
class FullSwing(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FullSwing));

class Nox : Components.StandardChasingAOEs
{
    public Nox(BossModule module) : base(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.NoxAOEFirst), ActionID.MakeSpell(AID.NoxAOERest), 5.5f, 1.6f, 5)
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class MarrowDrain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MarrowDrain1), new AOEShapeCone(10.44f, 60.Degrees()));
class MarrowDrain2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MarrowDrain2), new AOEShapeCone(10.44f, 60.Degrees()));
class BigHug(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BigHug), new AOEShapeRect(6, 1.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5515)]
public class A34Scathach(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Connla), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Connla2), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShadowLimb), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShadowcourtJester), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ChimeraPoppet), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShadowcourtHound), ArenaColor.Enemy);
    }
}
