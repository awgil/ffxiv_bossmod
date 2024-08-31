namespace BossMod.Heavensward.Dungeon.D02SohmAl.D023Tioman;
public enum OID : uint
{
    Boss = 0xE96, // R6.840, x?
    LeftWingOfTragedy = 0x10B4, // Spawn During the Fight
    RightWingOfInjury = 0x10B5, // Spawn During the Fight
    Helper = 0x233C, // Helper

}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AbyssicBuster = 3811, // E96->self, no cast, range 25+R ?-degree cone
    ChaosBlast = 3813, // E96->location, 2.0s cast, range 2 circle
    ChaosBlast2 = 3819, // 1B2->self, 2.0s cast, range 50+R width 4 rect
    Comet = 3816, // 1B2->location, 3.0s cast, range 4 circle
    Comet2 = 3814, // E96->self, 4.0s cast, single-target
    MeteorImpact = 4999, // 1B2->self, 3.5s cast, range 30+R circle
    MeteorImpact2 = 4997, // 13AD->self, no cast, range 30 circle
    Heavensfall = 3815, // E96->self, no cast, single-target
    Heavensfall2 = 3817, // 1B2->player, no cast, range 5 circle
    Heavensfall3 = 3818, // 1B2->location, 3.0s cast, range 5 circle
    HypothermalCombustion = 3156, // F2D->self, 4.0s cast, range 8+R circle
    DarkStar = 3812, // E96->self, 5.0s cast, range 50+R circle
}
public enum IconID : uint
{
    Comet = 10, // player
    Meteor = 7, // player
}

class AbyssicBuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AbyssicBuster), new AOEShapeCone(25, 45.Degrees()));
class ChaosBlast(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ChaosBlast), 2);
class ChaosBlast2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChaosBlast2), new AOEShapeRect(50, 2));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);
class Comet2(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(30), 7, default, 9.1f, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == 4999)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new AOEShapeCircle(23), Module.Center);
    }
}
class MultiAddModule(BossModule module) : Components.AddsMulti(module, [(uint)OID.LeftWingOfTragedy, (uint)OID.RightWingOfInjury]);
class MeteorImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeteorImpact), 30);
//class MeteorImpact2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeteorImpact), 30);
//class Heavensfall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Heavensfall), 5);
//class Heavensfall2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Heavensfall2), new AOEShapeCircle(5));
class Heavensfall3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Heavensfall3), 5);
class DarkStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DarkStar));

class D023TiomanStates : StateMachineBuilder
{
    public D023TiomanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssicBuster>()
            .ActivateOnEnter<ChaosBlast>()
            .ActivateOnEnter<ChaosBlast2>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<Comet2>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<Heavensfall3>()
            .ActivateOnEnter<DarkStar>()
            .ActivateOnEnter<MultiAddModule>();

    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3798)]
public class D023Tioman(WorldState ws, Actor primary) : BossModule(ws, primary, new(-103, -395), new ArenaBoundsCircle(27f));

