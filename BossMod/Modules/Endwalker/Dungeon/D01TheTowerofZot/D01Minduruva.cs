using System;
using System.Collections.Generic;
using Lumina.Data.Parsing.Layer;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-endwalker-tower-of-zot-dungeon-guide-walkthrough/
*/

namespace BossMod.Endwalker.Dungeon.D01TheTowerofZot.D01Minduruva
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */
    public enum OID : uint
    {
        Boss = 0x33EE, // R2.040, x1
        Helper = 0x233C, // R0.500, x34, 523 type
        Actor1e8f2f = 0x1E8F2F, // R0.500, x2, EventObj type
        Actor1ea1a1 = 0x1EA1A1, // R2.000, x4, EventObj type, and more spawn during fight
    };

    public enum AID : uint
    {
        Ability_UnknownB1 = 25241,       // Boss->location, no cast, single-target 
        Ability_UnknownB1b = 25243,      // Helper->Boss, 3.6s cast, single-target
        Attack = 870,                    // Boss->none, no cast, single-target
        Dhrupad = 25244,                 //done. Boss->self, 4.0s cast, single-target
        ManusyaBio = 25248,              // Boss->none, 4.0s cast, single-target
        ManusyaBioIII1 = 25236,          // Boss->self, 4.0s cast, single-target
        ManusyaBioIII2 = 25240,          //done. Helper->self, 4.0s cast, range 40+R 180-degree cone
        ManusyaBlizzard = 25246,         // Boss->none, no cast, single-target
        ManusyaBlizzardIII1 = 25234,     // Boss->self, 4.0s cast, single-target
        ManusyaBlizzardIII2 = 25238,     //done. Helper->self, 4.0s cast, range 40+R ?-degree cone this is the alternating lines of ice that you have to dodge
        ManusyaFire1 = 25245,            // Boss->player, no cast, single-target
        ManusyaFire2 = 25699,            // Boss->none, 2.0s cast, single-target
        ManusyaFireIII1 = 25233,         // Boss->self, 4.0s cast, single-target
        ManusyaFireIII2 = 25237,         //done. Helper->self, 4.0s cast, range ?-40 donut
        ManusyaThunder = 25247,          // Boss->player, no cast, single-target
        ManusyaThunderIII1 = 25235,      // Boss->self, 4.0s cast, single-target
        ManusyaThunderIII2 = 25239,      //done. Helper->self, 4.0s cast, range 3 circle //this might be the donut
        TransmuteBlizzardIII = 25371,    // Boss->self, 2.7s cast, single-target
        TransmuteFireIII = 25242,        // Boss->self, 2.7s cast, single-target
        TransmuteThunderIII = 25372,     // Boss->self, 2.7s cast, single-target
        TransmuteBioIII = 25373,         // Boss->self, 2.7s cast, single-target
    };

    public enum SID : uint
    {
        IceAlchemy = 2752, // Boss->Boss, extra=0x0
        Burns = 2082, // Boss->316C/player, extra=0x0
        Frostbite = 2083, // Boss->33CD/316C, extra=0x0
        Electrocution = 2086, // Boss->player/33CD/316C, extra=0x0
        FireAlchemy = 2751, // Boss->Boss, extra=0x0
        Poison = 18, // Boss->33CE, extra=0x0
        ThunderAlchemy = 2753, // Boss->Boss, extra=0x0

    };
    public enum IconID : uint
    {
        Icon_198 = 198, // 33CE
    };

    class Dhrupad : Components.SingleTargetCast
    {
        public Dhrupad() : base(ActionID.MakeSpell(AID.Dhrupad)) { }
    }
    class ManusyaThunderIII2 : Components.SelfTargetedAOEs{
        public ManusyaThunderIII2() : base(ActionID.MakeSpell(AID.ManusyaThunderIII2), new AOEShapeCircle(3),3) { }
    }
    class ManusyaBioIII2 : Components.SelfTargetedAOEs{ //targets tank
        public ManusyaBioIII2() : base(ActionID.MakeSpell(AID.ManusyaBioIII2), new AOEShapeCone(40,90.Degrees())) { }
    }
    class ManusyaBlizzardIII2 : Components.SelfTargetedAOEs{
        public ManusyaBlizzardIII2() : base(ActionID.MakeSpell(AID.ManusyaBlizzardIII2), new AOEShapeCone(40,10.Degrees())) { }
    }
    class ManusyaFireIII2 : Components.SelfTargetedAOEs{
       public ManusyaFireIII2() : base(ActionID.MakeSpell(AID.ManusyaFireIII2), new AOEShapeDonut(2,40)) { }
       /*public WPos ManusyaFireHack;
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastFinished(module, caster, spell);
            //ManusyaFireHack = spell.LocXZ;
            ManusyaFireHack = caster.Position;
        }
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (ManusyaFireHack.X != 0 || ManusyaFireHack.Z != 0) {
                base.AddAIHints(module, slot, actor, assignment, hints);
                hints.
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(ManusyaFireHack, 4));
            }
        }*/
    }
    class FireAlchemyResolver : BossComponent
    {
        public int ActiveDebuffs { get; private set; }
        public bool FireAlchMove = false;
        public WPos FireAlchSafeSpot = new WPos();
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID is SID.FireAlchemy)
            {
                //send the player to the grillboss
                // Add forbidden zone using the calculated function with current time
                FireAlchMove = true;
            }

        }
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            FireAlchSafeSpot = module.PrimaryActor.Position;
            if (FireAlchMove == true)
            {
                hints.Clear(); //dont think we need this? will test
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.PrimaryActor.Position, 0.5f));

            }
        }
        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            DrawCircles(arena);
        }

        private void DrawCircles(MiniArena arena)
        {
            if (FireAlchMove == true)
            {
                //arena.AddCircleFilled(new WPos(IsShe.X, IsShe.Z), 2.5f, 0xFF404040);
                arena.AddCircleFilled(new WPos(FireAlchSafeSpot.X, FireAlchSafeSpot.Z), 2.5f, ArenaColor.SafeFromAOE);
            }
            if (FireAlchMove == false)
            {
                //arena.AddCircleFilled(new WPos(IsShe.X, IsShe.Z), 2.5f, ArenaColor.SafeFromAOE);
                arena.AddCircleFilled(new WPos(FireAlchSafeSpot.X, FireAlchSafeSpot.Z), 2.5f, ArenaColor.Background);
            }
        }
    }
    class D01MinduruvaStates : StateMachineBuilder
    {
        public D01MinduruvaStates(BossModule module) : base(module)
        {
            TrivialPhase()
            //.ActivateOnEnter<Dhrupad>() //testing with this disabled next run
            //.ActivateOnEnter<FireAlchemyResolver>()
            .ActivateOnEnter<ManusyaThunderIII2>()
            .ActivateOnEnter<ManusyaFireIII2>()
            .ActivateOnEnter<ManusyaBioIII2>()
            .ActivateOnEnter<ManusyaBlizzardIII2>();
        }
    }
    //Seems to fail on the donut thing goes to wrong spot?
    //it doesnt prioritize     class ManusyaFireIII2 : Components.SelfTargetedAOEs{

    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */
    [ModuleInfo(CFCID = 783, NameID = 10256)]
    public class D01Minduruva : BossModule
    {
        public D01Minduruva(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(68.5f, -124.5f), 20)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, true);
        }
    }
}