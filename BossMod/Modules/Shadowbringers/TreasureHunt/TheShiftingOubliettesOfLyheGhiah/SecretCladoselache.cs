namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretCladoselache;

public enum OID : uint
{
    Boss = 0x3027, //R=2.47
    BossAdd = 0x3028, //R=3.0 
    BossHelper = 0x233C,
    BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
};

public enum AID : uint
{
    AutoAttack = 870, // BossAdd->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    PelagicCleaver = 21705, // Boss->self, 3,5s cast, range 40 60-degree cone
    TidalGuillotine = 21704, // Boss->self, 4,0s cast, range 13 circle
    ProtolithicPuncture = 21703, // Boss->player, 4,0s cast, single-target
    PelagicCleaverRotationStart = 21706, // Boss->self, 5,0s cast, range 40 60-degree cone
    PelagicCleaverDuringRotation = 21707, // Boss->self, no cast, range 40 60-degree cone
    BiteAndRun = 21709, // BossAdd->player, 5,0s cast, width 5 rect charge
    AquaticLance = 21708, // Boss->player, 5,0s cast, range 8 circle

    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
    Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
    Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
};

public enum IconID : uint
{
    spreadmarker = 135, // player
    RotateCCW = 168, // Boss
    RotateCW = 167, // Boss
};

class PelagicCleaverRotation : Components.GenericRotatingAOE
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -60.Degrees(),
            IconID.RotateCCW => 60.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(module, actor);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PelagicCleaverRotationStart)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
        if (_rotation != default)
            InitIfReady(module, caster);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.PelagicCleaverRotationStart or AID.PelagicCleaverDuringRotation)
            AdvanceSequence(0, module.WorldState.CurrentTime);
    }

    private void InitIfReady(BossModule module, Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 2.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class PelagicCleaver : Components.SelfTargetedAOEs
{
    public PelagicCleaver() : base(ActionID.MakeSpell(AID.PelagicCleaver), new AOEShapeCone(40, 30.Degrees())) { }
}

class TidalGuillotine : Components.SelfTargetedAOEs
{
    public TidalGuillotine() : base(ActionID.MakeSpell(AID.TidalGuillotine), new AOEShapeCircle(13)) { }
}

class ProtolithicPuncture : Components.SingleTargetCast
{
    public ProtolithicPuncture() : base(ActionID.MakeSpell(AID.ProtolithicPuncture)) { }
}

class BiteAndRun : Components.BaitAwayChargeCast
{
    public BiteAndRun() : base(ActionID.MakeSpell(AID.BiteAndRun), 2.5f) { }
}

class AquaticLance : Components.SpreadFromCastTargets
{
    public AquaticLance() : base(ActionID.MakeSpell(AID.AquaticLance), 8) { }
}

class Spin : Components.SelfTargetedAOEs
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
}

class Mash : Components.SelfTargetedAOEs
{
    public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
}

class Scoop : Components.SelfTargetedAOEs
{
    public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
}

class CladoselacheStates : StateMachineBuilder
{
    public CladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<PelagicCleaverRotation>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<BiteAndRun>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9778)]
public class Cladoselache : BossModule
{
    public Cladoselache(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_TheKeeperOfTheKeys => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
