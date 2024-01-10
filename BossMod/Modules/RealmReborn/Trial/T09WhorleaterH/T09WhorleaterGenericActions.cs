using BossMod.Components;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH;
 
     class GrandFall : LocationTargetedAOEs
    {
        public GrandFall() : base(ActionID.MakeSpell(AID.GrandFall), 8) { }
    }

    class Hydroshot : PersistentVoidzoneAtCastTarget
    {
        public Hydroshot() : base(5, ActionID.MakeSpell(AID.Hydroshot), m => m.Enemies(OID.HydroshotZone), 0) { }
    }
    class Dreadstorm : PersistentVoidzoneAtCastTarget
    {
        public Dreadstorm() : base(5, ActionID.MakeSpell(AID.Dreadstorm), m => m.Enemies(OID.DreadstormZone), 0) { }
    }
    class T09WhorleaterHStates : StateMachineBuilder
    {
        public T09WhorleaterHStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<GrandFall>()
                .ActivateOnEnter<Hydroshot>()
                .ActivateOnEnter<Dreadstorm>()
                .ActivateOnEnter<BodySlamKB>()
                .ActivateOnEnter<BodySlamAOE>()
                .ActivateOnEnter<SpinningDive>()
                .ActivateOnEnter<SpinningDiveKB>()
                .ActivateOnEnter<Hints>();
        }
    }