using System;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P2OptimizedSagittariusArrow : Components.SelfTargetedAOEs
    {
        public P2OptimizedSagittariusArrow() : base(ActionID.MakeSpell(AID.OptimizedSagittariusArrow), new AOEShapeRect(100, 5)) { }
    }

    class P2OptimizedBladedance : Components.BaitAwayTethers
    {
        public P2OptimizedBladedance() : base(new AOEShapeCone(100, 45.Degrees()), (uint)TetherID.OptimizedBladedance, ActionID.MakeSpell(AID.OptimizedBladedanceAOE)) { }

        public override void Init(BossModule module)
        {
            ForbiddenPlayers = module.Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
        }
    }

    class P2BeyondDefense : Components.UniformStackSpread
    {
        public enum Mechanic { None, Spread, Stack }

        public Mechanic CurMechanic;
        private Actor? _source;
        private DateTime _activation;
        private BitMask _forbiddenStack;

        public P2BeyondDefense() : base(6, 5, 3, alwaysShowSpreads: true) { }

        public override void Update(BossModule module)
        {
            Stacks.Clear();
            Spreads.Clear();
            if (_source != null)
            {
                switch (CurMechanic)
                {
                    case Mechanic.Spread:
                        AddSpreads(module.Raid.WithoutSlot().SortedByRange(_source.Position).Take(2), _activation);
                        break;
                    case Mechanic.Stack:
                        if (module.Raid.WithoutSlot().Closest(_source.Position) is var target && target != null)
                            AddStack(target, _activation, _forbiddenStack);
                        break;
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(_source, ArenaColor.Object, true);
            base.DrawArenaForeground(module, pcSlot, pc, arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.SyntheticShield:
                    _source = caster;
                    break;
                case AID.BeyondDefense:
                    _source = caster;
                    CurMechanic = Mechanic.Spread;
                    _activation = spell.NPCFinishAt.AddSeconds(0.2f);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.BeyondDefenseAOE:
                    foreach (var t in spell.Targets)
                        _forbiddenStack.Set(module.Raid.FindSlot(t.ID));
                    CurMechanic = Mechanic.Stack;
                    _activation = module.WorldState.CurrentTime.AddSeconds(3.2f);
                    break;
                case AID.PilePitch:
                    CurMechanic = Mechanic.None;
                    break;
            }
        }
    }

    class P2CosmoMemory : Components.CastCounter
    {
        public P2CosmoMemory() : base(ActionID.MakeSpell(AID.CosmoMemoryAOE)) { }
    }

    class P2OptimizedPassageOfArms : BossComponent
    {
        public Actor? _invincible;

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_invincible != null)
            {
                var e = hints.PotentialTargets.FirstOrDefault(e => e.Actor == _invincible);
                if (e != null)
                {
                    e.Priority = AIHints.Enemy.PriorityForbidFully;
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Invincibility && (OID)actor.OID == OID.OmegaM)
                _invincible = actor;
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Invincibility && _invincible == actor)
                _invincible = null;
        }
    }
}
