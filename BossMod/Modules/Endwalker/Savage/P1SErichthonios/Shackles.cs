namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to normal and fourfold shackles
class Shackles(BossModule module) : BossComponent(module)
{
    public int NumExpiredDebuffs { get; private set; }
    private bool _active;
    private BitMask _debuffsBlueImminent;
    private BitMask _debuffsBlueFuture;
    private BitMask _debuffsRedImminent;
    private BitMask _debuffsRedFuture;
    private BitMatrix _blueTetherMatrix;
    private BitMatrix _redTetherMatrix; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
    private BitMatrix _blueExplosionMatrix;
    private BitMatrix _redExplosionMatrix; // bit (8*i+j) is set if player i is inside explosion of player j; bit [i,i] is never set
    private readonly WPos[] _preferredPositions = new WPos[8];

    private const float _blueExplosionRadius = 4;
    private const float _redExplosionRadius = 8;
    private static uint TetherColor(bool blue, bool red) => blue ? (red ? Colors.Danger : Colors.Other1) : (red ? Colors.Other2 : Colors.PlayerGeneric);

    public override void Update()
    {
        _blueTetherMatrix = _redTetherMatrix = _blueExplosionMatrix = _redExplosionMatrix = default;
        var blueDebuffs = _debuffsBlueImminent | _debuffsBlueFuture;
        var redDebuffs = _debuffsRedImminent | _debuffsRedFuture;
        _active = (blueDebuffs | redDebuffs).Any();
        if (!_active)
            return; // nothing to do...

        // update tether matrices
        foreach ((var iSrc, var src) in Raid.WithSlot(false, true, true))
        {
            // blue => 3 closest
            if (blueDebuffs[iSrc])
            {
                _blueTetherMatrix[iSrc, iSrc] = true;
                foreach ((var iTgt, _) in Raid.WithSlot(false, true, true).Exclude(iSrc).SortedByRange(src.Position).Take(3))
                    _blueTetherMatrix[iTgt, iSrc] = true;
            }

            // red => 3 furthest
            if (redDebuffs[iSrc])
            {
                _redTetherMatrix[iSrc, iSrc] = true;
                foreach ((var iTgt, _) in Raid.WithSlot(false, true, true).Exclude(iSrc).SortedByRange(src.Position).TakeLast(3))
                    _redTetherMatrix[iTgt, iSrc] = true;
            }
        }

        // update explosion matrices and detect problems (has to be done in a separate pass)
        foreach ((var i, var actor) in Raid.WithSlot(false, true, true))
        {
            if (_blueTetherMatrix[i].Any())
                foreach ((var j, _) in Raid.WithSlot(false, true, true).InRadiusExcluding(actor, _blueExplosionRadius))
                    _blueExplosionMatrix[j, i] = true;

            if (_redTetherMatrix[i].Any())
                foreach ((var j, _) in Raid.WithSlot(false, true, true).InRadiusExcluding(actor, _redExplosionRadius))
                    _redExplosionMatrix[j, i] = true;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_blueTetherMatrix[slot].Any() && _redTetherMatrix[slot].Any())
        {
            hints.Add("Target of two tethers!");
        }
        if (_blueExplosionMatrix[slot].Any() || _redExplosionMatrix[slot].Any())
        {
            hints.Add("GTFO from explosion!");
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_preferredPositions[slot] != default)
        {
            movementHints.Add(actor.Position, _preferredPositions[slot], Colors.Safe);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        var drawBlueAroundMe = false;
        var drawRedAroundMe = false;
        foreach ((var i, var actor) in Raid.WithSlot(true, true, true))
        {
            var blueTetheredTo = _blueTetherMatrix[i];
            var redTetheredTo = _redTetherMatrix[i];
            Arena.Actor(actor, TetherColor(blueTetheredTo.Any(), redTetheredTo.Any()));

            // draw tethers
            foreach ((var j, var target) in Raid.WithSlot(true).Exclude(i).IncludedInMask(blueTetheredTo | redTetheredTo))
                Arena.AddLine(actor.Position, target.Position, TetherColor(blueTetheredTo[j], redTetheredTo[j]));

            // draw explosion circles that hit me
            if (_blueExplosionMatrix[pcSlot, i])
                Arena.ZoneCircleOutline(actor.Position, _blueExplosionRadius, Colors.Danger);
            if (_redExplosionMatrix[pcSlot, i])
                Arena.ZoneCircleOutline(actor.Position, _redExplosionRadius, Colors.Danger);

            drawBlueAroundMe |= _blueExplosionMatrix[i, pcSlot];
            drawRedAroundMe |= _redExplosionMatrix[i, pcSlot];
        }

        // draw explosion circles if I hit anyone
        if (drawBlueAroundMe)
            Arena.ZoneCircleOutline(pc.Position, _blueExplosionRadius, Colors.Danger);
        if (drawRedAroundMe)
            Arena.ZoneCircleOutline(pc.Position, _redExplosionRadius, Colors.Danger);

        // draw assigned spot, if any
        if (_preferredPositions[pcSlot] != new WPos())
            Arena.ZoneCircleOutline(_preferredPositions[pcSlot], 2, Colors.Safe);

    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ShacklesOfCompanionship0:
                _debuffsBlueFuture.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.ShacklesOfCompanionship1:
                _debuffsBlueFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 0, false);
                break;
            case (uint)SID.ShacklesOfCompanionship2:
                _debuffsBlueFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 1, false);
                break;
            case (uint)SID.ShacklesOfCompanionship3:
                _debuffsBlueFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 2, false);
                break;
            case (uint)SID.ShacklesOfCompanionship4:
                _debuffsBlueFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 3, false);
                break;
            case (uint)SID.ShacklesOfLoneliness0:
                _debuffsRedFuture.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.ShacklesOfLoneliness1:
                _debuffsRedFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 0, true);
                break;
            case (uint)SID.ShacklesOfLoneliness2:
                _debuffsRedFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 1, true);
                break;
            case (uint)SID.ShacklesOfLoneliness3:
                _debuffsRedFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 2, true);
                break;
            case (uint)SID.ShacklesOfLoneliness4:
                _debuffsRedFuture.Set(Raid.FindSlot(actor.InstanceID));
                AssignOrder(actor, 3, true);
                break;
            case (uint)SID.InescapableCompanionship:
                _debuffsBlueImminent.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.InescapableLoneliness:
                _debuffsRedImminent.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ShacklesOfCompanionship0:
            case (uint)SID.ShacklesOfCompanionship1:
            case (uint)SID.ShacklesOfCompanionship2:
            case (uint)SID.ShacklesOfCompanionship3:
            case (uint)SID.ShacklesOfCompanionship4:
                _debuffsBlueFuture.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.ShacklesOfLoneliness0:
            case (uint)SID.ShacklesOfLoneliness1:
            case (uint)SID.ShacklesOfLoneliness2:
            case (uint)SID.ShacklesOfLoneliness3:
            case (uint)SID.ShacklesOfLoneliness4:
                _debuffsRedFuture.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.InescapableCompanionship:
                _debuffsBlueImminent.Clear(Raid.FindSlot(actor.InstanceID));
                ++NumExpiredDebuffs;
                break;
            case (uint)SID.InescapableLoneliness:
                _debuffsRedImminent.Clear(Raid.FindSlot(actor.InstanceID));
                ++NumExpiredDebuffs;
                break;
        }
    }

    private void AssignOrder(Actor actor, int order, bool far)
    {
        var way1 = WorldState.Waymarks.GetFieldMark((int)Waymark.A + order);
        var way2 = WorldState.Waymarks.GetFieldMark((int)Waymark.N1 + order);
        if (way1 == null || way2 == null)
            return;

        var w1 = new WPos(way1.Value.XZ());
        var w2 = new WPos(way2.Value.XZ());
        var d1 = (w1 - Arena.Center).LengthSq();
        var d2 = (w2 - Arena.Center).LengthSq();
        var use1 = far ? d1 > d2 : d1 < d2;
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _preferredPositions[slot] = use1 ? w1 : w2;
    }
}
