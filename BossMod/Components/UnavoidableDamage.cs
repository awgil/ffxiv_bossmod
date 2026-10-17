namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
public class RaidwideCast(BossModule module, Enum aid, string hint = "Raidwide") : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }
}

// generic unavoidable raidwide, initiated by a custom condition and applied by an instant cast after a delay
public class RaidwideInstant(BossModule module, Enum aid, float delay, string hint = "Raidwide") : CastCounter(module, aid)
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
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Activation);
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
public class RaidwideCastDelay(BossModule module, Enum actionVisual, Enum actionAOE, float delay, string hint = "Raidwide") : RaidwideInstant(module, actionAOE, delay, hint)
{
    public ActionID ActionVisual = ActionID.MakeSpell(actionVisual);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
            Activation = Module.CastFinishAt(spell, Delay);
    }
}

// generic unavoidable multi-hit raidwide: a cast followed by a number of hits at a regular interval; predicted as one entry for the next hit
public class RaidwideMultiHit(BossModule module, Enum startAction, Enum[] hitActions, float firstDelay, float interval, int hits, string hint = "Raidwide") : CastCounter(module, startAction)
{
    public readonly ActionID[] HitActions = [.. hitActions.Select(a => ActionID.MakeSpell(a))];
    public string Hint = hint;
    public int Remaining;
    public DateTime NextHit;
    public DateTime LastHit;
    public int Hits = hits;
    public float Interval = interval;
    private float _interval;
    private DateTime _lastCounted;
    private int _numSequences;

    // hit count and interval of the n-th sequence (0-based), for fights where they differ between casts
    protected virtual (int Hits, float Interval) Sequence(int index) => (Hits, Interval);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Remaining > 0 && Hint.Length > 0)
            hints.Add($"{Hint} x{Remaining}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Remaining > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), NextHit, AIHints.PredictedDamageType.Raidwide, Remaining, LastHit);
    }

    public override void Update()
    {
        if (Remaining > 0 && WorldState.CurrentTime > LastHit.AddSeconds(3))
            Remaining = 0;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action != WatchedAction)
            return;
        (Remaining, _interval) = Sequence(_numSequences++);
        NextHit = Module.CastFinishAt(spell, firstDelay);
        LastHit = NextHit.AddSeconds(_interval * (Remaining - 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Remaining <= 0 || !HitActions.Contains(spell.Action) || WorldState.CurrentTime < _lastCounted.AddSeconds(0.3f))
            return; // several helpers can resolve the same hit
        _lastCounted = WorldState.CurrentTime;
        ++NumCasts;
        --Remaining;
        NextHit = WorldState.FutureTime(_interval);
        if (Remaining > 0)
            LastHit = NextHit.AddSeconds(_interval * (Remaining - 1));
    }
}

// generic unavoidable instant raidwide cast initiated by NPC yell
public class RaidwideAfterNPCYell(BossModule module, Enum aid, uint npcYellID, float delay, string hint = "Raidwide") : RaidwideInstant(module, aid, delay, hint)
{
    public uint NPCYellID = npcYellID;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id == NPCYellID)
            Activation = WorldState.FutureTime(Delay);
    }
}

// generic unavoidable single-target damage, started and finished by a single cast (typically tankbuster, but not necessary)
public class SingleTargetCast(BossModule module, Enum aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo != null)
            {
                var target = c.CastInfo.TargetID != c.InstanceID ? c.CastInfo.TargetID : c.TargetID; // assume self-targeted casts actually hit main target
                hints.AddPredictedDamage(new BitMask().WithBit(Raid.FindSlot(target)), Module.CastFinishAt(c.CastInfo), damageType);
            }
        }
    }
}

// generic unavoidable single-target damage, initiated by a custom condition and applied by an instant cast after a delay
public class SingleTargetInstant(BossModule module, Enum aid, float delay, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastCounter(module, aid)
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
            hints.AddPredictedDamage(new BitMask().WithBit(t.slot), t.activation, damageType);
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
public class SingleTargetCastDelay(BossModule module, Enum actionVisual, Enum actionAOE, float delay, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetInstant(module, actionAOE, delay, hint, damageType)
{
    public ActionID ActionVisual = ActionID.MakeSpell(actionVisual);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionVisual)
        {
            var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            Targets.Add((Raid.FindSlot(target), Module.CastFinishAt(spell, Delay)));
        }
    }
}

// generic unavoidable single-target damage, started and finished by a single cast, that can be delayed by moving out of range (typically tankbuster, but not necessary)
public class SingleTargetDelayableCast(BossModule module, Enum aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetCastDelay(module, aid, aid, 0, hint, damageType);
