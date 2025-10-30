namespace BossMod;

// utility that recalculates ai hints based on different data sources (eg active bossmodule, etc)
// when there is no active bossmodule (eg in outdoor or on trash), we try to guess things based on world state (eg actor casts)
public sealed class AIHintsBuilder : IDisposable
{
    public const float RaidwideSize = 30;
    public const float MaxError = 2000f / 65535f; // TODO: this should really be handled by the rasterization itself...

    private readonly SmartRotationConfig _gazeConfig = Service.Config.Get<SmartRotationConfig>();
    private readonly AIHintsConfig _hintConfig = Service.Config.Get<AIHintsConfig>();

    public readonly Pathfinding.ObstacleMapManager Obstacles;
    private readonly WorldState _ws;
    private readonly BossModuleManager _bmm;
    private readonly ZoneModuleManager _zmm;
    private readonly EventSubscriptions _subscriptions;
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = [];
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape)> _activeGazes = [];
    private readonly List<Actor> _invincible = [];
    private ArenaBoundsCircle? _activeFateBounds;
    private bool AvoidGazes => _gazeConfig.Enabled && _gazeConfig.AvoidGazes;

    private float ConeFallback => Math.Clamp(_hintConfig.ConeFallbackAngle, 1, 180);

    private static readonly List<uint> InvincibleStatuses =
    [
        151,
        198,
        325,
        328,
        385,
        394,
        469,
        529,
        592,
        656,
        671,
        775,
        776,
        895,
        969,
        981,
        1240,
        1302,
        1303,
        1567,
        1570,
        1697,
        1829,
        1936,
        2413,
        2654,
        3012,
        3039,
        3052,
        3054,
        4410,
        4175
    ];

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
            ws.Actors.StatusGain.Subscribe(OnStatusGain),
            ws.Actors.StatusLose.Subscribe(OnStatusLose),
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
            var outOfCombatPriority = activeModule?.ShouldPrioritizeAllEnemies == true ? 0 : AIHints.Enemy.PriorityUndesirable;
            FillEnemies(hints, playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !_ws.Party.WithoutSlot().Any(p => p != player && p.Role == Role.Tank), outOfCombatPriority);
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
    private void FillEnemies(AIHints hints, bool playerIsDefaultTank, int priorityPassive = AIHints.Enemy.PriorityUndesirable)
    {
        var allowedFateID = Utils.IsPlayerSyncedToFate(_ws) ? _ws.Client.ActiveFate.ID : 0;
        foreach (var actor in _ws.Actors.Where(a => IsTargetable(a) && !a.IsAlly && !a.IsDead))
        {
            var index = actor.CharacterSpawnIndex;
            if (index < 0 || index >= hints.Enemies.Length)
                continue;

            // determine default priority for the enemy
            var priority = actor.FateID > 0 && actor.FateID != allowedFateID ? AIHints.Enemy.PriorityInvincible // fate mob in fate we are NOT a part of can't be damaged at all
                : actor.PendingDead ? AIHints.Enemy.PriorityPointless // this mob is about to be dead, any attacks will likely ghost
                : actor.AggroPlayer ? 0 // enemies in our enmity list can be attacked, regardless of who they are targeting (since they are keeping us in combat)
                : actor.InCombat && _ws.Party.FindSlot(actor.TargetID) >= 0 ? 0 // we generally want to assist our party members (note that it includes allied npcs in duties)
                : priorityPassive; // this enemy is either not pulled yet or fighting someone we don't care about - try not to aggro it by default

            var enemy = hints.Enemies[index] = new(actor, priority, playerIsDefaultTank);

            // maybe unnecessary?
            if (actor.FateID > 0 && actor.FateID == allowedFateID && !Utils.IsBossFate(actor.FateID))
                enemy.ForbidDOTs = true;

            hints.PotentialTargets.Add(enemy);
        }
    }

    private bool IsTargetable(Actor a) => a.IsTargetable; //|| a.Statuses.Any(s => s.ID is 676 or 1621 or 3997);

    private void CalculateAutoHints(AIHints hints, Actor player)
    {
        var inFate = Utils.IsPlayerSyncedToFate(_ws);
        var center = inFate ? _ws.Client.ActiveFate.Center : player.PosRot.XYZ();
        var (e, bitmap) = Obstacles.Find(center);
        var resolution = bitmap?.PixelSize ?? 0.5f;
        if (inFate)
        {
            hints.PathfindMapCenter = new(_ws.Client.ActiveFate.Center.XZ());

            // if in a big fate with no obstacle map available, reduce resolution to avoid destroying fps
            // fates don't need precise pathfinding anyway since they are just orange circle simulators
            if (bitmap == null)
            {
                resolution = _ws.Client.ActiveFate.Radius switch
                {
                    > 60 => 2,
                    > 30 => 1,
                    _ => resolution
                };
            }

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
            if (aoe.Caster.IsAlly)
                continue;

            var targetPos = aoe.Caster.CastInfo!.LocXZ;
            if (aoe.Target is { } tar && tar != aoe.Caster)
                targetPos = tar.Position;
            var rot = aoe.Caster.CastInfo!.Rotation;
            var finishAt = _ws.FutureTime(aoe.Caster.CastInfo.NPCRemainingTime);
            if (aoe.IsCharge)
            {
                // ignore charge AOEs that target player, as they presumably can't be avoided
                if (aoe.Target != player)
                    hints.AddForbiddenZone(ShapeContains.Rect(aoe.Caster.Position, targetPos, ((AOEShapeRect)aoe.Shape).HalfWidth), finishAt, aoe.Caster.InstanceID);
            }
            else if (aoe.Shape is AOEShapeCone cone)
            {
                // not sure how best to adjust cone shape distance to account for quantization error - we just pretend it is being cast from MaxError units "behind" the reported position and increase radius similarly
                var adjustedSourcePos = targetPos + rot.ToDirection() * -MaxError;
                var adjustedRadius = cone.Radius + MaxError * 2;
                hints.AddForbiddenZone(ShapeContains.Cone(adjustedSourcePos, adjustedRadius, rot, cone.HalfAngle), finishAt, aoe.Caster.InstanceID);
            }
            else
            {
                hints.AddForbiddenZone(aoe.Shape, targetPos, rot, finishAt, aoe.Caster.InstanceID);
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

        foreach (var inv in _invincible)
            hints.SetPriority(inv, AIHints.Enemy.PriorityInvincible);
    }

    private bool IsValidEnemy(Actor actor)
    {
        if (actor.IsAlly)
            return false;

        if (actor.Type == ActorType.Enemy)
            return true;

        if (_hintConfig.EnableHelperHints && actor.Type == ActorType.Helper)
            return true;

        return false;
    }

    private void OnCastStarted(Actor actor)
    {
        if (!IsValidEnemy(actor))
            return;

        if (Service.LuminaRow<Lumina.Excel.Sheets.Action>(actor.CastInfo!.Action.ID) is not { } data)
            return;

        if (_hintConfig.OmenSetting == AIHintsConfig.OmenBehavior.OmenOnly && data.Omen.RowId == 0)
            return;

        // gaze
        if (data.VFX.RowId == 25 && AvoidGazes)
        {
            if (GuessShape(data, actor) is AOEShape sh)
                _activeGazes[actor.InstanceID] = (actor, _ws.Actors.Find(actor.CastInfo.TargetID), sh);
            return;
        }

        if (!actor.CastInfo!.IsSpell() || data.CastType == 1)
            return;
        //if (data.Omen.Row == 0)
        //    return; // to consider: ignore aoes without omen, such aoes typically need a module to resolve...

        if (_hintConfig.OmenSetting != AIHintsConfig.OmenBehavior.AutomaticConservative && data.CastType is 2 or 5 && data.EffectRange >= RaidwideSize)
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

    private void OnStatusGain(Actor actor, int index)
    {
        if (InvincibleStatuses.Contains(actor.Statuses[index].ID))
            _invincible.Add(actor);
    }

    private void OnStatusLose(Actor actor, int index)
    {
        if (InvincibleStatuses.Contains(actor.Statuses[index].ID))
            _invincible.Remove(actor);
    }

    private AOEShape? GuessShape(Lumina.Excel.Sheets.Action data, Actor actor)
    {
        switch (data.CastType)
        {
            case 2:
                return new AOEShapeCircle(data.EffectRange + MaxError); // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
            case 3:
                return new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * 0.5f);
            case 4:
                return new AOEShapeRect(data.EffectRange + actor.HitboxRadius + MaxError, data.XAxisModifier * 0.5f + MaxError, MaxError);
            case 5:
                return new AOEShapeCircle(data.EffectRange + actor.HitboxRadius + MaxError);
            case 8:
                return new AOEShapeRect(1, data.XAxisModifier * 0.5f + MaxError);
            case 10:
                var inner = DetermineDonutInner(data);
                if (inner == 0 && _hintConfig.DonutFallback == AIHintsConfig.DonutFallbackBehavior.Ignore)
                    return null;
                return new AOEShapeDonut(MathF.Max(0, inner - MaxError), data.EffectRange + MaxError);
            case 11:
                return new AOEShapeCross(data.EffectRange + MaxError, data.XAxisModifier * 0.5f + MaxError);
            case 12:
                return new AOEShapeRect(data.EffectRange + MaxError, data.XAxisModifier * 0.5f + MaxError, MaxError);
            case 13:
                return new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f);

            case 6: // ???
            case 7: // new AOEShapeCircle(data.EffectRange), used for player ground-targeted circles like asylum
            default:
                return null;
        }
    }

    private Angle DetermineConeAngle(Lumina.Excel.Sheets.Action data)
    {
        if (data.Omen.ValueNullable is not { } omen || omen.RowId == 0)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return ConeFallback.Degrees();
        }
        var path = omen.Path.ToString();
        var pos = path.IndexOf("fan", StringComparison.Ordinal);
        if (pos < 0 || pos + 6 > path.Length || !int.TryParse(path.AsSpan(pos + 3, 3), out var angle))
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return ConeFallback.Degrees();
        }
        return angle.Degrees();
    }

    private static float DetermineDonutInner(Lumina.Excel.Sheets.Action data)
    {
        if (Utils.DetermineDonutInner(data, out var radius))
        {
            if (radius != null)
                return radius.Value;
            else
            {
                Service.Log($"[AutoHints] Can't determine inner radius from omen ({data.Omen.Value.Path}/{data.Omen.Value.PathAlly}) for {data.RowId} '{data.Name}'...");
                return 0;
            }
        }

        Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
        return 0;
    }
}
