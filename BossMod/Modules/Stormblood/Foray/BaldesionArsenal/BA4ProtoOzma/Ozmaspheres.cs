namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class Ozmaspheres(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCapsule capsule = new(6, 3);

    private static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.Ozmasphere);
        var count = orbs.Count;
        if (count == 0)
            return [];
        List<Actor> orbz = [with(count)];
        for (var i = 0; i < count; ++i)
        {
            var o = orbs[i];
            if (!o.IsDead)
                orbz.Add(o);
        }
        return orbz;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count == 0 || actor.Role == Role.Tank)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var o = orbs[i];
            aoes[i] = new(capsule, o.Position, o.Rotation);
        }
        return aoes;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var orbs = GetOrbs(Module);
        if (orbs.Count != 0)
        {
            if (actor.Role == Role.Tank)
                hints.Add("Soak the orbs (with mitigations)!");
            else
                hints.Add("Avoid the orbs!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
        {
            var forbidden = new ShapeDistance[count];
            if (actor.Role == Role.Tank)
            {
                for (var i = 0; i < count; ++i)
                {
                    var o = orbs[i];
                    forbidden[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
                }
                hints.AddForbiddenZone(new SDIntersection(forbidden), DateTime.MaxValue);
            }
            else
                base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircleOutline(orbs[i].Position, 1f, pc.Role == Role.Tank ? Colors.Safe : default);
    }
}