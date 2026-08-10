namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class VirtualShiftIce(BossModule module) : Components.GenericAOEs(module, default, "GTFO from broken bridge!")
{
    private readonly List<AOEInstance> _unsafeBridges = [with(4)];
    private readonly List<Rectangle> _destroyedBridges = [new(new(95f, 96f), 3f, 2f), new(new(95f, 104f), 3f, 2f), new(new(105f, 96f), 3f, 2f), new(new(95f, 104f), 3f, 2f)];

    private static readonly AOEShapeRect _shape = new(2, 3, 2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_unsafeBridges);

    public override void OnMapEffect(byte index, uint state)
    {
        WPos center = index switch
        {
            0x04 => new(95f, 96f),
            0x05 => new(95f, 104f),
            0x06 => new(105f, 96f),
            0x07 => new(105f, 104f),
            _ => default
        };
        if (center == default)
        {
            return;
        }

        switch (state)
        {
            case 0x00020001u: // destroyed bridge respawns
                var count = _destroyedBridges.Count;
                var bridge = CollectionsMarshal.AsSpan(_destroyedBridges);
                for (var i = 0; i < count; ++i)
                {
                    if (bridge[i].Center == center)
                    {
                        _destroyedBridges.RemoveAt(i);
                        break;
                    }
                }
                UpdateArena();
                break;
            case 0x00200010u: // bridge gets damaged
                _unsafeBridges.Add(new(_shape, center));
                break;
            case 0x00400001u: // damaged bridge gets repaired
            case 0x00080004u: // bridges despawn
                RemoveUnsafeBridges(center);
                break;
            case 0x00800004u: // bridge gets destroyed
                RemoveUnsafeBridges(center);
                _destroyedBridges.Add(new(center, 3, 2));
                UpdateArena();
                break;
        }

        void RemoveUnsafeBridges(WPos origin)
        {
            var count = _unsafeBridges.Count;
            var bridge = CollectionsMarshal.AsSpan(_unsafeBridges);
            for (var i = 0; i < count; ++i)
            {
                if (bridge[i].Origin == origin)
                {
                    _unsafeBridges.RemoveAt(i);
                    break;
                }
            }
        }

        void UpdateArena() => Arena.Bounds = new ArenaBoundsCustom(Ex3QueenEternal.GetIceRects(), [.. _destroyedBridges]);
    }
}

sealed class LawsOfIce(BossModule module) : Components.StayMove(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LawsOfIce)
        {
            PlayerState state = new(Requirement.Move, WorldState.FutureTime(4.2d));
            SetState(Raid.FindSlot(actor.InstanceID), ref state);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LawsOfIceAOE)
        {
            ++NumCasts;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
        {
            ClearState(Raid.FindSlot(actor.InstanceID));
        }
    }
}

sealed class Rush(BossModule module) : Components.GenericBaitAway(module)
{
    public DateTime Activation;
    private BitMask _unstretched;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    private readonly AOEShapeRect _shapeTether = new(80f, 2f);
    private readonly AOEShapeCircle _shapeUntethered = new(8f); // if there is no tether, pillar will just explode; this can happen if someone is dead

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_unstretched[slot])
        {
            hints.Add("Stretch tether!");
        }
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            Arena.Actor(b.Source, Colors.Object, true);
            if (b.Target == pc)
            {
                Arena.AddLine(b.Source.Position, b.Target.Position, _unstretched[pcSlot] ? 0 : Colors.Safe);
                Arena.ZoneCircleOutline(SafeSpot(b.Source, _config), 1f, Colors.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RushFirst or (uint)AID.RushSecond)
        {
            Activation = Module.CastFinishAt(spell, 0.2d);
            if (!CurrentBaits.Any(b => b.Source == caster))
            {
                CurrentBaits.Add(new(caster, caster, _shapeUntethered, Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RushFirstAOE or (uint)AID.RushSecondAOE or (uint)AID.RushFirstFail or (uint)AID.RushSecondFail)
        {
            ++NumCasts;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RushShort or (uint)TetherID.RushLong && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            RemoveBait(source);
            CurrentBaits.Add(new(source, target, _shapeTether, Activation));

            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                _unstretched[slot] = tether.ID == (uint)TetherID.RushShort;
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RushShort or (uint)TetherID.RushLong)
        {
            RemoveBait(source);
            CurrentBaits.Add(new(source, source, _shapeUntethered, Activation));

            _unstretched.Clear(Raid.FindSlot(tether.Target));
        }
    }

    private void RemoveBait(Actor source)
    {
        var count = CurrentBaits.Count;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            if (baits[i].Source == source)
            {
                CurrentBaits.RemoveAt(i);
                break;
            }
        }
    }

    private static WPos SafeSpot(Actor source, Ex3QueenEternalConfig config)
    {
        var center = new WPos(100f, 100f);
        var pos = source.Position;
        var safeSide = pos.X > center.X ? -1 : +1;
        var offX = Math.Abs(pos.X - center.X);
        if (pos.Z > 110f)
        {
            // first order
            var inner = offX < 6f;
            return center + new WDir(safeSide * (inner ? 15f : 10f), -19f);
        }
        else
        {
            // second order
            var central = pos.Z < 96f;
            var strat = !config.SideTethersCrossStrategy ? (central ? -2f : 9f) : (central ? 9f : -9f);
            return center + new WDir(safeSide * 15f, strat);
        }
    }
}

sealed class IceDart(BossModule module) : Components.BaitAwayTethers(module, 16f, (uint)TetherID.IceDart, (uint)AID.IceDart)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            ForbiddenPlayers[Raid.FindSlot(spell.MainTargetID)] = true;
        }
    }
}

sealed class RaisedTribute(BossModule module) : Components.GenericWildCharge(module, 4f, (uint)AID.RaisedTribute, 80f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RaisedTribute)
        {
            Source = actor;
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var member = ref party[i];
                PlayerRoles[i] = member.InstanceID == targetID ? PlayerRole.Target : member.Tether.ID != 0 ? PlayerRole.Avoid : PlayerRole.Share;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Source = null;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.IceDart && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0 && PlayerRoles[slot] != PlayerRole.Target)
            PlayerRoles[slot] = PlayerRole.Avoid;
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.IceDart && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0 && PlayerRoles[slot] != PlayerRole.Target)
            PlayerRoles[slot] = PlayerRole.Share;
    }
}
