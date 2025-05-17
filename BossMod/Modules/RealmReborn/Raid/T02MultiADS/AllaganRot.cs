namespace BossMod.RealmReborn.Raid.T02MultiADS;

// note: currently we assume that there is max 1 rot being passed around
class AllaganRot(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] _rotExpiration = new DateTime[PartyState.MaxPartySize];
    private readonly DateTime[] _immunityExpiration = new DateTime[PartyState.MaxPartySize];
    private int _rotHolderSlot = -1;

    private const float _rotPassRadius = 3;
    private static readonly PartyRolesConfig.Assignment[] _rotPriority = [PartyRolesConfig.Assignment.R1, PartyRolesConfig.Assignment.M1, PartyRolesConfig.Assignment.M2, PartyRolesConfig.Assignment.H1, PartyRolesConfig.Assignment.H2, PartyRolesConfig.Assignment.R2];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_rotHolderSlot >= 0)
            hints.Add($"Rot: {(_rotExpiration[_rotHolderSlot] - WorldState.CurrentTime).TotalSeconds:f1}s");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_rotHolderSlot == -1 || _rotHolderSlot == slot)
            return; // nothing special if there is no rot yet or if we're rot holder (we let other come to us to get it)

        var rotHolder = Raid[_rotHolderSlot];
        if (rotHolder == null)
            return;

        if (WantToPickUpRot(assignment))
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(rotHolder.Position, _rotPassRadius - 1), _rotExpiration[_rotHolderSlot]);
        else
            hints.AddForbiddenZone(ShapeContains.Circle(rotHolder.Position, _rotPassRadius + 4), _immunityExpiration[slot]);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _rotHolderSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AllaganRot)
        {
            // predict rot target
            _rotHolderSlot = Raid.FindSlot(spell.TargetID);
            if (_rotHolderSlot >= 0)
                _rotExpiration[_rotHolderSlot] = Module.CastFinishAt(spell, 15);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AllaganRot:
                _rotHolderSlot = Raid.FindSlot(actor.InstanceID);
                if (_rotHolderSlot >= 0)
                    _rotExpiration[_rotHolderSlot] = status.ExpireAt;
                break;
            case SID.AllaganImmunity:
                if (Raid.TryFindSlot(actor, out var slot))
                    _immunityExpiration[slot] = status.ExpireAt;
                break;
        }
    }

    private bool WantToPickUpRot(PartyRolesConfig.Assignment assignment)
    {
        var deadline = _rotExpiration[_rotHolderSlot];
        if ((deadline - WorldState.CurrentTime).TotalSeconds > 7)
            return false; // let rot tick for a while; note that we start moving a bit early, since getting into position takes some time, sometimes up to ~5s

        // note: rot timer is 15s, immunity is 40s - so if we pass at <= 5s left, we need 5 people in rotation; currently we hardcode priority to R1 (assumed phys) -> M1 -> M2 -> H1 -> H2 -> R2 (spare, assumed caster)
        var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
        if (assignments.Length == 0)
            return false; // if assignments are unset, we can't define pass priority

        foreach (var next in _rotPriority)
        {
            int nextSlot = assignments[(int)next];
            if (nextSlot != _rotHolderSlot && _immunityExpiration[nextSlot] < deadline && !(Raid[nextSlot]?.IsDead ?? true))
                return next == assignment;
        }

        // we're fucked, probably too many people are dead
        return false;
    }
}
