using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class Gorgospit : Components.GenericAOEs
    {
        public List<(Actor caster, DateTime finish)> Casters = new();

        private static AOEShapeRect _shape = new(60, 5);

        public Gorgospit() : base(ActionID.MakeSpell(AID.Gorgospit)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in Casters)
            {
                if (c.caster.CastInfo == null)
                    yield return new(_shape, c.caster.Position, c.caster.Rotation, c.finish);
                else
                    yield return new(_shape, c.caster.Position, c.caster.CastInfo.Rotation, c.caster.CastInfo.FinishAt);
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.IllusoryHephaistosSnakes && id == 0x11D2)
                Casters.Add((actor, module.WorldState.CurrentTime.AddSeconds(8)));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                Casters.RemoveAll(c => c.caster == caster);
        }
    }

    // TODO: add various hints for gaze/explode
    class Snake2 : PetrifactionCommon
    {
        struct PlayerState
        {
            public bool LongPetrify;
            public bool HasCrown;
            public bool HasBreath;
            public int AssignedSnake; // -1 if not assigned, otherwise index of assigned snake

            public bool HasDebuff => HasCrown || HasBreath;
        }

        private PlayerState[] _players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { AssignedSnake = -1 });
        private int _gorgospitCounter;

        private const float _breathRadius = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            var state = _players[slot];
            hints.Add($"Petrify order: {(state.LongPetrify ? 2 : 1)}, {(state.HasCrown ? "hide behind snake" : "stack between snakes")}", false);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return NumCrownCasts == 0 && _players[playerSlot].HasCrown ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            if (NumEyeCasts < 8)
            {
                if (_players[pcSlot].LongPetrify != (NumEyeCasts < 4))
                    DrawPetrify(pc, NumCasts == 0, arena);
                else
                    DrawExplode(pc, NumCasts == 0, arena);
            }
            else if (NumBreathCasts == 0)
            {
                // show circle around assigned snake
                if (_players[pcSlot].AssignedSnake >= 0)
                    arena.AddCircle(ActiveGorgons[_players[pcSlot].AssignedSnake].caster.Position, 2, ArenaColor.Safe);

                foreach (var (slot, player) in module.Raid.WithSlot())
                    if (_players[slot].HasBreath)
                        arena.AddCircle(player.Position, _breathRadius, ArenaColor.Safe);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.EyeOfTheGorgon:
                    SetPlayerLongPetrify(module.Raid.FindSlot(actor.InstanceID), (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds > 25);
                    break;
                case SID.CrownOfTheGorgon:
                    SetPlayerCrown(module.Raid.FindSlot(actor.InstanceID), true);
                    break;
                case SID.BreathOfTheGorgon:
                    SetPlayerBreath(module.Raid.FindSlot(actor.InstanceID), true);
                    break;
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID != OID.IllusoryHephaistosSnakes || id != 0x11D2 || _gorgospitCounter++ != 4)
                return;

            int[] assignedSlots = { -1, -1, -1, -1, -1, -1, -1, -1 }; // supports then dd
            foreach (var a in Service.Config.Get<P8S1Config>().Snake2Assignments.Resolve(module.Raid))
                assignedSlots[a.group + (module.Raid[a.slot]?.Role is Role.Tank or Role.Healer ? 0 : 4)] = a.slot;
            if (assignedSlots[0] == -1)
                return; // invalid assignments

            // 5th gorgospit => find snakes that will survive it
            List<int> survivingSnakes = new();
            var normal = actor.Rotation.ToDirection().OrthoL();
            for (int i = 0; i < ActiveGorgons.Count; ++i)
                if (Math.Abs(normal.Dot(ActiveGorgons[i].caster.Position - actor.Position)) > 5)
                    survivingSnakes.Add(i);
            if (survivingSnakes.Count != 2)
                return;

            var (option1, option2) = AssignSnakesToGroups(survivingSnakes[0], survivingSnakes[1]);

            // both TH and DD should always get exactly 2 debuffs of same type
            bool flexTH = _players[assignedSlots[0]].HasDebuff == _players[assignedSlots[2]].HasDebuff;
            _players[assignedSlots[0]].AssignedSnake = flexTH ? option2 : option1;
            _players[assignedSlots[1]].AssignedSnake = flexTH ? option1 : option2;
            _players[assignedSlots[2]].AssignedSnake = option1;
            _players[assignedSlots[3]].AssignedSnake = option2;

            bool flexDD = _players[assignedSlots[4]].HasDebuff == _players[assignedSlots[6]].HasDebuff;
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
}
