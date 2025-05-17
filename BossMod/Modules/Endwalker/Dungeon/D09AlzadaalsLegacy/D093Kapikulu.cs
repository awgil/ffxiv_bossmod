namespace BossMod.Endwalker.Dungeon.D09AlzadaalsLegacy.D093Kapikulu;

public enum OID : uint
{
    Boss = 0x36C1, // R=5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BastingBlade = 28520, // Boss->self, 5.5s cast, range 60 width 15 rect
    BillowingBolts = 28528, // Boss->self, 5.0s cast, range 80 circle, raidwide
    CrewelSlice = 28530, // Boss->player, 5.0s cast, single-target, tankbuster
    MagnitudeOpusVisual = 28526, // Boss->self, 4.0s cast, single-target
    MagnitudeOpus = 28527, // Helper->players, 5.0s cast, range 6 circle, stack
    ManaExplosion = 28523, // Helper->self, 3.0s cast, range 15 circle
    PowerSerge = 28522, // Boss->self, 6.0s cast, single-target
    SpinOut = 28515, // Boss->self, 3.0s cast, single-target
    SpinPull = 28516, // Helper->player, 4.5s cast, single-target, pull 100, between centers
    ChangeModelState = 28517, // Boss->self, no cast, single-target
    RemoveFetters = 28518, // Helper->self, no cast, range 10 circle
    Traps = 28519, // Helper->self, 6.0s cast, range 6 width 6 rect
    BorderChange = 28529, // Helper->self, 5.0s cast, range 5 width 40 rect
    Teleport = 28514, // Boss->location, no cast, single-target
    WildWeave = 28521, // Boss->self, 4.0s cast, single-target
    RotaryGaleVisual = 28524, // Boss->self, 4.0+1,0s cast, single-target
    RotaryGale = 28525 // Helper->players, 5.0s cast, range 5 circle, spread
}

public enum TetherID : uint
{
    SpinTether = 177, // player->Boss
    ManaExplosion = 188 // Boss->Helper
}

class BillowingBoltsArenaChange(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsRect smallerBounds = new(15, 20);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BillowingBolts && Arena.Bounds != smallerBounds)
            Arena.Bounds = smallerBounds;
    }
}

class ManaExplosion(BossModule module) : Components.GenericAOEs(module)
{
    private enum Pattern { None, Pattern1, Pattern2 }
    private Pattern currentPattern;
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15);
    private static readonly WPos[] aoePositionsSet1 = [new(119, -68), new(101, -86), new(101, -50)]; // yellow P2, green P1
    private static readonly WPos[] aoePositionsSet2 = [new(119, -50), new(101, -68), new(119, -86)]; // yellow P1, green P2
    private Actor? _target;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ManaExplosion)
        {
            _target = WorldState.Actors.Find(tether.Target)!;
            _activation = WorldState.FutureTime(11.5f); // some variation here, have seen upto almost 12.3s
        }
    }

    public override void Update()
    {
        if (_target != default) // Helper can teleport after tether started, this fixes the rare problem
        {
            if (_target.Position == new WPos(110, -45.5f)) // green cloth tethered
                foreach (var c in currentPattern == Pattern.Pattern1 ? aoePositionsSet1 : aoePositionsSet2)
                    _aoes.Add(new(circle, c, default, _activation));
            else if (_target.Position == new WPos(110, -90.5f)) // yellow cloth tethered
                foreach (var c in currentPattern == Pattern.Pattern1 ? aoePositionsSet2 : aoePositionsSet1)
                    _aoes.Add(new(circle, c, default, _activation));
            if (_aoes.Count > 0)
                _target = default;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x0D) // 0x0D, 0x02, 0x0C, 0x04, 0x0E, 0x03 activate at the same time
                currentPattern = Pattern.Pattern1;
            if (index == 0x09) // 0x09, 0x06, 0x0B, 0x05, 0x0A, 0x07 activate at the same time
                currentPattern = Pattern.Pattern2;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ManaExplosion)
        {
            currentPattern = Pattern.None;
            _aoes.Clear();
        }
    }
}

class BastingBlade(BossModule module) : Components.StandardAOEs(module, AID.BastingBlade, new AOEShapeRect(60, 7.5f));

class SpikeTraps(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(6, 3);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Traps)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x01 && state is 0x00400004 or 0x00800004 or 0x00080004)
            _aoes.Clear();
    }
}

class BorderChange(BossModule module) : Components.StandardAOEs(module, AID.BorderChange, new AOEShapeRect(5, 20));
class MagnitudeOpus(BossModule module) : Components.StackWithCastTargets(module, AID.MagnitudeOpus, 6, 4, 4);
class RotaryGale(BossModule module) : Components.SpreadFromCastTargets(module, AID.RotaryGale, 5);
class CrewelSlice(BossModule module) : Components.SingleTargetDelayableCast(module, AID.CrewelSlice);
class BillowingBolts(BossModule module) : Components.RaidwideCast(module, AID.BillowingBolts);

class D093KapikuluStates : StateMachineBuilder
{
    public D093KapikuluStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BillowingBoltsArenaChange>()
            .ActivateOnEnter<ManaExplosion>()
            .ActivateOnEnter<BastingBlade>()
            .ActivateOnEnter<SpikeTraps>()
            .ActivateOnEnter<BorderChange>()
            .ActivateOnEnter<MagnitudeOpus>()
            .ActivateOnEnter<CrewelSlice>()
            .ActivateOnEnter<RotaryGale>()
            .ActivateOnEnter<BillowingBolts>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11238)]
public class D093Kapikulu(WorldState ws, Actor primary) : BossModule(ws, primary, new(110, -68), new ArenaBoundsRect(19.5f, 24.5f));
