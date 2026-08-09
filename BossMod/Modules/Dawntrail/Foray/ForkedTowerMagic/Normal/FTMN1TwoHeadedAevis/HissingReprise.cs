namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class HissingReprise(BossModule module) : Components.GenericKnockback(module)
{
    private readonly PoisonBreath poison = module.FindComponent<PoisonBreath>()!;
    private readonly IceCluster ice = module.FindComponent<IceCluster>()!;
    private readonly LightningCluster lightning = module.FindComponent<LightningCluster>()!;
    private readonly HypothermalCombustionShock hyposhock = module.FindComponent<HypothermalCombustionShock>()!;
    private DateTime activation = default;
    private BitMask easterly;
    private BitMask westerly;
    private readonly AOEShapeCircle pShape = new(18f);
    private readonly AOEShapeCircle cShape = new(15f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        List<Knockback> kb = [with(1)];
        if (easterly[slot])
        {
            kb.Add(new(new(-880f, Arena.Center.Z), 21f, activation, kind: Kind.DirRight));
        }
        else if (westerly[slot])
        {
            kb.Add(new(new(-920f, Arena.Center.Z), 21f, activation, kind: Kind.DirLeft));
        }

        return CollectionsMarshal.AsSpan(kb);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EasterlyReprise or (uint)SID.WesterlyReprise)
        {
            activation = status.ExpireAt;
            var slot = Raid.FindSlot(actor.InstanceID);
            switch (status.ID)
            {
                case (uint)SID.EasterlyReprise:
                    easterly.Set(slot);
                    break;
                case (uint)SID.WesterlyReprise:
                    westerly.Set(slot);
                    break;
            }
        }
        base.OnStatusGain(actor, ref status);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BuffetEastern or (uint)AID.BuffetWestern)
        {
            easterly.Reset();
            westerly.Reset();
            activation = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var movement = movements[i];
            if (DestinationUnsafe(slot, actor, movement.to) || InsideAOE(slot, actor, movement.to))
            {
                hints.Add("About to be knocked into danger!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        var count = kbs.Length;
        if (count != 0)
        {
            // knockback can happen by itself, poison breath, or clusters
            // rect/circ slightly larger to avoid sus knockback
            var kb = kbs[0];
            if (!IsImmune(slot, kb.Activation))
            {
                var direction = new WDir(kb.Kind == Kind.DirLeft ? 20f : -20f, 0f);

                var p = GetPoisonPositions(slot, actor);
                var pCount = p.Length;
                if (pCount != 0)
                {
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusAOECircles(Arena.Center, direction, 19f, p, 19f, pCount), kb.Activation);
                }
                else
                {
                    var c = GetClusterPositions(slot, actor);
                    var cCount = c.Length;
                    if (cCount != 0)
                    {
                        hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusAOECircles(Arena.Center, direction, 19f, c, 16f, cCount), kb.Activation);
                    }
                    else
                    {
                        hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirection(Arena.Center, direction, 19f), kb.Activation);
                    }
                }
            }
        }
    }

    private WPos[] GetPoisonPositions(int slot, Actor actor)
    {
        List<WPos> pos = [];

        var poisons = poison.ActiveAOEs(slot, actor);
        var poisonCount = poisons.Length;
        for (var i = 0; i < poisonCount; i++)
        {
            var aoe = poisons[i];
            pos.Add(aoe.Origin);
        }

        return pos.ToArray();
    }

    private WPos[] GetClusterPositions(int slot, Actor actor)
    {
        List<WPos> pos = [];

        var ices = ice.ActiveAOEs(slot, actor);
        var iceCount = ices.Length;
        for (var i = 0; i < iceCount; i++)
        {
            var aoe = ices[i];
            pos.Add(aoe.Origin);
        }

        var lightnings = lightning.ActiveAOEs(slot, actor);
        var lightningCount = lightnings.Length;
        for (var i = 0; i < lightningCount; i++)
        {
            var aoe = lightnings[i];
            pos.Add(aoe.Origin);
        }

        var orbs = hyposhock.ActiveAOEs(slot, actor);
        var orbCount = orbs.Length;
        for (var i = 0; i < orbCount; i++)
        {
            var aoe = orbs[i];
            pos.Add(aoe.Origin);
        }

        return pos.ToArray();
    }

    private bool InsideAOE(int slot, Actor actor, WPos to)
    {
        var p = GetPoisonPositions(slot, actor);
        var pCount = p.Length;

        for (var i = 0; i < pCount; i++)
        {
            if (pShape.Check(to, p[i]))
            {
                return true;
            }
        }

        var c = GetClusterPositions(slot, actor);
        var cCount = c.Length;
        for (var i = 0; i < cCount; i++)
        {
            if (cShape.Check(to, c[i]))
            {
                return true;
            }
        }

        return false;
    }
}
