using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.PlanTarget;
using Lumina.Data.Parsing.Layer;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-the-aetherfont-dungeon-guide-walkthrough/
https://ffxiv.consolegameswiki.com/wiki/The_Aetherfont
https://gamerescape.com/2023/05/25/ffxiv-endwalker-guide-the-aetherfont/
*/

namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D12Arkas
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */
    public enum OID : uint
    {
        Boss = 0x3EEA, // ???
        Helper = 0x233C , // ???
    };

    public enum AID : uint
    {
        AutoAttackAttack = 870,      //done.(ignored) Boss->none, no cast, single-target
        BattleCry1 = 33364,          //done. Boss->self, 5.0s cast, range 40 circle
        BattleCry2 = 34605,          //done. Boss->self, 5.0s cast, range 40 circle
        ElectricEruption = 33615,    //done. Boss->self, 5.0s cast, range 40 circle
        Electrify = 33367,           //done. Helper->location, 5.5s cast, range 10 circle
        LightningClaw1 = 33366,      //done. Boss->location, no cast, range 6 circle
        LightningClaw2 = 34712,      //done. Boss->none, 5.0s cast, single-target 
        LightningLeap1 = 33358,      //done. Boss->location, 4.0s cast, single-target         -> 
        LightningLeap2 = 33359,      //done. Boss->location, 5.0s cast, single-target         -> 
        LightningLeap3 = 33360,      //done. Helper->location, 6.0s cast, range 10 circle
        LightningLeap4 = 34713,      //done. Helper->location, 5.0s cast, range 10 circle
        LightningRampage1 = 34318,   //done. Boss->location, 4.0s cast, single-target         -> 
        LightningRampage2 = 34319,   //done. Boss->location, 2.0s cast, single-target         -> 
        LightningRampage3 = 34320,   //done. Boss->location, 2.0s cast, single-target         -> 
        LightningRampage4 = 34321,   //done. Helper->location, 5.0s cast, range 10 circle
        LightningRampage5 = 34714,   //done. Helper->location, 5.0s cast, range 10 circle
        RipperClaw = 33368,          //done. Boss->none, 5.0s cast, single-target
        Shock = 33365,               //done. Helper->location, 3.5s cast, range 6 circle
        SpinningClaw = 33362,        //done. Boss->self, 3.5s cast, range 10 circle
        ForkedFissures = 33361,      //done. Helper->location, 1.0s cast, width 4 rect charge
        SpunLightning = 33363,       //done. Helper->self, 3.5s cast, range 30 width 8 rect
    };

    public enum IconID : uint
    {
        Icon_218 = 218, // 33CE
        Icon_161 = 161, // 3DC2
    };
    class LightningRampage1 : Components.LocationTargetedAOEs
    {
        public LightningRampage1() : base(ActionID.MakeSpell(AID.LightningRampage1),5) { }
    }
    class LightningRampage2 : Components.LocationTargetedAOEs
    {
        public LightningRampage2() : base(ActionID.MakeSpell(AID.LightningRampage2), 5) { }
    }
    class LightningRampage3 : Components.LocationTargetedAOEs
    {
        public LightningRampage3() : base(ActionID.MakeSpell(AID.LightningRampage3), 5) { }
    }
    class LightningLeap1 : Components.LocationTargetedAOEs
    {
        public LightningLeap1() : base(ActionID.MakeSpell(AID.LightningLeap1), 5) { }
    }
    class LightningLeap2 : Components.LocationTargetedAOEs
    {
        public LightningLeap2() : base(ActionID.MakeSpell(AID.LightningLeap2), 5) { }
    }
    class LightningClaw2 : Components.LocationTargetedAOEs
    {
        public LightningClaw2() : base(ActionID.MakeSpell(AID.LightningClaw2), 5) { }
    }
    class SpunLightning : Components.SelfTargetedAOEs
    {
        public SpunLightning() : base(ActionID.MakeSpell(AID.SpunLightning), new AOEShapeRect(4,2,-50)) { }
    }   
    class LightningClaw1 : Components.LocationTargetedAOEs
    {
        public LightningClaw1() : base(ActionID.MakeSpell(AID.LightningClaw1), 6) { }
    }
    class ForkedFissures : Components.ChargeAOEs
    {
        public ForkedFissures() : base(ActionID.MakeSpell(AID.ForkedFissures), 2) { }
    }
    /*class ForkedFissures : Components.GenericAOEs
     {
         private List<AOEInstance> _aoes = new();
         private List<Actor?>? ForkHelper;
         private static readonly AOEShapeRect rect = new(0.5f, 0.25f, 40);
         public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;
         public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
         {
             //ForkHelper = [.. module.Enemies(OID.Helper)];
             //ForkHelper = module.Enemies(OID.Helper).ToList();
             if ((AID)spell.Action.ID == AID.ForkedFissures)
             {
                 foreach (var helper in module.Enemies(OID.Helper))
                 {
                     if (helper != null)
                     {
                         _aoes.Add(new(rect, caster.Position, helper.Rotation, spell.NPCFinishAt));
                         _aoes.SortBy(aoe => aoe.Activation);
                     }
                 }
             }
         }
         public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
         {
             if ((AID)spell.Action.ID is AID.ForkedFissures)
                 _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
         }
     }*/
    /*class ForkedFissures : Components.GenericAOEs
    {
        private Actor? _caster;
        private DateTime _predictedActivation;

        private static readonly AOEShapeRect rect = new(0.5f, 0.25f, 40);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_caster?.CastInfo != null)
                yield return new(rect, _caster.Position, _caster.CastInfo.Rotation, _caster.CastInfo.NPCFinishAt);
        }
    }*/
    class ElectricEruption : Components.RaidwideCast
    {
        public ElectricEruption() : base(ActionID.MakeSpell(AID.ElectricEruption)) { }
    }
    class Electrify : Components.LocationTargetedAOEs
    {
        public Electrify() : base(ActionID.MakeSpell(AID.Electrify), 10) { }
    }
    class LightningLeap3 : Components.LocationTargetedAOEs
    {
        public LightningLeap3() : base(ActionID.MakeSpell(AID.LightningLeap3),10) { }
    }
    class LightningLeap4 : Components.LocationTargetedAOEs
    {
        public LightningLeap4() : base(ActionID.MakeSpell(AID.LightningLeap4), 10) { }
    }
    class LightningRampage5 : Components.LocationTargetedAOEs
    {
        public LightningRampage5() : base(ActionID.MakeSpell(AID.LightningRampage5), 10) { }
    }
    class LightningRampage4 : Components.LocationTargetedAOEs
    {
        public LightningRampage4() : base(ActionID.MakeSpell(AID.LightningRampage4), 10) { }
    }
    class RipperClaw : Components.SingleTargetCast
    {
        public RipperClaw() : base(ActionID.MakeSpell(AID.RipperClaw)) { }
    }
    class Shock : Components.LocationTargetedAOEs
    {
        public Shock() : base(ActionID.MakeSpell(AID.Shock), 6) { }
    }
    class SpinningClaw : Components.SelfTargetedAOEs
    {
        public SpinningClaw() : base(ActionID.MakeSpell(AID.SpinningClaw), new AOEShapeCircle(10)) { }
    }
    class BattleCry1 : Components.RaidwideCast
    {
        public BattleCry1() : base(ActionID.MakeSpell(AID.BattleCry1)) { }
    }
    class BattleCry2 : Components.RaidwideCast
    {
        public BattleCry2() : base(ActionID.MakeSpell(AID.BattleCry1)) { }
    }
    class D12ArkasStates : StateMachineBuilder
    {
        public D12ArkasStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<LightningClaw1>()
            .ActivateOnEnter<LightningClaw2>()
            .ActivateOnEnter<SpunLightning>()
            .ActivateOnEnter<ForkedFissures>()
            .ActivateOnEnter<ElectricEruption>()
            .ActivateOnEnter<Electrify>()
            .ActivateOnEnter<LightningLeap1>()
            .ActivateOnEnter<LightningLeap2>()
            .ActivateOnEnter<LightningLeap3>()
            .ActivateOnEnter<LightningLeap4>()
            .ActivateOnEnter<LightningRampage1>()
            .ActivateOnEnter<LightningRampage2>()
            .ActivateOnEnter<LightningRampage3>()
            .ActivateOnEnter<LightningRampage4>()
            .ActivateOnEnter<LightningRampage5>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<SpinningClaw>()
            .ActivateOnEnter<BattleCry1>()
            .ActivateOnEnter<BattleCry2>();
        }
    }

    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */
    [ModuleInfo(CFCID = 822, NameID = 12337)]
    public class D12Arkas : BossModule
    {
        public D12Arkas(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(425, -440), 12)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }
    }
}
