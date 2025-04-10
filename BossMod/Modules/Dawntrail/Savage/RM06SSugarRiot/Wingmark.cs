namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class WingmarkKB(BossModule module) : Components.Knockback(module)
{
    private BitMask Players;
    private DateTime KnockbackFinishAt = DateTime.MaxValue;
    private WingmarkAdds? _adds;

    public bool Risky;

    public override void Update()
    {
        _adds ??= Module.FindComponent<WingmarkAdds>();
    }

    public bool StunHappened => KnockbackFinishAt != DateTime.MaxValue;
    public bool KnockbackFinished => WorldState.CurrentTime >= KnockbackFinishAt;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Wingmark)
            Players.Set(Raid.FindSlot(actor.InstanceID));
        if ((SID)status.ID == SID.Stun)
        {
            KnockbackFinishAt = WorldState.FutureTime(3);
            Players.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Players[slot])
            yield return new Source(actor.Position, 34, Direction: actor.Rotation, Kind: Kind.DirForward);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Risky && base.DestinationUnsafe(slot, actor, pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_adds?.SafeCorner() is { } p && actor.FindStatus(SID.Wingmark) is { } st)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(p, 34), st.ExpireAt);
            var angleToCorner = Angle.FromDirection(p - actor.Position);
            hints.ForbiddenDirections.Add((angleToCorner + 180.Degrees(), 178.Degrees(), st.ExpireAt));
        }
    }
}

class WingmarkAdds(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Adds = [];
    private DateTime Activation;
    public bool Risky;

    private BitMask DangerCorners;
    private static readonly WPos[] Corners = [new(80, 80), new(120, 80), new(120, 120), new(80, 120)];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.PinkTether or TetherID.BlueTether && !source.IsAlly)
        {
            Adds.Add(source);
            if (Activation == default)
                Activation = WorldState.FutureTime(13.2f);

            switch ((OID)source.OID)
            {
                case OID.PaintBomb:
                    MarkDanger(p => p.InCircle(source.Position, 15));
                    break;
                case OID.HeavenBomb:
                    MarkDanger(p => p.InCircle(source.Position + source.Rotation.ToDirection() * 16, 15));
                    break;
                case OID.MouthwateringMorbol:
                    MarkDanger(p => p.InCone(source.Position, source.Rotation, 50.Degrees()));
                    break;
                case OID.CandiedSuccubus:
                    MarkDanger(p => p.InCircle(source.Position, 30));
                    break;
            }
        }
    }

    private void MarkDanger(Func<WPos, bool> f)
    {
        for (var i = 0; i < Corners.Length; i++)
            if (f(Corners[i]))
                DangerCorners.Set(i);
    }

    public WPos? SafeCorner()
    {
        if (DangerCorners.NumSetBits() == 3)
            for (var i = 0; i < Corners.Length; i++)
                if (!DangerCorners[i])
                    return Corners[i];

        return null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HeavenBombBurst:
            case AID.PaintBombBurst:
            case AID.BadBreath:
            case AID.DarkMist:
                NumCasts++;
                Adds.Remove(caster);
                if (Adds.Count == 0)
                    DangerCorners.Reset();
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var add in Adds)
        {
            switch ((OID)add.OID)
            {
                case OID.PaintBomb:
                    yield return new AOEInstance(new AOEShapeCircle(15), add.Position, Activation: Activation, Risky: Risky);
                    break;
                case OID.HeavenBomb:
                    yield return new AOEInstance(new AOEShapeCircle(15), add.Position + add.Rotation.ToDirection() * 16, Activation: Activation, Risky: Risky);
                    break;
                case OID.CandiedSuccubus:
                    yield return new AOEInstance(new AOEShapeCircle(30), add.Position, Activation: Activation, Risky: Risky);
                    break;
                case OID.MouthwateringMorbol:
                    yield return new AOEInstance(new AOEShapeCone(50, 50.Degrees()), add.Position, add.Rotation, Activation: Activation, Risky: Risky);
                    break;
            }
        }
    }
}

class ColorClash(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Stack> SavedStacks = [];
    public int NumCasts;

    private string hint = "";

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColorClashParty:
                hint = "Next: party stacks";
                foreach (var h in Raid.WithoutSlot().Where(r => r.Class.GetRole() == Role.Healer).Take(2))
                    SavedStacks.Add(new(h, 6, 4, activation: WorldState.FutureTime(24.5f)));
                break;
            case AID.ColorClashPairs:
                hint = "Next: pairs";
                foreach (var h in Raid.WithoutSlot().Where(r => r.Class.GetRole3() == Role3.Support).Take(4))
                    SavedStacks.Add(new(h, 6, 2, 2, activation: WorldState.FutureTime(24.5f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ColorClash1 or AID.ColorClash2)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public void Activate()
    {
        Stacks.AddRange(SavedStacks);
        SavedStacks.Clear();
    }
}
