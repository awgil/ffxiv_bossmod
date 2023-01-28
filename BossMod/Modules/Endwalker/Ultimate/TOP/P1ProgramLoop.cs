using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P1ProgramLoop : P1CommonAssignments
    {
        public int NumTowersDone { get; private set; }
        public int NumTethersDone { get; private set; }
        private List<Actor> _towers = new();
        private BitMask _tethers;

        private static float _towerRadius = 3;
        private static float _tetherRadius = 15;

        protected override (GroupAssignmentUnique assignment, bool global) Assignments()
        {
            var config = Service.Config.Get<TOPConfig>();
            return (config.P1ProgramLoopAssignments, config.P1ProgramLoopGlobalPriority);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            var order = PlayerStates[slot].Order;
            if (order == 0)
                return;

            var nextTowers = _towers.Skip(NumTowersDone).Take(2);
            var soakingTower = nextTowers.InRadius(actor.Position, _towerRadius).Any();
            if (order == NextTowersOrder())
                hints.Add("Soak next tower", !soakingTower);
            else if (soakingTower)
                hints.Add("GTFO from tower!");

            if (order != NextTethersOrder())
            {
                if (_tethers[slot])
                    hints.Add("Pass the tether!");
                if (module.Raid.WithSlot().IncludedInMask(_tethers).InRadiusExcluding(actor, _tetherRadius).Any())
                    hints.Add("GTFO from tether targets!");
            }
            else if (_tethers.Any())
            {
                if (!_tethers[slot])
                    hints.Add("Grab the tether!");
                else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _tetherRadius).Any())
                    hints.Add("GTFO from raid!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var ps = PlayerStates[pcSlot];
            bool soakTowers = ps.Order == NextTowersOrder();
            var towerToSoak = soakTowers ? SelectTowerForGroup(module, ps.Group) : null;
            foreach (var t in _towers.Skip(NumTowersDone).Take(2))
            {
                arena.AddCircle(t.Position, _towerRadius, soakTowers && (towerToSoak == null || towerToSoak == t) ? ArenaColor.Safe : ArenaColor.Danger, 2);
            }

            if (ps.Order == NextTowersOrder(1))
            {
                // show next tower to soak if possible
                var futureTowerToSoak = SelectTowerForGroup(module, ps.Group, 1);
                if (futureTowerToSoak != null)
                    arena.AddCircle(futureTowerToSoak.Position, _towerRadius, ArenaColor.Safe);
            }

            foreach (var (s, t) in module.Raid.WithSlot().IncludedInMask(_tethers))
            {
                arena.AddCircle(t.Position, _tetherRadius, t == pc ? ArenaColor.Safe : ArenaColor.Danger);
                arena.AddLine(t.Position, module.PrimaryActor.Position, PlayerStates[s].Order == NextTethersOrder() ? ArenaColor.Safe : ArenaColor.Danger);
            }

            if (ps.Order == NextTethersOrder())
            {
                // show hint for tether position
                var spot = GetTetherDropSpot(module, ps.Group);
                if (spot != null)
                    arena.AddCircle(spot.Value, 1, ArenaColor.Safe);
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.Tower2)
                _towers.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.StorageViolation:
                case AID.StorageViolationObliteration:
                    ++NumTowersDone;
                    break;
                case AID.BlasterAOE:
                    ++NumTethersDone;
                    break;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Blaster)
                _tethers.Set(module.Raid.FindSlot(source.InstanceID));
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Blaster)
                _tethers.Clear(module.Raid.FindSlot(source.InstanceID));
        }

        private int NextTowersOrder(int skip = 0)
        {
            var index = NumTowersDone + skip * 2;
            return index < 8 ? (index >> 1) + 1 : 0;
        }

        private int NextTethersOrder(int skip = 0)
        {
            var index = NumTethersDone + skip * 2;
            return index < 8 ? (index >> 1) + (index < 4 ? 3 : -1) : 0;
        }

        // 0 = N, 1 = E, ... (CW)
        private int ClassifyTower(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Bounds.Center;
            if (Math.Abs(offset.Z) > Math.Abs(offset.X))
                return offset.Z < 0 ? 0 : 2;
            else
                return offset.X > 0 ? 1 : 3;
        }

        private Actor? SelectTowerForGroup(BossModule module, int group, int skip = 0)
        {
            var firstIndex = NumTowersDone + skip * 2;
            if (group == 0 || _towers.Count < firstIndex + 2)
                return null;
            var t1 = _towers[firstIndex];
            var t2 = _towers[firstIndex + 1];
            if (ClassifyTower(module, t2) < ClassifyTower(module, t1))
                Utils.Swap(ref t1, ref t2);
            return group == 1 ? t1 : t2;
        }

        private WPos? GetTetherDropSpot(BossModule module, int group)
        {
            if (group == 0 || _towers.Count < NumTowersDone + 2)
                return null;

            var t1 = ClassifyTower(module, _towers[NumTowersDone]);
            var t2 = ClassifyTower(module, _towers[NumTowersDone + 1]);
            var potentialSpots = Enumerable.Range(0, 4);
            if (group == 2)
                potentialSpots = potentialSpots.Reverse();
            var spot = potentialSpots.First(s => s != t1 && s != t2);
            return module.Bounds.Center + 18 * (180.Degrees() - 90.Degrees() * spot).ToDirection();
        }
    }
}
