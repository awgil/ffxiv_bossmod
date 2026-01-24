namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class RavenousReach(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RavenousReach2, new AOEShapeCone(35, 60.Degrees()));
class DirectedGrotesquerie(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    readonly Angle?[] _offset = new Angle?[8];
    DateTime _activation;

    public override void Update()
    {
        CurrentBaits.Clear();

        for (var i = 0; i < _offset.Length; i++)
        {
            if (_offset[i] is not { } offset)
                continue;

            if (Raid[i] is not { } target)
                continue;

            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCone(60, 15.Degrees(), target.Rotation + offset), _activation, true));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectedGrotesquerieVisual && Raid.TryFindSlot(actor, out var slot))
        {
            _offset[slot] = status.Extra switch
            {
                0x40D => -90.Degrees(),
                0x40E => 180.Degrees(),
                0x40F => 90.Degrees(),
                _ => default
            };
        }

        if ((SID)status.ID == SID._Gen_DirectedGrotesquerie)
            _activation = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectedGrotesquerieVisual && Raid.TryFindSlot(actor, out var slot))
            _offset[slot] = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_offset[slot] is not { } offset)
            return;

        var forbidden = new ArcList(actor.Position, 60);
        foreach (var ally in Raid.WithoutSlot().Exclude(actor))
        {
            var angle = actor.AngleTo(ally) - offset;
            forbidden.ForbidInfiniteCone(actor.Position, angle, 18.Degrees());
        }

        foreach (var (from, to) in forbidden.Forbidden.Segments)
        {
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;
            hints.ForbiddenDirections.Add((center.Radians(), width.Radians(), _activation));
        }
    }
}

class PhagocyteSpotlight(BossModule module) : Components.StandardAOEs(module, AID._Spell_PhagocyteSpotlight, 5)
{
    public bool Finished;
    DateTime _firstCast;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (_firstCast == default)
                _firstCast = WorldState.CurrentTime;
            else if (_firstCast.AddSeconds(5) < WorldState.CurrentTime)
                Finished = true;
        }
    }
}

class Act1GrotesquerieStackSpread(BossModule module) : Components.UniformStackSpread(module, 6, 6, 4, 4)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_BurstingGrotesquerie:
                AddSpread(actor, status.ExpireAt);
                break;
            case SID._Gen_SharedGrotesquerie:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_DramaticLysis:
                Stacks.Clear();
                NumCasts++;
                break;
            case AID._Spell_FourthWallFusion:
                Spreads.Clear();
                NumCasts++;
                break;
        }
    }
}

class BurstPre(BossModule module) : Components.GenericAOEs(module, AID._Spell_Burst)
{
    readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeCircle(12), c.Item1.Position, default, c.Item2));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBF29 && state == 0x00100020)
            _casters.Add((actor, WorldState.FutureTime(6.7f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Clear();
    }
}
class Burst(BossModule module) : Components.StandardAOEs(module, AID._Spell_Burst, 12);

class BurstStackSpread(BossModule module) : Components.UniformStackSpread(module, 6, 6, 6)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_com_share3t)
            AddStack(actor, WorldState.FutureTime(5.2f));
        if ((IconID)iconID == IconID._Gen_Icon_tank_lockonae_6m_5s_01t)
            AddSpread(actor, WorldState.FutureTime(5.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_FourthWallFusion1:
                NumCasts++;
                Stacks.Clear();
                break;
            case AID._Spell_VisceralBurst:
                NumCasts++;
                Spreads.Clear();
                break;
        }
    }
}
