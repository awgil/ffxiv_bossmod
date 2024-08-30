namespace BossMod.Heavensward.Dungeon.D04Vault.D041SerAdelphelBrightblade;

public enum OID : uint
{
    Boss = 0x104E, // R0.500, SerAdelphelBrightblade 
    VaultDeacon = 0x1050, // R0.500, x1
    VaultOstiary = 0x104F, // R0.500, x2
    SerAdelphel = 0x1051, // R2.200, x1
    Brightsphere = 0x1052, // R1.000, x?
    Helper2 = 0xD25,  // "DawnKnight"?
    Helper = 0x233C, // x3
}
public enum AID : uint
{
    Attack = 870, // 105A/105B/104F/104E/1051/1060/1069/105F/1053/1054/1068->player, no cast, single-target
    HoliestOfHoly = 4126, // 1051->self, 3.0s cast, range 80+R circle
    HeavenlySlash = 4125, // 1051->self, no cast, range 8+R ?-degree cone
    HolyShieldBash = 4127, // 1051->player, 4.0s cast, single-target
    SolidAscension = 4128, // 1051->player, no cast, single-target
    SolidAscension2 = 4129, // D25->player, no cast, single-target
    Advent = 4979, // 104E/1053->self, no cast, single-target
    Advent2 = 4980, // D25->self, no cast, range 80 circle
    Advent3 = 4122, // 104E/1053->self, no cast, single-target
    ShiningBlade = 4130, // 1051->location, no cast, width 6 rect charge
    Execution = 4131, // 1051->location, no cast, range 5 circle

    Bloodstain = 1099,
    BrightFlare = 4132, // 1052->self, no cast, range 5+R circle
}
public enum IconID : uint
{
    HolyShieldBash = 16, // player
    Execution = 32, // player
}
class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HoliestOfHoly));
class HeavenlySlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeavenlySlash), new AOEShapeCone(8, 45.Degrees()));
class HolyShieldBash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HolyShieldBash));
class SolidAscension(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SolidAscension));
class SolidAscension2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SolidAscension2));
//class ShiningBlade(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.ShiningBlade), 3);
class Bloodstain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bloodstain), new AOEShapeCircle(5));
class Execution(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Execution, ActionID.MakeSpell(AID.Execution), 5, 7.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Execution)
        {
            Spreads.Clear();
        }
    }
};
class BrightFlare(BossModule module) : Components.GenericAOEs(module)
{
    private IEnumerable<Actor> Brightspheres => Module.Enemies(OID.Brightsphere).Where(e => !e.IsDead);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Brightspheres.Select(b => new AOEInstance(new AOEShapeCircle(6f), b.Position));
}
class MultiAddsModule(BossModule module) : Components.AddsMulti(module, [(uint)OID.VaultOstiary, (uint)OID.VaultDeacon, (uint)OID.SerAdelphel]);
class D041SerAdelphelBrightbladeStates : StateMachineBuilder
{
    public D041SerAdelphelBrightbladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HoliestOfHoly>()
            .ActivateOnEnter<HeavenlySlash>()
            .ActivateOnEnter<HolyShieldBash>()
            .ActivateOnEnter<SolidAscension>()
            .ActivateOnEnter<SolidAscension2>()
            .ActivateOnEnter<Execution>()
            .ActivateOnEnter<BrightFlare>()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<MultiAddsModule>();

    }
}
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3849)]
public class D041SerAdelphelBrightblade(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsCircle(19.5f));
