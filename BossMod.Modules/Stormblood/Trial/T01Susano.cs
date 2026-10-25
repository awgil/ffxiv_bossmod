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
    DarkLevin = 0x1B9B, // R1.000
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
    UkehiFollowUp = 8231, // Boss->self, no cast, range 40+R circle, extra raidwide after some casts
    Seasplitter = 8232, // AmaNoIwato->self, 3.0s cast, range 21+R width 40 rect
    Seasplitter2 = 8233, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    Seasplitter3 = 8234, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    Seasplitter4 = 8235, // AmaNoIwato->self, no cast, range 7+R width 40 rect
    AmeNoMurakumoRect = 8588, // AmaNoIwato->self, 4.0s cast, range 40+R width 6 rect
    Shock = 8259, // DarkLevin->self, no cast, range 6 circle
    ThePartingClouds = 9631, // DarkCloud->self, 3.5s cast, range 50+R width 10 rect
    AmeNoMurakumoEnrage = 9506, // AmeNoMurakumo->self, 24.0s cast, single-target
}

public enum SID : uint
{
    Fetters = 292, // none->player, trapped in an Ama-no-iwato stone
}

public enum IconID : uint
{
    YataNoKagami = 23, // player
    Stack = 62, // player
    Stormsplitter = 230, // player
}

class RasenKaikyo(BossModule module) : Components.StandardAOEs(module, AID.RasenKaikyo, 6);
class Ukehi(BossModule module) : Components.RaidwideCast(module, AID.Ukehi);
class Brightstorm(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.Brightstorm, 6, 8);
class ThePartingClouds(BossModule module) : Components.StandardAOEs(module, AID.ThePartingClouds, new AOEShapeRect(53, 5));
class AmeNoMurakumoRaidwide(BossModule module) : Components.RaidwideInstant(module, AID.AmeNoMurakumoRaidwide, 0);
class AmeNoMurakumoRect(BossModule module) : Components.StandardAOEs(module, AID.AmeNoMurakumoRect, new AOEShapeRect(40.5f, 3));
class Shock(BossModule module) : Components.StandardAOEs(module, AID.Shock, 6);
class Seasplitter(BossModule module) : Components.GenericAOEs(module, AID.Seasplitter)
{
    private readonly List<AOEInstance> _aoes = [];

    private const float SafeStrip = 0.6f;
    private static readonly AOEShapeRect _shape = new(21.5f - SafeStrip, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Add(new(_shape, caster.Position + SafeStrip * spell.Rotation.ToDirection(), spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Seasplitter4)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
        }
    }
}

class Stormsplitter(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(23.5f, 2), (uint)IconID.Stormsplitter, AID.Stormsplitter, 5.1f, damageType: AIHints.PredictedDamageType.Tankbuster);
class SheerForce(BossModule module) : Components.RaidwideInstant(module, AID.SheerForce, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AmeNoMurakumoEnrage)
            Activation = WorldState.FutureTime(Delay);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction && NumCasts % 2 == 1)
            Activation = WorldState.FutureTime(5);
    }
}
class SusanoAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SusanoBig, (uint)OID.AmaNoIwato, (uint)OID.AmaNoIwatoStone, (uint)OID.AmeNoMurakumo, (uint)OID.DarkLevin], 1);

class SealedGate(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (TrappedStones().Any())
            hints.Add("Break the stone with the trapped player!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var trapped = TrappedStones().ToList();
        foreach (var stone in Module.Enemies(OID.AmaNoIwatoStone))
            if (hints.FindEnemy(stone) is { } e)
                e.Priority = trapped.Contains(stone) ? 2 : AIHints.Enemy.PriorityInvincible;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var stone in TrappedStones())
            Arena.AddCircle(stone.Position, 2, ArenaColor.Danger);
    }

    private IEnumerable<Actor> TrappedStones() => Module.Enemies(OID.AmaNoIwatoStone).Where(s => !s.IsDead && Raid.WithoutSlot().Any(p => p.FindStatus(SID.Fetters) != null && p.Position.InCircle(s.Position, 2.5f)));
}

class YataNoKagami(BossModule module) : Components.Knockback(module, AID.YataNoKagami, stopAtWall: true)
{
    private Actor? _target;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (actor == _target)
            yield return new(Module.PrimaryActor.Position, 20, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.YataNoKagami)
        {
            _target = actor;
            _activation = WorldState.FutureTime(5.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _target = null;
        }
    }
}

class DarkLevinOrbs(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Orbs().Any())
            hints.Add("Pop lightning orbs, one player each");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var z in Orbs())
            Arena.AddCircle(z.Position, 6, ArenaColor.Danger);
    }

    private IEnumerable<Actor> Orbs() => Module.Enemies(OID.DarkLevin).Where(z => !z.IsDead);
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
            .ActivateOnEnter<SusanoAdds>()
            .ActivateOnEnter<SealedGate>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 243, NameID = 6221)]
public class T01Susano(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
