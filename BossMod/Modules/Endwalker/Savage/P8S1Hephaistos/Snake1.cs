using System;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    // TODO: add various hints for gaze/explode
    class Snake1 : PetrifactionCommon
    {
        struct PlayerState
        {
            public int Order; // -1 means unassigned, otherwise 0 or 1
            public bool IsExplode; // undefined until order is assigned
            public int AssignedSnake; // -1 if not assigned, otherwise index of assigned snake
        }

        private PlayerState[] _players = new PlayerState[PartyState.MaxPartySize];

        public Snake1()
        {
            Array.Fill(_players, new PlayerState() { Order = -1, AssignedSnake = -1 });
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            if (_players[slot].Order >= 0)
            {
                hints.Add($"Order: {(_players[slot].IsExplode ? "explode" : "petrify")} {_players[slot].Order + 1}", false);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            var state = _players[pcSlot];
            if (state.Order >= 0)
            {
                // show circle around assigned snake
                if (state.AssignedSnake >= 0)
                    arena.AddCircle(ActiveGorgons[state.AssignedSnake].caster.Position, 2, ArenaColor.Safe);

                if (state.IsExplode)
                    DrawExplode(pc, state.Order == 1 && NumCasts < 2, arena);
                else
                    DrawPetrify(pc, state.Order == 1 && NumCasts < 2, arena);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstInLine:
                    SetPlayerOrder(module.Raid.FindSlot(actor.InstanceID), 0);
                    break;
                case SID.SecondInLine:
                    SetPlayerOrder(module.Raid.FindSlot(actor.InstanceID), 1);
                    break;
                case SID.EyeOfTheGorgon:
                    SetPlayerExplode(module.Raid.FindSlot(actor.InstanceID), false);
                    break;
                case SID.BloodOfTheGorgon:
                    SetPlayerExplode(module.Raid.FindSlot(actor.InstanceID), true);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstInLine:
                case SID.SecondInLine:
                    SetPlayerOrder(module.Raid.FindSlot(actor.InstanceID), -1);
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.Petrifaction)
            {
                if (ActiveGorgons.Count == 2)
                    InitAssignments(module, 0);
                else if (ActiveGorgons.Count == 4)
                    InitAssignments(module, 1);
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

        private void InitAssignments(BossModule module, int order)
        {
            int[] assignedSlots = { -1, -1, -1, -1 };
            foreach (var a in Service.Config.Get<P8S1Config>().SnakeAssignments.Resolve(module.Raid))
                if (_players[a.slot].Order == order)
                    assignedSlots[a.group] = a.slot;
            if (assignedSlots[0] == -1)
                return; // invalid assignments

            var option1 = order * 2; // first CW from N
            var option2 = option1 + 1; // first CCW from NW
            if (ActiveGorgons[option1].priority > ActiveGorgons[option2].priority)
                Utils.Swap(ref option1, ref option2);

            bool flex = _players[assignedSlots[0]].IsExplode == _players[assignedSlots[1]].IsExplode;
            _players[assignedSlots[0]].AssignedSnake = flex ? option1 : option2;
            _players[assignedSlots[1]].AssignedSnake = option2;
            _players[assignedSlots[2]].AssignedSnake = flex ? option2 : option1;
            _players[assignedSlots[3]].AssignedSnake = option1;
        }
    }
}
