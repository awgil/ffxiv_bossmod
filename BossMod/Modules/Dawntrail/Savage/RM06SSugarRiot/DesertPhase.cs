namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class SprayPain(BossModule module) : Components.StandardAOEs(module, AID.SprayPain1, new AOEShapeCircle(10), maxCasts: 10)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            if (++i > 5)
                yield return aoe with { Color = ArenaColor.AOE, Risky = false };
            else
                yield return aoe with { Color = ArenaColor.Danger };
        }
    }
}

class SprayPain2(BossModule module) : Components.StandardAOEs(module, AID.SprayPain2, new AOEShapeCircle(10));

class HeatingUpHints(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] SpreadAt = new DateTime[PartyState.MaxPartySize];
    private readonly DateTime[] StackAt = new DateTime[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HeatingUp:
                SpreadAt[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
                break;
            case SID.BurningUp:
                StackAt[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HeatingUp:
                SpreadAt[Raid.FindSlot(actor.InstanceID)] = default;
                break;
            case SID.BurningUp:
                StackAt[Raid.FindSlot(actor.InstanceID)] = default;
                break;
        }
    }

    public void Prune()
    {
        for (var i = 0; i < SpreadAt.Length; i++)
        {
            if (SpreadAt[i] > WorldState.CurrentTime && SpreadAt[i] < WorldState.FutureTime(20))
                SpreadAt[i] = default;
            if (StackAt[i] > WorldState.CurrentTime && StackAt[i] < WorldState.FutureTime(20))
                StackAt[i] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (SpreadAt[slot] != default)
            hints.Add($"Defamation in {(SpreadAt[slot] - WorldState.CurrentTime).TotalSeconds:f1}s", false);

        if (StackAt[slot] != default)
            hints.Add($"Party stack in {(StackAt[slot] - WorldState.CurrentTime).TotalSeconds:f1}s", false);
    }
}

class HeatingUp(BossModule module) : Components.UniformStackSpread(module, 6, 15, alwaysShowSpreads: true)
{
    public int NumCasts;
    public bool EnableAIHints;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HeatingUp:
                if (status.ExpireAt < WorldState.FutureTime(20))
                    AddSpread(actor, status.ExpireAt);
                break;
            case SID.BurningUp:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Brulee:
                NumCasts++;
                if (Spreads.Count > 0)
                    Spreads.RemoveAt(0);
                break;
            case AID.CrowdBrulee:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableAIHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Quicksand(BossModule module) : Components.GenericAOEs(module, warningText: "GTFO from quicksand!")
{
    public WPos? Center;
    public int AppearCount;
    public int ActivationCount;
    public int DisappearCount;
    public DateTime Activation;

    private BitMask StandInQuicksand;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.BlueTether)
            StandInQuicksand.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PuddingGraf)
            StandInQuicksand.Reset();
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x1F:
                    Activate(Arena.Center);
                    break;
                case 0x20:
                    Activate(new(100, 80));
                    break;
                case 0x21:
                    Activate(new(100, 120));
                    break;
                case 0x22:
                    Activate(new(120, 100));
                    break;
                case 0x23:
                    Activate(new(80, 100));
                    break;
            }
        }
        else if (state == 0x00080004 && index is >= 0x1F and <= 0x23)
        {
            DisappearCount++;
            Center = null;
        }
        else if (state == 0x00200010 && index is >= 0x1F and <= 0x23)
            ActivationCount++;
    }

    private void Activate(WPos p)
    {
        Center = p;
        Activation = WorldState.FutureTime(9);
        AppearCount++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Center).Select(c => new AOEInstance(new AOEShapeCircle(23), c, default, Activation, Color: StandInQuicksand[slot] ? ArenaColor.SafeFromAOE : 0, Risky: !StandInQuicksand[slot]));
}

class PuddingGraf(BossModule module) : Components.SpreadFromCastTargets(module, AID.PuddingGraf, 6);
class PuddingGrafAim(BossModule module) : BossComponent(module)
{
    private BitMask Aimers;
    private Quicksand? _qs;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.PinkTether)
            Aimers.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void Update()
    {
        _qs ??= Module.FindComponent<Quicksand>();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PuddingGraf)
            Aimers.Reset();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (GetBombLocation(pcSlot, pc) is { } bomb)
        {
            Arena.AddLine(pc.Position, bomb, ArenaColor.Danger);
            Arena.ActorProjected(pc.Position, bomb, pc.Rotation, ArenaColor.Danger);
        }
    }

    private WPos? GetBombLocation(int slot, Actor actor) => Aimers[slot] ? actor.Position + actor.Rotation.ToDirection() * 16 : null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (GetBombLocation(slot, actor) is { } bomb && _qs?.Center is { } qsCenter)
            hints.Add("Aim at quicksand!", !bomb.InCircle(qsCenter, 23));
    }
}
