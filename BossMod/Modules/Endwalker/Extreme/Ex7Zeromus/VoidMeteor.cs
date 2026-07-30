namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

sealed class MeteorImpactProximity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpactProximity, 10f); // TODO: verify falloff

sealed class MeteorImpactCharge(BossModule module) : Components.GenericAOEs(module)
{
    struct PlayerState
    {
        public Actor? TetherSource;
        public int Order;
        public bool Stretched;
        public bool NonClipping;
    }

    private int _numTethers;
    private readonly List<WPos> _meteors = [with(18)];
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private AOEInstance[] _aoe = [];
    private readonly List<PolygonCustom> polygons = [with(18)];

    private const float _radius = 2f;
    private const float _ownThickness = 2f;
    private const float _otherThickness = 1f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (SourceIfActive(slot) != null)
        {
            ref var state = ref _playerStates[slot];
            if (!state.NonClipping)
            {
                hints.Add("Avoid other meteors!");
            }
            if (!state.Stretched)
            {
                hints.Add("Stretch the tether!");
            }
        }

        if (IsClippedByOthers(actor))
        {
            hints.Add("GTFO from charges!");
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => SourceIfActive(playerSlot) != null ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (SourceIfActive(pcSlot) is var source && source != null)
        {
            if (_aoe.Length == 0)
            {
                var count = _meteors.Count;
                var center = Arena.Center;
                var error = Arena.Bounds.MaxApproxError;
                var pos = source.Position;
                for (var i = 0; i < count; ++i)
                {
                    polygons.Add(new PolygonCustom(BuildShadowPolygon(pos - center, _meteors[i] - center, error)));
                }
                var aoe = new AOEShapeCustom([.. polygons]);
                _aoe = [new(aoe, center, shapeDistance: aoe.Distance(center, default))];
            }
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _meteors.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(_meteors[i], _radius, Colors.Object);
        }

        foreach (var (slot, target) in Raid.WithSlot(true, true, true))
        {
            if (SourceIfActive(slot) is var source && source != null)
            {
                var thickness = slot == pcSlot ? _ownThickness : _otherThickness;
                if (thickness != 0)
                {
                    var norm = (target.Position - source.Position).Normalized().OrthoL() * 2f;
                    var rot = Angle.FromDirection(target.Position - source.Position);
                    Arena.PathArcTo(target.Position, 2f, (rot + 90f.Degrees()).Rad, (rot - 90f.Degrees()).Rad);
                    Arena.PathLineTo(source.Position - norm);
                    Arena.PathLineTo(source.Position + norm);
                    MiniArena.PathStroke(true, _playerStates[slot].NonClipping ? Colors.Safe : default, thickness);
                    Arena.AddLine(source.Position, target.Position, _playerStates[slot].Stretched ? Colors.Safe : default, thickness);
                }
            }
        }

        // circle showing approximate min stretch distance; for second order, we might be forced to drop meteor there and die to avoid wipe
        if (SourceIfActive(pcSlot) is var pcSource && pcSource != null)
            Arena.ZoneCircleOutline(pcSource.Position, 26f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MeteorImpactProximity:
                _meteors.Add(caster.Position);
                break;
            case (uint)AID.MeteorImpactChargeNormal:
            case (uint)AID.MeteorImpactChargeClipping:
                _meteors.Add(spell.TargetXZ);
                ++NumCasts;
                var (closestSlot, closestPlayer) = Raid.WithSlot(true, true, true).Closest(spell.TargetXZ);
                if (closestPlayer != null)
                    _playerStates[closestSlot].TetherSource = null;
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.VoidMeteorCloseClipping or (uint)TetherID.VoidMeteorCloseGood or (uint)TetherID.VoidMeteorStretchedClipping or (uint)TetherID.VoidMeteorStretchedGood && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            ref var state = ref _playerStates[slot];
            if (state.TetherSource == null)
            {
                state.Order = ++_numTethers;
            }
            state.TetherSource = source;
            var id = source.Tether.ID;
            state.Stretched = id is (uint)TetherID.VoidMeteorStretchedClipping or (uint)TetherID.VoidMeteorStretchedGood;
            state.NonClipping = id is (uint)TetherID.VoidMeteorCloseGood or (uint)TetherID.VoidMeteorStretchedGood;
        }
    }

    private Actor? SourceIfActive(int slot)
    {
        ref var state = ref _playerStates[slot];
        return NumCasts switch
        {
            < 4 => state.Order < 4 ? state.TetherSource : null,
            < 8 => state.Order < 8 ? state.TetherSource : null,
            _ => null
        };
    }

    private static WPos[] BuildShadowPolygon(WDir sourceOffset, WDir meteorOffset, float maxerror)
    {
        var center = Ex7Zeromus.ArenaCenter;
        var toMeteor = meteorOffset - sourceOffset;
        var dirToMeteor = Angle.FromDirection(toMeteor);
        var halfAngle = Angle.Asin(_radius * 2f / toMeteor.Length());
        // intersection point is at dirToMeteor -+ halfAngle relative to source; relative to meteor, it is (dirToMeteor + 180) +- (90 - halfAngle)
        var dirFromMeteor = dirToMeteor + 180f.Degrees();
        var halfAngleFromMeteor = 90f.Degrees() - halfAngle;
        var circlearc = CurveApprox.CircleArc(_radius * 2, dirFromMeteor + halfAngleFromMeteor, dirFromMeteor - halfAngleFromMeteor, maxerror);
        var count = circlearc.Length;
        WPos[] vertices = new WPos[count + 2];
        for (var i = 0; i < count; ++i)
        {
            vertices[i] = meteorOffset + circlearc[i] + center;
        }
        vertices[count] = sourceOffset + 100f * (dirToMeteor + halfAngle).ToDirection() + center;
        vertices[count + 1] = sourceOffset + 100f * (dirToMeteor - halfAngle).ToDirection() + center;
        return vertices;
    }

    private static bool IsClipped(WPos source, WPos target, WPos position) => position.InCircle(target, _radius) || position.InRect(source, target - source, _radius);

    private bool IsClippedByOthers(Actor player)
    {
        foreach (var (i, p) in Raid.WithSlot(true, true, true).Exclude(player))
            if (SourceIfActive(i) is var src && src != null && IsClipped(src.Position, p.Position, player.Position))
                return true;
        return false;
    }
}

sealed class MeteorImpactExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpactExplosion, 10f);
