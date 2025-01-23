namespace BossMod.Heavensward.Quest.DragoonsFate;

public enum OID : uint
{
    Boss = 0x10B9, // R7.000, x1
    _Gen_Icicle = 0x10BC, // R2.500, x0 (spawn during fight)
    _Gen_Graoully = 0x10BA, // R7.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player/10BB, no cast, single-target
    __ = 4258, // Boss->self, no cast, single-target
    _Weaponskill_PillarImpact = 3095, // 10BC->self, 3.0s cast, range 4+R circle
    _Weaponskill_PillarPierce = 4259, // 10BC->self, 2.0s cast, range 80+R width 4 rect
    _Weaponskill_Cauterize = 4260, // 10BA->self, 3.0s cast, range 48+R width 20 rect
    _Weaponskill_Touchdown = 4998, // Boss->self, no cast, range 7 circle
    _Weaponskill_FrostBreath = 4252, // Boss->self, no cast, range 20+R ?-degree cone
    _Weaponskill_SheetOfIce = 4261, // Boss->location, 2.5s cast, range 5 circle
}

public enum IconID : uint
{
    _Gen_Icon_22 = 22, // player/10BB
}

public enum SID : uint
{
    _Gen_Prey = 904, // none->player/10BB, extra=0x0
    _Gen_SlipperyPrey = 475, // none->player/10BB, extra=0x0
    _Gen_ThinIce = 905, // Boss->player/10BB, extra=0x1/0x2/0x3
    _Gen_DeepFreeze = 3479, // Boss->10BB/player, extra=0x1
}

class SheetOfIce(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SheetOfIce), 5);
class PillarImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_PillarImpact), new AOEShapeCircle(6.5f));
class PillarPierce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_PillarPierce), new AOEShapeRect(82.5f, 2));
class Cauterize(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Cauterize), new AOEShapeRect(55, 10));

class Prey(BossModule module) : BossComponent(module)
{
    private static readonly AOEShape Cleave = new AOEShapeCone(27, 65.Degrees());
    private int IceStacks(Actor actor) => actor.FindStatus(SID._Gen_ThinIce) is ActorStatus st ? st.Extra & 0xFF : 0;

    private Actor? PreyCur;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Prey)
            PreyCur = actor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PreyCur is not Actor prey)
            return;

        var partner = WorldState.Party[slot == 0 ? PartyState.MaxAllianceSize : slot]!;

        // force debuff swap
        if (IceStacks(prey) == 3)
            hints.GoalZones.Add(p => p.InCircle(partner.Position, 2) ? 1 : 0);
        else
        {
            // prevent premature swap, even though it doesn't really matter, because the debuff generally falls off with plenty of time left
            hints.AddForbiddenZone(ShapeDistance.Circle(partner.Position, 5), WorldState.FutureTime(1));

            if (Module.PrimaryActor.IsTargetable)
                hints.AddForbiddenZone(Cleave.Distance(Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(partner)), WorldState.FutureTime(1));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        // sometimes partner loses prey status *after* we get it
        if (status.ID == (uint)SID._Gen_Prey && actor == PreyCur)
            PreyCur = null;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (PreyCur is Actor p && Module.PrimaryActor is var primary && primary.IsTargetable)
            Cleave.Outline(Arena, primary.Position, primary.AngleTo(p), ArenaColor.Danger);
    }
}

class GraoullyStates : StateMachineBuilder
{
    public GraoullyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Prey>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<SheetOfIce>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 415, NameID = 4190)]
public class Graoully(WorldState ws, Actor primary) : BossModule(ws, primary, BCenter, BBounds)
{
    public static readonly WPos BCenter = new(-515.285f, -304.69f);
    private static readonly WPos[] Corners = [new(-483.91f, -299.22f), new(-519.70f, -272.85f), new(-546.66f, -309.50f), new(-510.38f, -336.53f)];
    public static readonly ArenaBoundsCustom BBounds = new(32, new(Corners.Select(c => c - BCenter)));
}
