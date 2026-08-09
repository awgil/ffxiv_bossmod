namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class ForkedFury(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> targets = [with(6)];
    private bool active;
    private DateTime activation;
    private AOEInstance[] _aoe = [];
    private readonly WPos[] squarePositions = FTB4Magitaur.GetSquarePositions();
    private readonly WDir[] squareDirs = FTB4Magitaur.GetSquareAnglesDirs();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (!active)
        {
            return;
        }
        targets.Clear();
        List<Actor>[] squareActors = [[with(16)], [with(16)], [with(16)]];
        var players = new List<Actor>(48);
        var primaryPos = Module.PrimaryActor.Position;

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
            if (countS == 0)
            {
                continue;
            }

            (Actor actor, float distSq)[] distances = new (Actor, float)[countS];
            for (var j = 0; j < countS; ++j)
            {
                var actor = square[j];
                var distSq = (actor.Position - primaryPos).LengthSq();
                distances[j] = (actor, distSq);
            }

            int minIdx = 0, maxIdx = 0;
            for (var j = 1; j < countS; ++j)
            {
                ref readonly var dist = ref distances[j].distSq;
                if (dist < distances[minIdx].distSq)
                {
                    minIdx = j;
                }
                if (dist > distances[maxIdx].distSq)
                {
                    maxIdx = j;
                }
            }

            targets.Add(distances[minIdx].actor);
            if (maxIdx != minIdx)
            {
                targets.Add(distances[maxIdx].actor);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ForkedFuryVisual)
        {
            activation = Module.CastFinishAt(spell, 0.6d);
            var center = Arena.Center;
            var shape = FTB4Magitaur.GetCircleMinusSquares(center);
            _aoe = [new(shape, center, default, activation, shapeDistance: shape.Distance(center, default))];
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ForkedFury)
        {
            ++NumCasts;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
        {
            hints.Add($"Targets closest and farthest player per square!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(targets, Colors.Vulnerable);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!active || actor.IsDead)
            return;
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

            List<Actor> squareActors = [with(16)];
            var primaryPos = Module.PrimaryActor.Position;
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

            (Actor actor, float distSq)[] distances = new (Actor, float)[countS];

            for (var i = 0; i < countS; ++i)
            {
                var p = squareActors[i];
                var distSq = (p.Position - primaryPos).LengthSq();
                distances[i] = (p, distSq);
            }

            int minIdx = 0, maxIdx = 0;
            for (var j = 1; j < countS; ++j)
            {
                ref readonly var dist = ref distances[j].distSq;
                if (dist < distances[minIdx].distSq)
                    minIdx = j;
                if (dist > distances[maxIdx].distSq)
                    maxIdx = j;
            }

            var targetClose = distances[minIdx].actor;
            var targetFar = distances[maxIdx].actor;

            var isTarget = targetClose == actor || targetFar == actor;
            var isNonTankClose = targetClose.Role != Role.Tank;
            var isNonTankFar = targetFar.Role != Role.Tank;

            hints.Add(isNonTankClose ? "Bait close!" : isNonTankFar ? "Bait far!" : "Bait tankbusters!", !isTarget && (isNonTankClose || isNonTankFar));
        }
        else
        {
            var count = targets.Count;
            var isTarget = false;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i] == actor)
                {
                    isTarget = true;
                }
            }
            hints.Add("Stay between tanks!", isTarget);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
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
