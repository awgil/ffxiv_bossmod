namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class SickestTakeOff(BossModule module) : Components.StandardAOEs(module, AID.SickestTakeOffAOE, new AOEShapeRect(50, 7.5f));
class SickSwell(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SickSwellAOE, 10, kind: Kind.DirForward)
{
    public bool EnableHints = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class AwesomeSplab : Components.GenericStackSpread
{
    public int NumCasts { get; private set; }

    public AwesomeSplab(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.DeepBlue && (SID)status.ID == SID.Unk2056)
        {
            switch (status.Extra)
            {
                case 0x3ED:
                    foreach (var p in Raid.WithoutSlot().OrderByDescending(a => a.Role == Role.Healer).Take(2))
                        Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(15.7f)));
                    break;
                case 0x3EE:
                    foreach (var p in Raid.WithoutSlot())
                        Spreads.Add(new(p, 5, WorldState.FutureTime(15.7f)));
                    break;
                case 0x3EF:
                    foreach (var p in Raid.WithoutSlot().Where(p => p.FindStatus(SID.Watersnaking) != null).OrderByDescending(a => a.Role == Role.Healer).Take(1))
                        Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(15.7f)));
                    break;
                case 0x3F0:
                    foreach (var p in Raid.WithoutSlot().Where(p => p.FindStatus(SID.Watersnaking) != null))
                        Spreads.Add(new(p, 5, WorldState.FutureTime(15.7f)));
                    break;
                default:
                    ReportError($"Unrecognized status param {status.Extra:X} on Deep Blue");
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AwesomeSplash1:
            case AID.AwesomeSplash2:
                NumCasts++;
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
            case AID.AwesomeSlab1:
            case AID.AwesomeSlab2:
                NumCasts++;
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}

class AlleyOopProteans(BossModule module) : Components.GenericBaitAway(module)
{
    private BitMask _targets;
    private bool _reverse;

    public static readonly AOEShapeCone Shape = new(60, 15.Degrees());

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add(_reverse ? "Proteans: split" : "Proteans: repeat");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AlleyOopDoubleDipCast or AID.ReverseAlleyOopCast)
        {
            _reverse = (AID)spell.Action.ID == AID.ReverseAlleyOopCast;

            var mask = _targets.NumSetBits() == 0 ? new(~0ul) : _targets;
            foreach (var (_, player) in Raid.WithSlot().IncludedInMask(mask))
                CurrentBaits.Add(new(caster, player, Shape, Module.CastFinishAt(spell)));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_targets[pcSlot] && _reverse && ActiveBaitsOn(pc).FirstOrNull() is { } b)
            Arena.AddCone(b.Source.Position, 60, b.Rotation, 30.Degrees(), ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_targets[slot] && _reverse && ActiveBaitsOn(actor).FirstOrNull() is { } b)
        {
            if (Raid.WithSlot().ExcludedFromMask(_targets).InShape(new AOEShapeCone(60, 30.Degrees()), b.Source.Position, b.Rotation).Any())
                hints.Add("Bait away from other party!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Watersnaking)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Watersnaking)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AlleyOopDoubleDipFirst or AID.ReverseAlleyOopFirst)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class AlleyOopProteanRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    const float Delay = 2.6f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlleyOopDoubleDipFirst:
                _predicted.Add(new(new AOEShapeCone(60, 15.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(Delay)));
                break;

            case AID.ReverseAlleyOopFirst:
                _predicted.Add(new(new AOEShapeCone(60, 7.5f.Degrees()), caster.Position, spell.Rotation + 22.5f.Degrees(), WorldState.FutureTime(Delay)));
                _predicted.Add(new(new AOEShapeCone(60, 7.5f.Degrees()), caster.Position, spell.Rotation - 22.5f.Degrees(), WorldState.FutureTime(Delay)));
                break;

            case AID.AlleyOopDoubleDipRepeat:
            case AID.ReverseAlleyOopRepeat:
                NumCasts++;
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}

class DeepImpactBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Actor? Source;
    private BitMask _targets;

    public override void Update()
    {
        CurrentBaits.Clear();

        if (Source != null)
        {
            var target = Raid.WithSlot().IncludedInMask(_targets.NumSetBits() > 0 ? _targets : new(0xFF)).Select(p => p.Item2).Farthest(Source.Position);
            if (target != null)
                CurrentBaits.Add(new(Source, target, new AOEShapeCircle(6), Module.CastFinishAt(Source.CastInfo)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeepImpactCast)
        {
            Source = caster;
            ForbiddenPlayers = Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DeepImpact)
        {
            NumCasts++;
            Source = null;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Watersnaking)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class DeepImpactKB(BossModule module) : Components.Knockback(module)
{
    private readonly DeepImpactBait _bait = module.FindComponent<DeepImpactBait>()!;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var b in _bait.ActiveBaitsOn(actor))
            yield return new(b.Source.Position, 10, b.Activation);
    }
}
