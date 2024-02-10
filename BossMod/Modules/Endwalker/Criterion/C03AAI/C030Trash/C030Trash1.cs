namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1
{
    public enum OID : uint
    {
        NKiwakin = 0x40C8, // R3.750, x1
        NSnipper = 0x40C9, // R3.600, x1
        NCrab = 0x40CA, // R1.120, x2
        NMonk = 0x40CB, // R3.000, x1
        NRay = 0x40CC, // R3.200, x1
        NPaddleBiter = 0x40CD, // R1.650, x2

        SKiwakin = 0x40D2, // R3.750, x1
        SSnipper = 0x40D3, // R3.600, x1
        SCrab = 0x40D4, // R1.120, x2
        SMonk = 0x40D5, // R3.000, x1
        SRay = 0x40D6, // R3.200, x1
        SPaddleBiter = 0x40D7, // R1.650, x2

        Helper = 0x233C, // R0.500, 523 type, spawn during fight
        Twister = 0x40CE, // R1.500, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack1 = 31318, // *Kiwakin/*Crab/*Snipper->player, no cast, single-target
        AutoAttack2 = 31320, // *PaddleBiter/*Ray/*Monk->player, no cast, single-target
        NTwister = 35776, // Helper->self, 0.5s cast, range 6 circle
        STwister = 35791, // Helper->self, 0.5s cast, range 6 circle
        // kiwakin
        NLeadHook = 35950, // NKiwakin->player, 4.0s cast, single-target, 3-hit tankbuster
        NLeadHookAOE1 = 35938, // NKiwakin->player, no cast, single-target, tankbuster second hit
        NLeadHookAOE2 = 35923, // NKiwakin->player, no cast, single-target, tankbuster third hit
        NSharpStrike = 35939, // NKiwakin->player, 5.0s cast, single-target, tankbuster
        NTailScrew = 35768, // NKiwakin->location, 5.0s cast, range 4 circle
        SLeadHook = 35783, // SKiwakin->player, 4.0s cast, single-target, 3-hit tankbuster
        SLeadHookAOE1 = 35782, // SKiwakin->player, no cast, single-target, tankbuster second hit
        SLeadHookAOE2 = 35924, // SKiwakin->player, no cast, single-target, tankbuster third hit
        SSharpStrike = 35784, // SKiwakin->player, 5.0s cast, single-target, tankbuster
        STailScrew = 35785, // SKiwakin->location, 5.0s cast, range 4 circle
        // snipper/crabs
        NWater = 35940, // NSnipper->players, 5.0s cast, range 8 circle stack
        NBubbleShower = 35769, // NSnipper->self, 5.0s cast, range 9 90-degree cone
        NCrabDribble = 35770, // NSnipper->self, 1.5s cast, range 6 120-degree cone
        SWater = 35788, // SSnipper->players, 5.0s cast, range 8 circle stack
        SBubbleShower = 35786, // SSnipper->self, 5.0s cast, range 9 90-degree cone
        SCrabDribble = 35787, // SSnipper->self, 1.5s cast, range 6 120-degree cone
        // ray/paddle-biters
        NHydrocannon = 35773, // NRay->self, 5.0s cast, range 15 width 6 rect
        NExpulsion = 35775, // NRay->self, 5.0s cast, range 8 circle
        NElectricWhorl = 35774, // NRay->self, 5.0s cast, range 8-60 donut
        SHydrocannon = 35915, // SRay->self, 5.0s cast, range 15 width 6 rect
        SExpulsion = 35790, // SRay->self, 5.0s cast, range 8 circle
        SElectricWhorl = 35789, // SRay->self, 5.0s cast, range 8-60 donut
        // monk
        NHydroshot = 35941, // NMonk->player, 5.0s cast, single-target
        NCrossAttack = 35771, // NMonk->player, 5.0s cast, single-target tankbuster
        SHydroshot = 35793, // SMonk->player, 5.0s cast, single-target
        SCrossAttack = 35919, // SMonk->player, 5.0s cast, single-target tankbuster
    };

    class Twister : Components.Adds
    {
        public Twister() : base((uint)OID.Twister) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in Actors)
                arena.ZoneCircle(twister.Position, 6, ArenaColor.AOE);
        }
    }

    public abstract class C030Trash1 : BossModule
    {
        public C030Trash1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 100), 20, 30)) { }
    }
}
