namespace BossMod.Endwalker.Alliance.A34Eulogia;

// see A31 for details; apparently there is only 1 pattern here (rotated CW or CCW)
// unlike A31, origins are not cell centers, but south sides
class Hieroglyphika(BossModule module) : Components.GenericAOEs(module, AID.HieroglyphikaAOE)
{
    public bool BindsAssigned;
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(12, 6);
    private static readonly WDir[] _canonicalSafespots = [new(-18, 18), new(18, -6)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            BindsAssigned = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        WDir dir = (IconID)iconID switch
        {
            IconID.HieroglyphikaCW => new(-1, 0),
            IconID.HieroglyphikaCCW => new(1, 0),
            _ => default
        };
        if (dir == default)
            return;

        WDir[] safespots = [.. _canonicalSafespots.Select(d => d.Rotate(dir))];
        var activation = WorldState.FutureTime(17.1f);
        for (int z = -3; z <= 3; z += 2)
        {
            for (int x = -3; x <= 3; x += 2)
            {
                var cellOffset = new WDir(x * 6, z * 6);
                if (!safespots.Any(s => s.AlmostEqual(cellOffset, 1)))
                {
                    AOEs.Add(new(_shape, Module.Center + cellOffset + new WDir(0, 6), 180.Degrees(), activation));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            var cnt = AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (cnt != 1)
                ReportError($"Incorrect AOE prediction: {caster.Position} matched {cnt} aoes");
        }
    }
}
