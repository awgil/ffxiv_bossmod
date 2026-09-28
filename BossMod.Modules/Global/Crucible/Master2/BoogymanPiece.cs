
namespace BossMod.Global.Crucible.BoogymanPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x5, Helper type
    Boss = 0x4CE2, // R1.800, x1
    _Gen_BombPiece = 0x4CE4, // R0.900, x0 (spawn during fight)
    _Gen_DeepeyePiece = 0x4CE3, // R1.200, x0 (spawn during fight)
    _Gen_LightSprite = 0x4CE5, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 49217, // Boss->player, no cast, single-target
    _Weaponskill_Swoop = 49219, // Boss->location, 3.0s cast, width 4 rect charge
    _Weaponskill_SpinningStrike = 49221, // Boss->self, 1.0+1.0s cast, single-target
    _Weaponskill_SpinningStrike1 = 49222, // Helper->self, 2.0s cast, range 30 circle
    _Weaponskill_Provision = 49215, // Boss->self, 3.0s cast, single-target
    _AutoAttack_1 = 49218, // Boss->player, no cast, single-target
    _Weaponskill_Swoop1 = 49220, // Boss->location, 3.0s cast, width 4 rect charge
    _Weaponskill_Clearout = 49223, // Boss->self, 1.0+0.5s cast, single-target
    _Weaponskill_Clearout1 = 49224, // Helper->self, 1.5s cast, range 40 120-degree cone
    _Weaponskill_VeilOfDarkness = 49209, // Boss->self, 4.0s cast, range 50 circle
    _Ability_ = 49229, // Boss->location, no cast, single-target
    _Weaponskill_Explosion = 49210, // 4CE4->self, 5.0s cast, range 9 circle
    _Weaponskill_FeralLeap = 49230, // Boss->player, no cast, single-target
    _Spell_Banish = 49213, // 4CE5->player, 1.5s cast, single-target
    _Weaponskill_DiffuseLight = 49212, // 4CE5->self, 1.5s cast, range 20 45-degree cone
    _Weaponskill_SelfDestruct = 49211, // 4CE4->self, 8.0s cast, range 50 circle
    _Weaponskill_Oogle = 49214, // 4CE3->self, 10.0s cast, range 40 circle
    _Weaponskill_SwingRound = 49227, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_SwingRound1 = 49228, // Helper->self, 5.0s cast, range 12 circle
    _AutoAttack_2 = 49682, // 4CE3->player, no cast, single-target
    _Weaponskill_Provision1 = 49216, // Boss->self, 3.0s cast, single-target
    _Weaponskill_SwingRound2 = 49225, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_SwingRound3 = 49226, // Helper->self, 5.0s cast, range 40 circle
    _Weaponskill_RipplesOfGloom = 49231, // Boss->self, 5.0s cast, range 50 circle
}

public enum SID : uint
{
    _Gen_SlashingResistanceDown = 3130, // Boss->player, extra=0x0
    _Gen_BluntResistanceDown = 3132, // Boss->player, extra=0x0
    _Gen_ = 2659, // Boss->Boss, extra=0xBC5
    _Gen_Concealed = 3997, // none->4CE4/4CE3, extra=0xA
    _Gen_Concealed1 = 1621, // none->Boss, extra=0x3
    _Gen_Invisible = 616, // Boss->Boss, extra=0x0
    _Gen_Blind = 5381, // Boss->player, extra=0x0
}

class Swoop(BossModule module) : Components.ChargeAOEs(module, AID._Weaponskill_Swoop, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Swoop or AID._Weaponskill_Swoop1)
        {
            var dir = spell.LocXZ - caster.Position;
            Casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth), Angle.FromDirection(dir)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Swoop or AID._Weaponskill_Swoop1)
            Casters.RemoveAll(e => e.caster == caster);
    }
}

class SpinningStrike(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_SpinningStrike1)
{
    AOEInstance? _predicted;

    public static readonly AOEShape Shape = new AOEShapeCircle(30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Swoop)
        {
            var dest = spell.LocXZ;
            _predicted = new(Shape, dest, default, Module.CastFinishAt(spell, 5.1f));
        }

        if (spell.Action == WatchedAction)
            _predicted = new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _predicted = null;
    }
}

class Clearout(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Clearout1)
{
    AOEInstance? _predicted;

    public static readonly AOEShape Shape = new AOEShapeCone(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Swoop1)
        {
            var dest = spell.LocXZ;
            _predicted = new(Shape, dest, Angle.FromDirection(Arena.Center - dest), Module.CastFinishAt(spell, 5.1f));
        }

        if (spell.Action == WatchedAction)
            _predicted = new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _predicted = null;
    }
}

class VeilOfDarkness(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_VeilOfDarkness);

class Adds1(BossModule module) : Components.AddsMulti(module, [OID._Gen_BombPiece, OID._Gen_DeepeyePiece], 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var add in Actors)
        {
            hints.SetPriority(add, 2);

            // if actor is too far away from us they won't have an Enemy entry, since Concealed makes them untargetable
            if (add.PendingHPRaw > 0)
                hints.GoalZones.Add(AIHints.GoalSingleTarget(add.Position, add.HitboxRadius + actor.HitboxRadius + 3, 2));
        }
    }
}
class Adds2(BossModule module) : Components.Adds(module, (uint)OID._Gen_LightSprite, 1);

class Explosion(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Explosion, 9);
class DiffuseLight(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DiffuseLight, new AOEShapeCone(20, 22.5f.Degrees()));
class SelfDestruct(BossModule module) : Components.CastHint(module, AID._Weaponskill_SelfDestruct, "Bomb enraging!", true);
class Oogle(BossModule module) : Components.CastGaze(module, AID._Weaponskill_Oogle);

class SwingRoundAOE(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SwingRound1, 12);
class SwingRoundKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_SwingRound3, 20);

class RipplesOfGloom(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_RipplesOfGloom, "Raidwide + blind");

class BoogymanPieceStates : StateMachineBuilder
{
    public BoogymanPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<SpinningStrike>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<VeilOfDarkness>()
            .ActivateOnEnter<Adds1>()
            .ActivateOnEnter<Adds2>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<DiffuseLight>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<Oogle>()
            .ActivateOnEnter<SwingRoundAOE>()
            .ActivateOnEnter<SwingRoundKB>()
            .ActivateOnEnter<RipplesOfGloom>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14638)]
public class BoogymanPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));
