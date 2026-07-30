namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class PortentousCometeor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PortentousCometeor, 43f);

sealed class PortentousCometeorBait(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true, centerAtTarget: true)
{
    private readonly AOEShapeCircle circle = new(43f);
    private Actor? meteor;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PortentousCometeor)
        {
            meteor = actor;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CraterLater)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(12d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PortentousCometeor)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsBaitTarget(actor) && meteor != null)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(meteor.Position, 1f), CurrentBaits.Ref(0).Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Take bait to side with meteor!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (IsBaitTarget(pc) && meteor != null)
        {
            Arena.ZoneCircleOutline(meteor.Position, 2f, Colors.Vulnerable, 2f);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (IsBaitTarget(actor) && meteor != null)
        {
            movementHints.Add(actor.Position, meteor.Position, Colors.Safe);
        }
    }
}

sealed class PortentousCometKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly AOEShapeCircle circle = new(4f);
    private readonly List<(Actor target, Angle dir)> targets = [with(4)];
    private DateTime activation;
    private readonly LandingKnockback _kb = module.FindComponent<LandingKnockback>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kb.Casters.Count != 0)
            return [];
        var count = targets.Count;
        if (count == 0)
            return [];
        var pos = actor.Position;
        var center = Arena.Center;
        var dirRect = new WDir(default, 1f);
        var isTarget = false;
        for (var i = 0; i < count; ++i)
        {
            if (targets[i].target == actor)
            {
                isTarget = true;
                break;
            }
        }
        for (var i = 0; i < count; ++i)
        {
            var kb = targets[i];
            if (isTarget && kb.target == actor || !isTarget && pos.InCircle(kb.target.Position, 4f)) // only draw one knockback since they cant be chained, give priority to actor's own knockback
            {
                var knockback = new Knockback[1];
                var dir = kb.dir.ToDirection();
                var distance = 13f;
                if ((pos + 13f * dir).InRect(center, dirRect, 3f, 3f, 15f))
                {
                    distance = (center + 6f * dir - pos).Length();
                }
                // the knockback range for knockbacks away from meteor side does not seem very consistent. Theory: if the knockback ends up inside the demon tablet, it gets extended to land 3y behind the wall
                knockback[0] = new Knockback(kb.target.Position, distance, activation, circle, kb.dir, kind: Kind.DirForward, ignoreImmunes: true);
                return knockback;
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2 && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            targets.Add((target, id == (uint)AID.PortentousComet1 ? 180f.Degrees() : default));
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2)
        {
            var count = targets.Count;
            var id = spell.TargetID;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i].target.InstanceID == id)
                {
                    targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class PortentousComet(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumFinishedStacks;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2 && WorldState.Actors.Find(spell.TargetID) is Actor t)
        {
            Stacks.Add(new(t, 4f, 12, 12, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2)
        {
            var count = Stacks.Count;
            var id = spell.TargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                ref var stack = ref stacks[i];
                if (stack.Target.InstanceID == id)
                {
                    Stacks.RemoveAt(i);
                    ++NumFinishedStacks;
                    return;
                }
            }
        }
    }
}
