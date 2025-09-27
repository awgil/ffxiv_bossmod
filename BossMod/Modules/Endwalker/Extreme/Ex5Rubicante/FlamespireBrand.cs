namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class Welts(BossModule module) : Components.GenericStackSpread(module, true)
{
    public enum Mechanic { StackFlare, Spreads, Done }

    public Mechanic NextMechanic;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BloomingWelt:
                Spreads.Add(new(actor, 15));
                break;
            case SID.FuriousWelt:
                Stacks.Add(new(actor, 6, 4, 4)); // TODO: verify flare falloff
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FuriousWelt:
                NextMechanic = Mechanic.Spreads;
                Stacks.Clear();
                Spreads.Clear();
                foreach (var t in Raid.WithoutSlot())
                    Spreads.Add(new(t, 6));
                break;
            case AID.StingingWelt:
                NextMechanic = Mechanic.Done;
                Spreads.Clear();
                break;
        }
    }
}

class Flamerake(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _offset;
    private DateTime _activation;

    private static readonly AOEShapeCross _first = new(20, 6);
    private static readonly AOEShapeRect _rest = new(8, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        if (NumCasts == 0)
        {
            yield return new(_first, Module.Center, _offset, _activation);
        }
        else
        {
            float offset = NumCasts == 1 ? 6 : 14;
            for (int i = 0; i < 4; ++i)
            {
                var dir = i * 90.Degrees() + _offset;
                yield return new(_rest, Module.Center + offset * dir.ToDirection(), dir, _activation);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlamerakeAOE11:
            case AID.FlamerakeAOE12:
                if (NumCasts == 0)
                {
                    ++NumCasts;
                    _activation = WorldState.FutureTime(2.1f);
                }
                break;
            case AID.FlamerakeAOE21:
            case AID.FlamerakeAOE22:
                if (NumCasts == 1)
                {
                    ++NumCasts;
                    _activation = WorldState.FutureTime(2.5f);
                }
                break;
            case AID.FlamerakeAOE31:
            case AID.FlamerakeAOE32:
                if (NumCasts == 2)
                {
                    ++NumCasts;
                    _activation = default;
                }
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 4)
        {
            switch (state)
            {
                // 00080004 when rotation ends
                case 0x00010001:
                case 0x00100010:
                    _offset = 45.Degrees();
                    _activation = WorldState.FutureTime(8.5f);
                    break;
                case 0x00200020:
                case 0x00800080:
                    _offset = 0.Degrees();
                    _activation = WorldState.FutureTime(8.5f);
                    break;
            }
        }
    }
}
