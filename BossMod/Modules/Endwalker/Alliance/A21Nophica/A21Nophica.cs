namespace BossMod.Endwalker.Alliance.A21Nophica;

class Voidzone : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index == 0x39)
        {
            if (state == 0x02000200)
                module.Arena.Bounds = new ArenaBoundsCircle(module.Bounds.Center, 28);
            if (state == 0x00400004)
                module.Arena.Bounds =  new ArenaBoundsCircle(module.Bounds.Center, 34);
        }
    }
}

class SummerShade() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SummerShade), new AOEShapeDonut(12, 40));
class SpringFlowers() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SpringFlowers), new AOEShapeCircle(12));
class ReapersGale() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.ReapersGaleAOE), new AOEShapeRect(36, 4, 36), 9);
class Landwaker() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.LandwakerAOE), 10);
class Furrow() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.Furrow), 6, 8);
class HeavensEarth() : Components.BaitAwayCast(ActionID.MakeSpell(AID.HeavensEarthAOE), new AOEShapeCircle(5), true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12065)]
public class A21Nophica(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(0, -238), 34));