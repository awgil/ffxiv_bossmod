namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class RM08SHowlingBladeStates : StateMachineBuilder
{
    public RM08SHowlingBladeStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ExtraplanarPursuit(id, 10.2f);
        WindStonefang(id + 0x10000, 8.9f);
        RevolutionaryReign(id + 0x20000, 5.1f);
        ExtraplanarPursuit(id + 0x30000, 2.2f);
        MillennialDecay(id + 0x40000, 8.5f);
        TrackingTremors(id + 0x50000, 0.8f);
        ExtraplanarPursuit(id + 0x60000, 1.8f);
        TerrestrialTitans(id + 0x70000, 3.8f);
        RevolutionaryReign(id + 0x80000, 0.3f);
        TacticalPack(id + 0x90000, 9.2f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void ExtraplanarPursuit(uint id, float delay)
    {
        CastStart(id, AID.ExtraplanarPursuitVisual, delay)
            .ActivateOnEnter<ExtraplanarPursuit>();
        ComponentCondition<ExtraplanarPursuit>(id + 1, 4, e => e.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ExtraplanarPursuit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WindStonefang(uint id, float delay)
    {
        CastMulti(id, [AID.WindfangIntercards, AID.WindfangCards, AID.StonefangCards, AID.StonefangIntercards], delay, 6, "In/out")
            .ActivateOnEnter<WindStonefangCross>()
            .ActivateOnEnter<WindfangDonut>()
            .ActivateOnEnter<StonefangCircle>()
            .ActivateOnEnter<WindStonefang>();

        ComponentCondition<WindStonefang>(id + 2, 0.1f, w => w.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<WindStonefangCross>()
            .DeactivateOnExit<WindfangDonut>()
            .DeactivateOnExit<StonefangCircle>()
            .DeactivateOnExit<WindStonefang>();
    }

    private void RevolutionaryReign(uint id, float delay)
    {
        CastMulti(id, [AID.EminentReignVisual1, AID.RevolutionaryReignVisual1, AID.EminentReignVisual2, AID.RevolutionaryReignVisual2], delay, 5.1f)
            .ActivateOnEnter<ReignJumpCounter>()
            .ActivateOnEnter<WolvesReign>()
            .ActivateOnEnter<ReignInout>()
            .ActivateOnEnter<WolvesReignRect>();

        ComponentCondition<ReignJumpCounter>(id + 2, 1.9f, e => e.NumCasts > 0, "Boss jump")
            .DeactivateOnExit<ReignJumpCounter>()
            .DeactivateOnExit<WolvesReign>();
        ComponentCondition<WolvesReignRect>(id + 3, 2.5f, w => w.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        ComponentCondition<ReignInout>(id + 4, 3.1f, r => r.NumCasts > 0, "In/out")
            .ActivateOnEnter<ReignsEnd>()
            .ActivateOnEnter<SovereignScar>()
            .ExecOnEnter<ReignInout>(r => r.Risky = true)
            .DeactivateOnExit<ReignInout>()
            .DeactivateOnExit<SovereignScar>()
            .DeactivateOnExit<ReignsEnd>();
    }

    private void MillennialDecay(uint id, float delay)
    {
        Cast(id, AID.MillennialDecay, delay, 5)
            .ActivateOnEnter<MillennialDecay>()
            .ActivateOnEnter<BreathOfDecay>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<ProwlingGale>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<AeroIII>(id + 0x10, 10.7f, e => e.NumCasts > 0, "Knockback");

        ComponentCondition<BreathOfDecay>(id + 0x11, 1.5f, b => b.NumCasts > 0, "Line AOE 1");

        ComponentCondition<Gust>(id + 0x12, 0.4f, g => g.NumFinishedSpreads > 0, "Spreads 1");
        Timeout(id + 0x13, 5.1f, "Spreads 2")
            .DeactivateOnExit<Gust>();
        ComponentCondition<BreathOfDecay>(id + 0x14, 2.5f, b => b.NumCasts > 4, "Line AOE 5")
            .ActivateOnEnter<WindsOfDecay>()
            .ActivateOnEnter<WindsOfDecayTether>()
            .DeactivateOnExit<BreathOfDecay>();

        ComponentCondition<AeroIII>(id + 0x20, 6.2f, a => a.NumCasts > 1, "Knockback")
            .ExecOnExit<WindsOfDecay>(w => w.EnableHints = true)
            .ExecOnExit<WindsOfDecayTether>(w => w.EnableHints = true);

        ComponentCondition<ProwlingGale>(id + 0x22, 2.2f, p => p.NumCasts > 0, "Towers");
        ComponentCondition<WindsOfDecay>(id + 0x23, 0.2f, w => w.NumCasts > 0, "Baits")
            .ActivateOnEnter<TrackingTremors>()
            .ActivateOnEnter<TrackingTremorsStack>();
    }

    private void TrackingTremors(uint id, float delay)
    {
        Cast(id, AID.TrackingTremorsVisual, delay, 5);

        ComponentCondition<TrackingTremors>(id + 2, 0.9f, t => t.NumCasts > 0, "Stack 1");

        ComponentCondition<TrackingTremors>(id + 5, 7.5f, t => t.NumCasts == 8, "Stack 8")
            .DeactivateOnExit<TrackingTremors>()
            .DeactivateOnExit<TrackingTremorsStack>();
    }

    private void TerrestrialTitans(uint id, float delay)
    {
        Cast(id, AID.GreatDivide, delay, 5, "Tankbuster")
            .ActivateOnEnter<GreatDivide>()
            .DeactivateOnExit<GreatDivide>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        Cast(id + 0x10, AID.TerrestrialTitansVisual, 11, 4, "Pillars appear")
            .ActivateOnEnter<TerrestrialTitans>()
            .DeactivateOnExit<TerrestrialTitans>();

        CastStart(id + 0x20, AID.TitanicPursuitVisual, 3.2f)
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FangedCrossing>();

        ComponentCondition<TitanicPursuit>(id + 0x21, 4, t => t.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TitanicPursuit>();

        ComponentCondition<FangedCrossing>(id + 0x30, 7.9f, f => f.NumCasts > 0, "Safe spot")
            .DeactivateOnExit<FangedCrossing>()
            .DeactivateOnExit<Towerfall>();
    }

    private void TacticalPack(uint id, float delay)
    {
        Cast(id, AID.TacticalPackVisual, delay, 3)
            .ActivateOnEnter<HowlingHavoc>()
            .ActivateOnEnter<AddsVoidzone>()
            .ActivateOnEnter<WolfOfWindyStone>()
            .ActivateOnEnter<StalkingWindyStone>()
            .ActivateOnEnter<AlphaWindyStone>();

        Targetable(id + 0x10, false, 2, "Boss disappears");

        id += 0x10000;

        ComponentCondition<HowlingHavoc>(id, 7.2f, h => h.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<WolfOfWindyStone>(id + 1, 2, w => w.WolfOfStone != null, "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<StalkingWindyStone>(id + 0x10, 8.3f, s => s.NumCasts > 0, "Baits 1")
            .ActivateOnEnter<EarthyWindborneEnd>();

        ComponentCondition<StalkingWindyStone>(id + 0x20, 14.2f, s => s.NumCasts > 2, "Baits 2");

        Timeout(id + 0xFF00, 30, "IDK");
    }
}

class HowlingHavoc(BossModule module) : Components.RaidwideCast(module, AID._Spell_HowlingHavoc1);
class AddsVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(8), Arena.Center, Activation: _activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00200010)
            _activation = WorldState.FutureTime(5.6f); // verify
        if (index == 1 && state == 0x00020001)
            _activation = WorldState.CurrentTime;
    }
}

public enum Aspect
{
    None,
    Wind,
    Stone,
}

class WolfOfWindyStone(BossModule module) : BossComponent(module)
{
    public Actor? WolfOfWind { get; private set; }
    public Actor? WolfOfStone { get; private set; }

    public readonly Aspect[] Aspects = new Aspect[PartyState.MaxPartySize];

    public override void OnTargetable(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID._Gen_WolfOfWind2:
                WolfOfWind = actor;
                break;
            case OID._Gen_WolfOfStone1:
                WolfOfStone = actor;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_Stonepack:
                SetAspect(Raid.FindSlot(actor.InstanceID), Aspect.Stone);
                break;
            case SID._Gen_Windpack:
                SetAspect(Raid.FindSlot(actor.InstanceID), Aspect.Wind);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Aspects[slot] == Aspect.Wind)
            hints.SetPriority(WolfOfWind, AIHints.Enemy.PriorityInvincible);
        if (Aspects[slot] == Aspect.Stone)
            hints.SetPriority(WolfOfStone, AIHints.Enemy.PriorityInvincible);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (WolfOfWind != null)
            Arena.Actor(WolfOfWind, Aspects[pcSlot] == Aspect.Wind ? ArenaColor.Object : ArenaColor.Enemy);
        if (WolfOfStone != null)
            Arena.Actor(WolfOfStone, Aspects[pcSlot] == Aspect.Stone ? ArenaColor.Object : ArenaColor.Enemy);
    }

    private void SetAspect(int slot, Aspect a)
    {
        if (slot >= 0)
            Aspects[slot] = a;
    }

    public Actor? MatchingWolf(Actor player)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        return slot >= 0
            ? Aspects[slot] switch
            {
                Aspect.Stone => WolfOfStone,
                Aspect.Wind => WolfOfWind,
                _ => null
            }
            : null;
    }

    public Actor? OtherWolf(Actor player)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        return slot >= 0
            ? Aspects[slot] switch
            {
                Aspect.Stone => WolfOfWind,
                Aspect.Wind => WolfOfStone,
                _ => null
            }
            : null;
    }
}

class StalkingWindyStone(BossModule module) : Components.CastCounter(module, null)
{
    private record struct Bait(Actor Source, Actor Target, DateTime Activation)
    {
        public readonly bool Hits(Actor player) => player.Position.InRect(Source.Position, Source.AngleTo(Target), 40, 0, 3);
    }

    private IEnumerable<Actor> PlayersHitBy(Bait b) => Raid.WithoutSlot().Where(r => r.Role != Role.Tank && b.Hits(r));

    private readonly List<Bait> Baits = [];
    private readonly WolfOfWindyStone? _wolves = module.FindComponent<WolfOfWindyStone>();

    private void AddBait(Actor target)
    {
        if (_wolves?.OtherWolf(target) is { } from)
            Baits.Add(new(from, target, WorldState.FutureTime(5.1f)));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LockOn)
            AddBait(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_StalkingStone or AID._Ability_StalkingWind)
        {
            NumCasts++;
            Baits.Clear();
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in Baits)
            DrawBait(pc, b);
    }

    private void DrawBait(Actor pc, Bait b)
    {
        if (b.Target == pc)
            Arena.AddRect(b.Source.Position, b.Source.DirectionTo(pc).Normalized(), 40, 0, 3, ArenaColor.Safe);
        else
            Arena.ZoneRect(b.Source.Position, b.Source.AngleTo(b.Target), 40, 0, 3, pc.Role == Role.Tank ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baits.Count == 0)
            return;

        if (actor.Role == Role.Tank)
        {
            if (Baits.Any(b => b.Hits(actor)))
                hints.Add("GTFO from stack!");
        }
        else
        {
            var mine = Baits.FindIndex(b => b.Target == actor);
            if (mine >= 0)
                hints.Add("Stack!", !PlayersHitBy(Baits[mine]).Exclude(actor).Any());
            else
                hints.Add("Stack!", !Baits.Any(b => b.Hits(actor)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in Baits)
            hints.PredictedDamage.Add((Raid.WithSlot().Where(p => b.Hits(p.Item2)).Mask(), b.Activation));
    }
}

class AlphaWindyStone(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly WolfOfWindyStone? _wolves = module.FindComponent<WolfOfWindyStone>();

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LockOn && CurrentBaits.Count == 0)
        {
            foreach (var t in Raid.WithoutSlot().Where(r => r.Role == Role.Tank))
                AddBait(t);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AlphaStone or AID._Ability_AlphaStone)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    private void AddBait(Actor target)
    {
        if (_wolves?.OtherWolf(target) is not { } src)
            return;

        CurrentBaits.Add(new(src, target, new AOEShapeCone(40, 30.Degrees()), WorldState.FutureTime(5.1f)));
    }
}

class EarthyWindborneEnd : BossComponent
{
    public record struct Debuff(int Order, Aspect Aspect, DateTime Expire);

    public readonly Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];

    public EarthyWindborneEnd(BossModule module) : base(module)
    {
        foreach (var (slot, player) in Raid.WithSlot())
        {
            var ix = Array.FindIndex(player.Statuses, s => (SID)s.ID is SID._Gen_EarthborneEnd or SID._Gen_WindborneEnd);
            if (ix >= 0)
            {
                var status = player.Statuses[ix];
                var remaining = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
                var order = remaining > 50 ? 3 : remaining > 30 ? 2 : 1;
                Debuffs[slot] = new(order, status.ID == (uint)SID._Gen_EarthborneEnd ? Aspect.Stone : Aspect.Wind, status.ExpireAt);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_EarthborneEnd or SID._Gen_WindborneEnd)
            Debuffs[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Debuffs[slot].Order > 0)
            hints.Add($"Order: {Debuffs[slot].Order}", false);

        if (ShouldCleanse(slot))
            hints.Add("Cleanse!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var e in Module.Enemies(OID._Gen_FontOfEarthAether))
            Arena.ZoneCircle(e.Position, 5, ShouldCleanse(pcSlot) && Debuffs[pcSlot].Aspect == Aspect.Stone ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        foreach (var e in Module.Enemies(OID._Gen_FontOfWindAether))
            Arena.ZoneCircle(e.Position, 5, ShouldCleanse(pcSlot) && Debuffs[pcSlot].Aspect == Aspect.Wind ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    private bool ShouldCleanse(int slot) => Debuffs[slot].Order > 0 && Debuffs[slot].Expire < WorldState.FutureTime(10);
}
