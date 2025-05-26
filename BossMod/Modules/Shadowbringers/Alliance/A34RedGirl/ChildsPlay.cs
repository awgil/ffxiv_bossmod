namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class ChildsPlay(BossModule module) : Components.GenericForcedMarch(module)
{
    private BitMask _targets = new();
    private Angle _direction;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Chain && Raid.TryFindSlot(source.InstanceID, out var slot))
            _targets.Set(slot);
    }

    public WDir? PredictedDirection(Actor player)
    {
        // ostensibly 24 units (6 units/sec * 4s debuff), actually 21-23 units depending on latency/acceleration, but we report a conservative estimate to avoid walking into the wall
        // the pylon explosions go off about a second after the debuff expires, so there is still time to adjust to a safe spot
        if (State.TryGetValue(player.InstanceID, out var state) && state.PendingMoves.Count > 0)
            return _direction.ToDirection() * 24;

        return null;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ChildsPlayNorth:
                _direction = 180.Degrees();
                AddMarches(Module.CastFinishAt(spell));
                break;
            case AID._Weaponskill_ChildsPlayEast:
                _direction = 90.Degrees();
                AddMarches(Module.CastFinishAt(spell));
                break;
        }
    }

    public override void Update()
    {
        // awful hack to make forced march always point in the correct cardinal direction
        // this mechanic really works more like a knockback, but i still want the nice realtime update from forced march :(
        foreach (var (id, state) in State)
            if (state.PendingMoves.Count > 0 && WorldState.Actors.Find(id) is { } player)
                state.PendingMoves.Ref(0).dir = _direction - player.Rotation;
    }

    private void AddMarches(DateTime activation)
    {
        foreach (var (_, player) in Raid.WithSlot().IncludedInMask(_targets))
            AddForcedMovement(player, default, 4, activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PayingThePiper)
        {
            if (State.TryGetValue(actor.InstanceID, out var st))
                st.PendingMoves.Clear();

            ActivateForcedMovement(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PayingThePiper)
            DeactivateForcedMovement(actor);
    }
}

class PylonExplosion(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Explosion, 9)
{
    private readonly ChildsPlay _march = module.FindComponent<ChildsPlay>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var displacement = _march.PredictedDirection(actor) is { } dir ? dir * -1 : default;

        foreach (var c in Casters)
            hints.AddForbiddenZone(Shape, c.Position + displacement, default, Module.CastFinishAt(c.CastInfo));
    }
}
