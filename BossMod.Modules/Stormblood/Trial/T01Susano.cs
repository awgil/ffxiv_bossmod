namespace BossMod.Stormblood.Trial.T01Susano;

public enum OID : uint
{
    Boss = 0x1AF7,
    Helper = 0x233C,
    SusanoBig = 0x1AF8,
    AmaNoIwato = 0x1BA1, // R0.500
    AmaNoIwatoStone = 0x1C20, // R1.800
    AmeNoMurakumo = 0x1C84, // R8.000, Part
    DarkCloud = 0x1F53, // R3.000
    DarkLevin = 0x1B9B, // R1.000, lightning orbs
    BladesShadow = 0x1EA479, // R2.000, EventObj
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Assail = 8220, // Boss->player, no cast, single-target
    RasenKaikyoVisual = 8221, // Boss->self, 3.0s cast, single-target
    RasenKaikyo = 8222, // AmaNoIwato->self, 3.0s cast, range 6 circle
    YataNoKagami = 8223, // Boss->player, no cast, single-target
    Brightstorm = 8224, // Boss->players, no cast, range 6 circle
    SheerForce = 8225, // AmaNoIwato->self, no cast, range 40+R circle
    AmeNoMurakumoRaidwide = 8226, // SusanoBig->self, no cast, range 40+R circle
    Stormsplitter = 8227, // Boss->self/player, 5.0s cast, range 20+R width 4 rect
    Ukehi = 8230, // Boss->self, 4.0s cast, range 40+R circle
    Seasplitter = 8232, // AmaNoIwato->self, 3.0s cast, range 21+R width 40 rect
    Seasplitter2 = 8233, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    Seasplitter3 = 8234, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    Seasplitter4 = 8235, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    AmeNoMurakumoRect = 8588, // AmaNoIwato->self, 4.0s cast, range 40+R width 6 rect
    Shock = 8259, // DarkLevin->self, no cast, range 6 circle
    ThePartingClouds = 9631, // DarkCloud->self, 3.5s cast, range 50+R width 10 rect
    AmeNoMurakumoEnrage = 9506, // AmeNoMurakumo->self, 24.0s cast, single-target
}

public enum IconID : uint
{
    Stack = 62, // player
    Stormsplitter = 230, // player
}

class RasenKaikyo(BossModule module) : Components.StandardAOEs(module, AID.RasenKaikyo, 6);
class Ukehi(BossModule module) : Components.RaidwideCast(module, AID.Ukehi);
class Brightstorm(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.Brightstorm, 6, 8);
class ThePartingClouds(BossModule module) : Components.StandardAOEs(module, AID.ThePartingClouds, new AOEShapeRect(65, 5));
class AmeNoMurakumoRaidwide(BossModule module) : Components.RaidwideInstant(module, AID.AmeNoMurakumoRaidwide, 0);
class AmeNoMurakumoRect(BossModule module) : Components.StandardAOEs(module, AID.AmeNoMurakumoRect, new AOEShapeRect(65, 3));
class Shock(BossModule module) : Components.StandardAOEs(module, AID.Shock, 6);
class Seasplitter(BossModule module) : Components.StandardAOEs(module, AID.Seasplitter, new AOEShapeRect(41, 20));
class Stormsplitter(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.Stormsplitter, AID.Stormsplitter, new AOEShapeRect(40, 2));
class SheerForce(BossModule module) : Components.RaidwideInstant(module, AID.SheerForce, 0);
class SusanoAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SusanoBig, (uint)OID.AmaNoIwato, (uint)OID.AmaNoIwatoStone, (uint)OID.AmeNoMurakumo, (uint)OID.DarkLevin], 1);

class YataNoKagami(BossModule module) : Components.Knockback(module, AID.YataNoKagami)
{
    private readonly List<(WPos origin, DateTime activation)> _sources = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.YataNoKagami)
        {
            ++NumCasts;
            _sources.Add((caster.Position, WorldState.CurrentTime.AddSeconds(1)));
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;
        foreach (var s in _sources)
        {
            if (now <= s.activation)
                yield return new(s.origin, 20, s.activation);
        }
    }

    public override void Update() => _sources.RemoveAll(s => WorldState.CurrentTime > s.activation);
}

class DarkLevinOrbs(BossModule module) : BossComponent(module)
{
    // TODO: proper soak towers / bait radius once Dark Levin size is confirmed from replay
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.Enemies((uint)OID.DarkLevin).Any(z => !z.IsDead))
            hints.Add("Soak lightning orbs!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var z in Module.Enemies((uint)OID.DarkLevin).Where(z => !z.IsDead))
            Arena.AddCircle(z.Position, 1, ArenaColor.Danger);
    }
}

class T01SusanoStates : StateMachineBuilder
{
    public T01SusanoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RasenKaikyo>()
            .ActivateOnEnter<YataNoKagami>()
            .ActivateOnEnter<Ukehi>()
            .ActivateOnEnter<Brightstorm>()
            .ActivateOnEnter<ThePartingClouds>()
            .ActivateOnEnter<AmeNoMurakumoRaidwide>()
            .ActivateOnEnter<AmeNoMurakumoRect>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<DarkLevinOrbs>()
            .ActivateOnEnter<Seasplitter>()
            .ActivateOnEnter<Stormsplitter>()
            .ActivateOnEnter<SheerForce>()
            .ActivateOnEnter<SusanoAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 243, NameID = 6221)] // TODO: clear Incomplete after live/replay pass (Seasplitter timing, Levin soak)
public class T01Susano(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
