// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.PVP.HiddenGorge.GoblinMercenary
{
    public enum OID : uint
    {
        Boss = 0x25FA, //R=2.0
        BossHelper = 0x233C,
    };

public enum AID : uint
{
    IronKiss = 14562, // 233C->location, 5,0s cast, range 7 circle
    GobfireShootypopsCCW = 14563, // 25FA->self, 5,0s cast, range 30+R width 6 rect
    GobfireShootypops = 14564, // 25FA->self, no cast, range 30+R width 6 rect
    unknown = 14567, // 233C->self, 1,0s cast, single-target
    Plannyplot = 14558, // 25FA->self, 4,0s cast, single-target
    GobspinWhooshdrops = 14559, // 25FA->self, no cast, range 8 circle
    unknown2 = 14568, // BossHelper->self, 1,0s cast, single-target
    GobswipeConklops = 14560, // Boss->self, no cast, range 30 circle, knockback 15 away from source
};
    public enum IconID : uint
    {
       RotationCW = 168, // Boss
       RotationCCW = 167, // Boss
    };

    class GobfireShootypops : Components.SimpleRotationAOE
    {
        public GobfireShootypops() : base(ActionID.MakeSpell(AID.GobfireShootypopsCCW), ActionID.MakeSpell(AID.GobfireShootypops), default, default, new AOEShapeRect(32,3), 6, 60.Degrees(), 0.Degrees(), true) { }
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if(iconID == (uint)IconID.RotationCW)
                RotationDirIncrement = -60.Degrees();
            if(iconID == (uint)IconID.RotationCCW)
                RotationDirIncrement = 60.Degrees();
        }
    }
    class IronKiss : Components.LocationTargetedAOEs
    {
        public IronKiss() : base(ActionID.MakeSpell(AID.IronKiss), 7) { }
    }
    class GoblinMercenaryStates : StateMachineBuilder
    {
        public GoblinMercenaryStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<IronKiss>()
                .ActivateOnEnter<GobfireShootypops>();
        }
    }

    [ModuleInfo(CFCID = 599, NameID = 7906)]
    public class GoblinMercenary : BossModule
    {
        public GoblinMercenary(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0,0), 0))  {}

        protected override void UpdateModule()
        {
            if (Enemies(OID.Boss).Any(e => e.Position.AlmostEqual(new(0,-125),1)))
                Arena.Bounds = new ArenaBoundsSquare(new(0,-125), 20);
            if (Enemies(OID.Boss).Any(e => e.Position.AlmostEqual(new(0,144.5f),1)))
                Arena.Bounds = new ArenaBoundsCircle(new(0,144.5f), 30);
        }
    }
}