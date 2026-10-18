namespace BossMod.Stormblood.Raid.O5NPhantomTrain;

public enum OID : uint
{
    Boss = 0x213E, // R15.750, the train south of the cart
    Helper = 0x18D6, // R0.500
    WrothGhost = 0x21AB, // R1.800, the squares
    Ghost = 0x213F, // R1.800, chasers
    PassengerGhost = 0x2148, // R1.000, the ghost inside a passenger car
    PassengerGhost2 = 0x21E8, // R1.000
    Siegfried = 0x214D, // R0.500, x0 (spawn during fight), special ghost inside a passenger car
    PutridPassenger = 0x2140, // R0.500
    DoomChimney = 0x2174, // R3.850, on the train roof
    Phantom = 0x214F, // R1.000
    CartJump = 0x1EA940, // R2.000
    RoofJump = 0x1EA941, // R2.000
}

public enum AID : uint
{
    AutoAttack = 10402, // Boss->player, no cast, single-target
    Encumber = 10414, // WrothGhost->self, 4.0s cast, 8x10 rect
    HeadOn = 10404, // Boss->self, 7.0s cast, single-target
    HeadOnAOE = 10415, // Helper->location, 7.8s cast, range 50 circle
    SaintlyBeam = 10410, // Helper->self, no cast, range 6 circle
    SaintlyBeamVisual = 10431, // Boss->self, no cast, single-target
    DiabolicHeadlamp = 10406, // Boss->self, 5.0s cast, single-target
    DiabolicHeadlampAOE = 10416, // Helper->self, no cast, range 50 width 6 rect
    DiabolicWhistle = 10405, // Boss->self, 1.5s cast, single-target
    Possess = 10412, // Ghost->self, no cast, range 3 circle
    DiabolicWind = 10425, // Helper->location, no cast, range 5 circle
    DiabolicLight = 10426, // Helper->location, 5.0s cast, range 50 circle, proximity
    AcidRain = 10427, // Boss->self, 3.0s cast, single-target
    AcidRainAOE = 10411, // Helper->location, no cast, range 50 circle
    DoomStrike = 10403, // Boss->player, 3.5s cast, single-target, tankbuster
    DiabolicChimney = 10409, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    Prey = 1253, // drops Diabolic Light where you are standing
    Throttle = 938, // get caught by a ghost on purpose
}

public enum IconID : uint
{
    SaintlyBeam = 1, // player or wroth ghost, spotlight resolves 11.1s later
    DiabolicWind = 70, // player
}

class Encumber(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(4, 5, 4);
    private readonly Dictionary<ulong, DateTime> _telegraph = [];
    private readonly HashSet<ulong> _solid = [];

    public IEnumerable<Actor> ActiveBlocks => Module.Enemies(OID.WrothGhost).Where(Visible);

    private bool Visible(Actor ghost) => !ghost.IsDead && (_telegraph.ContainsKey(ghost.InstanceID) || _solid.Contains(ghost.InstanceID)) && Module.FindComponent<SaintlyBeam>()?.Cleared(ghost) != true;

    private bool Solid(Actor ghost) => _solid.Contains(ghost.InstanceID) || _telegraph.TryGetValue(ghost.InstanceID, out var at) && WorldState.CurrentTime >= at;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var ghost in Module.Enemies(OID.WrothGhost))
        {
            if (!Visible(ghost))
                continue;
            var solid = Solid(ghost);
            var activation = _telegraph.GetValueOrDefault(ghost.InstanceID);
            yield return new(Shape, ghost.Position, Activation: solid ? default : activation, Risky: !solid);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var ghost in Module.Enemies(OID.WrothGhost))
            if (Visible(ghost) && Solid(ghost))
                hints.TemporaryObstacles.Add(ShapeDistance.Rect(ghost.Position, new Angle(), 4, 4, 5));
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.WrothGhost && modelState == 4)
            _telegraph.TryAdd(actor.InstanceID, WorldState.FutureTime(4.2f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.Encumber))
            _telegraph[caster.InstanceID] = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!spell.IsSpell(AID.Encumber))
            return;
        _telegraph.Remove(caster.InstanceID);
        _solid.Add(caster.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _telegraph.Remove(actor.InstanceID);
        _solid.Remove(actor.InstanceID);
    }

    public override void Update()
    {
        var beams = Module.FindComponent<SaintlyBeam>();
        foreach (var ghost in Module.Enemies(OID.WrothGhost))
        {
            if (ghost.IsDead || beams?.Cleared(ghost) == true)
            {
                _telegraph.Remove(ghost.InstanceID);
                _solid.Remove(ghost.InstanceID);
                continue;
            }
            if (ghost.ModelState.ModelState == 4 && !_solid.Contains(ghost.InstanceID))
                _telegraph.TryAdd(ghost.InstanceID, WorldState.FutureTime(4.2f));
        }
    }
}

class HeadOn(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var helper in Module.Enemies(OID.Helper))
            if (helper.CastInfo is { } cast && cast.IsSpell(AID.HeadOnAOE))
                yield return new(Shape, cast.LocXZ, Activation: Module.CastFinishAt(cast));
    }
}

class SaintlyBeam(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(6);
    private readonly List<Beam> _beams = [];
    private readonly HashSet<ulong> _cleared = [];

    public bool Cleared(Actor ghost) => _cleared.Contains(ghost.InstanceID);

    // don't try and target wroths when you're tethered, the tethered is the target
    private Actor? TetheredGhost(Actor player) => Module.Enemies(OID.Ghost).FirstOrDefault(g => !g.IsDead && g.Tether.ID != 0 && g.Tether.Target == player.InstanceID);

    // try to prio hitting as many wroths (2) as possible
    public WPos? BaitSpot(Actor player)
    {
        if (TetheredGhost(player) != null)
            return null;
        if (!_beams.Any(b => b.Target.InstanceID == player.InstanceID && b.Locked == null))
            return null;

        var ghosts = (Module.FindComponent<Encumber>()?.ActiveBlocks ?? Module.Enemies(OID.WrothGhost))
            .Where(g => !g.IsDead && !_cleared.Contains(g.InstanceID) && !_beams.Any(b => b.Target.InstanceID == g.InstanceID))
            .OrderBy(g => g.Position.Z)
            .ToList();

        WPos? best = null;
        var bestDist = float.MaxValue;
        for (var i = 0; i + 1 < ghosts.Count; ++i)
        {
            var delta = ghosts[i + 1].Position - ghosts[i].Position;
            if (delta.LengthSq() is < 36 or > 144)
                continue;
            var mid = ghosts[i].Position + delta * 0.5f;
            var dist = (mid - player.Position).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                best = mid;
            }
        }
        if (best != null)
            return best;

        foreach (var ghost in ghosts)
        {
            ReadOnlySpan<WPos> spots = [ghost.Position + new WDir(5.3f, 0), ghost.Position + new WDir(-5.3f, 0), ghost.Position + new WDir(0, 4.3f), ghost.Position + new WDir(0, -4.3f)];
            foreach (var spot in spots)
            {
                var dist = (spot - player.Position).LengthSq();
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = spot;
                }
            }
        }
        return best;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var beam in _beams)
        {
            if (beam.Target.IsDead && beam.Locked == null)
                continue;
            var chasing = beam.Locked == null && beam.Target.InstanceID == actor.InstanceID;
            yield return new(Shape, beam.Locked ?? beam.Target.Position, Activation: beam.Activation, Risky: !chasing);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (TetheredGhost(actor) is { } ghost && _beams.Any(b => b.Target.InstanceID == actor.InstanceID && b.Locked == null))
        {
            hints.GoalZones.Add(p =>
            {
                var distance = (p - ghost.Position).Length();
                return distance is >= 4.5f and <= 5.5f ? 10 : 0;
            });
            return;
        }
        if (BaitSpot(actor) is { } spot)
            hints.GoalZones.Add(p => p.InCircle(spot, 0.4f) ? 10 : 0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TetheredGhost(actor) != null && _beams.Any(b => b.Target.InstanceID == actor.InstanceID && b.Locked == null))
            hints.Add("Bait the ghost into the beam!");
        else if (BaitSpot(actor) != null)
            hints.Add("Bait the beam onto a ghost!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SaintlyBeam)
            _beams.Add(new(actor, WorldState.FutureTime(11.1f), WorldState.FutureTime(8.5f)));
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID != (uint)OID.Phantom || animState1 != 1)
            return;
        Beam? best = null;
        var bestDist = 16f;
        foreach (var beam in _beams)
        {
            if (beam.Locked != null)
                continue;
            var dist = (beam.Target.Position - actor.Position).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                best = beam;
            }
        }
        best?.Locked = actor.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!spell.IsSpell(AID.SaintlyBeam))
            return;
        foreach (var t in spell.Targets)
            _cleared.Add(t.ID);
    }

    public override void Update()
    {
        foreach (var beam in _beams)
            if (beam.Locked == null && WorldState.CurrentTime >= beam.LockAt)
                beam.Locked = beam.Target.Position;
        _beams.RemoveAll(b => WorldState.CurrentTime > b.Activation);
    }

    private sealed class Beam(Actor target, DateTime activation, DateTime lockAt)
    {
        public Actor Target = target;
        public DateTime Activation = activation;
        public DateTime LockAt = lockAt;
        public WPos? Locked;
    }
}

class DiabolicHeadlamp(BossModule module) : Components.GenericAOEs(module, default, invertedText: "Stack in the line")
{
    private static readonly AOEShapeRect Shape = new(60, 3);
    private static readonly WPos Origin = new(0, -22);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.PrimaryActor.CastInfo is not { } cast || !cast.IsSpell(AID.DiabolicHeadlamp))
            yield break;
        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target != null)
            yield return new(Shape, Origin, Angle.FromDirection(target.Position - Origin), Module.CastFinishAt(cast), Inverted: true);
    }
}

class DiabolicWind(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DiabolicWind)
            AddSpread(actor, WorldState.FutureTime(5.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!spell.IsSpell(AID.DiabolicWind))
            return;
        var pos = spell.TargetXZ;
        Spreads.RemoveAll(s => s.Target.Position.AlmostEqual(pos, 10) || WorldState.CurrentTime >= s.Activation);
    }

    public override void Update()
    {
        Spreads.RemoveAll(s => WorldState.CurrentTime > s.Activation);
    }
}
class DiabolicLight(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(15);
    private static readonly WPos Bait = new(0, 22);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var helper in Module.Enemies(OID.Helper))
            if (helper.CastInfo is { } cast && cast.IsSpell(AID.DiabolicLight))
                yield return new(Shape, cast.LocXZ, Activation: Module.CastFinishAt(cast));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (actor.FindStatus(SID.Prey) != null)
            hints.GoalZones.Add(p => p.InCircle(Bait, 3) ? 5 : 0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus(SID.Prey) != null)
            hints.Add("Bait to the back of the train!");
        else
            base.AddHints(slot, actor, hints);
    }
}

class AcidRain(BossModule module) : Components.RaidwideCast(module, AID.AcidRain);
class DoomStrike(BossModule module) : Components.SingleTargetCast(module, AID.DoomStrike);
class ChasingGhost(BossModule module) : Components.Voidzone(module, 4, OID.Ghost, a => a.IsDead)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (actor.FindStatus(SID.Throttle) != null)
            yield break;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(SID.Throttle) != null)
        {
            var ghost = Sources.Where(s => !s.IsDead).MinBy(s => (s.Position - actor.Position).LengthSq());
            if (ghost != null)
                hints.GoalZones.Add(p => p.InCircle(ghost.Position, 1) ? 5 : 0);
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus(SID.Throttle) != null)
            hints.Add("Get caught by a ghost");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (pc.FindStatus(SID.Throttle) == null)
        {
            base.DrawArenaBackground(pcSlot, pc);
            return;
        }
        foreach (var ghost in Sources)
            Arena.ZoneCircle(ghost.Position, Shape.Radius, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.FindStatus(SID.Throttle) == null)
            return;
        foreach (var ghost in Sources)
            Arena.AddCircle(ghost.Position, Shape.Radius, ArenaColor.Safe);
    }
}

class Passengers(BossModule module) : Components.Adds(module, (uint)OID.PutridPassenger);
class CarGhost(BossModule module) : Components.AddsMulti(module, [(uint)OID.PassengerGhost, (uint)OID.PassengerGhost2, (uint)OID.Siegfried]);
class Chimney(BossModule module) : Components.Adds(module, (uint)OID.DoomChimney);

class JumpPads(BossModule module) : BossComponent(module)
{
    private bool _up;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is not (uint)OID.CartJump and not (uint)OID.RoofJump)
            return;
        if (state == 0x00010002)
            _up = true;
        else if (state is 0x00040008 or 0x00040004)
            _up = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_up || actor.Position.X > 100)
            return;

        var chimney = Module.Enemies(OID.DoomChimney).FirstOrDefault(a => !a.IsDead);
        if (chimney == null)
            return;

        if (actor.Position.X < -20)
        {
            var pad = Pad(OID.RoofJump);
            if (pad != null && actor.Position.Z < pad.Position.Z + 8)
                hints.GoalZones.Add(p => p.InCircle(pad.Position, 1.5f) ? 10 : 0);
            else
                hints.GoalZones.Add(p => p.InCircle(chimney.Position, 15) ? 5 : 0);
            return;
        }

        var passengers = Module.Enemies(OID.PutridPassenger).Any(a => a.IsTargetable && !a.IsDead);
        if (!passengers && Pad(OID.CartJump) is { } cart)
            hints.GoalZones.Add(p => p.InCircle(cart.Position, 1.5f) ? 10 : 0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_up || actor.Position.X > 100 || Module.Enemies(OID.DoomChimney).All(a => a.IsDead))
            return;
        if (actor.Position.X < -20)
        {
            var pad = Pad(OID.RoofJump);
            hints.Add(pad != null && actor.Position.Z < pad.Position.Z + 8 ? "Go to the jump pad!" : "Chase the chimney!");
            return;
        }
        if (!Module.Enemies(OID.PutridPassenger).Any(a => a.IsTargetable && !a.IsDead))
            hints.Add("Go to the jump pad!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_up)
            return;
        if (Pad(OID.CartJump) is { } cart)
            Arena.ZoneCircle(cart.Position, 2, ArenaColor.SafeFromAOE);
        if (Pad(OID.RoofJump) is { } roof)
            Arena.ZoneCircle(roof.Position, 2, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_up)
            return;
        if (Pad(OID.CartJump) is { } cart)
            Arena.AddCircle(cart.Position, 2, ArenaColor.Safe);
        if (Pad(OID.RoofJump) is { } roof)
            Arena.AddCircle(roof.Position, 2, ArenaColor.Safe);
    }

    private Actor? Pad(OID oid) => WorldState.Actors.FirstOrDefault(a => a.OID == (uint)oid);
}

class O5NPhantomTrainStates : StateMachineBuilder
{
    public O5NPhantomTrainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Encumber>()
            .ActivateOnEnter<HeadOn>()
            .ActivateOnEnter<SaintlyBeam>()
            .ActivateOnEnter<DiabolicHeadlamp>()
            .ActivateOnEnter<DiabolicWind>()
            .ActivateOnEnter<DiabolicLight>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<DoomStrike>()
            .ActivateOnEnter<ChasingGhost>()
            .ActivateOnEnter<Passengers>()
            .ActivateOnEnter<CarGhost>()
            .ActivateOnEnter<Chimney>()
            .ActivateOnEnter<JumpPads>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 286, NameID = 7127)]
public class O5NPhantomTrain(ModuleInit init) : BossModule(init, MainCenter, MainBounds)
{
    public static readonly WPos MainCenter = new(0, 4);
    public static readonly ArenaBoundsRect MainBounds = new(10, 20, MapResolution: 0.1f); // this is needed because the space between the wroths in miniscule
    public static readonly ArenaBoundsRect CarBounds = new(7, 7);
    private static readonly WPos RoofCenter = new(-30.6f, 51);
    private static readonly ArenaBoundsCustom RoofBounds = BuildRoofArena();

    // two cars at (-30.6, 1) and (-30.6, 90), conencted via jump pad
    private static ArenaBoundsCustom BuildRoofArena()
    {
        var forward = new WDir(0, 1);
        var lower = new PolygonClipper.Operand(CurveApprox.Rect(new WDir(0, 1 - RoofCenter.Z), forward, 7, 24));
        var upper = new PolygonClipper.Operand(CurveApprox.Rect(new WDir(0, 90 - RoofCenter.Z), forward, 7, 35));
        return new(76, new PolygonClipper().Union(lower, upper));
    }

    private bool _occupied;

    private static bool Near(WPos a, WPos b, float radius) => (a - b).LengthSq() <= radius * radius;

    private static bool OnRoof(WPos pos) => pos.X < -20;

    private IEnumerable<Actor> Streams => WorldState.Actors.Where(a => a.OID is >= 0x1EA938 and <= 0x1EA93F);

    private Actor? CarStream(Actor player) => Streams.FirstOrDefault(s => Near(player.Position, s.Position, 12));

    protected override void UpdateModule()
    {
        var player = Raid.Player();
        if (player == null)
            return;

        if (CarStream(player) is { } stream)
        {
            Arena.Center = stream.Position;
            Arena.Bounds = CarBounds;
        }
        else if (OnRoof(player.Position))
        {
            Arena.Center = RoofCenter;
            Arena.Bounds = RoofBounds;
        }
        else
        {
            Arena.Center = MainCenter;
            Arena.Bounds = MainBounds;
        }
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedInQuestEvent])
            _occupied = true;

        var stream = CarStream(actor);
        var backOnCart = stream == null && !OnRoof(actor.Position) && MainBounds.Contains(actor.Position - MainCenter);
        if (_occupied && backOnCart)
            _occupied = false;

        if (stream != null)
        {
            // forbid far away enemies cause it messes with the interact
            foreach (var enemy in hints.PotentialTargets)
                if (!Near(enemy.Actor.Position, stream.Position, 7))
                    enemy.Priority = AIHints.Enemy.PriorityForbidden;

            if (stream.IsTargetable && !_occupied)
                hints.InteractWithTarget = stream;
        }

        // drop when we're occupied (and thus interacting)
        // nothing can cancel it at this point unless user error
        if (_occupied && !backOnCart)
            hints.ForcedMovement = default;
    }

    public override Actor? GetDefaultTarget(int slot)
    {
        var player = Raid.Player();
        if (player == null)
            return base.GetDefaultTarget(slot);
        if (CarStream(player) is { } stream)
            return WorldState.Actors.FirstOrDefault(a => a.Type == ActorType.Enemy && a.IsTargetable && !a.IsDead && Near(a.Position, stream.Position, 7));
        if (_occupied)
            return null;
        if (OnRoof(player.Position))
            return Enemies(OID.DoomChimney).FirstOrDefault(a => a.IsTargetable && !a.IsDead) ?? base.GetDefaultTarget(slot);
        return base.GetDefaultTarget(slot);
    }
}
