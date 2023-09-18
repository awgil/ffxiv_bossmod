namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash2
{
    class Issen : Components.SingleTargetCast
    {
        public Issen(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NIssen : Issen { public NIssen() : base(AID.NIssen) { } }
    class SIssen : Issen { public SIssen() : base(AID.SIssen) { } }

    class Huton : Components.SingleTargetCast
    {
        public Huton(AID aid) : base(ActionID.MakeSpell(aid), "Cast speed buff") { }
    }
    class NHuton : Huton { public NHuton() : base(AID.NHuton) { } }
    class SHuton : Huton { public SHuton() : base(AID.SHuton) { } }

    class JujiShuriken : Components.SelfTargetedAOEs
    {
        public JujiShuriken(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(40, 1.5f)) { }
    }
    class NJujiShuriken : JujiShuriken { public NJujiShuriken() : base(AID.NJujiShuriken) { } }
    class SJujiShuriken : JujiShuriken { public SJujiShuriken() : base(AID.SJujiShuriken) { } }

    class JujiShurikenFast : Components.SelfTargetedAOEs
    {
        public JujiShurikenFast(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(40, 1.5f)) { }
    }
    class NJujiShurikenFast : JujiShurikenFast { public NJujiShurikenFast() : base(AID.NJujiShurikenFast) { } }
    class SJujiShurikenFast : JujiShurikenFast { public SJujiShurikenFast() : base(AID.SJujiShurikenFast) { } }

    class C020OnmitsugashiraStates : StateMachineBuilder
    {
        private bool _savage;

        public C020OnmitsugashiraStates(BossModule module, bool savage) : base(module)
        {
            _savage = savage;
            TrivialPhase()
                .ActivateOnEnter<NIssen>(!savage)
                .ActivateOnEnter<NHuton>(!savage)
                .ActivateOnEnter<NJujiShuriken>(!savage)
                .ActivateOnEnter<NJujiShurikenFast>(!savage)
                .ActivateOnEnter<SIssen>(savage)
                .ActivateOnEnter<SHuton>(savage)
                .ActivateOnEnter<SJujiShuriken>(savage)
                .ActivateOnEnter<SJujiShurikenFast>(savage)
                // for yamabiko
                .ActivateOnEnter<NMountainBreeze>(!savage)
                .ActivateOnEnter<SMountainBreeze>(savage);
        }
    }
    class C020NOnmitsugashiraStates : C020OnmitsugashiraStates { public C020NOnmitsugashiraStates(BossModule module) : base(module, false) { } }
    class C020SOnmitsugashiraStates : C020OnmitsugashiraStates { public C020SOnmitsugashiraStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NOnmitsugashira)]
    public class C020NOnmitsugashira : C020Trash2 { public C020NOnmitsugashira(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SOnmitsugashira)]
    public class C020SOnmitsugashira : C020Trash2 { public C020SOnmitsugashira(WorldState ws, Actor primary) : base(ws, primary) { } }
}
