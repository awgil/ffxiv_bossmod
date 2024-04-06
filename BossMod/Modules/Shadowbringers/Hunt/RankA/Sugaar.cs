namespace BossMod.Shadowbringers.Hunt.RankA.Sugaar;

public enum OID : uint
{
    Boss = 0x2875, // R5.500, x1
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle

    NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
    NumbingNoiseRotating = 18098, // during rotation, 0.3s cast time, 13 range, 120 degree cone
    NumbingNoiseAttract = 18100, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of NumbingNoiseRotating

    TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
    TailSnapRotating = 18099, // Boss->self (behind), 0.3s cast, range 18, 120 degree cone
    TailSnapAttract = 18101, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of TailSnapRotating
}

class BodySlam : Components.SelfTargetedAOEs
{
    public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(11)) { }
}

class NumbingNoise : Components.SelfTargetedAOEs
{
    public NumbingNoise() : base(ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13, 60.Degrees())) { }
}

class TailSnap : Components.SelfTargetedAOEs
{
    public TailSnap() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18, 60.Degrees())) { }
}

class NumbingNoiseTailSnapRotating : Components.GenericRotatingAOE
{
    private static readonly AOEShapeCone _shapeNumbingNoise = new(13, 60.Degrees());
    private static readonly AOEShapeCone _shapeTailSnap = new(18, 60.Degrees());

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NumbingNoiseAttract: // NN always seems to go CCW
                Sequences.Add(new(_shapeNumbingNoise, module.PrimaryActor.Position, spell.Rotation, 120.Degrees(), spell.NPCFinishAt.AddSeconds(1.1f), 2.7f, 3));
                break;
            case AID.TailSnapAttract: // TS always seems to go CW
                Sequences.Add(new(_shapeTailSnap, module.PrimaryActor.Position, spell.Rotation + 180.Degrees(), -120.Degrees(), spell.NPCFinishAt.AddSeconds(1.1f), 2.7f, 3));
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.NumbingNoiseRotating or AID.TailSnapRotating)
            AdvanceSequence(0, module.WorldState.CurrentTime);
    }
}

class NumbingNoiseTailSnapAttract : Components.Knockback
{
    private NumbingNoiseTailSnapRotating? _rotating;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(30);

    public override void Init(BossModule module) => _rotating = module.FindComponent<NumbingNoiseTailSnapRotating>();

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(module.PrimaryActor.Position, 25, _activation, _shape, default, Kind.TowardsOrigin, module.PrimaryActor.HitboxRadius + actor.HitboxRadius);
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => _rotating?.ActiveAOEs(module, slot, actor).Any(aoe => aoe.Check(pos)) ?? false;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.NumbingNoiseAttract or AID.TailSnapAttract)
            _activation = spell.NPCFinishAt;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.NumbingNoiseAttract or AID.TailSnapAttract)
            _activation = default;
    }
}

class SugaarStates : StateMachineBuilder
{
    public SugaarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<NumbingNoise>()
            .ActivateOnEnter<TailSnap>()
            .ActivateOnEnter<NumbingNoiseTailSnapRotating>()
            .ActivateOnEnter<NumbingNoiseTailSnapAttract>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8902)]
public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
