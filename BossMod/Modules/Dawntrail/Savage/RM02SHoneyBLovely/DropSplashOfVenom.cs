namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

class DropSplashOfVenom(BossModule module) : Components.UniformStackSpread(module, 6, 6, 2, 2, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Pairs, Spread }

    public Mechanic NextMechanic;
    public DateTime Activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NextMechanic != Mechanic.None)
            hints.Add(NextMechanic.ToString());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SplashOfVenom:
            case AID.SpreadLove:
                NextMechanic = Mechanic.Spread;
                break;
            case AID.DropOfVenom:
            case AID.DropOfLove:
                NextMechanic = Mechanic.Pairs;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TemptingTwistAOE:
            case AID.HoneyBeelineAOE:
            case AID.TemptingTwistBeatAOE:
            case AID.HoneyBeelineBeatAOE:
                switch (NextMechanic)
                {
                    case Mechanic.Pairs:
                        // note: it's random whether dd or supports are hit, select supports arbitrarily
                        Activation = WorldState.FutureTime(4.5f);
                        AddStacks(Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Activation);
                        break;
                    case Mechanic.Spread:
                        Activation = WorldState.FutureTime(4.5f);
                        AddSpreads(Raid.WithoutSlot(true), Activation);
                        break;
                }
                break;
            case AID.SplashOfVenomAOE:
            case AID.DropOfVenomAOE:
            case AID.SpreadLoveAOE:
            case AID.DropOfLoveAOE:
                Spreads.Clear();
                Stacks.Clear();
                NextMechanic = Mechanic.None;
                break;
        }
    }
}

class TemptingTwist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TemptingTwistAOE), new AOEShapeDonut(6, 30)); // TODO: verify inner radius
class TemptingTwistBeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TemptingTwistBeatAOE), new AOEShapeDonut(6, 30)); // TODO: verify inner radius
class HoneyBeeline(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HoneyBeelineAOE), new AOEShapeRect(30, 7, 30));
class HoneyBeelineBeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HoneyBeelineBeatAOE), new AOEShapeRect(30, 7, 30));
class PoisonCloudSplinter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PoisonCloudSplinter), new AOEShapeCircle(8));
class SweetheartSplinter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SweetheartSplinter), new AOEShapeCircle(8));
