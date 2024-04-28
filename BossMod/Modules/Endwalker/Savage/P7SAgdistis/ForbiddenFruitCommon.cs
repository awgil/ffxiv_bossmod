namespace BossMod.Endwalker.Savage.P7SAgdistis;

// common parts of various forbidden fruit / harvest mechanics
// platform id's: 0 = W, 1 = S, 2 = E
// TODO: show knockback for bird tethers, something for bull/minotaur tethers...
class ForbiddenFruitCommon(BossModule module, ActionID watchedAction) : Components.GenericAOEs(module, watchedAction)
{
    public int NumAssignedTethers { get; private set; }
    public bool MinotaursBaited { get; private set; }
    protected Actor?[] TetherSources = new Actor?[8];
    protected BitMask[] SafePlatforms = new BitMask[8];
    private readonly List<(Actor, AOEShape, DateTime)> _predictedAOEs = [];
    private readonly List<(Actor, AOEShape)> _activeAOEs = [];
    private BitMatrix _tetherClips; // [i,j] is set if i is tethered and clips j

    protected static readonly BitMask ValidPlatformsMask = new(7);
    protected static readonly AOEShapeCircle ShapeBullUntethered = new(10);
    protected static readonly AOEShapeRect ShapeBirdUntethered = new(60, 4);
    protected static readonly AOEShapeRect ShapeBullBirdTethered = new(60, 4);
    protected static readonly AOEShapeCone ShapeMinotaurUntethered = new(60, 45.Degrees());
    protected static readonly AOEShapeCone ShapeMinotaurTethered = new(60, 22.5f.Degrees());

    public bool CastsActive => _activeAOEs.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (source, shape, time) in _predictedAOEs)
            yield return new(shape, source.Position, source.Rotation, time);
        foreach (var (source, shape) in _activeAOEs)
            yield return new(shape, source.Position, source.CastInfo!.Rotation, source.CastInfo.NPCFinishAt);
    }

    public override void Update()
    {
        _tetherClips.Reset();
        foreach (var (slot, player) in Raid.WithSlot())
        {
            var tetherSource = TetherSources[slot];
            if (tetherSource != null)
            {
                AOEShape tetherShape = (OID)tetherSource.OID == OID.ImmatureMinotaur ? ShapeMinotaurTethered : ShapeBullBirdTethered;
                _tetherClips[slot] = Raid.WithSlot().Exclude(player).InShape(tetherShape, tetherSource.Position, Angle.FromDirection(player.Position - tetherSource.Position)).Mask();
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_tetherClips[slot].Any())
            hints.Add("Hitting others with tether!");
        if (_tetherClips.AnyBitInColumn(slot))
            hints.Add("Clipped by other tethers!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _tetherClips[playerSlot, pcSlot] || _tetherClips[pcSlot, playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tetherSource = TetherSources[pcSlot];
        if (tetherSource != null)
            Arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

        foreach (var platform in SafePlatforms[pcSlot].SetBits())
            Arena.AddCircle(Module.Center + PlatformDirection(platform).ToDirection() * Border.SmallPlatformOffset, Border.SmallPlatformRadius, ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        TryAssignTether(source, tether);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StaticMoon:
                _predictedAOEs.Clear();
                _activeAOEs.Add((caster, ShapeBullUntethered));
                break;
            case AID.StymphalianStrike:
                _predictedAOEs.Clear();
                _activeAOEs.Add((caster, ShapeBirdUntethered));
                break;
            case AID.BullishSwipeAOE:
                MinotaursBaited = true;
                _activeAOEs.Add((caster, ShapeMinotaurUntethered));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StaticMoon or AID.StymphalianStrike or AID.BullishSwipeAOE)
            _activeAOEs.RemoveAll(i => i.Item1 == caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1)
        {
            var castStart = PredictUntetheredCastStart(actor);
            if (castStart != null)
            {
                AOEShape? shape = (OID)actor.OID switch
                {
                    OID.ForbiddenFruitBull => ShapeBullUntethered,
                    OID.ForbiddenFruitBird => ShapeBirdUntethered,
                    _ => null
                };
                if (shape != null)
                {
                    _predictedAOEs.Add((actor, shape, castStart.Value.AddSeconds(3)));
                }
            }
        }
    }

    // subclass can override and return non-null if specified fruit will become of untethered variety
    protected virtual DateTime? PredictUntetheredCastStart(Actor fruit) => null;

    // this is called by default OnTethered, but subclasses might want to call it themselves and use returned info (target slot if tether was assigned)
    protected int TryAssignTether(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Bull or TetherID.MinotaurClose or TetherID.MinotaurFar or TetherID.Bird)
        {
            int slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                TetherSources[slot] = source;
                ++NumAssignedTethers;
                return slot;
            }
        }
        return -1;
    }

    protected uint TetherColor(Actor source) => (OID)source.OID switch
    {
        OID.ImmatureMinotaur => 0xffff00ff,
        OID.BullTetherSource => 0xffffff00,
        OID.ImmatureStymphalide => 0xff00ffff,
        _ => 0
    };

    protected int PlatformIDFromOffset(WDir offset) => offset.Z > 0 ? 1 : offset.X > 0 ? 2 : 0;
    protected Angle PlatformDirection(int id) => (id - 1) * 120.Degrees();
}
