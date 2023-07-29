using System;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6MortalVow : Components.UniformStackSpread
    {
        public int Progress { get; private set; } // 0 before application, N before Nth pass
        private DSW2Config _config;
        private Actor? _vow;
        private Actor? _target;
        private DateTime _vowExpiration;
        private DateTime[] _atonementExpiration = new DateTime[PartyState.MaxPartySize];

        public P6MortalVow() : base(5, 5, 2, 2, true, false)
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public void ShowNextPass(BossModule module)
        {
            if (_vow == null)
                return;
            _target = DetermineNextPassTarget(module);
            var forbidden = _target != null ? module.Raid.WithSlot(true).Exclude(_target).Mask() : module.Raid.WithSlot(true).WhereSlot(i => _atonementExpiration[i] < _vowExpiration).Mask();
            AddStack(_vow, _vowExpiration, forbidden);
        }

        public override void Init(BossModule module)
        {
            // prepare for initial application on random DD
            AddSpreads(module.Raid.WithoutSlot(true).Where(p => p.Class.IsDD())); // TODO: activation
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (movementHints != null && _vow != null && _target != null && (actor == _vow || actor == _target))
                movementHints.Add(actor.Position, (actor == _vow ? _target : _vow).Position, ArenaColor.Safe);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            if (_vow != null && _target != null && (pc == _vow || pc == _target))
                arena.AddCircle(module.Bounds.Center, 1, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.MortalVow:
                    _vow = actor;
                    _vowExpiration = status.ExpireAt;
                    break;
                case SID.MortalAtonement:
                    var slot = module.Raid.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        _atonementExpiration[slot] = status.ExpireAt;
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.MortalVow:
                    if (_vow == actor)
                        _vow = null;
                    break;
                case SID.MortalAtonement:
                    var slot = module.Raid.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        _atonementExpiration[slot] = default;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.MortalVowApply or AID.MortalVowPass)
            {
                ++Progress;
                Spreads.Clear();
                Stacks.Clear();
                _target = null;
            }
        }

        private Actor? DetermineNextPassTarget(BossModule module)
        {
            if (_config.P6MortalVowOrder == DSW2Config.P6MortalVow.None)
                return null;

            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(module.Raid);
            if (assignments.Length == 0)
                return null; // if assignments are unset, we can't define pass priority

            var role = Progress switch
            {
                1 => PartyRolesConfig.Assignment.MT,
                2 => PartyRolesConfig.Assignment.OT,
                3 => CanPassVowTo(assignments, PartyRolesConfig.Assignment.M1) ? PartyRolesConfig.Assignment.M1 : PartyRolesConfig.Assignment.M2,
                4 => _config.P6MortalVowOrder == DSW2Config.P6MortalVow.TanksMeleeR1 ? PartyRolesConfig.Assignment.R1 : PartyRolesConfig.Assignment.R2,
                _ => PartyRolesConfig.Assignment.Unassigned
            };
            return role != PartyRolesConfig.Assignment.Unassigned && CanPassVowTo(assignments, role) ? module.Raid[assignments[(int)role]] : null;
        }

        private bool CanPassVowTo(int[] assignments, PartyRolesConfig.Assignment role) => _atonementExpiration[assignments[(int)role]] < _vowExpiration;
    }
}
