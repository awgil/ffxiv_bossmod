namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class RuneAxe(BossModule module) : Components.GenericAOEs(module)
{
    public record struct Spread(Actor Target, float Radius, DateTime Activation)
    {
        public BitMask Platforms;
    }

    public readonly List<Spread> Spreads = [];

    public bool Enabled = true;

    private int NumActive => Math.Min(NumCasts == 0 ? 1 : 3, Spreads.Count);
    public IEnumerable<Spread> ActiveSpreads => NumCasts == 0 ? Spreads.Take(1) : Spreads.Take(3);

    private Spread? ActiveSpreadOn(Actor player) => ActiveSpreads.FirstOrNull(s => s.Target == player);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Enabled)
            yield break;

        var activePlatforms = ActiveSpreads.Where(s => s.Target != actor).Aggregate(default(BitMask), (m, s) => m | s.Platforms);

        foreach (var bit in activePlatforms.SetBits())
        {
            if (bit == 0)
                yield return new AOEInstance(FT04Magitaur.NotPlatforms, Arena.Center, default, Spreads[0].Activation);
            else
                yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + FT04Magitaur.Platforms[bit - 1].Item1, FT04Magitaur.Platforms[bit - 1].Item2, Spreads[0].Activation);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PreyLesserAxebit:
                Spreads.Add(new(actor, 5, status.ExpireAt));
                Spreads.SortBy(s => s.Activation);
                break;
            case SID.PreyGreaterAxebit:
                Spreads.Add(new(actor, 11, status.ExpireAt));
                Spreads.SortBy(s => s.Activation);
                break;
        }
    }

    public override void Update()
    {
        if (NumActive == 0)
            return;

        for (var i = 0; i < NumActive; i++)
        {
            var spread = Spreads[i];
            Spreads.Ref(i).Platforms.Reset();

            var platformIndex = 1;

            foreach (var (offset, angle) in FT04Magitaur.Platforms)
            {
                // if small spread, check if it's far enough from platform center that it will clip outer floor
                if (spread.Radius == 5 && spread.Target.Position.InRect(Arena.Center + offset, angle, 10, 10, 10) && !spread.Target.Position.InRect(Arena.Center + offset, angle, 5, 5, 5))
                    Spreads.Ref(i).Platforms.Set(0);

                if (Intersect.CircleRect(spread.Target.Position, spread.Radius, Arena.Center + offset, angle.ToDirection(), 10, 10))
                    Spreads.Ref(i).Platforms.Set(platformIndex);

                platformIndex++;
            }

            // large spread can't avoid clipping outer floor
            if (spread.Radius == 11)
                Spreads.Ref(i).Platforms.Set(0);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Enabled)
            return;

        foreach (var s in ActiveSpreads)
        {
            Arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Danger);

            if (s.Target == pc && s.Radius == 11)
                foreach (var p in FT04Magitaur.Platforms)
                    Arena.AddRect(Arena.Center + p.Item1, p.Item2.ToDirection(), 10, 10, 10, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RuinousRuneLarge or AID.RuinousRuneSmall)
        {
            NumCasts++;
            if (Spreads.Count > 0)
                Spreads.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Enabled)
            return;

        base.AddAIHints(slot, actor, assignment, hints);

        var center = Arena.Center;

        if (ActiveSpreadOn(actor) is { } spread)
        {
            if (spread.Radius == 11)
            {
                foreach (var (off, dir) in FT04Magitaur.Platforms)
                    hints.AddForbiddenZone(p => Intersect.CircleRect(p, spread.Radius, center + off, dir.ToDirection(), 10, 10), spread.Activation);
            }
            else
            {
                foreach (var (off, dir) in FT04Magitaur.Platforms)
                    if (actor.Position.InRect(Arena.Center + off, dir, 10, 10, 10))
                    {
                        hints.AddForbiddenZone(ShapeContains.InvertedRect(Arena.Center + off, dir, 5, 5, 5), spread.Activation);
                        break;
                    }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Enabled)
            return;

        if (ActiveSpreadOn(actor) is { } spread)
        {
            if (spread.Radius == 11)
                hints.Add("GTFO from platforms!", (spread.Platforms.Raw & 0b1110) != 0);
        }
        else
            base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveSpreads.Any(t => t.Target == player) ? PlayerPriority.Danger : PlayerPriority.Normal;
}
