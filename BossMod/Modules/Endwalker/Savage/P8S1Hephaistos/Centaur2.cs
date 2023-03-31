using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class QuadrupedalImpact : Components.Knockback
    {
        private WPos? _source;

        public QuadrupedalImpact() : base(ActionID.MakeSpell(AID.QuadrupedalImpactAOE), true) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_source != null)
                yield return new(_source.Value, 30); // TODO: activation
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.QuadrupedalImpact)
                _source = spell.LocXZ;
        }
    }

    class QuadrupedalCrush : Components.GenericAOEs
    {
        private WPos? _source;
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(30);

        public QuadrupedalCrush() : base(ActionID.MakeSpell(AID.QuadrupedalCrushAOE)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_source != null)
                yield return new(_shape, _source.Value, new(), _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.QuadrupedalCrush)
            {
                _source = spell.LocXZ;
                _activation = spell.FinishAt.AddSeconds(0.9f);
            }
        }
    }

    class CentaurTetraflare : TetraOctaFlareCommon
    {
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.ConceptualTetraflareCentaur)
                SetupMasks(module, Concept.Tetra);
        }
    }

    class CentaurDiflare : Components.UniformStackSpread
    {
        public CentaurDiflare() : base(6, 0, 4, 4) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.ConceptualDiflare)
                AddStacks(module.Raid.WithoutSlot().Where(a => a.Role == Role.Healer));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.EmergentDiflare)
                Stacks.Clear();
        }
    }

    // TODO: hints
    class BlazingFootfalls : BossComponent
    {
        public int NumMechanicsDone { get; private set; }
        private int _seenVisuals;
        private bool _firstCrush;
        private bool _firstSafeLeft;
        private bool _secondSafeTop;

        private const float _trailblazeHalfWidth = 7;
        private const float _trailblazeKnockbackDistance = 10;
        private const float _crushRadius = 30;
        private const float _impactKnockbackRadius = 30;
        private const float _safespotOffset = 15;
        private const float _safespotRadius = 3;

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumMechanicsDone == 0)
            {
                // draw first trailblaze
                arena.ZoneRect(module.Bounds.Center, new WDir(0, 1), module.Bounds.HalfSize, module.Bounds.HalfSize, _trailblazeHalfWidth, ArenaColor.AOE);
            }
            if (NumMechanicsDone == 2)
            {
                // draw second trailblaze
                arena.ZoneRect(module.Bounds.Center, new WDir(1, 0), module.Bounds.HalfSize, module.Bounds.HalfSize, _trailblazeHalfWidth, ArenaColor.AOE);
            }

            if (_firstCrush && NumMechanicsDone < 2)
            {
                // draw first crush
                arena.ZoneCircle(module.Bounds.Center + module.Bounds.HalfSize * new WDir(_firstSafeLeft ? 1 : -1, 0), _crushRadius, ArenaColor.AOE);
            }
            if (!_firstCrush && NumMechanicsDone is >= 2 and < 4)
            {
                // draw second crush
                arena.ZoneCircle(module.Bounds.Center + module.Bounds.HalfSize * new WDir(0, _secondSafeTop ? 1 : -1), _crushRadius, ArenaColor.AOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumMechanicsDone < 2 && _seenVisuals > 0)
            {
                // draw first safespot
                arena.AddCircle(module.Bounds.Center + _safespotOffset * new WDir(_firstSafeLeft ? -1 : 1, 0), _safespotRadius, ArenaColor.Safe, 2);
            }
            if (NumMechanicsDone < 4 && _seenVisuals > 1)
            {
                // draw second safespot
                arena.AddCircle(module.Bounds.Center + _safespotOffset * new WDir(0, _secondSafeTop ? -1 : 1), _safespotRadius, ArenaColor.Safe, 2);
            }

            if (NumMechanicsDone == 0)
            {
                // draw knockback from first trailblaze
                var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(pc.Position.X < module.Bounds.Center.X ? -1 : 1, 0);
                Components.Knockback.DrawKnockback(pc, adjPos, arena);
            }
            if (NumMechanicsDone == 2)
            {
                // draw knockback from second trailblaze
                var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(0, pc.Position.Z < module.Bounds.Center.Z ? -1 : 1);
                Components.Knockback.DrawKnockback(pc, adjPos, arena);
            }

            if (!_firstCrush && NumMechanicsDone == 1)
            {
                // draw knockback from first impact
                var adjPos = Components.Knockback.AwayFromSource(pc.Position, module.Bounds.Center + module.Bounds.HalfSize * new WDir(_firstSafeLeft ? -1 : 1, 0), _impactKnockbackRadius);
                Components.Knockback.DrawKnockback(pc, adjPos, arena);
            }
            if (_firstCrush && NumMechanicsDone == 3)
            {
                // draw knockback from second impact
                var adjPos = Components.Knockback.AwayFromSource(pc.Position, module.Bounds.Center + module.Bounds.HalfSize * new WDir(0, _secondSafeTop ? -1 : 1), _impactKnockbackRadius);
                Components.Knockback.DrawKnockback(pc, adjPos, arena);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.BlazingFootfallsImpactVisual:
                    if (_seenVisuals > 0)
                    {
                        _secondSafeTop = spell.LocXZ.Z < module.Bounds.Center.Z;
                    }
                    else
                    {
                        _firstSafeLeft = spell.LocXZ.X < module.Bounds.Center.X;
                    }
                    ++_seenVisuals;
                    break;
                case AID.BlazingFootfallsCrushVisual:
                    if (_seenVisuals > 0)
                    {
                        _secondSafeTop = spell.LocXZ.Z > module.Bounds.Center.Z;
                    }
                    else
                    {
                        _firstCrush = true;
                        _firstSafeLeft = spell.LocXZ.X > module.Bounds.Center.X;
                    }
                    ++_seenVisuals;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.BlazingFootfallsTrailblaze or AID.BlazingFootfallsImpact or AID.BlazingFootfallsCrush)
                ++NumMechanicsDone;
        }
    }
}
