namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class WickedWheel : Components.CastCounter
{
    private DateTime _expectedNext;
    private static readonly float _radius = 8.7f;

    public WickedWheel() : base(ActionID.MakeSpell(AID.WickedWheel)) { }

    public override void Init(BossModule module)
    {
        _expectedNext = module.WorldState.CurrentTime.AddSeconds(25);
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_expectedNext != default)
            hints.Add($"Wicked wheel in ~{Math.Max((_expectedNext - module.WorldState.CurrentTime).TotalSeconds, 0)}s");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // note: suparna also casts this, but we generally ignore it...
        if (_expectedNext != default && module.PrimaryActor.TargetID != actor.InstanceID && (_expectedNext - module.WorldState.CurrentTime).TotalSeconds < 3)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, _radius), _expectedNext);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_expectedNext != default && (_expectedNext - module.WorldState.CurrentTime).TotalSeconds < 3)
            arena.AddCircle(module.PrimaryActor.Position, _radius, ArenaColor.Danger);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if (spell.Action == WatchedAction)
        {
            // not sure about this ...
            _expectedNext = module.Enemies(OID.Suparna).Any(a => a.IsTargetable && !a.IsDead) ? module.WorldState.CurrentTime.AddSeconds(25) : new();
        }
    }
}
