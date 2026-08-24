namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class ManaScreen(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var ne in Module.Enemies(OID.ManaScreenNESW))
            Arena.AddLine(ne.Position + new WDir(-10, 8), ne.Position + new WDir(10, -8), ArenaColor.Object, 2);
        foreach (var nw in Module.Enemies(OID.ManaScreenNWSE))
            Arena.AddLine(nw.Position + new WDir(10, 8), nw.Position + new WDir(-10, -8), ArenaColor.Object, 2);
    }
}

class ReflectedRay(BossModule module) : Components.GenericAOEs(module)
{
    record struct Ray(WPos Origin, float Width, Angle Angle, DateTime Activation);
    private readonly List<Ray> _rays = [];

    private IEnumerable<Actor> Mirrors => Module.Enemies(OID.ManaScreenNESW).Concat(Module.Enemies(OID.ManaScreenNWSE));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _rays.Select(r => new AOEInstance(new AOEShapeRect(60, r.Width), r.Origin, r.Angle, r.Activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyRay1)
        {
            if (Mirrors.FirstOrDefault(m => m.Position.InRect(caster.Position, caster.Rotation, 40, 0, 8)) is { } mirror)
            {
                var angleStart = caster.Rotation.ToDirection();
                var angle = mirror.OID == (uint)OID.ManaScreenNESW ? angleStart.OrthoL() : angleStart.OrthoR();
                _rays.Add(new(mirror.Position, 10, angle.ToAngle(), Module.CastFinishAt(spell, 0.7f)));

                if (Mirrors.Exclude(mirror).FirstOrDefault(m => m.Position.InRect(mirror.Position, angle.ToAngle(), 60, 0, 10)) is { } mirror2)
                {
                    var angle2 = mirror2.OID == (uint)OID.ManaScreenNESW ? angle.OrthoR() : angle.OrthoL();
                    _rays.Add(new(mirror2.Position, 8, angle2.ToAngle(), Module.CastFinishAt(spell, 1.3f)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnergyRay4 or AID.EnergyRay5 or AID.EnergyRay3 or AID.EnergyRay2)
        {
            _rays.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1) && r.Angle.AlmostEqual(spell.Rotation, 0.1f));
            NumCasts++;
        }
    }
}
