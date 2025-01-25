namespace BossMod.Stormblood.Dungeon.D08DrownedCityOfSkalla.D082TheOldOne;

public enum OID : uint
{
    Boss = 0x1FAC, // R4.600
    Helper = 0x233C,
    _Gen_UrolithSentinel = 0x1FBD, // R2.100, x1
    _Gen_TheOldOne = 0x18D6, // R0.500, x12, Helper type
    _Gen_Subservient = 0x1FAD, // R1.725, x0 (spawn during fight)
    _Gen_ = 0x204F, // R5.000, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 29791, // Boss->player, no cast, single-target
    _Weaponskill_MysticLight = 9815, // Boss->self, 4.0s cast, range 40+R 60-degree cone
    _Weaponskill_MysticFlame = 9816, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MysticFlame1 = 9817, // _Gen_TheOldOne->self, 3.5s cast, range 8 circle
    _Weaponskill_ShiftingLight = 9818, // Boss->self, 3.0s cast, range 20+R circle
    _Spell_Shatterstone = 9824, // _Gen_TheOldOne->self, 2.0s cast, range 5 circle - cast by helper when transfigured player drops a mine, does not hit players
    _Weaponskill_OrderToDetonate = 9819, // Boss->self, 20.0s cast, single-target
}

public enum SID : uint
{
    _Gen_Invincibility = 325, // none->_Gen_TheOldOne/Boss/_Gen_, extra=0x0
    _Gen_Transfiguration = 1448, // none->player, extra=0x4A
}

class MysticLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MysticLight), new AOEShapeCone(44.6f, 30.Degrees()));
class MysticFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MysticFlame1), new AOEShapeCircle(8));
class ShiftingLight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_ShiftingLight));

class Transfiguration(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> MineCasters = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(SID._Gen_Transfiguration) == null)
            return;

        var closestTarget = InterestingTargets().MinBy(t => (t.Position - actor.Position).Length());
        if (closestTarget == null)
            return;

        if ((closestTarget.Position - actor.Position).Length() < 5)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Shatterstone), actor, ActionQueue.Priority.High);
        else
            hints.ForcedMovement = actor.DirectionTo(closestTarget).ToVec3();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in MineCasters)
            Arena.ZoneCircle(c.Position, 5, ArenaColor.SafeFromAOE);
    }

    private IEnumerable<Actor> InterestingTargets()
    {
        var md = MineCasters.Count == 0
            ? _ => float.MaxValue
            : ShapeDistance.Union(MineCasters.Select(c => ShapeDistance.Circle(c.Position, 5)).ToList());

        return Module.Enemies(OID._Gen_Subservient).Where(x => !x.IsDead && md(x.Position) > 0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_Shatterstone)
            MineCasters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_Shatterstone)
            MineCasters.Remove(caster);
    }
}

class Subservient(BossModule module) : Components.Adds(module, (uint)OID._Gen_Subservient)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            h.Priority = (OID)h.Actor.OID switch
            {
                OID._Gen_Subservient => 1,
                _ => 0
            };
        }
    }
}

class TheOldOneStates : StateMachineBuilder
{
    public TheOldOneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Subservient>()
            .ActivateOnEnter<MysticFlame>()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<ShiftingLight>()
            .ActivateOnEnter<Transfiguration>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 279, NameID = 6908)]
public class TheOldOne(WorldState ws, Actor primary) : BossModule(ws, primary, new(115, 4), new ArenaBoundsCircle(20));

