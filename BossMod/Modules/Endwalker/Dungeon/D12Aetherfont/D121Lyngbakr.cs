// CONTRIB: made by dhoggpt, not checked
namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D121Lyngbakr
{
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
        BodySlam = 33335, // Boss->self, 3.0s cast, range 40 circle Lyngbakr moves to the center of the area and unleashes a arena-wide AoE with moderate damage to the entire party and spawn crystals of various sizes that will cast an AoE dependent on the crystals size once Upsweep is cast.
        SonicBloop = 33345, // Boss->none, 5.0s cast, single-target Moderate damage tank buster.
        ExplosiveFrequency = 33340, // Helper->self, 10.0s cast, range 15 circle i think this is the exploding crystals
        ResonantFrequency = 33339, // Helper->self, 5.0s cast, range 8 circle
        Floodstide = 33341, // Boss->self, 3.0s cast, single-target Casts AoE markers on all party members that do light damage
        TidalBreath = 33344, // Boss->self, 5.0s cast, range 40 180-degree cone Lyngbakr turns around and casts a Large arena-wide AoE with high damage to anyone not behind them.
        Tidalspout = 33343, // Helper->none, 5.0s cast, range 6 circle
        Upsweep = 33338, // Boss->self, 5.0s cast, range 40 circle Moderate damage to the entire party. Will cause any crystals formed to cast a wide or small AoE dependent on crystal size.
        Waterspout = 33342, // Helper->player, 5.0s cast, range 5 circle THIS IS THE SPREADER
    };

    class SonicBloop : Components.StackWithCastTargets
    {
        public SonicBloop() : base(ActionID.MakeSpell(AID.SonicBloop),3,4) { }
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

    class D121LyngbakrStates : StateMachineBuilder
    {
        public D121LyngbakrStates(BossModule module) : base(module)
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

    [ModuleInfo(CFCID = 822, NameID = 12336)]
    public class D121Lyngbakr : BossModule
    {
        public D121Lyngbakr(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-322, 120), 20)) { }
    }
}
