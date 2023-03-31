using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // leap (icons spread) + rage (rest stack)
    // note: we currently don't show stack hints, that happens automatically if mechanic is resolved properly
    // TODO: figure out rage target - it is probably a random non-tank non-spread
    class P2StrengthOfTheWard2SpreadStack : Components.UniformStackSpread
    {
        public bool LeapsDone { get; private set; }
        public bool RageDone { get; private set; }
        private Angle _dirToStackPos;

        public P2StrengthOfTheWard2SpreadStack() : base(8, 24, 5) { }

        public override void Init(BossModule module)
        {
            WDir offset = new();
            foreach (var s in module.Enemies(OID.SerAdelphel))
                offset += s.Position - module.Bounds.Center;
            foreach (var s in module.Enemies(OID.SerJanlenoux))
                offset += s.Position - module.Bounds.Center;
            _dirToStackPos = Angle.FromDirection(offset) + 180.Degrees();
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            // draw safe spots
            bool pcIsLeapTarget = IsSpreadTarget(pc);
            if (pcIsLeapTarget && !LeapsDone)
            {
                // TODO: select single safe spot for a player based on some criterion...
                DrawSafeSpot(arena, _dirToStackPos + 90.Degrees());
                DrawSafeSpot(arena, _dirToStackPos + 180.Degrees());
                DrawSafeSpot(arena, _dirToStackPos - 90.Degrees());
            }
            if (!pcIsLeapTarget && !RageDone)
            {
                DrawSafeSpot(arena, _dirToStackPos);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.SkywardLeap:
                    LeapsDone = true;
                    Spreads.Clear();
                    break;
                case AID.DragonsRageAOE:
                    RageDone = true;
                    Stacks.Clear();
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID == IconID.SkywardLeap)
                AddSpread(actor);
        }

        private void DrawSafeSpot(MiniArena arena, Angle dir)
        {
            arena.AddCircle(arena.Bounds.Center + 20 * dir.ToDirection(), 2, ArenaColor.Safe);
        }
    }

    // growing voidzones
    class P2StrengthOfTheWard2Voidzones : Components.LocationTargetedAOEs
    {
        public P2StrengthOfTheWard2Voidzones() : base(ActionID.MakeSpell(AID.DimensionalCollapseAOE), 9, "GTFO from voidzone aoe!") { }
    }

    // charges on tethered targets
    class P2StrengthOfTheWard2Charges : Components.CastCounter
    {
        private List<Actor> _chargeSources = new();

        private static float _chargeHalfWidth = 4;

        public P2StrengthOfTheWard2Charges() : base(ActionID.MakeSpell(AID.HolyShieldBash)) { }

        public override void Init(BossModule module)
        {
            _chargeSources.AddRange(module.Enemies(OID.SerAdelphel));
            _chargeSources.AddRange(module.Enemies(OID.SerJanlenoux));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumCasts > 0)
                return;

            var tetherSource = _chargeSources.Find(s => s.Tether.Target == actor.InstanceID);
            if (actor.Role == Role.Tank)
            {
                if (tetherSource == null)
                    hints.Add("Grab tether!");
                else if (ChargeHitsNonTanks(module, tetherSource, actor))
                    hints.Add("Move away from raid!");
            }
            else
            {
                if (tetherSource != null)
                    hints.Add("Pass tether!");
                else if (IsInChargeAOE(module, actor))
                    hints.Add("GTFO from tanks!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                {
                    arena.ZoneRect(source.Position, target.Position, _chargeHalfWidth, ArenaColor.AOE);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // draw tethers
            foreach (var source in _chargeSources)
            {
                module.Arena.Actor(source, ArenaColor.Enemy, true);
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                    module.Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
            {
                _chargeSources.Remove(caster);
                ++NumCasts;
            }
        }

        private bool ChargeHitsNonTanks(BossModule module, Actor source, Actor target)
        {
            var dir = target.Position - source.Position;
            var len = dir.Length();
            dir /= len;
            return module.Raid.WithoutSlot().Any(p => p.Role != Role.Tank && p.Position.InRect(source.Position, dir, len, 0, _chargeHalfWidth));
        }

        private bool IsInChargeAOE(BossModule module, Actor player)
        {
            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null && player.Position.InRect(source.Position, target.Position - source.Position, _chargeHalfWidth))
                    return true;
            }
            return false;
        }
    }

    // towers
    // TODO: assign tower to proper player
    class P2StrengthOfTheWard2Towers : Components.CastCounter
    {
        private List<Actor> _towers = new();

        private static float _towerRadius = 3;

        public P2StrengthOfTheWard2Towers() : base(ActionID.MakeSpell(AID.Conviction1AOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_towers.Count > 0 && actor.Role != Role.Tank)
                hints.Add("Soak tower", !_towers.InRadius(actor.Position, _towerRadius).Any());
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in _towers)
                arena.AddCircle(t.CastInfo!.LocXZ, _towerRadius, ArenaColor.Safe);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _towers.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _towers.Remove(caster);
        }
    }
}
