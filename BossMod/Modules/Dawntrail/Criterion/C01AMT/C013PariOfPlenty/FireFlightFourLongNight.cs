namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class WitchHunt(BossModule module) : Components.GenericBaitAway(module, (uint)AID.FellFirework)
{
    private enum Proximity { Close, Far }
    private readonly List<Proximity> Order = [];
    private readonly DateTime[] Vulns = new DateTime[PartyState.MaxPartySize];
    private DateTime NextActivation;
    public bool Active => Order.Count > 0;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.PariOfPlenty && status.ID == (uint)SID.WitchHunt)
        {
            Order.Add(status.Extra == 0x3F4 ? Proximity.Close : Proximity.Far);
        }
        else if (status.ID == (uint)SID.DarkResistanceDown)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            Vulns[slot] = status.ExpireAt;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Order.Count > 0)
        {
            var hintString = new StringBuilder();

            for (var i = 0; i < Order.Count; ++i)
            {
                if (i > 0)
                {
                    hintString.Append(" -> ");
                }
                hintString.Append(Order[i].ToString());
            }

            hints.Add(hintString.ToString());
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireflightFourLongNightsRight or (uint)AID.FireflightFourLongNightsLeft)
        {
            NextActivation = WorldState.FutureTime(19.2d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (++NumCasts > 0 && Order.Count > 0)
            {
                Order.RemoveAt(0);
                if (Order.Count > 0)
                {
                    NextActivation = WorldState.FutureTime(3.1d);
                    NumCasts++;
                }
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (!Active)
        {
            return;
        }

        var targets = Raid.WithoutSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position);
        targets = Order[0] == Proximity.Close ? targets.Take(1) : targets.TakeLast(1);
        foreach (var t in targets)
        {
            CurrentBaits.Add(new(t, t, new AOEShapeCircle(3f), NextActivation));
        }

        ForbiddenPlayers = Raid.WithSlot(false, true, true).Where(p => Vulns[p.Item1] > NextActivation).Mask();
    }
}

class WitchHuntStack(BossModule module) : Components.GenericBaitStack(module, (uint)AID.FellFirework, true)
{
    private DateTime NextActivation;
    private int RemainingStacks;
    public bool Active => RemainingStacks > 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireflightFourLongNightsRight or (uint)AID.FireflightFourLongNightsLeft)
        {
            NextActivation = WorldState.FutureTime(17.0f + 2.2f);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.PariOfPlenty && status.ID == (uint)SID.WitchHunt)
        {
            RemainingStacks++;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FireWell)
        {
            if (RemainingStacks > 0)
            {
                RemainingStacks--;
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (!Active)
        {
            return;
        }

        var targets = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position);
        var target = targets.ElementAt(1);
        CurrentBaits.Add(new(target, target, new AOEShapeCircle(3), NextActivation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

class FireFlightFourLongNight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Angle> rotations = [];
    public bool startLeft = false;
    private IconID lastAction = 0;
    private Angle bossAngle;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.FireflightFourLongNightsRight)
        {
            startLeft = false;
        }
        else if (id == (uint)AID.FireflightFourLongNightsLeft)
        {
            startLeft = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflight or (uint)AID.WheelOfFireflight1 or (uint)AID.WheelOfFireflight2 or (uint)AID.WheelOfFireflight3)
        {
            if (rotations.Count > 0)
            {
                rotations.RemoveAt(0);
                ++NumCasts;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.TurnLeft or (uint)IconID.TurnRight or (uint)IconID.TurnRLeft or (uint)IconID.TurnRRight)
        {
            if (lastAction == 0)
            {
                if (startLeft)
                {
                    bossAngle = iconID is (uint)IconID.TurnRight or (uint)IconID.TurnRRight ? 180.Degrees() : default;
                }

                if (!startLeft)
                {
                    bossAngle = iconID is (uint)IconID.TurnRight or (uint)IconID.TurnRRight ? default : 180.Degrees();
                }
            }

            if (lastAction == (IconID)iconID)
            {
                bossAngle += 180.Degrees();
            }

            rotations.Add(bossAngle);
            lastAction = (IconID)iconID;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        var shown = 0;
        foreach (var rotation in rotations.Take(2))
        {
            var color = (shown == 0) ? Colors.Danger : default;
            aoes.Add(new AOEInstance(new AOEShapeCone(40f, 90f.Degrees()), Module.PrimaryActor.Position, rotation, default, color, shown == 0));
            shown++;
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

class CharmedFlightFourNights(BossModule module) : FireFlightFourLongNight(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.CharmedFlightFourNightsRight)
        {
            startLeft = false;
        }
        else if (id == (uint)AID.CharmedFlightFourNightsLeft)
        {
            startLeft = true;
        }
    }
}