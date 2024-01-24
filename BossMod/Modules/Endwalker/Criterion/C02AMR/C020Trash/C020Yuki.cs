namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1
{
    class RightSwipe : Components.SelfTargetedAOEs
    {
        public RightSwipe(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class NRightSwipe : RightSwipe { public NRightSwipe() : base(AID.NRightSwipe) { } }
    class SRightSwipe : RightSwipe { public SRightSwipe() : base(AID.SRightSwipe) { } }

    class LeftSwipe : Components.SelfTargetedAOEs
    {
        public LeftSwipe(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class NLeftSwipe : LeftSwipe { public NLeftSwipe() : base(AID.NLeftSwipe) { } }
    class SLeftSwipe : LeftSwipe { public SLeftSwipe() : base(AID.SLeftSwipe) { } }

    class C020YukiStates : StateMachineBuilder
    {
        public C020YukiStates(BossModule module, bool savage) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NRightSwipe>(!savage)
                .ActivateOnEnter<NLeftSwipe>(!savage)
                .ActivateOnEnter<SRightSwipe>(savage)
                .ActivateOnEnter<SLeftSwipe>(savage);
        }
    }
    class C020NYukiStates : C020YukiStates { public C020NYukiStates(BossModule module) : base(module, false) { } }
    class C020SYukiStates : C020YukiStates { public C020SYukiStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NYuki, CFCID = 946, NameID = 12425)]
    public class C020NYuki : C020Trash1 { public C020NYuki(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SYuki, CFCID = 947, NameID = 12425)]
    public class C020SYuki : C020Trash1 { public C020SYuki(WorldState ws, Actor primary) : base(ws, primary) { } }
}
