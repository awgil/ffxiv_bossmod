namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
public class RaidwideCast : CastHint
{
    public RaidwideCast(ActionID aid, string hint = "Raidwide") : base(aid, hint) { }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), c.CastInfo?.NPCFinishAt ?? default));
    }
}

// generic unavoidable raidwide, initiated by a custom condition and applied by an instant cast after a delay
public class RaidwideInstant : CastCounter
{
    public float Delay;
    public string Hint;
    public DateTime Activation; // default if inactive, otherwise expected cast time

    public RaidwideInstant(ActionID aid, float delay, string hint = "Raidwide") : base(aid)
    {
        Delay = delay;
        Hint = hint;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Activation != default && Hint.Length > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), Activation));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Activation = default;
        }
    }
}

// generic unavoidable instant raidwide initiated by a cast (usually visual-only)
public class RaidwideCastDelay : RaidwideInstant
{
    public ActionID ActionVisual;

    public RaidwideCastDelay(ActionID actionVisual, ActionID actionAOE, float delay, string hint = "Raidwide") : base(actionAOE, delay, hint)
    {
        ActionVisual = actionVisual;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
            Activation = spell.NPCFinishAt.AddSeconds(Delay);
    }
}

// generic unavoidable instant raidwide cast initiated by NPC yell
public class RaidwideAfterNPCYell : RaidwideInstant
{
    public uint NPCYellID;

    public RaidwideAfterNPCYell(ActionID aid, uint npcYellID, float delay, string hint = "Raidwide") : base(aid, delay, hint)
    {
        NPCYellID = npcYellID;
    }

    public override void OnActorNpcYell(BossModule module, Actor actor, ushort id)
    {
        if (id == NPCYellID)
            Activation = module.WorldState.CurrentTime.AddSeconds(Delay);
    }
}

// generic unavoidable single-target damage, started and finished by a single cast (typically tankbuster, but not necessary)
public class SingleTargetCast : CastHint
{
    public SingleTargetCast(ActionID aid, string hint = "Tankbuster") : base(aid, hint) { }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo != null)
            {
                hints.PredictedDamage.Add((new BitMask().WithBit(module.Raid.FindSlot(c.CastInfo.TargetID)), c.CastInfo.NPCFinishAt));
            }
        }
    }
}

// generic unavoidable single-target damage, initiated by a custom condition and applied by an instant cast after a delay
public class SingleTargetInstant : CastCounter
{
    public float Delay; // delay from visual cast end to cast event
    public string Hint;
    public List<(int slot, DateTime activation)> Targets = new();

    public SingleTargetInstant(ActionID aid, float delay, string hint = "Tankbuster") : base(aid)
    {
        Delay = delay;
        Hint = hint;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Targets.Count > 0 && Hint.Length > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Targets)
            hints.PredictedDamage.Add((new BitMask().WithBit(t.slot), t.activation));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Targets.RemoveAll(t => module.Raid[t.slot]?.InstanceID == spell.MainTargetID);
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public class SingleTargetCastDelay : SingleTargetInstant
{
    public ActionID ActionVisual;

    public SingleTargetCastDelay(ActionID actionVisual, ActionID actionAOE, float delay, string hint = "Tankbuster") : base(actionAOE, delay, hint)
    {
        ActionVisual = actionVisual;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
            Targets.Add((module.Raid.FindSlot(spell.TargetID), spell.NPCFinishAt.AddSeconds(Delay)));
    }
}

// generic unavoidable single-target damage, started and finished by a single cast, that can be delayed by moving out of range (typically tankbuster, but not necessary)
public class SingleTargetDelayableCast : SingleTargetCastDelay
{
    public SingleTargetDelayableCast(ActionID aid, string hint = "Tankbuster") : base(aid, aid, 0, hint) { }
}
