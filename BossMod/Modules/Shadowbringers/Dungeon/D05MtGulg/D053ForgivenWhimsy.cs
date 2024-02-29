// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D053ForgivenWhimsy
{
    public enum OID : uint
    {
        Boss = 0x27CC, //R=20.00
        Helper = 0x2E8, //R=0.5
        Brightsphere = 0x27CD, //R=1.0
        Towers = 0x1EAACF, //R=0.5
    }

    public enum AID : uint
    {
        AutoAttack = 15624, // 27CC->player, no cast, single-target
        SacramentOfPenance = 15627, // 27CC->self, 4,0s cast, single-target
        SacramentOfPenance2 = 15628, // 233C->self, no cast, range 50 circle
        Reformation = 15620, // 27CC->self, no cast, single-target, boss changes pattern
        ExegesisA = 16989, // 27CC->self, 5,0s cast, single-target
        ExegesisB = 16987, // 27CC->self, 5,0s cast, single-target
        ExegesisC = 15622, // 27CC->self, 5,0s cast, single-target
        ExegesisD = 16988, // 27CC->self, 5,0s cast, single-target
        Exegesis = 15623, // 233C->self, no cast, range 10 width 10 rect
        Catechism = 15625, // 27CC->self, 4,0s cast, single-target
        Catechism2 = 15626, // 233C->player, no cast, single-target
        JudgmentDay = 15631, // 27CC->self, 3,0s cast, single-target, tower circle 5
        Judged = 15633, // 233C->self, no cast, range 5 circle, tower success
        FoundWanting = 15632, // 233C->self, no cast, range 40 circle, tower fail
        RiteOfTheSacrament = 15629, // 27CC->self, no cast, single-target
        PerfectContrition = 15630, // 27CD->self, 6,0s cast, range 5-15 donut
    };

    class Catechism : Components.SingleTargetCast
    {
        public Catechism() : base(ActionID.MakeSpell(AID.Catechism)) { } //Note: actual tb happens about 0.5s later by helper with 0s cast

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
            {
                BitMask targets = new();
                targets.Set(module.Raid.FindSlot(c.CastInfo!.TargetID));
                hints.PredictedDamage.Add((targets, c.CastInfo!.NPCFinishAt.AddSeconds(0.5f)));
            }
        }
    }

    class SacramentOfPenance : Components.RaidwideCast
    {
        public SacramentOfPenance() : base(ActionID.MakeSpell(AID.SacramentOfPenance)) { } //Note: actual raidwide happens about 0.5s later by helper with 0s cast

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), c.CastInfo!.NPCFinishAt.AddSeconds(0.8f)));
        }
    }

    class PerfectContrition : Components.SelfTargetedAOEs
    {
        public PerfectContrition() : base(ActionID.MakeSpell(AID.PerfectContrition), new AOEShapeDonut(5, 15)) { }
    }

    public class JudgmentDay : Components.GenericTowers
    {
        private bool tower1;
        private bool tower2;
        private Actor? Tower1;

        public override void OnActorEState(BossModule module, Actor actor, ushort state)
        {
            if (state is 0x01C or 0x02C)
            {
                if (!tower1)
                {
                    tower1 = true;
                    Towers.Add(new(actor.Position, 5, 1, 1));
                    Tower1 = actor;
                }
                if (tower1 && !tower2 && actor != Tower1)
                {
                    tower2 = true;
                    Towers.Add(new(actor.Position, 5, 1, 1));
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Judged or AID.FoundWanting)
            {
                Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
                if (Towers.Count == 0) //Note: I don't think towers can repeat, this is just a safety precaution
                {
                    tower1 = default;
                    tower2 = default;
                    Tower1 = default;
                }
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Towers.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 5));
            if (Towers.Count > 1)
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
        }
    }

    class Exegesis : Components.GenericAOEs
    {
        private DateTime _activation;
        private bool ExegesisA;
        private bool ExegesisB;
        private bool ExegesisC;
        private bool ExegesisD;
        private static readonly AOEShapeRect rect = new(5, 5, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (ExegesisA) //diagonal squares
            {
                yield return new(rect, new(-240, -50), default, _activation);
                yield return new(rect, new(-250, -40), default, _activation);
                yield return new(rect, new(-230, -40), default, _activation);
                yield return new(rect, new(-250, -60), default, _activation);
                yield return new(rect, new(-230, -60), default, _activation);
            }
            if (ExegesisB) //West+East Square
            {
                yield return new(rect, new(-250, -50), default, _activation);
                yield return new(rect, new(-230, -50), default, _activation);
            }
            if (ExegesisC) //North+South Square
            {
                yield return new(rect, new(-240, -60), default, _activation);
                yield return new(rect, new(-240, -40), default, _activation);
            }
            if (ExegesisD) //cross pattern
            {
                yield return new(rect, new(-230, -50), default, _activation);
                yield return new(rect, new(-240, -60), default, _activation);
                yield return new(rect, new(-240, -40), default, _activation);
                yield return new(rect, new(-250, -50), default, _activation);
                yield return new(rect, new(-240, -50), default, _activation);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ExegesisA:
                    ExegesisA = true;
                    _activation = spell.NPCFinishAt;
                    break;
                case AID.ExegesisB:
                    ExegesisB = true;
                    _activation = spell.NPCFinishAt;
                    break;
                case AID.ExegesisC:
                    ExegesisC = true;
                    _activation = spell.NPCFinishAt;
                    break;
                case AID.ExegesisD:
                    ExegesisD = true;
                    _activation = spell.NPCFinishAt;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ExegesisA or AID.ExegesisB or AID.ExegesisC or AID.ExegesisD)
            {
                ExegesisA = false;
                ExegesisB = false;
                ExegesisC = false;
                ExegesisD = false;
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (ExegesisA || ExegesisB || ExegesisC || ExegesisD)
            {
                arena.AddLine(new(-235, -65), new(-235, -35), ArenaColor.Border, 2);
                arena.AddLine(new(-245, -65), new(-245, -35), ArenaColor.Border, 2);
                arena.AddLine(new(-225, -55), new(-255, -55), ArenaColor.Border, 2);
                arena.AddLine(new(-225, -45), new(-255, -45), ArenaColor.Border, 2);
            }
        }
    }

    class D053ForgivenWhimsyStates : StateMachineBuilder
    {
        public D053ForgivenWhimsyStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Catechism>()
                .ActivateOnEnter<SacramentOfPenance>()
                .ActivateOnEnter<PerfectContrition>()
                .ActivateOnEnter<JudgmentDay>()
                .ActivateOnEnter<Exegesis>();
        }
    }

    [ModuleInfo(CFCID = 659, NameID = 8261)]
    public class D053ForgivenWhimsy : BossModule
    {
        public D053ForgivenWhimsy(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-240, -50), 15)) { }
    }
}
