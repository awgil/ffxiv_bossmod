namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D052Rhitahtyn;

public enum OID : uint
{
    Boss = 0x346B, // R=9.0
    Helper = 0x233C,
    Helper2 = 0x35DA, // R1.000, x4
    Actor346c = 0x346C, // R1.000, x0 (spawn during fight)
    Actor346d = 0x346D, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AnvilOfTartarus = 25686, // Boss->player, 5.0s cast, single-target //Tankbuster
    Impact = 25679, // Helper->self, 4.0s cast, range 14 width 40 rect

    ShieldSkewer = 25680, // Boss->location, 11.0s cast, range 40 width 14 rect
    ShrapnelShellVisual = 25682, // Boss->self, 3.0s cast, single-target
    ShrapnelShellAOE = 25684, // Helper->location, 3.5s cast, range 5 circle

    TartareanImpact = 25685, // Boss->self, 5.0s cast, range 60 circle //Raidwide
    TartareanSpark = 25687, // Boss->self, 3.0s cast, range 40 width 6 rect

    UnknownAbility = 25688, // Boss->location, no cast, single-target //Likely Teleport
    UnknownWeaponskill = 25683, // 346C/346D->self, no cast, single-target
    Vexillatio = 25678, // Boss->self, 4.0s cast, single-target
}

public enum SID : uint
{
    Unknown1 = 2056, // none->35DA/Boss, extra=0x135/0x14E
    Unknown2 = 2193, // none->35DA, extra=0x136
}

public enum IconID : uint
{
    Icon218 = 218, // player
}

public enum TetherID : uint
{
    Tether161 = 161, // 35DA->Boss
}

class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, new AOEShapeRect(14, 20));

class ImpactWalls(BossModule module) : BossComponent(module)
{
    private static readonly List<WPos> WallContour = [new(4, 124), new(4, 134.5f), new(0.5f, 134.5f), new(0.5f, 137.5f), new(4, 137.5f), new(4, 150.5f), new(0.5f, 150.5f), new(0.5f, 153.5f), new(4, 153.5f), new(4, 164), new(18, 164), new(18, 153.5f), new(21.5f, 153.5f), new(21.5f, 150.5f), new(18, 150.5f), new(18, 137.5f), new(22, 137.5f), new(22, 134.5f), new(18, 134.5f), new(18, 124)];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsCustom(20, new(WallContour.Select(c => c - Arena.Center)));

            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class ShrapnelShellAOE(BossModule module) : Components.StandardAOEs(module, AID.ShrapnelShellAOE, 6);
class TartareanSpark(BossModule module) : Components.StandardAOEs(module, AID.TartareanSpark, new AOEShapeRect(40, 3));

class AnvilOfTartarus(BossModule module) : Components.SingleTargetCast(module, AID.AnvilOfTartarus);
class TartareanImpact(BossModule module) : Components.RaidwideCast(module, AID.TartareanImpact);
class ShieldSkewer(BossModule module) : Components.StandardAOEs(module, AID.ShieldSkewer, new AOEShapeRect(40, 7)) { }

class ImpactSafezone(BossModule module) : Components.GenericAOEs(module)
{
    private bool _isActive;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_isActive)
            yield break;

        foreach (var icicle in Module.Enemies(OID.Helper2))
        {
            if (icicle.FindStatus(SID.Unknown2) != null)
                yield return new AOEInstance(new AOEShapeRect(1.5f, 2, 1.5f), icicle.Position, Color: ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Boss && (AID)spell.Action.ID is AID.ShieldSkewer)
            _isActive = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {

        if ((OID)caster.OID is OID.Boss && (AID)spell.Action.ID is AID.ShieldSkewer)
            _isActive = false;
    }
}

class ShellTelegraph(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var x in Module.Enemies(OID.Actor346d))
            yield return new AOEInstance(new AOEShapeCircle(5), x.Position);
    }
}

class D052RhitahtynStates : StateMachineBuilder
{
    public D052RhitahtynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<ImpactWalls>()
            .ActivateOnEnter<ShrapnelShellAOE>()
            .ActivateOnEnter<TartareanSpark>()
            .ActivateOnEnter<AnvilOfTartarus>()
            .ActivateOnEnter<TartareanImpact>()
            .ActivateOnEnter<ShieldSkewer>()
            .ActivateOnEnter<ImpactSafezone>()
            .ActivateOnEnter<ShellTelegraph>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10292)]
public class D052Rhitahtyn(WorldState ws, Actor primary) : BossModule(ws, primary, new(11, 144), new ArenaBoundsSquare(20));
