namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class DeepVarial(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DeepVarial1, BigCone)
{
    public static readonly AOEShape BigCone = new AOEShapeCone(60, 60.Degrees());
}

class DeepVarialPredict(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_DeepVarial1)
{
    private DateTime _activation;
    private Angle _rotation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCone(60, 60.Degrees()), Arena.Center - _rotation.ToDirection() * 20, _rotation, _activation, Risky: false);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SickSwell && id == 0x2487)
        {
            _rotation = actor.Rotation + 180.Degrees();
            _activation = WorldState.FutureTime(10);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _rotation = default;
            _activation = default;
        }
    }
}

// small wave is a mapeffect (02 = north, 04 = south)
// 00800040 = 1 ball
// 08000400 = 4 balls
// 270.18 -> 280.10

class DeepVarialSpreadStack(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 2 or 4)
        {
            if (state == 0x00800040)
            {
                var stackTarget = Raid.WithoutSlot().Where(r => r.FindStatus(SID._Gen_Watersnaking) != null).OrderByDescending(r => r.Role == Role.Healer).FirstOrDefault();
                if (stackTarget != null)
                    Stacks.Add(new(stackTarget, 6, minSize: 4, activation: WorldState.FutureTime(20)));
            }

            if (state == 0x08000400)
            {
                foreach (var t in Raid.WithoutSlot().Where(r => r.FindStatus(SID._Gen_Watersnaking) != null))
                    Spreads.Add(new(t, 5, WorldState.FutureTime(20)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_AwesomeSplash1:
                NumCasts++;
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
            case AID._Weaponskill_AwesomeSlab1:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class SnakingHotImpact(BossModule module) : Components.CastSharedTankbuster(module, AID._Weaponskill_HotImpact1, 6);

// steam burst: 4.3s after hit by aoe

class SteamBurst(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SteamBurst, 9)
{
    public void Reset()
    {
        NumCasts = 0;
    }
}
class SteamBurstPredict(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _triggers = [];
    private readonly List<(Actor Actor, bool Casting)> _balls = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (ball, _) in _balls.Where(b => !b.Casting))
        {
            if (_triggers.FirstOrNull(t => t.Check(ball.Position)) is { } trigger)
                yield return new(new AOEShapeCircle(9), ball.Position, default, trigger.Activation, Risky: false);
        }
    }

    public override void Update()
    {
        _triggers.RemoveAll(t => t.Activation < WorldState.CurrentTime.AddSeconds(2));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_DeepVarial1:
                _triggers.Add(new(new AOEShapeCone(60, 60.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.3f)));
                break;
            case AID._Weaponskill_SickSwell1:
                _triggers.Add(new(new AOEShapeRect(50, 25), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.3f)));
                break;

            case AID._Weaponskill_SteamBurst:
                for (var i = 0; i < _balls.Count; i++)
                {
                    if (_balls[i].Actor.Position.AlmostEqual(caster.Position, 0.1f))
                        _balls.Ref(i).Casting = true;
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_SteamBurst)
        {
            for (var i = 0; i < _balls.Count; i++)
                if (_balls[i].Actor.Position.AlmostEqual(caster.Position, 0.1f))
                    _balls.Ref(i).Casting = false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.FlamePuddle5 or OID.FlamePuddle6)
            _balls.Add((actor, false));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.FlamePuddle5 or OID.FlamePuddle6)
            _balls.Remove((actor, false));
    }
}

class HotAerial(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private DateTime _next;
    private BitMask _targets;
    private WPos _origin;

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_next == default)
            return;

        var source = _origin;
        for (var i = 0; i < Math.Min(2, 4 - NumCasts); i++)
        {
            var target = Raid.WithSlot().IncludedInMask(_targets).Select(p => p.Item2).Farthest(source);
            if (target != null)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCircle(6), _next.AddSeconds(i * 2)));
                source = target.Position;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HotAerial)
            _next = Module.CastFinishAt(spell, 0.4f);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Firesnaking)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
        if ((SID)status.ID == SID._Gen_FireResistanceDownII)
            ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HotAerial1)
        {
            _origin = WorldState.Actors.Find(spell.MainTargetID)?.Position ?? default;
            if (++NumCasts >= 4)
                _next = default;
        }
    }
}

class HotAerialPuddle(BossModule module) : FlamePuddle(module, [AID._Weaponskill_HotAerial2, AID._Weaponskill_HotAerial3, AID._Weaponskill_HotAerial4, AID._Weaponskill_HotAerial5], new AOEShapeCircle(6), OID.FlamePuddle6, true);
