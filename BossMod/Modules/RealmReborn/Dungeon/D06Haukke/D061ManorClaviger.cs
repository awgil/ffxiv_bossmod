namespace BossMod.RealmReborn.Dungeon.D06Haukke.D061ManorClaviger
{
    public enum OID : uint
    {
        Boss = 0x110, // x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        SweetSteel = 489, // Boss->self, no cast, range 7.4 ?-degree cone cleave
        VoidFire2 = 855, // Boss->location, 3.0s cast, range 5 aoe
        DarkMist = 705, // Boss->self, 4.0s cast, range 9.4 aoe
    };

    class SweetSteel : Components.Cleave
    {
        public SweetSteel() : base(ActionID.MakeSpell(AID.SweetSteel), new AOEShapeCone(7.4f, 45.Degrees())) { } // TODO: verify angle
    }

    class VoidFire2 : Components.LocationTargetedAOEs
    {
        public VoidFire2() : base(ActionID.MakeSpell(AID.VoidFire2), 5) { }
    }

    class DarkMist : Components.SelfTargetedAOEs
    {
        public DarkMist() : base(ActionID.MakeSpell(AID.DarkMist), new AOEShapeCircle(9.4f)) { }
    }

    class D061ManorClavigerStates : StateMachineBuilder
    {
        public D061ManorClavigerStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SweetSteel>()
                .ActivateOnEnter<VoidFire2>()
                .ActivateOnEnter<DarkMist>();
        }
    }

    [ModuleInfo(CFCID = 6, NameID = 423)]
    public class D061ManorClaviger : BossModule
    {
        public D061ManorClaviger(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(2.5f, 0), 16)) { } // TODO: really a rect, x=[-25, +20], y=[-16, +16]
    }
}
