namespace BossMod.Endwalker.Ultimate.DSW2;

class P6HallowedWings(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE; // origin is always (122, 100 +- 11), direction -90

    private static readonly AOEShapeRect _shape = new(50, 11);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.HallowedWingsLN or AID.HallowedWingsLF => _shape.HalfWidth,
            AID.HallowedWingsRN or AID.HallowedWingsRF => -_shape.HalfWidth,
            _ => 0
        };
        if (offset == 0)
            return;
        var origin = caster.Position + offset * spell.Rotation.ToDirection().OrthoL();
        AOE = new(_shape, origin, spell.Rotation, spell.NPCFinishAt.AddSeconds(0.8f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HallowedWingsAOELeft or AID.HallowedWingsAOERight or AID.CauterizeN)
            ++NumCasts;
    }
}

// note: we want to show hint much earlier than cast start - we assume component is created right as hallowed wings starts, meaning nidhogg is already in place
class P6CauterizeN : Components.GenericAOEs
{
    public AOEInstance? AOE; // origin is always (100 +- 11, 100 +- 34), direction 0/180

    private static readonly AOEShapeRect _shape = new(80, 11);

    public P6CauterizeN(BossModule module) : base(module, ActionID.MakeSpell(AID.CauterizeN))
    {
        var caster = module.Enemies(OID.NidhoggP6).FirstOrDefault();
        if (caster != null)
            AOE = new(_shape, caster.Position, caster.Rotation, WorldState.FutureTime(8.6f));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);
}

abstract class P6HallowedPlume(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HallowedPlume), centerAtTarget: true)
{
    protected P6HallowedWings? _wings = module.FindComponent<P6HallowedWings>();
    protected bool _far;
    private Actor? _caster;

    private static readonly AOEShapeCircle _shape = new(10);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_caster != null)
        {
            var players = Raid.WithoutSlot().SortedByRange(_caster.Position);
            var targets = _far ? players.TakeLast(2) : players.Take(2);
            foreach (var t in targets)
                CurrentBaits.Add(new(_caster, t, _shape));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool shouldBait = actor.Role == Role.Tank;
        bool isBaiting = ActiveBaitsOn(actor).Any();
        bool stayFar = shouldBait == _far;
        hints.Add(stayFar ? "Stay far!" : "Stay close!", shouldBait != isBaiting);

        if (shouldBait == isBaiting)
        {
            if (shouldBait)
            {
                if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
                    hints.Add("GTFO from baited aoe!");
            }
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(actor))
            movementHints.Add(actor.Position, p, ArenaColor.Safe);
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
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        bool? far = (AID)spell.Action.ID switch
        {
            AID.HallowedWingsLN or AID.HallowedWingsRN => false,
            AID.HallowedWingsLF or AID.HallowedWingsRF => true,
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

class P6HallowedPlume1(BossModule module) : P6HallowedPlume(module)
{
    private readonly P6CauterizeN? _cauterize = module.FindComponent<P6CauterizeN>();

    protected override IEnumerable<WPos> SafeSpots(Actor actor)
    {
        if (_wings?.AOE == null || _cauterize?.AOE == null)
            yield break;

        var safeSpotCenter = Module.Center;
        safeSpotCenter.Z -= _wings.AOE.Value.Origin.Z - Module.Center.Z;
        safeSpotCenter.X -= _cauterize.AOE.Value.Origin.X - Module.Center.X;

        bool shouldBait = actor.Role == Role.Tank;
        bool stayFar = shouldBait == _far;
        float xOffset = stayFar ? -9 : +9; // assume hraesvelgr is always at +22
        if (shouldBait)
        {
            // TODO: configurable tank assignments (e.g. MT always center/out/N/S)
            yield return safeSpotCenter + new WDir(xOffset, 9);
            yield return safeSpotCenter + new WDir(xOffset, -9);
        }
        else
        {
            yield return safeSpotCenter + new WDir(xOffset, 0);
        }
    }
}

class P6HallowedPlume2(BossModule module) : P6HallowedPlume(module)
{
    private readonly P6HotWingTail? _wingTail = module.FindComponent<P6HotWingTail>();

    protected override IEnumerable<WPos> SafeSpots(Actor actor)
    {
        if (_wings?.AOE == null || _wingTail == null)
            yield break;

        float zCoeff = _wingTail.NumAOEs switch
        {
            1 => 15.75f / 11,
            2 => 4.0f / 11,
            _ => 1
        };
        var safeSpotCenter = Module.Center;
        safeSpotCenter.Z -= zCoeff * (_wings.AOE.Value.Origin.Z - Module.Center.Z);

        bool shouldBait = actor.Role == Role.Tank;
        bool stayFar = shouldBait == _far;
        float xOffset = stayFar ? -20 : +20; // assume hraesvelgr is always at +22
        if (shouldBait)
        {
            // TODO: configurable tank assignments (e.g. MT always center/border/near/far)
            yield return safeSpotCenter;
            yield return safeSpotCenter + new WDir(xOffset, 0);
        }
        else
        {
            yield return safeSpotCenter + new WDir(xOffset, 0);
        }
    }
}
