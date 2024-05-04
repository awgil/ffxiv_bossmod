namespace BossMod.RealmReborn.Dungeon.D09Cutter.D093Chimera;

public enum OID : uint
{
    Boss = 0x64C, // x1
    Cacophony = 0x64D, // spawn during fight
    RamsKeeper = 0x1E8713, // EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    LionsBreath = 1101, // Boss->self, no cast, range 9.7 ?-degree cleave
    RamsBreath = 1102, // Boss->self, 2.0s cast, range 9.7 120-degree cone, -45 degree offset
    DragonsBreath = 1103, // Boss->self, 2.0s cast, range 9.7 120-degree cone, +45 degree offset
    RamsVoice = 1104, // Boss->self, 3.0s cast, range 9.7 aoe
    DragonsVoice = 1442, // Boss->self, 4.5s cast, range 7-30 donut aoe
    RamsKeeper = 1106, // Boss->location, 3.0s cast, range 6 voidzone
    Cacophony = 1107, // Boss->self, no cast, visual, summons orb
    ChaoticChorus = 1108, // Cacophony->self, no cast, range 6 aoe
}

class LionsBreath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.LionsBreath), new AOEShapeCone(9.7f, 60.Degrees())); // TODO: verify angle
class RamsBreath(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.RamsBreath), new AOEShapeCone(9.7f, 60.Degrees(), -45.Degrees()));
class DragonsBreath(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.DragonsBreath), new AOEShapeCone(9.7f, 60.Degrees(), 45.Degrees()));
class RamsVoice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RamsVoice), new AOEShapeCircle(9.7f));
class DragonsVoice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DragonsVoice), new AOEShapeDonut(7, 30));
class RamsKeeper(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RamsKeeper), 6);
class RamsKeeperVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.RamsKeeper));

class ChaoticChorus(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChaoticChorus))
{
    private readonly AOEShape _shape = new AOEShapeCircle(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: timings
        return Module.Enemies(OID.Cacophony).Where(c => !c.IsDead).Select(c => new AOEInstance(_shape, c.Position, c.Rotation));
    }
}

class D093ChimeraStates : StateMachineBuilder
{
    public D093ChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LionsBreath>()
            .ActivateOnEnter<RamsBreath>()
            .ActivateOnEnter<DragonsBreath>()
            .ActivateOnEnter<RamsVoice>()
            .ActivateOnEnter<DragonsVoice>()
            .ActivateOnEnter<RamsKeeper>()
            .ActivateOnEnter<RamsKeeperVoidzone>()
            .ActivateOnEnter<ChaoticChorus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12, NameID = 1590)]
public class D093Chimera(WorldState ws, Actor primary) : BossModule(ws, primary, new(-170, -200), new ArenaBoundsCircle(30));
