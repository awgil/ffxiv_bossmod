namespace BossMod.Endwalker.Alliance.A31Thaliak;

// arena is split into 4x4 squares, with 2 safe spots - one along edge, another in farthest corner
// there are only two possible patterns here - safe spot is either along N or E edge (out of 8 possible ones):
// XXOX  and  OXXX
// XXXX       XXXX
// XXXX       XXXO
// OXXX       XXXX
// the pattern is then rotated CW or CCW, giving 4 possible results
class Hieroglyphika(BossModule module) : Components.GenericAOEs(module, AID.HieroglyphikaAOE)
{
    public bool BindsAssigned;
    public WDir SafeSideDir;
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(6, 6, 6);
    private static readonly WDir[] _canonicalSafespots = [new(6, -18), new(-18, 18)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            BindsAssigned = true;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001)
            return;

        WDir dir = index switch
        {
            0x17 => new(-1, 0),
            0x4A => new(0, 1),
            _ => default
        };
        if (dir != default)
            SafeSideDir = dir;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var dir = (IconID)iconID switch
        {
            IconID.HieroglyphikaCW => SafeSideDir.OrthoR(),
            IconID.HieroglyphikaCCW => SafeSideDir.OrthoL(),
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
                    AOEs.Add(new(_shape, Module.Center + cellOffset, default, activation));
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
