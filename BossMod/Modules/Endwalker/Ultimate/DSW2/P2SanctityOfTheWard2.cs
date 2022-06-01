using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2SanctityOfTheWard2HeavensStakeCircles : CommonComponents.Puddles
    {
        public P2SanctityOfTheWard2HeavensStakeCircles() : base(ActionID.MakeSpell(AID.HeavensStakeAOE), 7) { }
    }

    class P2SanctityOfTheWard2HeavensStakeDonut : CommonComponents.SelfTargetedAOE
    {
        public P2SanctityOfTheWard2HeavensStakeDonut() : base(ActionID.MakeSpell(AID.HeavensStakeDonut), new AOEShapeDonut(15, 30)) { }
    }

    class P2SanctityOfTheWard2 : BossModule.Component
    {
        private struct QuadrantAssignments
        {
            private int _mask; // bits 2*i & 2*i+1 = quadrant index for player in slot i

            public int this[int slot]
            {
                get => (_mask >> (2 * slot)) & 3;
                set
                {
                    int offset = 2 * slot;
                    _mask &= ~(3 << offset);
                    _mask |= (value << offset);
                }
            }
        }

        public bool StormDone { get; private set; }
        public int TowersDone { get; private set; }
        private DSW2Config _config;
        private List<Actor> _towers = new();
        private ulong _preyTargets;
        private ulong _activeTowersMask; // index: bits 0-1 are tower type (00 = 'inner', 01 = 'outer CCW', 10 = 'outer cardinal', 11 = 'outer CW'), bits 2-3 are quadrant index (for inner: NW-NE-SE-SW, for outer: N-E-S-W)
        private bool _preyOnTH;
        private bool _preyGoCCW;
        private bool _preyGoEW;
        private QuadrantAssignments _assignedQuadrants;
        private string _preySwap = "none";
        private ulong _assignedTowers; // [i * 8 + j] = whether tower j is assigned to player in slot i

        private static float _towerRadius = 3;
        private static float _stormRadius = 7;
        private static float _stormPlacementOffset = 9;

        public P2SanctityOfTheWard2()
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (movementHints != null)
            {
                var from = actor.Position;
                var color = module.Arena.ColorSafe;
                if (!StormDone)
                {
                    var stormPos = StormPlacementPosition(module, _assignedQuadrants[slot]);
                    movementHints.Add(from, stormPos, color);
                    from = stormPos;
                    color = module.Arena.ColorDanger;
                }
                if (_towers.Count == 8 && TowersDone < 8)
                {
                    var assignments = BitVector.ExtractVectorFromMatrix8x8(_assignedTowers, slot);
                    for (int i = 0; i < _towers.Count; ++i)
                        if (BitVector.IsVector8BitSet(assignments, i))
                            movementHints.Add(from, _towers[i].CastInfo!.Location, color);
                }
            }
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (_activeTowersMask != 0 && TowersDone < 8)
            {
                //hints.Add($"Prey: {(_preyOnTH ? "T/H" : "DD")} {(_preyGoEW ? "E/W" : "N/S")} {(_preyGoCCW ? "counterclockwise" : "clockwise")}");
                hints.Add($"Prey: {(_preyOnTH ? "T/H" : "DD")}, swap {_preySwap}, {(_preyGoCCW ? "counterclockwise" : "clockwise")}");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddLine(module.Arena.WorldNW, module.Arena.WorldSE, arena.ColorBorder);
            arena.AddLine(module.Arena.WorldNE, module.Arena.WorldSW, arena.ColorBorder);

            foreach (var (slot, player) in module.Raid.WithSlot())
                arena.Actor(player, BitVector.IsVector64BitSet(_preyTargets, slot) ? arena.ColorDanger : arena.ColorPlayerGeneric);

            if (_towers.Count == 8 && TowersDone < 8)
            {
                var assignments = BitVector.ExtractVectorFromMatrix8x8(_assignedTowers, pcSlot);
                for (int i = 0; i < _towers.Count; ++i)
                {
                    if (BitVector.IsVector8BitSet(assignments, i))
                        arena.AddCircle(_towers[i].CastInfo!.Location, _towerRadius, arena.ColorSafe, 2);
                    else
                        arena.AddCircle(_towers[i].CastInfo!.Location, _towerRadius, arena.ColorDanger, 1);
                }
            }

            if (!StormDone)
                arena.AddCircle(pc.Position, _stormRadius, arena.ColorDanger);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.Conviction2AOE:
                    _towers.Add(actor);
                    InitAssignments(module);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.Conviction2AOE:
                    _towers.Remove(actor);
                    ++TowersDone;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.HiemalStormAOE))
                StormDone = true;
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Prey)
            {
                int slot = module.Raid.FindSlot(actorID);
                if (slot >= 0)
                {
                    _preyOnTH = module.Raid[slot]!.Role is Role.Tank or Role.Healer;
                    BitVector.SetVector64Bit(ref _preyTargets, slot);
                }
                InitAssignments(module);
            }
        }

        private void InitAssignments(BossModule module)
        {
            if (_towers.Count != 8 || BitOperations.PopCount(_preyTargets) != 2)
                return; // not ready yet...

            foreach (var tower in _towers)
            {
                BitVector.SetVector64Bit(ref _activeTowersMask, ClassifyTower(module, tower));
            }

            _preyGoEW = _config.P2Sanctity2PreferEWPrey;
            int scoreCW = ScoreForAssignment(_preyGoEW, false);
            int scoreCCW = ScoreForAssignment(_preyGoEW, true);
            if (scoreCW == 0 && scoreCCW == 0) // TODO: if allowed by config...
            {
                _preyGoEW = !_preyGoEW;
                scoreCW = ScoreForAssignment(_preyGoEW, false);
                scoreCCW = ScoreForAssignment(_preyGoEW, true);
            }
            _preyGoCCW = scoreCCW > scoreCW;

            // the rest is only done if we have proper group assignments
            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(module.WorldState.Party);
            if (_config.P2Sanctity2Pairs.Validate() && assignments.Length == _config.P2Sanctity2Pairs.Assignments.Length)
            {
                // assign cardinal bait positions: start with assigned groups, swap prey role to desired cardinals
                for (int i = 0; i < assignments.Length; ++i)
                {
                    var quardant = _config.P2Sanctity2Pairs.Assignments[i];
                    if ((quardant & 1) != 0) // W=1, E=3
                        quardant ^= 2; // ... is swapped in assignments and in component (because I want sane defaults match my static assignments ...)
                    _assignedQuadrants[assignments[i]] = quardant;
                }

                int[] preySlotsPerQuadrant = FindSlotsForRole(module, true);
                int preyQ1 = _preyGoEW ? 1 : 0;
                int preyQ2 = preyQ1 + 2;
                bool slot1Swaps = !BitVector.IsVector64BitSet(_preyTargets, preySlotsPerQuadrant[preyQ1]);
                bool slot2Swaps = !BitVector.IsVector64BitSet(_preyTargets, preySlotsPerQuadrant[preyQ2]);
                if (slot1Swaps && slot2Swaps)
                {
                    // both prey markers at wrong cardinals
                    if (_config.P2Sanctity2SwapCCW != _preyGoEW)
                    {
                        // CCW + N/S, preys are at E/W -> W swaps with S, E with N
                        // CW + E/W, preys are at N/S -> N swaps with E, S with W
                        _assignedQuadrants[preySlotsPerQuadrant[0]] = 1;
                        _assignedQuadrants[preySlotsPerQuadrant[1]] = 0;
                        _assignedQuadrants[preySlotsPerQuadrant[2]] = 3;
                        _assignedQuadrants[preySlotsPerQuadrant[3]] = 2;
                    }
                    else
                    {
                        // CW + N/S, preys are at E/W -> W swaps with N, E with S
                        // CCW + E/W, preys are at N/S -> N swaps with W, S with E
                        _assignedQuadrants[preySlotsPerQuadrant[0]] = 3;
                        _assignedQuadrants[preySlotsPerQuadrant[1]] = 2;
                        _assignedQuadrants[preySlotsPerQuadrant[2]] = 1;
                        _assignedQuadrants[preySlotsPerQuadrant[3]] = 0;
                    }
                    _preySwap = "both";
                }
                else if (slot1Swaps || slot2Swaps)
                {
                    int swapQ1 = slot1Swaps ? preyQ1 : preyQ2;
                    int swapQ2 = _preyGoEW ? 0 : 1;
                    if (!BitVector.IsVector64BitSet(_preyTargets, preySlotsPerQuadrant[swapQ2]))
                        swapQ2 += 2;
                    _assignedQuadrants[preySlotsPerQuadrant[swapQ1]] = swapQ2;
                    _assignedQuadrants[preySlotsPerQuadrant[swapQ2]] = swapQ1;
                    _preySwap = $"{WaymarkForQuadrant(module, swapQ1)}/{WaymarkForQuadrant(module, swapQ2)}";
                }

                // assign towers to prey role
                var unassignedTowers = _activeTowersMask;
                foreach (int preySlot in preySlotsPerQuadrant)
                {
                    int quadrant = _assignedQuadrants[preySlot];
                    int tower = SelectOuterTower(quadrant, _preyGoCCW);
                    AssignTower(module, preySlot, tower);
                    BitVector.ClearVector64Bit(ref unassignedTowers, tower);
                }

                // assign towers to non-prey roles: first assign two outer towers
                var nonPreySlotsPerQuadrant = FindSlotsForRole(module, false);
                int numInnerTowers = 0;
                for (int i = 0; i < nonPreySlotsPerQuadrant.Length; ++i)
                {
                    int slot = nonPreySlotsPerQuadrant[i];
                    int tower = SelectOuterTower(i, !_preyGoCCW);
                    if (BitVector.IsVector64BitSet(unassignedTowers, tower))
                    {
                        AssignTower(module, slot, tower);
                        BitVector.ClearVector64Bit(ref unassignedTowers, tower);
                        nonPreySlotsPerQuadrant[i] = -1;
                    }
                    else
                    {
                        ++numInnerTowers;
                    }
                }

                // assign inner towers, if it can be done non-ambiguously
                bool tryAssignInner = numInnerTowers > 0;
                while (tryAssignInner)
                {
                    tryAssignInner = false;

                    int unambiguousInnerTower = -1;
                    int unambiguousQuadrant = -1;
                    for (int i = 0; i < nonPreySlotsPerQuadrant.Length; ++i)
                    {
                        int slot = nonPreySlotsPerQuadrant[i];
                        if (slot == -1)
                            continue;

                        int potential = FindUnassignedUnambiguousInnerTower(unassignedTowers, i);
                        if (potential == -1)
                            continue; // this quadrant has 2 or 0 unassigned inner towers

                        if (unambiguousInnerTower == -1)
                        {
                            // new potential assignment
                            unambiguousInnerTower = potential;
                            unambiguousQuadrant = i;
                        }
                        else if (unambiguousInnerTower == potential)
                        {
                            // we have two quadrants that have 1 common inner tower, this is a bad pattern...
                            unambiguousInnerTower = -1;
                            break; 
                        }
                        // else: ignore this tower on this iteration...
                    }

                    if (unambiguousInnerTower != -1)
                    {
                        AssignTower(module, nonPreySlotsPerQuadrant[unambiguousQuadrant], unambiguousInnerTower);
                        BitVector.ClearVector64Bit(ref unassignedTowers, unambiguousInnerTower);
                        nonPreySlotsPerQuadrant[unambiguousQuadrant] = -1;
                        --numInnerTowers;
                        tryAssignInner = true; // try assigning more towers on next iteration...
                    }
                }

                // if we still have unassigned towers, assign each of them to each remaining player
                while (unassignedTowers != 0)
                {
                    int nextAmbiguousTower = BitOperations.TrailingZeroCount(unassignedTowers);
                    module.ReportError(this, $"Failed to assign inner tower {nextAmbiguousTower}");
                    foreach (int slot in nonPreySlotsPerQuadrant)
                        if (slot != -1)
                            AssignTower(module, slot, nextAmbiguousTower);
                    BitVector.ClearVector64Bit(ref unassignedTowers, nextAmbiguousTower);
                }
            }
        }

        // 'score' depends on angle between preys: 0 for 120, 1 for 150, 2 for 180
        private int ScoreForAssignment(bool ew, bool ccw)
        {
            float dir1 = DirectionForOuterTower(ew ? 1 : 0, ccw);
            float dir2 = DirectionForOuterTower(ew ? 3 : 2, ccw);
            return MathF.Cos(dir1 - dir2) switch
            {
                < -0.9f => 2,
                < -0.6f => 1,
                _ => 0
            };
        }

        private int ClassifyTower(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Arena.WorldCenter;
            var dir = GeometryUtils.DirectionFromVec3(offset);
            int index;
            if (offset.LengthSquared() < 7 * 7)
            {
                // inner tower: intercardinal, ~6m from center
                index = dir > 0 ? (dir > MathF.PI / 2 ? 1 : 2) : (dir < -MathF.PI / 2 ? 0 : 3);
                index <<= 2;
            }
            else
            {
                // outer tower: ~18m from center, at cardinal or +- 30 degrees
                index = (7 - (int)MathF.Round(dir / MathF.PI * 6)) % 12;
                index = ((index / 3) << 2) | ((index % 3) + 1);
            }
            return index;
        }

        private int SelectOuterTower(int quadrant, bool ccw)
        {
            ulong relevantTowers = 0b1110ul << (quadrant * 4);
            ulong outerTowersMask = _activeTowersMask & relevantTowers;
            return ccw ? BitOperations.TrailingZeroCount(outerTowersMask) : 63 - BitOperations.LeadingZeroCount(outerTowersMask);
        }

        private float DirectionForOuterTower(int quadrant, bool ccw)
        {
            int tower = SelectOuterTower(quadrant, ccw);
            float dir = MathF.PI - quadrant * MathF.PI / 2;
            int idx = tower - quadrant * 4 - 2;
            return dir + idx * MathF.PI / 6;
        }

        private int[] FindSlotsForRole(BossModule module, bool preyRole)
        {
            int[] slots = { -1, -1, -1, -1 };
            foreach (var (slot, player) in module.Raid.WithSlot(true))
            {
                bool isTH = player.Role is Role.Tank or Role.Healer;
                bool isPrey = isTH == _preyOnTH;
                if (isPrey == preyRole)
                    slots[_assignedQuadrants[slot]] = slot;
            }
            return slots;
        }

        private int FindUnassignedUnambiguousInnerTower(ulong available, int quadrant)
        {
            int candidate1 = quadrant;
            int candidate2 = (quadrant + 1) & 3;
            int index1 = candidate1 << 2;
            int index2 = candidate2 << 2;
            bool available1 = BitVector.IsVector64BitSet(available, index1);
            bool available2 = BitVector.IsVector64BitSet(available, index2);
            if (available1 == available2)
                return -1;
            else
                return available1 ? index1 : index2;
        }

        private void AssignTower(BossModule module, int slot, int tower)
        {
            int towerIndex = _towers.FindIndex(t => ClassifyTower(module, t) == tower);
            BitVector.SetMatrix8x8Bit(ref _assignedTowers, slot, towerIndex, true);
        }

        private Vector3 StormPlacementPosition(BossModule module, int quadrant)
        {
            float dir = MathF.PI - quadrant * MathF.PI / 2;
            return module.Arena.WorldCenter + _stormPlacementOffset * GeometryUtils.DirectionToVec3(dir);
        }

        private string WaymarkForQuadrant(BossModule module, int quadrant)
        {
            var pos = StormPlacementPosition(module, quadrant);

            Waymark closest = Waymark.Count;
            float closestD = float.MaxValue;

            for (int i = 0; i < (int)Waymark.Count; ++i)
            {
                var w = (Waymark)i;
                var p = module.WorldState.Waymarks[w];
                float d = p != null ? (pos - p.Value).LengthSquared() : float.MaxValue;
                if (d < closestD)
                {
                    closest = w;
                    closestD = d;
                }
            }
            return closest < Waymark.Count ? closest.ToString() : "-";
        }
    }
}
