using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un1Ultima
{
    // common mechanics that are used for entire fight
    // TODO: consider splitting into multiple components, at least for mechanics that start in later phases...
    class Mechanics : BossComponent
    {
        private int[] _tankStacks = new int[PartyState.MaxPartySize];

        private HashSet<ulong> _orbsSharedExploded = new();
        // TODO: think how to associate kiters with orbs
        private HashSet<ulong> _orbsKitedExploded = new();
        private List<ulong> _orbKiters = new();

        private Angle? _magitekOffset;

        private static AOEShapeCircle _aoeCleave = new(2);
        private static AOEShapeCone _aoeDiffractive = new(12, 60.Degrees());
        private static AOEShapeRect _aoeAssaultCannon = new(45, 1);
        private static AOEShapeRect _aoeMagitekRay = new(40, 3);
        //private static float _homingLasersRange = 4;
        //private static float _ceruleumVentRange = 8;
        private static float _orbSharedRange = 8;
        private static float _orbFixateRange = 6;

        public override void Update(BossModule module)
        {
            // TODO: this is bad, we need to find a way to associate orb to kiter...
            if (_orbKiters.Count > 0 && module.Enemies(OID.Aetheroplasm).Count == 0)
                _orbKiters.Clear();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var mtSlot = module.WorldState.Party.FindSlot(module.PrimaryActor.TargetID);
            if (actor.Role == Role.Tank)
            {
                if (module.PrimaryActor.TargetID == actor.InstanceID)
                {
                    if (_tankStacks[slot] >= 4)
                        hints.Add("Pass aggro to co-tank!");
                }
                else
                {
                    if (mtSlot >= 0 && _tankStacks[mtSlot] >= 4)
                        hints.Add("Taunt boss!");
                }
            }

            var mt = module.WorldState.Party[mtSlot];
            if (slot != mtSlot && mt != null && (_aoeCleave.Check(actor.Position, mt) || _aoeDiffractive.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(mt.Position - module.PrimaryActor.Position))))
            {
                hints.Add("GTFO from tank!");
            }

            // TODO: reconsider whether we really care about spread for vents/lasers...
            //if (actor.Role is Role.Healer or Role.Ranged && GeometryUtils.InCircle(actor.Position - module.PrimaryActor.Position, _ceruleumVentRange))
            //{
            //    hints.Add("Move from boss");
            //}

            //if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _homingLasersRange).Any())
            //{
            //    hints.Add("Spread");
            //}

            if (_magitekOffset != null && _aoeMagitekRay.Check(actor.Position, module.PrimaryActor.Position, module.PrimaryActor.Rotation + _magitekOffset.Value))
            {
                hints.Add("GTFO from ray aoe!");
            }

            if (_orbKiters.Contains(actor.InstanceID))
            {
                hints.Add("Kite the orb!");
            }

            if (module.Enemies(OID.MagitekBit).Any(bit => bit.CastInfo != null && _aoeAssaultCannon.Check(actor.Position, bit)))
            {
                hints.Add("GTFO from bit aoe!");
            }

            // TODO: large detonations
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_magitekOffset != null)
                _aoeMagitekRay.Draw(arena, module.PrimaryActor.Position, module.PrimaryActor.Rotation + _magitekOffset.Value);

            foreach (var bit in module.Enemies(OID.MagitekBit).Where(bit => bit.CastInfo != null))
                _aoeAssaultCannon.Draw(arena, bit);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var mt = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(player, _orbKiters.Contains(player.InstanceID) ? ArenaColor.Danger : player == mt ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            if (mt != null)
                arena.AddCircle(mt.Position, _aoeCleave.Radius, ArenaColor.Danger);

            //if (pc.Role is Role.Healer or Role.Ranged)
            //    arena.AddCircle(module.PrimaryActor.Position, _ceruleumVentRange, ArenaColor.Danger);

            foreach (var orb in module.Enemies(OID.Ultimaplasm).Where(orb => !_orbsSharedExploded.Contains(orb.InstanceID)))
            {
                // TODO: line between paired orbs
                arena.Actor(orb, ArenaColor.Danger, true);
                arena.AddCircle(orb.Position, _orbSharedRange, ArenaColor.Safe);
            }

            foreach (var orb in module.Enemies(OID.Aetheroplasm).Where(orb => !_orbsKitedExploded.Contains(orb.InstanceID)))
            {
                // TODO: line from corresponding target
                arena.Actor(orb, ArenaColor.Danger, true);
                arena.AddCircle(orb.Position, _orbFixateRange, ArenaColor.Danger);
            }

            foreach (var bit in module.Enemies(OID.MagitekBit))
            {
                arena.Actor(bit, ArenaColor.Danger);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ViscousAetheroplasm)
                SetTankStacks(module, actor, status.Extra);
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ViscousAetheroplasm)
                SetTankStacks(module, actor, 0);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            Angle? ray = (AID)spell.Action.ID switch
            {
                AID.MagitekRayCenter => 0.Degrees(),
                AID.MagitekRayLeft => 45.Degrees(),
                AID.MagitekRayRight => -45.Degrees(),
                _ => null
            };
            if (ray == null)
                return;
            if (_magitekOffset != null)
                module.ReportError(this, "Several concurrent magitek rays");
            _magitekOffset = ray.Value;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.MagitekRayCenter or AID.MagitekRayLeft or AID.MagitekRayRight)
                _magitekOffset = null;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AetheroplasmBoom:
                    _orbsSharedExploded.Add(caster.InstanceID);
                    break;
                case AID.AetheroplasmFixated:
                    _orbsKitedExploded.Add(caster.InstanceID);
                    break;
                case AID.OrbFixate:
                    _orbKiters.Add(spell.MainTargetID);
                    break;
            }
        }

        private void SetTankStacks(BossModule module, Actor actor, int stacks)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _tankStacks[slot] = stacks;
        }
    }
}
