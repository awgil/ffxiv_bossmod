using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Raid.T01Caduceus
{
    public enum OID : uint
    {
        Boss = 0x7D7, // x1, and more spawn during fight
        Helper = 0x1B2, // x1
        DarkMatterSlime = 0x7D8, // spawn during fight
        Platform = 0x1E8729, // x13
        Regorge = 0x1E8B20, // EventObj type, spawn during fight
        Syrup = 0x1E88F1, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 1207, // Boss->self, no cast, range 6+R ?-degree cone
        HoodSwing = 1208, // Boss->self, no cast, range 8+R ?-degree cone cleave
        WhipBack = 1209, // Boss->self, 2.0s cast, range 6+R 120-degree cone (baited backward cleave)
        Regorge = 1210, // Boss->location, no cast, range 4 circle aoe that leaves voidzone
        SteelScales = 1211, // Boss->self, no cast, single-target, damage up buff stack

        PlatformExplosion = 674, // Helper->self, no cast, hits players on glowing platforms and spawns dark matter slime on them
        AutoAttackSlime = 872, // DarkMatterSlime->player, no cast, single-target
        Syrup = 1214, // DarkMatterSlime->location, 0.5s cast, range 4 circle aoe that leaves voidzone
        Rupture = 1213, // DarkMatterSlime->self, no cast, range 16+R circle aoe suicide (damage depends on cur hp?)
        Devour = 1454, // Boss->DarkMatterSlime, no cast, single-target visual
    };

    public enum SID : uint
    {
        SteelScales = 349, // Boss->Boss, extra=1-8 (num stacks)
    };

    class HoodSwing : Components.Cleave
    {
        private DateTime _lastBossCast; // assume boss/add cleaves are synchronized?..
        public float SecondsUntilNextCast(BossModule module) => Math.Max(0, 18 - (float)(module.WorldState.CurrentTime - _lastBossCast).TotalSeconds);

        public HoodSwing() : base(ActionID.MakeSpell(AID.HoodSwing), new AOEShapeCone(11, 60.Degrees()), (uint)OID.Boss) { } // TODO: verify angle

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"Next cleave in ~{SecondsUntilNextCast(module):f1}s");
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction && caster == module.PrimaryActor)
                _lastBossCast = module.WorldState.CurrentTime;
        }
    }

    class WhipBack : Components.SelfTargetedAOEs
    {
        public WhipBack() : base(ActionID.MakeSpell(AID.WhipBack), new AOEShapeCone(9, 60.Degrees())) { }
    }

    class Regorge : Components.PersistentVoidzoneAtCastTarget
    {
        public Regorge() : base(4, ActionID.MakeSpell(AID.Regorge), m => m.Enemies(OID.Regorge).Where(z => z.EventState != 7), 2.1f) { }
    }

    class Syrup : Components.PersistentVoidzoneAtCastTarget
    {
        public Syrup() : base(4, ActionID.MakeSpell(AID.Syrup), m => m.Enemies(OID.Syrup).Where(z => z.EventState != 7), 0.3f) { }
    }

    // TODO: merge happens if bosses are 'close enough' (threshold is >20.82 at least) or have high enough hp difference (>5% at least) and more than 20s passed since split
    class CloneMerge : BossComponent
    {
        public Actor? Clone { get; private set; }
        public DateTime CloneSpawnTime { get; private set; }
        public Actor? CloneIfValid => Clone != null && !Clone.IsDestroyed && !Clone.IsDead && Clone.IsTargetable ? Clone : null;

        public override void Update(BossModule module)
        {
            if (Clone != null || module.PrimaryActor.HP.Cur > module.PrimaryActor.HP.Max / 2)
                return;
            Clone = module.Enemies(OID.Boss).FirstOrDefault(a => a != module.PrimaryActor);
            if (Clone != null)
                CloneSpawnTime = module.WorldState.CurrentTime;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var clone = CloneIfValid;
            if (clone != null && !module.PrimaryActor.IsDestroyed && !module.PrimaryActor.IsDead && module.PrimaryActor.IsTargetable)
            {
                var hpDiff = (int)(clone.HP.Cur - module.PrimaryActor.HP.Cur) * 100.0f / module.PrimaryActor.HP.Max;
                var checkIn = Math.Max(0, 20 - (module.WorldState.CurrentTime - CloneSpawnTime).TotalSeconds);
                hints.Add($"Clone HP: {(hpDiff > 0 ? "+" : "")}{hpDiff:f1}%, distance: {(clone.Position - module.PrimaryActor.Position).Length():f2}, check in {checkIn:f1}s");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(Clone, ArenaColor.Enemy);
        }
    }

    class T01CaduceusStates : StateMachineBuilder
    {
        public T01CaduceusStates(BossModule module) : base(module)
        {
            SimplePhase(0, id => { SimpleState(id, 600, "Enrage"); }, "Boss death")
                .ActivateOnEnter<HoodSwing>()
                .ActivateOnEnter<WhipBack>()
                .ActivateOnEnter<Regorge>()
                .ActivateOnEnter<Syrup>()
                .ActivateOnEnter<CloneMerge>()
                .ActivateOnEnter<T01AI>()
                .Raw.Update = () => (module.PrimaryActor.IsDead || module.PrimaryActor.IsDestroyed) && module.FindComponent<CloneMerge>()!.CloneIfValid == null;
        }
    }

    [ConfigDisplay(Order = 0x110, Parent = typeof(RealmRebornConfig))]
    public class T01CaduceusConfig : CooldownPlanningConfigNode
    {
        public T01CaduceusConfig() : base(50) { }
    }

    public class T01Caduceus : BossModule
    {
        public T01Caduceus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-26, -407), 35, 43))
        {
            ActivateComponent<Platforms>();
        }

        public override bool NeedToJump(WPos from, WDir dir) => Platforms.IntersectJumpEdge(from, dir, 2.5f);

        // don't activate module created for clone (this is a hack...)
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat && PrimaryActor.HP.Cur > PrimaryActor.HP.Max / 2; }
    }
}
