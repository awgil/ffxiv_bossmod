using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // general mechanic tracking
    class Palladion : BossComponent
    {
        public Actor?[] JumpTargets = new Actor?[PartyState.MaxPartySize];
        public Actor?[] Partners = new Actor?[PartyState.MaxPartySize];
        public BitMask BaitOrder; // bit i is set if i'th action is a bait rather than center aoe
        public int NumBaitsAssigned;
        public int NumBaitsDone;
        private Dictionary<ulong, bool> _baitedLight = new(); // key = instance id, value = true if bait, false if center aoe

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var order = Array.IndexOf(JumpTargets, actor);
            if (order >= 0)
                hints.Add($"Order: {order + 1}", false);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var (target, partner) = (IconID)iconID switch
            {
                IconID.Palladion1 => (0, 2),
                IconID.Palladion2 => (1, 3),
                IconID.Palladion3 => (2, 0),
                IconID.Palladion4 => (3, 1),
                IconID.Palladion5 => (4, 6),
                IconID.Palladion6 => (5, 7),
                IconID.Palladion7 => (6, 4),
                IconID.Palladion8 => (7, 5),
                _ => (-1, -1)
            };
            if (target >= 0)
            {
                JumpTargets[target] = actor;
                Partners[partner] = actor;
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Anthropos)
            {
                switch (id)
                {
                    case 0x11D3:
                        _baitedLight[actor.InstanceID] = false;
                        break;
                    case 0x11D4:
                        _baitedLight[actor.InstanceID] = true;
                        break;
                    case 0x1E39: // add jumping away - staggered, 1s apart, order corresponds to center aoe/bait
                        if (_baitedLight.TryGetValue(actor.InstanceID, out var bait))
                            BaitOrder[NumBaitsAssigned++] = bait;
                        break;
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ClearCut or AID.WhiteFlame)
                ++NumBaitsDone;
        }
    }

    // limited arena for limit cuts
    // TODO: reconsider - base activation on env controls, show danger zone instead of border?..
    class PalladionArena : BossComponent
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            for (int i = 0; i < 8; ++i)
                arena.PathLineTo(module.Bounds.Center + 14 * (i * 45).Degrees().ToDirection());
            arena.PathStroke(true, ArenaColor.Border, 2);
        }
    }

    // shockwave is targeted at next jump target; everyone except target and partner should avoid it
    class PalladionShockwave : Components.GenericAOEs
    {
        private Palladion? _palladion;
        private WPos _origin;
        private DateTime _activation;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_palladion != null && NumCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[NumCasts] is var target && target != null && actor != target && actor != _palladion.Partners[NumCasts])
                yield return new(BuildShape(target.Position), _origin, default, _activation);
        }

        public override void Init(BossModule module)
        {
            _palladion = module.FindComponent<Palladion>();
            _origin = module.PrimaryActor.Position; // note: assumed to be activated when cast starts, so boss is in initial jump position; PATE 1E43 happens 1s earlier, but icons only appear right before cast start
            _activation = module.PrimaryActor.CastInfo?.FinishAt.AddSeconds(0.3f) ?? default;

        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_palladion != null && NumCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[NumCasts] is var target && target != null && (pc == target || pc == _palladion.Partners[NumCasts]))
                BuildShape(target.Position).Outline(arena, _origin, default, ArenaColor.Safe);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            // note: shockwave cast happens at exactly the same time, but it targets a player rather than location, making it slightly harder to work with
            if ((AID)spell.Action.ID == AID.PalladionAOE)
            {
                _origin = spell.TargetXZ;
                _activation = module.WorldState.CurrentTime.AddSeconds(3);
                ++NumCasts;
            }
        }

        private AOEShapeRect BuildShape(WPos target)
        {
            var shape = new AOEShapeRect(0, 2);
            shape.SetEndPoint(target, _origin, default);
            return shape;
        }
    }

    class PalladionStack : Components.UniformStackSpread
    {
        private int _numCasts;
        private Palladion? _palladion;

        public PalladionStack() : base(6, 0, raidwideOnResolve: false) { }

        public override void Init(BossModule module)
        {
            _palladion = module.FindComponent<Palladion>();
            UpdateStack(module, module.PrimaryActor.CastInfo?.FinishAt.AddSeconds(0.3f) ?? default);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return IsStackTarget(player) ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.PalladionAOE)
            {
                ++_numCasts;
                UpdateStack(module, module.WorldState.CurrentTime.AddSeconds(3));
            }
        }

        private void UpdateStack(BossModule module, DateTime activation)
        {
            Stacks.Clear();
            if (_palladion != null && _numCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[_numCasts] is var target && target != null)
                AddStack(target, activation, module.Raid.WithSlot(true).Exclude(_palladion.Partners[_numCasts]).Mask());
        }
    }

    class PalladionVoidzone : Components.PersistentVoidzoneAtCastTarget
    {
        public PalladionVoidzone() : base(6, ActionID.MakeSpell(AID.PalladionAOE), m => m.Enemies(OID.PalladionVoidzone).Where(z => z.EventState != 7), 0.9f) { }
    }

    class PalladionClearCut : Components.GenericAOEs
    {
        private Palladion? _palladion;

        private static AOEShapeCircle _shape = new(4); // note: it's really a 270? degree cone, but we don't really know rotation early enough, and we just shouldn't stay in center anyway

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_palladion != null && _palladion.NumBaitsDone < _palladion.NumBaitsAssigned && !_palladion.BaitOrder[_palladion.NumBaitsDone])
                yield return new(_shape, module.Bounds.Center);
        }

        public override void Init(BossModule module)
        {
            _palladion = module.FindComponent<Palladion>();
        }
    }

    // TODO: reconsider - show always, even if next is clear cut?..
    class PalladionWhiteFlame : Components.GenericBaitAway
    {
        private Palladion? _palladion;
        private Actor _fakeSource = new(0, 0, -1, "dummy", ActorType.None, Class.None, new(100, 0, 100, 0)); // fake actor used as bait source

        private static AOEShapeRect _shape = new(100, 2);

        public override void Init(BossModule module)
        {
            _palladion = module.FindComponent<Palladion>();
            UpdateBaiters(module);
        }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_palladion != null && _palladion.NumBaitsDone < _palladion.NumBaitsAssigned && _palladion.BaitOrder[_palladion.NumBaitsDone])
                foreach (var t in module.Raid.WithoutSlot().SortedByRange(module.Bounds.Center).Take(2))
                    CurrentBaits.Add(new(_fakeSource, t, _shape));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumCasts < 4 && !ForbiddenPlayers[slot])
                hints.Add("Bait next aoe", CurrentBaits.Count > 0 && !ActiveBaitsOn(actor).Any());
            base.AddHints(module, slot, actor, hints, movementHints);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (CurrentBaits.Count > 0)
                arena.Actor(module.Bounds.Center, default, ArenaColor.Object);
            base.DrawArenaForeground(module, pcSlot, pc, arena);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.WhiteFlame)
            {
                ++NumCasts;
                UpdateBaiters(module);
            }
        }

        private void UpdateBaiters(BossModule module)
        {
            // TODO: we assume very strict bait order (5/7, 6/8, 1/3, 2/4), this is not strictly required...
            ForbiddenPlayers.Reset();
            if (_palladion != null && NumCasts < 4)
            {
                var (b1, b2) = NumCasts switch
                {
                    0 => (4, 6),
                    1 => (5, 7),
                    2 => (0, 2),
                    _ => (1, 3)
                };
                ForbiddenPlayers = module.Raid.WithSlot(true).Exclude(_palladion.JumpTargets[b1]).Exclude(_palladion.JumpTargets[b2]).Mask();
            }
        }
    }

    class PalladionDestroyPlatforms : Components.GenericAOEs
    {
        private static AOEShapeRect _shape = new(10, 20, 10);

        public PalladionDestroyPlatforms() : base(ActionID.MakeSpell(AID.PalladionDestroyPlatforms), "Go to safe platform!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            yield return new(_shape, module.Bounds.Center);
        }
    }
}
