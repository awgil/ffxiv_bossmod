using System.Linq;

namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    class MythrilGreataxe : Components.SelfTargetedAOEs
    {
        public MythrilGreataxe() : base(ActionID.MakeSpell(AID.MythrilGreataxe), new AOEShapeCone(71, 30.Degrees())) { }
    }

    class Hydroptosis : Components.SpreadFromCastTargets
    {
        public Hydroptosis() : base(ActionID.MakeSpell(AID.HydroptosisAOE), 6) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.Althyk)]
    public class A22AlthykNymeia : BossModule
    {
        private Actor? _nymeia;

        public Actor? Althyk() => PrimaryActor;
        public Actor? Nymeia() => _nymeia;

        public A22AlthykNymeia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(50, -750), 25)) { }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _nymeia ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Nymeia).FirstOrDefault() : null;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(_nymeia, ArenaColor.Enemy);
        }
    }
}
