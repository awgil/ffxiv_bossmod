#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.BorgnyTheVenomous;

public enum OID : uint
{
    Boss = 0x4CD8, // R7.000, x1
    _Gen_ToxicMass = 0x4CD9, // R1.200, x0 (spawn during fight)
    _Gen_PoisonCloud = 0x4CDA, // R2.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x7, Helper type

    PoisonPuddle = 0x1EB704
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Ability_ = 48806, // Boss->location, no cast, single-target
    _Weaponskill_ToxicBreath = 48807, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ToxicBreath1 = 48808, // Helper->self, no cast, range ?-60 120?-degree donut cone
    _Weaponskill_ToxicVomit = 48809, // Boss->player, 3.5+1.5s cast, range 6 circle
    _Weaponskill_ToxicVomit1 = 48810, // Boss->player, no cast, range 6 circle
    _Weaponskill_FumingVomit = 48811, // Boss->self, 4.0+2.0s cast, single-target
    _Weaponskill_FumingVomit1 = 48812, // Helper->location, 6.0s cast, range 6 circle
    _Weaponskill_WrigglingPhlegm = 48817, // Boss->self, 7.9s cast, single-target
    _Weaponskill_WrigglingPhlegm1 = 48818, // Boss->location, no cast, single-target
    _Weaponskill_WrigglingPhlegm2 = 48819, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_Cauterize = 48813, // Boss->self, 5.2+0.8s cast, single-target
    _Weaponskill_Cauterize1 = 48814, // Helper->self, 6.0s cast, range 48 width 20 rect
    _Weaponskill_Touchdown = 48815, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Touchdown1 = 48816, // Helper->self, 4.0s cast, range 6-60 donut
    _Weaponskill_Touchdown2 = 48828, // Helper->self, 5.5s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_Toxicosis = 5183, // Helper/Boss->player, extra=0x1/0x2/0x3/0x4
}

public enum IconID : uint
{
    _Gen_Icon_m0128_trg_t1 = 171, // player->self
    _Gen_Icon_suteloc6s6m_1k1 = 669, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 17, // Boss->player
}

class ToxicBreath(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_ToxicBreath1)
{
    AOEInstance? _source;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ToxicBreath)
        {
            var pt = Arena.Center - spell.Rotation.ToDirection() * 20;
            _source = new(new AOEShapeCone(50, 60.Degrees()), pt, spell.Rotation, WorldState.FutureTime(5.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (++NumCasts >= 6)
            {
                _source = null;
                NumCasts = 0;
            }
        }
    }
}

class ToxicVomit(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, AID._Weaponskill_ToxicVomit, OID.PoisonPuddle, 1.4f)
{
    readonly ResistHelper Resists = module.FindComponent<ResistHelper>()!;

    bool PoisonImmune => Resists[ResistHelper.Resistance.Poison] > WorldState.FutureTime(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(p => p with { Risky = p.Risky && !PoisonImmune });

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!PoisonImmune)
            base.AddHints(slot, actor, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ToxicVomit or AID._Weaponskill_ToxicVomit1)
            _predictedByEvent.Add((WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn + ActivationDelay)));
    }
}

class FumingVomit(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_FumingVomit1, 6);

class PoisonCloud(BossModule module) : Components.Voidzone(module, 2, OID._Gen_PoisonCloud, moveHintLength: 6);

class WrigglingPhlegm(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WrigglingPhlegm2, 6);
class ToxicMass(BossModule module) : Components.Adds(module, (uint)OID._Gen_ToxicMass, 1);

class Cauterize(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Cauterize1, new AOEShapeRect(48, 10));
class TouchdownCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Touchdown2, 6);
class Touchdown(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_Touchdown1, 30, ignoreImmunes: true, stopAtWall: true);

class BorgnyTheVenomousStates : StateMachineBuilder
{
    public BorgnyTheVenomousStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ResistHelper>()
            .ActivateOnEnter<ToxicBreath>()
            .ActivateOnEnter<ToxicVomit>()
            .ActivateOnEnter<FumingVomit>()
            .ActivateOnEnter<PoisonCloud>()
            .ActivateOnEnter<WrigglingPhlegm>()
            .ActivateOnEnter<ToxicMass>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<TouchdownCircle>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14628, BitmapType = BossModuleInfo.BitmapType.Enabled)]
public class BorgnyTheVenomous(ModuleInit init) : BossModule(init, new(920, -420), new ArenaBoundsCircle(20));
