#pragma warning disable IDE0028
namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class ManaSphere(BossModule module) : BossComponent(module)
{
    public enum Shape
    {
        BlueSphere,
        GreenDonut,
        PurpleBowtie,
        OrangeBowtie
    }

    public sealed class Sphere(Actor actor)
    {
        public Actor Actor = actor;
        public Shape Shape;
        public WPos Origin;
        public int Side;  // 0 west, 1 east
        public int Order; // 0 first wave, 1 delayed
    }

    public sealed class BlackHole
    {
        public WPos Position;

        // waves[waveIndex][entryIndex]
        public Shape[][] Waves =
        [
            new Shape[4],
            new Shape[4]
        ];

        public int[] WaveCounts = new int[2];
    }

    public readonly List<Sphere> Spheres = new(8);
    public readonly BlackHole[] BlackHoles =
    [
        new() { Position = new(90, 100) },
        new() { Position = new(110, 100) }
    ];

    public bool SwapDone;
    public bool HaveDebuff;

    const uint Green = 0xE0B7EA3C;
    const uint Blue = 0xE0F4E414;
    const uint Orange = 0xE03A90F6;
    const uint Purple = 0xE0FF9DCF;

    static readonly AOEShapeDonut Donut = new(0.6f, 1.2f);
    static readonly AOEShapeCircle Circle = new(1.2f);
    static readonly AOEShapeCone BowtieVertical = new(1.2f, 30f.Degrees());
    static readonly AOEShapeCone BowtieHorizontal = new(1.2f, 30f.Degrees(), 90f.Degrees());

    private readonly Shape[] _closeShapes = new Shape[4];
    private int _closeShapeCount;
    private int _closeSide = -1;

    enum Letter { None, A, B }

    private record struct Debuff(Letter L, DateTime Expire);
    private readonly Debuff[] _playerAssignments = new Debuff[8];

    // ============================
    // Events
    // ============================

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ManaSphereSpawn:
                AddSphere(caster, spell.TargetXZ);
                break;

            case (uint)AID.BlackHoleAbsorb:
                AbsorbSphere(caster);
                break;

            case (uint)AID.BloodyBurst:
                DelayNearbySpheres(spell);
                break;
        }
    }

    private void AddSphere(Actor caster, WPos origin)
    {
        Shape shape;
        switch (caster.OID)
        {
            case (uint)OID.ManaSphereBlueSphere: shape = Shape.BlueSphere; break;
            case (uint)OID.ManaSphereGreenDonut: shape = Shape.GreenDonut; break;
            case (uint)OID.ManaSpherePurpleBowtie: shape = Shape.PurpleBowtie; break;
            case (uint)OID.ManaSphereOrangeBowtie: shape = Shape.OrangeBowtie; break;
            default: return;
        }

        Spheres.Add(new Sphere(caster)
        {
            Shape = shape,
            Origin = origin
        });

        if (Spheres.Count == 8)
            SortSpheres();
    }

    private void AbsorbSphere(Actor caster)
    {
        var span = CollectionsMarshal.AsSpan(Spheres);
        for (var i = 0; i < span.Length; ++i)
        {
            if (span[i].Actor == caster)
            {
                ref var s = ref span[i];
                var hole = BlackHoles[s.Side];
                var wave = s.Order;

                hole.Waves[wave][hole.WaveCounts[wave]++] = s.Shape;
                Spheres.RemoveAt(i);
                return;
            }
        }
    }

    private void DelayNearbySpheres(ActorCastEvent spell)
    {
        var target = WorldState.Actors.Find(spell.MainTargetID);
        if (target == null)
            return;

        var span = CollectionsMarshal.AsSpan(Spheres);
        var pos = target.Position;

        for (var i = 0; i < span.Length; ++i)
        {
            ref var s = ref span[i];
            if (s.Order == 0 && s.Actor.Position.InCircle(pos, 5))
                s.Order = 1;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        Letter letter;
        switch (status.ID)
        {
            case (uint)SID.MutationA:
                letter = Letter.A;
                break;

            case (uint)SID.MutationB:
                letter = Letter.B;
                break;

            case (uint)SID.MutatingCells:
                letter = Letter.None;
                SwapDone = true;
                break;

            default:
                return;
        }

        if (Raid.FindSlot(actor.InstanceID) is int slot && slot >= 0)
        {
            HaveDebuff = true;
            _playerAssignments[slot] = new(letter, status.ExpireAt);
        }
    }

    // ============================
    // Sorting
    // ============================

    private void SortSpheres()
    {
        _closeShapeCount = 0;
        _closeSide = -1;

        var span = CollectionsMarshal.AsSpan(Spheres);

        // determine side
        for (var i = 0; i < span.Length; ++i)
            span[i].Side = span[i].Origin.X < 100 ? 0 : 1;

        for (var side = 0; side < 2; ++side)
        {
            var foundClose = false;

            for (var i = 0; i < span.Length; ++i)
            {
                if (span[i].Side == side &&
                    span[i].Origin.InCircle(HolePos(side), 8))
                {
                    foundClose = true;
                    break;
                }
            }

            if (!foundClose)
                continue;

            _closeSide = side;

            for (var i = 0; i < span.Length; ++i)
            {
                ref var s = ref span[i];
                if (s.Side != side)
                    continue;

                if (s.Origin.InCircle(HolePos(side), 8))
                {
                    s.Order = 0;
                    _closeShapes[_closeShapeCount++] = s.Shape;
                }
                else
                {
                    s.Order = 1;
                }
            }
        }
    }

    private static WPos HolePos(int side)
        => new(side == 0 ? 90 : 110, 100);

    // ============================
    // Drawing
    // ============================

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var span = CollectionsMarshal.AsSpan(Spheres);

        for (var i = 0; i < span.Length; ++i)
        {
            ref var sphere = ref span[i];

            AOEShape shape;
            uint color;

            switch (sphere.Shape)
            {
                case Shape.BlueSphere:
                    shape = Circle; color = Blue; break;
                case Shape.GreenDonut:
                    shape = Donut; color = Green; break;
                case Shape.PurpleBowtie:
                    shape = BowtieVertical; color = Purple; break;
                default:
                    shape = BowtieHorizontal; color = Orange; break;
            }

            shape.Draw(Arena, sphere.Actor.Position, default, color);

            if (shape is AOEShapeCone)
                shape.Draw(Arena, sphere.Actor.Position, 180f.Degrees(), color);

            if (_playerAssignments[pcSlot].L == Letter.B &&
                sphere.Order == 0 &&
                sphere.Side != _closeSide &&
                IsCloseShape(sphere.Shape))
            {
                Arena.ZoneCircleOutline(sphere.Actor.Position, 2, Colors.Safe);
            }
        }
    }

    private bool IsCloseShape(Shape shape)
    {
        for (var i = 0; i < _closeShapeCount; ++i)
            if (_closeShapes[i] == shape)
                return true;
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerAssignments[slot].L != Letter.None)
            hints.Add($"Debuff: {_playerAssignments[slot].L}", false);
    }
}
sealed class BloodWakeningReplay(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEInstance[] _wave1 = new AOEInstance[16];
    private readonly AOEInstance[] _wave2 = new AOEInstance[16];

    private int _wave1Count;
    private int _wave2Count;

    private int _wave1Resolved;
    private int _wave2Resolved;

    private int _activeWave = -1;

    public static readonly AOEShapeCircle Water = new(8);
    public static readonly AOEShapeDonut Aero = new(5, 60);
    public static readonly AOEShapeCone ThunderFire = new(40f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _activeWave switch
        {
            0 => _wave1.AsSpan(_wave1Resolved, _wave1Count - _wave1Resolved),
            1 => _wave2.AsSpan(_wave2Resolved, _wave2Count - _wave2Resolved),
            _ => ReadOnlySpan<AOEInstance>.Empty
        };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.BloodWakening)
            return;

        var activation1 = Module.CastFinishAt(spell, 1.7d);
        var activation2 = activation1.AddSeconds(5.1d);

        var mana = Module.FindComponent<ManaSphere>();
        if (mana == null)
            return;

        BuildWave(mana.BlackHoles, 0, activation1, _wave1, ref _wave1Count);
        BuildWave(mana.BlackHoles, 1, activation2, _wave2, ref _wave2Count);

        _wave1Resolved = 0;
        _wave2Resolved = 0;

        _activeWave = _wave1Count > 0 ? 0 : -1;
    }

    private void BuildWave(ManaSphere.BlackHole[] holes, int waveIndex,
        DateTime activation, AOEInstance[] buffer, ref int count)
    {
        count = 0;

        for (var h = 0; h < holes.Length; ++h)
        {
            var hole = holes[h];
            var shapes = hole.Waves[waveIndex];
            var shapeCount = hole.WaveCounts[waveIndex];

            for (var i = 0; i < shapeCount; ++i)
            {
                AddShapeAOEs(shapes[i], hole.Position, activation, buffer, ref count);
            }
        }
    }

    private void AddShapeAOEs(ManaSphere.Shape shape, WPos origin,
        DateTime activation, AOEInstance[] buffer, ref int count)
    {
        switch (shape)
        {
            case ManaSphere.Shape.BlueSphere:
                buffer[count++] = new(Water, origin, default, activation);
                break;

            case ManaSphere.Shape.GreenDonut:
                buffer[count++] = new(Aero, origin, default, activation);
                break;

            case ManaSphere.Shape.PurpleBowtie:
                buffer[count++] = new(ThunderFire, origin, default, activation);
                buffer[count++] = new(ThunderFire, origin, 180f.Degrees(), activation);
                break;

            case ManaSphere.Shape.OrangeBowtie:
                buffer[count++] = new(ThunderFire, origin, 90f.Degrees(), activation);
                buffer[count++] = new(ThunderFire, origin, (-90f).Degrees(), activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LindwurmsWaterIII:
            case (uint)AID.LindwurmsAeroIII:
            case (uint)AID.StraightforwardThunderII:
            case (uint)AID.SidewaysFireII:

                ++NumCasts;
                ResolveOne();
                break;
        }
    }

    private void ResolveOne()
    {
        if (_activeWave == 0)
        {
            ++_wave1Resolved;

            if (_wave1Resolved >= _wave1Count)
            {
                _activeWave = _wave2Count > 0 ? 1 : -1;
            }
        }
        else if (_activeWave == 1)
        {
            ++_wave2Resolved;

            if (_wave2Resolved >= _wave2Count)
            {
                _activeWave = -1;
            }
        }
    }
}
sealed class Netherworld(BossModule module) : Components.UniformStackSpread(module, 6, 0, maxStackSize: 4)
{
    private BitMask _forbidden;
    private DateTime _activation;
    private bool _far;

    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        bool? far = spell.Action.ID switch
        {
            (uint)AID.NetherworldNear => false,
            (uint)AID.NetherworldFar => true,
            _ => null
        };

        if (far == null)
            return;

        _far = far.Value;
        _activation = Module.CastFinishAt(spell, 1.3d);

        BuildForbiddenMask();
    }

    private void BuildForbiddenMask()
    {
        _forbidden = default;

        var party = Raid.WithSlot(true, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            var (slot, actor) = party[i];
            if (actor.FindStatus((uint)SID.MutationA) != null)
                _forbidden.Set(slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WailingWave)
        {
            _activation = default;
            ++NumCasts;
        }
    }

    public override void Update()
    {
        Stacks.Clear();

        if (_activation == default)
            return;

        var target = _far
            ? Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position)
            : Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);

        if (target != null)
            AddStack(target, _activation, _forbidden);
    }
}
