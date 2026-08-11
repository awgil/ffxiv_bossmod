namespace BossMod.Dawntrail.Foray.CriticalEngagement.ClaretDragon;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x19, Helper type
    ClaretDragon = 0x4D25, // R1.000, x1
    Boss = 0x4C46, // R5.000, x1
    Necrohaze = 0x4C47, // R1.500, x0 (spawn during fight)
    AetherialWard = 0x4C48, // R7.000, x0 (spawn during fight)

    AetherialWardShield = 0x1EC094,
    AetherialWardPuddle = 0x1EC095,
}

public enum AID : uint
{
    DeathWall = 48279, // _Gen_ClaretDragon->self, no cast, ???
    AutoAttack = 48259, // Boss->player, no cast, single-target
    HowlingDarknessCast = 48277, // Boss->self, 5.0s cast, single-target
    HowlingDarkness = 48278, // Helper->self, no cast, ???
    SnakingNecrobreath = 48260, // Boss->self, 6.0s cast, range 60 270-degree cone
    GraveMoldCast = 48261, // Boss->self, 5.0s cast, single-target
    GraveMold = 48262, // Helper->self, 6.0s cast, range 8 circle
    NecrohazePuddle1 = 48263, // _Gen_Necrohaze->self, no cast, range 5 circle
    NecrohazePuddle2 = 48269, // Helper->self, no cast, range 5 circle
    NecrohazePuddle3 = 48268, // Helper->location, no cast, range 5 circle
    Soar = 50488, // Boss->self, 4.0s cast, single-target
    CauterizeCast = 48264, // Boss->self, 6.0s cast, single-target
    Cauterize = 48265, // Helper->self, 7.0s cast, range 40 width 10 rect
    Catching = 48267, // _Gen_Necrohaze->self, no cast, range 30 width 10 rect
    AetherialWardCast = 48271, // Boss->self, 4.0+0.5s cast, single-target
    NecrohazeWard = 50484, // Helper->self, 4.0s cast, range 5 circle
    BreathInThreesSlow = 48270, // Boss->self, 5.0s cast, range 60 120-degree cone
    BreathInThreesFast = 48248, // Boss->self, 2.5s cast, range 60 120-degree cone
    Unk1 = 48302, // Boss->self, no cast, single-target
    Unk2 = 48275, // Boss->self, no cast, single-target
    Unk3 = 48276, // Boss->self, no cast, single-target
    Unk4 = 48266, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    GradualZombification = 5059, // Boss/Helper/_Gen_Necrohaze->player, extra=0x1
    Zombification = 2305, // _Gen_Necrohaze/Helper->player, extra=0x0
    ZombieProof = 5138, // Helper/_Gen_Necrohaze->player, extra=0x0
    UnkBoss = 2056, // Boss->Boss, extra=0x164
    Heavy = 1796, // none->_Gen_Necrohaze, extra=0x32
    DirectionalInvincibility = 1125, // none->_Gen_AetherialWard, extra=0x0
}

class HowlingDarkness(BossModule module) : Components.RaidwideCastDelay(module, AID.HowlingDarknessCast, AID.HowlingDarkness, 0.8f);
class SnakingNecrobreath(BossModule module) : Components.StandardAOEs(module, AID.SnakingNecrobreath, new AOEShapeCone(60, 135.Degrees()));

class GraveMold(BossModule module) : Components.StandardAOEs(module, AID.GraveMold, 8);
class Necrohaze : Components.PersistentVoidzone
{
    readonly List<Actor> _actors = [];

    public Necrohaze(BossModule module) : base(module, 5, m => [])
    {
        Sources = _ => _actors;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var m in Sources(Module))
        {
            var rotationHalf = 25;

            hints.AddForbiddenZone(ShapeDistance.Circle(m.Position, 5));
            if (m.LastFrameMovement != default)
            {
                var cw = m.Rotation.ToDirection().OrthoR().Dot(m.DirectionTo(Arena.Center)) > 0;
                var diff = m.Position - Arena.Center;
                var angle = diff.ToAngle();
                var dist = diff.Length();
                hints.AddForbiddenZone(ShapeDistance.DonutSector(Arena.Center, dist - 5, dist + 5, angle + (cw ? -rotationHalf : rotationHalf).Degrees(), rotationHalf.Degrees()), WorldState.FutureTime(4));
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center + (angle + 2 * (cw ? -rotationHalf : rotationHalf).Degrees()).ToDirection() * dist, 5), WorldState.FutureTime(4));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Necrohaze)
            _actors.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Catching)
            _actors.Remove(caster);
    }

    public override void Update()
    {
        _actors.RemoveAll(a => a.IsDeadOrDestroyed);
    }
}

class NecrohazeBossPuddle(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.AetherialWardPuddle));

class AetherialWard(BossModule module) : Components.DirectionalParry(module, (uint)OID.AetherialWard)
{
    Side _activeSides;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.AetherialWardShield && state == 0x00010002)
        {
            if (ActiveActors.FirstOrDefault() is not { } ward)
                return;
            var diff = (actor.Rotation - ward.Rotation).Normalized();
            var side = MathF.Abs(diff.Deg) switch
            {
                < 45 => Side.Front,
                < 135 => diff.Deg < 0 ? Side.Right : Side.Left,
                _ => Side.Back
            };

            _activeSides |= side;

            PredictParrySide(ward.InstanceID, _activeSides);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectionalInvincibility)
            _actorStates[actor.InstanceID] = (int)_activeSides;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectionalInvincibility)
        {
            _actorStates[actor.InstanceID] = 0;
            _activeSides = Side.None;
        }
    }
}

class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(40, 5));

class Catching(BossModule module) : Components.GenericAOEs(module, AID.Catching)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Cauterize)
            foreach (var puddle in Module.Enemies(OID.Necrohaze))
            {
                if (puddle.Position.InRect(spell.LocXZ, spell.Rotation, 40, 0, 6))
                {
                    var left = spell.Rotation.ToDirection().OrthoL().Dot(puddle.Position - spell.LocXZ) > 0;
                    _predicted.Add(new(new AOEShapeRect(30, 5), puddle.Position, spell.Rotation + (left ? 90 : -90).Degrees(), Module.CastFinishAt(spell, 0.9f)));
                }
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class BreathInThrees(BossModule module) : Components.GroupedAOEs(module, [AID.BreathInThreesFast, AID.BreathInThreesSlow], new AOEShapeCone(60, 60.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // ranged should move close to the boss since the subsequent casts are much faster
        if (Casters.FirstOrDefault(c => c.CastInfo!.IsSpell(AID.BreathInThreesSlow)) is { } c1)
            hints.GoalZones.Add(hints.GoalSingleTarget(c1.Position, 10, 0.5f));
    }
}

class ClaretDragonStates : StateMachineBuilder
{
    public ClaretDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HowlingDarkness>()
            .ActivateOnEnter<SnakingNecrobreath>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Catching>()
            .ActivateOnEnter<AetherialWard>()
            .ActivateOnEnter<GraveMold>()
            .ActivateOnEnter<Necrohaze>()
            .ActivateOnEnter<NecrohazeBossPuddle>()
            .ActivateOnEnter<BreathInThrees>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14787)]
public class ClaretDragon(WorldState ws, Actor primary) : CEModule(ws, primary, new(-688, 150), new ArenaBoundsSquare(20));

