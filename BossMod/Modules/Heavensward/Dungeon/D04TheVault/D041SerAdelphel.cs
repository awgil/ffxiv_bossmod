namespace BossMod.Heavensward.Dungeon.D04TheVault.D041SerAdelphel;

public enum OID : uint
{
    Boss = 0x1051, // R2.200, x1
    Helper = 0xD25, // R0.500, x3, mixed types
    BrightSphere = 0x1052, // R1.000, x0 (spawn during fight)
    SerAdelphelBrightblade = 0x104E, // R0.500, x1
    VaultDeacon = 0x1050, // R0.500, x1
    VaultOstiary = 0x104F, // R0.500, x2
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
    Actor1e9867 = 0x1E9867, // R2.000, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // VaultOstiary/Boss->player, no cast, single-target
    HoliestOfHoly = 4126, // Boss->self, 3.0s cast, range 80+R circle
    HeavenlySlash = 4125, // Boss->self, no cast, range 8+R ?-degree cone
    HolyShieldBash = 4127, // Boss->player, 4.0s cast, single-target
    SolidAscension1 = 4128, // Boss->player, no cast, single-target
    SolidAscension2 = 4129, // Helper->player, no cast, single-target
    ShiningBlade = 4130, // Boss->location, no cast, width 6 rect charge
    BrightFlare = 4132, // Brightsphere->self, no cast, range 5+R circle
    Unknown1 = 4121, // Boss->self, no cast, single-target
    Execution = 4131, // Boss->location, no cast, range 5 circle
    Unknown2 = 4124, // Boss->self, no cast, single-target
    Unknown3 = 4256, // SerAdelphelBrightblade->self, no cast, single-target
    Retreat = 4257, // SerAdelphelBrightblade->self, no cast, single-target
}

public enum SID : uint
{
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Bleeding = 273, // Brightsphere->player, extra=0x0

}

public enum IconID : uint
{
    Icon_16 = 16, // player
    Icon_32 = 32, // player
}

class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HoliestOfHoly), "Raidwide");
class HolyShieldBash(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HolyShieldBash), 3);

class BrightSphere(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5);
    private readonly List<Actor> _spheres = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in _spheres)
            yield return new(circle, s.Position, default);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BrightSphere)
            _spheres.Add(actor);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BrightFlare)
            _spheres.Clear();
    }
}

class D041SerAdelphelStates : StateMachineBuilder
{
    public D041SerAdelphelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<HolyShieldBash>()
            //.ActivateOnEnter<BrightSphere>()
            .ActivateOnEnter<HoliestOfHoly>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team, Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3634)]
public class D041SerAdelphel(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsCircle(20));
