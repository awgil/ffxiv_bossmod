

namespace BossMod.Global.Crucible.IceDragonPiece;

public enum OID : uint
{
    Boss = 0x4CC0, // R7.000, x1
    _Gen_IceSprite = 0x4CC1, // R1.200, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x16, Helper type
    WitheringEternity = 0x1E972A
}

public enum AID : uint
{
    _AutoAttack_ = 50784, // Boss->player, no cast, single-target
    _Ability_ = 48695, // Boss->location, no cast, single-target
    _Weaponskill_IcyTorment = 48698, // Boss->self, 4.7+1.8s cast, single-target
    _Weaponskill_IcyTorment1 = 48699, // Helper->self, 6.5s cast, range 100 width 15 rect
    _Weaponskill_RimeWreath = 48709, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_IcyTorment2 = 48696, // Boss->self, 5.0+1.5s cast, single-target
    _Weaponskill_IcyTorment3 = 48697, // Helper->self, 6.5s cast, range 100 width 15 rect
    _Weaponskill_WitheringEternity = 48707, // Boss->self, 3.0s cast, single-target
    _Weaponskill_WitheringEternity1 = 48708, // Helper->location, 5.0s cast, range 9 circle
    _Weaponskill_SheetOfIce = 48710, // Boss->location, 4.5s cast, single-target
    _Weaponskill_SheetOfIce1 = 48712, // Helper->location, 5.5s cast, range 5 circle
    _Weaponskill_SheetOfIce2 = 48711, // Boss->location, no cast, single-target
    _Weaponskill_ = 48701, // Helper->self, 2.0s cast, range 46 width 23 rect
    _Weaponskill_Cauterize = 48700, // Boss->self, 10.0s cast, single-target
    _Weaponskill_1 = 48702, // Helper->self, 2.0s cast, range 60 circle
    _Spell_Blizzard = 48621, // 4CC1->player, no cast, single-target
    _Weaponskill_Cauterize1 = 48703, // Boss->self, no cast, single-target
    _Weaponskill_Cauterize2 = 48704, // Helper->self, 1.0s cast, range 46 width 23 rect
    _Weaponskill_Touchdown = 48705, // Boss->self, no cast, single-target
    _Weaponskill_Touchdown1 = 48706, // Helper->self, 0.5s cast, range 60 circle
}

class IcyTorment(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_IcyTorment1, AID._Weaponskill_IcyTorment3], new AOEShapeRect(100, 7.5f));
class RimeWreath(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_RimeWreath, "Raidwide + frostbite");
class WitheringEternity(BossModule module) : Components.VoidzoneAtCastTarget(module, 9, AID._Weaponskill_WitheringEternity1, OID.WitheringEternity, 0.9f);
class SheetOfIce(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SheetOfIce1, 5);
class IceSprite(BossModule module) : Components.Adds(module, (uint)OID._Gen_IceSprite);

class Cauterize(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Cauterize2)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_)
            _predicted.Add(new(new AOEShapeRect(46, 11.5f), spell.LocXZ, spell.Rotation, _predicted.Count > 0 ? _predicted[^1].Activation.AddSeconds(4.3f) : Module.CastFinishAt(spell, 13)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class Touchdown(BossModule module) : Components.Knockback(module, AID._Weaponskill_Touchdown1)
{
    Source? _src;
    int _diveCounter;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_src).Where(_ => _diveCounter == 4);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Cauterize2)
            _diveCounter++;

        if (spell.Action == WatchedAction)
        {
            _src = null;
            _diveCounter = 0;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_1)
            _src = new(spell.LocXZ, 30, Module.CastFinishAt(spell, 22.7f));
    }
}

class IceDragonPieceStates : StateMachineBuilder
{
    public IceDragonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcyTorment>()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<WitheringEternity>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<IceSprite>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14606)]
public class IceDragonPiece(ModuleInit init) : BossModule(init, new(120, 0), new ArenaBoundsSquare(20));

