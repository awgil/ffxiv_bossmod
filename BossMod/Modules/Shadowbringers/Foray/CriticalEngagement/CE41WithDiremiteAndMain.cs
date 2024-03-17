using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE41WithDiremiteAndMain
{
    public enum OID : uint
    {
        Helper = 0x233C, // R0.500, x6
        Boss = 0x31CC, // R7.200, x1
        DimCrystal = 0x31CD, // R1.600, spawn during fight
        CorruptedCrystal = 0x31CE, // R1.600, spawn during fight
        SandSphere = 0x31CF, // R4.000, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast, single-target

        Shardfall = 24071, // Boss->self, 2.0s cast, single-target, visual
        CrystallineFracture = 24072, // CorruptedCrystal/DimCrystal->self, 3.0s cast, range 4 circle aoe (cast on spawn)
        ResonantFrequencyDim = 24073, // DimCrystal->self, 3.0s cast, range 6 circle, suicide, cast early if crystal is hit by shardstrike
        ResonantFrequencyCorrupted = 24074, // CorruptedCrystal->self, 3.0s cast, range 6 circle, suicide, cast early if crystal is hit by shardstrike
        ResonantFrequencyDimStinger = 24075, // DimCrystal->self, no cast, single-target, suicide, after aetherial stingers
        ResonantFrequencyCorruptedStinger = 24076, // CorruptedCrystal->self, no cast, single-target, suicide, after crystalline stingers
        CrystallineStingers = 24077, // Boss->self, 5.0s cast, range 60 circle, hide behind dim
        AetherialStingers = 24078, // Boss->self, 5.0s cast, range 60 circle, hide behind corrupted
        SandSphere = 24079, // Boss->self, 5.0s cast, single-target, visual
        Subduction = 24080, // SandSphere->self, 4.0s cast, range 8 circle aoe with knockback 10
        Earthbreaker = 24081, // Boss->self, 5.0s cast, single-target, visual
        EarthbreakerAOE1 = 24082, // Helper->self, 5.0s cast, range 10 circle
        EarthbreakerAOE2 = 24083, // Helper->self, 3.0s cast, range 10-20 donut
        EarthbreakerAOE3 = 24084, // Helper->self, 3.0s cast, range 20-30 donut

        CrystalNeedle = 24085, // Boss->player, 5.0s cast, single-target, tankbuster
        Shardstrike = 24086, // Boss->self, 2.0s cast, single-target, visual
        ShardstrikeAOE = 24087, // Helper->players, 5.0s cast, range 5 circle spread
        Hailfire = 24088, // Boss->self, 8.0s cast, single-target, visual
        HailfireAOE = 24089, // Boss->self, no cast, range 40 width 4 rect aoe
        Teleport = 24090, // Boss->location, no cast, single-target
    };

    public enum IconID : uint
    {
        Shardstrike = 96, // player
        Hailfire1 = 79, // player
        Hailfire2 = 80, // player
        Hailfire3 = 81, // player
        Hailfire4 = 82, // player
    };

    class CrystallineFracture : Components.SelfTargetedAOEs
    {
        public CrystallineFracture() : base(ActionID.MakeSpell(AID.CrystallineFracture), new AOEShapeCircle(4)) { }
    }

    class ResonantFrequencyDim : Components.SelfTargetedAOEs
    {
        public ResonantFrequencyDim() : base(ActionID.MakeSpell(AID.ResonantFrequencyDim), new AOEShapeCircle(6)) { }
    }

    class ResonantFrequencyCorrupted : Components.SelfTargetedAOEs
    {
        public ResonantFrequencyCorrupted() : base(ActionID.MakeSpell(AID.ResonantFrequencyCorrupted), new AOEShapeCircle(6)) { }
    }

    class CrystallineStingers : Components.CastLineOfSightAOE
    {
        public CrystallineStingers() : base(ActionID.MakeSpell(AID.CrystallineStingers), 60, false) { }
        public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.DimCrystal).Where(a => !a.IsDead);
    }

    class AetherialStingers : Components.CastLineOfSightAOE
    {
        public AetherialStingers() : base(ActionID.MakeSpell(AID.AetherialStingers), 60, false) { }
        public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.CorruptedCrystal).Where(a => !a.IsDead);
    }

    class Subduction : Components.SelfTargetedAOEs
    {
        public Subduction() : base(ActionID.MakeSpell(AID.Subduction), new AOEShapeCircle(8)) { }
    }

    // next aoe starts casting slightly before previous, so use a custom component
    class Earthbreaker : Components.GenericAOEs
    {
        private List<(Actor caster, AOEShape shape)> _active = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _active.Take(1).Select(e => new AOEInstance(e.shape, e.caster.Position, e.caster.CastInfo!.Rotation, e.caster.CastInfo.NPCFinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.EarthbreakerAOE1 => new AOEShapeCircle(10),
                AID.EarthbreakerAOE2 => new AOEShapeDonut(10, 20),
                AID.EarthbreakerAOE3 => new AOEShapeDonut(20, 30),
                _ => null
            };
            if (shape != null)
                _active.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            _active.RemoveAll(c => c.caster == caster);
        }
    }

    class CrystalNeedle : Components.SingleTargetCast
    {
        public CrystalNeedle() : base(ActionID.MakeSpell(AID.CrystalNeedle)) { }
    }

    class Shardstrike : Components.SpreadFromCastTargets
    {
        public Shardstrike() : base(ActionID.MakeSpell(AID.ShardstrikeAOE), 5) { }
    }

    // TODO: this should probably be generalized
    class Hailfire : Components.GenericAOEs
    {
        private Actor?[] _targets = new Actor?[4];
        private DateTime _activation;

        private static AOEShapeRect _shape = new(40, 2);

        private Actor? NextTarget => NumCasts < _targets.Length ? _targets[NumCasts] : null;

        public Hailfire() : base(ActionID.MakeSpell(AID.HailfireAOE)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NextTarget is var target && target != null && target != actor)
                yield return new(_shape, module.PrimaryActor.Position, Angle.FromDirection(target.Position - module.PrimaryActor.Position), _activation);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NextTarget == pc)
                _shape.Outline(arena, module.PrimaryActor.Position, Angle.FromDirection(pc.Position - module.PrimaryActor.Position), ArenaColor.Danger);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction && NumCasts < _targets.Length)
            {
                _targets[NumCasts] = null;
                _activation = module.WorldState.CurrentTime.AddSeconds(2.3f);
            }
            base.OnEventCast(module, caster, spell);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var order = (IconID)iconID switch
            {
                IconID.Hailfire1 => 0,
                IconID.Hailfire2 => 1,
                IconID.Hailfire3 => 2,
                IconID.Hailfire4 => 3,
                _ => -1
            };
            if (order >= 0)
            {
                NumCasts = 0;
                _targets[order] = actor;
                _activation = module.WorldState.CurrentTime.AddSeconds(8.2f);
            }
        }
    }

    class CE41WithDiremiteAndMainStates : StateMachineBuilder
    {
        public CE41WithDiremiteAndMainStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CrystallineFracture>()
                .ActivateOnEnter<ResonantFrequencyDim>()
                .ActivateOnEnter<ResonantFrequencyCorrupted>()
                .ActivateOnEnter<CrystallineStingers>()
                .ActivateOnEnter<AetherialStingers>()
                .ActivateOnEnter<Subduction>()
                .ActivateOnEnter<Earthbreaker>()
                .ActivateOnEnter<CrystalNeedle>()
                .ActivateOnEnter<Shardstrike>()
                .ActivateOnEnter<Hailfire>();
        }
    }

    [ModuleInfo(CFCID = 778, DynamicEventID = 21)]
    public class CE41WithDiremiteAndMain : BossModule
    {
        private IReadOnlyList<Actor> _dimCrystals;
        private IReadOnlyList<Actor> _corruptedCrystals;

        public CE41WithDiremiteAndMain(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-220, 530), 30))
        {
            _dimCrystals = Enemies(OID.DimCrystal);
            _corruptedCrystals = Enemies(OID.CorruptedCrystal);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(_dimCrystals.Where(c => !c.IsDead), ArenaColor.Object, true);
            Arena.Actors(_corruptedCrystals.Where(c => !c.IsDead), ArenaColor.Object, true);
        }
    }
}
