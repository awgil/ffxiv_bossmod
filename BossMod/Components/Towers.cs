﻿namespace BossMod.Components;

public class GenericTowers(BossModule module, ActionID aid = default) : CastCounter(module, aid)
{
    public struct Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, BitMask forbiddenSoakers = default, DateTime activation = default)
    {
        public WPos Position = position;
        public float Radius = radius;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public BitMask ForbiddenSoakers = forbiddenSoakers;
        public DateTime Activation = activation;

        public readonly bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);
        public readonly int NumInside(BossModule module) => module.Raid.WithSlot().ExcludedFromMask(ForbiddenSoakers).InRadius(Position, Radius).Count();
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
    }

    public List<Tower> Towers = [];

    // default tower styling
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        if (arena.Config.ShowOutlinesAndShadows)
            arena.AddCircle(pos, radius, 0xFF000000, 3);
        arena.AddCircle(pos, radius, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Towers.Any(t => t.ForbiddenSoakers[slot] && t.IsInside(actor)))
        {
            hints.Add("GTFO from tower!");
        }
        else if (Towers.FindIndex(t => !t.ForbiddenSoakers[slot] && t.IsInside(actor)) is var soakedIndex && soakedIndex >= 0) // note: this assumes towers don't overlap
        {
            var count = Towers[soakedIndex].NumInside(Module);
            if (count < Towers[soakedIndex].MinSoakers)
                hints.Add("Too few soakers in the tower!");
            else if (count > Towers[soakedIndex].MaxSoakers)
                hints.Add("Too many soakers in the tower!");
        }
        else if (Towers.Any(t => !t.ForbiddenSoakers[slot] && !t.CorrectAmountInside(Module)))
        {
            hints.Add("Soak the tower!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count == 0)
            return;

        // we consider some list of towers part of the same "group" if their activations are within 500ms, as there can be varying delays between helper actors in an encounter casting the "same" spell
        // generally, successive towers that are meant to be soaked by one player (i.e. in quest battles) activate no more frequently than about 2 seconds apart, so this is pretty conservative
        var firstActivation = Towers.MinBy(t => t.Activation).Activation;
        var deadline = firstActivation.AddSeconds(0.5f);

        // first see if we have one or more towers we need to soak - if so, add hints to take one of them
        // if there are no towers to soak, add hints to avoid forbidden ones
        // note that if we're currently inside a tower that has min number of soakers, we can't leave it
        List<Func<WPos, float>> zones = [];
        bool haveTowersToSoak = false;
        foreach (var t in Towers.Where(t => t.Activation <= deadline))
        {
            var effNumSoakers = t.ForbiddenSoakers[slot] ? int.MaxValue : t.NumInside(Module);
            if (effNumSoakers < t.MinSoakers || effNumSoakers == t.MinSoakers && t.IsInside(actor))
            {
                // this tower needs to be soaked; if this is the first one, clear out any previously found towers to avoid
                if (!haveTowersToSoak)
                {
                    zones.Clear();
                    haveTowersToSoak = true;
                }
                zones.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }
            else if (effNumSoakers > t.MaxSoakers && !haveTowersToSoak)
            {
                // this tower needs to be avoided; if we already have towers to soak, do nothing - presumably soaking other tower will automatically avoid this one
                zones.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }
        }
        var zoneUnion = ShapeDistance.Union(zones);
        hints.AddForbiddenZone(haveTowersToSoak ? p => -zoneUnion(p) : zoneUnion, firstActivation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in Towers)
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot]);
    }
}

public class CastTowers(BossModule module, ActionID aid, float radius, int minSoakers = 1, int maxSoakers = 1) : GenericTowers(module, aid)
{
    public float Radius = radius;
    public int MinSoakers = minSoakers;
    public int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(DeterminePosition(caster, spell), Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var pos = DeterminePosition(caster, spell);
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
        }
    }

    private WPos DeterminePosition(Actor caster, ActorCastInfo spell) => spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
}
