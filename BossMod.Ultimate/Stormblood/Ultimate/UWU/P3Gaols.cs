namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: add sludge voidzones?..
class P3Gaols(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { None, TargetSelection, Fetters, Done }

    public State CurState { get; private set; }
    private BitMask _targets;

    readonly List<Actor> TargetsOrdered = [];

    int _numLandslides;
    DateTime _fettersAt;

    private static readonly AOEShapeCircle _freefireShape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurState == State.Fetters && !_targets[slot])
            foreach (var (_, target) in Raid.WithSlot(true).IncludedInMask(_targets))
                yield return new(_freefireShape, target.Position);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurState == State.TargetSelection && _targets.Any())
        {
            var hint = string.Join(" > ", TargetsOrdered.Select(t => t.Name));
            hints.Add($"Gaols: {hint}");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (TargetsOrdered.Count != 3 || CurState != State.TargetSelection)
            return;

        var myOrder = TargetsOrdered.IndexOf(actor);
        if (myOrder < 0)
            return;

        var toBoss = ((UWU)Module).Titan()!.Position - Arena.Center;
        var pos = Arena.Center + toBoss.Normalized() * (6.7f * (-myOrder + 1));

        hints.GoalZones.Add(AIHints.GoalProximity(pos, 10, 10));
        if (_numLandslides == 0)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(pos, toBoss.Normalized().OrthoR(), 5, 5, 3), _fettersAt);
            hints.GoalZones.Add(AIHints.GoalProximity(pos, 10, 10));
        }
        else
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(pos, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), _fettersAt);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurState == State.TargetSelection)
            foreach (var (_, target) in Raid.WithSlot().IncludedInMask(_targets))
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(target.Position, 6, 0xFF000000, 2);
                Arena.AddCircle(target.Position, 6, ArenaColor.Danger);
            }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Fetters)
        {
            if (CurState == State.TargetSelection)
            {
                CurState = State.Fetters;
                TargetsOrdered.Clear();
                _targets.Reset(); // note that if target dies, fetters is applied to random player
            }
            SetTarget(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.RockThrowBoss:
            case AID.RockThrowHelper:
                CurState = State.TargetSelection;
                _fettersAt = WorldState.FutureTime(4.8f);
                SetTarget(Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.FreefireGaol:
                var (closestSlot, closestPlayer) = Raid.WithSlot(true).IncludedInMask(_targets).Closest(caster.Position);
                if (closestPlayer != null)
                {
                    _targets.Clear(closestSlot);
                    if (_targets.None())
                        CurState = State.Done;
                }
                break;
            case AID.LandslideBoss:
            case AID.LandslideHelper:
                _numLandslides++;
                break;
        }
    }

    void SetTarget(int slot)
    {
        _targets.Set(slot);
        TargetsOrdered.Clear();
        TargetsOrdered.AddRange(Service.Config.Get<UWUConfig>().P3GaolPriorities.Resolve(Raid).Where(i => _targets[i.slot]).OrderBy(i => i.group).Select(i => Raid[i.slot]!));
    }
}

class P3Sludge(BossModule module) : Components.VoidzoneAtCastTarget(module, 6.3f, AID.FreefireGaol, OID.GaolSludge, 0.9f, activationDelay: 2);

class P3SingleGaol(BossModule module) : Components.Adds(module, (uint)OID.GraniteGaol, 1, true);

class P3GraniteImpact(BossModule module) : Components.CastHint(module, AID.GraniteImpactShort, "Break the rock!", true);
