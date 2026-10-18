namespace BossMod.Stormblood.Raid.O6NChadarnook;

public enum OID : uint
{
    Boss = 0x21D1, // R3.800, Demon Chadarnook
    Goddess = 0x21D0, // R3.600, untargetable portrait
    Haunt = 0x21D2, // R3.800, clone
    Helper = 0x18D6, // R0.500, portrayals and casts
    Easterly = 0x210D, // R1.000, wind orbs
    Canvas = 0x1EA1A1,
}

public enum AID : uint
{
    AutoAttack = 10237, // Boss->player, no cast, single-target
    GoddessAuto = 10239, // Goddess->player, no cast, single-target
    DemonicHowl = 10284, // Boss->self, 3.0s cast, range 60 circle
    DemonicShear = 10282, // Boss->player, 3.0s cast, single-target
    Possession = 10243, // Boss->location, 10.0s cast, single-target
    HauntPossession = 10245, // Haunt->location, 10.0s cast, single-target
    Release = 10244, // Boss->location, 3.0s cast, single-target
    HauntRelease = 10249, // Haunt->location, 3.0s cast, single-target
    FlashFire = 10251, // Helper->self, no cast, range 100 circle
    Earthquake = 10257, // Helper->self, no cast, range 60 circle
    DemonicStone = 10311, // Helper->location, 5.0s cast, range 3 circle
    DemonicWave = 10287, // Boss->self, 5.0s cast, single-target
    DemonicWaveHit = 10289, // Helper->self, no cast, range 60 circle, knockback
    DemonicSpout1 = 10293, // Helper->location, 3.0s cast, range 4 circle
    DemonicSpout2 = 10295, // Helper->location, 3.0s cast, range 4 circle
    Featherlance = 10984, // Easterly->self, 1.0s cast, range 8 circle
    DemonicPain = 10987, // Boss->self, 6.0s cast, single-target, proximity tether
    FlashGale = 10306, // Helper->player, no cast, range 6 circle
    Downpour = 10269, // Waterfall->player, no cast, single-target
    GoddessPortrait = 10944, // Goddess->self, no cast, single-target
    PossessionVisual = 10242, // Boss->self, no cast, single-target
    DemonicStoneVisual = 10310, // Boss->self, no cast, single-target
    DemonicStoneRepeat = 10312, // Helper->location, no cast, range 3 circle
    DemonicSpoutVisual = 10291, // Boss->self, no cast, single-target
    DemonicTyphoon = 10301, // Boss->self, no cast, single-target
    DullPain = 10982, // Helper->player, no cast, range 2 circle
    FlashGaleVisual = 10305, // Boss->self, no cast, single-target
    HauntVisual = 11068, // Haunt->self, no cast, single-target
    Materialize = 10285, // Boss->self, 5.0s cast, single-target
    HauntWeaponskill = 10939, // Haunt->self, no cast, single-target
}

public enum IconID : uint
{
    FlashGale = 62, // stack, resolves about 6.3s later
}

enum Element { Fire, Earth, Water, Wind }

class DemonicHowl(BossModule module) : Components.RaidwideCast(module, AID.DemonicHowl);
class DemonicShear(BossModule module) : Components.SingleTargetCast(module, AID.DemonicShear);
class DemonicStone(BossModule module) : Components.StandardAOEs(module, AID.DemonicStone, 3);
class DemonicSpout(BossModule module) : Components.StandardAOEs(module, AID.DemonicSpout1, 4);
class DemonicSpoutSecond(BossModule module) : Components.StandardAOEs(module, AID.DemonicSpout2, 4);
class FlashGale(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FlashGale, AID.FlashGale, 6, 6.3f);

class DemonicPain(BossModule module) : Components.CastHint(module, AID.DemonicPain, "Away from boss")
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.DemonicPain) != true)
            return;
        var boss = Module.PrimaryActor.Position;
        hints.GoalZones.Add(p => (p - boss).LengthSq() > 225 ? 5 : 0); // 15 yalms
    }
}

class Easterlies(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(8);
    private readonly Dictionary<ulong, DateTime> _activation = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (id, activation) in _activation)
        {
            var orb = WorldState.Actors.Find(id);
            if (orb != null)
                yield return new(Shape, orb.Position, Activation: activation, Risky: activation < WorldState.FutureTime(4));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Easterly)
            _activation[actor.InstanceID] = WorldState.FutureTime(14.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.Featherlance))
            _activation.Remove(caster.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor) => _activation.Remove(actor.InstanceID);

}

class Paintings(BossModule module) : BossComponent(module)
{
    private const uint EarthMount = 157;
    private const uint WindMount = 155;

    // always do fire/water before earth/wind since you can't paint while mounted
    private static readonly (Element Element, WPos Painting, WPos Canvas)[] Spots =
    [
        (Element.Fire, new(16, 8.5f), new(7.5f, 7)),
        (Element.Water, new(-16, -8.5f), new(-11.4f, -3.7f)),
        (Element.Earth, new(16, -8.5f), new(8.5f, -9.7f)),
        (Element.Wind, new(-16, 8.5f), new(-7.5f, 5)),
    ];

    private readonly List<Element> _needed = [];
    private readonly HashSet<Element> _brushed = [];
    private readonly Dictionary<ulong, WPos> _orbSpawn = [];
    private bool _quakeDone;

    private static Element? FromPos(WPos pos)
    {
        var best = float.MaxValue;
        Element? element = null;
        foreach (var spot in Spots)
        {
            var dist = (spot.Painting - pos).LengthSq();
            if (dist < best)
            {
                best = dist;
                element = spot.Element;
            }
        }
        return best < 25 ? element : null; // 5 yalms
    }

    private void Need(WPos pos)
    {
        if (FromPos(pos) is not { } element || _needed.Contains(element))
            return;
        _needed.Add(element);
        if (element == Element.Earth)
            _quakeDone = false;
    }

    private void Done(Element element)
    {
        _needed.Remove(element);
        _brushed.Remove(element);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.Possession) || spell.IsSpell(AID.HauntPossession))
            Need(spell.LocXZ);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.FlashFire))
            Done(Element.Fire);
        else if (spell.IsSpell(AID.Earthquake))
        {
            _quakeDone = true;
            Done(Element.Earth);
        }
        else if (spell.IsSpell(AID.DemonicWaveHit))
            Done(Element.Water);
        else if (spell.IsSpell(AID.Featherlance))
            Done(Element.Wind);
        else if (spell.IsSpell(AID.Release) || spell.IsSpell(AID.HauntRelease))
        {
            if (FromPos(caster.Position) is { } element)
                Done(element);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Easterly)
            _orbSpawn[actor.InstanceID] = actor.Position;
    }

    public override void OnActorDestroyed(Actor actor) => _orbSpawn.Remove(actor.InstanceID);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.Canvas)
            return;
        foreach (var spot in Spots)
            if ((actor.Position - spot.Canvas).LengthSq() < 9)
                _brushed.Add(spot.Element);
    }

    private bool AllyOnWindMount(Actor player) => WorldState.Party.WithoutSlot().Any(p => p != player && p.MountId == WindMount);

    private bool Open(Element element, Actor player)
    {
        if (!_needed.Contains(element) || _brushed.Contains(element))
            return false;
        // only need one person to be on typhoon 
        return element != Element.Wind || !AllyOnWindMount(player);
    }

    private WPos? NextCanvas(Actor player)
    {
        foreach (var spot in Spots)
            if (Open(spot.Element, player))
                return spot.Canvas;
        return null;
    }

    private IEnumerable<Actor> Orbs() => Module.Enemies(OID.Easterly);

    private bool Blown(Actor orb) => _orbSpawn.TryGetValue(orb.InstanceID, out var spawn) && (orb.Position - spawn).LengthSq() > 36;

    private WPos? SafePocket(WPos from)
    {
        if (!Orbs().Any(Blown))
            return null;
        WPos? best = null;
        var bestDist = float.MaxValue;
        for (var x = -12; x <= 12; ++x)
        {
            for (var z = -12; z <= 12; ++z)
            {
                var p = new WPos(x, z);
                if (Orbs().Any(o => o.Position.InCircle(p, 8.5f)))
                    continue;
                var dist = (p - from).LengthSq();
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = p;
                }
            }
        }
        return best;
    }

    private void Goal(AIHints hints, WPos pos, float radius) => hints.GoalZones.Add(p => p.InCircle(pos, radius) ? 10 : 0);

    private void WindMounted(Actor actor, AIHints hints)
    {
        if (SafePocket(actor.Position) is { } pocket)
        {
            hints.WantDismount = true;
            Goal(hints, pocket, 1.5f);
            return;
        }

        var orb = Orbs().Where(o => !Blown(o)).MinBy(o => (o.Position - actor.Position).LengthSq());
        if (orb == null)
            return;
        var towardCenter = new WPos(0, 0) - orb.Position;
        var stand = towardCenter.LengthSq() < 1 ? orb.Position + new WDir(0, -4) : orb.Position + 3 * towardCenter.Normalized();
        Goal(hints, stand, 1.5f);
        // face cyclone from the arena side so it blows out
        var inside = towardCenter.Dot(actor.Position - orb.Position) > 0;
        if (inside && (actor.Position - orb.Position).LengthSq() < 64)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Snort), actor, ActionQueue.Priority.VeryHigh, facingAngle: actor.AngleTo(orb), forced: true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.MountId == EarthMount && _quakeDone)
            hints.WantDismount = true;

        if (actor.MountId == WindMount)
        {
            WindMounted(actor, hints);
            return;
        }

        // wave's shadow is handled by BoulderShadow, so stay there instead of chasing the other canvas
        if (_needed.Contains(Element.Water) && _brushed.Contains(Element.Water))
            return;

        if (NextCanvas(actor) is { } canvas)
        {
            Goal(hints, canvas, 1.5f);
            if ((actor.Position - canvas).LengthSq() <= 25)
            {
                var paint = ActionID.MakeSpell(ClassShared.AID.BlackPaint);
                var duty = WorldState.Client.DutyActions.FirstOrDefault(a => a.Action.ID == paint.ID);
                hints.ActionsToExecute.Push(duty.Action != default ? duty.Action : paint, actor, ActionQueue.Priority.VeryHigh, forced: true);
            }
            return;
        }

        if (AllyOnWindMount(actor) && _needed.Contains(Element.Wind) && SafePocket(actor.Position) is { } pocket)
            Goal(hints, pocket, 1.5f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.MountId == EarthMount && _quakeDone)
            hints.Add("Dismount");
        else if (actor.MountId == WindMount)
            hints.Add(SafePocket(actor.Position) != null ? "Dismount" : "Snort");
        else if (NextCanvas(actor) is { } canvas)
            hints.Add((actor.Position - canvas).LengthSq() < 9 ? "Use the brush" : "Canvas");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var spot in Spots)
            if (Open(spot.Element, pc))
                Arena.AddCircle(spot.Canvas, 1.5f, ArenaColor.Safe);

        if ((pc.MountId == WindMount || AllyOnWindMount(pc) && _needed.Contains(Element.Wind)) && SafePocket(pc.Position) is { } pocket)
        {
            Arena.ZoneCircle(pocket, 1.5f, ArenaColor.SafeFromAOE);
            Arena.AddCircle(pocket, 1.5f, ArenaColor.Safe);
        }
    }
}

class BoulderShadow(BossModule module) : Components.GenericLineOfSightAOE(module, AID.DemonicWave, 60, true)
{
    private static readonly WPos Painting = new(-16, -8.5f);
    private static readonly WPos Boulder = new(-11.37f, -3.68f);
    private const float Radius = 1.0f;
    private bool _water;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((spell.IsSpell(AID.Possession) || spell.IsSpell(AID.HauntPossession)) && (spell.LocXZ - Painting).LengthSq() < 25)
            _water = true;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (_water && actor.OID == (uint)OID.Canvas && (actor.Position - Boulder).LengthSq() < 9)
            Modify(Painting, [(Boulder, Radius)]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.DemonicWaveHit) || spell.IsSpell(AID.Release) && (caster.Position - Painting).LengthSq() < 25 || spell.IsSpell(AID.HauntRelease) && (caster.Position - Painting).LengthSq() < 25)
        {
            _water = false;
            Modify(null, []);
        }
    }
}

class O6NChadarnookStates : StateMachineBuilder
{
    public O6NChadarnookStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DemonicHowl>()
            .ActivateOnEnter<DemonicShear>()
            .ActivateOnEnter<DemonicStone>()
            .ActivateOnEnter<DemonicSpout>()
            .ActivateOnEnter<DemonicSpoutSecond>()
            .ActivateOnEnter<FlashGale>()
            .ActivateOnEnter<DemonicPain>()
            .ActivateOnEnter<Easterlies>()
            .ActivateOnEnter<Paintings>()
            .ActivateOnEnter<BoulderShadow>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 287, NameID = 7097)]
public class O6NChadarnook(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsRect(23, 15));
