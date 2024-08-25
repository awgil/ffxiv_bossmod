namespace BossMod.Heavensward.Quest.Heliodrome;

public enum OID : uint
{
    Boss = 0x195E,
    Helper = 0x233C,
    _Gen_ImperialSagittarius = 0x1965, // R0.500, x0 (spawn during fight)
    _Gen_ImperialEques = 0x1962, // R0.500, x0 (spawn during fight)
    _Gen_ImperialSecutor = 0x1963, // R0.500, x0 (spawn during fight)
    _Gen_ImperialSignifer = 0x1964, // R0.500, x0 (spawn during fight)
    _Gen_ImperialLaquearius = 0x1961, // R0.500, x0 (spawn during fight)
    _Gen_ImperialHoplomachus = 0x1960, // R0.500, x0 (spawn during fight)
    GrynewahtP2 = 0x195F, // R0.500, x0 (spawn during fight)
    _Gen_ImperialColossus = 0x1966, // R3.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_AugmentedShatter = 7609, // Boss->player/195D, no cast, single-target
    _Weaponskill_AugmentedUprising = 7608, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    _Weaponskill_AugmentedSuffering = 7607, // Boss->self, 3.5s cast, range 6+R circle
    _Weaponskill_Heartstopper = 866, // _Gen_ImperialEques->self, 2.5s cast, range 3+R width 3 rect
    _Spell_Thunder = 968, // _Gen_ImperialSignifer->player/195C/195B, 1.0s cast, single-target
    _Ability_FightOrFlight = 20, // _Gen_ImperialHoplomachus->self, no cast, single-target
    _Weaponskill_Windbite = 113, // _Gen_ImperialSagittarius->player/195B/195C, no cast, single-target
    _Weaponskill_VenomousBite = 100, // _Gen_ImperialSagittarius->player/195B/195C, no cast, single-target
    _Weaponskill_Overpower = 720, // _Gen_ImperialLaquearius->self, 2.1s cast, range 6+R 90-degree cone
    _Ability_SecondWind = 57, // _Gen_ImperialSecutor->self, no cast, single-target
    _Weaponskill_Feint = 76, // _Gen_ImperialEques->195C/195B, no cast, single-target
    _Weaponskill_GrandSword = 7615, // _Gen_ImperialColossus->self, 3.0s cast, range 18+R 120-degree cone
    _Weaponskill_MagitekRay = 7617, // _Gen_ImperialColossus->location, 3.0s cast, range 6 circle
    _Weaponskill_SelfDetonate = 7618, // _Gen_ImperialColossus->self, 10.0s cast, range 40+R circle
    _Weaponskill_GrandStrike = 7616, // _Gen_ImperialColossus->self, 2.5s cast, range 45+R width 4 rect
    _Weaponskill_Firebomb = 7610, // Boss->players, no cast, range 4 circle
    __ShrapnelShell = 7613, // Boss->self, no cast, single-target
    _Weaponskill_ShrapnelShell = 7614, // GrynewahtP2->location, 2.5s cast, range 6 circle
    __MagitekMissiles = 7611, // Boss->self, no cast, single-target
    _Weaponskill_MagitekMissiles = 7612, // GrynewahtP2->location, 5.0s cast, range 15 circle

}

class MagitekMissiles(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekMissiles), 15);
class ShrapnelShell(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ShrapnelShell), 6);
class Firebomb(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(0x1E86DF).Where(e => e.EventState != 7));

class Uprising(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedUprising), new AOEShapeCone(8.5f, 60.Degrees()));
class Suffering(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedSuffering), new AOEShapeCircle(6.5f));
class Heartstopper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Heartstopper), new AOEShapeRect(3.5f, 1.5f));
class Overpower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Overpower), new AOEShapeCone(6, 45.Degrees()));
class GrandSword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GrandSword), new AOEShapeCone(21, 60.Degrees()));
class MagitekRay(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekRay), 6);
class GrandStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GrandStrike), new AOEShapeRect(48, 2));

class Adds(BossModule module) : Components.AddsMulti(module, [0x1960, 0x1961, 0x1962, 0x1963, 0x1964, 0x1965, 0x1966])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID == OID._Gen_ImperialColossus ? 5 : e.Actor.TargetID == actor.InstanceID ? 1 : 0;
    }
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnDirectorUpdate(uint directorID, uint updateID, uint[] updateParams)
    {
        if (updateID == 0x10000002)
            Arena.Bounds = new ArenaBoundsCircle(20);
    }
}

class ReaperAI(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget != null && Player.MountId == 103)
        {
            if ((OID)primaryTarget.OID == OID._Gen_ImperialColossus)
                UseGCD(Roleplay.AID.DiffractiveMagitekCannon, primaryTarget, targetPos: primaryTarget.PosRot.XYZ());

            UseGCD(Roleplay.AID.MagitekCannon, primaryTarget, targetPos: primaryTarget.PosRot.XYZ());
        }
    }
}

class GrynewahtStates : StateMachineBuilder
{
    public GrynewahtStates(BossModule module) : base(module)
    {
        State build(uint id) => SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Uprising>()
            .ActivateOnEnter<Suffering>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<ShrapnelShell>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<MagitekMissiles>();

        SimplePhase(1, id => build(id).ActivateOnEnter<Bounds>(), "P1")
            .Raw.Update = () => Module.Enemies(OID.GrynewahtP2).Any();
        DeathPhase(0x100, id => build(id).ActivateOnEnter<ReaperAI>().OnEnter(() =>
        {
            Module.Arena.Bounds = new ArenaBoundsCircle(20);
        }));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 222, NameID = 5576)]
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), HexBounds)
{
    public static readonly ArenaBoundsCustom HexBounds = BuildHexBounds();

    private static ArenaBoundsCustom BuildHexBounds()
    {
        var hexSideLen = 20 / MathF.Sqrt(3);

        // slight adjustment to account for player hitbox radius, otherwise dodges can get very sketchy
        hexSideLen -= 1.5f;

        List<WDir> verts = [new(hexSideLen, 0), hexSideLen * 30.Degrees().ToDirection(), -hexSideLen * 150.Degrees().ToDirection(), new(-hexSideLen, 0), hexSideLen * -30.Degrees().ToDirection(), hexSideLen * 150.Degrees().ToDirection()];
        return new(hexSideLen, new(verts));
    }

    protected override bool CheckPull() => true;
}
