namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class ClamorousChaseBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly AOEShapeCircle circle = new(6f);
    private readonly uint[] order = new uint[PartyState.MaxPartySize];
    private readonly Ex6GuardianArkveldConfig _config = Service.Config.Get<Ex6GuardianArkveldConfig>();
    private bool isCCW;
    private bool active;
    public bool IconsAssigned;

    public override void Update()
    {
        if (!active)
        {
            return;
        }
        switch (_config.LimitCutHints)
        {
            case Ex6GuardianArkveldConfig.LimitCutStrategy.Circle:
                AddBaits(4);
                break;
            case Ex6GuardianArkveldConfig.LimitCutStrategy.EvenNorth:
                AddBaits(2);
                break;
        }

        void AddBaits(int amount)
        {
            if (CurrentBaits.Count >= amount)
            {
                return;
            }
            var party = Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len && CurrentBaits.Count < amount; ++i)
            {
                ref var p = ref party[i];
                var count = CurrentBaits.Count;
                if (order[p.Item1] == NumCasts + count + 1)
                {
                    CurrentBaits.Add(new(Module.PrimaryActor, p.Item2, circle, count == 0 ? WorldState.FutureTime(8.3d) : CurrentBaits.Ref(count - 1).Activation.AddSeconds(3d)));
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!active)
        {
            return;
        }
        base.DrawArenaForeground(pcSlot, pc);
        var spot = GetPositionHint(order[pcSlot]);
        if (spot is WPos s)
        {
            Arena.ZoneCircleOutline(s, 2f, Colors.Safe, 2f);
        }
    }

    private WPos? GetPositionHint(uint o)
    {
        if (o > 0)
        {
            switch (_config.LimitCutHints)
            {
                case Ex6GuardianArkveldConfig.LimitCutStrategy.Circle:
                    if (o <= NumCasts + 4 && o > NumCasts)
                    {
                        if (isCCW)
                        {
                            WPos pos = o switch
                            {
                                1 or 5 => new(80f, 100f),
                                2 or 6 => new(100f, 120f),
                                3 or 7 => new(120f, 100f),
                                _ => new(100f, 80f)
                            };
                            return pos;
                        }
                        else
                        {
                            WPos pos = o switch
                            {
                                1 or 5 => new(120f, 100f),
                                2 or 6 => new(100f, 120f),
                                3 or 7 => new(80f, 100f),
                                _ => new(100f, 80f)
                            };
                            return pos;
                        }
                    }
                    return Arena.Center;
                case Ex6GuardianArkveldConfig.LimitCutStrategy.EvenNorth:
                    if (o <= NumCasts + 2 && o > NumCasts)
                    {
                        return (o & 1) == 0 ? new(100f, 80f) : new(100f, 120f);
                    }
                    return (o & 1) == 0 ? new(100f, 90f) : new(100f, 110f);
            }
        }
        return null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!active)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
        var spot = GetPositionHint(order[slot]);
        if (spot is WPos s)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(s, 2f), WorldState.FutureTime(3d));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ClamorousChaseVisual1:
                isCCW = true;
                active = true;
                break;
            case (uint)AID.ClamorousChaseVisual2:
                active = true;
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && order[slot] > 0)
        {
            hints.Add($"Your order: {order[slot]}, current order: {NumCasts + 1}", false);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Icon1 and <= (uint)IconID.Icon8)
        {
            order[Raid.FindSlot(targetID)] = iconID - (uint)IconID.Icon1 + 1u;
            IconsAssigned = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClamorousChaseJump1 or (uint)AID.ClamorousChaseJump2)
        {
            if (++NumCasts == 8)
            {
                active = false;
            }
            if (CurrentBaits.Count != 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }
}

sealed class ClamorousChaseAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCone cone = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClamorousChaseCleave1 or (uint)AID.ClamorousChaseCleave2) // update AOE prediction to ensure correctness
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoe = [new(cone, loc, rot, Module.CastFinishAt(spell), shapeDistance: cone.Distance(loc, rot))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClamorousChaseCleave1 or (uint)AID.ClamorousChaseCleave2)
        {
            _aoe = [];
            ++NumCasts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Angle? offset = spell.Action.ID switch
        {
            (uint)AID.ClamorousChaseJump1 => -90f.Degrees(),
            (uint)AID.ClamorousChaseJump2 => 90f.Degrees(),
            _ => null
        };
        if (offset is Angle o)
        {
            var loc = spell.TargetXZ;
            var rot = spell.Rotation + o;
            _aoe = [new(cone, loc, rot, WorldState.FutureTime(2d), shapeDistance: cone.Distance(loc, rot))];
        }
    }
}
