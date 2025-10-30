namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class HowlingHavoc(BossModule module) : Components.RaidwideCast(module, AID.HowlingHavoc);
class AddsVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation > WorldState.CurrentTime)
            yield return new(new AOEShapeCircle(8), Arena.Center, Activation: _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 1)
        {
            switch (state)
            {
                case 0x00200010:
                    _activation = WorldState.FutureTime(5.6f); // verify
                    break;
                case 0x00020001:
                    Arena.Bounds = new ArenaBoundsCustom(12, new(CurveApprox.Donut(8, 12, 1 / 90f)), RM08SHowlingBlade.MapResolution);
                    break;
                case 0x00080004:
                    Arena.Bounds = new ArenaBoundsCircle(12, RM08SHowlingBlade.MapResolution);
                    break;
            }
        }
    }
}

// using flags enum because status gain/loss order is not consistent, i.e. actor can have stonepack and windpack simultaneously for one frame
[Flags]
public enum Aspect
{
    None,
    Wind = 1,
    Stone = 2,
}

class WolfOfWindStone(BossModule module) : BossComponent(module)
{
    public Actor? WolfOfWind { get; private set; }
    public Actor? WolfOfStone { get; private set; }

    public readonly Aspect[] Aspects = new Aspect[PartyState.MaxPartySize];

    public override void OnTargetable(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.WolfOfWindTactical:
                WolfOfWind = actor;
                break;
            case OID.WolfOfStoneTactical:
                WolfOfStone = actor;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Stonepack:
                SetAspect(Raid.FindSlot(actor.InstanceID), Aspect.Stone);
                break;
            case SID.Windpack:
                SetAspect(Raid.FindSlot(actor.InstanceID), Aspect.Wind);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Stonepack:
                ClearAspect(Raid.FindSlot(actor.InstanceID), Aspect.Stone);
                break;
            case SID.Windpack:
                ClearAspect(Raid.FindSlot(actor.InstanceID), Aspect.Wind);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Aspects[slot].HasFlag(Aspect.Wind))
            hints.SetPriority(WolfOfWind, AIHints.Enemy.PriorityInvincible);
        if (Aspects[slot].HasFlag(Aspect.Stone))
            hints.SetPriority(WolfOfStone, AIHints.Enemy.PriorityInvincible);
    }

    private void SetAspect(int slot, Aspect a)
    {
        if (slot >= 0)
            Aspects[slot] |= a;
    }

    private void ClearAspect(int slot, Aspect a)
    {
        if (slot >= 0)
            Aspects[slot] &= ~a;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (WolfOfWind != null)
            Arena.Actor(WolfOfWind, Aspects[pcSlot].HasFlag(Aspect.Wind) ? ArenaColor.Object : ArenaColor.Enemy);
        if (WolfOfStone != null)
            Arena.Actor(WolfOfStone, Aspects[pcSlot].HasFlag(Aspect.Stone) ? ArenaColor.Object : ArenaColor.Enemy);
    }

    public Actor? MatchingWolf(Actor player)
    {
        return Raid.TryFindSlot(player, out var slot)
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
        return Raid.TryFindSlot(player, out var slot)
            ? Aspects[slot] switch
            {
                Aspect.Stone => WolfOfWind,
                Aspect.Wind => WolfOfStone,
                _ => null
            }
            : null;
    }
}

class StalkingWindStone(BossModule module) : Components.CastCounter(module, null)
{
    public record struct Bait(Actor Source, Actor Target, DateTime Activation)
    {
        public readonly bool Hits(Actor player) => player.Position.InRect(Source.Position, Source.AngleTo(Target), 40, 0, 3);
    }

    private IEnumerable<Actor> PlayersHitBy(Bait b) => Raid.WithoutSlot().Where(b.Hits);

    public readonly List<Bait> Baits = [];
    private readonly WolfOfWindStone? _wolves = module.FindComponent<WolfOfWindStone>();

    private void AddBait(Actor target)
    {
        if (_wolves?.OtherWolf(target) is { } from)
            Baits.Add(new(from, target, WorldState.FutureTime(5.1f)));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Target)
            AddBait(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StalkingStone or AID.StalkingWind)
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
            Arena.AddRect(b.Source.Position, b.Source.DirectionTo(pc).Normalized(), 40, 0, 3, ArenaColor.Danger);
        else if (pc.Role == Role.Tank || Baits.Any(b => b.Target == pc))
            Arena.ZoneRect(b.Source.Position, b.Source.AngleTo(b.Target), 40, 0, 3, ArenaColor.AOE);
        else
            Arena.AddRect(b.Source.Position, b.Source.DirectionTo(b.Target).Normalized(), 40, 0, 3, ArenaColor.Danger);
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
            {
                var otherBaits = Baits.Select(b => b.Target).Exclude(actor);
                hints.Add("Bait away from raid!", PlayersHitBy(Baits[mine]).Any(x => x.Role == Role.Tank || otherBaits.Contains(x)));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in Baits)
            hints.AddPredictedDamage(Raid.WithSlot().Where(p => b.Hits(p.Item2)).Mask(), b.Activation);
    }
}

class AlphaWindStone(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly WolfOfWindStone? _wolves = module.FindComponent<WolfOfWindStone>();

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Target && CurrentBaits.Count == 0)
        {
            foreach (var t in Raid.WithoutSlot().Where(r => r.Role == Role.Tank))
                AddBait(t);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AlphaStone or AID.AlphaStone)
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

    public const float Radius = 2f;

    public bool StoneOff;
    public bool WindOff;

    public EarthyWindborneEnd(BossModule module) : base(module)
    {
        foreach (var (slot, player) in Raid.WithSlot())
        {
            var ix = Array.FindIndex(player.Statuses, s => (SID)s.ID is SID.EarthborneEnd or SID.WindborneEnd);
            if (ix >= 0)
            {
                var status = player.Statuses[ix];
                var remaining = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
                var order = remaining > 50 ? 3 : remaining > 30 ? 2 : 1;
                Debuffs[slot] = new(order, status.ID == (uint)SID.EarthborneEnd ? Aspect.Stone : Aspect.Wind, status.ExpireAt);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        StoneOff |= spell.Action.ID == (uint)AID.SandSurgeFinal;
        WindOff |= spell.Action.ID == (uint)AID.WindSurgeFinal;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.EarthborneEnd or SID.WindborneEnd)
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
        if (!StoneOff)
            foreach (var e in Module.Enemies(OID.FontOfEarthAether))
                Arena.ZoneCircle(e.Position, Radius, ShouldCleanse(pcSlot) && Debuffs[pcSlot].Aspect.HasFlag(Aspect.Stone) ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        if (!WindOff)
            foreach (var e in Module.Enemies(OID.FontOfWindAether))
                Arena.ZoneCircle(e.Position, Radius, ShouldCleanse(pcSlot) && Debuffs[pcSlot].Aspect.HasFlag(Aspect.Wind) ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    private bool ShouldCleanse(int slot) => Debuffs[slot].Order > 0 && Debuffs[slot].Expire < WorldState.FutureTime(10);
}

class ForlornWindStone(BossModule module) : Components.CastCounterMulti(module, [AID.ForlornStoneCast, AID.ForlornWind])
{
    public readonly List<Actor> Casters = [];
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (WatchedActions.Contains(spell.Action))
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (WatchedActions.Contains(spell.Action))
            Casters.Remove(caster);
    }
}

class RavenousSaber(BossModule module) : Components.CastCounter(module, null)
{
    private readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RavenousSaber1 or AID.RavenousSaber2 or AID.RavenousSaber3 or AID.RavenousSaber4 or AID.RavenousSaber5)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RavenousSaber1 or AID.RavenousSaber2 or AID.RavenousSaber3 or AID.RavenousSaber4 or AID.RavenousSaber5)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Raidwide");
    }
}
