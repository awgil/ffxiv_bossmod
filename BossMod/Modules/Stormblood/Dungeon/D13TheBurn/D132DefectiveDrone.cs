namespace BossMod.Stormblood.Dungeon.D13TheBurn.D132DefectiveDrone;
public enum OID : uint
{
    Boss = 0x23AA, // R3.200, x1
    MiningDrone = 0x23AB, // R1.920, x0 (spawn during fight)
    RockBiter = 0x23AC, // R3.000, x0 (spawn during fight)
    RepurposedDreadnaught = 0x23AD, // R2.400, x0 (spawn during fight)
    SludgePuddle = 0x1EA9EF,
}

public enum AID : uint
{
    Attack = 872, // Boss/23AD->player, no cast, single-target
    AetherochemicalFlame = 11635, // Boss->self, 3.0s cast, range 40 circle
    AetherochemicalResidue = 11636, // Boss->location, no cast, range 5 circle
    AetherochemicalCoil = 11634, // Boss->player, 3.0s cast, single-target
    SelfDetonate = 11639, // 23AB->self, 3.0s cast, range 5 circle
    Throttle = 11638, // 23AB->location, 3.0s cast, width 3 rect charge
    FullThrottle = 11637, // Boss->location, 3.0s cast, width 5 rect charge
    AditDriver = 11640, // 23AC->self, 6.0s cast, range 30+R width 6 rect

    A = 13526, // Boss->self, no cast, single-target
    B = 13529, // 23AB->self, no cast, single-target
    C = 13525, // Boss->self, no cast, single-target
}
public enum SID : uint
{
    Burns = 284, // none->player, extra=0x0
    Sludge = 287, // none->player, extra=0x0
    Leaden = 67, // none->player, extra=0x4B
}
public enum IconID : uint
{
    SludgeIcon = 2, // player
    Charges = 158, // MiningDrone/Boss
    Tankbuster = 381, // player
}
class AetherochemicalFlame(BossModule module) : Components.RaidwideCast(module, AID.AetherochemicalFlame);
class AetherochemicalResidue(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), (uint)IconID.SludgeIcon, AID.AetherochemicalResidue, centerAtTarget: true);
class SludgeVoidZone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var i in _aoes)
                yield return i with { Color = ArenaColor.AOE };
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SludgePuddle)
        {
            _aoes.Add(new AOEInstance(new AOEShapeCircle(3), actor.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.SludgePuddle)
        {
            _aoes.Clear();
        }
    }
}
class AetherochemicalCoil(BossModule module) : Components.SingleTargetCast(module, AID.AetherochemicalCoil);
class Throttles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var i in _aoes)
                yield return i with { Color = ArenaColor.AOE };
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Throttle)
        {
            _aoes.Add(new AOEInstance(new AOEShapeRect(36, 1.5f, 5), caster.Position + 2 * caster.Rotation.ToDirection(), caster.Rotation));
        }
        if ((AID)spell.Action.ID is AID.FullThrottle)
        {
            _aoes.Add(new AOEInstance(new AOEShapeRect(36, 2.5f, 5), caster.Position + 2 * caster.Rotation.ToDirection(), caster.Rotation));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Throttle or AID.FullThrottle)
        {
            _aoes.Clear();
        }
    }
}
class SelfDetonate(BossModule module) : Components.StandardAOEs(module, AID.SelfDetonate, new AOEShapeCircle(5));
class AditDriver(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var i in _aoes)
                yield return i with { Color = ArenaColor.AOE };
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AditDriver)
        {
            _aoes.Add(new AOEInstance(new AOEShapeRect(33, 3f, 33), caster.Position + 2 * caster.Rotation.ToDirection(), caster.Rotation));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AditDriver)
        {
            _aoes.Clear();
        }
    }
}
class Firewalls(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _arenaVoidZones = [];
    private static readonly AOEShapeRect rect = new(10, 4, 10);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arenaVoidZones.Count > 0)
        {
            for (var i = 0; i < _arenaVoidZones.Count; i++)
            {
                yield return _arenaVoidZones[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _arenaVoidZones.Clear();
        int[] xOffsets = [-14, 14];
        int[] zOffsets = [-70];

        foreach (var zOffset in zOffsets)
        {
            foreach (var xOffset in xOffsets)
            {
                _arenaVoidZones.Add(new(rect, new(xOffset, zOffset)));
            }
        }
        base.DrawArenaBackground(pcSlot, pc);
    }
}
class Adds(BossModule module) : Components.Adds(module, (uint)OID.RepurposedDreadnaught)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.RepurposedDreadnaught => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class D132DefectiveDroneStates : StateMachineBuilder
{
    public D132DefectiveDroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalFlame>()
            .ActivateOnEnter<AetherochemicalResidue>()
            .ActivateOnEnter<AetherochemicalCoil>()
            .ActivateOnEnter<Throttles>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<AditDriver>()
            .ActivateOnEnter<SludgeVoidZone>()
            .ActivateOnEnter<Firewalls>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7669)]
public class D132DefectiveDrone(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -70f), new ArenaBoundsRect(14.6f, 9.5f));
