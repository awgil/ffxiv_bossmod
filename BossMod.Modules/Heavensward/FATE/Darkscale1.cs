namespace BossMod.Heavensward.FATE.Darkscale1;

public enum OID : uint
{
    Boss = 0x13E9, // R12.500, x1
    Helper = 0x233C,

    Vedrfolnir = 0x12F5,
}

public enum AID : uint
{
    EvilEye = 5041, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    AutoAttack = 870, // Boss->Vedrfolnir, no cast, single-target
    SheetOfIce = 5042, // Boss->location, 3.0s cast, range 5 circle
    SalivousSnap = 5040, // Boss->Vedrfolnir, no cast, single-target
    Roast = 5043, // Boss->self, no cast, range 30+R width 8 rect
    GraniteRain = 5045, // Boss->self, 3.5s cast, range 10+R circle
}

class EvilEye(BossModule module) : Components.GenericGaze(module, AID.EvilEye)
{
    readonly List<Actor> Casters = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        foreach (var c in Casters)
            if (actor.Position.InCircleCone(c.CastInfo!.LocXZ, 20.5f, c.CastInfo!.Rotation, 45.Degrees()))
                yield return new(c.CastInfo!.LocXZ, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

class SheetOfIce(BossModule module) : Components.StandardAOEs(module, AID.SheetOfIce, 5);
class GraniteRain(BossModule module) : Components.StandardAOEs(module, AID.GraniteRain, 22.5f);

class DragonBabysitter(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.Enemies(OID.Vedrfolnir).FirstOrDefault() is { } ved)
            hints.Add($"Vedrfolnir HP: {ved.PendingHPRatio * 100:f1}% (minimum 20)", ved.PendingHPRatio < 0.3f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.Vedrfolnir).FirstOrDefault() is { PendingHPRatio: < 0.25f } ved)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Benediction), ved, ActionQueue.Priority.Medium);
    }
}

class Darkscale1States : StateMachineBuilder
{
    public Darkscale1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<GraniteRain>()
            .ActivateOnEnter<DragonBabysitter>()
            .Raw.Update = () => Module.PrimaryActor is { IsTargetable: false, HPRatio: <= 0.01f } or { IsDestroyed: true };
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Fate, GroupID = 868, NameID = 3983)]
public class Darkscale1(ModuleInit init) : BossModule(init, new(-202.19f, -504.73f), new ArenaBoundsCircle(25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actors(Enemies(OID.Vedrfolnir), ArenaColor.Object);
    }
}

