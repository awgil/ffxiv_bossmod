namespace BossMod.Endwalker.Ultimate.TOP;

class P5OmegaDoubleAOEs : Components.GenericAOEs
{
    public List<AOEInstance> AOEs = new();

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        var midpoint = AOEs.FirstOrDefault().Activation.AddSeconds(2);
        return NumCasts == 0 ? AOEs.TakeWhile(aoe => aoe.Activation <= midpoint) : AOEs.SkipWhile(aoe => aoe.Activation <= midpoint);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BeyondStrength or AID.EfficientBladework or AID.SuperliminalSteel or AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if (id != 0x1E43)
            return;
        switch ((OID)actor.OID)
        {
            case OID.OmegaMP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(new AOEShapeDonut(10, 40), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(13.2f)));
                }
                else
                {
                    AOEs.Add(new(new AOEShapeCircle(10), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(13.2f)));
                }
                break;
            case OID.OmegaFP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation + 90.Degrees(), module.WorldState.CurrentTime.AddSeconds(13.2f)));
                    AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation - 90.Degrees(), module.WorldState.CurrentTime.AddSeconds(13.2f)));
                }
                else
                {
                    AOEs.Add(new(new AOEShapeCross(100, 5), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(13.2f)));
                }
                break;
        }
    }
}

class P5OmegaDiffuseWaveCannon : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeCone _shape = new(100, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OmegaDiffuseWaveCannonFront or AID.OmegaDiffuseWaveCannonSides)
        {
            var first = spell.Rotation + ((AID)spell.Action.ID == AID.OmegaDiffuseWaveCannonFront ? 0 : 90).Degrees();
            _aoes.Add(new(_shape, caster.Position, first, spell.NPCFinishAt.AddSeconds(1.1f)));
            _aoes.Add(new(_shape, caster.Position, first + 180.Degrees(), spell.NPCFinishAt.AddSeconds(1.1f)));
            _aoes.Add(new(_shape, caster.Position, first + 90.Degrees(), spell.NPCFinishAt.AddSeconds(5.2f)));
            _aoes.Add(new(_shape, caster.Position, first - 90.Degrees(), spell.NPCFinishAt.AddSeconds(5.2f)));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OmegaDiffuseWaveCannonAOE)
        {
            ++NumCasts;
            var count = _aoes.RemoveAll(aoe => aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (count != 1)
                module.ReportError(this, $"Unexpected removed count: {count}");
        }
    }
}

class P5OmegaNearDistantWorld : P5NearDistantWorld
{
    private BitMask _near;
    private BitMask _distant;
    private BitMask _first;
    private BitMask _second;
    private DateTime _firstActivation;
    private DateTime _secondActivation;

    public bool HaveDebuffs => (_near | _distant | _first | _second).Any();

    public void ShowFirst(BossModule module) => Reset(module.Raid[(_near & _first).LowestSetBit()], module.Raid[(_distant & _first).LowestSetBit()], _firstActivation);
    public void ShowSecond(BossModule module) => Reset(module.Raid[(_near & _second).LowestSetBit()], module.Raid[(_distant & _second).LowestSetBit()], _secondActivation);

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HelloNearWorld:
                _near.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.HelloDistantWorld:
                _distant.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.InLine1:
                _first.Set(module.Raid.FindSlot(actor.InstanceID));
                _firstActivation = status.ExpireAt;
                break;
            case SID.InLine2:
                _second.Set(module.Raid.FindSlot(actor.InstanceID));
                _secondActivation = status.ExpireAt;
                break;
        }
    }
}

// TODO: assign soakers
class P5OmegaOversampledWaveCannon : Components.UniformStackSpread
{
    private P5OmegaNearDistantWorld? _ndw;
    private Actor? _boss;
    private Angle _bossAngle;

    private static readonly AOEShapeRect _shape = new(50, 50);

    public bool IsActive => _boss != null;

    public P5OmegaOversampledWaveCannon() : base(0, 7) { }

    public override void Init(BossModule module) => _ndw = module.FindComponent<P5OmegaNearDistantWorld>();

    public override void Update(BossModule module)
    {
        Spreads.Clear();
        if (_boss != null)
            AddSpreads(module.Raid.WithoutSlot().InShape(_shape, _boss.Position, _boss.Rotation + _bossAngle));
        base.Update(module);
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_boss != null)
            _shape.Draw(arena, _boss.Position, _boss.Rotation + _bossAngle, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var p in SafeSpots(module, pcSlot, pc))
            arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var angle = (AID)spell.Action.ID switch
        {
            AID.DeltaOversampledWaveCannonL => 90.Degrees(),
            AID.DeltaOversampledWaveCannonR => -90.Degrees(),
            _ => default
        };
        if (angle == default)
            return;
        _boss = caster;
        _bossAngle = angle;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OversampledWaveCannonAOE)
        {
            Spreads.Clear();
            _boss = null;
        }
    }

    private IEnumerable<WPos> SafeSpots(BossModule module, int slot, Actor actor)
    {
        if (_ndw == null || _boss == null)
            yield break;

        if (actor == _ndw.NearWorld)
        {
            yield return module.Bounds.Center + 10 * (_boss.Rotation - _bossAngle).ToDirection();
        }
        else if (actor == _ndw.DistantWorld)
        {
            yield return module.Bounds.Center + 10 * (_boss.Rotation + 2.05f * _bossAngle).ToDirection();
        }
        else
        {
            // TODO: assignments...
            yield return module.Bounds.Center + 19 * (_boss.Rotation - 0.05f * _bossAngle).ToDirection(); // '1' - first distant
            yield return module.Bounds.Center + 19 * (_boss.Rotation - 0.95f * _bossAngle).ToDirection(); // '2' - first near
            yield return module.Bounds.Center + 19 * (_boss.Rotation - 1.05f * _bossAngle).ToDirection(); // '3' - second near
            yield return module.Bounds.Center + 19 * (_boss.Rotation - 1.95f * _bossAngle).ToDirection(); // '4' - second distant
            yield return module.Bounds.Center + 15 * (_boss.Rotation + 0.50f * _bossAngle).ToDirection(); // first soaker
            yield return module.Bounds.Center + 15 * (_boss.Rotation + 1.50f * _bossAngle).ToDirection(); // second soaker
        }
    }
}

// TODO: assign soakers
class P5OmegaBlaster : Components.BaitAwayTethers
{
    private P5OmegaNearDistantWorld? _ndw;

    public P5OmegaBlaster() : base(new AOEShapeCircle(15), (uint)TetherID.Blaster, ActionID.MakeSpell(AID.OmegaBlasterAOE)) { }

    public override void Init(BossModule module)
    {
        CenterAtTarget = true;
        ForbiddenPlayers = new(0xFF);
        _ndw = module.FindComponent<P5OmegaNearDistantWorld>();
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var p in SafeSpots(module, pcSlot, pc))
            arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.QuickeningDynamis && status.Extra >= 3)
            ForbiddenPlayers.Clear(module.Raid.FindSlot(actor.InstanceID));
    }

    private IEnumerable<WPos> SafeSpots(BossModule module, int slot, Actor actor)
    {
        if (_ndw == null || CurrentBaits.Count == 0)
            yield break;

        var toBoss = (CurrentBaits.First().Source.Position - module.Bounds.Center).Normalized();
        if (actor == _ndw.NearWorld)
        {
            yield return module.Bounds.Center - 10 * toBoss;
        }
        else if (actor == _ndw.DistantWorld)
        {
            // TODO: select one of the spots...
            yield return module.Bounds.Center + 10 * toBoss.OrthoL();
            yield return module.Bounds.Center + 10 * toBoss.OrthoR();
        }
        else if (CurrentBaits.Any(b => b.Target == actor))
        {
            var p = module.Bounds.Center + 16 * toBoss;
            yield return p + 10 * toBoss.OrthoL();
            yield return p + 10 * toBoss.OrthoR();
        }
        else
        {
            // TODO: assignments...
            yield return module.Bounds.Center + 19 * toBoss.OrthoL(); // '1' - first distant
            yield return module.Bounds.Center - 18 * toBoss + 5 * toBoss.OrthoL(); // '2' - first near
            yield return module.Bounds.Center - 18 * toBoss + 5 * toBoss.OrthoR(); // '3' - second near
            yield return module.Bounds.Center + 19 * toBoss.OrthoR(); // '4' - second distant
        }
    }
}
