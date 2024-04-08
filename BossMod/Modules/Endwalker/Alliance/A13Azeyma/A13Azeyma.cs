namespace BossMod.Endwalker.Alliance.A13Azeyma;

class Voidzone : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index == 0x1C)
        {
            if (state == 0x00020001)
                module.Arena.Bounds = new ArenaBoundsTri(module.Bounds.Center, 18);
            if (state == 0x00080004)
                module.Arena.Bounds =  new ArenaBoundsCircle(module.Bounds.Center, 30);
        }
    }
}

class WildfireWard() : Components.KnockbackFromCastTarget(ActionID.MakeSpell(AID.IlluminatingGlimpse), 15, false, 1, kind: Kind.DirLeft);
class WardensWarmth() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.WardensWarmthAOE), 6);
class FleetingSpark() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.FleetingSpark), new AOEShapeCone(60, 135.Degrees()));
class SolarFold() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SolarFoldAOE), new AOEShapeCross(30, 5));
class Sunbeam() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Sunbeam), new AOEShapeCircle(9), 14);
class SublimeSunset() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.SublimeSunsetAOE), 40); // TODO: check falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11277, SortOrder = 5)]
public class A13Azeyma(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(-750, -750), 30));