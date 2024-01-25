﻿namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash2
{
    class GravityForce : Components.StackWithCastTargets
    {
        public GravityForce(AID aid) : base(ActionID.MakeSpell(aid), 6, 4) { }
    }
    class NGravityForce : GravityForce { public NGravityForce() : base(AID.NGravityForce) { } }
    class SGravityForce : GravityForce { public SGravityForce() : base(AID.SGravityForce) { } }

    class IsleDrop : Components.LocationTargetedAOEs
    {
        public IsleDrop(AID aid) : base(ActionID.MakeSpell(aid), 6) { }
    }
    class NIsleDrop : IsleDrop { public NIsleDrop() : base(AID.NIsleDrop) { } }
    class SIsleDrop : IsleDrop { public SIsleDrop() : base(AID.SIsleDrop) { } }

    class C030IslekeeperStates : StateMachineBuilder
    {
        private bool _savage;

        public C030IslekeeperStates(BossModule module, bool savage) : base(module)
        {
            _savage = savage;
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            AncientQuaga(id, 11.9f);
            GravityForce(id + 0x10000, 6.3f);
            IsleDrop(id + 0x20000, 2.1f);
            AncientQuaga(id + 0x30000, 8.5f);
            Cast(id + 0x40000, _savage ? AID.SAncientQuagaEnrage : AID.NAncientQuagaEnrage, 4.1f, 10, "Enrage");
        }

        private void AncientQuaga(uint id, float delay)
        {
            Cast(id, _savage ? AID.SAncientQuaga : AID.NAncientQuaga, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void GravityForce(uint id, float delay)
        {
            Cast(id, _savage ? AID.SGravityForce : AID.NGravityForce, delay, 5, "Stack")
                .ActivateOnEnter<NGravityForce>(!_savage)
                .ActivateOnEnter<SGravityForce>(_savage)
                .DeactivateOnExit<GravityForce>();
        }

        private void IsleDrop(uint id, float delay)
        {
            Cast(id, _savage ? AID.SIsleDrop : AID.NIsleDrop, delay, 5, "Puddle")
                .ActivateOnEnter<NIsleDrop>(!_savage)
                .ActivateOnEnter<SIsleDrop>(_savage)
                .DeactivateOnExit<IsleDrop>();
        }
    }
    class C030NIslekeeperStates : C030IslekeeperStates { public C030NIslekeeperStates(BossModule module) : base(module, false) { } }
    class C030SIslekeeperStates : C030IslekeeperStates { public C030SIslekeeperStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NIslekeeper, CFCID = 979, NameID = 12561)]
    public class C030NIslekeeper : C030Trash2
    {
        public C030NIslekeeper(WorldState ws, Actor primary) : base(ws, primary) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SIslekeeper, CFCID = 980, NameID = 12561)]
    public class C030SIslekeeper : C030Trash2
    {
        public C030SIslekeeper(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
