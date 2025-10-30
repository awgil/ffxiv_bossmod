namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationStar(BossModule module) : Components.RaidwideCastDelay(module, AID.ImitationStarCast, AID.ImitationStar, 1.8f);

class ImitationIcicle : Components.StandardAOEs
{
    public ImitationIcicle(BossModule module) : base(module, AID.ImitationIcicleAOE, 8)
    {
        Color = ArenaColor.Danger;
    }
}

class DraconiformMotion(BossModule module) : Components.GroupedAOEs(module, [AID.DraconiformMotion1, AID.DraconiformMotion2], new AOEShapeCone(60, 45.Degrees()));
class DraconiformHint(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(pc), 45.Degrees(), ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Stack with party!", false);
    }
}

class DreadDeluge(BossModule module) : Components.SingleTargetCast(module, AID.DreadDeluge, "Tankbusters");
class Golem(BossModule module) : Components.Adds(module, (uint)OID.IceGolem, 1);
class FrigidDive : Components.StandardAOEs
{
    public FrigidDive(BossModule module) : base(module, AID.FrigidDive, new AOEShapeRect(60, 10))
    {
        Color = ArenaColor.Danger;
    }
}

class IceSprite(BossModule module) : Components.Adds(module, (uint)OID.IceSprite, 1);

class SpriteInvincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);

class LifelessLegacy(BossModule module) : Components.RaidwideCastDelay(module, AID.LifelessLegacyCast, AID.LifelessLegacy, 1.8f)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Activation != default && Activation < WorldState.FutureTime(10))
            hints.Add(Hint);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13838, PlanLevel = 100)]
public class FT03MarbleDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-337, 157), new ArenaBoundsCircle(30))
{
    public override bool DrawAllPlayers => true;
}

