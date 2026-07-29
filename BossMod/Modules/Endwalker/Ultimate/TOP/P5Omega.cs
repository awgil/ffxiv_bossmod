namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P5OmegaDoubleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape[] Shapes = [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f), new AOEShapeRect(40f, 40f, -4f), new AOEShapeCross(100f, 5f)];
    public readonly List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        int startIndex = 0, endIndex = count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        ref var aoe0 = ref aoes[0];
        var midpoint = aoe0.Activation.AddSeconds(2d);

        if (NumCasts == 0)
        {
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Activation > midpoint)
                {
                    endIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Activation > midpoint)
                {
                    startIndex = i;
                    break;
                }
            }
        }
        return aoes[startIndex..endIndex];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BeyondStrength or (uint)AID.EfficientBladework or (uint)AID.SuperliminalSteel or (uint)AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E43)
            return;
        void AddAOE(AOEShape shape, Angle offset = default) => AOEs.Add(new(shape, actor.Position.Quantized(), actor.Rotation + offset, WorldState.FutureTime(13.2d)));
        switch (actor.OID)
        {
            case (uint)OID.OmegaMP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AddAOE(Shapes[0]);
                }
                else
                {
                    AddAOE(Shapes[1]);
                }
                break;
            case (uint)OID.OmegaFP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AddAOE(Shapes[2], 90f.Degrees());
                    AddAOE(Shapes[2], -90f.Degrees());
                }
                else
                {
                    AddAOE(Shapes[3]);
                }
                break;
        }
    }
}

sealed class P5OmegaDiffuseWaveCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OmegaDiffuseWaveCannonFront or (uint)AID.OmegaDiffuseWaveCannonSides)
        {
            var first = spell.Rotation + (spell.Action.ID == (uint)AID.OmegaDiffuseWaveCannonFront ? default : 90f).Degrees();
            var pos = spell.LocXZ;
            var act1st = Module.CastFinishAt(spell, 1.1d);
            var act2nd = Module.CastFinishAt(spell, 5.2d);
            _aoes.Add(new(_shape, pos, first, act1st));
            _aoes.Add(new(_shape, pos, first + 180f.Degrees(), act1st));
            _aoes.Add(new(_shape, pos, first + 90f.Degrees(), act2nd));
            _aoes.Add(new(_shape, pos, first - 90f.Degrees(), act2nd));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OmegaDiffuseWaveCannonAOE)
        {
            ++NumCasts;
            var count = _aoes.RemoveAll(aoe => aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (count != 1)
                ReportError($"Unexpected removed count: {count}");
        }
    }
}

sealed class P5OmegaNearDistantWorld(BossModule module) : P5NearDistantWorld(module)
{
    private BitMask _near;
    private BitMask _distant;
    private BitMask _first;
    private BitMask _second;
    private DateTime _firstActivation;
    private DateTime _secondActivation;

    public bool HaveDebuffs => (_near | _distant | _first | _second).Any();

    public void ShowFirst() => Reset(Raid[(_near & _first).LowestSetBit()], Raid[(_distant & _first).LowestSetBit()], _firstActivation);
    public void ShowSecond() => Reset(Raid[(_near & _second).LowestSetBit()], Raid[(_distant & _second).LowestSetBit()], _secondActivation);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.HelloNearWorld:
                _near.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.HelloDistantWorld:
                _distant.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.InLine1:
                _first.Set(Raid.FindSlot(actor.InstanceID));
                _firstActivation = status.ExpireAt;
                break;
            case (uint)SID.InLine2:
                _second.Set(Raid.FindSlot(actor.InstanceID));
                _secondActivation = status.ExpireAt;
                break;
        }
    }
}

// TODO: assign soakers
sealed class P5OmegaOversampledWaveCannon(BossModule module) : Components.UniformStackSpread(module, default, 7f)
{
    private readonly P5OmegaNearDistantWorld? _ndw = module.FindComponent<P5OmegaNearDistantWorld>();
    private Actor? _boss;
    private Angle _bossAngle;

    private static readonly AOEShapeRect _shape = new(50f, 50f);

    public bool IsActive => _boss != null;

    public override void Update()
    {
        Spreads.Clear();
        if (_boss != null)
            AddSpreads(Raid.WithoutSlot(false, true, true).InShape(_shape, _boss.Position, _boss.Rotation + _bossAngle));
        base.Update();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boss != null)
            _shape.Draw(Arena, _boss.Position, _boss.Rotation + _bossAngle);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var angle = spell.Action.ID switch
        {
            (uint)AID.DeltaOversampledWaveCannonL => 90f.Degrees(),
            (uint)AID.DeltaOversampledWaveCannonR => -90f.Degrees(),
            _ => default
        };
        if (angle == default)
            return;
        _boss = caster;
        _bossAngle = angle;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OversampledWaveCannonAOE)
        {
            Spreads.Clear();
            _boss = null;
        }
    }

    private List<WPos> SafeSpots(int slot, Actor actor)
    {
        if (_ndw == null || _boss == null)
            return [];

        var center = Arena.Center;
        var rot = _boss.Rotation;
        if (actor == _ndw.NearWorld)
        {
            return [center + 10f * (rot - _bossAngle).ToDirection()];
        }
        else if (actor == _ndw.DistantWorld)
        {
            return [center + 10f * (rot + 2.05f * _bossAngle).ToDirection()];
        }
        else
        {
            return
            [
                // TODO: assignments...
                center + 19f * (rot - 0.05f * _bossAngle).ToDirection(), // '1' - first distant
                center + 19f * (rot - 0.95f * _bossAngle).ToDirection(), // '2' - first near
                center + 19f * (rot - 1.05f * _bossAngle).ToDirection(), // '3' - second near
                center + 19f * (rot - 1.95f * _bossAngle).ToDirection(), // '4' - second distant
                center + 15f * (rot + 0.50f * _bossAngle).ToDirection(), // first soaker
                center + 15f * (rot + 1.50f * _bossAngle).ToDirection(), // second soaker
            ];
        }
    }
}

// TODO: assign soakers
sealed class P5OmegaBlaster : Components.BaitAwayTethers
{
    private readonly P5OmegaNearDistantWorld? _ndw;

    public P5OmegaBlaster(BossModule module) : base(module, 15f, (uint)TetherID.Blaster, (uint)AID.OmegaBlasterAOE)
    {
        ForbiddenPlayers = new(0xFF);
        _ndw = module.FindComponent<P5OmegaNearDistantWorld>();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pc))
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.QuickeningDynamis && status.Extra >= 3)
            ForbiddenPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    private List<WPos> SafeSpots(Actor actor)
    {
        if (_ndw == null || CurrentBaits.Count == 0)
            return [];

        var center = Arena.Center;
        var toBoss = (CurrentBaits.Ref(0).Source.Position - center).Normalized();
        var toBossOrthoL = toBoss.OrthoL();
        var toBossOrthoR = toBoss.OrthoR();
        if (actor == _ndw.NearWorld)
        {
            return [center - 10f * toBoss];
        }
        else if (actor == _ndw.DistantWorld)
        {
            // TODO: select one of the spots...
            return [center + 10f * toBossOrthoL, center + 10f * toBossOrthoR];
        }
        else if (IsBaitTarget(actor))
        {
            var p = Arena.Center + 16f * toBoss;
            return [p + 10f * toBossOrthoL, p + 10f * toBossOrthoR];
        }
        else
        {
            // TODO: assignments...
            return
            [
                center + 19f * toBossOrthoL, // '1' - first distant
                center - 18f * toBoss + 5f * toBossOrthoL, // '2' - first near
                center - 18f * toBoss + 5f * toBossOrthoR, // '3' - second near
                center + 19f * toBossOrthoR // '4' - second distant
            ];
        }
    }
}
