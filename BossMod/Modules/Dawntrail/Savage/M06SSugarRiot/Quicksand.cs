namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Quicksand(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(23f);
    private static readonly AOEShapeCircle circleInvert = new(23f, true);

    public AOEInstance[] AOE = [];
    private readonly QuicksandDoubleStyleHeavenBomb _baits1 = module.FindComponent<QuicksandDoubleStyleHeavenBomb>()!;
    private readonly QuicksandDoubleStylePaintBomb _baits2 = module.FindComponent<QuicksandDoubleStylePaintBomb>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOE.Length == 0)
        {
            return [];
        }
        var countBaits1 = _baits1.Targets != default;
        var countBaits2 = _baits2.Targets != default;
        if (!countBaits1 && !countBaits2 || _baits1.Targets[slot])
        {
            return AOE;
        }
        else if (_baits2.Targets[slot])
        {
            ref var aoe = ref AOE[0];
            return new AOEInstance[1] { aoe with { Shape = circleInvert, Color = Colors.SafeFromAOE, Risky = false } };
        }
        return [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1F and <= 0x23)
        {
            if (state == 0x00020001u)
            {
                var pos = index switch
                {
                    0x1F => Arena.Center,
                    0x20 => new(100f, 80f),
                    0x21 => new(100f, 120f),
                    0x22 => new(120f, 100f),
                    0x23 => new(80f, 100f),
                    _ => default
                };
                if (pos != default)
                    AOE = [new(circle, pos, default, WorldState.FutureTime(6d))];
            }
            else if (state == 0x00080004u)
            {
                AOE = [];
            }
        }
    }
}

sealed class QuicksandDoubleStylePaintBomb(BossModule module) : BossComponent(module)
{
    public BitMask Targets;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ActivateMechanicDoubleStyle2)
        {
            Targets.Set(Raid.FindSlot(source.InstanceID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.PaintBomb && id == 0x11D1)
        {
            Targets = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets[slot])
        {
            hints.Add("Place bomb inside quicksand");
        }
    }
}

sealed class QuicksandDoubleStyleHeavenBomb(BossModule module) : Components.GenericKnockback(module, default, stopAfterWall: true)
{
    public BitMask Targets;
    private Quicksand? _qs = module.FindComponent<Quicksand>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Targets[slot])
        {
            return new Knockback[1] { new(actor.Position, 16f, default, default, actor.Rotation, Kind.DirForward, ignoreImmunes: true) };
        }
        return [];
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ActivateMechanicDoubleStyle1)
        {
            Targets.Set(Raid.FindSlot(source.InstanceID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.HeavenBomb && id == 0x11D1)
        {
            Targets = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CollectionsMarshal.AsSpan(CalculateMovements(slot, actor));
        if (movements.Length != 0)
        {
            ref var m = ref movements[0];
            hints.Add("Aim bomb into quicksand!", DestinationUnsafe(slot, actor, m.to));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Arena.InBounds(pos))
            return true;
        _qs ??= Module.FindComponent<Quicksand>();
        if (_qs != null)
        {
            var aoes = _qs.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (!aoe.Check(pos))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

sealed class PaintBombHeavenBomb(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Burst1, (uint)AID.Burst2], 10f);
sealed class PuddingGraf(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PuddingGraf, 6f);
