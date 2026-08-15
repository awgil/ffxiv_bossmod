namespace BossMod;

// utility that recalculates ai hints based on different data sources (eg active bossmodule, etc)
// when there is no active bossmodule (eg in outdoor or on trash), we try to guess things based on world state (eg actor casts)
[SkipLocalsInit]
public sealed class AIHintsBuilder : IDisposable
{
    private const float RaidwideSize = 30f;
    private const float HalfWidth = 0.5f;
    public readonly Pathfinding.ObstacleMapManager Obstacles;
    private readonly WorldState _ws;
    private readonly BossModuleManager _bmm;
    private readonly ZoneModuleManager _zmm;
    private readonly EventSubscriptions _subscriptions;
    private readonly RotationSolverRebornModule? _rsr;
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = [];
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape)> _activeGazes = [];
    private readonly List<Actor> _invincible = [];
    private ArenaBoundsCircle? _activeFateBounds;

    private static readonly uint[] invincibleStatuses =
    [
        151u, 198u, 325u, 328u, 385u, 394u,
        469u, 529u, 592u, 656u, 671u, 775u,
        776u, 895u, 969u, 981u, 1240u, 1302u,
        1303u, 1567u, 1570u, 1697u, 1829u, 1936u,
        2413u, 2654u, 3012u, 3039u, 3052u, 3054u,
        4410u, 4175u
    ];
    private static readonly HashSet<uint> ignore = [27503u, 33626u]; // action IDs that the AI should ignore
    private static readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();
    private static readonly Dictionary<uint, (byte, byte, byte, uint, string, string, string, int, bool, uint)> _spellCache = [];

    public AIHintsBuilder(WorldState ws, BossModuleManager bmm, ZoneModuleManager zmm, RotationSolverRebornModule? rsr)
    {
        _ws = ws;
        _bmm = bmm;
        _zmm = zmm;
        _rsr = rsr;
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
        {
            hints.MaxCastTime = 0;
        }

        if (player != null)
        {
            var playerAssignment = _partyConfig[_ws.Party.Members[playerSlot].ContentId];
            var activeModule = _bmm.ActiveModule?.StateMachine.ActivePhase != null ? _bmm.ActiveModule : null;
            var outOfCombatPriority = activeModule?.ShouldPrioritizeAllEnemies == true ? 0 : AIHints.Enemy.PriorityUndesirable;
            var isTank = playerAssignment == PartyRolesConfig.Assignment.MT;
            if (!isTank && playerAssignment == PartyRolesConfig.Assignment.OT)
            {
                var anyOtherTank = false;
                foreach (var p in _ws.Party.WithoutSlot(false, false, true))
                {
                    if (p != player && p.Role == Role.Tank)
                    {
                        anyOtherTank = true;
                        break;
                    }
                }

                isTank = !anyOtherTank;
            }
            FillEnemies(hints, isTank, outOfCombatPriority);
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
        if (_rsr != null)
        {
            hints.RSRDesiredPositional = _rsr.IsInstalled ? _rsr.DesiredPositional : Positional.Any;
            var now = _ws.CurrentTime;
            var soon = now.AddSeconds(0.75d);
            var hasForbiddenDirection = hints.ForbiddenDirections.Count > 0;
            var forbiddenDirActivation = hasForbiddenDirection ? hints.ForbiddenDirections.Ref(0).activation : DateTime.MaxValue;
            var isPyretic = hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic;
            var finish = hints.ImminentSpecialMode.finish;
            var pyreticActivation = isPyretic ? hints.ImminentSpecialMode.activation : DateTime.MaxValue;

            if (_rsr.IsInstalled && (hasForbiddenDirection && forbiddenDirActivation < soon || isPyretic && pyreticActivation < soon))
            {
                var activationTime = hasForbiddenDirection && forbiddenDirActivation < soon ? forbiddenDirActivation : pyreticActivation;
                _ = Math.Max(0f, (float)(activationTime - now).TotalSeconds);
                _rsr.TriggerSpecialStateWithDuration(RotationSolverRebornModule.SpecialCommandType.NoCasting, finish != default ? (float)(finish - now).TotalSeconds : _config.PyreticThreshold);
                if (isPyretic)
                {
                    hints.ForceCancelCastMechanic = true;
                }
            }
        }
    }

    // Fill list of potential targets from world state
    private void FillEnemies(AIHints hints, bool playerIsDefaultTank, int priorityPassive = AIHints.Enemy.PriorityUndesirable)
    {
        var playerY = _ws.Party.Player() is { } p ? p.PosRot.Y : 0f;

        var allowedFateID = Utils.IsPlayerSyncedToFate(_ws) ? _ws.Client.ActiveFate.ID : default;

        foreach (var actor in _ws.Actors.Actors.Values)
        {
            if (!actor.IsTargetable || actor.IsAlly || actor.IsDead)
            {
                continue;
            }

            var index = actor.CharacterSpawnIndex;
            if (index is < 0 or >= AIHints.NumEnemies)
            {
                continue;
            }

            // determine default priority for the enemy
            var (priority, reason) = actor.FateID > 0 && actor.FateID != allowedFateID ? (AIHints.Enemy.PriorityInvincible, $"fate {actor.FateID} != ${allowedFateID}") // fate mob in fate we are NOT a part of can't be damaged at all
                : MathF.Abs(actor.PosRot.Y - playerY) > 12 ? (AIHints.Enemy.PriorityInvincible, "delta Y") // FIXME: this should be deleted once I work out how raycasting should interact with target priority
                : actor.PendingDead ? (AIHints.Enemy.PriorityPointless, "dying") // this mob is about to be dead, any attacks will likely ghost
                : actor.AggroPlayer ? (0, "aggro table") // enemies in our enmity list can be attacked, regardless of who they are targeting (since they are keeping us in combat)
                : actor.InCombat && _ws.Party.FindSlot(actor.TargetID) >= 0 ? (0, "attacking party") // we generally want to assist our party members (note that it includes allied npcs in duties)
                : (priorityPassive, "passive"); // this enemy is either not pulled yet or fighting someone we don't care about - try not to aggro it by default

            var enemy = hints.Enemies[index] = new(actor, priority, playerIsDefaultTank);

            // maybe unnecessary?
            if (actor.FateID > 0u && actor.FateID == allowedFateID && !Utils.IsBossFate(actor.FateID))
            {
                enemy.ForbidDOTs = true;
            }

            hints.PotentialTargets.Add(enemy);
        }
    }

    private void CalculateAutoHints(AIHints hints, Actor player)
    {
        var inFate = Utils.IsPlayerSyncedToFate(_ws);
        var fate = _ws.Client.ActiveFate;
        var center = inFate ? fate.Center : player.PosRot.XYZ();
        var (e, bitmap) = Obstacles.Find(center);
        var resolution = bitmap?.PixelSize ?? 0.5f;
        if (inFate)
        {
            hints.PathfindMapCenter = new(fate.Center.XZ());

            // if in a big fate with no obstacle map available, reduce resolution to avoid slowing down rasterization significantly
            if (bitmap == null)
            {
                resolution = fate.Radius switch
                {
                    > 60f => 2,
                    > 30f => 1,
                    _ => resolution
                };
            }

            hints.PathfindMapBounds = (_activeFateBounds ??= new ArenaBoundsCircle(fate.Radius, resolution));
            if (e != null && bitmap != null)
            {
                var originCell = (hints.PathfindMapCenter - e.Origin) / resolution;
                var originX = (int)originCell.X;
                var originZ = (int)originCell.Z;
                var halfSize = (int)(fate.Radius / resolution);
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
            hints.PathfindMapBounds = new ArenaBoundsRect(e.ViewWidth * resolution, e.ViewHeight * resolution, mapResolution: resolution); // note: we don't bother caching these bounds, they are very lightweight
            hints.PathfindMapObstacles = new(bitmap, new(originX - e.ViewWidth, originZ - e.ViewHeight, originX + e.ViewWidth, originZ + e.ViewHeight));
        }
        else
        {
            hints.PathfindMapCenter = player.Position.Rounded(5);
            // try to keep player near grid center
            var playerOffset = player.Position - hints.PathfindMapCenter;
            var playerOffetX = playerOffset.X;
            var playerOffsetZ = playerOffset.Z;
            var pathfindMapCenterX = hints.PathfindMapCenter.X;
            var pathfindMapCenterZ = hints.PathfindMapCenter.Z;
            if (playerOffetX < -1.25f)
            {
                pathfindMapCenterX -= 2.5f;
            }
            else if (playerOffetX > 1.25f)
            {
                pathfindMapCenterX += 2.5f;
            }

            if (playerOffsetZ < -1.25f)
            {
                pathfindMapCenterZ -= 2.5f;
            }
            else if (playerOffsetZ > 1.25f)
            {
                pathfindMapCenterZ += 2.5f;
            }

            hints.PathfindMapCenter = new(pathfindMapCenterX, pathfindMapCenterZ);
            // keep default bounds
        }

        foreach (var aoe in _activeAOEs.Values)
        {
            var caster = aoe.Caster.CastInfo!;
            var target = caster.LocXZ;
            var rot = caster.Rotation;
            var finishAt = _ws.FutureTime(caster.NPCRemainingTime);
            if (aoe.IsCharge)
            {
                hints.AddForbiddenZone(new SDRect(aoe.Caster.Position.Quantized(), target, ((AOEShapeRect)aoe.Shape).HalfWidth), finishAt, aoe.Caster.InstanceID);
            }
            else
            {
                hints.AddForbiddenZone(aoe.Shape, target, rot, finishAt);
            }
        }

        foreach (var gaze in _activeGazes.Values)
        {
            var target = gaze.Target?.Position ?? gaze.Caster.CastInfo!.LocXZ;
            var rot = gaze.Caster.CastInfo!.Rotation;
            var finishAt = _ws.FutureTime(gaze.Caster.CastInfo.NPCRemainingTime);
            if (gaze.Shape.Check(player.Position, target, rot))
            {
                hints.ForbiddenDirections.Add((Angle.FromDirection(target - player.Position), 45f.Degrees(), finishAt));
            }
        }

        var count = _invincible.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.SetPriority(_invincible[i], AIHints.Enemy.PriorityInvincible);
        }
    }

    private void OnCastStarted(Actor actor)
    {
        if (_bmm.ActiveModule?.StateMachine.ActivePhase != null) // no need to do all of this if it won't be used anyway
        {
            return;
        }

        if (actor.Type is not ActorType.Enemy and not ActorType.Helper || actor.IsAlly)
        {
            return;
        }

        var castInfo = actor.CastInfo!;
        var actionID = castInfo.Action.ID;
        if (ignore.Contains(actionID))
        {
            return;
        }

        var data = GetSpellData(actionID);

        // gaze
        if (data.VFX == 25u)
        {
            if (GuessShape(ref data, ref actor) is AOEShape sh)
            {
                _activeGazes[actor.InstanceID] = (actor, _ws.Actors.Find(castInfo.TargetID), sh);
            }

            return;
        }

        if (!castInfo.IsSpell() || data.CastType == 1)
        {
            return;
        }

        if (data.CastType is 2 or 5 && data.EffectRange >= RaidwideSize)
        {
            return;
        }

        if (GuessShape(ref data, ref actor) is not AOEShape shape)
        {
            Service.Log($"[AutoHints] Unknown cast type {data.CastType} for {castInfo.Action}");
            return;
        }
        var target = _ws.Actors.Find(castInfo.TargetID);
        _activeAOEs[actor.InstanceID] = (actor, target, shape, data.CastType == 8);
    }

    private void OnCastFinished(Actor actor)
    {
        _activeAOEs.Remove(actor.InstanceID);
        _activeGazes.Remove(actor.InstanceID);
    }

    private void OnStatusGain(Actor actor, int index)
    {
        var statusID = actor.Statuses[index].ID;
        for (var i = 0; i < 32; ++i)
        {
            if (statusID == invincibleStatuses[i])
            {
                _invincible.Add(actor);
                return;
            }
        }
    }

    private void OnStatusLose(Actor actor, int index)
    {
        var statusID = actor.Statuses[index].ID;
        for (var i = 0; i < 32; ++i)
        {
            if (statusID == invincibleStatuses[i])
            {
                _invincible.Remove(actor);
                return;
            }
        }
    }

    private static Angle DetermineConeAngle(ref (byte, byte, byte, uint RowId, string Name, string PathAlly, string Path, int Pos, bool Omen, uint) data)
    {
        if (!data.Omen)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return 180f.Degrees();
        }
        var path = data.Path;
        var pos = data.Pos;
        if (pos < 0 || pos + 6 > path.Length || !int.TryParse(path.AsSpan(pos + 3, 3), out var angle))
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{data.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        return angle.Degrees();
    }

    private static AOEShape? GuessShape(ref (byte CastType, byte EffectRange, byte XAxisModifier, uint, string, string, string, int, bool, uint) data, ref Actor actor) => data.CastType switch
    {
        2 => new AOEShapeCircle(data.EffectRange), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
        3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(ref data) * HalfWidth),
        4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius, data.XAxisModifier * HalfWidth),
        5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius),
        //6 => custom shapes
        //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
        8 => new AOEShapeRect(default, data.XAxisModifier * HalfWidth), // charges
        10 => new AOEShapeDonut(3, data.EffectRange),
        11 => new AOEShapeCross(data.EffectRange, data.XAxisModifier * HalfWidth),
        12 => new AOEShapeRect(data.EffectRange, data.XAxisModifier * HalfWidth),
        13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(ref data) * HalfWidth),
        _ => null
    };

    private static (byte CastType, byte EffectRange, byte XAxisModifier, uint RowId, string Name, string PathAlly, string path, int pos, bool Omen, uint VFX) GetSpellData(uint actionID)
    {
        if (_spellCache.TryGetValue(actionID, out var actionRow))
        {
            return actionRow;
        }

        var row = Service.LuminaRow<Lumina.Excel.Sheets.Action>(actionID);
        (byte, byte, byte, uint, string, string, string, int, bool, uint)? data;
        var omenPath = row!.Value.Omen.Value.Path.ToString();
        data = (row.Value.CastType, row.Value.EffectRange, row.Value.XAxisModifier, row.Value.RowId, row.Value.Name.ToString(), row.Value.Omen.Value.PathAlly.ToString(), omenPath, omenPath.IndexOf("fan", StringComparison.Ordinal), row.Value.Omen.ValueNullable != null, row.Value.VFX.RowId);
        return _spellCache[actionID] = data!.Value;
    }
}
