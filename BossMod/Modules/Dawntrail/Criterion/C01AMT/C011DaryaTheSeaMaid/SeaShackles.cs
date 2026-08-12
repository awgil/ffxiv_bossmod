namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

class SeaShackles(BossModule module) : Components.StretchTetherDuo(module, minimumDistance: 16f, activationDelay: 28f, tetherIDBad: (uint)TetherID.Far, tetherIDGood: (uint)TetherID.Safe)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var (player, partner) = DeterminePlayerTetherSides(source, tether);
        if (player is null || partner is null)
        {
            return;
        }

        if (!ActivationDelayOnActor.Any(x => x.Item1 == player))
        {
            ActivationDelayOnActor.Add((player, WorldState.FutureTime(ActivationDelay)));
        }

        CurrentBaits.Add(new(partner, player, Shape ?? new AOEShapeCircle(default), ActivationDelayOnActor.FirstOrDefault(x => x.Item1 == player).Item2));
        TetherOnActor.Add((player, tether.ID));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var (player, partner) = DeterminePlayerTetherSides(source, tether);
        if (player is null || partner is null)
        {
            return;
        }

        uint tetherId = tether.ID;
        CurrentBaits.RemoveAll(b => b.Target == player && b.Source == partner);
        TetherOnActor.RemoveAll(t => t.Item1 == player && t.Item2 == tetherId);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ActiveBaits.Count <= 1)
        {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);

        var baits = ActiveBaits;
        foreach (var bait in baits)
        {
            if (bait.Source != pc && bait.Target != pc)
            {
                continue;
            }

            var tetherId = TetherOnActor.FirstOrDefault(t => t.Item1 == bait.Target).Item1 != null
                ? TetherOnActor.FirstOrDefault(t => t.Item1 == bait.Target).Item2
                : TetherOnActor.FirstOrDefault(t => t.Item1 == bait.Source).Item2;

            uint color = tetherId == (uint)TetherID.Safe ? Colors.Safe : Colors.Danger;
            Arena.AddLine(bait.Source.Position, bait.Target.Position, color);
        }
    }

    private (Actor? Player, Actor? Partner) DeterminePlayerTetherSides(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (uint)TetherID.Safe and not (uint)TetherID.Far and not (uint)TetherID.Near)
        {
            return (null, null);
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return (null, null);
        }

        bool sourceIsPlayer = Raid.WithoutSlot().Contains(source);
        bool targetIsPlayer = Raid.WithoutSlot().Contains(target);
        if (!sourceIsPlayer || !targetIsPlayer)
        {
            return (null, null);
        }

        return (source, target);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (ActiveBaits.Count <= 1)
        {
            return PlayerPriority.Irrelevant;
        }

        if (player == pc)
        {
            return PlayerPriority.Normal;
        }

        foreach (var bait in CurrentBaits)
        {
            if (bait.Source == pc && bait.Target == player || bait.Source == player && bait.Target == pc)
            {
                return PlayerPriority.Interesting;
            }
        }

        return PlayerPriority.Irrelevant;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaits.Count <= 1)
        {
            return;
        }

        var bait = CurrentBaits.FirstOrDefault(b => b.Source == actor || b.Target == actor);
        if (bait.Target == null || bait.Source == null)
        {
            return;
        }

        var partner = bait.Target == actor ? bait.Source : bait.Target;
        var tether = TetherOnActor.FirstOrDefault(t => t.Item1 == actor || t.Item1 == partner);

        switch (tether.Item2)
        {
            case (uint)TetherID.Safe:
                hints.Add("Tether distance is correct");
                return;
            case (uint)TetherID.Far:
                hints.Add("Move closer to tether partner!");
                return;
            case (uint)TetherID.Near:
                hints.Add("Move away from tether partner!");
                break;
        }
    }
}

class HydrobulletStack(BossModule module) : Components.UniformStackSpread(module, 15, 0, 1)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HydrobulletTarget)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrobullet2)
        {
            Stacks.Clear();
        }
    }
}