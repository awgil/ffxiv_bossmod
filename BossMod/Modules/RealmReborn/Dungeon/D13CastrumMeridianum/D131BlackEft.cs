namespace BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D131BlackEft
{
    public enum OID : uint
    {
        Boss = 0x38CA, // x1
        Helper = 0x233C, // x3
        AddSignifer = 0x38CC, // x2, and more spawn during fight
        AddLaquearius = 0x38CB, // x2, and more spawn during fight
        AddMagitekColossus = 0x394C, // spawn during fight
    };

    public enum AID : uint
    {
        PhotonStream = 28776, // Boss->player, no cast, single-target, auto-attack
        IncendiarySupport = 29268, // Boss->self, 3.0s cast, single-target, visual
        IncendiarySupportAOE = 29269, // Helper->self, no cast, range 60 circle, raidwide
        HighPoweredMagitekRay = 28773, // Boss->self, 5.0s cast, range 60 width 4 rect aoe
        RequestAssistance = 28774, // Boss->self, 4.0s cast, single-target, summon adds
        MagitekCannon = 28775, // Boss->location, 3.0s cast, range 6 circle aoe
    };

    class IncendiarySupport : Components.CastHint
    {
        public IncendiarySupport() : base(ActionID.MakeSpell(AID.IncendiarySupport), "Raidwide x3") { }
    }

    class HighPoweredMagitekRay : Components.SelfTargetedAOEs
    {
        public HighPoweredMagitekRay() : base(ActionID.MakeSpell(AID.HighPoweredMagitekRay), new AOEShapeRect(50, 2)) { }
    }

    class MagitekCannon : Components.LocationTargetedAOEs
    {
        public MagitekCannon() : base(ActionID.MakeSpell(AID.MagitekCannon), 6) { }
    }

    class D131BlackEftStates : StateMachineBuilder
    {
        public D131BlackEftStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<IncendiarySupport>()
                .ActivateOnEnter<HighPoweredMagitekRay>()
                .ActivateOnEnter<MagitekCannon>();
        }
    }

    // adds:
    // initial = 2x signifier + 2x laquearius
    // first wave = 3x signifier + 3x laquearius
    // second wave = 2x colossus
    // third wave = 2x colossus + 2x signifier + 2x laquearius
    [ModuleInfo(CFCID = 15, NameID = 557)]
    public class D131BlackEft : BossModule
    {
        public D131BlackEft(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(10, -40), 20)) { }
    }
}
