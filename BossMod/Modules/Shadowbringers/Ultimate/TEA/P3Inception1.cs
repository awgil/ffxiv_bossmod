using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P3Inception1 : Components.CastCounter
    {
        private List<Actor> _plasmaspheres = new();
        private Actor?[] _tetherSources = new Actor?[PartyState.MaxPartySize];
        private WPos[] _assignedPositions = new WPos[PartyState.MaxPartySize];

        public bool AllSpheresSpawned => _plasmaspheres.Count == 4;
        public bool CrystalsDone => NumCasts > 0;

        private static float _crystalRadius = 5;
        private static float _sphereRadius = 6;

        public P3Inception1() : base(ActionID.MakeSpell(AID.JudgmentCrystalAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!AllSpheresSpawned)
                return;

            var sphere = _tetherSources[slot];
            if (sphere != null)
            {
                if (!sphere.IsDead && module.Raid.WithSlot(true).WhereSlot(s => _tetherSources[s] != null).InRadiusExcluding(actor, _sphereRadius * 2).Any())
                    hints.Add("GTFO from other tethers!");
            }
            else if (!CrystalsDone)
            {
                if (module.Raid.WithSlot(true).WhereSlot(s => _tetherSources[s] == null).InRadiusExcluding(actor, _crystalRadius * 2).Any())
                    hints.Add("GTFO from other crystals!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!AllSpheresSpawned)
                return;

            foreach (var (slot, player) in module.Raid.WithSlot(true))
            {
                var sphere = _tetherSources[slot];
                if (sphere != null)
                {
                    if (!sphere.IsDead)
                    {
                        arena.Actor(sphere, ArenaColor.Object, true);
                        arena.AddLine(sphere.Position, player.Position, slot == pcSlot ? ArenaColor.Safe : ArenaColor.Danger);
                        arena.AddCircle(player.Position, _sphereRadius, ArenaColor.Danger);
                    }
                }
                else if (!CrystalsDone)
                {
                    arena.AddCircle(player.Position, _crystalRadius, ArenaColor.Danger);
                }
            }

            var pcSphere = _tetherSources[pcSlot];
            if (pcSphere != null)
            {
                if (!pcSphere.IsDead)
                {
                    arena.AddCircle(_assignedPositions[pcSlot], 1, ArenaColor.Safe);
                }
            }
            else if (!CrystalsDone)
            {
                arena.AddCircle(_assignedPositions[pcSlot] + new WDir(-5, -5), 1, ArenaColor.Safe);
                arena.AddCircle(_assignedPositions[pcSlot] + new WDir(-5, +5), 1, ArenaColor.Safe);
                arena.AddCircle(_assignedPositions[pcSlot] + new WDir(+5, -5), 1, ArenaColor.Safe);
                arena.AddCircle(_assignedPositions[pcSlot] + new WDir(+5, +5), 1, ArenaColor.Safe);
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Plasmasphere && (OID)source.OID == OID.Plasmasphere)
            {
                _plasmaspheres.Add(source);
                var slot = module.Raid.FindSlot(tether.Target);
                if (slot >= 0)
                    _tetherSources[slot] = source;

                if (AllSpheresSpawned)
                    InitAssignments(module);
            }
        }

        private void InitAssignments(BossModule module)
        {
            // alex is either at N or S cardinal; 2 spheres are E and 2 spheres are W
            // for tethered player, assign 45-degree spot on alex's side, as far away from source as possible
            bool alexNorth = ((TEA)module).AlexPrime()?.Position.Z < module.Bounds.Center.Z;
            var boxPos = module.Bounds.Center + new WDir(0, alexNorth ? 13 : -13);
            for (int slot = 0; slot < _tetherSources.Length; ++slot)
            {
                var sphere = _tetherSources[slot];
                if (sphere != null)
                {
                    var sphereWest = sphere.Position.X < module.Bounds.Center.X;
                    var sameSideSphere = _plasmaspheres.Find(o => o != sphere && (o.Position.X < module.Bounds.Center.X) == sphereWest);
                    var sphereNorth = sphere.Position.Z < sameSideSphere?.Position.Z;

                    var spotDir = alexNorth ? (sphereNorth ? 90.Degrees() : 135.Degrees()) : (sphereNorth ? 45.Degrees() : 90.Degrees());
                    if (!sphereWest)
                        spotDir = -spotDir;
                    _assignedPositions[slot] = module.Bounds.Center + 18 * spotDir.ToDirection();
                }
                else
                {
                    // TODO: consider assigning concrete spots...
                    _assignedPositions[slot] = boxPos;
                }
            }
        }
    }
}
