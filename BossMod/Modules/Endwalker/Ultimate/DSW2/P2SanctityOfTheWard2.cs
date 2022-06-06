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

    class P2SanctityOfTheWard2Knockback : CommonComponents.KnockbackFromCaster
    {
        public P2SanctityOfTheWard2Knockback() : base(ActionID.MakeSpell(AID.FaithUnmoving), 16) { }
    }

    // this component is about tower assignments, depending on initial assignments, tower positions and prey markers
    // identifiers used by this component:
    // - quadrant: N=0, E=1, S=2, W=3
    // - towers: [0,11] are outer towers in CW order, starting from '11 o'clock' (CCW tower of N quadrant); [12,15] are inner towers in CCW order, starting from NE (NE-SE-SW-NW)
    //   so, inner towers for quadrant k are [3*k, 3*k+2]; neighbouring inner are 12+k & 12+(k+3)%4
    // TODO: assignments for second towers...
    class P2SanctityOfTheWard2 : BossModule.Component
    {
        struct PlayerData
        {
            public bool HavePrey;
            public int AssignedQuadrant;
            public ulong AssignedTowers; // note: typically we have only 1 assigned tower, but in some cases two players can have two towers assigned to them, since we can't determine reliable priority
        }

        struct QuadrantData
        {
            public int PreySlot;
            public int NonPreySlot;
        }

        struct TowerData
        {
            public Actor? Actor; // null if tower is inactive
            public ulong AssignedPlayers;
        }

        public bool StormDone { get; private set; }
        public int Towers1Done { get; private set; }
        public int Towers2Done { get; private set; }
        private DSW2Config _config;
        private PlayerData[] _players = new PlayerData[PartyState.MaxSize];
        private QuadrantData[] _quadrants = new QuadrantData[4];
        private TowerData[] _towers = new TowerData[16];
        private int _activeTowers1;
        private int _assignedPreys;
        private bool _preyOnTH;
        private bool _preyGoCCW;
        private bool _preyGoEW;
        private string _preySwap = "none";
        private List<Actor> _towers2 = new();

        private static float _towerRadius = 3;
        private static float _stormRadius = 7;
        private static float _stormPlacementOffset = 10;

        public P2SanctityOfTheWard2()
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_activeTowers1 != 8)
                return;

            if (movementHints != null)
            {
                var from = actor.Position;
                var color = module.Arena.ColorSafe;
                if (!StormDone)
                {
                    var stormPos = StormPlacementPosition(module, _players[slot].AssignedQuadrant);
                    movementHints.Add(from, stormPos, color);
                    from = stormPos;
                    color = module.Arena.ColorDanger;
                }

                foreach (var tower in _towers)
                {
                    if (tower.Actor?.CastInfo != null && BitVector.IsVector64BitSet(tower.AssignedPlayers, slot))
                    {
                        movementHints.Add(from, tower.Actor.CastInfo.Location, color);
                    }
                }
            }
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (_activeTowers1 == 8)
            {
                hints.Add($"Prey: {(_preyOnTH ? "T/H" : "DD")}, swap {_preySwap}, {(_preyGoCCW ? "counterclockwise" : "clockwise")}");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (slot, player) in module.Raid.WithSlot().Exclude(pc))
                arena.Actor(player, _players[slot].HavePrey ? arena.ColorDanger : arena.ColorPlayerGeneric);

            if (_activeTowers1 == 8)
            {
                float diag = arena.WorldHalfSize / 1.414214f;
                arena.AddLine(arena.WorldCenter + new Vector3(diag, 0, diag), arena.WorldCenter - new Vector3(diag, 0, diag), arena.ColorBorder);
                arena.AddLine(arena.WorldCenter + new Vector3(diag, 0, -diag), arena.WorldCenter - new Vector3(diag, 0, -diag), arena.ColorBorder);

                foreach (var tower in _towers)
                {
                    if (tower.Actor?.CastInfo != null)
                    {
                        if (BitVector.IsVector64BitSet(tower.AssignedPlayers, pcSlot))
                            arena.AddCircle(tower.Actor.CastInfo.Location, _towerRadius, arena.ColorSafe, 2);
                        else
                            arena.AddCircle(tower.Actor.CastInfo.Location, _towerRadius, arena.ColorDanger, 1);
                    }
                }

                if (!StormDone)
                    arena.AddCircle(pc.Position, _stormRadius, arena.ColorDanger);
            }

            foreach (var tower in _towers2)
                arena.AddCircle(tower.CastInfo!.Location, _towerRadius, arena.ColorSafe);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.Conviction2AOE:
                    int id = ClassifyTower(module, actor);
                    _towers[id].Actor = actor;
                    ++_activeTowers1;
                    InitAssignments(module);
                    break;
                case AID.Conviction3AOE:
                    _towers2.Add(actor);
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
                    --_activeTowers1;
                    ++Towers1Done;
                    break;
                case AID.Conviction3AOE:
                    _towers2.Remove(actor);
                    ++Towers2Done;
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
                    _players[slot].HavePrey = true;
                    ++_assignedPreys;
                }
                InitAssignments(module);
            }
        }

        private int ClassifyTower(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Arena.WorldCenter;
            var dir = GeometryUtils.DirectionFromVec3(offset);
            if (offset.LengthSquared() < 7 * 7)
            {
                // inner tower: intercardinal, ~6m from center
                return 12 + (dir > 0 ? (dir > MathF.PI / 2 ? 0 : 1) : (dir < -MathF.PI / 2 ? 3 : 2));
            }
            else
            {
                // outer tower: ~18m from center, at cardinal or +- 30 degrees
                return (7 - (int)MathF.Round(dir / MathF.PI * 6)) % 12;
            }
        }

        private void InitAssignments(BossModule module)
        {
            if (_activeTowers1 != 8 || _assignedPreys != 2)
                return; // not ready yet...

            // prey position assignment - can be done even if we don't have valid group assignments
            InitPreyPositions(module);

            // the rest is only done if we have proper group assignments
            // assign cardinal bait positions: start with assigned groups
            if (InitQuadrantAssignments(module))
            {
                // swap prey role to desired cardinals
                InitQuadrantSwaps(module);

                // now assign towers to players
                InitTowers(module);
            }
            else
            {
                _preySwap = "unconfigured";
            }
        }

        private void InitPreyPositions(BossModule module)
        {
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
        }

        private bool InitQuadrantAssignments(BossModule module)
        {
            bool validAssignments = false;
            foreach (var (slot, group) in _config.P2Sanctity2Pairs.Resolve(module.Raid))
            {
                validAssignments = true;
                int quadrant = group;
                if ((quadrant & 1) != 0) // W=1, E=3
                    quadrant ^= 2; // ... is swapped in assignments and in component (because I want sane defaults match my static assignments ...)

                _players[slot].AssignedQuadrant = quadrant;

                bool isTH = module.Raid[slot]!.Role is Role.Tank or Role.Healer;
                if (isTH == _preyOnTH)
                    _quadrants[quadrant].PreySlot = slot;
                else
                    _quadrants[quadrant].NonPreySlot = slot;
            }
            return validAssignments;
        }

        private void InitQuadrantSwaps(BossModule module)
        {
            int preyQ1 = _preyGoEW ? 1 : 0;
            int preyQ2 = preyQ1 + 2;
            bool slot1Swaps = !_players[_quadrants[preyQ1].PreySlot].HavePrey;
            bool slot2Swaps = !_players[_quadrants[preyQ2].PreySlot].HavePrey;
            if (slot1Swaps && slot2Swaps)
            {
                // both prey markers at wrong cardinals
                if (_config.P2Sanctity2SwapCCW != _preyGoEW)
                {
                    // CCW + N/S, preys are at E/W -> W swaps with S, E with N
                    // CW + E/W, preys are at N/S -> N swaps with E, S with W
                    SwapPreyQuadrants(0, 1);
                    SwapPreyQuadrants(2, 3);
                }
                else
                {
                    // CW + N/S, preys are at E/W -> W swaps with N, E with S
                    // CCW + E/W, preys are at N/S -> N swaps with W, S with E
                    SwapPreyQuadrants(0, 3);
                    SwapPreyQuadrants(1, 2);
                }
                _preySwap = "both";
            }
            else if (slot1Swaps || slot2Swaps)
            {
                int swapQ1 = slot1Swaps ? preyQ1 : preyQ2;
                int swapQ2 = _preyGoEW ? 0 : 1;
                if (!_players[_quadrants[swapQ2].PreySlot].HavePrey)
                    swapQ2 += 2;
                SwapPreyQuadrants(swapQ1, swapQ2);
                _preySwap = $"{WaymarkForQuadrant(module, swapQ1)}/{WaymarkForQuadrant(module, swapQ2)}";
            }
            else
            {
                _preySwap = "none";
            }
        }

        private void InitTowers(BossModule module)
        {
            // assign outer towers
            for (int q = 0; q < _quadrants.Length; ++q)
            {
                // first (or only) - to prey role
                var t1 = SelectOuterTower(q, _preyGoCCW);
                AssignTower(_quadrants[q].PreySlot, t1);

                // if there is second one, assign to non-prey role
                var t2 = SelectOuterTower(q, !_preyGoCCW);
                if (t2 != t1)
                {
                    AssignTower(_quadrants[q].NonPreySlot, t2);
                }
            }

            // now assign remaining inner towers, as long as it can be done non-ambiguously
            while (true)
            {
                int unambiguousInnerTower = -1;
                int unambiguousQuadrant = -1;
                for (int q = 0; q < _quadrants.Length; ++q)
                {
                    if (_players[_quadrants[q].NonPreySlot].AssignedTowers != 0)
                        continue;

                    int potential = FindUnassignedUnambiguousInnerTower(q);
                    if (potential == -1)
                        continue; // this quadrant has 2 or 0 unassigned inner towers

                    if (unambiguousInnerTower == -1)
                    {
                        // new potential assignment
                        unambiguousInnerTower = potential;
                        unambiguousQuadrant = q;
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
                    AssignTower(_quadrants[unambiguousQuadrant].NonPreySlot, unambiguousInnerTower);
                else
                    break;
            }

            // if we still have unassigned towers, assign each of them to each remaining player
            var ambiguousQuadrants = _quadrants.Where(q => _players[q.NonPreySlot].AssignedTowers == 0).ToArray();
            for (int t = 12; t < _towers.Length; ++t)
            {
                if (_towers[t].Actor != null && _towers[t].AssignedPlayers == 0)
                {
                    foreach (var q in ambiguousQuadrants)
                    {
                        AssignTower(q.NonPreySlot, t);
                    }
                }
            }
        }

        // 'score' depends on angle between preys: 0 for 120, 1 for 150, 2 for 180
        private int ScoreForAssignment(bool ew, bool ccw)
        {
            float dir1 = DirectionForOuterTower(SelectOuterTower(ew ? 1 : 0, ccw));
            float dir2 = DirectionForOuterTower(SelectOuterTower(ew ? 3 : 2, ccw));
            return MathF.Cos(dir1 - dir2) switch
            {
                < -0.9f => 2,
                < -0.6f => 1,
                _ => 0
            };
        }

        private int SelectOuterTower(int quadrant, bool ccw)
        {
            int begin = 3 * quadrant, end = begin + 3;
            if (ccw)
            {
                for (int i = begin; i < end; ++i)
                    if (_towers[i].Actor != null)
                        return i;
            }
            else
            {
                for (int i = end - 1; i >= begin; --i)
                    if (_towers[i].Actor != null)
                        return i;
            }
            return -1;
        }

        private float DirectionForOuterTower(int tower)
        {
            return (7 - tower) * MathF.PI / 6;
        }

        private void SwapPreyQuadrants(int q1, int q2)
        {
            int s1 = _quadrants[q1].PreySlot;
            int s2 = _quadrants[q2].PreySlot;
            _quadrants[q1].PreySlot = s2;
            _quadrants[q2].PreySlot = s1;
            _players[s1].AssignedQuadrant = q2;
            _players[s2].AssignedQuadrant = q1;
        }

        private int FindUnassignedUnambiguousInnerTower(int quadrant)
        {
            int candidate1 = 12 + quadrant;
            int candidate2 = 12 + ((quadrant + 3) & 3);
            bool available1 = _towers[candidate1].Actor != null && _towers[candidate1].AssignedPlayers == 0;
            bool available2 = _towers[candidate2].Actor != null && _towers[candidate2].AssignedPlayers == 0;
            if (available1 == available2)
                return -1;
            else
                return available1 ? candidate1 : candidate2;
        }

        private void AssignTower(int slot, int tower)
        {
            BitVector.SetVector64Bit(ref _players[slot].AssignedTowers, tower);
            BitVector.SetVector64Bit(ref _towers[tower].AssignedPlayers, slot);
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
