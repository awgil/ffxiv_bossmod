using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P3QuickmarchTrio : BossComponent
    {
        private Actor? _relNorth;
        private WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

        public bool Active => _relNorth != null;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(_relNorth, ArenaColor.Object, true);
            var safespot = _safeSpots[pcSlot];
            if (safespot != default)
                arena.AddCircle(safespot, 1, ArenaColor.Safe);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
            {
                _relNorth = actor;
                var dirToNorth = Angle.FromDirection(actor.Position - module.Bounds.Center);
                foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(module.Raid))
                {
                    bool left = p.group < 4;
                    int order = p.group & 3;
                    var offset = (60 + order * 20).Degrees();
                    var dir = dirToNorth + (left ? offset : -offset);
                    _safeSpots[p.slot] = module.Bounds.Center + 20 * dir.ToDirection();
                }
            }
        }
    }

    class P3TwistingDive : Components.SelfTargetedAOEs
    {
        public P3TwistingDive() : base(ActionID.MakeSpell(AID.TwistingDive), new AOEShapeRect(60, 4)) { }
    }

    class P3LunarDive : Components.SelfTargetedAOEs
    {
        public P3LunarDive() : base(ActionID.MakeSpell(AID.LunarDive), new AOEShapeRect(60, 4)) { }
    }

    class P3MegaflareDive : Components.SelfTargetedAOEs
    {
        public P3MegaflareDive() : base(ActionID.MakeSpell(AID.MegaflareDive), new AOEShapeRect(60, 6)) { }
    }

    class P3Twister : Components.ImmediateTwister
    {
        public P3Twister() : base(2, (uint)OID.VoidzoneTwister, 1.4f) { } // TODO: verify radius
    }

    class P3MegaflareSpreadStack : Components.UniformStackSpread
    {
        private BitMask _stackTargets;

        public P3MegaflareSpreadStack() : base(5, 5, 3, 3, alwaysShowSpreads: true) { } // TODO: verify stack radius

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true), module.WorldState.CurrentTime.AddSeconds(2.6f));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.MegaflareStack)
                _stackTargets.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.MegaflareSpread:
                    Spreads.Clear();
                    var stackTarget = module.Raid.WithSlot().IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                    if (stackTarget != null)
                        AddStack(stackTarget, module.WorldState.CurrentTime.AddSeconds(4), ~_stackTargets);
                    break;
                case AID.MegaflareStack:
                    Stacks.Clear();
                    break;
            }
        }
    }

    class P3MegaflarePuddle : Components.LocationTargetedAOEs
    {
        public P3MegaflarePuddle() : base(ActionID.MakeSpell(AID.MegaflarePuddle), 6) { }
    }

    class P3EarthShaker : Components.GenericBaitAway
    {
        private static AOEShapeCone _shape = new(60, 45.Degrees());

        public P3EarthShaker() : base(ActionID.MakeSpell(AID.EarthShakerAOE)) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Earthshaker && module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
                CurrentBaits.Add(new(source, actor, _shape));
        }
    }

    class P3EarthShakerVoidzone : Components.GenericAOEs
    {
        private IReadOnlyList<Actor> _voidzones = ActorEnumeration.EmptyList;
        private List<AOEInstance> _predicted = new();
        private BitMask _targets;

        private static AOEShapeCircle _shape = new(5); // TODO: verify radius

        public P3EarthShakerVoidzone() : base(default, "GTFO from voidzone!") { }

        public override void Init(BossModule module)
        {
            _voidzones = module.Enemies(OID.VoidzoneEarthShaker);
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var z in _voidzones.Where(z => z.EventState != 7))
                yield return new(_shape, z.Position);
            foreach (var p in _predicted)
                yield return p;
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.VoidzoneEarthShaker)
                _predicted.Clear();
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Earthshaker)
                _targets.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.EarthShaker)
                foreach (var (_, p) in module.Raid.WithSlot().IncludedInMask(_targets))
                    _predicted.Add(new(_shape, p.Position, default, module.WorldState.CurrentTime.AddSeconds(1.4f)));
        }
    }

    class P3TempestWing : Components.TankbusterTether
    {
        public P3TempestWing() : base(ActionID.MakeSpell(AID.TempestWing), (uint)TetherID.TempestWing, 5) { }
    }
}
