using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: arena bounds
// TODO: enrage (~13 min?)
// TODO: tank swaps on death sentence (is it true that taunt mid cast makes OT eat debuff? is it true that boss can be single-tanked in p2+?)
// TODO: is it possible to plan for death sentence in P2+? specifically, how death sentence timings align with phase change / fireball timings?..
// TODO: boss / neurolink positions
// TODO: P2 positioning for fireballs and conflags (how long will conflags even live?)
// TODO: P3 divebombs
// TODO: P3 hygieia explosion avoidance for people (esp. tank)
// TODO: P3 hygieia priority (keep 2nd at low hp etc)
// TODO: P3 intermission positioning (inside neurolinks)
// TODO: P4 twisters & dreadknights (looks quite simple)
// TODO: P5 liquid hells
// TODO: P5 hatch (OT in center, otherwise target should run to neurolink, others should avoid line between hatch and target/neurolink?)
namespace BossMod.RealmReborn.Raid.T05Twintania
{
    public enum OID : uint
    {
        Boss = 0x7E5, // R5.400, x1
        ScourgeOfMeracydia = 0x7E7, // R3.600, x3
        Neurolink = 0x7E6, // R2.000, spawn during fight
        Conflagration = 0x7F2, // R6.000, spawn during fight
        Hygieia = 0x7E8, // R1.200, spawn during fight
        Asclepius = 0x7E9, // R1.800, spawn during fight
        Dreadknight = 0x7EA, // R1.700, spawn during fight
        Oviform = 0x7F3, // R1.000, spawn during fight

        HelperTwister = 0x8EB, // R0.500, x8
        HelperMarker = 0x8EE, // R0.500, x2
        LiquidHell = 0x1E88FE, // R0.500, EventObj type, spawn during fight
        Twister = 0x1E8910, // R0.500, EventObj type, spawn during fight

        //_Gen_Actor7f1 = 0x7F1, // R1.000, spawn during fight
        //_Gen_AllaganTerminal = 0x1E890B, // R2.000, x1, EventObj type
        //_Gen_Actor1e871a = 0x1E871A, // R2.000, x1, EventObj type
        //_Gen_Actor1e86f7 = 0x1E86F7, // R2.000, x1, EventObj type, and more spawn during fight
        //_Gen_Actor1e86ef = 0x1E86EF, // R2.000, x1, EventObj type
        //_Gen_Actor1e88ff = 0x1E88FF, // R0.500, x0, EventObj type, and more spawn during fight
        //_Gen_Actor1e8ef2 = 0x1E8EF2, // R0.500, x0, EventObj type, and more spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 1461, // Boss->player, no cast, single-target
        AutoAttackAdds = 870, // ScourgeOfMeracydia/Hygieia/Asclepius->player, no cast, single-target
        Plummet = 1240, // Boss->self, no cast, ??? cleave
        LiquidHellAdds = 1243, // ScourgeOfMeracydia->location, 3.0s cast, range 6 circle voidzone
        DeathSentence = 1458, // Boss->player, 2.0s cast, single-target, visual (2.4s cast until last phase)
        DeathSentenceP1 = 1241, // Boss->player, no cast, single-target, tankbuster (only during p1)
        DeathSentenceP2 = 1242, // Boss->player, no cast, single-target, tankbuster + decrease healing received debuff

        FireballMarker = 1452, // HelperMarker->player, no cast, single-target, visual icon for fireball
        FireballAOE = 1246, // Boss->player, no cast, range 4 circle shared aoe
        FirestormMarker = 1451, // HelperMarker->player, no cast, single-target, visual icon for conflagration
        FirestormAOE = 1245, // Conflagration->self, no cast, one-shot if not killed in time

        DivebombMarker = 1456, // HelperMarker->player, no cast, single-target, visual icon for divebomb
        DivebombAOE = 1247, // Boss->self, 1.2s cast, range 35 width 12 rect aoe with knockback 30
        WildRattle = 669, // Asclepius/Hygieia->player, no cast, single-target
        Disseminate = 1212, // Hygieia->self, no cast, radius 8 circle aoe applying vuln debuff on death
        AethericProfusion = 1248, // Boss->self, no cast, raidwide

        Twister = 1249, // Boss->self, 2.4s cast, single-target, visual
        TwisterVisual = 671, // HelperTwister->self, no cast, visual explosion?
        TwisterKill = 1250, // HelperTwister->self, no cast, instant death for anyone caught
        UnwovenWill = 1251, // Boss->player, no cast, single-target, stun & fixate for dreadknight
        CaberToss = 1257, // Dreadknight->player, no cast, single-target, instant death if dreadknight reaches target

        HatchMarker = 1453, // HelperMarker->player, no cast, single-target, visual icon for hatch
        Hatch = 1256, // Oviform->self, no cast, ???
        LiquidHellMarker = 1457, // Boss->player, no cast, single-target, visual icon for liquid hell
        LiquidHellBoss = 670, // Boss->location, no cast, range 6 circle voidzone
    };

    class Plummet : Components.Cleave
    {
        public Plummet() : base(ActionID.MakeSpell(AID.Plummet), new AOEShapeCone(8, 60.Degrees())) { } // TODO: verify shape...
    }

    class LiquidHellAdds : Components.PersistentVoidzoneAtCastTarget
    {
        public LiquidHellAdds() : base(6, ActionID.MakeSpell(AID.LiquidHellAdds), m => m.Enemies(OID.LiquidHell).Where(z => z.EventState != 7), 3, true) { } // note: voidzone appears ~1.2s after cast ends, but we want to try avoiding initial damage too
    }

    // TODO: death sentence - every ~36s
    // TODO: p1->p2 when first neurolink appears, p2->p3 when second neurolink appears, p4->p5 when third neurolink appears

    // TODO: what happens here is marker appears -> 5 liquid hells drop at (0.6 + 1.7*N)s; each liquid hell cast does small damage and spawns voidzone 1.2s later
    //class LiquidHellBoss : Components.PersistentVoidzoneAtCastTarget
    //{
    //    public LiquidHellBoss() : base(6, ActionID.MakeSpell(AID.LiquidHellBoss), m => m.Enemies(OID.LiquidHell).Where(z => z.EventState != 7), 3, true) { } // note: voidzone appears ~1.2s after cast ends, but we want to try avoiding initial damage too
    //}

    class T05TwintaniaStates : StateMachineBuilder
    {
        public T05TwintaniaStates(BossModule module) : base(module)
        {
            TrivialPhase();
            //.ActivateOnEnter<>()
            //;
        }
    }

    public class T05Twintania : BossModule
    {
        public T05Twintania(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-7, 5), 30)) { } // TODO

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);

            hints.UpdatePotentialTargets(e =>
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss => 1,
                    OID.ScourgeOfMeracydia => 2,
                    OID.Conflagration => 2,
                    _ => 0
                };
            });
        }
    }
}
