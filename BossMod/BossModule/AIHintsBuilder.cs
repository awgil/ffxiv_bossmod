namespace BossMod;

// utility that recalculates ai hints based on different data sources (eg active bossmodule, etc)
// when there is no active bossmodule (eg in outdoor or on trash), we try to guess things based on world state (eg actor casts)
public sealed class AIHintsBuilder : IDisposable
{
    private const float RaidwideSize = 30;
    public const float MaxError = 2000f / 65535f; // TODO: this should really be handled by the rasterization itself...

    public readonly Pathfinding.ObstacleMapManager Obstacles;
    private readonly WorldState _ws;
    private readonly BossModuleManager _bmm;
    private readonly ZoneModuleManager _zmm;
    private readonly EventSubscriptions _subscriptions;
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = [];
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape)> _activeGazes = [];
    private ArenaBoundsCircle? _activeFateBounds;

    public AIHintsBuilder(WorldState ws, BossModuleManager bmm, ZoneModuleManager zmm)
    {
        _ws = ws;
        _bmm = bmm;
        _zmm = zmm;
        Obstacles = new(ws);
        _subscriptions = new
        (
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Client.ActiveFateChanged.Subscribe(_ => _activeFateBounds = null)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        Obstacles.Dispose();
    }

    public void Update(AIHints hints, int playerSlot, bool moveImminent)
    {
        var player = _ws.Party[playerSlot];

        hints.Clear();
        if (moveImminent || player?.PendingKnockbacks.Count > 0)
            hints.MaxCastTime = 0;
        if (player != null)
        {
            var playerAssignment = Service.Config.Get<PartyRolesConfig>()[_ws.Party.Members[playerSlot].ContentId];
            var activeModule = _bmm.ActiveModule?.StateMachine.ActivePhase != null ? _bmm.ActiveModule : null;
            FillEnemies(hints, playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !_ws.Party.WithoutSlot().Any(p => p != player && p.Role == Role.Tank));
            if (activeModule != null)
            {
                activeModule.CalculateAIHints(playerSlot, player, playerAssignment, hints);
            }
            else
            {
                CalculateAutoHints(hints, player);
                _zmm.ActiveModule?.CalculateAIHints(playerSlot, player, hints);
            }
        }
        hints.Normalize();
    }

    // fill list of potential targets from world state
    private void FillEnemies(AIHints hints, bool playerIsDefaultTank)
    {
        var allowedFateID = Utils.IsPlayerSyncedToFate(_ws) ? _ws.Client.ActiveFate.ID : 0;
        foreach (var actor in _ws.Actors.Where(a => a.IsTargetable && !a.IsAlly && !a.IsDead))
        {
            var index = actor.CharacterSpawnIndex;
            if (index < 0 || index >= hints.Enemies.Length)
                continue;

            // determine default priority for the enemy
            var priority = actor.FateID > 0 && actor.FateID != allowedFateID ? AIHints.Enemy.PriorityInvincible // fate mob in fate we are NOT a part of can't be damaged at all
                : actor.PredictedDead ? AIHints.Enemy.PriorityPointless // this mob is about to be dead, any attacks will likely ghost
                : actor.AggroPlayer ? 0 // enemies in our enmity list can be attacked, regardless of who they are targeting (since they are keeping us in combat)
                : actor.InCombat && _ws.Party.FindSlot(actor.TargetID) >= 0 ? 0 // we generally want to assist our party members (note that it includes allied npcs in duties)
                : AIHints.Enemy.PriorityUndesirable; // this enemy is either not pulled yet or fighting someone we don't care about - try not to aggro it by default

            var enemy = hints.Enemies[index] = new(actor, priority, playerIsDefaultTank);
            hints.PotentialTargets.Add(enemy);
        }
    }

    private void CalculateAutoHints(AIHints hints, Actor player)
    {
        var inFate = Utils.IsPlayerSyncedToFate(_ws);
        var center = inFate ? _ws.Client.ActiveFate.Center : player.PosRot.XYZ();
        var (e, bitmap) = Obstacles.Find(center);
        var resolution = bitmap?.PixelSize ?? 0.5f;
        if (inFate)
        {
            hints.PathfindMapCenter = new(_ws.Client.ActiveFate.Center.XZ());
            hints.PathfindMapBounds = (_activeFateBounds ??= new ArenaBoundsCircle(_ws.Client.ActiveFate.Radius, resolution));
            if (e != null && bitmap != null)
            {
                var originCell = (hints.PathfindMapCenter - e.Origin) / resolution;
                var originX = (int)originCell.X;
                var originZ = (int)originCell.Z;
                var halfSize = (int)(_ws.Client.ActiveFate.Radius / resolution);
                hints.PathfindMapObstacles = new(bitmap, new(originX - halfSize, originZ - halfSize, originX + halfSize, originZ + halfSize));
            }
        }
        else if (e != null && bitmap != null)
        {
            var originCell = (player.Position - e.Origin) / resolution;
            var originX = (int)originCell.X;
            var originZ = (int)originCell.Z;
            // if player is too close to the border, adjust origin
            originX = Math.Min(originX, bitmap.Width - e.ViewWidth);
            originZ = Math.Min(originZ, bitmap.Height - e.ViewHeight);
            originX = Math.Max(originX, e.ViewWidth);
            originZ = Math.Max(originZ, e.ViewHeight);
            // TODO: consider quantizing even more, to reduce jittering when player moves?..
            hints.PathfindMapCenter = e.Origin + resolution * new WDir(originX, originZ);
            hints.PathfindMapBounds = new ArenaBoundsRect(e.ViewWidth * resolution, e.ViewHeight * resolution, MapResolution: resolution); // note: we don't bother caching these bounds, they are very lightweight
            hints.PathfindMapObstacles = new(bitmap, new(originX - e.ViewWidth, originZ - e.ViewHeight, originX + e.ViewWidth, originZ + e.ViewHeight));
        }
        else
        {
            hints.PathfindMapCenter = player.Position.Rounded(5);
            // try to keep player near grid center
            var playerOffset = player.Position - hints.PathfindMapCenter;
            if (playerOffset.X < -1.25f)
                hints.PathfindMapCenter.X -= 2.5f;
            else if (playerOffset.X > 1.25f)
                hints.PathfindMapCenter.X += 2.5f;
            if (playerOffset.Z < -1.25f)
                hints.PathfindMapCenter.Z -= 2.5f;
            else if (playerOffset.Z > 1.25f)
                hints.PathfindMapCenter.Z += 2.5f;
            // keep default bounds
        }

        foreach (var aoe in _activeAOEs.Values)
        {
            var target = aoe.Target?.Position ?? aoe.Caster.CastInfo!.LocXZ;
            var rot = aoe.Caster.CastInfo!.Rotation;
            var finishAt = _ws.FutureTime(aoe.Caster.CastInfo.NPCRemainingTime);
            if (aoe.IsCharge)
            {
                hints.AddForbiddenZone(ShapeDistance.Rect(aoe.Caster.Position, target, ((AOEShapeRect)aoe.Shape).HalfWidth), finishAt, aoe.Caster.InstanceID);
            }
            else if (aoe.Shape is AOEShapeCone cone)
            {
                // not sure how best to adjust cone shape distance to account for quantization error - we just pretend it is being cast from MaxError units "behind" the reported position and increase radius similarly
                var adjustedSourcePos = target + rot.ToDirection() * -MaxError;
                var adjustedRadius = cone.Radius + MaxError * 2;
                hints.AddForbiddenZone(ShapeDistance.Cone(adjustedSourcePos, adjustedRadius, rot, cone.HalfAngle), finishAt, aoe.Caster.InstanceID);
            }
            else
            {
                hints.AddForbiddenZone(aoe.Shape, target, rot, finishAt, aoe.Caster.InstanceID);
            }
        }

        foreach (var gaze in _activeGazes.Values)
        {
            var target = gaze.Target?.Position ?? gaze.Caster.CastInfo!.LocXZ;
            var rot = gaze.Caster.CastInfo!.Rotation;
            var finishAt = _ws.FutureTime(gaze.Caster.CastInfo.NPCRemainingTime);
            if (gaze.Shape.Check(player.Position, target, rot))
                hints.ForbiddenDirections.Add((Angle.FromDirection(target - player.Position), 45.Degrees(), finishAt));
        }
    }

    private void OnCastStarted(Actor actor)
    {
        if (actor.Type != ActorType.Enemy || actor.IsAlly)
            return;
        if (Service.LuminaRow<Lumina.Excel.Sheets.Action>(actor.CastInfo!.Action.ID) is not { } data)
            return;

        // gaze
        if (data.VFX.RowId == 25)
        {
            if (GuessShape(data, actor) is AOEShape sh)
                _activeGazes[actor.InstanceID] = (actor, _ws.Actors.Find(actor.CastInfo.TargetID), sh);
            return;
        }

        if (!actor.CastInfo!.IsSpell() || data.CastType == 1)
            return;
        //if (data.Omen.Row == 0)
        //    return; // to consider: ignore aoes without omen, such aoes typically need a module to resolve...
        if (data.CastType is 2 or 5 && data.EffectRange >= RaidwideSize)
            return;
        if (GuessShape(data, actor) is not AOEShape shape)
        {
            Service.Log($"[AutoHints] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
            return;
        }
        var target = _ws.Actors.Find(actor.CastInfo.TargetID);
        _activeAOEs[actor.InstanceID] = (actor, target, shape, data.CastType == 8);
    }

    private void OnCastFinished(Actor actor)
    {
        _activeAOEs.Remove(actor.InstanceID);
        _activeGazes.Remove(actor.InstanceID);
    }

    private static AOEShape? GuessShape(Lumina.Excel.Sheets.Action data, Actor actor) => data.CastType switch
    {
        2 => new AOEShapeCircle(data.EffectRange + MaxError), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
        3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * 0.5f),
        4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius + MaxError, data.XAxisModifier * 0.5f + MaxError, MaxError),
        5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius + MaxError),
        //6 => ???
        //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
        //8 => charge rect
        10 => new AOEShapeDonut(MathF.Max(0, DetermineDonutInner(data) - MaxError), data.EffectRange + MaxError),
        11 => new AOEShapeCross(data.EffectRange + MaxError, data.XAxisModifier * 0.5f + MaxError),
        12 => new AOEShapeRect(data.EffectRange + MaxError, data.XAxisModifier * 0.5f + MaxError, MaxError),
        13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f),
        _ => null
    };

    private static Angle DetermineConeAngle(Lumina.Excel.Sheets.Action data)
    {
        var omen = data.Omen.ValueNullable;
        if (omen == null)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        var path = omen.Value.Path.ToString();
        var pos = path.IndexOf("fan", StringComparison.Ordinal);
        if (pos < 0 || pos + 6 > path.Length || !int.TryParse(path.AsSpan(pos + 3, 3), out var angle))
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.Value.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        return angle.Degrees();
    }

    private static float DetermineDonutInner(Lumina.Excel.Sheets.Action data)
    {
        var omen = data.Omen.ValueNullable;
        if (omen == null)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return 0;
        }
        var path = omen.Value.Path.ToString();
        var pos = path.IndexOf("sircle_", StringComparison.Ordinal);
        if (pos >= 0 && pos + 11 <= path.Length && int.TryParse(path.AsSpan(pos + 9, 2), out var inner))
            return inner;

        pos = path.IndexOf("circle", StringComparison.Ordinal);
        if (pos >= 0 && pos + 10 <= path.Length && int.TryParse(path.AsSpan(pos + 8, 2), out inner))
            return inner;

        Service.Log($"[AutoHints] Can't determine inner radius from omen ({path}/{omen.Value.PathAlly}) for {data.RowId} '{data.Name}'...");
        return 0;
    }
}
