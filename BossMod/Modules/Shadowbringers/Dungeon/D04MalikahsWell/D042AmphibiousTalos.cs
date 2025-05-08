namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D042AmphibiousTalos;

public enum OID : uint
{
    Boss = 0x267A, // R=3.15
    Geyser = 0x1EAAC9,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Efface = 15595, // Boss->player, 4.5s cast, single-target
    Wellbore = 15597, // Boss->self, 7.0s cast, range 15 circle
    GeyserEruption = 15598, // Helper->self, 3.5s cast, range 8 circle
    HighPressure = 15596, // Boss->self, 4.0s cast, range 40 circle, knockback 20, away from source
    SwiftSpillFirst = 15599, // Boss->self, 7.0s cast, range 50 60-degree cone
    SwiftSpillRest = 15600 // Boss->self, no cast, range 50 60-degree cone
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    RotateCCW = 157, // Boss
    RotateCW = 156 // Boss
}

class SwiftSpillRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly Angle a60 = 60.Degrees();
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(50, 30.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -a60,
            IconID.RotateCCW => a60,
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SwiftSpillFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
        if (_rotation != default)
            InitIfReady(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.SwiftSpillFirst or AID.SwiftSpillRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 1.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class Efface(BossModule module) : Components.SingleTargetCast(module, AID.Efface);
class HighPressureRaidwide(BossModule module) : Components.RaidwideCast(module, AID.HighPressure);
class HighPressureKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.HighPressure, 20, stopAtWall: true);
class GeyserEruption(BossModule module) : Components.StandardAOEs(module, AID.GeyserEruption, new AOEShapeCircle(8));
class Geysers(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.Geyser).Where(v => v.EventState != 7));
class Wellbore(BossModule module) : Components.StandardAOEs(module, AID.Wellbore, new AOEShapeCircle(15));

class D042AmphibiousTalosStates : StateMachineBuilder
{
    public D042AmphibiousTalosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwiftSpillRotation>()
            .ActivateOnEnter<Efface>()
            .ActivateOnEnter<HighPressureKnockback>()
            .ActivateOnEnter<HighPressureRaidwide>()
            .ActivateOnEnter<GeyserEruption>()
            .ActivateOnEnter<Geysers>()
            .ActivateOnEnter<Wellbore>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8250)]
public class D042AmphibiousTalos(WorldState ws, Actor primary) : BossModule(ws, primary, new(208, 275), new ArenaBoundsCircle(19.55f));
