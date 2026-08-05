namespace BossMod.Dawntrail.Foray.CriticalEngagement.ClaretDragon;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x19, Helper type
    _Gen_ClaretDragon = 0x4D25, // R1.000, x1
    Boss = 0x4C46, // R5.000, x1
    _Gen_Necrohaze = 0x4C47, // R1.500, x0 (spawn during fight)
    _Gen_AetherialWard = 0x4C48, // R7.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Ability_ = 48279, // _Gen_ClaretDragon->self, no cast, ???
    _AutoAttack_ = 48259, // Boss->player, no cast, single-target
    _Spell_HowlingDarkness = 48277, // Boss->self, 5.0s cast, single-target
    _Spell_HowlingDarkness1 = 48278, // Helper->self, no cast, ???
    _Spell_SnakingNecrobreath = 48260, // Boss->self, 6.0s cast, range 60 270-degree cone
    _Spell_GraveMold = 48261, // Boss->self, 5.0s cast, single-target
    _Spell_GraveMold1 = 48262, // Helper->self, 6.0s cast, range 8 circle
    _Ability_Necrohaze = 48263, // _Gen_Necrohaze->self, no cast, range 5 circle
    _Ability_Soar = 50488, // Boss->self, 4.0s cast, single-target
    _Ability_1 = 48302, // Boss->self, no cast, single-target
    _Weaponskill_Cauterize = 48264, // Boss->self, 6.0s cast, single-target
    _Weaponskill_Cauterize1 = 48265, // Helper->self, 7.0s cast, range 40 width 10 rect
    _Ability_Catching = 48267, // _Gen_Necrohaze->self, no cast, range 30 width 10 rect
    _Weaponskill_ = 48266, // Boss->self, no cast, single-target
    _Ability_AetherialWard = 48271, // Boss->self, 4.0+0.5s cast, single-target
    _Spell_Necrohaze = 50484, // Helper->self, 4.0s cast, range 5 circle
    _Ability_2 = 48275, // Boss->self, no cast, single-target
    _Ability_Necrohaze1 = 48269, // Helper->self, no cast, range 5 circle
    _Ability_Necrohaze2 = 48268, // Helper->location, no cast, range 5 circle
    _Ability_3 = 48276, // Boss->self, no cast, single-target
    _Spell_BreathInThrees = 48270, // Boss->self, 5.0s cast, range 60 120-degree cone
    _Spell_BreathInThrees1 = 48248, // Boss->self, 2.5s cast, range 60 120-degree cone
}

public enum SID : uint
{
    _Gen_GradualZombification = 5059, // Boss/Helper/_Gen_Necrohaze->player, extra=0x1
    _Gen_ZombieProof = 5138, // Helper/_Gen_Necrohaze->player, extra=0x0
    _Gen_Zombification = 2305, // _Gen_Necrohaze/Helper->player, extra=0x0
    _Gen_ = 2056, // Boss->Boss, extra=0x164
    _Gen_Heavy = 1796, // none->_Gen_Necrohaze, extra=0x32
    _Gen_DirectionalInvincibility = 1125, // none->_Gen_AetherialWard, extra=0x0
}

class AetherialWard(BossModule module) : Components.Adds(module, (uint)OID._Gen_AetherialWard, 1);
class Thingy(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _shield = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EC094)
        {
            if (state == 0x00010002)
                _shield.Add(actor);
            if (state == 0x00040008)
                _shield.Remove(actor);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in _shield)
        {
            Arena.Actor(e.Position + e.Rotation.ToDirection() * 5, e.Rotation, ArenaColor.Object);
        }
    }
}

class ClaretDragonStates : StateMachineBuilder
{
    public ClaretDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherialWard>()
            .ActivateOnEnter<Thingy>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14787)]
public class ClaretDragon(WorldState ws, Actor primary) : CEModule(ws, primary, new(-688, 150), new ArenaBoundsSquare(20));

