namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class DarkNebula(BossModule module) : Components.GenericKnockback(module)
{
    private const float Length = 4f;
    private const float HalfWidth = 1.75f;

    public readonly List<Knockback> KBs = [with(8)];
    private readonly AOEShapeRect rect = new(60f, 50f);
    private readonly WDir[] directions = [45f.Degrees().ToDirection(), -135f.Degrees().ToDirection(), -45f.Degrees().ToDirection(), 135f.Degrees().ToDirection()];
    private readonly WPos[] circleCenters = [new(166.251f, 800f), new(133.788f, 800f), new(150f, 816.227f), new(150f, 783.812f)];
    private readonly List<ShapeDistance> shapeDistances = [with(4)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = KBs.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(KBs)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
        {
            var act = Module.CastFinishAt(spell);
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var countKBs = KBs.Count;
            AddSource(Kind.DirRight);
            AddSource(Kind.DirLeft, 180f.Degrees());

            void AddSource(Kind knockbackkind, Angle dir = default) => KBs.Add(new(loc, 20f, act, rect, rot + dir, knockbackkind));

            var indices = IndicesForRotation(rot);
            if (indices != default)
            {
                if (countKBs is 0 or 6)
                {
                    var forbidden = new List<ShapeDistance>(2);

                    for (var i = 0; i < 2; ++i)
                    {
                        forbidden.Add(new SDInvertedRect(circleCenters[indices.Item1[i]], indices.Item2, Length, 0f, HalfWidth));
                    }
                    shapeDistances.Add(new SDIntersection([.. forbidden]));
                }
                else
                {
                    ref readonly var kbPrev = ref KBs.Ref(countKBs - 2);
                    ref readonly var kbCur = ref KBs.Ref(countKBs);
                    var rotationMatch = kbPrev.Direction.AlmostEqual(kbCur.Direction + 90f.Degrees(), Angle.DegToRad);
                    var prevIndices = IndicesForRotation(kbPrev.Direction);
                    var circleIndex = rotationMatch ? indices.Item1[0] : indices.Item1[1];
                    if (countKBs == 2)
                    {
                        var prevCircleIndex = rotationMatch ? prevIndices.Item1[0] : prevIndices.Item1[1];
                        shapeDistances[0] = new SDInvertedRect(circleCenters[prevCircleIndex], prevIndices.Item2, Length, 0f, HalfWidth);
                    }
                    shapeDistances.Add(new SDInvertedRect(circleCenters[circleIndex], indices.Item2, Length, 0f, HalfWidth));
                }
            }
        }

        (int[], WDir) IndicesForRotation(Angle rot)
        => (int)rot.Deg switch
        {
            -45 => ([2, 0], directions[2]),
            -135 => ([1, 2], directions[3]),
            134 => ([3, 1], directions[0]),
            44 => ([0, 3], directions[1]),
            _ => default
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
        {
            ++NumCasts;
            if (KBs.Count > 1)
            {
                KBs.RemoveRange(0, 2);
                shapeDistances.RemoveAt(0);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (KBs.Count == 0)
        {
            return;
        }
        ref readonly var kb = ref KBs.Ref(0);
        hints.AddForbiddenZone(shapeDistances[0], kb.Activation);
    }
}
