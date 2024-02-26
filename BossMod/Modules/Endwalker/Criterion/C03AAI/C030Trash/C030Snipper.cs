using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1
{
    class Water : Components.StackWithCastTargets
    {
        public Water(AID aid) : base(ActionID.MakeSpell(aid), 8, 4) { }
    }
    class NWater : Water { public NWater() : base(AID.NWater) { } }
    class SWater : Water { public SWater() : base(AID.SWater) { } }

    class BubbleShowerCrabDribble : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shape1 = new(9, 45.Degrees());
        private static AOEShapeCone _shape2 = new(6, 60.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NBubbleShower or AID.SBubbleShower)
            {
                _aoes.Clear();
                _aoes.Add(new(_shape1, caster.Position, spell.Rotation, spell.NPCFinishAt));
                _aoes.Add(new(_shape2, caster.Position, spell.Rotation + 180.Degrees(), spell.NPCFinishAt.AddSeconds(3.6f)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NBubbleShower or AID.SBubbleShower or AID.NCrabDribble or AID.SCrabDribble && _aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    class C030SnipperStates : StateMachineBuilder
    {
        private bool _savage;

        public C030SnipperStates(BossModule module, bool savage) : base(module)
        {
            _savage = savage;
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<NWater>(!_savage)
                .ActivateOnEnter<SWater>(_savage)
                .ActivateOnEnter<BubbleShowerCrabDribble>()
                .ActivateOnEnter<NTailScrew>(!_savage) // note: first mob is often pulled together with second one
                .ActivateOnEnter<STailScrew>(_savage)
                .ActivateOnEnter<Twister>();
        }

        private void SinglePhase(uint id)
        {
            Water(id, 7.7f);
            BubbleShowerCrabDribble(id + 0x10000, 2.1f);
            Water(id + 0x20000, 11.3f);
            BubbleShowerCrabDribble(id + 0x30000, 2.1f);
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void Water(uint id, float delay)
        {
            Cast(id, _savage ? AID.SWater : AID.NWater, delay, 5, "Stack");
        }

        private void BubbleShowerCrabDribble(uint id, float delay)
        {
            Cast(id, _savage ? AID.SBubbleShower : AID.NBubbleShower, delay, 5, "Cleave front");
            Cast(id + 0x10, _savage ? AID.SCrabDribble : AID.NCrabDribble, 2.1f, 1.5f, "Cleave back");
        }
    }
    class C030NSnipperStates : C030SnipperStates { public C030NSnipperStates(BossModule module) : base(module, false) { } }
    class C030SSnipperStates : C030SnipperStates { public C030SSnipperStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NSnipper, CFCID = 979, NameID = 12537)]
    public class C030NSnipper : C030Trash1
    {
        public C030NSnipper(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.NCrab), ArenaColor.Enemy);
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SSnipper, CFCID = 980, NameID = 12537)]
    public class C030SSnipper : C030Trash1
    {
        public C030SSnipper(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.SCrab), ArenaColor.Enemy);
        }
    }
}
