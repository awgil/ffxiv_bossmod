namespace BossMod.Dawntrail.Ultimate.FRU;

abstract class P2Banish(BossModule module) : Components.UniformStackSpread(module, 5, 5, 2, 2, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BanishStack:
                // TODO: this can target either supports or dd
                AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 0.1f));
                break;
            case AID.BanishSpread:
                AddSpreads(Module.Raid.WithoutSlot(true), Module.CastFinishAt(spell, 0.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BanishStackAOE:
                Stacks.Clear();
                break;
            case AID.BanishSpreadAOE:
                Spreads.Clear();
                break;
        }
    }
}

// this variant provides hints after mirrors
class P2Banish1(BossModule module) : P2Banish(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var prepos = PrepositionLocation(assignment);
        if (prepos != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(prepos.Value, 1), DateTime.MaxValue);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos? PrepositionLocation(PartyRolesConfig.Assignment assignment)
    {
        // TODO: consider a different strategy for melee (left if more left)
        if (Stacks.Count > 0 && Stacks[0].Activation > WorldState.FutureTime(2.5f))
        {
            // preposition for stacks
            var boss = Module.Enemies(OID.BossP2).FirstOrDefault();
            return assignment switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 => boss != null ? boss.Position + 6 * boss.Rotation.ToDirection().OrthoL() : null,
                PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 => boss != null ? boss.Position + 6 * boss.Rotation.ToDirection().OrthoR() : null,
                _ => null // TODO: implement positioning for ranged
            };
        }
        else if (Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(2.5f))
        {
            // preposition for spreads
            var boss = Module.Enemies(OID.BossP2).FirstOrDefault();
            return assignment switch
            {
                PartyRolesConfig.Assignment.MT => boss != null ? boss.Position + 6 * (boss.Rotation + 45.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.OT => boss != null ? boss.Position + 6 * (boss.Rotation - 45.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.M1 => boss != null ? boss.Position + 6 * (boss.Rotation + 135.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.M2 => boss != null ? boss.Position + 6 * (boss.Rotation - 135.Degrees()).ToDirection() : null,
                _ => null // TODO: implement positioning for ranged
            };
        }
        return null;
    }
}

// this variant provides hints after rampant
class P2Banish2(BossModule module) : P2Banish(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private bool _allowHints;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_allowHints)
            return; // don't interfere with tower hints until it's done
        var prepos = PrepositionLocation(assignment);
        if (prepos != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(prepos.Value, 1), DateTime.MaxValue);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos? PrepositionLocation(PartyRolesConfig.Assignment assignment)
    {
        var clockspot = _config.P2Banish2SpreadSpots[assignment];
        if (clockspot < 0)
            return null; // no assignment

        var assignedDirection = (180 - 45 * clockspot).Degrees();
        if (Stacks.Count > 0 && Stacks[0].Activation > WorldState.FutureTime(1))
        {
            var isSupport = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2;
            if (_config.P2Banish2SupportsMoveToStack == isSupport)
                assignedDirection += (_config.P2Banish2MoveCCWToStack ? 45 : -45).Degrees();
            return Module.Center + 10 * assignedDirection.ToDirection();
        }
        else if (Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(1))
        {
            return Module.Center + 13 * assignedDirection.ToDirection();
        }
        return null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrightHunger)
            _allowHints = true;
    }
}
