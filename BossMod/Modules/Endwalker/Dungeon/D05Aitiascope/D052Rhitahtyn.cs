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

class Impact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Impact), new AOEShapeRect(14, 20));

class ImpactWalls(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _safeWalls = [];
    private readonly List<AOEInstance> _unsafeWalls = [];

    private bool _haveSafeZone;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _haveSafeZone ? _safeWalls : _unsafeWalls;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Impact)
        {
            var angle = caster.Rotation.Deg > 0 ? 90.Degrees() : 270.Degrees();

            _unsafeWalls.Add(new AOEInstance(new AOEShapeRect(13, 20), caster.Position, angle, default, ArenaColor.Danger));

            _safeWalls.Add(new AOEInstance(new AOEShapeRect(9.5f, 20), caster.Position, angle, default, ArenaColor.Danger));
            _safeWalls.Add(new AOEInstance(new AOEShapeRect(5, 6.45f), caster.Position + caster.Rotation.ToDirection() * 8, angle, default, ArenaColor.Danger));
            _safeWalls.Add(new AOEInstance(new AOEShapeRect(5, 6), caster.Position + caster.Rotation.ToDirection() * 8 + new WDir(0, -15.5f), angle, default, ArenaColor.Danger));
            _safeWalls.Add(new AOEInstance(new AOEShapeRect(5, 6), caster.Position + caster.Rotation.ToDirection() * 8 + new WDir(0, 15.5f), angle, default, ArenaColor.Danger));
        }

        if (spell.Action.ID == (uint)AID.ShieldSkewer)
        {
            _haveSafeZone = false;
            _unsafeWalls.Clear();
            _safeWalls.Clear();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShieldSkewer)
            _haveSafeZone = true;
    }
}

class ShrapnelShellAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ShrapnelShellAOE), 6);
class TartareanSpark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TartareanSpark), new AOEShapeRect(40, 3));

class AnvilOfTartarus(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.AnvilOfTartarus));
class TartareanImpact(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TartareanImpact));
class ShieldSkewer(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShieldSkewer), new AOEShapeRect(40, 7)) { }

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
                yield return new AOEInstance(new AOEShapeRect(1.5f, 1.75f, 1.5f), DoAdjust(icicle.Position));
        }
    }

    private WPos DoAdjust(WPos pos)
    {
        if (pos.X < 10)
            pos.X += 0.25f;
        else
            pos.X -= 0.25f;
        return pos;
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
