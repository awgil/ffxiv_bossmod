namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P6HallowedWings(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = []; // origin is always (122, 100 +- 11), direction -90

    private static readonly AOEShapeRect _shape = new(50f, 11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = spell.Action.ID switch
        {
            (uint)AID.HallowedWingsLN or (uint)AID.HallowedWingsLF => _shape.HalfWidth,
            (uint)AID.HallowedWingsRN or (uint)AID.HallowedWingsRF => -_shape.HalfWidth,
            _ => 0
        };
        if (offset == 0)
        {
            return;
        }
        var origin = caster.Position + offset * spell.Rotation.ToDirection().OrthoL();
        AOE = [new(_shape, origin.Quantized(), spell.Rotation, Module.CastFinishAt(spell, 0.8d))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HallowedWingsAOELeft or (uint)AID.HallowedWingsAOERight or (uint)AID.CauterizeN)
        {
            ++NumCasts;
        }
    }
}

// note: we want to show hint much earlier than cast start - we assume component is created right as hallowed wings starts, meaning nidhogg is already in place
sealed class P6CauterizeN : Components.GenericAOEs
{
    public AOEInstance[] AOE = []; // origin is always (100 +- 11, 100 +- 34), direction 0/180

    private static readonly AOEShapeRect _shape = new(80f, 11f);

    public P6CauterizeN(BossModule module) : base(module, (uint)AID.CauterizeN)
    {
        var caster = module.Enemies((uint)OID.NidhoggP6).FirstOrDefault();
        if (caster != null)
        {
            AOE = [new(_shape, caster.Position.Quantized(), caster.Rotation, WorldState.FutureTime(8.6d))];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;
}

abstract class P6HallowedPlume(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HallowedPlume, centerAtTarget: true)
{
    protected P6HallowedWings? _wings = module.FindComponent<P6HallowedWings>();
    protected bool _far;
    private Actor? _caster;

    private static readonly AOEShapeCircle _shape = new(10f);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_caster != null)
        {
            var players = Raid.WithoutSlot(false, true, true).SortedByRange(_caster.Position);
            var targets = _far ? players.TakeLast(2) : players.Take(2);
            foreach (var t in targets)
                CurrentBaits.Add(new(_caster, t, _shape));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var shouldBait = actor.Role == Role.Tank;
        var isBaiting = IsBaitTarget(actor);
        var stayFar = shouldBait == _far;
        hints.Add(stayFar ? "Stay far!" : "Stay close!", shouldBait != isBaiting);

        if (shouldBait == isBaiting)
        {
            if (shouldBait)
            {
                if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(ref b).Count != 0))
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, ref b)))
                    hints.Add("GTFO from baited aoe!");
            }
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(actor))
            movementHints.Add(actor.Position, p, Colors.Safe);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_caster != null)
            hints.Add($"Tankbuster {(_far ? "far" : "near")}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pc))
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        bool? far = spell.Action.ID switch
        {
            (uint)AID.HallowedWingsLN or (uint)AID.HallowedWingsRN => false,
            (uint)AID.HallowedWingsLF or (uint)AID.HallowedWingsRF => true,
            _ => null
        };
        if (far != null)
        {
            _caster = caster;
            _far = far.Value;
        }
    }

    protected abstract IEnumerable<WPos> SafeSpots(Actor actor);
}

sealed class P6HallowedPlume1(BossModule module) : P6HallowedPlume(module)
{
    private readonly P6CauterizeN? _cauterize = module.FindComponent<P6CauterizeN>();

    protected override WPos[] SafeSpots(Actor actor)
    {
        if (_wings?.AOE.Length == 0 || _cauterize?.AOE.Length == 0)
        {
            return [];
        }

        var safeSpotCenter = Arena.Center;
        var safeSpotCenterX = safeSpotCenter.X;
        var safeSpotCenterZ = safeSpotCenter.Z;
        ref var aoe1 = ref _wings!.AOE[0];
        ref var aoe2 = ref _cauterize!.AOE[0];
        safeSpotCenterZ -= aoe1.Origin.Z - safeSpotCenterZ;
        safeSpotCenterX -= aoe2.Origin.X - safeSpotCenterX;
        safeSpotCenter = new(safeSpotCenterX, safeSpotCenterZ);

        var shouldBait = actor.Role == Role.Tank;
        var stayFar = shouldBait == _far;
        var xOffset = stayFar ? -9f : 9f; // assume hraesvelgr is always at +22
        if (shouldBait)
        {
            // TODO: configurable tank assignments (e.g. MT always center/out/N/S)
            return [safeSpotCenter + new WDir(xOffset, 9f), safeSpotCenter + new WDir(xOffset, -9f)];
        }
        else
        {
            return [safeSpotCenter + new WDir(xOffset, default)];
        }
    }
}

sealed class P6HallowedPlume2(BossModule module) : P6HallowedPlume(module)
{
    private readonly P6HotWingTail? _wingTail = module.FindComponent<P6HotWingTail>();

    protected override WPos[] SafeSpots(Actor actor)
    {
        if (_wings?.AOE.Length == 0 || _wingTail == null)
            return [];

        var zCoeff = _wingTail.NumAOEs switch
        {
            1 => 15.75f / 11f,
            2 => 4.0f / 11f,
            _ => 1f
        };
        var safeSpotCenter = Arena.Center;
        var safeSpotCenterZ = safeSpotCenter.Z;
        ref var aoe = ref _wings!.AOE[0];
        safeSpotCenterZ -= zCoeff * (aoe.Origin.Z - safeSpotCenterZ);
        safeSpotCenter = new(safeSpotCenter.X, safeSpotCenterZ);

        var shouldBait = actor.Role == Role.Tank;
        var stayFar = shouldBait == _far;
        var xOffset = stayFar ? -20f : 20f; // assume hraesvelgr is always at +22
        if (shouldBait)
        {
            // TODO: configurable tank assignments (e.g. MT always center/border/near/far)
            return [safeSpotCenter, safeSpotCenter + new WDir(xOffset, default)];
        }
        else
        {
            return [safeSpotCenter + new WDir(xOffset, default)];
        }
    }
}
