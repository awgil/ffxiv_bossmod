namespace BossMod.RealmReborn.Alliance.A13Thanatos;

public enum OID : uint
{
    Boss = 0x92E, // R3.000, x1
    _Gen_Thanatos = 0x92F, // R0.500, x1
    _Gen_Nemesis = 0x930, // R2.000, x0 (spawn during fight)
    _Gen_Sandman = 0x983, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 1461, // Boss/930->player, no cast, single-target
    _Weaponskill_BlightedGloom = 759, // Boss->self, no cast, range 10+R circle
    _Weaponskill_BlackCloud = 758, // Boss->location, no cast, range 6 circle
    _Weaponskill_Cloudscourge = 760, // 92F->location, 3.0s cast, range 6 circle
    _Weaponskill_Knout = 763, // Boss->self, no cast, single-target
    _Spell_CrepusculeBlade = 762, // Boss->self, 3.0s cast, range 8+R ?-degree cone
    _Spell_VoidFireII = 1829, // 930->location, 3.0s cast, range 5 circle
}

public enum SID : uint
{
    _Gen_AstralRealignment = 398, // none->player, extra=0x0
    _Gen_Leaden = 67, // none->931, extra=0x50
}

class Cloudscourge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Cloudscourge, 6);
class VoidFireII(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidFireII, 5);
class CrepusculeBlade(BossModule module) : Components.StandardAOEs(module, AID._Spell_CrepusculeBlade, new AOEShapeCone(11, 60.Degrees()));
class CrepusculeInterrupt(BossModule module) : Components.CastInterruptHint(module, AID._Spell_CrepusculeBlade);

class Adds(BossModule module) : Components.AddsMulti(module, [OID._Gen_Nemesis, OID._Gen_Sandman]);

class AstralRealignment(BossModule module) : Components.GenericInvincible(module)
{
    private BitMask _playerStates;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        if (!_playerStates[slot])
            yield return Module.PrimaryActor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.SetPriority(Module.PrimaryActor, _playerStates[slot] ? 5 : AIHints.Enemy.PriorityInvincible);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_AstralRealignment && Raid.TryFindSlot(actor, out var slot))
            _playerStates.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_AstralRealignment && Raid.TryFindSlot(actor, out var slot))
            _playerStates.Clear(slot);
    }
}

class A13ThanatosStates : StateMachineBuilder
{
    public A13ThanatosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AstralRealignment>()
            .ActivateOnEnter<CrepusculeBlade>()
            .ActivateOnEnter<CrepusculeInterrupt>()
            .ActivateOnEnter<Cloudscourge>()
            .ActivateOnEnter<VoidFireII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 710, DevOnly = true)]
public class A13Thanatos(WorldState ws, Actor primary) : BossModule(ws, primary, new(440.4f, 280), ThanatosBounds)
{
    public static readonly ArenaBoundsCustom ThanatosBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var bounds = new PolygonClipper.Operand(CurveApprox.Circle(27, 1 / 90f));
        var rect0 = CurveApprox.Rect(new(10, 0), new(0, 8)).Select(r => r + new WDir(33.5f, 0));
        var rect1 = CurveApprox.Rect(new(10, 0), new(0, 8)).Select(r => r + new WDir(33.5f, 0)).Select(r => r.Rotate(120.Degrees()));
        var rect2 = CurveApprox.Rect(new(10, 0), new(0, 8)).Select(r => r + new WDir(33.5f, 0)).Select(r => r.Rotate(-120.Degrees()));

        var clipper = new PolygonClipper();

        return new(43, clipper.Union(new(clipper.Union(new(clipper.Union(bounds, new(rect0))), new(rect1))), new(rect2)), MapResolution: 1);
    }
}
