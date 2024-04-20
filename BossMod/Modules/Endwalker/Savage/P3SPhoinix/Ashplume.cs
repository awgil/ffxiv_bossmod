namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to ashplumes (normal or parts of gloryplume)
// normal ashplume is boss cast (with different IDs depending on stack/spread) + instant aoe some time later
// gloryplume is one instant cast with animation only soon after boss cast + instant aoe some time later
class Ashplume : BossComponent
{
    public enum State { UnknownGlory, Stack, Spread, Done }

    public State CurState { get; private set; }

    private const float _stackRadius = 8;
    private const float _spreadRadius = 6;

    public Ashplume(BossModule module) : base(module)
    {
        CurState = (AID)(Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.ExperimentalAshplumeStack => State.Stack,
            AID.ExperimentalAshplumeSpread => State.Spread,
            AID.ExperimentalGloryplumeSingle or AID.ExperimentalGloryplumeMulti => State.UnknownGlory, // instant cast turns this into correct state ~3 sec after cast end
            _ => State.Done
        };
        if (CurState == State.Done)
            ReportError($"Failed to initialize ashplume component, unexpected cast {Module.PrimaryActor.CastInfo?.Action}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurState == State.Stack)
        {
            // note: it seems to always target 1 tank & 1 healer, so correct stacks are always tanks+dd and healers+dd
            int numStacked = 0;
            bool haveTanks = actor.Role == Role.Tank;
            bool haveHealers = actor.Role == Role.Healer;
            foreach (var pair in Raid.WithoutSlot().InRadiusExcluding(actor, _stackRadius))
            {
                ++numStacked;
                haveTanks |= pair.Role == Role.Tank;
                haveHealers |= pair.Role == Role.Healer;
            }
            if (numStacked != 3)
            {
                hints.Add("Stack in fours!");
            }
            else if (haveTanks && haveHealers)
            {
                hints.Add("Incorrect stack!");
            }
        }
        else if (CurState == State.Spread)
        {
            if (Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any())
            {
                hints.Add("Spread!");
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurState == State.Stack)
            hints.Add("Stack!");
        else if (CurState == State.Spread)
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurState is State.UnknownGlory or State.Done)
            return;

        // draw all raid members, to simplify positioning
        float aoeRadius = CurState == State.Stack ? _stackRadius : _spreadRadius;
        foreach (var player in Raid.WithoutSlot().Exclude(pc))
        {
            Arena.Actor(player, player.Position.InCircle(pc.Position, aoeRadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }

        // draw circle around pc
        Arena.AddCircle(pc.Position, aoeRadius, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalGloryplumeSpread:
                CurState = State.Spread;
                break;
            case AID.ExperimentalGloryplumeStack:
                CurState = State.Stack;
                break;
            case AID.ExperimentalGloryplumeSpreadAOE:
            case AID.ExperimentalGloryplumeStackAOE:
            case AID.ExperimentalAshplumeSpreadAOE:
            case AID.ExperimentalAshplumeStackAOE:
                CurState = State.Done;
                break;
        }
    }
}
