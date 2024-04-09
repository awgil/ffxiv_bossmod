namespace BossMod.Components;

// generic 'wild charge': various mechanics that consist of charge aoe on some target that other players have to stay in; optionally some players can be marked as 'having to be closest to source' (usually tanks)
public class GenericWildCharge(BossModule module, float halfWidth, ActionID aid = default, float fixedLength = 0) : CastCounter(module, aid)
{
    public enum PlayerRole
    {
        Ignore, // player completely ignores the mechanic; no hints for such players are displayed
        Target, // player is charge target
        Share, // player has to stay inside aoe
        ShareNotFirst, // player has to stay inside aoe, but not as a closest raid member
        Avoid, // player has to avoid aoe
    }

    public float HalfWidth = halfWidth;
    public float FixedLength = fixedLength; // if == 0, length is up to target
    public Actor? Source; // if null, mechanic is not active
    public PlayerRole[] PlayerRoles = new PlayerRole[PartyState.MaxAllianceSize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null || PlayerRoles[slot] is PlayerRole.Ignore or PlayerRole.Target) // TODO: consider hints for target?..
            return;

        var target = Raid[Array.IndexOf(PlayerRoles, PlayerRole.Target)];
        if (target == null)
            return;

        var toTarget = target.Position - Source.Position;
        var length = FixedLength > 0 ? FixedLength : toTarget.Length();
        var dir = toTarget.Normalized();
        bool inAOE = actor.Position.InRect(Source.Position, dir, length, 0, HalfWidth);
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Share:
                if (!inAOE)
                    hints.Add("Stay inside charge!");
                else if (AnyRoleCloser(Source.Position, dir, length, PlayerRole.ShareNotFirst, (actor.Position - Source.Position).LengthSq()))
                    hints.Add("Move closer to charge source!");
                break;
            case PlayerRole.ShareNotFirst:
                if (!inAOE)
                    hints.Add("Stay inside charge!");
                else if (!AnyRoleCloser(Source.Position, dir, length, PlayerRole.Share, (actor.Position - Source.Position).LengthSq()))
                    hints.Add("Hide behind tank!");
                break;
            case PlayerRole.Avoid:
                if (inAOE)
                    hints.Add("GTFO from charge!");
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: implement
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null || PlayerRoles[pcSlot] == PlayerRole.Ignore)
            return;

        var target = Raid[Array.IndexOf(PlayerRoles, PlayerRole.Target)];
        if (target != null)
        {
            var dir = target.Position - Source.Position;
            var length = FixedLength > 0 ? FixedLength : dir.Length();
            dir = dir.Normalized();
            Arena.ZoneRect(Source.Position, dir, length, 0, HalfWidth, PlayerRoles[pcSlot] == PlayerRole.Avoid ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
        }
    }

    private bool AnyRoleCloser(WPos sourcePos, WDir direction, float length, PlayerRole role, float thresholdSq)
        => Raid.WithSlot().Any(ia => PlayerRoles[ia.Item1] == role && ia.Item2.Position.InRect(sourcePos, direction, length, 0, HalfWidth) && (ia.Item2.Position - sourcePos).LengthSq() < thresholdSq);
}
