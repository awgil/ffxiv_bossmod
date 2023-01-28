using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P2PartySynergy : CommonAssignments
    {
        public enum Glitch { Unknown, Mid, Remote }

        public Glitch ActiveGlitch;

        protected override (GroupAssignmentUnique assignment, bool global) Assignments()
        {
            var config = Service.Config.Get<TOPConfig>();
            return (config.P2PartySynergyAssignments, config.P2PartySynergyGlobalPriority);
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
                case OID.OmegaM2:
                    if (actor.ModelState.ModelState == 4)
                    {
                        AOEs.Add(new(new AOEShapeDonut(10, 40), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    else
                    {
                        AOEs.Add(new(new AOEShapeCircle(10), actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.1f)));
                    }
                    break;
                case OID.OmegaF2:
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

    class P2PartySynergyOptimizedFire : Components.StackSpread
    {
        public P2PartySynergyOptimizedFire() : base(0, 7, alwaysShowSpreads: true) { }

        public override void Init(BossModule module) => SpreadTargets.AddRange(module.Raid.WithoutSlot(true));

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.OptimizedFire)
                SpreadTargets.Clear();
        }
    }

    class P2PartySynergyOpticalLaser : Components.GenericAOEs
    {
        private P2PartySynergy? _synergy;
        private AOEInstance? _aoe;

        public P2PartySynergyOpticalLaser() : base(ActionID.MakeSpell(AID.OpticalLaser)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void Init(BossModule module)
        {
            _synergy = module.FindComponent<P2PartySynergy>();

            var source = module.Enemies(OID.OpticalUnit).FirstOrDefault();
            if (source != null)
                _aoe = new(new AOEShapeRect(100, 8), source.Position, source.Rotation, module.WorldState.CurrentTime.AddSeconds(6.8f));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pos = AssignedPosition(module, pcSlot);
            if (pos != default)
                arena.AddCircle(module.Bounds.Center + pos, 1, ArenaColor.Safe);
        }

        private WDir AssignedPosition(BossModule module, int slot)
        {
            if (_synergy == null || _aoe == null)
                return new();

            var ps = _synergy.PlayerStates[slot];
            if (ps.Order == 0 || ps.Group == 0)
                return new();

            var eyeOffset = _aoe.Value.Origin - module.Bounds.Center;
            switch (_synergy.ActiveGlitch)
            {
                case P2PartySynergy.Glitch.Mid:
                    var toRelNorth = eyeOffset.Normalized();
                    return 10 * (2.5f - ps.Order) * toRelNorth + 9 * (ps.Group == 1 ? toRelNorth.OrthoL() : toRelNorth.OrthoR());
                case P2PartySynergy.Glitch.Remote:
                    return 19 * (Angle.FromDirection(eyeOffset) + ps.Order * 40.Degrees() - 10.Degrees() + (ps.Group == 1 ? 0.Degrees() : 180.Degrees())).ToDirection();
                default:
                    return new();
            }
        }
    }
}
