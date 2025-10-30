namespace BossMod.Components;

// generic 'wild charge': various mechanics that consist of charge aoe on some target that other players have to stay in; optionally some players can be marked as 'having to be closest to source' (usually tanks)
public class GenericWildCharge(BossModule module, float halfWidth, Enum? aid = default, float fixedLength = 0) : CastCounter(module, aid)
{
    public enum PlayerRole
    {
        Ignore, // player completely ignores the mechanic; no hints for such players are displayed
        Target, // player is charge target
        TargetNotFirst, // player is charge target, and has to hide behind other raid member
        Share, // player has to stay inside aoe
        ShareNotFirst, // player has to stay inside aoe, but not as a closest raid member
        Avoid, // player has to avoid aoe
    }

    public float HalfWidth = halfWidth;
    public float FixedLength = fixedLength; // if == 0, length is up to target
    public Actor? Source; // if null, mechanic is not active
    public DateTime Activation;
    public PlayerRole[] PlayerRoles = new PlayerRole[PartyState.MaxAllies];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null)
            return;

        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
            case PlayerRole.Target: // TODO: consider hints for target?..
                break; // nothing to advise
            case PlayerRole.TargetNotFirst:
                if (EnumerateAOEs(slot).Any(aoe => InAOE(aoe, actor)))
                    hints.Add("GTFO from other charges!");
                else if (!AnyRoleCloser(GetAOEForTarget(Source.Position, actor.Position), PlayerRole.Share, PlayerRole.Share, (actor.Position - Source.Position).LengthSq()))
                    hints.Add("Hide behind tank!");
                break;
            case PlayerRole.Share:
            case PlayerRole.ShareNotFirst:
                bool badShare = false;
                int numShares = 0;
                foreach (var aoe in EnumerateAOEs().Where(aoe => InAOE(aoe, actor)))
                {
                    if (++numShares > 1)
                        break;

                    badShare = PlayerRoles[slot] == PlayerRole.Share
                        ? AnyRoleCloser(aoe, PlayerRole.ShareNotFirst, PlayerRole.TargetNotFirst, (actor.Position - Source.Position).LengthSq())
                        : !AnyRoleCloser(aoe, PlayerRole.Share, PlayerRole.Target, (actor.Position - Source.Position).LengthSq());
                }
                if (numShares == 0)
                    hints.Add("Stay inside charge!");
                else if (numShares > 1)
                    hints.Add("Stay in single charge!");
                else if (badShare)
                    hints.Add(PlayerRoles[slot] == PlayerRole.Share ? "Move closer to charge source!" : "Hide behind tank!");
                break;
            case PlayerRole.Avoid:
                if (EnumerateAOEs().Any(aoe => InAOE(aoe, actor)))
                    hints.Add("GTFO from charge!");
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
                break;
            case PlayerRole.Target:
            case PlayerRole.TargetNotFirst: // TODO: consider some hint to hide behind others?..
                if (Source != null)
                {
                    // try to stack with furthest target (by angle delta) to hit as many teammates as possible
                    var baitDir = (actor.Position - Source.Position).ToAngle();
                    var farthest = Raid.WithSlot().WhereSlot(i => PlayerRoles[i] is PlayerRole.Share or PlayerRole.ShareNotFirst).Actors().MaxBy(a => baitDir.DistanceToAngle((a.Position - Source.Position).ToAngle()).Abs());
                    if (farthest != null)
                    {
                        var stack = GetAOEForTarget(Source.Position, farthest.Position);
                        hints.AddForbiddenZone(ShapeContains.InvertedCone(stack.origin, 100, stack.dir.ToAngle(), Angle.Asin(HalfWidth / (Source.Position - farthest.Position).Length())), Activation);
                    }
                }
                break;
            case PlayerRole.Share: // TODO: some hint to be first in line...
            case PlayerRole.ShareNotFirst:
                foreach (var aoe in EnumerateAOEs())
                    hints.AddForbiddenZone(ShapeContains.InvertedRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth), Activation);
                break;
            case PlayerRole.Avoid:
                foreach (var aoe in EnumerateAOEs())
                    hints.AddForbiddenZone(ShapeContains.Rect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth), Activation);
                break;
        }

        foreach (var aoe in EnumerateAOEs())
            // TODO add separate "tankbuster" hint for PlayerRole.Share if there are any ShareNotFirsts in the party
            hints.AddPredictedDamage(Raid.WithSlot().Where(p => InAOE(aoe, p.Item2)).Mask(), Activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null || PlayerRoles[pcSlot] == PlayerRole.Ignore)
            return;

        foreach (var aoe in EnumerateAOEs())
        {
            var dangerous = PlayerRoles[pcSlot] == PlayerRole.Avoid; // TODO: reconsider this condition
            Arena.ZoneRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth, dangerous ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
        }
    }

    private (WPos origin, WDir dir, float length) GetAOEForTarget(WPos sourcePos, WPos targetPos)
    {
        var toTarget = targetPos - sourcePos;
        var length = FixedLength > 0 ? FixedLength : toTarget.Length();
        var dir = toTarget.Normalized();
        return (sourcePos, dir, length);
    }

    protected bool InAOE((WPos origin, WDir dir, float length) aoe, Actor actor) => actor.Position.InRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth);

    protected IEnumerable<(WPos origin, WDir dir, float length)> EnumerateAOEs(int targetSlotToSkip = -1)
    {
        if (Source == null)
            yield break;
        foreach (var (i, p) in Module.Raid.WithSlot().WhereSlot(i => i != targetSlotToSkip && PlayerRoles[i] is PlayerRole.Target or PlayerRole.TargetNotFirst))
            yield return GetAOEForTarget(Source.Position, p.Position);
    }

    private bool AnyRoleCloser((WPos origin, WDir dir, float length) aoe, PlayerRole role1, PlayerRole role2, float thresholdSq)
        => Raid.WithSlot().Any(ia => (PlayerRoles[ia.Item1] == role1 || PlayerRoles[ia.Item1] == role2) && InAOE(aoe, ia.Item2) && (ia.Item2.Position - aoe.origin).LengthSq() < thresholdSq);
}

// simple line stack where target is determined by 'target select' cast
public class SimpleLineStack(BossModule module, float halfWidth, float fixedLength, Enum aidTargetSelect, Enum aidResolve, float activationDelay) : GenericWildCharge(module, halfWidth, aidResolve, fixedLength)
{
    public readonly ActionID TargetSelect = ActionID.MakeSpell(aidTargetSelect);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == TargetSelect)
        {
            Source = caster;
            Activation = WorldState.FutureTime(activationDelay);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
        }
        else if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}

public class IconLineStack(BossModule module, float halfWidth, float fixedLength, uint iconID, Enum aidResolve, float activationDelay) : GenericWildCharge(module, halfWidth, aidResolve, fixedLength)
{
    public uint Icon => iconID;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (Icon == iconID)
        {
            Source = actor;
            Activation = WorldState.FutureTime(activationDelay);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}

// component for multiple simultaneous line stacks from different sources
// we assume getting hit by two line stacks is fatal (they typically give a vuln stack or something)
public class MultiLineStack(BossModule module, float halfWidth, float fixedLength, Enum aidTargetSelect, Enum aidResolve, float activationDelay) : CastCounter(module, aidResolve)
{
    public float ActivationDelay = activationDelay;

    public struct Stack(WPos source, Actor target, DateTime activation, BitMask forbiddenPlayers = default)
    {
        public WPos Source = source;
        public Actor Target = target;
        public DateTime Activation = activation;
        public BitMask ForbiddenPlayers = forbiddenPlayers;
    }

    protected bool Check(Stack s, Actor player) => player.Position.InRect(s.Source, (s.Target.Position - s.Source).Normalized(), fixedLength, 0, halfWidth);

    public readonly List<Stack> Stacks = [];

    private Func<WPos, bool> ShapeFn(Stack s) => ShapeContains.Rect(s.Source, (s.Target.Position - s.Source).Normalized(), fixedLength, 0, halfWidth);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Stacks.RemoveAll(c => c.Target.InstanceID == spell.MainTargetID);
        }

        if (spell.Action.ID == (uint)(object)aidTargetSelect)
        {
            if (WorldState.Actors.Find(spell.MainTargetID) is { } tar)
                Stacks.Add(new(caster.Position, tar, WorldState.FutureTime(ActivationDelay)));
            else
                ReportError($"Unable to find target with ID {spell.MainTargetID:X} for stack");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var s in Stacks)
            Arena.ZoneRect(s.Source, (s.Target.Position - s.Source).Normalized(), fixedLength, 0, halfWidth, s.ForbiddenPlayers[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Stacks.Count == 0)
            return;

        if (Stacks.Any(s => s.Target == actor))
            hints.Add("Stack with party!", false);
        else
        {
            int countAllowed = 0, countOk = 0, countForbidden = 0;
            foreach (var s in Stacks)
            {
                if (!s.ForbiddenPlayers[slot])
                    countAllowed++;

                if (Check(s, actor))
                {
                    if (s.ForbiddenPlayers[slot])
                        countForbidden++;
                    else
                        countOk++;
                }
            }

            if (countAllowed == 0)
                return;

            if (countForbidden > 0)
                hints.Add("GTFO from forbidden stack!");
            else if (countOk == 0)
                hints.Add("Stack!");
            else
            {
                hints.Add("Stack!", false);
                if (countOk > 1)
                    hints.Add("GTFO from other stacks!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count == 0)
            return;

        foreach (var group in Stacks.GroupBy(s => s.ForbiddenPlayers[slot]))
        {
            if (group.Key) // player is not allowed to stack with these
            {
                foreach (var s in group)
                    hints.AddForbiddenZone(ShapeFn(s), s.Activation);
            }
            else
            {
                var zones = group.Select(ShapeFn).ToList();
                hints.AddForbiddenZone(p => zones.Count(f => f(p)) != 1, group.First().Activation);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Stacks.Any(s => s.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Normal;
}
