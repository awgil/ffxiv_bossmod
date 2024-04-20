namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class LeftArmMetalCutterAOE(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { FirstAOEs, SecondAOEs, Done }

    public State CurState { get; private set; }
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(40, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeftArmMetalCutterAOE1)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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
                        aoe.Activation = WorldState.FutureTime(5.1f);
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

class LeftArmMetalCutterKnockback(BossModule module, AID aid, float distance) : Components.Knockback(module, ActionID.MakeSpell(aid))
{
    private readonly float _distance = distance;
    private Source? _instance;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_instance);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftArmMetalCutter or AID.ArmUnit)
            _instance = new(caster.Position, _distance, spell.NPCFinishAt.AddSeconds(0.6f));
    }
}
class LeftArmMetalCutterKnockbackShort(BossModule module) : LeftArmMetalCutterKnockback(module, AID.LeftArmMetalCutterKnockbackShort, 5);
class LeftArmMetalCutterKnockbackLong(BossModule module) : LeftArmMetalCutterKnockback(module, AID.LeftArmMetalCutterKnockbackLong, 15);
