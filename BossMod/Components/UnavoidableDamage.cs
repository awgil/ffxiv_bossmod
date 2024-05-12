namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
public class RaidwideCast(BossModule module, ActionID aid, string hint = "Raidwide") : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), c.CastInfo?.NPCFinishAt ?? default));
    }
}

// generic unavoidable raidwide, initiated by a custom condition and applied by an instant cast after a delay
public class RaidwideInstant(BossModule module, ActionID aid, float delay, string hint = "Raidwide") : CastCounter(module, aid)
{
    public float Delay = delay;
    public string Hint = hint;
    public DateTime Activation; // default if inactive, otherwise expected cast time

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Activation != default && Hint.Length > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Activation = default;
        }
    }
}

// generic unavoidable instant raidwide initiated by a cast (usually visual-only)
public class RaidwideCastDelay(BossModule module, ActionID actionVisual, ActionID actionAOE, float delay, string hint = "Raidwide") : RaidwideInstant(module, actionAOE, delay, hint)
{
    public ActionID ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
            Activation = spell.NPCFinishAt.AddSeconds(Delay);
    }
}

// generic unavoidable instant raidwide cast initiated by NPC yell
public class RaidwideAfterNPCYell(BossModule module, ActionID aid, uint npcYellID, float delay, string hint = "Raidwide") : RaidwideInstant(module, aid, delay, hint)
{
    public uint NPCYellID = npcYellID;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id == NPCYellID)
            Activation = WorldState.FutureTime(Delay);
    }
}

// generic unavoidable single-target damage, started and finished by a single cast (typically tankbuster, but not necessary)
public class SingleTargetCast(BossModule module, ActionID aid, string hint = "Tankbuster") : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo != null)
            {
                var target = c.CastInfo.TargetID != c.InstanceID ? c.CastInfo.TargetID : c.TargetID; // assume self-targeted casts actually hit main target
                hints.PredictedDamage.Add((new BitMask().WithBit(Raid.FindSlot(target)), c.CastInfo.NPCFinishAt));
            }
        }
    }
}

// generic unavoidable single-target damage, initiated by a custom condition and applied by an instant cast after a delay
public class SingleTargetInstant(BossModule module, ActionID aid, float delay, string hint = "Tankbuster") : CastCounter(module, aid)
{
    public float Delay = delay; // delay from visual cast end to cast event
    public string Hint = hint;
    public readonly List<(int slot, DateTime activation)> Targets = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Targets.Count > 0 && Hint.Length > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Targets)
            hints.PredictedDamage.Add((new BitMask().WithBit(t.slot), t.activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Targets.RemoveAll(t => Raid[t.slot]?.InstanceID == spell.MainTargetID);
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public class SingleTargetCastDelay(BossModule module, ActionID actionVisual, ActionID actionAOE, float delay, string hint = "Tankbuster") : SingleTargetInstant(module, actionAOE, delay, hint)
{
    public ActionID ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
        {
            var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            Targets.Add((Raid.FindSlot(target), spell.NPCFinishAt.AddSeconds(Delay)));
        }
    }
}

// generic unavoidable single-target damage, started and finished by a single cast, that can be delayed by moving out of range (typically tankbuster, but not necessary)
public class SingleTargetDelayableCast(BossModule module, ActionID aid, string hint = "Tankbuster") : SingleTargetCastDelay(module, aid, aid, 0, hint);
