namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class ManaSphere(BossModule module) : BossComponent(module)
{
    public enum Shape
    {
        BlueSphere,
        GreenDonut,
        PurpleBowtie,
        OrangeBowtie
    }

    public class Sphere(Actor actor)
    {
        public Actor Actor = actor;
        public Shape Shape;
        public WPos Origin;
        public int Side; // 0 (west) or 1 (east)
        public int Order; // 0 or 1
    }

    public readonly List<Sphere> Spheres = [];

    public class BlackHole
    {
        public WPos Position;
        public readonly List<Shape>[] Waves = [[], []];
    }

    public bool SwapDone { get; private set; }
    public bool HaveDebuff { get; private set; }

    const uint Green = 0xE0B7EA3C;
    const uint Blue = 0xE0F4E414;
    const uint Orange = 0xE03A90F6;
    const uint Purple = 0xE0FF9DCF;

    static readonly AOEShapeDonut Donut = new(0.6f, 1.2f);
    static readonly AOEShapeCircle Circle = new(1.2f);
    static readonly AOEShapeCone BowtieVertical = new(1.2f, 30.Degrees());
    static readonly AOEShapeCone BowtieHorizontal = new(1.2f, 30.Degrees(), 90.Degrees());

    public readonly BlackHole[] BlackHoles = [new() { Position = new(90, 100) }, new() { Position = new(110, 100) }];

    readonly List<Shape> _closeShapes = [];
    int _closeSide = -1;

    enum Letter
    {
        None,
        A,
        B
    }

    record struct Debuff(Letter L, DateTime Expire);

    readonly Debuff[] _playerAssignments = new Debuff[8];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ManaSphereSpawn)
        {
            Shape? s = (OID)caster.OID switch
            {
                OID.ManaSphereBlueSphere => Shape.BlueSphere,
                OID.ManaSphereGreenDonut => Shape.GreenDonut,
                OID.ManaSpherePurpleBowtie => Shape.PurpleBowtie,
                OID.ManaSphereOrangeBowtie => Shape.OrangeBowtie,
                _ => null
            };
            if (s != null)
            {
                Spheres.Add(new(caster) { Origin = spell.TargetXZ, Shape = s.Value });
                if (Spheres.Count == 8)
                    SortSpheres();
            }
        }

        if ((AID)spell.Action.ID == AID.BlackHoleAbsorb)
        {
            var ix = Spheres.FindIndex(s => s.Actor == caster);
            if (ix >= 0)
            {
                var sphere = Spheres[ix];
                BlackHoles[sphere.Side].Waves[sphere.Order].Add(sphere.Shape);
                Spheres.RemoveAt(ix);
            }
        }

        if ((AID)spell.Action.ID == AID.BloodyBurst)
        {
            var targetPos = WorldState.Actors.Find(spell.MainTargetID)!.Position;
            foreach (var s in Spheres)
            {
                if (s.Actor.Position.InCircle(targetPos, 5) && s.Order == 0)
                    // spheres can't be delayed multiple times, i.e. subsequent pops won't change their speed
                    s.Order = 1;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        Letter? l = (SID)status.ID switch
        {
            SID.MutationA => Letter.A,
            SID.MutationB => Letter.B,
            SID.MutatingCells => Letter.None,
            _ => null
        };
        if (l.HasValue && Raid.TryFindSlot(actor, out var slot))
        {
            HaveDebuff = true;
            _playerAssignments[slot] = new(l.Value, status.ExpireAt);
            if (l.Value == default)
                SwapDone = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var sphere in Spheres)
        {
            (var s, var c) = sphere.Shape switch
            {
                Shape.BlueSphere => (Circle, Blue),
                Shape.GreenDonut => (Donut, Green),
                Shape.PurpleBowtie => (BowtieVertical, Purple),
                Shape.OrangeBowtie => ((AOEShape)BowtieHorizontal, Orange),
                _ => (new AOEShapeCircle(0), 0u)
            };
            s.Draw(Arena, sphere.Actor.Position, default, c);
            if (s is AOEShapeCone)
                s.Draw(Arena, sphere.Actor.Position, 180.Degrees(), c);

            if (_playerAssignments[pcSlot].L == Letter.B && _closeShapes.Contains(sphere.Shape) && sphere.Order == 0 && sphere.Side != _closeSide)
                Arena.AddCircle(sphere.Actor.Position, 2, ArenaColor.Safe);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerAssignments[slot].L != default)
            hints.Add($"Debuff: {_playerAssignments[slot].L}", false);
    }

    void SortSpheres()
    {
        foreach (var s in Spheres)
            s.Side = s.Origin.X < 100 ? 0 : 1;

        void order(int side)
        {
            if (Spheres.Where(s => s.Side == side).Any(s => s.Origin.InCircle(HolePos(side), 8)))
            {
                _closeSide = side;
                foreach (var s in Spheres)
                {
                    if (s.Side == side)
                    {
                        if (s.Origin.InCircle(HolePos(side), 8))
                        {
                            s.Order = 0;
                            _closeShapes.Add(s.Shape);
                        }
                        else
                            s.Order = 1;
                    }
                }
            }
        }

        order(0);
        order(1);
    }

    static WPos HolePos(int side) => new(side == 0 ? 90 : 110, 100);
}

class BloodWakeningReplay(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<List<AOEInstance>> _predicted = [];

    public static readonly AOEShapeCircle Water = new(8);
    public static readonly AOEShapeDonut Aero = new(5, 60);
    public static readonly AOEShapeCone ThunderFire = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Count > 0 ? _predicted[0] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BloodWakening)
        {
            var activation1 = Module.CastFinishAt(spell, 1.7f);
            var activation2 = activation1.AddSeconds(5.1f);

            var holes = Module.FindComponent<ManaSphere>()!.BlackHoles;
            _predicted.Add([.. holes.SelectMany(h => h.Waves[0].SelectMany(s => MakeAOE(h.Position, s, activation1)))]);
            _predicted.Add([.. holes.SelectMany(h => h.Waves[1].SelectMany(s => MakeAOE(h.Position, s, activation2)))]);
        }
    }

    IEnumerable<AOEInstance> MakeAOE(WPos origin, ManaSphere.Shape shape, DateTime activation)
    {
        switch (shape)
        {
            case ManaSphere.Shape.BlueSphere:
                yield return new(Water, origin, default, activation);
                break;
            case ManaSphere.Shape.GreenDonut:
                yield return new(Aero, origin, default, activation);
                break;
            case ManaSphere.Shape.PurpleBowtie:
                yield return new(ThunderFire, origin, default, activation);
                yield return new(ThunderFire, origin, 180.Degrees(), activation);
                break;
            case ManaSphere.Shape.OrangeBowtie:
                yield return new(ThunderFire, origin, 90.Degrees(), activation);
                yield return new(ThunderFire, origin, -90.Degrees(), activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LindwurmsWaterIII:
            case AID.LindwurmsAeroIII:
            case AID.StraightforwardThunderII:
            case AID.SidewaysFireII:
                NumCasts++;
                if (_predicted.Count > 0)
                {
                    if (_predicted[0].Count > 0)
                        _predicted[0].RemoveAt(0);
                    if (_predicted[0].Count == 0)
                        _predicted.RemoveAt(0);
                }
                break;
        }
    }
}

class Netherworld(BossModule module) : Components.UniformStackSpread(module, 6, 0, maxStackSize: 4)
{
    BitMask Forbidden;
    DateTime Activation;
    bool Far;
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        bool? far = (AID)spell.Action.ID switch
        {
            AID.NetherworldNear => false,
            AID.NetherworldFar => true,
            _ => null
        };
        if (far.HasValue)
        {
            Forbidden = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.MutationA) != null).Mask();
            Activation = Module.CastFinishAt(spell, 1.3f);
            Far = far.Value;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WailingWave)
        {
            Activation = default;
            NumCasts++;
        }
    }

    public override void Update()
    {
        Stacks.Clear();

        if (Activation == default)
            return;

        var target = Far ? Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position) : Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);
        if (target != null)
            AddStack(target, Activation, Forbidden);
    }
}
