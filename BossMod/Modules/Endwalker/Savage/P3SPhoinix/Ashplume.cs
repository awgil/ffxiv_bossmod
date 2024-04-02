namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to ashplumes (normal or parts of gloryplume)
// normal ashplume is boss cast (with different IDs depending on stack/spread) + instant aoe some time later
// gloryplume is one instant cast with animation only soon after boss cast + instant aoe some time later
class Ashplume : BossComponent
{
    public enum State { UnknownGlory, Stack, Spread, Done }

    public State CurState { get; private set; }

    private static readonly float _stackRadius = 8;
    private static readonly float _spreadRadius = 6;

    public override void Init(BossModule module)
    {
        CurState = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.ExperimentalAshplumeStack => State.Stack,
            AID.ExperimentalAshplumeSpread => State.Spread,
            AID.ExperimentalGloryplumeSingle or AID.ExperimentalGloryplumeMulti => State.UnknownGlory, // instant cast turns this into correct state ~3 sec after cast end
            _ => State.Done
        };
        if (CurState == State.Done)
            module.ReportError(this, $"Failed to initialize ashplume component, unexpected cast {module.PrimaryActor.CastInfo?.Action}");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (CurState == State.Stack)
        {
            // note: it seems to always target 1 tank & 1 healer, so correct stacks are always tanks+dd and healers+dd
            int numStacked = 0;
            bool haveTanks = actor.Role == Role.Tank;
            bool haveHealers = actor.Role == Role.Healer;
            foreach (var pair in module.Raid.WithoutSlot().InRadiusExcluding(actor, _stackRadius))
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
            if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any())
            {
                hints.Add("Spread!");
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurState == State.Stack)
            hints.Add("Stack!");
        else if (CurState == State.Spread)
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (CurState == State.UnknownGlory || CurState == State.Done)
            return;

        // draw all raid members, to simplify positioning
        float aoeRadius = CurState == State.Stack ? _stackRadius : _spreadRadius;
        foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
        {
            arena.Actor(player, player.Position.InCircle(pc.Position, aoeRadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }

        // draw circle around pc
        arena.AddCircle(pc.Position, aoeRadius, ArenaColor.Danger);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
