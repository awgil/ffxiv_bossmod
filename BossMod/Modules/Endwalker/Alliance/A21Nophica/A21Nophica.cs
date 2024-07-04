namespace BossMod.Endwalker.Alliance.A21Nophica;

class ArenaBounds(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(28, 34);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x39)
        {
            if (state == 0x02000200)
                _aoe = new(donut, Module.Center, default, WorldState.FutureTime(5.8f));
            if (state is 0x00200010 or 0x00020001)
            {
                Arena.Bounds = A21Nophica.SmallerBounds;
                _aoe = null;
            }
            if (state is 0x00080004 or 0x00400004)
                Arena.Bounds = A21Nophica.DefaultBounds;
        }
    }
}

class FloralHaze(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
class SummerShade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SummerShade), new AOEShapeDonut(12, 40));
class SpringFlowers(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpringFlowers), new AOEShapeCircle(12));
class ReapersGale(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ReapersGaleAOE), new AOEShapeRect(36, 4, 36), 9);
class Landwaker(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LandwakerAOE), 10);
class Furrow(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Furrow), 6, 8);
class HeavensEarth(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HeavensEarthAOE), new AOEShapeCircle(5), true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12065)]
public class A21Nophica(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -238), DefaultBounds)
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(34);
    public static readonly ArenaBoundsCircle SmallerBounds = new(28);
}