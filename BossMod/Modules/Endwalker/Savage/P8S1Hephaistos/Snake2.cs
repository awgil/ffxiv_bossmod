namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Gorgospit(BossModule module) : Components.GenericAOEs(module, (uint)AID.Gorgospit)
{
    public List<(Actor caster, DateTime finish)> Casters = [];

    private static readonly AOEShapeRect _shape = new(60f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            if (c.caster.CastInfo == null)
                aoes[i] = new(_shape, c.caster.Position, c.caster.Rotation, c.finish);
            else
                aoes[i] = new(_shape, c.caster.CastInfo.LocXZ, c.caster.CastInfo.Rotation, Module.CastFinishAt(c.caster.CastInfo));
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.IllusoryHephaistosSnakes && id == 0x11D2)
            Casters.Add((actor, WorldState.FutureTime(8d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.RemoveAll(c => c.caster == caster);
    }
}

// TODO: add various hints for gaze/explode
class Snake2(BossModule module) : PetrifactionCommon(module)
{
    struct PlayerState
    {
        public bool LongPetrify;
        public bool HasCrown;
        public bool HasBreath;
        public int AssignedSnake; // -1 if not assigned, otherwise index of assigned snake

        public readonly bool HasDebuff => HasCrown || HasBreath;
    }

    private readonly PlayerState[] _players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { AssignedSnake = -1 });
    private int _gorgospitCounter;

    private const float _breathRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var state = _players[slot];
        hints.Add($"Petrify order: {(state.LongPetrify ? 2 : 1)}, {(state.HasCrown ? "hide behind snake" : "stack between snakes")}", false);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => NumCrownCasts == 0 && _players[playerSlot].HasCrown ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (NumEyeCasts < 8)
        {
            if (_players[pcSlot].LongPetrify != (NumEyeCasts < 4))
                DrawPetrify(pc, NumCasts == 0);
            else
                DrawExplode(pc, NumCasts == 0);
        }
        else if (NumBreathCasts == 0)
        {
            // show circle around assigned snake
            if (_players[pcSlot].AssignedSnake >= 0)
                Arena.ZoneCircleOutline(ActiveGorgons[_players[pcSlot].AssignedSnake].caster.Position, 2f, Colors.Safe);

            foreach (var (slot, player) in Raid.WithSlot(false, true, true))
                if (_players[slot].HasBreath)
                    Arena.ZoneCircleOutline(player.Position, _breathRadius, Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.EyeOfTheGorgon:
                SetPlayerLongPetrify(Raid.FindSlot(actor.InstanceID), (status.ExpireAt - WorldState.CurrentTime).TotalSeconds > 25d);
                break;
            case (uint)SID.CrownOfTheGorgon:
                SetPlayerCrown(Raid.FindSlot(actor.InstanceID), true);
                break;
            case (uint)SID.BreathOfTheGorgon:
                SetPlayerBreath(Raid.FindSlot(actor.InstanceID), true);
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.IllusoryHephaistosSnakes || id != 0x11D2 || _gorgospitCounter++ != 4)
            return;

        int[] assignedSlots = [-1, -1, -1, -1, -1, -1, -1, -1]; // supports then dd
        foreach (var a in Service.Config.Get<P8S1Config>().Snake2Assignments.Resolve(Raid))
            assignedSlots[a.group + (Raid[a.slot]?.Role is Role.Tank or Role.Healer ? 0 : 4)] = a.slot;
        if (assignedSlots[0] == -1)
            return; // invalid assignments

        // 5th gorgospit => find snakes that will survive it
        List<int> survivingSnakes = [];
        var normal = actor.Rotation.ToDirection().OrthoL();
        for (var i = 0; i < ActiveGorgons.Count; ++i)
            if (Math.Abs(normal.Dot(ActiveGorgons[i].caster.Position - actor.Position)) > 5)
                survivingSnakes.Add(i);
        if (survivingSnakes.Count != 2)
            return;

        var (option1, option2) = AssignSnakesToGroups(survivingSnakes[0], survivingSnakes[1]);

        // both TH and DD should always get exactly 2 debuffs of same type
        var flexTH = _players[assignedSlots[0]].HasDebuff == _players[assignedSlots[2]].HasDebuff;
        _players[assignedSlots[0]].AssignedSnake = flexTH ? option2 : option1;
        _players[assignedSlots[1]].AssignedSnake = flexTH ? option1 : option2;
        _players[assignedSlots[2]].AssignedSnake = option1;
        _players[assignedSlots[3]].AssignedSnake = option2;

        var flexDD = _players[assignedSlots[4]].HasDebuff == _players[assignedSlots[6]].HasDebuff;
        _players[assignedSlots[4]].AssignedSnake = flexDD ? option2 : option1;
        _players[assignedSlots[5]].AssignedSnake = flexDD ? option1 : option2;
        _players[assignedSlots[6]].AssignedSnake = option1;
        _players[assignedSlots[7]].AssignedSnake = option2;
    }

    private void SetPlayerLongPetrify(int slot, bool value)
    {
        if (slot >= 0)
            _players[slot].LongPetrify = value;
    }

    private void SetPlayerCrown(int slot, bool value)
    {
        if (slot >= 0)
            _players[slot].HasCrown = value;
    }

    private void SetPlayerBreath(int slot, bool value)
    {
        if (slot >= 0)
            _players[slot].HasBreath = value;
    }

    private (int, int) AssignSnakesToGroups(int snake1, int snake2)
    {
        if (Service.Config.Get<P8S1Config>().Snake2CardinalPriorities)
        {
            // G1/G2 take N/S, or if Z coords are equal - G1/G2 take W/E
            var pos1 = ActiveGorgons[snake1].caster.Position;
            var pos2 = ActiveGorgons[snake2].caster.Position;
            var (coord1, coord2) = Math.Abs(pos1.Z - pos2.Z) > 5 ? (pos1.Z, pos2.Z) : (pos1.X, pos2.X);
            if (coord1 > coord2)
                Utils.Swap(ref snake1, ref snake2);
        }
        else
        {
            // G1 takes higher priority
            if (ActiveGorgons[snake1].priority < ActiveGorgons[snake2].priority)
                Utils.Swap(ref snake1, ref snake2);
        }
        return (snake1, snake2);
    }
}
