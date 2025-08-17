namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

enum Summon
{
    None,
    Seiryu,
    Genbu,
    Suzaku,
    Byakko
}

class SummonShijin(BossModule module) : BossComponent(module)
{
    public Summon NextSummon { get; private set; }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        switch (((OID)actor.OID, id))
        {
            case (OID._Gen_DawnboundSeiryu, 0x11D1):
                NextSummon = Summon.Seiryu;
                break;
            case (OID._Gen_MoonboundGenbu, 0x11D1):
                NextSummon = Summon.Genbu;
                break;
            case (OID._Gen_SunboundSuzaku, 0x11D2):
                NextSummon = Summon.Suzaku;
                break;
            case (OID._Gen_DuskboundByakko, 0x11D5):
                NextSummon = Summon.Byakko;
                break;
            default: break;
        }
    }
}

#region Seiryu
class EastwindWheel1(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(60, 9), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_EastwindWheel or AID._Ability_EastwindWheel3)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_EastwindWheel1)
        {
            NumCasts++;
            if (NumCasts % 4 == 0)
                _casters.Clear();
        }
    }
}
class EastwindWheel2(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _predicted;

    // 94.48
    // 105.20
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_m0851_turnleft_c0g:
                _predicted = new(new AOEShapeCone(60, 45.Degrees()), actor.Position, actor.Rotation + 45.Degrees(), WorldState.FutureTime(10.7f));
                break;
            case IconID._Gen_Icon_m0851_turnright_c0g:
                _predicted = new(new AOEShapeCone(60, 45.Degrees()), actor.Position, actor.Rotation - 45.Degrees(), WorldState.FutureTime(10.7f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_EastwindWheel2)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}

class StonegaIII(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_StonegaIII, 6);
class Quake2(BossModule module) : Components.StandardAOEs(module, AID._Spell_Quake2, 6);
#endregion

#region Genbu
class MoontideFont(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_MoontideFont, AID._Ability_MoontideFont1], new AOEShapeCircle(9), maxCasts: 11);
class MidwinterMarch(BossModule module) : Components.StandardAOEs(module, AID._Ability_MidwinterMarch1, 12);
class NorthernCurrent(BossModule module) : Components.StandardAOEs(module, AID._Ability_NorthernCurrent, new AOEShapeDonut(12, 60));
#endregion

#region Suzaku
class VermilionFlight(BossModule module) : Components.StandardAOEs(module, AID._Ability_VermilionFlight1, new AOEShapeRect(60, 10));
class ArmOfPurgatory(BossModule module) : Components.StandardAOEs(module, AID._Ability_ArmOfPurgatory1, 3);
class StonegaIII2(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_StonegaIII2, 6);
#endregion

#region Byakko
class ByakkoWalls(BossModule module) : Components.StandardAOEs(module, AID._Ability_1, new AOEShapeRect(5, 8));
// we could show this as soon as he jumps to the mirror but fuck that its not like its an ultimate
class GloamingGleam(BossModule module) : Components.StandardAOEs(module, AID._Ability_GloamingGleam, new AOEShapeRect(50, 6));
class RazorFang(BossModule module) : Components.StandardAOEs(module, AID._Ability_RazorFang, 20);
class Quake(BossModule module) : Components.StandardAOEs(module, AID._Spell_Quake1, 10);
#endregion
