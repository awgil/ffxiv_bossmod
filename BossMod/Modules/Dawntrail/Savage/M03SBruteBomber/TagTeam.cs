namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

sealed class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(2)];
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];
    private readonly AOEInstance[][] _safespot = new AOEInstance[PartyState.MaxPartySize][];
    private static readonly AOEShapeRect rect = new(70f, 17f);
    private ConeHA[] cone = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        if (_tetherSource[slot] != null)
        {
            var safespot = _safespot[slot];
            if (safespot == null)
            {
                var safeShapes = new RectangleSE[1];
                var dangerShapes = new RectangleSE[1];
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var isSafe = _tetherSource[slot]!.Position.AlmostEqual(aoe.Origin, 25f);
                    var shape = new RectangleSE(aoe.Origin, aoe.Origin + rect.LengthFront * aoe.Rotation.ToDirection(), rect.HalfWidth);

                    if (isSafe)
                    {
                        safeShapes[0] = shape;
                    }
                    else
                    {
                        dangerShapes[0] = shape;
                    }
                }

                ref var aoe0 = ref aoes[0];
                var center = Arena.Center;
                var aoeShape = new AOEShapeCustom(center, safeShapes, dangerShapes, cone, cone.Length != 0 ? OperandType.Intersection : OperandType.Union, invertForbiddenZone: true);
                safespot = [new(aoeShape, center, default, aoe0.Activation, Colors.SafeFromAOE, shapeDistance: aoeShape.InvertedDistance(center, default))];
            }
            return safespot;
        }
        return CollectionsMarshal.AsSpan(AOEs);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tetherSource[slot] == null)
        {
            base.AddHints(slot, actor, hints);
        }
        else if (_safespot[slot] is AOEInstance[] aoes)
        {
            ref var aoe = ref aoes[0];
            hints.Add("Go to correct spot!", !aoe.Check(actor.Position));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainDeathmatch && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            _tetherSource[slot] = source;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
                AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.FusesOfFuryMurderousMist:
                cone = [new(spell.LocXZ, 40f, spell.Rotation, 135f.Degrees())];
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
            case (uint)AID.TagTeamLariatComboSecondRAOE:
            case (uint)AID.TagTeamLariatComboSecondLAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondRAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondLAOE:
                ++NumCasts;
                new Span<Actor?>(_tetherSource, 0, PartyState.MaxPartySize).Clear();
                var index = -1;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                var len = aoes.Length;
                var pos = spell.LocXZ;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Origin.AlmostEqual(pos, 1f))
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    ReportError($"Failed to find AOE for {spell.LocXZ}");
                }
                else if (id is (uint)AID.TagTeamLariatComboFirstRAOE or (uint)AID.TagTeamLariatComboFirstLAOE or (uint)AID.FusesOfFuryLariatComboFirstRAOE or (uint)AID.FusesOfFuryLariatComboFirstLAOE)
                {
                    ref var aoe = ref aoes[index];
                    aoe.Origin = (Arena.Center - (aoe.Origin - Arena.Center)).Quantized();
                    aoe.Rotation += 180f.Degrees();
                    aoe.Activation = WorldState.FutureTime(4.3d);
                }
                else
                {
                    AOEs.RemoveAt(index);
                }
                break;
        }
    }
}
