using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE42FromBeyondTheGrave
{
    public enum OID : uint
    {
        Boss = 0x2E35, // R8.250, x1
        Deathwall = 0x2EE8, // R0.500, x1
        Helper = 0x233C, // R0.500, x6, and more spawn during fight
        ShockSphere = 0x3232, // R1.000, spawn during fight
        WarWraith = 0x3233, // R1.800, spawn during fight
        HernaisTheTenacious = 0x3234, // R0.500, spawn during fight
        DyunbuTheAccursed = 0x3235, // R0.500, spawn during fight
        LlofiiTheForthright = 0x3236, // R0.500, x1
        Monoceros = 0x3237, // R1.800, x1
        PurifyingLight = 0x1EB173, // R0.500, EventObj type, spawn during fight
        LivingCorpseSpawn = 0x1EB07A, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 24692, // Boss->player, no cast, single-target
        AutoAttackHernais = 6497, // HernaisTheTenacious->player, no cast, single-target
        AutoAttackWraith = 6498, // WarWraith->player, no cast, single-target

        DevourSoul = 24093, // Boss->player, 5.0s cast, single-target, tankbuster
        Blight = 24094, // Boss->self, 5.0s cast, single-target, visual
        BlightAOE = 24095, // Helper->self, 5.0s cast, ???, raidwide
        GallowsMarch = 24096, // Boss->self, 3.0s cast, single-target, visual (applies doom and forced march)
        LivingCorpse = 24097, // Boss->self, 3.0s cast, single-target, visual
        ChainMagick = 24098, // Boss->self, 3.0s cast, single-target, applies dualcast for next soul purge
        SoulPurgeCircle = 24099, // Boss->self, 5.0s cast, range 10 circle
        SoulPurgeCircleDual = 24100, // Boss->self, no cast, range 10 circle
        SoulPurgeDonut = 24101, // Boss->self, 5.0s cast, range 10-30 donut
        SoulPurgeDonutDual = 24102, // Boss->self, no cast, range 10-30 donut
        CrimsonBlade = 24103, // HernaisTheTenacious->self, 8.0s cast, range 50 180-degree cone aoe
        //BloodCyclone = 21104, // HernaisTheTenacious->self, 3.0s cast, ???
        Aethertide = 24105, // DyunbuTheAccursed->self, 8.0s cast, single-target, visual
        AethertideAOE = 24106, // Helper->players, 8.0s cast, range 8 circle spread
        MarchingBreath = 24107, // DyunbuTheAccursed->self, 8.0s cast, interruptible, ???
        TacticalStone = 24108, // DyunbuTheAccursed->player, 2.5s cast, single-target, autoattack
        TacticalAero = 24109, // DyunbuTheAccursed->self, 3.0s cast, range 40 width 8 rect
        //Enrage = 24110, // DyunbuTheAccursed->self, 3.0s cast, ???
        EntropicFlame = 24111, // WarWraith->self, 4.0s cast, range 60 width 8 rect
        //DarkFlare = 24112, // WarWraith->location, 5.0s cast, ???
        //SoulSacrifice = 24113, // WarWraith->Boss, 6.0s cast, interruptible, ???

        DeadlyToxin = 24699, // Deathwall->self, no cast, range 25-30 donut, deathwall
        Shock = 24114, // ShockSphere->self, no cast, range 7 circle aoe around sphere

        AutoAttackMonoceros = 871, // Monoceros->Boss, no cast, single-target
        PurifyingLight = 24115, // Monoceros->location, 11.0s cast, range 12 circle, visual
        PurifyingLightAOE = 24116, // Helper->location, no cast, range 12 circle, cleanse doom
        Ruin = 24119, // LlofiiTheForthright->Boss, 2.5s cast, single-target, autoattack
        Cleanse = 24969, // LlofiiTheForthright->location, 5.0s cast, range 6 circle, damages boss
        SoothingGlimmer = 24970, // LlofiiTheForthright->self, 2.5s cast, single-target, heal
    };

    public enum SID : uint
    {
        ForwardMarch = 2161, // Boss->player, extra=0x0
        AboutFace = 2162, // Boss->player, extra=0x0
        LeftFace = 2163, // Boss->player, extra=0x0
        RightFace = 2164, // Boss->player, extra=0x0
        ForcedMarch = 1257, // Boss->player, extra=0x2/0x1/0x8/0x4
    };

    class DevourSoul : Components.SingleTargetCast
    {
        public DevourSoul() : base(ActionID.MakeSpell(AID.DevourSoul)) { }
    }

    class Blight : Components.RaidwideCast
    {
        public Blight() : base(ActionID.MakeSpell(AID.Blight)) { }
    }

    class GallowsMarch : Components.StatusDrivenForcedMarch
    {
        public GallowsMarch() : base(3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => !module.FindComponent<PurifyingLight>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? true;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.GallowsMarch) ?? false)
                hints.Add("Apply doom & march debuffs");
        }
    }

    class ShockSphere : Components.PersistentVoidzone
    {
        public ShockSphere() : base(7, m => m.Enemies(OID.ShockSphere)) { }
    }

    class SoulPurge : Components.GenericAOEs
    {
        private bool _dualcast;
        private List<AOEInstance> _imminent = new();

        private static AOEShapeCircle _shapeCircle = new(10);
        private static AOEShapeDonut _shapeDonut = new(10, 30);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _imminent.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ChainMagick:
                    _dualcast = true;
                    break;
                case AID.SoulPurgeCircle:
                    SetupImminentAOEs(_shapeCircle, _shapeDonut, caster.Position, spell.FinishAt);
                    break;
                case AID.SoulPurgeDonut:
                    SetupImminentAOEs(_shapeDonut, _shapeCircle, caster.Position, spell.FinishAt);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.SoulPurgeCircle or AID.SoulPurgeCircleDual or AID.SoulPurgeDonut or AID.SoulPurgeDonutDual && _imminent.Count > 0)
                _imminent.RemoveAt(0);
        }

        private void SetupImminentAOEs(AOEShape main, AOEShape dual, WPos center, DateTime activation)
        {
            _imminent.Add(new(main, center, activation: activation));
            if (_dualcast)
            {
                _imminent.Add(new(dual, center, activation: activation.AddSeconds(2.1f)));
                _dualcast = false;
            }
        }
    }

    class CrimsonBlade : Components.SelfTargetedAOEs
    {
        public CrimsonBlade() : base(ActionID.MakeSpell(AID.CrimsonBlade), new AOEShapeCone(50, 90.Degrees())) { }
    }

    class Aethertide : Components.SpreadFromCastTargets
    {
        public Aethertide() : base(ActionID.MakeSpell(AID.AethertideAOE), 8) { }
    }

    // TODO: no idea what it does if not interrupted...
    class MarchingBreath : Components.CastHint
    {
        public MarchingBreath() : base(ActionID.MakeSpell(AID.MarchingBreath), "Interrupt") { }
    }

    class TacticalAero : Components.SelfTargetedAOEs
    {
        public TacticalAero() : base(ActionID.MakeSpell(AID.TacticalAero), new AOEShapeRect(40, 4)) { }
    }

    class EntropicFlame : Components.SelfTargetedAOEs
    {
        public EntropicFlame() : base(ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(60, 4)) { }
    }

    class PurifyingLight : Components.LocationTargetedAOEs
    {
        public PurifyingLight() : base(ActionID.MakeSpell(AID.PurifyingLight), 12)
        {
            Color = ArenaColor.SafeFromAOE;
            Risky = false;
        }
    }

    // TODO: there are some spells that i've not seen effects of (due to being interrupted or adds killed before they could finish casting)
    class CE42FromBeyondTheGraveStates : StateMachineBuilder
    {
        public CE42FromBeyondTheGraveStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<DevourSoul>()
                .ActivateOnEnter<Blight>()
                .ActivateOnEnter<GallowsMarch>()
                .ActivateOnEnter<ShockSphere>()
                .ActivateOnEnter<SoulPurge>()
                .ActivateOnEnter<CrimsonBlade>()
                .ActivateOnEnter<Aethertide>()
                .ActivateOnEnter<MarchingBreath>()
                .ActivateOnEnter<TacticalAero>()
                .ActivateOnEnter<EntropicFlame>()
                .ActivateOnEnter<PurifyingLight>();
        }
    }

    public class CE42FromBeyondTheGrave : BossModule
    {
        public CE42FromBeyondTheGrave(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-60, 800), 30)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(Enemies(OID.WarWraith), ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.HernaisTheTenacious), ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.DyunbuTheAccursed), ArenaColor.Enemy);
        }
    }
}
