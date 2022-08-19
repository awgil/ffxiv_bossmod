using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator
{
    public enum OID : uint
    {
        Boss = 0x6DC, // x1
        SunJuror = 0x6E1, // spawn during fight
        MythrilVerge = 0x6DD, // spawn during fight
        Platform1 = 0x1E870F, // x1, EventObj type
        Platform2 = 0x1E8710, // x1, EventObj type
        Platform3 = 0x1E8711, // x1, EventObj type
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/SunJuror->player, no cast
        Darkness = 928, // Boss/SunJuror->self, 2.5s cast, range 7.5 120-degree cone aoe
        Paralyze = 308, // Boss->player, 4.0s cast, single-target
        CreepingDarkness = 927, // Boss->self, 2.5s cast, raidwide
        VergeLine = 929, // MythrilVerge->self, 4.0s cast, range 60 width 4 rect aoe
        VergePulse = 930, // MythrilVerge->self, 10.0s cast, range 60 ???
    };

    class Darkness : Components.SelfTargetedAOEs
    {
        public Darkness() : base(ActionID.MakeSpell(AID.Darkness), new AOEShapeCone(7.5f, 60.Degrees()), true) { }
    }

    class VergeLine : Components.SelfTargetedAOEs
    {
        public VergeLine() : base(ActionID.MakeSpell(AID.VergeLine), new AOEShapeRect(60, 2), true) { }
    }

    class D083AdjudicatorStates : StateMachineBuilder
    {
        public D083AdjudicatorStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Darkness>()
                .ActivateOnEnter<VergeLine>();
        }
    }

    public class D083Adjudicator : BossModule
    {
        private List<Actor> _juror;
        private List<Actor> _verge;
        private List<Actor> _platform1;
        private List<Actor> _platform2;
        private List<Actor> _platform3;

        public D083Adjudicator(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(238, 0), 20))
        {
            _juror = Enemies(OID.SunJuror);
            _verge = Enemies(OID.MythrilVerge);
            _platform1 = Enemies(OID.Platform1);
            _platform2 = Enemies(OID.Platform2);
            _platform3 = Enemies(OID.Platform3);
        }

        public override bool FillTargets(BossTargets targets, int pcSlot)
        {
            if (!targets.AddIfValid(_verge) && !targets.AddIfValid(JurorsNearPlatforms()))
                targets.AddIfValid(PrimaryActor);
            return true;
        }

        private IEnumerable<Actor> JurorsNearPlatforms()
        {
            var platforms = _platform1.Concat(_platform2).Concat(_platform3);
            return _juror.Where(j => platforms.InRadius(j.Position, 1).Any());
        }
    }
}
