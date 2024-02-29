﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HuntA.Sugriva
{
    public enum OID : uint
    {
        Boss = 0x35FC, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Twister = 27219, // Boss->players, 5.0s cast, range 8 circle stack + knockback 20
        BarrelingSmash = 27220, // Boss->player, no cast, single-target, charges to random player and starts casting Spark or Scythe Tail immediately afterwards
        Spark = 27221, // Boss->self, 5.0s cast, range 14-24+R donut
        ScytheTail = 27222, // Boss->self, 5.0s cast, range 17 circle
        Butcher = 27223, // Boss->self, 5.0s cast, range 8 120-degree cone
        Rip = 27224, // Boss->self, 2.5s cast, range 8 120-degree cone
        RockThrowFirst = 27225, // Boss->location, 4.0s cast, range 6 circle
        RockThrowRest = 27226, // Boss->location, 1.6s cast, range 6 circle
        Crosswind = 27227, // Boss->self, 5.0s cast, range 36 circle
        ApplyPrey = 27229, // Boss->player, 0.5s cast, single-target
    }

    class Twister : Components.KnockbackFromCastTarget
    {
        public Twister() : base(ActionID.MakeSpell(AID.Twister), 20, shape: new AOEShapeCircle(8)) { }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Casters.Count > 0)
                hints.Add("Stack and knockback");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return Casters.FirstOrDefault()?.CastInfo?.TargetID == player.InstanceID ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            var caster = Casters.FirstOrDefault();
            var target = caster != null ? module.WorldState.Actors.Find(caster.CastInfo!.TargetID) : null;
            if (target != null)
                Shape!.Outline(arena, target);
        }
    }

    class Spark : Components.SelfTargetedAOEs
    {
        public Spark() : base(ActionID.MakeSpell(AID.Spark), new AOEShapeDonut(14, 30)) { }
    }

    class ScytheTail : Components.SelfTargetedAOEs
    {
        public ScytheTail() : base(ActionID.MakeSpell(AID.ScytheTail), new AOEShapeCircle(17)) { }
    }

    class Butcher : Components.BaitAwayCast
    {
        public Butcher() : base(ActionID.MakeSpell(AID.Butcher), new AOEShapeCone(8, 60.Degrees())) { }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell) //tankbuster resolves on cast event, which can be delayed by moving out of tankbuster range
        {
            if (spell.Action == WatchedAction)
                CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }

    class Rip : Components.SelfTargetedAOEs
    {
        public Rip() : base(ActionID.MakeSpell(AID.Rip), new AOEShapeCone(8, 60.Degrees())) { }
    }

    // TODO: generalize to baited aoe
    class RockThrow : Components.GenericAOEs
    {
        private Actor? _target;
        private static AOEShapeCircle _shape = new(6);

        public RockThrow() : base(ActionID.MakeSpell(AID.RockThrowRest)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (Active(module))
                yield return new(_shape, module.PrimaryActor.CastInfo!.LocXZ, default, module.PrimaryActor.CastInfo.NPCFinishAt);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return player == _target ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_target != null)
                arena.AddCircle(_target.Position, _shape.Radius, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ApplyPrey:
                    NumCasts = 0;
                    _target = module.WorldState.Actors.Find(spell.TargetID);
                    if (_target?.Type == ActorType.Chocobo) //Player Chocobos are immune against prey, so mechanic doesn't happen if a chocobo gets selected
                        _target = null;
                    break;
                case AID.RockThrowRest:
                    if (NumCasts >= 1)
                        _target = null;
                    break;
            }
        }

        private bool Active(BossModule module) => (module.PrimaryActor.CastInfo?.IsSpell() ?? false) && (AID)module.PrimaryActor.CastInfo!.Action.ID is AID.RockThrowFirst or AID.RockThrowRest;
    }

    class Crosswind : Components.RaidwideCast
    {
        public Crosswind() : base(ActionID.MakeSpell(AID.Crosswind)) { }
    }

    class SugrivaStates : StateMachineBuilder
    {
        public SugrivaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Twister>()
                .ActivateOnEnter<Spark>()
                .ActivateOnEnter<ScytheTail>()
                .ActivateOnEnter<Butcher>()
                .ActivateOnEnter<Rip>()
                .ActivateOnEnter<RockThrow>()
                .ActivateOnEnter<Crosswind>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 185)]
    public class Sugriva : SimpleBossModule
    {
        public Sugriva(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
