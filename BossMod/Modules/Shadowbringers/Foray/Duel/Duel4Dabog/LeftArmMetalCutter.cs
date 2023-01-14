using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog
{
    class LeftArmMetalCutterAOE : Components.GenericAOEs
    {
        public enum State { FirstAOEs, SecondAOEs, Done }

        public State CurState { get; private set; }
        private List<AOEInstance> _aoes = new();
        private static AOEShapeCone _shape = new(40, 45.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.LeftArmMetalCutterAOE1)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.FinishAt));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LeftArmMetalCutterAOE1:
                    if (CurState == State.FirstAOEs)
                    {
                        for (int i = 0; i < _aoes.Count; i++)
                        {
                            var aoe = _aoes[i];
                            aoe.Rotation += 180.Degrees();
                            aoe.Activation = module.WorldState.CurrentTime.AddSeconds(5.1f);
                            _aoes[i] = aoe;
                        }
                        CurState = State.SecondAOEs;
                    }
                    break;
                case AID.LeftArmMetalCutterAOE2:
                    CurState = State.Done;
                    _aoes.Clear();
                    break;
            }
        }
    }

    class LeftArmMetalCutterKnockback : Components.Knockback
    {
        private float _distance;
        private Source? _instance;

        public LeftArmMetalCutterKnockback(AID aid, float distance) : base(ActionID.MakeSpell(aid))
        {
            _distance = distance;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_instance != null)
                yield return _instance.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LeftArmMetalCutter or AID.ArmUnit)
                _instance = new(caster.Position, _distance, spell.FinishAt.AddSeconds(0.6f));
        }
    }

    class LeftArmMetalCutterKnockbackShort : LeftArmMetalCutterKnockback
    {
        public LeftArmMetalCutterKnockbackShort() : base(AID.LeftArmMetalCutterKnockbackShort, 5) { }
    }

    class LeftArmMetalCutterKnockbackLong : LeftArmMetalCutterKnockback
    {
        public LeftArmMetalCutterKnockbackLong() : base(AID.LeftArmMetalCutterKnockbackLong, 15) { }
    }
}
