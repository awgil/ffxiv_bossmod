namespace BossMod.Endwalker.Quest.AnUnforeseenBargain.P2Andromalius;

public enum OID : uint
{
    Boss = 0x3D76, // R6.000, x1
    Helper = 0x233C, // R0.500, x13, Helper type
    AlphiShield = 0x1EB87A,
}

public enum AID : uint
{
    StraightSpindleFast = 31808, // 3D78->self, 5.0s cast, range 50+R width 5 rect
    Dark = 31815, // Helper->location, 5.0s cast, range 10 circle
    StraightSpindleSlow = 31809, // 3D78->self, 9.0s cast, range 50+R width 5 rect
    EvilMist = 31825, // Boss->self, 5.0s cast, range 60 circle
    Explosion = 33010, // Helper->self, 10.0s cast, range 5 circle
    Hellsnap = 31816, // Boss->3D80, 5.0s cast, range 6 circle
    Decay = 32857, // _Gen_VisitantVoidskipper->self, 13.0s cast, range 60 circle
    StraightSpindleAdds = 33174, // 3D77->self, 8.0s cast, range 50+R width 5 rect
    Voidblood = 33172, // 3EE4->location, 9.0s cast, range 6 circle
    VoidSlash = 33173, // 3EE5->self, 11.0s cast, range 8+R 90-degree cone
}

class StraightSpindleAdds(BossModule module) : Components.StandardAOEs(module, AID.StraightSpindleAdds, new AOEShapeRect(50, 2.5f));
class Voidblood(BossModule module) : Components.StandardAOEs(module, AID.Voidblood, 6);
class VoidSlash(BossModule module) : Components.StandardAOEs(module, AID.VoidSlash, new AOEShapeCone(9.7f, 45.Degrees()));
class EvilMist(BossModule module) : Components.RaidwideCast(module, AID.EvilMist);
class Explosion(BossModule module) : Components.CastTowers(module, AID.Explosion, 5);
class Dark(BossModule module) : Components.StandardAOEs(module, AID.Dark, 10);
class Hellsnap(BossModule module) : Components.StackWithCastTargets(module, AID.Hellsnap, 6);

class StraightSpindle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(50, 2.5f), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo))).OrderBy(x => x.Activation).Take(3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StraightSpindleFast or AID.StraightSpindleSlow)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StraightSpindleFast or AID.StraightSpindleSlow)
            Casters.Remove(caster);
    }
}
class Decay(BossModule module) : Components.CastHint(module, AID.Decay, "Kill wasp before enrage!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}
class ShieldHint(BossModule module) : BossComponent(module)
{
    private Actor? Shield;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AlphiShield)
            Shield = actor;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000C && param1 == 0x46)
            Shield = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield is Actor s)
            hints.GoalZones.Add(hints.GoalSingleTarget(s.Position, 5));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield is Actor s && !actor.Position.InCircle(s.Position, 5))
            hints.Add("Take cover!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield is Actor s)
            Arena.ZoneCircle(s.Position, 5, ArenaColor.SafeFromAOE);
    }
}

class ProtectZero(BossModule module) : BossComponent(module)
{
    private Actor? CastingZero => Raid.WithoutSlot().FirstOrDefault(x => x.OID == 0x3D80 && x.FindStatus(2056) != null);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CastingZero is Actor z)
        {
            foreach (var h in hints.PotentialTargets)
                if (h.Actor.TargetID == z.InstanceID)
                    h.Priority = 5;
        }
    }
}

class AndromaliusStates : StateMachineBuilder
{
    public AndromaliusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<EvilMist>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Hellsnap>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<ShieldHint>()
            .ActivateOnEnter<StraightSpindleAdds>()
            .ActivateOnEnter<Voidblood>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<ProtectZero>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209, NameID = 12071)]
public class Andromalius(WorldState ws, Actor primary) : BossModule(ws, primary, new(97.85f, 286), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
