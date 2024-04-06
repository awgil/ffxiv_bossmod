namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenPedantry;

public enum OID : uint
{
    Boss = 0x298A, // R=5.5
};

public enum AID : uint
{
    AutoAttack_SanctifiedScathe = 17439, // 298A->player, no cast, single-target
    LeftCheek = 17446, // 298A->self, 5,0s cast, range 60 180-degree cone
    LeftCheek2 = 17447, // 298A->self, no cast, range 60 180-degree cone
    RightCheek = 17448, // 298A->self, 5,0s cast, range 60 180-degree cone
    RightCheek2 = 17449, // 298A->self, no cast, range 60 180-degree cone
    TerrifyingGlance = 17955, // 298A->self, 3,0s cast, range 50 circle, gaze
    TheStake = 17443, // 298A->self, 4,0s cast, range 18 circle
    SecondCircle = 17441, // 298A->self, 3,0s cast, range 40 width 8 rect
    CleansingFire = 17442, // 298A->self, 4,0s cast, range 40 circle
    FeveredFlagellation = 17440, // 298A->players, 4,0s cast, range 15 90-degree cone, tankbuster
    SanctifiedShock = 17900, // 298A->player, no cast, single-target, stuns target before WitchHunt
    WitchHunt = 17444, // 298A->players, 3,0s cast, width 10 rect charge
    WitchHunt2 = 17445, // 298A->players, no cast, width 10 rect charge, targets main tank
};

class LeftRightCheek : Components.GenericAOEs
{
    private static readonly AOEShapeCone cone = new(60, 90.Degrees());
    private DateTime _activation;
    private Angle _rotation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
        {
            if (NumCasts == 0)
            {
                yield return new(cone, module.PrimaryActor.Position, _rotation, _activation, ArenaColor.Danger);
                yield return new(cone, module.PrimaryActor.Position, _rotation + 180.Degrees(), _activation.AddSeconds(3.1f), risky: false);
            }
            if (NumCasts == 1)
                yield return new(cone, module.PrimaryActor.Position, _rotation + 180.Degrees(), _activation.AddSeconds(3.1f), ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftCheek or AID.RightCheek)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftCheek or AID.RightCheek)
            ++NumCasts;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LeftCheek2 or AID.RightCheek2)
        {
            NumCasts = 0;
            _activation = default;
        }
    }
}

class TerrifyingGlance : Components.CastGaze
{
    public TerrifyingGlance() : base(ActionID.MakeSpell(AID.TerrifyingGlance)) { }
}

class TheStake : Components.SelfTargetedAOEs
{
    public TheStake() : base(ActionID.MakeSpell(AID.TheStake), new AOEShapeCircle(18)) { }
}

class SecondCircle : Components.SelfTargetedAOEs
{
    public SecondCircle() : base(ActionID.MakeSpell(AID.SecondCircle), new AOEShapeRect(40, 4)) { }
}

class CleansingFire : Components.CastGaze
{
    public CleansingFire() : base(ActionID.MakeSpell(AID.CleansingFire)) { }
}

class FeveredFlagellation : Components.BaitAwayCast
{
    public FeveredFlagellation() : base(ActionID.MakeSpell(AID.FeveredFlagellation), new AOEShapeCone(15, 45.Degrees())) { }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { }
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell) //tankbuster resolves on cast event, which can be delayed by moving out of tankbuster range
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class FeveredFlagellationHint : Components.SingleTargetCast
{
    public FeveredFlagellationHint() : base(ActionID.MakeSpell(AID.FeveredFlagellation), "Cleave tankbuster") { }
}

class WitchHunt : Components.GenericBaitAway
{
    private static readonly AOEShapeRect rect = new AOEShapeRect(0, 5);
    private bool witchHunt1done;

    public override void Update(BossModule module)
    {
        foreach (var b in CurrentBaits)
            ((AOEShapeRect)b.Shape).LengthFront = (b.Target.Position - b.Source.Position).Length();
        if (CurrentBaits.Count > 0 && witchHunt1done) //updating WitchHunt2 target incase of sudden tank swap
        {
            var Target = CurrentBaits[0];
            Target.Target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID)!;
            CurrentBaits[0] = Target;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SanctifiedShock)
            CurrentBaits.Add(new(module.PrimaryActor, module.WorldState.Actors.Find(spell.MainTargetID)!, rect));
        if ((AID)spell.Action.ID == AID.WitchHunt2)
        {
            CurrentBaits.Clear();
            witchHunt1done = false;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WitchHunt)
        {
            CurrentBaits.Clear();
            CurrentBaits.Add(new(module.PrimaryActor, module.WorldState.Actors.Find(module.PrimaryActor.TargetID)!, rect));
        }
    }
}

class ForgivenPedantryStates : StateMachineBuilder
{
    public ForgivenPedantryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftRightCheek>()
            .ActivateOnEnter<TerrifyingGlance>()
            .ActivateOnEnter<TheStake>()
            .ActivateOnEnter<SecondCircle>()
            .ActivateOnEnter<CleansingFire>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<FeveredFlagellationHint>()
            .ActivateOnEnter<WitchHunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8910)]
public class ForgivenPedantry : SimpleBossModule
{
    public ForgivenPedantry(WorldState ws, Actor primary) : base(ws, primary) { }
}
