namespace BossMod.Endwalker.Savage.P7SAgdistis;

// common parts of various forbidden fruit / harvest mechanics
// platform id's: 0 = W, 1 = S, 2 = E
// TODO: show knockback for bird tethers, something for bull/minotaur tethers...
class ForbiddenFruitCommon(BossModule module, uint watchedAction) : Components.GenericAOEs(module, watchedAction)
{
    public int NumAssignedTethers;
    public bool MinotaursBaited;
    protected Actor?[] TetherSources = new Actor?[8];
    protected BitMask[] SafePlatforms = new BitMask[8];
    private readonly List<(Actor, AOEShape, DateTime)> _predictedAOEs = [];
    private readonly List<(Actor, AOEShape)> _activeAOEs = [];
    private BitMatrix _tetherClips; // [i,j] is set if i is tethered and clips j

    protected static readonly BitMask ValidPlatformsMask = new(7);
    protected static readonly AOEShapeCircle ShapeBullUntethered = new(10f);
    protected static readonly AOEShapeRect ShapeRect = new(60f, 4f);
    protected static readonly AOEShapeCone ShapeMinotaurUntethered = new(60f, 45f.Degrees());
    protected static readonly AOEShapeCone ShapeMinotaurTethered = new(60f, 22.5f.Degrees());

    public bool CastsActive => _activeAOEs.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countP = _predictedAOEs.Count;
        var countA = _activeAOEs.Count;
        var aoes = new AOEInstance[countP + countA];
        var index = 0;
        for (var i = 0; i < countP; ++i)
        {
            var p = _predictedAOEs[i];
            aoes[index++] = new(p.Item2, p.Item1.Position, p.Item1.Rotation, p.Item3);
        }
        for (var i = 0; i < countA; ++i)
        {
            var p = _activeAOEs[i];
            aoes[index++] = new(p.Item2, p.Item1.CastInfo!.LocXZ, p.Item1.CastInfo!.Rotation, Module.CastFinishAt(p.Item1.CastInfo));
        }
        return aoes;
    }

    public override void Update()
    {
        _tetherClips = default;
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            var tetherSource = TetherSources[p.Item1];
            if (tetherSource != null)
            {
                AOEShape tetherShape = tetherSource.OID == (uint)OID.ImmatureMinotaur ? ShapeMinotaurTethered : ShapeRect;
                _tetherClips[p.Item1] = party.Exclude(p.Item2).InShape(tetherShape, tetherSource.Position, Angle.FromDirection(p.Item2.Position - tetherSource.Position)).Mask();
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
            Arena.ZoneCircleOutline(Arena.Center + PlatformDirection(platform).ToDirection() * 16.5f, 10f, Colors.Safe);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        TryAssignTether(source, tether);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.StaticMoon:
                _predictedAOEs.Clear();
                _activeAOEs.Add((caster, ShapeBullUntethered));
                break;
            case (uint)AID.StymphalianStrike:
                _predictedAOEs.Clear();
                _activeAOEs.Add((caster, ShapeRect));
                break;
            case (uint)AID.BullishSwipeAOE:
                MinotaursBaited = true;
                _activeAOEs.Add((caster, ShapeMinotaurUntethered));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.StaticMoon or (uint)AID.StymphalianStrike or (uint)AID.BullishSwipeAOE)
            _activeAOEs.RemoveAll(i => i.Item1 == caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1)
        {
            var castStart = PredictUntetheredCastStart(actor);
            if (castStart != null)
            {
                AOEShape? shape = actor.OID switch
                {
                    (uint)OID.ForbiddenFruitBull => ShapeBullUntethered,
                    (uint)OID.ForbiddenFruitBird => ShapeRect,
                    _ => null
                };
                if (shape != null)
                {
                    _predictedAOEs.Add((actor, shape, castStart.Value.AddSeconds(3d)));
                }
            }
        }
    }

    // subclass can override and return non-null if specified fruit will become of untethered variety
    protected virtual DateTime? PredictUntetheredCastStart(Actor fruit) => null;

    // this is called by default OnTethered, but subclasses might want to call it themselves and use returned info (target slot if tether was assigned)
    protected int TryAssignTether(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.Bull or (uint)TetherID.MinotaurClose or (uint)TetherID.MinotaurFar or (uint)TetherID.Bird)
        {
            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                TetherSources[slot] = source;
                ++NumAssignedTethers;
                return slot;
            }
        }
        return -1;
    }

    protected static uint TetherColor(Actor source) => source.OID switch
    {
        (uint)OID.ImmatureMinotaur => Colors.Vulnerable,
        (uint)OID.BullTetherSource => Colors.Other7,
        (uint)OID.ImmatureStymphalide => Colors.Danger,
        _ => 0
    };

    protected static int PlatformIDFromOffset(WDir offset) => offset.Z > 0 ? 1 : offset.X > 0 ? 2 : 0;
    protected static Angle PlatformDirection(int id) => (id - 1) * 120f.Degrees();
}
