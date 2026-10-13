namespace BossMod.RealmReborn.Alliance.A37CloudOfDarkness;

public enum OID : uint
{
    Boss = 0xCFA, // R14.000, x1
    Helper = 0x1B2, // R0.500
    HelperAlt = 0x8EE, // R0.500
    DarkCloud = 0xE1C, // R1.000
    DarkStorm = 0xE1D, // R2.000
    HyperchargedCloud = 0xE03, // R1.000
    Shadowlurker = 0xCFB, // R1.000
    ParticleBeamTower = 0x1E974E, // R0.500, EventObj
    HyperchargedHexA = 0x1E9752, // A
    HyperchargedHexB = 0x1E9750, // B
    HyperchargedHexC = 0x1E9751, // C
}

public enum AID : uint
{
    AutoAttack = 3306, // Boss->player, no cast, single-target
    FeintParticleBeam = 3298, // Boss->location, 3.5s cast, range 8 circle
    FeintParticleBeamChase = 3299, // Helper->location, no cast, range 3 circle
    ZeroFormParticleBeam = 3297, // Boss->self, 2.5s cast, range 60+R width 24 rect
    ParticleBeam = 3301, // Helper->location, no cast, range 5 circle (soaked comet)
    ParticleBeamFail = 3300, // Helper->location, no cast, range 60 circle (unsoaked comet)
    Unknown = 2369, // HelperAlt->self, no cast, range 20
    FloodOfDarkness = 3305, // Boss->location, no cast, range 60 circle
    ParticleBeamEnrage = 3302, // HyperchargedCloud->self, 20.0s cast, range 60 circle
    BlackThunder = 3304, // Shadowlurker->players, no cast, range 20 circle
    BadBreath = 3303, // Shadowlurker->self, no cast, range 60+R cone
}

class ZeroFormParticleBeam(BossModule module) : Components.StandardAOEs(module, AID.ZeroFormParticleBeam, new AOEShapeRect(74, 12));

class FeintParticleBeam(BossModule module) : Components.StandardChasingAOEs(module, _chase, AID.FeintParticleBeam, AID.FeintParticleBeamChase, 3, 0.6f, 17)
{
    private static readonly AOEShapeCircle _first = new(8);
    private static readonly AOEShapeCircle _chase = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Chasers)
        {
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            var shape = c.NumRemaining >= MaxCasts ? _first : c.Shape;
            yield return new(shape, pos, off.LengthSq() > 0 ? Angle.FromDirection(off) : default, c.NextActivation);
        }
    }

    public override void Update()
    {
        for (var i = Chasers.Count - 1; i >= 0; --i)
        {
            var c = Chasers[i];
            if (c.Target.IsDestroyed || c.Target.IsDead) // ends if the target dies
            {
                ExcludedTargets.Clear(Raid.FindSlot(c.Target.InstanceID));
                Chasers.RemoveAt(i);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Chasers.All(c => c.Target != actor))
            return;

        // drag chase away from the raid
        var awayFromBoss = (Arena.Center - Module.PrimaryActor.Position).Normalized();
        if (awayFromBoss.LengthSq() > 0.01f)
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(Arena.Center, awayFromBoss));

        // and don't run the puddles through other players
        foreach (var p in Raid.WithoutSlot().Exclude(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, 4));
    }
}

class ParticleBeamTowers(BossModule module) : Components.GenericTowers(module, AID.ParticleBeam)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ParticleBeamTower)
            Towers.Add(new(actor.Position, 5, minSoakers: 1, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(10)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.ParticleBeamTower)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ParticleBeam or AID.ParticleBeamFail)
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));
    }
}

class HyperchargedClouds(BossModule module) : BossComponent(module)
{
    public const float HexRadius = 8.5f;
    private static readonly uint[] _hexOIDs = [(uint)OID.HyperchargedHexA, (uint)OID.HyperchargedHexB, (uint)OID.HyperchargedHexC];
    private readonly HashSet<ulong> _active = [];

    private static AllianceLetter HexAlliance(uint oid) => oid switch
    {
        (uint)OID.HyperchargedHexA => AllianceLetter.A,
        (uint)OID.HyperchargedHexB => AllianceLetter.B,
        (uint)OID.HyperchargedHexC => AllianceLetter.C,
        _ => AllianceLetter.None
    };

    private IEnumerable<Actor> ActiveHexes => WorldState.Actors.Where(a => _active.Contains(a.InstanceID));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (!_hexOIDs.Contains(actor.OID))
            return;
        if (state == 0x00040008)
            _active.Add(actor.InstanceID);
        else if (state == 0x00010040)
            _active.Remove(actor.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor) => _active.Remove(actor.InstanceID);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var mine = ActiveHexes.FirstOrDefault(h => HexAlliance(h.OID) == Raid.Alliance);
        if (mine != null)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(mine.Position, HexRadius, 5));

        foreach (var c in Module.Enemies(OID.HyperchargedCloud).Where(c => c.IsTargetable && !c.IsDead))
            hints.SetPriority(c, 3);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var myAlliance = Raid.Alliance;
        foreach (var hex in ActiveHexes)
            DrawHex(hex.Position, HexAlliance(hex.OID) == myAlliance ? ArenaColor.Safe : ArenaColor.Danger);
    }

    private void DrawHex(WPos center, uint color)
    {
        Span<WPos> pts = stackalloc WPos[6];
        for (var i = 0; i < 6; ++i)
            pts[i] = center + HexRadius * (i * 60).Degrees().ToDirection();
        Arena.AddPolygon(pts, color, 2);
    }
}

class ParticleBeamEnrage(BossModule module) : Components.RaidwideCast(module, AID.ParticleBeamEnrage, "Kill Hypercharged Clouds!");
class Shadowlurkers(BossModule module) : Components.Adds(module, (uint)OID.Shadowlurker, 2);
class Adds(BossModule module) : Components.AddsMulti(module, [OID.DarkCloud, OID.DarkStorm, OID.HyperchargedCloud], 1);

class A37CloudOfDarknessStates : StateMachineBuilder
{
    public A37CloudOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ZeroFormParticleBeam>()
            .ActivateOnEnter<FeintParticleBeam>()
            .ActivateOnEnter<ParticleBeamTowers>()
            .ActivateOnEnter<HyperchargedClouds>()
            .ActivateOnEnter<ParticleBeamEnrage>()
            .ActivateOnEnter<Shadowlurkers>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3240)]
public class A37CloudOfDarkness(ModuleInit init) : BossModule(init, new(-300, -400), new ArenaBoundsCircle(30));
