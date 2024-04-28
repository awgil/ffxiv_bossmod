namespace BossMod.Endwalker.Ultimate.TOP;

class P5OmegaDoubleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var midpoint = AOEs.FirstOrDefault().Activation.AddSeconds(2);
        return NumCasts == 0 ? AOEs.TakeWhile(aoe => aoe.Activation <= midpoint) : AOEs.SkipWhile(aoe => aoe.Activation <= midpoint);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BeyondStrength or AID.EfficientBladework or AID.SuperliminalSteel or AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E43)
            return;
        switch ((OID)actor.OID)
        {
            case OID.OmegaMP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(new AOEShapeDonut(10, 40), actor.Position, actor.Rotation, WorldState.FutureTime(13.2f)));
                }
                else
                {
                    AOEs.Add(new(new AOEShapeCircle(10), actor.Position, actor.Rotation, WorldState.FutureTime(13.2f)));
                }
                break;
            case OID.OmegaFP5:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation + 90.Degrees(), WorldState.FutureTime(13.2f)));
                    AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation - 90.Degrees(), WorldState.FutureTime(13.2f)));
                }
                else
                {
                    AOEs.Add(new(new AOEShapeCross(100, 5), actor.Position, actor.Rotation, WorldState.FutureTime(13.2f)));
                }
                break;
        }
    }
}

class P5OmegaDiffuseWaveCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OmegaDiffuseWaveCannonAOE)
        {
            ++NumCasts;
            var count = _aoes.RemoveAll(aoe => aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (count != 1)
                ReportError($"Unexpected removed count: {count}");
        }
    }
}

class P5OmegaNearDistantWorld(BossModule module) : P5NearDistantWorld(module)
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

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HelloNearWorld:
                _near.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.HelloDistantWorld:
                _distant.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.InLine1:
                _first.Set(Raid.FindSlot(actor.InstanceID));
                _firstActivation = status.ExpireAt;
                break;
            case SID.InLine2:
                _second.Set(Raid.FindSlot(actor.InstanceID));
                _secondActivation = status.ExpireAt;
                break;
        }
    }
}

// TODO: assign soakers
class P5OmegaOversampledWaveCannon(BossModule module) : Components.UniformStackSpread(module, 0, 7)
{
    private readonly P5OmegaNearDistantWorld? _ndw = module.FindComponent<P5OmegaNearDistantWorld>();
    private Actor? _boss;
    private Angle _bossAngle;

    private static readonly AOEShapeRect _shape = new(50, 50);

    public bool IsActive => _boss != null;

    public override void Update()
    {
        Spreads.Clear();
        if (_boss != null)
            AddSpreads(Raid.WithoutSlot().InShape(_shape, _boss.Position, _boss.Rotation + _bossAngle));
        base.Update();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boss != null)
            _shape.Draw(Arena, _boss.Position, _boss.Rotation + _bossAngle, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OversampledWaveCannonAOE)
        {
            Spreads.Clear();
            _boss = null;
        }
    }

    private IEnumerable<WPos> SafeSpots(int slot, Actor actor)
    {
        if (_ndw == null || _boss == null)
            yield break;

        if (actor == _ndw.NearWorld)
        {
            yield return Module.Center + 10 * (_boss.Rotation - _bossAngle).ToDirection();
        }
        else if (actor == _ndw.DistantWorld)
        {
            yield return Module.Center + 10 * (_boss.Rotation + 2.05f * _bossAngle).ToDirection();
        }
        else
        {
            // TODO: assignments...
            yield return Module.Center + 19 * (_boss.Rotation - 0.05f * _bossAngle).ToDirection(); // '1' - first distant
            yield return Module.Center + 19 * (_boss.Rotation - 0.95f * _bossAngle).ToDirection(); // '2' - first near
            yield return Module.Center + 19 * (_boss.Rotation - 1.05f * _bossAngle).ToDirection(); // '3' - second near
            yield return Module.Center + 19 * (_boss.Rotation - 1.95f * _bossAngle).ToDirection(); // '4' - second distant
            yield return Module.Center + 15 * (_boss.Rotation + 0.50f * _bossAngle).ToDirection(); // first soaker
            yield return Module.Center + 15 * (_boss.Rotation + 1.50f * _bossAngle).ToDirection(); // second soaker
        }
    }
}

// TODO: assign soakers
class P5OmegaBlaster : Components.BaitAwayTethers
{
    private readonly P5OmegaNearDistantWorld? _ndw;

    public P5OmegaBlaster(BossModule module) : base(module, new AOEShapeCircle(15), (uint)TetherID.Blaster, ActionID.MakeSpell(AID.OmegaBlasterAOE))
    {
        CenterAtTarget = true;
        ForbiddenPlayers = new(0xFF);
        _ndw = module.FindComponent<P5OmegaNearDistantWorld>();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.QuickeningDynamis && status.Extra >= 3)
            ForbiddenPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    private IEnumerable<WPos> SafeSpots(int slot, Actor actor)
    {
        if (_ndw == null || CurrentBaits.Count == 0)
            yield break;

        var toBoss = (CurrentBaits[0].Source.Position - Module.Center).Normalized();
        if (actor == _ndw.NearWorld)
        {
            yield return Module.Center - 10 * toBoss;
        }
        else if (actor == _ndw.DistantWorld)
        {
            // TODO: select one of the spots...
            yield return Module.Center + 10 * toBoss.OrthoL();
            yield return Module.Center + 10 * toBoss.OrthoR();
        }
        else if (CurrentBaits.Any(b => b.Target == actor))
        {
            var p = Module.Center + 16 * toBoss;
            yield return p + 10 * toBoss.OrthoL();
            yield return p + 10 * toBoss.OrthoR();
        }
        else
        {
            // TODO: assignments...
            yield return Module.Center + 19 * toBoss.OrthoL(); // '1' - first distant
            yield return Module.Center - 18 * toBoss + 5 * toBoss.OrthoL(); // '2' - first near
            yield return Module.Center - 18 * toBoss + 5 * toBoss.OrthoR(); // '3' - second near
            yield return Module.Center + 19 * toBoss.OrthoR(); // '4' - second distant
        }
    }
}
