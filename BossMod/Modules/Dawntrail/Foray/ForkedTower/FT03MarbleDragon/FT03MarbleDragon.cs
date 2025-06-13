namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationStar(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_ImitationStar, AID._Weaponskill_ImitationStar1, 1.8f);

class DraconiformMotion(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_DraconiformMotion1, AID._Weaponskill_DraconiformMotion2], new AOEShapeCone(60, 45.Degrees()));
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

class DreadDeluge(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_DreadDeluge1, "Tankbusters");
class Golem(BossModule module) : Components.Adds(module, (uint)OID._Gen_IceGolem, 1);
class FrigidDive : Components.StandardAOEs
{
    public FrigidDive(BossModule module) : base(module, AID._Weaponskill_FrigidDive1, new AOEShapeRect(60, 10))
    {
        Color = ArenaColor.Danger;
    }
}

class IceSprite(BossModule module) : Components.Adds(module, (uint)OID._Gen_IceSprite, 1);

class LifelessLegacy(BossModule module) : Components.RaidwideCastDelay(module, AID._Ability_LifelessLegacy, AID._Weaponskill_LifelessLegacy, 1.8f, hint: "");

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13838)]
public class FT03MarbleDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-337, 157), new ArenaBoundsCircle(30))
{
    public override bool DrawAllPlayers => true;
}

