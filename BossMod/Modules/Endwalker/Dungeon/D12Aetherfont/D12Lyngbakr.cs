using System;
using System.Collections.Generic;
using Lumina.Data.Parsing.Layer;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-the-aetherfont-dungeon-guide-walkthrough/
https://ffxiv.consolegameswiki.com/wiki/The_Aetherfont
https://gamerescape.com/2023/05/25/ffxiv-endwalker-guide-the-aetherfont/
*/

namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D12Lyngbakr
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */
    public enum OID : uint
    {
        Boss = 0x3EEB, // ???
        Helper = 0x233C, // ???
        Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
        Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
        Actor1eb882 = 0x1EB882, // R0.500, EventObj type, spawn during fight
        Actor1eb883 = 0x1EB883, // R0.500, EventObj type, spawn during fight
    };
    public enum AID : uint
    {
        BodySlam = 33335,           //done. Boss->self, 3.0s cast, range 40 circle Lyngbakr moves to the center of the area and unleashes a arena-wide AoE with moderate damage to the entire party and spawn crystals of various sizes that will cast an AoE dependent on the crystals size once Upsweep is cast.
        SonicBloop = 33345,         //done. Boss->none, 5.0s cast, single-target Moderate damage tank buster.
        ExplosiveFrequency = 33340, //done. Helper->self, 10.0s cast, range 15 circle i think this is the exploding crystals
        ResonantFrequency = 33339,  //done. Helper->self, 5.0s cast, range 8 circle
        Floodstide = 33341,         //ignored. its actualyl waterspout? Boss->self, 3.0s cast, single-target Casts AoE markers on all party members that do light damage
        TidalBreath = 33344,        //done. Boss->self, 5.0s cast, range 40 180-degree cone Lyngbakr turns around and casts a Large arena-wide AoE with high damage to anyone not behind them.
        Tidalspout = 33343,         //done. Helper->none, 5.0s cast, range 6 circle
        Upsweep = 33338,            //done. Boss->self, 5.0s cast, range 40 circle Moderate damage to the entire party. Will cause any crystals formed to cast a wide or small AoE dependent on crystal size.
        Waterspout = 33342,         //done. Helper->player, 5.0s cast, range 5 circle THIS IS THE SPREADER
    };

    public enum SID : uint
    {
        VulnerabilityUp = 1789, // Boss->player, extra=0x1
    };

    public enum IconID : uint
    {
        Icon_96 = 96, // player/33CE/33CF/3DC2
        Icon_161 = 161, // 33CF
        Icon_218 = 218, // 33CE
    };
    class SonicBloop : Components.SingleTargetCast
    {
        public SonicBloop() : base(ActionID.MakeSpell(AID.SonicBloop)) { }
    }
    class Waterspout : Components.SpreadFromCastTargets
    {
        public Waterspout() : base(ActionID.MakeSpell(AID.Waterspout),5) { }
    }
    class TidalBreath : Components.SelfTargetedAOEs
    {
        public TidalBreath() : base(ActionID.MakeSpell(AID.TidalBreath), new AOEShapeCone(40, 90.Degrees())) { }
    }
    class Tidalspout : Components.SelfTargetedAOEs
    {
        public Tidalspout() : base(ActionID.MakeSpell(AID.Tidalspout), new AOEShapeCircle(6)) { }
    }
    class Upsweep : Components.RaidwideCast
    {
        public Upsweep() : base(ActionID.MakeSpell(AID.Upsweep)) { }
    }
    class BodySlam : Components.RaidwideCast
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam)) { }
    }
    class ExplosiveFrequency : Components.SelfTargetedAOEs
    {
        public ExplosiveFrequency() : base(ActionID.MakeSpell(AID.ExplosiveFrequency), new AOEShapeCircle(15)) { }
    }
    class ResonantFrequency : Components.SelfTargetedAOEs
    {
        public ResonantFrequency() : base(ActionID.MakeSpell(AID.ResonantFrequency), new AOEShapeCircle(8)) { }
    }
    class D12LyngbakrStates : StateMachineBuilder
    {
        public D12LyngbakrStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<SonicBloop>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Tidalspout>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Upsweep>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<ExplosiveFrequency>()
            .ActivateOnEnter<ResonantFrequency>();
        }
    }

    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */
    [ModuleInfo(CFCID = 822, NameID = 12336)]
    public class D12Lyngbakr : BossModule
    {
        public D12Lyngbakr(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-322, 120), 20)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }
    }
}