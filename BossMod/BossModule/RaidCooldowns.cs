namespace BossMod;

// TODO: this should probably store available-at per player per cooldown group...
public sealed class RaidCooldowns : IDisposable
{
    private readonly WorldState _ws;
    private readonly EventSubscriptions _subscriptions;
    private readonly List<(int Slot, ActionID Action, DateTime AvailableAt)> _damageCooldowns = []; // TODO: this should be improved - determine available cooldowns by class?..
    private readonly DateTime[] _interruptCooldowns = new DateTime[PartyState.MaxPartySize];

    public RaidCooldowns(WorldState ws)
    {
        _ws = ws;
        _subscriptions = new
        (
            _ws.Party.Modified.Subscribe(HandlePartyUpdate),
            _ws.Actors.CastEvent.Subscribe(HandleCast),
            _ws.DirectorUpdate.Subscribe(HandleDirectorUpdate)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public float NextDamageBuffIn()
    {
        // TODO: this is currently quite hacky
        if (_damageCooldowns.Count == 0)
        {
            // if there are no entries, assume it is an opener and cooldowns are imminent
            // this doesn't handle e.g. someone not pressing CD during opener (but fuck that?)
            return 0;
        }
        // find first ability coming off CD and return time until it happens
        var firstAvailable = _damageCooldowns.Min(e => e.AvailableAt);
        return MathF.Max(0, (float)(firstAvailable - _ws.CurrentTime).TotalSeconds);
    }

    // NextDamageBuffIn, but null at fight start before any raidbuffs have been used
    public float? NextDamageBuffIn2()
    {
        if (_damageCooldowns.Count == 0)
            return null;

        var firstAvailable = _damageCooldowns.Min(e => e.AvailableAt);
        return MathF.Min(float.MaxValue, (float)(firstAvailable - _ws.CurrentTime).TotalSeconds);
    }

    public static bool IsDamageBuff(uint statusID) => statusID
        is (uint)AST.SID.Divination or (uint)DRG.SID.BattleLitany or (uint)RPR.SID.ArcaneCircle or (uint)MNK.SID.Brotherhood
        or (uint)BRD.SID.BattleVoice or (uint)DNC.SID.TechnicalFinish or (uint)SMN.SID.SearingLight or (uint)RDM.SID.Embolden
        or (uint)PCT.SID.StarryMuse;

    public static bool IsDamageDebuff(uint statusID) => statusID is (uint)SCH.SID.ChainStratagem or (uint)NIN.SID.Dokumori or (uint)NIN.SID.VulnerabilityUp;

    public float DamageBuffLeft(Actor player, Actor? target)
    {
        DateTime expireMax = _ws.CurrentTime;
        foreach (var status in player.Statuses.Where(s => IsDamageBuff(s.ID)))
            if (status.ExpireAt > expireMax)
                expireMax = status.ExpireAt;
        if (target is { } t)
            foreach (var status in t.Statuses.Where(s => IsDamageDebuff(s.ID)))
                if (status.ExpireAt > expireMax)
                    expireMax = status.ExpireAt;
        return (float)(expireMax - _ws.CurrentTime).TotalSeconds;
    }

    public float InterruptAvailableIn(int slot, DateTime now) => MathF.Max(0, (float)(_interruptCooldowns[slot] - now).TotalSeconds);

    private void HandlePartyUpdate(PartyState.OpModify op)
    {
        _damageCooldowns.RemoveAll(e => e.Slot == op.Slot);
    }

    private void HandleCast(Actor actor, ActorCastEvent cast)
    {
        if (!cast.IsSpell())
            return;
        // see https://i.redd.it/xrtgpras94881.png
        // TODO: AST card buffs?, all non-damage buffs
        _ = cast.Action.ID switch
        {
            (uint)SCH.AID.ChainStratagem => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)AST.AID.Divination => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)DRG.AID.BattleLitany => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)RPR.AID.ArcaneCircle => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)MNK.AID.Brotherhood => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)NIN.AID.Mug or (uint)NIN.AID.Dokumori => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)BRD.AID.BattleVoice => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            //(uint)BRD.AID.RadiantFinale => UpdateDamageCooldown(actor.InstanceID, cast.Action), // note that even though CD is 110, it's used together with other 2min cds
            (uint)DNC.AID.QuadrupleTechnicalFinish => UpdateDamageCooldown(actor.InstanceID, cast.Action), // DNC technical finish
            (uint)SMN.AID.SearingLight => UpdateDamageCooldown(actor.InstanceID, cast.Action),
            (uint)RDM.AID.Embolden => UpdateDamageCooldown(actor.InstanceID, cast.Action), // RDM embolden
            (uint)PCT.AID.StarryMuse => UpdateDamageCooldown(actor.InstanceID, cast.Action), // PCT starry muse
            (uint)WAR.AID.Interject or (uint)BRD.AID.HeadGraze => UpdateInterruptCooldown(actor.InstanceID, cast.Action, 30),
            _ => false
        };
    }

    private bool UpdateDamageCooldown(ulong casterID, ActionID action)
    {
        int slot = _ws.Party.FindSlot(casterID);
        if (slot is < 0 or >= PartyState.MaxPartySize) // ignore cooldowns from other alliance parties
            return false;

        var availableAt = _ws.CurrentTime.AddSeconds(120);
        var index = _damageCooldowns.FindIndex(e => e.Slot == slot && e.Action == action);
        if (index < 0)
        {
            _damageCooldowns.Add((slot, action, availableAt));
        }
        else
        {
            _damageCooldowns[index] = (slot, action, availableAt);
        }
        Service.Log($"[RaidCooldowns] Updating damage cooldown: {action} by {_ws.Party[slot]?.Name}; there are now {_damageCooldowns.Count} entries");
        return true;
    }

    private bool UpdateInterruptCooldown(ulong casterID, ActionID action, float cooldown)
    {
        int slot = _ws.Party.FindSlot(casterID);
        if (slot is < 0 or >= PartyState.MaxPartySize) // ignore cooldowns from other alliance parties
            return false;
        _interruptCooldowns[slot] = _ws.CurrentTime.AddSeconds(cooldown);
        Service.Log($"[RaidCooldowns] Updating interrupt cooldown: {action} by {_ws.Party[slot]?.Name} will next be available in {cooldown:f1}s");
        return true;
    }

    private void HandleDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        if (op.UpdateID is 0x40000001 or 0x40000005) // init or fade-out (wipe)
        {
            Service.Log($"[RaidCooldowns] Clearing cooldowns ({_damageCooldowns.Count} damage entries)");
            _damageCooldowns.Clear();
            Array.Fill(_interruptCooldowns, default);
        }
    }
}
