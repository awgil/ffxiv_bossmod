using System;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2SanctityOfTheWard2HeavensStakeCircles : Components.LocationTargetedAOEs
    {
        public P2SanctityOfTheWard2HeavensStakeCircles() : base(ActionID.MakeSpell(AID.HeavensStakeAOE), 7) { }
    }

    class P2SanctityOfTheWard2HeavensStakeDonut : Components.SelfTargetedAOEs
    {
        public P2SanctityOfTheWard2HeavensStakeDonut() : base(ActionID.MakeSpell(AID.HeavensStakeDonut), new AOEShapeDonut(15, 30)) { }
    }

    class P2SanctityOfTheWard2Knockback : Components.KnockbackFromCaster
    {
        public P2SanctityOfTheWard2Knockback() : base(ActionID.MakeSpell(AID.FaithUnmoving), 16) { }
    }

    // this component is about tower assignments, depending on initial assignments, tower positions and prey markers
    // identifiers used by this component:
    // - quadrant: N=0, E=1, S=2, W=3
    // - towers 1: [0,11] are outer towers in CW order, starting from '11 o'clock' (CCW tower of N quadrant); [12,15] are inner towers in CCW order, starting from NE (NE-SE-SW-NW)
    //   so, inner towers for quadrant k are [3*k, 3*k+2]; neighbouring inner are 12+k & 12+(k+3)%4
    // - towers 2: [0,7] - CW order, starting from N
    class P2SanctityOfTheWard2 : BossComponent
    {
        struct PlayerData
        {
            public bool HavePrey;
            public int AssignedQuadrant;
            public BitMask AssignedTowers1; // note: typically we have only 1 assigned tower, but in some cases two players can have two towers assigned to them, since we can't determine reliable priority
            public int AssignedTower2;
        }

        struct QuadrantData
        {
            public int PreySlot;
            public int NonPreySlot;
        }

        struct Tower1Data
        {
            public Actor? Actor; // null if tower is inactive
            public BitMask AssignedPlayers;
        }

        public bool StormDone { get; private set; }
        public int Towers1Done { get; private set; }
        public int Towers2Done { get; private set; }
        private DSW2Config _config;
        private PlayerData[] _players = new PlayerData[PartyState.MaxPartySize];
        private QuadrantData[] _quadrants = new QuadrantData[4];
        private Tower1Data[] _towers1 = new Tower1Data[16];
        private Actor?[] _towers2 = new Actor?[8];
        private int _activeTowers1;
        private int _assignedPreys;
        private bool _preyOnTH;
        private bool _preyGoCCW;
        private bool _preyGoEW;
        private int _preyScore;
        private int _preyLimitedRangeQuadrant = -1; // if score is < 2, index of quadrant that has less than 180 degrees of distance to place comets
        private string _preySwap = "none";

        private static float _towerRadius = 3;
        private static float _stormRadius = 7;
        private static float _stormPlacementOffset = 10;
        private static float _cometLinkRange = 5;

        public P2SanctityOfTheWard2()
        {
            _config = Service.Config.Get<DSW2Config>();
            for (int i = 0; i < _players.Length; ++i)
                _players[i] = new() { AssignedQuadrant = -1, AssignedTower2 = -1 };
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_activeTowers1 != 8 || _assignedPreys != 2)
                return;

            if (_players[slot].HavePrey && _players[slot].AssignedQuadrant == _preyLimitedRangeQuadrant)
            {
                hints.Add("Limited range", false);
            }

            if (movementHints != null && _players[slot].AssignedQuadrant >= 0)
            {
                var from = actor.Position;
                var color = ArenaColor.Safe;
                if (!StormDone)
                {
                    var stormPos = StormPlacementPosition(module, _players[slot].AssignedQuadrant);
                    movementHints.Add(from, stormPos, color);
                    from = stormPos;
                    color = ArenaColor.Danger;
                }

                foreach (var tower in _towers1)
                {
                    if (tower.Actor?.CastInfo != null && tower.AssignedPlayers[slot])
                    {
                        movementHints.Add(from, tower.Actor.CastInfo.LocXZ, color);
                    }
                }
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_activeTowers1 == 8 && _assignedPreys == 2)
            {
                hints.Add($"Prey: {(_preyOnTH ? "T/H" : "DD")}, swap {_preySwap}, {(_preyGoCCW ? "counterclockwise" : "clockwise")} ({_preyScore * 30 + 120} degrees)");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (slot, player) in module.Raid.WithSlot().Exclude(pc))
                arena.Actor(player, _players[slot].HavePrey ? ArenaColor.Danger : ArenaColor.PlayerGeneric);

            if (_activeTowers1 == 8)
            {
                float diag = module.Bounds.HalfSize / 1.414214f;
                arena.AddLine(module.Bounds.Center + new WDir(diag,  diag), module.Bounds.Center - new WDir(diag,  diag), ArenaColor.Border);
                arena.AddLine(module.Bounds.Center + new WDir(diag, -diag), module.Bounds.Center - new WDir(diag, -diag), ArenaColor.Border);

                foreach (var tower in _towers1)
                {
                    if (tower.Actor?.CastInfo != null)
                    {
                        if (tower.AssignedPlayers[pcSlot])
                            arena.AddCircle(tower.Actor.CastInfo.LocXZ, _towerRadius, ArenaColor.Safe, 2);
                        else
                            arena.AddCircle(tower.Actor.CastInfo.LocXZ, _towerRadius, ArenaColor.Danger, 1);
                    }
                }

                if (!StormDone)
                    arena.AddCircle(pc.Position, _stormRadius, ArenaColor.Danger);
            }

            for (int i = 0; i < _towers2.Length; ++i)
            {
                var tower = _towers2[i];
                if (tower?.CastInfo != null)
                {
                    if (_players[pcSlot].AssignedTower2 == i)
                        arena.AddCircle(tower.CastInfo.LocXZ, _towerRadius, ArenaColor.Safe, 2);
                    else
                        arena.AddCircle(tower.CastInfo.LocXZ, _towerRadius, ArenaColor.Danger, 1);
                }
            }

            if (_players[pcSlot].HavePrey)
            {
                foreach (var comet in module.Enemies(OID.HolyComet))
                {
                    arena.Actor(comet, ArenaColor.Object, true);
                    arena.AddCircle(comet.Position, _cometLinkRange, ArenaColor.Object);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Conviction2AOE:
                    int id1 = ClassifyTower1(module, caster);
                    _towers1[id1].Actor = caster;
                    ++_activeTowers1;
                    InitAssignments(module);
                    break;
                case AID.Conviction3AOE:
                    int id2 = ClassifyTower2(module, caster);
                    _towers2[id2] = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Conviction2AOE:
                    --_activeTowers1;
                    ++Towers1Done;
                    break;
                case AID.Conviction3AOE:
                    ++Towers2Done;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.HiemalStormAOE)
                StormDone = true;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Prey)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _preyOnTH = actor.Role is Role.Tank or Role.Healer;
                    _players[slot].HavePrey = true;
                    ++_assignedPreys;
                }
                InitAssignments(module);
            }
        }

        private int ClassifyTower1(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Bounds.Center;
            var dir = Angle.FromDirection(offset);
            if (offset.LengthSq() < 7 * 7)
            {
                // inner tower: intercardinal, ~6m from center
                return 12 + (dir.Rad > 0 ? (dir.Rad > MathF.PI / 2 ? 0 : 1) : (dir.Rad < -MathF.PI / 2 ? 3 : 2));
            }
            else
            {
                // outer tower: ~18m from center, at cardinal or +- 30 degrees
                return (7 - (int)MathF.Round(dir.Rad / MathF.PI * 6)) % 12;
            }
        }

        private int ClassifyTower2(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Bounds.Center;
            var dir = Angle.FromDirection(offset);
            return (4 - (int)MathF.Round(dir.Rad / MathF.PI * 4)) % 8;
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
                InitTowers1(module);
                InitTowers2(module);
            }
            else
            {
                _preySwap = "unconfigured";
            }
        }

        private void InitPreyPositions(BossModule module)
        {
            _preyGoEW = _config.P2Sanctity2PreferEWPrey;
            var (scoreCW, qCW) = ScoreForAssignment(_preyGoEW, false);
            var (scoreCCW, qCCW) = ScoreForAssignment(_preyGoEW, true);
            if (scoreCW == 0 && scoreCCW == 0) // TODO: if allowed by config...
            {
                _preyGoEW = !_preyGoEW;
                (scoreCW, qCW) = ScoreForAssignment(_preyGoEW, false);
                (scoreCCW, qCCW) = ScoreForAssignment(_preyGoEW, true);
            }
            _preyGoCCW = scoreCCW > scoreCW;
            _preyLimitedRangeQuadrant = _preyGoCCW ? qCCW : qCW;
            _preyScore = Math.Max(scoreCCW, scoreCW);
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
                if (_config.P2Sanctity2SwapBothNE)
                {
                    SwapPreyQuadrants(0, 1);
                    SwapPreyQuadrants(2, 3);
                }
                else
                {
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

        private void InitTowers1(BossModule module)
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
                    if (_players[_quadrants[q].NonPreySlot].AssignedTowers1.Any())
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
            var ambiguousQuadrants = _quadrants.Where(q => _players[q.NonPreySlot].AssignedTowers1.None()).ToArray();
            for (int t = 12; t < _towers1.Length; ++t)
            {
                if (_towers1[t].Actor != null && _towers1[t].AssignedPlayers.None())
                {
                    foreach (var q in ambiguousQuadrants)
                    {
                        AssignTower(q.NonPreySlot, t);
                    }
                }
            }
        }

        private void InitTowers2(BossModule module)
        {
            for (int q = 0; q < _quadrants.Length; ++q)
            {
                var quadrant = _quadrants[q];
                _players[quadrant.PreySlot].AssignedTower2 = _players[quadrant.PreySlot].HavePrey ? (2 * q + 4) % 8 : (2 * q);
                _players[quadrant.NonPreySlot].AssignedTower2 = _config.P2Sanctity2NonPreyTowerCW ? (2 * q + 1) : (2 * q + 7) % 8;
            }
        }

        // 'score' depends on angle between preys: 0 for 120, 1 for 150, 2 for 180
        // 'limited quadrant' is -1 if angle is 180, otherwise it is index of the quadrant that has shorter movement range
        private (int, int) ScoreForAssignment(bool ew, bool ccw)
        {
            int q1 = ew ? 1 : 0;
            int q2 = q1 + 2;
            int t1 = SelectOuterTower(q1, ccw);
            int t2 = SelectOuterTower(q2, ccw);
            return (t2 - t1) switch
            {
                4 => (0, ccw ? q2 : q1),
                5 => (1, ccw ? q2 : q1),
                6 => (2, -1),
                7 => (1, ccw ? q1 : q2),
                8 => (0, ccw ? q1 : q2),
                _ => (2, -1) // that's an error, really...
            };
        }

        private int SelectOuterTower(int quadrant, bool ccw)
        {
            int begin = 3 * quadrant, end = begin + 3;
            if (ccw)
            {
                for (int i = begin; i < end; ++i)
                    if (_towers1[i].Actor != null)
                        return i;
            }
            else
            {
                for (int i = end - 1; i >= begin; --i)
                    if (_towers1[i].Actor != null)
                        return i;
            }
            return -1;
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
            bool available1 = _towers1[candidate1].Actor != null && _towers1[candidate1].AssignedPlayers.None();
            bool available2 = _towers1[candidate2].Actor != null && _towers1[candidate2].AssignedPlayers.None();
            if (available1 == available2)
                return -1;
            else
                return available1 ? candidate1 : candidate2;
        }

        private void AssignTower(int slot, int tower)
        {
            _players[slot].AssignedTowers1.Set(tower);
            _towers1[tower].AssignedPlayers.Set(slot);
        }

        private WPos StormPlacementPosition(BossModule module, int quadrant)
        {
            var dir = (180 - quadrant * 90).Degrees();
            return module.Bounds.Center + _stormPlacementOffset * dir.ToDirection();
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
                float d = p != null ? (pos - new WPos(p.Value.XZ())).LengthSq() : float.MaxValue;
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
