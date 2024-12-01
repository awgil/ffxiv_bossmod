namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: fixed tethers strat variant (tether target with clone on safe side goes S, other goes N, if any group has 5 players prio1 adjusts)
class P1BoundOfFaith(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly int[] _assignedGroups = new int[PartyState.MaxPartySize];
    private OID _safeHalo;
    private int _safeSide; // -1 if X<0, +1 if X>0

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_assignedGroups[pcSlot] != 0 && _safeSide != 0)
        {
            var safeDir = _safeSide * (90 - _assignedGroups[pcSlot] * 22.5f).Degrees();
            Arena.AddCircle(Module.Center + 19 * safeDir.ToDirection(), 1, ArenaColor.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _safeHalo = (AID)spell.Action.ID switch
        {
            AID.TurnOfHeavensFire => OID.HaloOfLevin,
            AID.TurnOfHeavensLightning => OID.HaloOfFlame,
            _ => _safeHalo
        };
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fire && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            AddStack(target, WorldState.FutureTime(10.6f));
            if (Stacks.Count == 2)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BoundOfFaithSinsmoke)
        {
            Stacks.Clear();
        }
    }

    private void InitAssignments()
    {
        if (_safeHalo != default)
        {
            WDir averageOffset = default;
            foreach (var aoe in Module.Enemies(_safeHalo))
                averageOffset += aoe.Position - Module.Center;
            _safeSide = averageOffset.X > 0 ? 1 : -1;
        }

        // initial assignments
        Span<int> tetherSlots = [-1, -1];
        Span<int> prio = [0, 0, 0, 0, 0, 0, 0, 0];
        foreach (var (slot, group) in _config.P1BoundOfFaithAssignment.Resolve(Raid))
        {
            _assignedGroups[slot] = group < 4 ? -1 : 1;
            prio[slot] = group & 3;
            if (IsStackTarget(Raid[slot]))
                tetherSlots[tetherSlots[0] < 0 ? 0 : 1] = slot;
        }

        // swaps
        if (tetherSlots[0] >= 0 && _assignedGroups[tetherSlots[0]] == _assignedGroups[tetherSlots[1]])
        {
            // flex tether with lower prio
            var tetherFlexSlot = prio[tetherSlots[0]] < prio[tetherSlots[1]] ? tetherSlots[0] : tetherSlots[1];
            _assignedGroups[tetherFlexSlot] = -_assignedGroups[tetherFlexSlot];

            // now the group where we've moved flex slot has 5 people, find untethered with lowest prio
            for (var normalFlexSlot = 0; normalFlexSlot < PartyState.MaxPartySize; ++normalFlexSlot)
            {
                if (normalFlexSlot != tetherFlexSlot && prio[normalFlexSlot] == 0 && _assignedGroups[normalFlexSlot] == _assignedGroups[tetherFlexSlot])
                {
                    _assignedGroups[normalFlexSlot] = -_assignedGroups[normalFlexSlot];
                    break;
                }
            }
        }
    }
}
