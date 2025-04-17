namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class ManifoldFlames : Components.UniformStackSpread
{
    public ManifoldFlames(BossModule module) : base(module, 0, 6)
    {
        AddSpreads(Raid.WithoutSlot(true));
    }
}

class NestOfFlamevipersCommon(BossModule module) : Components.CastCounter(module, AID.NestOfFlamevipersAOE)
{
    protected BitMask BaitingPlayers;

    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Raid.WithSlot().IncludedInMask(BaitingPlayers).WhereActor(p => p != actor && _shape.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(p.Position - Module.PrimaryActor.Position))).Any())
            hints.Add("GTFO from baited aoe!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (_, player) in Raid.WithSlot().IncludedInMask(BaitingPlayers))
            _shape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(player.Position - Module.PrimaryActor.Position));
    }
}

// variant that happens right after manifold flames and baits to 4 closest players
class NestOfFlamevipersBaited(BossModule module) : NestOfFlamevipersCommon(module)
{
    private BitMask _forbiddenPlayers;
    public bool Active => NumCasts == 0 && _forbiddenPlayers.Any();

    public override void Update()
    {
        BaitingPlayers = Active ? Raid.WithSlot().SortedByRange(Module.PrimaryActor.Position).Take(4).Mask() : new();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Active)
        {
            bool shouldBait = !_forbiddenPlayers[slot];
            hints.Add(shouldBait ? "Move closer to bait" : "GTFO to avoid baits", shouldBait != BaitingPlayers[slot]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.HemitheosFlare)
            _forbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
    }
}

// variant that happens when cast is started and baits to everyone
class NestOfFlamevipersEveryone(BossModule module) : NestOfFlamevipersCommon(module)
{
    public bool Active => NumCasts == 0 && BaitingPlayers.Any();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NestOfFlamevipers)
            BaitingPlayers = Raid.WithSlot().Mask();
    }
}
