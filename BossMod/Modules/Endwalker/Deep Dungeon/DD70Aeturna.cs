using System;
using System.Collections.Generic;
using static BossMod.ActorCastEvent;
using static BossMod.BossComponent;

namespace BossMod.Endwalker.DeepDungeon.DD70Aeturna
{
    public enum OID : uint
    {
        AllaganCrystal = 0x3D1C, // R1.500, x4
        Helper = 0x233C, // R0.500, x12, 523 type
        Boss = 0x3D1B, // R5.950, x1
        //_Gen_UneiDemiclone = 0x3E1A, // R0.600, spawn during fight
    };


    public enum AID : uint
    {
        //AllaganCureII = 32811,               // 3E1A->player, 1.5s cast, single-target
        //AllaganStoneIII = 32808,             // 3E1A->3D1B, 1.5s cast, single-target
        //AllaganStoneskin = 32810,            // 3E1A->self, 1.5s cast, range 30 circle
        //AllaganTornado = 32809,              // 3E1A->3D1B, 1.5s cast, single-target
        BossAutoAttack = 6497,                 // 3D1B->player, no cast, single-target
        FallingRock = 31441,                   // 233C->self, 2.5s cast, range 3 circle // done
        Ferocity = 31442,                      // 3D1B->self, 5.0s cast, single-target // done
        FerocityTetherStretchSuccess = 31443,  // 3D1B->player, no cast, single-target // done
        FerocityTetherStretchFail = 31444,     // 3D1B->player, no cast, single-target // done
        ImpactAOE = 31438,                     // 3D1C->self, 2.5s cast, range 5 circle
        PreternaturalTurnCirc = 31436,         // 3D1B->self, 6.0s cast, range 15 circle // done
        PreternaturalTurnDonut = 31437,        // 3D1B->self, 6.0s cast, range ?-30 donut // done
        Roar = 31435,                          // 3D1B->self, 5.0s cast, range 60 circle // Roomwide // Done
        ShatterDonutAOE = 31439,               // 3D1C->self, 3.0s cast, range 8 circle // Done
        ShatterCircleAOE = 31440,              // 3D1C->self, 2.5s cast, range 18+R 150-degree cone // Done
        SteelClaw = 31445,                     // 3D1B->player, 5.0s cast, single-target // Done
        Unknown = 31446,                       // 3D1B->location, no cast, single-target
    };
    public enum IconID : uint
    {
        tankbuster = 198, // player
    };

    public enum TetherID : uint
    {
        FerocityTetherGood = 1, // Boss->player
        FerocityTetherStretch = 57, // Boss->player
    };

    class SteelClaw : Components.SingleTargetCast
    {
        public SteelClaw() : base(ActionID.MakeSpell(AID.SteelClaw), "Tankbuster") { }
    }

    class FerocityGood : Components.BaitAwayTethers  //TODO: consider generalizing stretched tethers?
    {
        private ulong target;
        public FerocityGood() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.FerocityTetherGood) { }
        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            base.OnTethered(module, source, tether);
            if (tether.ID == (uint)TetherID.FerocityTetherGood)
                target = tether.Target;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
            {
                foreach (var b in ActiveBaits)
                {
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                    arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Safe);
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (target == actor.InstanceID && CurrentBaits.Count > 0)
                hints.Add("Tether is stretched!", false);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor.InstanceID && CurrentBaits.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
        }
    }

    class FerocityBad : Components.BaitAwayTethers
    {
        private ulong target;
        public FerocityBad() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.FerocityTetherStretch) { }
        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            base.OnTethered(module, source, tether);
            if (tether.ID == (uint)TetherID.FerocityTetherStretch)
                target = tether.Target;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
            {
                foreach (var b in ActiveBaits)
                {
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                    arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (target == actor.InstanceID && CurrentBaits.Count > 0)
                hints.Add("Stretch tether further!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor.InstanceID && CurrentBaits.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
        }
    }

    class PreternaturalTurnCirc : Components.SelfTargetedAOEs
    {
        public PreternaturalTurnCirc() : base(ActionID.MakeSpell(AID.PreternaturalTurnCirc), new AOEShapeCircle(15), 2) { }
    }

    class PreternaturalTurnDonut : Components.SelfTargetedAOEs
    {
        public PreternaturalTurnDonut() : base(ActionID.MakeSpell(AID.PreternaturalTurnDonut), new AOEShapeDonut(5, 30)) { }
    }

    class Roar : Components.RaidwideCast
    {
        public Roar() : base(ActionID.MakeSpell(AID.Roar)) { }
    }

    class ShatterDonutAOE : Components.SelfTargetedAOEs
    {
        public ShatterDonutAOE() : base(ActionID.MakeSpell(AID.ShatterDonutAOE), new AOEShapeCircle(8)) { }
    }

    class ShatterCircleAOE : Components.SelfTargetedAOEs
    {
        public ShatterCircleAOE() : base(ActionID.MakeSpell(AID.ShatterCircleAOE), new AOEShapeCone(18, 75.Degrees())) { }
    }

    class FallingRock : Components.SelfTargetedAOEs
    {
        public FallingRock() : base(ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3)) { }
    }

    class ImpactAOE : Components.SelfTargetedAOEs
    {
        public ImpactAOE() : base(ActionID.MakeSpell(AID.ImpactAOE), new AOEShapeCircle(5)) { }
    }

    class DD70AeturnaStates : StateMachineBuilder
    {
        public DD70AeturnaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SteelClaw>()
                .ActivateOnEnter<FerocityGood>()
                .ActivateOnEnter<FerocityBad>()
                .ActivateOnEnter<PreternaturalTurnCirc>()
                .ActivateOnEnter<PreternaturalTurnDonut>()
                .ActivateOnEnter<ShatterDonutAOE>()
                .ActivateOnEnter<ShatterCircleAOE>()
                .ActivateOnEnter<FallingRock>()
                .ActivateOnEnter<ImpactAOE>();
        }
    }

    [ModuleInfo(CFCID = 903, NameID = 12246)]
    public class DD70Aeturna : BossModule
    {
        public DD70Aeturna(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300.008f, -300.008f), 20)) { }
    }
}
