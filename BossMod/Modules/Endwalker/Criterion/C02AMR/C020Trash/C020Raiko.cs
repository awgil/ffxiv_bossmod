namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1
{
    class BloodyCaress : Components.SelfTargetedAOEs
    {
        public BloodyCaress(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(12, 60.Degrees())) { }
    }
    class NBloodyCaress : BloodyCaress { public NBloodyCaress() : base(AID.NBloodyCaress) { } }
    class SBloodyCaress : BloodyCaress { public SBloodyCaress() : base(AID.SBloodyCaress) { } }

    class DisciplesOfLevin : Components.SelfTargetedAOEs
    {
        public DisciplesOfLevin(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(10)) { }
    }
    class NDisciplesOfLevin : DisciplesOfLevin { public NDisciplesOfLevin() : base(AID.NDisciplesOfLevin) { } }
    class SDisciplesOfLevin : DisciplesOfLevin { public SDisciplesOfLevin() : base(AID.SDisciplesOfLevin) { } }

    // TODO: better component (auto update rect length)
    class BarrelingSmash : Components.BaitAwayCast
    {
        public BarrelingSmash(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(50, 3.5f)) { } // TODO: it should be safe to stay beyond target...
    }
    class NBarrelingSmash : BarrelingSmash { public NBarrelingSmash() : base(AID.NBarrelingSmash) { } }
    class SBarrelingSmash : BarrelingSmash { public SBarrelingSmash() : base(AID.SBarrelingSmash) { } }

    class Howl : Components.RaidwideCast
    {
        public Howl(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NHowl : Howl { public NHowl() : base(AID.NHowl) { } }
    class SHowl : Howl { public SHowl() : base(AID.SHowl) { } }

    class MasterOfLevin : Components.SelfTargetedAOEs
    {
        public MasterOfLevin(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(5, 30)) { }
    }
    class NMasterOfLevin : MasterOfLevin { public NMasterOfLevin() : base(AID.NMasterOfLevin) { } }
    class SMasterOfLevin : MasterOfLevin { public SMasterOfLevin() : base(AID.SMasterOfLevin) { } }

    class C020RaikoStates : StateMachineBuilder
    {
        private bool _savage;

        public C020RaikoStates(BossModule module, bool savage) : base(module)
        {
            _savage = savage;
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<NBloodyCaress>(!savage)
                .ActivateOnEnter<NDisciplesOfLevin>(!savage)
                .ActivateOnEnter<NBarrelingSmash>(!savage)
                .ActivateOnEnter<NHowl>(!savage)
                .ActivateOnEnter<NMasterOfLevin>(!savage)
                .ActivateOnEnter<SBloodyCaress>(savage)
                .ActivateOnEnter<SDisciplesOfLevin>(savage)
                .ActivateOnEnter<SBarrelingSmash>(savage)
                .ActivateOnEnter<SHowl>(savage)
                .ActivateOnEnter<SMasterOfLevin>(savage)
                // for yuki
                .ActivateOnEnter<NRightSwipe>(!savage)
                .ActivateOnEnter<NLeftSwipe>(!savage)
                .ActivateOnEnter<SRightSwipe>(savage)
                .ActivateOnEnter<SLeftSwipe>(savage);
        }

        private void SinglePhase(uint id)
        {
            DisciplesOfLevin(id, 5.3f);
            BarrelingSmashHowl(id + 0x10000, 6.1f);
            MasterOfLevin(id + 0x20000, 7.6f);
            BarrelingSmashHowl(id + 0x30000, 6.5f);
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void DisciplesOfLevin(uint id, float delay)
        {
            Cast(id, _savage ? AID.SDisciplesOfLevin : AID.NDisciplesOfLevin, delay, 4, "Out");
        }

        private void BarrelingSmashHowl(uint id, float delay)
        {
            Cast(id, _savage ? AID.SBarrelingSmash : AID.NBarrelingSmash, delay, 4, "Charge");
            Cast(id + 0x1000, _savage ? AID.SHowl : AID.NHowl, 2.1f, 4, "Raidwide");
        }

        private void MasterOfLevin(uint id, float delay)
        {
            Cast(id, _savage ? AID.SMasterOfLevin : AID.NMasterOfLevin, delay, 4, "In");
        }
    }
    class C020NRaikoStates : C020RaikoStates { public C020NRaikoStates(BossModule module) : base(module, false) { } }
    class C020SRaikoStates : C020RaikoStates { public C020SRaikoStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NRaiko)]
    public class C020NRaiko : C020Trash1
    {
        public C020NRaiko(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.NFurutsubaki), ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.NYuki), ArenaColor.Object);
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SRaiko)]
    public class C020SRaiko : C020Trash1
    {
        public C020SRaiko(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.SFurutsubaki), ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.SYuki), ArenaColor.Object);
        }
    }
}
