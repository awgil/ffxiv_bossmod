namespace BossMod.Dawntrail.Foray.ForkedTower.FT01DemonTablet;

class PortentousCometeor : Components.StandardAOEs
{
    public PortentousCometeor(BossModule module) : base(module, AID.PortentousCometeor, new AOEShapeCircle(43))
    {
        Risky = false;
    }
}

class PortentousComet : Components.UniformStackSpread
{
    private Actor? _meteor;

    public PortentousComet(BossModule module) : base(module, 4, 43, minStackSize: 4, includeDeadTargets: false)
    {
        EnableHints = false;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Meteor)
            _meteor ??= actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PortentousCometKBNorth or AID.PortentousCometKBSouth && WorldState.Actors.Find(spell.TargetID) is { } tar)
            AddStack(tar, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PortentousCometKBNorth or AID.PortentousCometKBSouth)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CraterLater)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CraterLater)
            Spreads.RemoveAll(s => s.Target == actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (IsSpreadTarget(actor) && _meteor != null)
            hints.Add("Drop meteor near marker!", MathF.Abs(actor.Position.Z - _meteor.Position.Z) > 3);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsSpreadTarget(actor) && _meteor != null)
            hints.AddForbiddenZone(p => MathF.Abs(p.Z - _meteor.Position.Z) > 3, Spreads[0].Activation);

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (IsSpreadTarget(pc) && _meteor != null)
            Arena.AddCircle(_meteor.Position, 1, ArenaColor.Safe);
    }
}

class PortentousCometKB(BossModule module) : Components.Knockback(module, null, ignoreImmunes: true)
{
    private readonly List<(Actor Target, DateTime Activation, bool North)> _targets = [];

    private Actor? _meteor;
    private bool _meteorsCasting;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Meteor)
            _meteor ??= actor;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_meteor == null)
            yield break;

        var meteorSouth = _meteor.Position.Z > Arena.Center.Z;

        foreach (var t in _targets)
        {
            // knockback distance is based on meteor spawn side
            var dist = t.North == meteorSouth ? 18 : 13;
            yield return new(t.Target.Position, dist, t.Activation, new AOEShapeCircle(4), Direction: t.North ? 180.Degrees() : default, Kind: Kind.DirForward);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PortentousCometKBNorth:
                if (WorldState.Actors.Find(spell.TargetID) is { } target)
                    _targets.Add((target, Module.CastFinishAt(spell), true));
                break;
            case AID.PortentousCometKBSouth:
                if (WorldState.Actors.Find(spell.TargetID) is { } target2)
                    _targets.Add((target2, Module.CastFinishAt(spell), false));
                break;
            case AID.PortentousCometeor:
                _meteorsCasting = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PortentousCometKBNorth or AID.PortentousCometKBSouth)
        {
            NumCasts++;
            _targets.RemoveAll(t => t.Target.InstanceID == spell.MainTargetID);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var ix = _targets.FindIndex(t => t.Target == actor);
        if (ix >= 0 && _meteor != null)
        {
            var meteorSouth = _meteor.Position.Z > Arena.Center.Z;
            var playerSouth = actor.Position.Z > Arena.Center.Z;
            var shouldMatchSide = _targets[ix].North == meteorSouth;
            if (shouldMatchSide)
                hints.Add("Stay on meteor side!", meteorSouth != playerSouth);
            else
                hints.Add("Stay away from meteor side!", meteorSouth == playerSouth);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (_meteorsCasting)
            return;

        var ix = _targets.FindIndex(t => t.Target == pc);
        if (ix >= 0 && _meteor != null)
        {
            var meteorSouth = _meteor.Position.Z > Arena.Center.Z;
            var shouldMatchSide = _targets[ix].North == meteorSouth;
            var meteorDir = new WDir(0, meteorSouth ? 33 : -33);

            Arena.ZoneRect(Arena.Center, Arena.Center + (shouldMatchSide ? meteorDir : -meteorDir), 15, ArenaColor.SafeFromAOE);
        }
    }
}
