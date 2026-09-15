namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D133Malphas;

public enum OID : uint
{
    Boss = 0x4C28, // R5.460, x1
    Helper = 0x233C, // R0.500, x21, Helper type
    ScrapGargoyle = 0x4C29, // R2.070, x6
    ScrapByblos = 0x4C2A, // R3.600, x2
    ScrapHeapGargoyle = 0x4E4E, // R1.620, x6
    ScrapHeapByblos = 0x4C2B, // R3.600, x2
}

public enum AID : uint
{
    AutoAttack = 50220, // Boss->player, no cast, single-target
    RubbishDisposal = 48920, // Boss->self, 5.0s cast, range 50 circle
    VoidDark = 50313, // Boss->self, 5.0s cast, range 30 180-degree cone
    Jump = 48932, // Boss->location, no cast, single-target
    ScrapMeddle = 48921, // Boss->self, 2.3+0.7s cast, single-target
    Goekinesis = 48933, // Helper->self, 3.0s cast, range 80 width 4 rect
    PuppetStrings = 48922, // Boss->self, 5.0+1.5s cast, single-target
    MetallicMiasma1 = 48936, // ScrapByblos->self, 0.5s cast, single-target
    MetallicMiasmaFast = 48937, // Helper->self, 6.5s cast, range 60 30-degree cone
    MetallicMiasma2 = 48942, // ScrapByblos->self, no cast, single-target
    MetallicMiasmaSlow = 48943, // Helper->self, 14.5s cast, range 60 30-degree cone
    CastOffHalo1 = 48934, // ScrapGargoyle->self, 0.5s cast, single-target
    CastOffHaloFast = 48935, // Helper->self, 6.5s cast, range 7 circle
    CastOffHalo2 = 48939, // ScrapGargoyle->self, no cast, single-target
    CastOffHaloSlow = 48940, // Helper->self, 14.5s cast, range 7 circle
    GluttonousWireCast = 48929, // Boss->self, 4.0+1.0s cast, single-target
    GluttonousWire = 48930, // Helper->players, 5.0s cast, range 6 circle
    PuppetPair = 48923, // Boss->self, 5.0+1.5s cast, single-target
    StringUp = 48931, // Boss->self, 5.0s cast, range 50 circle
    ShadowPlayCast = 50315, // Boss->self, 4.2+0.8s cast, single-target
    ShadowPlay = 50314, // Helper->player, 5.0s cast, range 6 circle
    Unk1 = 48938, // ScrapGargoyle->self, no cast, single-target
    Unk2 = 48941, // ScrapByblos->self, no cast, single-target
    PuppetMastery = 48925, // Boss->self, 13.0+1.5s cast, single-target
    WrathfulWire = 48928, // Helper->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    Unk1 = 2056, // none->ScrapByblos/ScrapGargoyle, extra=0x468/0x469
    Unk2 = 2552, // Boss->player, extra=0x479
}

public enum IconID : uint
{
    Stack = 62, // player->self
    Countdown = 706, // player->self
    Checkmark = 136, // player->self
    Tankbuster = 344, // player->self
    Spread = 558, // player->self
}

class RubbishDisposal(BossModule module) : Components.RaidwideCast(module, AID.RubbishDisposal);
class VoidDark(BossModule module) : Components.StandardAOEs(module, AID.VoidDark, new AOEShapeCone(30f, 90.Degrees()));
class Goekinesis(BossModule module) : Components.StandardAOEs(module, AID.Goekinesis, new AOEShapeRect(80f, 2f));
class MetallicMiasma(BossModule module) : Components.GroupedAOEs(module, [AID.MetallicMiasmaFast, AID.MetallicMiasmaSlow], new AOEShapeCone(60, 15.Degrees()));
class CastOffHalo(BossModule module) : Components.GroupedAOEs(module, [AID.CastOffHaloFast, AID.CastOffHaloSlow], new AOEShapeCircle(7));
class StringUp(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StringUp)
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StringUp)
            Array.Fill(PlayerStates, default);
    }
}
class GluttonousWire(BossModule module) : Components.StackWithCastTargets(module, AID.GluttonousWire, 6);
class WrathfulWire(BossModule module) : Components.SpreadFromCastTargets(module, AID.WrathfulWire, 5);
class ShadowPlay(BossModule module) : Components.BaitAwayCast(module, AID.ShadowPlay, new AOEShapeCircle(6), true);

class D133MalphasStates : StateMachineBuilder
{
    public D133MalphasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RubbishDisposal>()
            .ActivateOnEnter<VoidDark>()
            .ActivateOnEnter<Goekinesis>()
            .ActivateOnEnter<MetallicMiasma>()
            .ActivateOnEnter<CastOffHalo>()
            .ActivateOnEnter<StringUp>()
            .ActivateOnEnter<GluttonousWire>()
            .ActivateOnEnter<ShadowPlay>()
            .ActivateOnEnter<WrathfulWire>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14758)]
public class D133Malphas(WorldState ws, Actor primary) : BossModule(ws, primary, new(760, -803), new ArenaBoundsCircle(20));
