namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to belone coils mechanic (role towers)
class BeloneCoils : BossComponent
{
    public enum Soaker { Unknown, TankOrHealer, DamageDealer }

    public Soaker ActiveSoakers { get; private set; } = Soaker.Unknown;
    private List<Actor> _activeTowers = new();

    private static readonly float _towerRadius = 4;

    public bool IsValidSoaker(Actor player)
    {
        return ActiveSoakers switch
        {
            Soaker.TankOrHealer => player.Role == Role.Tank || player.Role == Role.Healer,
            Soaker.DamageDealer => player.Role == Role.Melee || player.Role == Role.Ranged,
            _ => false
        };
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (ActiveSoakers == Soaker.Unknown)
            return;

        bool isSoaking = _activeTowers.InRadius(actor.Position, _towerRadius).Any();
        if (IsValidSoaker(actor))
        {
            hints.Add("Soak the tower", !isSoaking);
        }
        else
        {
            hints.Add("GTFO from tower", isSoaking);
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (ActiveSoakers == Soaker.Unknown)
            return;

        bool validSoaker = IsValidSoaker(pc);
        foreach (var tower in _activeTowers)
        {
            arena.AddCircle(tower.Position, _towerRadius, validSoaker ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BeloneCoilsDPS or AID.BeloneCoilsTH)
        {
            _activeTowers.Add(caster);
            ActiveSoakers = (AID)spell.Action.ID == AID.BeloneCoilsDPS ? Soaker.DamageDealer : Soaker.TankOrHealer;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BeloneCoilsDPS or AID.BeloneCoilsTH)
        {
            _activeTowers.Remove(caster);
            if (_activeTowers.Count == 0)
                ActiveSoakers = Soaker.Unknown;
        }
    }
}
