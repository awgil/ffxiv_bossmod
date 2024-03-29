namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarWingsL : Components.SelfTargetedAOEs
{
    public SolarWingsL() : base(ActionID.MakeSpell(AID.SolarWingsL), new AOEShapeCone(30, 75.Degrees())) { }
}

class SolarWingsR : Components.SelfTargetedAOEs
{
    public SolarWingsR() : base(ActionID.MakeSpell(AID.SolarWingsR), new AOEShapeCone(30, 75.Degrees())) { }
}

class SolarFlair : Components.GenericAOEs
{
    private List<WPos> _sunstorms = new();
    private BitMask _adjusted;

    private static readonly float _kickDistance = 18;
    private static readonly AOEShapeCircle _shape = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _sunstorms.Select(p => new AOEInstance(_shape, p));

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.Sunstorm)
            _sunstorms.Add(actor.Position);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TeleportHeat:
                if (_sunstorms.Count > 0)
                {
                    var closestSunstorm = _sunstorms.Select((p, i) => (p, i)).Where(pi => !_adjusted[pi.i]).MinBy(pi => (pi.p - spell.TargetXZ).LengthSq());
                    // should teleport within range ~6
                    if ((closestSunstorm.p - spell.TargetXZ).LengthSq() < 50)
                    {
                        _sunstorms[closestSunstorm.i] = closestSunstorm.p + _kickDistance * (closestSunstorm.p - spell.TargetXZ).Normalized();
                        _adjusted.Set(closestSunstorm.i);
                    }
                    else
                    {
                        module.ReportError(this, $"Unexpected teleport location: {spell.TargetXZ}, closest sunstorm at {closestSunstorm.p}");
                    }
                }
                else
                {
                    module.ReportError(this, "Unexpected teleport, no sunstorms active");
                }
                break;
            case AID.SolarFlair:
                ++NumCasts;
                if (_sunstorms.RemoveAll(p => p.AlmostEqual(caster.Position, 1)) != 1)
                    module.ReportError(this, $"Unexpected solar flair position {caster.Position}");
                break;
        }
    }
}
