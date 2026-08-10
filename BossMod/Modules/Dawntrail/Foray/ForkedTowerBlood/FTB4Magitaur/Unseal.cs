namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class Unseal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> targets = [with(6)];
    private bool? isClose;
    private DateTime activation;
    private AOEInstance[] _aoe = [];
    private readonly WPos[] squarePositions = FTB4Magitaur.GetSquarePositions();
    private readonly WDir[] squareDirs = FTB4Magitaur.GetSquareAnglesDirs();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (isClose is not bool close)
        {
            return;
        }
        targets.Clear();
        List<Actor>[] squareActors = [[with(16)], [with(16)], [with(16)]];

        var primaryPos = Module.PrimaryActor.Position;

        var players = new List<Actor>(48);
        // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
        // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
        // caching removed players is also pointless since the player position no longer gets updated when this happens
        foreach (var a in Module.WorldState.Actors.Actors.Values)
        {
            if (a.OID == default && !a.IsDead)
            {
                players.Add(a);
            }
        }

        var count = players.Count;

        for (var i = 0; i < count; ++i)
        {
            var a = players[i];
            for (var j = 0; j < 3; ++j)
            {
                if (a.Position.InSquare(squarePositions[j], 10f, squareDirs[j]))
                {
                    squareActors[j].Add(a);
                    break;
                }
            }
        }

        for (var i = 0; i < 3; ++i)
        {
            var square = squareActors[i];
            var countS = square.Count;
            (Actor actor, float distSq)[] distances = new (Actor, float)[countS];

            for (var j = 0; j < countS; ++j)
            {
                var p = square[j];
                var distSq = (p.Position - primaryPos).LengthSq();
                distances[j] = (p, distSq);
            }

            var counttargets = Math.Min(2, countS);
            for (var j = 0; j < counttargets; ++j)
            {
                var selIdx = j;
                for (var k = j + 1; k < countS; ++k)
                {
                    var distJSq = distances[k].distSq;
                    var distSelIdx = distances[selIdx].distSq;
                    var isBetter = close ? distJSq < distSelIdx : distJSq > distSelIdx;
                    if (isBetter)
                        selIdx = k;
                }

                if (selIdx != j)
                {
                    (distances[selIdx], distances[j]) = (distances[j], distances[selIdx]);
                }

                targets.Add(distances[j].actor);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.Unseal1 or (uint)AID.Unseal2)
        {
            isClose = id == (uint)AID.Unseal2;
            activation = Module.CastFinishAt(spell, 8d);
            SetAOE();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (isClose == null && spell.Action.ID == (uint)AID.ForkedFury)
        {
            isClose = Module.PrimaryActor.FindStatus((uint)SID.Unsealed) is ActorStatus status && status.Extra == 0x353;
            activation = Module.CastFinishAt(spell, 6.2d);
            SetAOE();
        }
    }

    private void SetAOE()
    {
        var pos = Arena.Center;
        var shape = FTB4Magitaur.GetCircleMinusSquares(pos);
        _aoe = [new(shape, pos, default, activation, shapeDistance: shape.Distance(pos, default))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Attack1 or (uint)AID.Attack2)
        {
            ++NumCasts;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (isClose is bool close)
        {
            var target = close ? "closest" : "farthest";
            hints.Add($"Targets two {target} players per square!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(targets, Colors.Vulnerable);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (isClose is not bool close || actor.IsDead)
        {
            return;
        }
        if (actor.Role == Role.Tank)
        {
            var inSquare = -1;
            for (var i = 0; i < 3; ++i)
            {
                if (actor.Position.InSquare(squarePositions[i], 10f, squareDirs[i]))
                {
                    inSquare = i;
                    break;
                }
            }
            if (inSquare == -1)
            {
                return;
            }
            List<Actor> squareActors = [with(16)];
            var primaryPos = Module.PrimaryActor.Position;

            var players = new List<Actor>(48);
            // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
            // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
            // caching removed players is also pointless since the player position no longer gets updated when this happens
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default && !a.IsDead)
                {
                    players.Add(a);
                }
            }

            var count = players.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = players[i];
                if (a.Position.InSquare(squarePositions[inSquare], 10f, squareDirs[inSquare]))
                {
                    squareActors.Add(a);
                }
            }

            var countS = squareActors.Count;
            if (countS < 2)
                return;

            var targetsOnSquare = new List<Actor>(2);

            (Actor actor, float distSq)[] distances = new (Actor, float)[countS];

            for (var i = 0; i < countS; ++i)
            {
                var p = squareActors[i];
                var distSq = (p.Position - primaryPos).LengthSq();
                distances[i] = (p, distSq);
            }

            var counttargets = Math.Min(2, countS);
            for (var i = 0; i < counttargets; ++i)
            {
                var selIdx = i;
                for (var j = i + 1; j < countS; ++j)
                {
                    var distJSq = distances[j].distSq;
                    var distSelIdx = distances[selIdx].distSq;
                    var isBetter = close ? distJSq < distSelIdx : distJSq > distSelIdx;
                    if (isBetter)
                        selIdx = j;
                }

                if (selIdx != i)
                {
                    (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
                }

                targetsOnSquare.Add(distances[i].actor);
            }

            var countT = targetsOnSquare.Count;
            var isTarget = false;
            var anyNonTank = false;
            for (var i = 0; i < countT; ++i)
            {
                var t = targetsOnSquare[i];
                if (t == actor)
                {
                    isTarget = true;
                }
                else if (t.Role != Role.Tank)
                {
                    anyNonTank = true;
                }
            }

            hints.Add(close ? "Bait close!" : "Bait far!", !isTarget && anyNonTank);
        }
        else
        {
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i] == actor)
                {
                    hints.Add(close ? "Stay away!" : "Go close!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (isClose != null)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var count = targets.Count;
            BitMask mask = default;
            for (var i = 0; i < count; ++i)
            {
                if (Raid.FindSlot(targets[i].InstanceID) is var slot2 && slot2 >= 0)
                {
                    mask.Set(slot2);
                }
            }
            hints.AddPredictedDamage(mask, activation, AIHints.PredictedDamageType.Tankbuster);
        }
    }
}
