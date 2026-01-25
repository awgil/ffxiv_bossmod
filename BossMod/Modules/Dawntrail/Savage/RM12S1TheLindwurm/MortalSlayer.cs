namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class MortalSlayer(BossModule module) : Components.GenericStackSpread(module)
{
    readonly List<Pair> Baits = [];
    readonly List<(Actor, Orb)> _unassigned = [];
    readonly DateTime[] _poisoned = new DateTime[8];

    class Side
    {
        public readonly List<(Actor, Orb)> Orbs = [];

        public override string ToString() => Orbs.Count == 0
            ? "none"
            : Orbs.Count == 1
                ? (Orbs[0].Item2 & Orb.Color).ToString()
                : ((Orbs[0].Item2 & Orb.Color) == (Orbs[1].Item2 & Orb.Color))
                    ? $"{Orbs[0].Item2 & Orb.Color} x2"
                    : $"{Orbs[0].Item2 & Orb.Color} close, {Orbs[1].Item2 & Orb.Color} far";
    }
    class Pair
    {
        public Side Left = new();
        public Side Right = new();

        public DateTime Activation;

        public void Push((Actor, Orb) ao)
        {
            var (a, o) = ao;
            if (o.HasFlag(Orb.Left))
                Left.Orbs.Add((a, o));
            if (o.HasFlag(Orb.Right))
                Right.Orbs.Add((a, o));
        }

        public override string ToString() => Left.Orbs.Count == 0 ? $"R={Right}" : Right.Orbs.Count == 0 ? $"L={Left}" : $"L={Left}; R={Right}";
    }

    [Flags]
    enum Orb
    {
        None = 0,

        Tank = 0x01,
        Spread = 0x02,
        Left = 0x10,
        Right = 0x20,

        Color = 0x03,
        Side = 0x30,
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        for (var i = 0; i < Math.Min(2, Baits.Count); i++)
        {
            if (i == 0)
                hints.Add($"Next: {Baits[i]}");
            else
                hints.Add($"Then: {Baits[i]}");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveSpreads.FirstOrNull(s => s.Target == actor) is { } spread && _poisoned[slot] > spread.Activation)
            hints.Add("Avoid baiting!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void OnActorCreated(Actor actor)
    {
        var lr = actor.Position.X < 100 ? Orb.Left : Orb.Right;
        switch ((OID)actor.OID)
        {
            case OID.RedOrb:
                _unassigned.Add((actor, lr | Orb.Tank));
                break;
            case OID.GreenOrb:
                _unassigned.Add((actor, lr | Orb.Spread));
                break;
        }
        if (_unassigned.Count == 2)
        {
            Pair p = new();
            p.Push(_unassigned[0]);
            p.Push(_unassigned[1]);
            p.Left.Orbs.SortBy(o => o.Item1.Position.X);
            p.Right.Orbs.SortByReverse(o => o.Item1.Position.X);
            p.Activation = Baits.Count > 0 ? Baits[^1].Activation.AddSeconds(3) : WorldState.FutureTime(10.7f);
            Baits.Add(p);
            _unassigned.Clear();
        }
    }

    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PoisonResistanceDownII && Raid.TryFindSlot(actor, out var slot))
            _poisoned[slot] = status.ExpireAt;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MortalSlayerSpread or AID.MortalSlayerTank)
        {
            NumCasts++;
            if (NumCasts % 2 == 0 && Baits.Count > 0)
                Baits.RemoveAt(0);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var i = 0;
        foreach (var b in Baits.Take(2))
        {
            void draw(WPos p, float r, uint t)
            {
                if (i == 0)
                    Arena.AddCircleFilled(p, r, t);
                else
                    Arena.AddCircle(p, r, t);
            }
            foreach (var (orba, orb) in b.Left.Orbs)
                draw(orba.Position with { Z = 82 }, 1, orb.HasFlag(Orb.Tank) ? ArenaColor.Vulnerable : ArenaColor.Safe);
            foreach (var (orba, orb) in b.Right.Orbs)
                draw(orba.Position with { Z = 82 }, 1, orb.HasFlag(Orb.Tank) ? ArenaColor.Vulnerable : ArenaColor.Safe);
            i++;
        }
    }

    public override void Update()
    {
        Spreads.Clear();

        foreach (var b in Baits.Take(1))
        {
            foreach (var (baiter, _) in Raid.WithoutSlot().Where(a => a.Position.X < 100).SortedByRange(Module.PrimaryActor.Position).Zip(b.Left.Orbs))
                Spreads.Add(new(baiter, 6, b.Activation));
            foreach (var (baiter, _) in Raid.WithoutSlot().Where(a => a.Position.X > 100).SortedByRange(Module.PrimaryActor.Position).Zip(b.Right.Orbs))
                Spreads.Add(new(baiter, 6, b.Activation));
        }
    }
}
