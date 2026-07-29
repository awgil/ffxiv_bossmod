namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

sealed class RightArmRayNormal(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RightArmRayNormalAOE, 10f);

sealed class RightArmRayBuffed(BossModule module) : Components.GenericRotatingAOE(module)
{
    public struct SphereState(Actor sphere, Angle increment)
    {
        public readonly Actor Sphere = sphere;
        public Angle RotNext = sphere.Rotation;
        public Angle RotIncrement = increment;
        public int NumCastsLeft = 11;
    }

    private readonly List<SphereState> _spheres = [];
    private DateTime _activation;
    private static readonly AOEShapeCross _shape = new(16f, 3f);

    public bool Active => Sequences.Count > 0;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _spheres.Count;
        if (count == 4 && NumCasts == 0)
        {
            // show positioning hint: find a pair of nearby spheres with opposite rotations, such that CCW is to the left of midpoint (if facing center)
            for (var i = 0; i < count; ++i)
            {
                var ccwSphere = _spheres[i];
                if (ccwSphere.RotIncrement.Rad > 0f)
                {
                    var ccwOffset = ccwSphere.Sphere.Position - Arena.Center;
                    for (var j = 0; j < count; ++j)
                    {
                        var cwSphere = _spheres[j];
                        if (cwSphere.RotIncrement.Rad < 0f)
                        {
                            // nearby spheres have distance ~20
                            var cwOffset = cwSphere.Sphere.Position - Arena.Center;
                            if ((ccwOffset - cwOffset).LengthSq() < 500f)
                            {
                                var midpointOffset = (ccwOffset + cwOffset) * 0.5f;
                                if (midpointOffset.OrthoL().Dot(ccwOffset) < 0f)
                                {
                                    Arena.ZoneCircleOutline(Arena.Center + midpointOffset, 1f, Colors.Safe);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RightArmRayAOEFirst)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RightArmRayAOEFirst or (uint)AID.RightArmRayAOERest)
        {
            ++NumCasts;
            var count = _spheres.Count;
            for (var i = 0; i < count; ++i)
            {
                var s = _spheres[i];
                if (s.Sphere.Position.AlmostEqual(caster.Position, 1f))
                {
                    var sphere = _spheres[i];
                    sphere.RotNext += sphere.RotIncrement;
                    if (--sphere.NumCastsLeft == 0)
                        _spheres.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.AtomicSphereCW => -15f.Degrees(),
            (uint)IconID.AtomicSphereCCW => 15f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(_shape, actor.Position.Quantized(), actor.Rotation, increment, _activation, 1.6f, 11));
            _spheres.Add(new(actor, increment));
        }
    }
}

sealed class RightArmRayVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.RightArmRayVoidzone, GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AtomicSphereVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
