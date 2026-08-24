namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class RightArmRayNormal(BossModule module) : Components.StandardAOEs(module, AID.RightArmRayNormalAOE, new AOEShapeCircle(10));

class RightArmRayBuffed(BossModule module) : Components.GenericAOEs(module)
{
    public class SphereState(Actor sphere, Angle increment)
    {
        public Actor Sphere = sphere;
        public Angle RotNext = sphere.Rotation;
        public Angle RotIncrement = increment;
        public int NumCastsLeft = 11;
    }

    private readonly List<SphereState> _spheres = [];
    private DateTime _activation;
    private static readonly AOEShapeCross _shape = new(16, 3);

    public bool Active => _spheres.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        foreach (var s in _spheres.Where(s => s.NumCastsLeft > 1))
            yield return new(_shape, s.Sphere.Position, s.RotNext + s.RotIncrement, _activation, Risky: false);
        foreach (var s in _spheres)
            yield return new(_shape, s.Sphere.Position, s.RotNext, _activation, ArenaColor.Danger);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_spheres.Count == 4 && NumCasts == 0)
        {
            // show positioning hint: find a pair of nearby spheres with opposite rotations, such that CCW is to the left of midpoint (if facing center)
            foreach (var ccwSphere in _spheres.Where(s => s.RotIncrement.Rad > 0))
            {
                var ccwOffset = ccwSphere.Sphere.Position - Module.Center;
                foreach (var cwSphere in _spheres.Where(s => s.RotIncrement.Rad < 0))
                {
                    // nearby spheres have distance ~20
                    var cwOffset = cwSphere.Sphere.Position - Module.Center;
                    if ((ccwOffset - cwOffset).LengthSq() < 500)
                    {
                        var midpointOffset = (ccwOffset + cwOffset) * 0.5f;
                        if (midpointOffset.OrthoL().Dot(ccwOffset) < 0)
                        {
                            Arena.AddCircle(Module.Center + midpointOffset, 1, ArenaColor.Safe);
                        }
                    }
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RightArmRayAOEFirst)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RightArmRayAOEFirst or AID.RightArmRayAOERest)
        {
            ++NumCasts;
            var sphereIndex = _spheres.FindIndex(s => s.Sphere.Position.AlmostEqual(caster.Position, 1));
            if (sphereIndex >= 0)
            {
                var sphere = _spheres[sphereIndex];
                sphere.RotNext += sphere.RotIncrement;
                if (--sphere.NumCastsLeft == 0)
                    _spheres.RemoveAt(sphereIndex);
            }
            _activation = WorldState.FutureTime(1.6f);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.AtomicSphereCW => -15.Degrees(),
            IconID.AtomicSphereCCW => 15.Degrees(),
            _ => default
        };
        if (increment != default)
            _spheres.Add(new(actor, increment));
    }
}

class RightArmRayVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.RightArmRayVoidzone, m => m.Enemies(OID.AtomicSphereVoidzone), 0.9f);
