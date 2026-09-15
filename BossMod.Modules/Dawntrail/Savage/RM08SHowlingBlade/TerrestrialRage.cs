namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class FangedCharge(BossModule module) : Components.StandardAOEs(module, AID.FangedCharge, new AOEShapeRect(30, 3), maxCasts: 2);

class HeavensearthSuspendedStone(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Stack, (uint)IconID.Spread, AID.Heavensearth, AID.SuspendedStone, 6, 6, 5.1f, alwaysShowSpreads: true);

class Shadowchase(BossModule module) : PlayActionAOEs(module, (uint)OID.Shadow, 0x11D1, new AOEShapeRect(40, 4), AID.Shadowchase, 3.15f);

class RoaringWind(BossModule module) : PlayActionAOEs(module, 0x485F, 0x11D2, new AOEShapeRect(40, 4), AID.RoaringWind, 5.5f, actorIsCaster: false)
{
    public bool Enabled;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Enabled ? base.ActiveAOEs(slot, actor) : [];
}

class TRHints(BossModule module) : BossComponent(module)
{
    private readonly Shadowchase _shadows = module.FindComponent<Shadowchase>()!;
    private readonly HeavensearthSuspendedStone _spread = module.FindComponent<HeavensearthSuspendedStone>()!;
    private readonly RM08SHowlingBladeConfig _config = Service.Config.Get<RM08SHowlingBladeConfig>();
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var shadows = _shadows.Casters.ToList();

        if (_config.TRHints != RM08SHowlingBladeConfig.TerrestrialRageStrategy.Clock || _spread.Spreads.Count == 0 || shadows.Count == 0)
            return;

        var isSpread = _spread.ActiveSpreadTargets.Contains(pc);
        if (isSpread && _spread.ActiveSpreadTargets.Any(t => t.Class.IsSupport() != pc.Class.IsSupport()))
        {
            ReportError("nonstandard comp or missing bodies for stack/spread, not drawing hints");
            return;
        }

        var northSafe = shadows.Any(s => s.Rotation.AlmostEqual(default, 0.1f));
        var adj = northSafe ? 0.Degrees() : -36.Degrees();

        Angle safeAngle;

        if (isSpread)
        {
            switch (_prc[WorldState.Party.Members[pcSlot].ContentId])
            {
                case PartyRolesConfig.Assignment.MT:
                case PartyRolesConfig.Assignment.R1:
                    safeAngle = -108.Degrees();
                    break;
                case PartyRolesConfig.Assignment.H1:
                case PartyRolesConfig.Assignment.M1:
                    safeAngle = -36.Degrees();
                    break;
                case PartyRolesConfig.Assignment.OT:
                case PartyRolesConfig.Assignment.M2:
                    safeAngle = 36.Degrees();
                    break;
                case PartyRolesConfig.Assignment.H2:
                case PartyRolesConfig.Assignment.R2:
                    safeAngle = 108.Degrees();
                    break;
                default:
                    return;
            }
        }
        else
            safeAngle = 180.Degrees();

        Arena.AddCircle(Arena.Center + (safeAngle + adj).ToDirection() * 10, 0.5f, ArenaColor.Safe);
    }
}
