namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D011PrimePunutiy;

public enum OID : uint
{
    Boss = 0x4190, // R7.990, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), Helper type
    ProdigiousPunutiy = 0x4191, // R4.230, x0 (spawn during fight)
    Punutiy = 0x4192, // R2.820, x0 (spawn during fight)
    PetitPunutiy = 0x4193, // R2.115, x0 (spawn during fight)
    IhuykatumuFlytrap = 0x4194, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Punutiy/ProdigiousPunutiy/PetitPunutiy->player, no cast, single-target
    PunutiyPress = 36492, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Hydrowave = 36493, // Boss->self, 4.0s cast, range 60 30-degree cone
    Resurface = 36494, // Boss->self, 5.0s cast, range 100 60-degree cone
    ResurfacePersistent = 36495, // Boss->self, 7.0s cast, single-target, visual (constant inhale casts while being cast)
    ResurfaceInhale = 36496, // Helper->self, no cast, range 100 60-degree cone
    Bury1 = 36497, // Helper->self, 4.0s cast, range 12 circle
    Bury2 = 36498, // Helper->self, 4.0s cast, range 8 circle
    Bury3 = 36499, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury4 = 36500, // Helper->self, 4.0s cast, range 35 width 10 rect
    Bury5 = 36501, // Helper->self, 4.0s cast, range 4 circle
    Bury6 = 36502, // Helper->self, 4.0s cast, range 6 circle
    Bury7 = 36503, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury8 = 36504, // Helper->self, 4.0s cast, range 35 width 10 rect
    Decay = 36505, // IhuykatumuFlytrap->self, 7.0s cast, range 6-40 donut

    SongOfThePunutiy = 36506, // Boss->self, 5.0s cast, single-target, visual (adds)
    PunutiyFlopLarge = 36508, // ProdigiousPunutiy->player, 8.0s cast, range 14 circle
    PunutiyHydrowave = 36509, // Punutiy->self, 8.0s cast, single-target, visual (baited cone)
    PunutiyHydrowaveTargetSelect = 36510, // Helper->player, no cast, single-target, visual (target select for baited cone)
    PunutiyHydrowaveVisual = 36511, // Punutiy->self, no cast, single-target, visual (baited cone hit)
    PunutiyHydrowaveAOE = 36512, // Helper->self, no cast, range 60 60-degree cone
    PunutiyFlopSmall = 36513, // PetitPunutiy->player, 8.0s cast, range 6 circle
    ShoreShaker = 36514, // Boss->self, 4.0+1.0s cast, single-target
    ShoreShakerAOE1 = 36515, // Helper->self, 5.0s cast, range 10 circle
    ShoreShakerAOE2 = 36516, // Helper->self, 7.0s cast, range 10-20 donut
    ShoreShakerAOE3 = 36517, // Helper->self, 9.0s cast, range 20-30 donut
}

public enum IconID : uint
{
    PunutiyFlopLarge = 505, // player
    PunutiyFlopSmall = 196, // player
}

public enum TetherID : uint
{
    SongOfThePunutiy = 17, // ProdigiousPunutiy/Punutiy/PetitPunutiy->player
}

class PunutiyFlop(BossModule module) : Components.RaidwideCast(module, AID.PunutiyPress);
class Hydrowave(BossModule module) : Components.StandardAOEs(module, AID.Hydrowave, new AOEShapeCone(60, 15.Degrees()));

class Resurface(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(100, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Resurface)
        {
            // the boss typically stays slightly in front of the border (15.40 rather than 15.00), and all cones other than first one originate from border, making them slightly bigger
            var origin = Module.Center - 20 * spell.Rotation.ToDirection();
            _aoe = new(_shape, origin, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ResurfacePersistent)
            _aoe = null;
    }
}

class Bury(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, AOEInstance AOE)> _activeAOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activeAOEs.Select(x => x.AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? toAdd = (AID)spell.Action.ID switch
        {
            AID.Bury1 => new AOEShapeCircle(12),
            AID.Bury2 => new AOEShapeCircle(8),
            AID.Bury3 or AID.Bury7 => new AOEShapeRect(25, 3),
            AID.Bury4 or AID.Bury8 => new AOEShapeRect(35, 5),
            AID.Bury5 => new AOEShapeCircle(4),
            AID.Bury6 => new AOEShapeCircle(6),
            _ => null
        };
        if (toAdd != null)
            _activeAOEs.Add((caster, new AOEInstance(toAdd, caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        _activeAOEs.RemoveAll(x => x.Caster == caster);
    }
}

class Decay(BossModule module) : Components.StandardAOEs(module, AID.Decay, new AOEShapeDonut(5, 40))
{
    private readonly IReadOnlyList<Actor> _flytrap = module.Enemies(OID.IhuykatumuFlytrap);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_flytrap.Where(x => !x.IsDead), ArenaColor.Object, true);
    }
}

class PunitiyFlopLarge(BossModule module) : Components.BaitAwayCast(module, AID.PunutiyFlopLarge, new AOEShapeCircle(14), true);
class PunitiyFlopSmall(BossModule module) : Components.BaitAwayCast(module, AID.PunutiyFlopSmall, new AOEShapeCircle(6), true);
class PunitiyHydrowave(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _shape = new(60, 30.Degrees());

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PunutiyHydrowaveTargetSelect:
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target != null)
                    CurrentBaits.Add(new(caster, target, _shape, WorldState.FutureTime(8.6f)));
                break;
            case AID.PunutiyHydrowaveAOE:
                CurrentBaits.Clear();
                break;
        }
    }
}

class PunitiyAdds(BossModule module) : Components.AddsMulti(module, [OID.ProdigiousPunutiy, OID.Punutiy, OID.PetitPunutiy]);

class ShoreShaker(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShoreShakerAOE1)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.ShoreShakerAOE1 => 0,
            AID.ShoreShakerAOE2 => 1,
            AID.ShoreShakerAOE3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}

class D011PrimePunutiyStates : StateMachineBuilder
{
    public D011PrimePunutiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PunutiyFlop>()
            .ActivateOnEnter<Hydrowave>()
            .ActivateOnEnter<Resurface>()
            .ActivateOnEnter<Bury>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<PunitiyFlopLarge>()
            .ActivateOnEnter<PunitiyFlopSmall>()
            .ActivateOnEnter<PunitiyHydrowave>()
            .ActivateOnEnter<PunitiyAdds>()
            .ActivateOnEnter<ShoreShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12723)]
public class D011PrimePunutiy(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, -95), new ArenaBoundsSquare(19.5f));
