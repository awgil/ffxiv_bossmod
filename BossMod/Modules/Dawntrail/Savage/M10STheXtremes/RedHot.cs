namespace BossMod.Dawntrail.Savage.M10STheXtremes;

public class VoidzoneShape(BossModule module, uint OID, AOEShape shape) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    public VoidzoneShape(BossModule module, uint OID, float radius) : this(module, OID, new AOEShapeCircle(radius)) { }
    public readonly AOEShape Shape = shape;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        foreach (var source in GetVoidActors())
        {
            aoes.Add(new(Shape, source.Position, source.Rotation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    private ReadOnlySpan<Actor> GetVoidActors()
    {
        var enemies = Module.Enemies(OID);
        var enemySpan = CollectionsMarshal.AsSpan(enemies);
        var len = enemySpan.Length;

        if (len == 0)
            return [];

        List<Actor> voids = [];
        for (var i = 0; i < len; i++)
        {
            var enemy = enemySpan[i];
            if (enemy.EventState != 7)
            {
                voids.Add(enemy);
            }
        }

        return CollectionsMarshal.AsSpan(voids);
    }
}

sealed class FlameFloaterPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleFlameFloater, new AOEShapeRect(60f, 4f));
sealed class AlleyOopInfernoPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleAlleyOopInferno, 5f);
sealed class CutbackBlazePuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleCutbackBlaze, new AOEShapeCone(60f, 165f.Degrees()));
sealed class Pyrorotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PyrorotationStack, (uint)AID.Pyrotation, 6f, 5d, 6, 8, 3);
sealed class PyrorotationPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddlePyrorotation, 6f);
sealed class BlastingSnapPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleInsaneAirCone, new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact, 6f);
sealed class CutbackBlaze(BossModule module) : Components.CastCounter(module, (uint)AID.CutbackBlaze)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private bool _active = false;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        // don't make it AOE so AI doesn't freak out
        // assuming it's baited by top aggro
        // looks more like furthest player
        var primary = Module.PrimaryActor;
        var target = GetFurthest();

        if (target == null)
            return;

        Arena.ZoneCone(primary.Position, 0f, 60f, (target.Position - primary.Position).ToAngle(), 165f.Degrees(), Colors.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CutbackBlazeCast)
        {
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _active = false;
        }
    }

    private Actor? GetFurthest()
    {
        var party = Raid.WithSlot(false, true, true);
        var fires = _debuff.FirePlayers;

        if (fires.Any())
        {
            party = [.. party.IncludedInMask(fires)];
        }

        var len = party.Length;

        if (len == 0)
            return null;

        var source = Module.PrimaryActor;
        Actor? furthest = null;
        var furthestDistSq = float.MinValue;

        for (var i = 0; i < len; i++)
        {
            ref readonly var player = ref party[i].Item2;
            var distSq = (player.Position - source.Position).LengthSq();

            if (distSq > furthestDistSq)
            {
                furthestDistSq = distSq;
                furthest = player;
            }
        }
        return furthest;
    }
}
sealed class DiversDareRed(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDareRed);
sealed class AlleyOopInferno(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno, 5f)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private readonly M10STheXtremesConfig _config = Service.Config.Get<M10STheXtremesConfig>();
    private readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();

    public enum State { None, Snaking, DeepVarial, SplitArena }
    public State CurrentState = State.None;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_config.ShowFireAlleyOopHints)
        {
            // targetted just before deep varial finishes casting; don't use active spreads
            if (CurrentState == State.None)
                return;

            if (CurrentState == State.DeepVarial)
            {
                var deepvarial = Module.FindComponent<DeepVarial>();
                if (deepvarial == null)
                    return;

                var cleaves = deepvarial.ActiveCasters;
                if (cleaves.Length == 0)
                    return;

                var origin = cleaves[0].Origin;
                var rotation = cleaves[0].Rotation;

                DrawPlayerPosition(pcSlot, pc, origin, rotation);
            }
            else
            {
                DrawPlayerPosition(pcSlot, pc);
            }
        }
    }

    private void DrawPlayerPosition(int pcSlot, Actor pc, WPos? varialOrigin = null, Angle? varialRotation = null)
    {
        var firePlayers = _debuff.FirePlayers;
        if (firePlayers.Any())
        {
            if (!firePlayers[pcSlot])
                return;
        }

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        if (assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        var role = assignment switch
        {
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => 0,
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => 1,
            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => 2,
            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => 3,
            _ => -1
        };

        var radius = CurrentState switch
        {
            State.Snaking => 1.5f,
            State.DeepVarial => 1f,
            State.SplitArena => 2f,
            _ => 0f
        };

        if (CurrentState == State.Snaking)
        {
            // After snaking
            // Hector -> M R T H in 2x2 box next to wall
            // JP -> R T M H top-down single column along wall
            // both NA and JP resolve fire on east side

            if (_config.HintOption == Strategy.Hector)
            {
                // 1f from edge, 6f apart
                var row = role is 0 or 2 ? 1 : 0;
                var col = role is 2 or 3 ? 1 : 0;
                var origin = new WPos(113f, 97f);
                var targetPos = origin + new WDir(col * 6f, row * 6f);
                var targetSd = new SDCircle(targetPos, radius);
                Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
            }
            else
            {
                var origin = new WPos(119f, 93f);
                var mult = role == 3 ? 0 : role + 1;
                var targetPos = origin + new WDir(0f, mult * 6f);
                var targetSd = new SDCircle(targetPos, radius);
                Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
            }
        }
        else if (CurrentState == State.DeepVarial)
        {
            if (varialOrigin == null || varialRotation == null)
                return;

            // Deep Varial
            // Hector -> T M H R from boss, R on other wall
            // JP -> T M H R from boss, all on same wall as deep varial, much tighter

            if (_config.HintOption == Strategy.Hector)
            {
                // more leeway than JP, say 6.5f inbetween
                var targetPos = varialOrigin + varialRotation.Value.ToDirection() + new WDir(5f + (role < 3 ? role : 2) * 6.5f, 0f);
                if (role == 3)
                {
                    targetPos += varialRotation.Value.ToDirection() * 7f;
                }
                var targetSd = new SDCircle(targetPos.Value, radius);
                Arena.ZoneCircleOutline(targetPos.Value, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
            }
            else
            {
                // T M H R -> AOE 5f radius, assume 1f from edge, say 5.5f inbetween -> 102.5f, 108f, 113.5f, 119f
                // leeway to move after deep varial resolves
                var targetPos = varialOrigin + varialRotation.Value.ToDirection() + new WDir(2.5f + role * 5.5f, 0f);
                var targetSd = new SDCircle(targetPos.Value, radius);
                Arena.ZoneCircleOutline(targetPos.Value, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
            }
        }
        else
        {
            // NA and JP use same strat for split arena, static positions
            WPos targetPos = assignment switch
            {
                PartyRolesConfig.Assignment.R1 => new(81f, 82f),
                PartyRolesConfig.Assignment.MT => new(81f, 89f),
                PartyRolesConfig.Assignment.M1 => new(81f, 111f),
                PartyRolesConfig.Assignment.H1 => new(81f, 118f),
                PartyRolesConfig.Assignment.R2 => new(119f, 82f),
                PartyRolesConfig.Assignment.OT => new(119f, 89f),
                PartyRolesConfig.Assignment.M2 => new(119f, 111f),
                PartyRolesConfig.Assignment.H2 => new(119f, 118f),
                _ => default
            };
            var targetSd = new SDCircle(targetPos, radius);
            Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
        }
    }
}
sealed class HotImpact2(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact2, 6f);
sealed class HotAerial(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private bool _active = false;

    // TODO: jumps onto furthest player with firesnaking (or random if players dead?)
    // don't show all baits at once
    // only show baits if player has the right status
    // missing cutback 330 cone, not necessarily boss rotation? or just lag?
    // missing purorotation stack indicator
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HotAeriaCast)
        {
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HotAerial4)
        {
            _active = false;
            Spreads.Clear();
        }
    }

    public override void Update()
    {
        if (!_active)
            return;

        Spreads.Clear();

        var target = FindActorByDistance(false);
        if (target == null)
            return;

        Spreads.Add(new(target, 6f));
    }

    private Actor? FindActorByDistance(bool findClosest)
    {
        var fires = Raid.WithSlot(false, true, true).IncludedInMask(_debuffs.FirePlayers);
        var source = Module.PrimaryActor;
        Actor? selected = null;

        var bestDistance = findClosest ? float.MaxValue : float.MinValue;

        foreach (var (_, p) in fires)
        {
            var distSq = (p.Position - source.Position).LengthSq();
            var isBetter = findClosest ? distSq < bestDistance : distSq > bestDistance;

            if (isBetter)
            {
                bestDistance = distSq;
                selected = p;
            }
        }
        return selected;
    }
}
sealed class FreakyPyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FreakyPyrotation, (uint)AID.FreakyPyrotation, 6f, 5f, 2, 2);
sealed class FlameFloaterSplit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlameFloaterSplit, new AOEShapeRect(60f, 4f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameFloaterSplitCast)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            Casters.Add(new(Shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Clear();
        }
    }
}
