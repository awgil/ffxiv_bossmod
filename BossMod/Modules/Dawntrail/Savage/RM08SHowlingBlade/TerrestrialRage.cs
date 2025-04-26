namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class FangedCharge(BossModule module) : Components.StandardAOEs(module, AID.FangedCharge, new AOEShapeRect(30, 3), maxCasts: 2);

class Heavensearth(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Heavensearth, AID.Heavensearth, 6, 5.1f);
class SuspendedStone(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SuspendedStone, AID.SuspendedStone, 6, 5.1f);

class Shadowchase(BossModule module) : PlayActionAOEs(module, (uint)OID._Gen_HowlingBlade, 0x11D1, new AOEShapeRect(40, 4), AID.Shadowchase, 3.15f);

class RoaringWind(BossModule module) : PlayActionAOEs(module, 0x485F, 0x11D2, new AOEShapeRect(40, 4), AID.RoaringWind, 5.5f)
{
    public bool Enabled;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.Clear();
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Enabled ? base.ActiveAOEs(slot, actor) : [];
}
