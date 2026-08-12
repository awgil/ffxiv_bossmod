namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO figure out who gets the stack - its always support / DPS - but can we tell who?
class WheelOfFableFlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private Angle offset;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.WheelOfFableflightRight)
        {
            offset = -90.Degrees();
        }
        else if (spell.Action.ID == (uint)AID.WheelOfFableflightLeft)
        {
            offset = 90.Degrees();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FalseFlameRight)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.AOE));
        }

        else if (iconID == (uint)IconID.FalseFlameLeft)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, -offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.AOE));
        }

        else if (iconID == (uint)IconID.FalseFlameRRight)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.AOE));
        }

        else if (iconID == (uint)IconID.FalseFlameRLeft)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, -offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.AOE));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflight or (uint)AID.WheelOfFireflight1 or (uint)AID.WheelOfFireflight2 or (uint)AID.WheelOfFireflight3)
        {
            aoes.Clear();
            ++NumCasts;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class WheelofFableFlightStackSpread(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.KindledFlameStack)
        {
            AddStacks(Raid.WithoutSlot().Where(p => p.Class.IsSupport()));
        }
        else if (id == (uint)AID.ScatteredKindlingSpread)
        {
            foreach (var (i, player) in Raid.WithSlot())
            {
                AddSpread(player);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.KindledFlame1)
        {
            Stacks.Clear();
        }
        else if (id == (uint)AID.ScatteredKindling1)
        {
            Spreads.Clear();
        }
    }
}