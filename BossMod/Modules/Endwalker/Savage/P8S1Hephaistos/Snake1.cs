namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

// TODO: add various hints for gaze/explode
class Snake1(BossModule module) : PetrifactionCommon(module)
{
    struct PlayerState
    {
        public int Order; // -1 means unassigned, otherwise 0 or 1
        public bool IsExplode; // undefined until order is assigned
        public int AssignedSnake; // -1 if not assigned, otherwise index of assigned snake
    }

    private readonly PlayerState[] _players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { Order = -1, AssignedSnake = -1 });

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_players[slot].Order >= 0)
        {
            hints.Add($"Order: {(_players[slot].IsExplode ? "explode" : "petrify")} {_players[slot].Order + 1}", false);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var state = _players[pcSlot];
        if (state.Order >= 0)
        {
            // show circle around assigned snake
            if (state.AssignedSnake >= 0)
                Arena.AddCircle(ActiveGorgons[state.AssignedSnake].caster.Position, 2, ArenaColor.Safe);

            if (state.IsExplode)
                DrawExplode(pc, state.Order == 1 && NumCasts < 2);
            else
                DrawPetrify(pc, state.Order == 1 && NumCasts < 2);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FirstInLine:
                SetPlayerOrder(Raid.FindSlot(actor.InstanceID), 0);
                break;
            case SID.SecondInLine:
                SetPlayerOrder(Raid.FindSlot(actor.InstanceID), 1);
                break;
            case SID.EyeOfTheGorgon:
                SetPlayerExplode(Raid.FindSlot(actor.InstanceID), false);
                break;
            case SID.BloodOfTheGorgon:
                SetPlayerExplode(Raid.FindSlot(actor.InstanceID), true);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FirstInLine:
            case SID.SecondInLine:
                SetPlayerOrder(Raid.FindSlot(actor.InstanceID), -1);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.Petrifaction)
        {
            if (ActiveGorgons.Count == 2)
                InitAssignments(0);
            else if (ActiveGorgons.Count == 4)
                InitAssignments(1);
        }
    }

    private void SetPlayerOrder(int slot, int order)
    {
        if (slot >= 0)
            _players[slot].Order = order;
    }

    private void SetPlayerExplode(int slot, bool explode)
    {
        if (slot >= 0)
            _players[slot].IsExplode = explode;
    }

    private void InitAssignments(int order)
    {
        int[] assignedSlots = [-1, -1, -1, -1];
        foreach (var a in Service.Config.Get<P8S1Config>().Snake1Assignments.Resolve(Raid))
            if (_players[a.slot].Order == order)
                assignedSlots[a.group] = a.slot;
        if (assignedSlots[0] == -1)
            return; // invalid assignments

        var option1 = order * 2; // first CW from N
        var option2 = option1 + 1; // first CCW from NW
        if (ActiveGorgons[option1].priority > ActiveGorgons[option2].priority)
            Utils.Swap(ref option1, ref option2);

        bool flex = _players[assignedSlots[0]].IsExplode == _players[assignedSlots[1]].IsExplode;
        _players[assignedSlots[0]].AssignedSnake = option1;
        _players[assignedSlots[1]].AssignedSnake = flex ? option2 : option1;
        _players[assignedSlots[2]].AssignedSnake = flex ? option1 : option2;
        _players[assignedSlots[3]].AssignedSnake = option2;
    }
}
