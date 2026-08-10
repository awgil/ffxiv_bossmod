namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class RuneAxeStatus(BossModule module) : BossComponent(module)
{
    private int numStatuses;
    public readonly List<(int Order, Actor Actor, DateTime expireAt)> StatusBig = [];
    public readonly List<(int Order, Actor Actor, DateTime expireAt)> StatusSmall = [];
    public int CurrentOrder;
    public BitMask IsTargetAny;
    public BitMask IsTargetSmall;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RuinousRuneBig or (uint)AID.RuinousRuneSmall)
        {
            var curOrder = 3;
            UpdateCurrentOrder(StatusSmall);
            UpdateCurrentOrder(StatusBig);

            void UpdateCurrentOrder(List<(int Order, Actor, DateTime)> list)
            {
                var count = list.Count;
                var statuses = CollectionsMarshal.AsSpan(list);
                for (var i = 0; i < count; ++i)
                {
                    ref var s = ref statuses[i];
                    var order = s.Order;
                    if (curOrder > order)
                    {
                        curOrder = order;
                    }
                }
            }
            ++NumCasts;
            CurrentOrder = curOrder;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var expire = status.ExpireAt;
        var order = (expire - WorldState.CurrentTime).TotalSeconds switch
        {
            < 10d => 0,
            < 15d => 1,
            _ => 2
        };
        switch (status.ID)
        {
            case (uint)SID.PreyGreaterAxebit:
                AddTarget(StatusBig);
                break;
            case (uint)SID.PreyLesserAxebit:
                AddTarget(StatusSmall);
                IsTargetSmall.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
        void AddTarget(List<(int, Actor, DateTime)> list)
        {
            list.Add((order, actor, expire));
            ++numStatuses;
            IsTargetAny.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PreyGreaterAxebit:
                RemoveStatus(StatusBig);
                break;
            case (uint)SID.PreyLesserAxebit:
                RemoveStatus(StatusSmall);
                IsTargetSmall.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
        void RemoveStatus(List<(int, Actor, DateTime)> list)
        {
            var count = list.Count;
            var statuses = CollectionsMarshal.AsSpan(list);
            for (var i = 0; i < count; ++i)
            {
                ref var s = ref statuses[i];
                if (s.Item2 == actor)
                {
                    list.RemoveAt(i);
                    IsTargetAny[Raid.FindSlot(actor.InstanceID)] = false;
                    return;
                }
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (numStatuses > 7)
        {
            hints.Add($"Too many targets, mechanic potentially unsolveable!");
        }
    }
}

sealed class RuneAxeAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly RuneAxeStatus _status = module.FindComponent<RuneAxeStatus>()!;
    private AOEInstance[] _aoePrepare = [];
    private readonly List<AOEInstance>[] _aoeHintsForStatus = [[with(1)], [with(1)], [with(1)], [with(1)]];
    private readonly List<AOEInstance>[] _aoeHintsNoStatus = [[with(1)], [with(1)]];
    private bool prepare;
    public bool Show = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        if (prepare)
        {
            return _aoePrepare;
        }

        if (_status.IsTargetAny[slot])
        {
            var playerOrder = 3;
            var isSmall = _status.IsTargetSmall[slot];
            if (isSmall)
            {
                CheckListForOrder(_status.StatusSmall);
            }
            else
            {
                CheckListForOrder(_status.StatusBig);
            }
            void CheckListForOrder(List<(int Order, Actor Actor, DateTime)> list)
            {
                var count = list.Count;
                var span = CollectionsMarshal.AsSpan(list);
                for (var i = 0; i < count; ++i)
                {
                    ref var s = ref span[i];
                    if (s.Actor == actor)
                    {
                        playerOrder = s.Order;
                        break;
                    }
                }
            }
            if (_status.CurrentOrder == playerOrder)
            {
                return CollectionsMarshal.AsSpan(_aoeHintsForStatus[playerOrder == 2 && isSmall ? 3 : playerOrder]);
            }
        }
        else
        {
            if (_status.CurrentOrder is 0 or 2)
            {
                return CollectionsMarshal.AsSpan(_aoeHintsNoStatus[_status.CurrentOrder == 0 ? 0 : 1]);
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuneAxe)
        {
            var squares = FTB4Magitaur.GetSquarePositions();
            var angles = FTB4Magitaur.GetSquareAngles();
            prepare = true;
            var act = Module.CastFinishAt(spell);
            var center = Arena.Center;

            var bigSquares = FTB4Magitaur.GetSquares();
            Square[] defaultSq = [new Square(new(700f, -674f), 31.5f)];
            var circleMinusSquaresShape = new AOEShapeCustom(center, defaultSq, bigSquares);

            _aoePrepare = [new(circleMinusSquaresShape, center, default, act, shapeDistance: circleMinusSquaresShape.Distance(center, default))];
            var actOrder1 = act.AddSeconds(10.2d);
            var actOrder2 = act.AddSeconds(14.2d);
            var actOrder3 = act.AddSeconds(22.2d);

            var circleMinusSquaresSpreadShape = new AOEShapeCustom(center, defaultSq, [new Square(squares[0], 5f, angles[0]), new Square(squares[1], 5f, angles[1]),
                new Square(squares[2], 5f, angles[2])]);

            var polyBigSpread = PolygonClipper.GetCombinedPolygon(center, bigSquares).Offset(11f, Clipper2Lib.JoinType.Round);
            var aoeBigSpread = new AOEShapeCustom(center, [], skipPolygonInit: true);
            aoeBigSpread.ReplacePolygon(polyBigSpread, center);

            AddAOE(_aoeHintsForStatus[0], aoeBigSpread, center, actOrder1);
            AddAOE(_aoeHintsForStatus[2], aoeBigSpread, center, actOrder3);
            AddAOE(_aoeHintsNoStatus[0], circleMinusSquaresShape, center, actOrder1);
            AddAOE(_aoeHintsNoStatus[1], circleMinusSquaresShape, center, actOrder3);
            AddAOE(_aoeHintsForStatus[1], circleMinusSquaresSpreadShape, center, actOrder2);
            AddAOE(_aoeHintsForStatus[3], circleMinusSquaresSpreadShape, center, actOrder3);
            static void AddAOE(List<AOEInstance> list, AOEShape shape, WPos position, DateTime activation, Angle rotation = default)
            => list.Add(new(shape, position, rotation, activation, shapeDistance: shape.Distance(position, default)));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.PreyGreaterAxebit or (uint)SID.PreyLesserAxebit)
        {
            prepare = false;
            _aoePrepare = [];
        }
    }
}

sealed class RuneAxeSmallSpreadAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly RuneAxeStatus _status = module.FindComponent<RuneAxeStatus>()!;
    public bool Show = true;
    private readonly AOEShapeRect smallSquare = new(5f, 5f, 5f), bigSquare = new(10f, 10f, 10f);
    private readonly WPos[] squarePositions = FTB4Magitaur.GetSquarePositions();
    private readonly WDir[] squareDirs = FTB4Magitaur.GetSquareAnglesDirs();
    private readonly Angle[] squareAngles = FTB4Magitaur.GetSquareAngles();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        if (_status.IsTargetSmall[slot])
        {
            var playerOrder = 3;
            var count = _status.StatusSmall.Count;
            DateTime act = default;

            for (var i = 0; i < count; ++i)
            {
                var s = _status.StatusSmall[i];
                if (s.Actor == actor)
                {
                    playerOrder = s.Order;
                    act = s.expireAt;
                    break;
                }
            }
            if (_status.CurrentOrder != playerOrder)
            {
                return [];
            }
            var playersWithSameOrder = new List<Actor>();
            for (var i = 0; i < count; ++i)
            {
                var s = _status.StatusSmall[i];
                if (s.Order == playerOrder)
                {
                    playersWithSameOrder.Add(s.Actor);
                }
            }

            var aoes = new List<AOEInstance>(3);
            var countP = playersWithSameOrder.Count;
            for (var i = 0; i < countP; ++i)
            {
                var a = playersWithSameOrder[i];
                if (a == actor)
                {
                    continue;
                }
                InSquare(smallSquare, a.Position, aoes, act);
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        else
        {
            var aoes = new List<AOEInstance>(3);
            var countP = _status.StatusSmall.Count;
            for (var i = 0; i < countP; ++i)
            {
                var a = _status.StatusSmall[i];
                if (a.Order != _status.CurrentOrder)
                {
                    continue;
                }
                InSquare(bigSquare, a.Actor.Position, aoes, a.expireAt);
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        void InSquare(AOEShape shape, WPos position, List<AOEInstance> list, DateTime activation)
        {
            for (var j = 0; j < 3; ++j)
            {
                var pos = squarePositions[j];
                if (position.InSquare(pos, 10f, squareDirs[j]))
                {
                    list.Add(new(shape, pos, squareAngles[j], activation));
                    break;
                }
            }
        }
    }
}
