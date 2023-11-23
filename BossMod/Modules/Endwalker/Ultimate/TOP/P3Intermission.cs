using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P3SniperCannon : Components.UniformStackSpread
    {
        enum PlayerRole { None, Stack, Spread }

        struct PlayerState
        {
            public PlayerRole Role;
            public int Order;
        }

        private TOPConfig _config = Service.Config.Get<TOPConfig>();
        private PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
        private bool _haveSafeSpots;

        public P3SniperCannon() : base(6, 6, alwaysShowSpreads: true) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            foreach (var s in EnumerateSafeSpots(module, pcSlot))
                arena.AddCircle(s, 1, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.SniperCannonFodder:
                    AddSpread(actor, status.ExpireAt);
                    Assign(module, module.Raid.FindSlot(actor.InstanceID), PlayerRole.Spread);
                    break;
                case SID.HighPoweredSniperCannonFodder:
                    AddStack(actor, status.ExpireAt);
                    Assign(module, module.Raid.FindSlot(actor.InstanceID), PlayerRole.Stack);
                    break;
            }
        }

        // note: if player dies, stack/spread immediately hits random target, so we use status loss to end stack/spread
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.SniperCannonFodder:
                    Spreads.RemoveAll(s => s.Target == actor);
                    break;
                case SID.HighPoweredSniperCannonFodder:
                    Stacks.RemoveAll(s => s.Target == actor);
                    break;
            }
        }

        private void Assign(BossModule module, int slot, PlayerRole role)
        {
            if (slot < 0)
                return;
            _playerStates[slot].Role = role;

            if (Spreads.Count < 4 || Stacks.Count < 2)
                return; // too early to build assignments

            _haveSafeSpots = true;

            int[] slotsInPriorityOrder = Utils.MakeArray(PartyState.MaxPartySize, -1);
            foreach (var a in _config.P3IntermissionAssignments.Resolve(module.Raid))
                slotsInPriorityOrder[a.group] = a.slot;

            int[] assignedRoles = { 0, 0, 0 };
            foreach (var s in slotsInPriorityOrder.Where(s => s >= 0))
                _playerStates[s].Order = ++assignedRoles[(int)_playerStates[s].Role];
        }

        private IEnumerable<WPos> EnumerateSafeSpots(BossModule module, int slot)
        {
            if (!_haveSafeSpots)
                yield break;

            var ps = _playerStates[slot];
            if (ps.Role == PlayerRole.Spread)
            {
                if (ps.Order is 0 or 1)
                    yield return SafeSpotAt(module, -90.Degrees());
                if (ps.Order is 0 or 2)
                    yield return SafeSpotAt(module, -45.Degrees());
                if (ps.Order is 0 or 3)
                    yield return SafeSpotAt(module, 45.Degrees());
                if (ps.Order is 0 or 4)
                    yield return SafeSpotAt(module, 90.Degrees());
            }
            else
            {
                if (ps.Order is 0 or 1)
                    yield return SafeSpotAt(module, -135.Degrees());
                if (ps.Order is 0 or 2)
                    yield return SafeSpotAt(module, 135.Degrees());
            }
        }

        private WPos SafeSpotAt(BossModule module, Angle dirIfStacksNorth) => module.Bounds.Center + 19 * (_config.P3IntermissionStacksNorth ? dirIfStacksNorth : 180.Degrees() - dirIfStacksNorth).ToDirection();
    }

    class P3WaveRepeater : Components.ConcentricAOEs
    {
        private static AOEShape[] _shapes = { new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24) };

        public P3WaveRepeater() : base(_shapes) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.WaveRepeater1)
                AddSequence(caster.Position, spell.FinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.WaveRepeater1 => 0,
                AID.WaveRepeater2 => 1,
                AID.WaveRepeater3 => 2,
                AID.WaveRepeater4 => 3,
                _ => -1
            };
            if (!AdvanceSequence(order, caster.Position, module.WorldState.CurrentTime.AddSeconds(2.1f)))
                module.ReportError(this, $"Unexpected ring {order}");
        }
    }

    class P3IntermissionVoidzone : Components.PersistentVoidzone
    {
        public P3IntermissionVoidzone() : base(6, m => m.Enemies(OID.P3IntermissionVoidzone).Where(z => z.EventState != 7)) { }
    }

    class P3ColossalBlow : Components.GenericAOEs
    {
        public List<AOEInstance> AOEs = new();

        private static AOEShapeCircle _shape = new(11);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs.Take(3);

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID is OID.LeftArmUnit or OID.RightArmUnit && id is 0x1E43 or 0x1E44)
                AOEs.Add(new(_shape, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(13.5f)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ColossalBlow)
            {
                ++NumCasts;
                AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            }
        }
    }
}
