using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P2PartySynergy : CommonAssignments
    {
        public enum Glitch { Unknown, Mid, Remote }

        public Glitch ActiveGlitch;
        public bool EnableDistanceHints;

        protected override (GroupAssignmentUnique assignment, bool global) Assignments()
        {
            var config = Service.Config.Get<TOPConfig>();
            return (config.P2PartySynergyAssignments, config.P2PartySynergyGlobalPriority);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (ActiveGlitch != Glitch.Unknown)
                hints.Add($"Glitch: {ActiveGlitch}");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (EnableDistanceHints && FindPartner(module, slot) is var partner && partner != null)
            {
                var distSq = (partner.Position - actor.Position).LengthSq();
                var range = DistanceRange;
                if (distSq < range.min * range.min)
                    hints.Add("Move away from partner!");
                else if (distSq > range.max * range.max)
                    hints.Add("Move closer to partner!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var partner = FindPartner(module, pcSlot);
            if (partner != null)
            {
                var distSq = (partner.Position - pc.Position).LengthSq();
                var range = DistanceRange;
                arena.AddLine(pc.Position, partner.Position, distSq < range.min * range.min || distSq > range.max * range.max ? ArenaColor.Danger : ArenaColor.Safe);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.MidGlitch:
                    ActiveGlitch = Glitch.Mid;
                    break;
                case SID.RemoteGlitch:
                    ActiveGlitch = Glitch.Remote;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            // assuming standard 'blue-purple-orange-green' order
            var order = (IconID)iconID switch
            {
                IconID.PartySynergyCross => 1,
                IconID.PartySynergySquare => 2,
                IconID.PartySynergyCircle => 3,
                IconID.PartySynergyTriangle => 4,
                _ => 0
            };
            Assign(module, actor, order);
        }

        private Actor? FindPartner(BossModule module, int slot)
        {
            var ps = PlayerStates[slot];
            var partnerSlot = ps.Order > 0 ? Array.FindIndex(PlayerStates, s => s.Order == ps.Order && s.Group != ps.Group) : -1;
            return module.Raid[partnerSlot];
        }

        private (float min, float max) DistanceRange => ActiveGlitch switch
        {
            Glitch.Mid => (20, 26),
            Glitch.Remote => (34, 50),
            _ => (0, 50)
        };
    }

    class P2PartySynergyDoubleAOEs : Components.GenericAOEs
    {
        public List<AOEInstance> AOEs = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BeyondStrength or AID.EfficientBladework or AID.SuperliminalSteel or AID.OptimizedBlizzard)
                ++NumCasts;
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id != 0x1E43)
                return;
            switch ((OID)actor.OID)
            {
                case OID.OmegaMHelper:
                    if (actor.ModelState.ModelState == 4)
                    {
                        AOEs.Add(new(new AOEShapeDonut(10, 40), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    else
                    {
                        AOEs.Add(new(new AOEShapeCircle(10), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    break;
                case OID.OmegaFHelper:
                    if (actor.ModelState.ModelState == 4)
                    {
                        AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation + 90.Degrees(), module.WorldState.CurrentTime.AddSeconds(5.1f)));
                        AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation - 90.Degrees(), module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    else
                    {
                        AOEs.Add(new(new AOEShapeCross(100, 5), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    break;
            }
        }
    }

    class P2PartySynergyOptimizedFire : Components.UniformStackSpread
    {
        public P2PartySynergyOptimizedFire() : base(0, 7, alwaysShowSpreads: true) { }

        public override void Init(BossModule module) => AddSpreads(module.Raid.WithoutSlot(true));

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.OptimizedFire)
                Spreads.Clear();
        }
    }

    class P2PartySynergyOpticalLaser : Components.GenericAOEs
    {
        private P2PartySynergy? _synergy;
        private Actor? _source;
        private DateTime _activation;

        private static AOEShapeRect _shape = new(100, 8);

        public P2PartySynergyOpticalLaser() : base(ActionID.MakeSpell(AID.OpticalLaser)) { }

        public void Show(BossModule module)
        {
            _activation = module.WorldState.CurrentTime.AddSeconds(6.8f);
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != default && _source != null)
                yield return new(_shape, _source.Position, _source.Rotation, _activation);
        }

        public override void Init(BossModule module)
        {
            _synergy = module.FindComponent<P2PartySynergy>();
            _source = module.Enemies(OID.OpticalUnit).FirstOrDefault();
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(_source, ArenaColor.Object, true);
            var pos = AssignedPosition(module, pcSlot);
            if (pos != default)
                arena.AddCircle(module.Bounds.Center + pos, 1, ArenaColor.Safe);
        }

        private WDir AssignedPosition(BossModule module, int slot)
        {
            if (_synergy == null || _source == null || _activation == default)
                return new();

            var ps = _synergy.PlayerStates[slot];
            if (ps.Order == 0 || ps.Group == 0)
                return new();

            var eyeOffset = _source.Position - module.Bounds.Center;
            switch (_synergy.ActiveGlitch)
            {
                case P2PartySynergy.Glitch.Mid:
                    var toRelNorth = eyeOffset.Normalized();
                    return 10 * (2.5f - ps.Order) * toRelNorth + 11 * (ps.Group == 1 ? toRelNorth.OrthoL() : toRelNorth.OrthoR());
                case P2PartySynergy.Glitch.Remote:
                    return 19 * (Angle.FromDirection(eyeOffset) + ps.Order * 40.Degrees() - 10.Degrees() + (ps.Group == 1 ? 0.Degrees() : 180.Degrees())).ToDirection();
                default:
                    return new();
            }
        }
    }

    class P2PartySynergyDischarger : Components.Knockback
    {
        public P2PartySynergyDischarger() : base(ActionID.MakeSpell(AID.Discharger)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            yield return new(module.Bounds.Center, 13); // TODO: activation
        }
    }

    class P2PartySynergyEfficientBladework : Components.GenericAOEs
    {
        private P2PartySynergy? _synergy;
        private DateTime _activation;
        private List<Actor> _sources = new();
        private int _firstStackSlot = -1;
        private BitMask _firstGroup;

        private static AOEShapeCircle _shape = new(10);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != default)
                foreach (var s in _sources)
                    yield return new(_shape, s.Position, new(), _activation);
        }

        public override void Init(BossModule module)
        {
            _synergy = module.FindComponent<P2PartySynergy>();
            _sources.AddRange(module.Enemies(OID.OmegaF));
            // by default, use same group as for synergy
            if (_synergy != null)
                _firstGroup = module.Raid.WithSlot(true).WhereSlot(s => _synergy.PlayerStates[s].Group == 1).Mask();
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pos = AssignedPosition(module, pcSlot);
            if (pos != default)
                arena.AddCircle(module.Bounds.Center + pos, 1, ArenaColor.Safe);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x1E43 && (OID)actor.OID == OID.OmegaMHelper)
                _sources.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.EfficientBladework:
                    ++NumCasts;
                    break;
                case AID.OpticalLaser:
                    _activation = module.WorldState.CurrentTime.AddSeconds(9.8f);
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Spotlight && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && _synergy != null)
            {
                if (_firstStackSlot < 0)
                {
                    _firstStackSlot = slot;
                }
                else
                {
                    // as soon as we have two stacks, check whether they are from same group - if so, we need adjusts
                    var s1 = _synergy.PlayerStates[_firstStackSlot];
                    var s2 = _synergy.PlayerStates[slot];
                    if (s1.Group == s2.Group)
                    {
                        // ok, we need adjusts - assume whoever is more S adjusts - that is higher order in G1 or G2 with mid glitch, or lower order in G2 with remote glitch
                        var adjustOrder = s1.Group == 2 && _synergy.ActiveGlitch == P2PartySynergy.Glitch.Remote ? Math.Min(s1.Order, s2.Order): Math.Max(s1.Order, s2.Order);
                        for (int s = 0; s < _synergy.PlayerStates.Length; ++s)
                            if (_synergy.PlayerStates[s].Order == adjustOrder)
                                _firstGroup.Toggle(s);
                    }
                }
            }
        }

        private WDir AssignedPosition(BossModule module, int slot)
        {
            if (_activation == default || _synergy == null || _sources.Count == 0)
                return new();

            // assumption: first source (F) is our relative north, G1 always goes to relative west, G2 goes to relative S/E depending on glitch
            var relNorth = 1.4f * (_sources[0].Position - module.Bounds.Center);
            return _firstGroup[slot] ? relNorth.OrthoL() : _synergy.ActiveGlitch == P2PartySynergy.Glitch.Mid ? -relNorth : relNorth.OrthoR();
        }
    }

    class P2PartySynergySpotlight : Components.UniformStackSpread
    {
        private List<Actor> _stackTargets = new(); // don't show anything until knockbacks are done, to reduce visual clutter

        public P2PartySynergySpotlight() : base(6, 0, 4, 4) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Spotlight)
                _stackTargets.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Discharger:
                    AddStacks(_stackTargets);
                    break;
                case AID.Spotlight:
                    Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                    break;
            }
        }
    }
}
