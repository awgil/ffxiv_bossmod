using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala
{
    class PlanarTactics : Components.GenericAOEs
    {
        public struct PlayerState
        {
            public int SubtractiveStacks;
            public bool StackTarget;
            public WDir[]? StartingOffsets;
        }

        public List<AOEInstance> Mines = new();
        public PlayerState[] Players = new PlayerState[4];

        private static AOEShapeRect _shape = new(4, 4, 4);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Mines;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            ref var p = ref Players[pcSlot];
            if (p.StartingOffsets != null)
                foreach (var off in p.StartingOffsets)
                    arena.AddCircle(module.Bounds.Center + off, 1, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.SubtractiveSuppressorAlpha:
                    if (module.Raid.FindSlot(actor.InstanceID) is var slot3 && slot3 >= 0 && slot3 < Players.Length)
                        Players[slot3].SubtractiveStacks = status.Extra;
                    break;
                case SID.SurgeVector:
                    if (module.Raid.FindSlot(actor.InstanceID) is var slot4 && slot4 >= 0 && slot4 < Players.Length)
                        Players[slot4].StackTarget = true;
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NArcaneMineAOE or AID.SArcaneMineAOE)
            {
                Mines.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
                if (Mines.Count == 8)
                {
                    InitSafespots(module);
                }
            }
        }

        private void InitSafespots(BossModule module)
        {
            WDir safeCornerOffset = default;
            foreach (var m in Mines)
                safeCornerOffset -= m.Origin - module.Bounds.Center;
            var relSouth = (safeCornerOffset + safeCornerOffset.OrthoL()) / 16;
            var relWest = relSouth.OrthoR();
            var off1 = 5 * relSouth + 13 * relWest;
            var off2a = 3 * relSouth + 13 * relWest;
            var off2b = -8 * relSouth + 16 * relWest;
            var off3 = 13 * relSouth - 8 * relWest;
            var sumStacks = Players.Sum(p => p.StackTarget ? p.SubtractiveStacks : 0); // can be 3 (1+2), 4 (2+2 or 1+3) or 5 (2+3)
            foreach (ref var p in Players.AsSpan())
            {
                p.StartingOffsets = (p.SubtractiveStacks, sumStacks) switch
                {
                    (1, _) => [off1],
                    (2, 3) => [p.StackTarget ? off2b : off2a],
                    (2, 4) => [off2a, off2b],
                    (2, 5) => [p.StackTarget ? off2a : off2b],
                    (3, _) => [off3],
                    _ => null
                };
            }
        }
    }

    class PlanarTacticsForcedMarch : Components.GenericForcedMarch
    {
        private int[] _rotationCount = new int[4];
        private Angle[] _rotation = new Angle[4];
        private DateTime _activation;

        public PlanarTacticsForcedMarch()
        {
            MovementSpeed = 4;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_rotation[slot] != default)
                hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
            base.AddHints(module, slot, actor, hints, movementHints);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.TimesThreePlayer:
                    _activation = status.ExpireAt;
                    if (module.Raid.FindSlot(actor.InstanceID) is var slot1 && slot1 >= 0 && slot1 < _rotationCount.Length)
                        _rotationCount[slot1] = -1;
                    break;
                case SID.TimesFivePlayer:
                    _activation = status.ExpireAt;
                    if (module.Raid.FindSlot(actor.InstanceID) is var slot2 && slot2 >= 0 && slot2 < _rotationCount.Length)
                        _rotationCount[slot2] = 1;
                    break;
                case SID.ForcedMarch:
                    State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
                    ActivateForcedMovement(actor, status.ExpireAt);
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var rot = (IconID)iconID switch
            {
                IconID.PlayerRotateCW => -90.Degrees(),
                IconID.PlayerRotateCCW => 90.Degrees(),
                _ => default
            };
            if (rot != default && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotationCount.Length)
            {
                _rotation[slot] = rot * _rotationCount[slot];
                AddForcedMovement(actor, _rotation[slot], 6, _activation);
            }
        }
    }
}
