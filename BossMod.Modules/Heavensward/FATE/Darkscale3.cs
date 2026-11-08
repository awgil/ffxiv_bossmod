namespace BossMod.Heavensward.FATE.Darkscale3;

public enum OID : uint
{
    Boss = 0x12BB, // R12.500
    DarkLevinCrystal = 0x1351, // R0.500-1.500, x0 (spawn during fight)

    Vedrfolnir = 0x12F5,
}

public enum AID : uint
{
    SheetOfIce = 5042, // Boss->location, 3.0s cast, range 5 circle
    AutoAttack = 870, // Boss->player/12F5, no cast, single-target
    EvilEye = 5041, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    SalivousSnap = 5040, // Boss->12F5, no cast, single-target
    GraniteRain = 5045, // Boss->self, 3.5s cast, range 10+R circle
    Roast = 5043, // Boss->self, no cast, range 30+R width 8 rect
    PlasmaRelease = 5044, // Boss->self, 2.0s cast, range 30 circle
}

public enum SID : uint
{
    Frostbite = 268, // Boss->player, extra=0x0
    Poison = 18, // Boss->12F5, extra=0x0
    VulnerabilityDown = 350, // none->Boss, extra=0x0
    Infirmity = 172, // 12F5->Boss, extra=0x0
    VulnerabilityUp = 202, // Boss->player/12F5, extra=0x1
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
class PlasmaRelease(BossModule module) : Components.RaidwideCast(module, AID.PlasmaRelease, "Raidwide + vulnerability up");
class DarkLevinCrystal(BossModule module) : Components.Adds(module, (uint)OID.DarkLevinCrystal);
class VulnerabilityDown(BossModule module) : Components.InvincibleStatus(module, (uint)SID.VulnerabilityDown, priority: -1);

class Darkscale3States : StateMachineBuilder
{
    public Darkscale3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<GraniteRain>()
            .ActivateOnEnter<PlasmaRelease>()
            .ActivateOnEnter<DarkLevinCrystal>()
            .ActivateOnEnter<VulnerabilityDown>()
            .ActivateOnEnter<Darkscale1.DragonBabysitter>()
            .Raw.Update = () => Module.PrimaryActor is { HPRatio: < 0.01f, IsTargetable: false } or { IsDestroyed: true };
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Fate, GroupID = 870, NameID = 3983)]
public class Darkscale3(ModuleInit init) : BossModule(init, new(-218.0157f, -684.3176f), new ArenaBoundsCircle(40))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actors(Enemies(OID.Vedrfolnir), ArenaColor.Object);
    }
}
