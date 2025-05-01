namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class FangedCharge(BossModule module) : Components.StandardAOEs(module, AID.FangedCharge, new AOEShapeRect(30, 3), maxCasts: 2);

class HeavensearthSuspendedStone(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Stack, (uint)IconID.Spread, AID.Heavensearth, AID.SuspendedStone, 6, 6, 5.1f, alwaysShowSpreads: true);

class Shadowchase(BossModule module) : PlayActionAOEs(module, (uint)OID.Shadow, 0x11D1, new AOEShapeRect(40, 4), AID.Shadowchase, 3.15f);

class RoaringWind(BossModule module) : PlayActionAOEs(module, 0x485F, 0x11D2, new AOEShapeRect(40, 4), AID.RoaringWind, 5.5f, actorIsCaster: false)
{
    public bool Enabled;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Enabled ? base.ActiveAOEs(slot, actor) : [];
}
