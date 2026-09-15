namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class QuakeIII(BossModule module) : Components.CastCounter(module, AID.QuakeIIIStack)
{
    public readonly List<Actor> Baits = [];
    private DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Stack)
        {
            _activation = WorldState.FutureTime(5.1f);
            Baits.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
        {
            Baits.Clear();
            _activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var isBait = Baits.Contains(pc);

        foreach (var b in Baits)
        {
            var isSatisfied = Raid.WithoutSlot().OnSamePlatform(b).Count() == 4;

            uint color;

            // this is our bait, follow convention (green = stack)
            if (b == pc)
                color = ArenaColor.SafeFromAOE;
            // other bait, follow convention (yellow = forbidden stack)
            else if (isBait)
                color = ArenaColor.AOE;
            // stack is satisfied without us being there
            else if (isSatisfied && !P2Platforms.SamePlatform(pc, b))
                color = ArenaColor.AOE;
            // stack with target
            else
                color = ArenaColor.SafeFromAOE;

            P2Platforms.ZonePlatform(Arena, P2Platforms.GetPlatform(b), color);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default || Baits.Count == 0)
            return;

        var isBait = Baits.Contains(actor);
        var otherBaits = Baits.Exclude(actor);

        if (isBait)
        {
            if (otherBaits.OnSamePlatform(actor).Any())
                hints.Add("GTFO from other stack!");

            hints.Add("Stack with party!", Raid.WithoutSlot().OnSamePlatform(actor).Count() != 4);
        }
        else
            hints.Add("Stack!", !Baits.OnSamePlatform(actor).Any());
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in Baits)
            hints.AddPredictedDamage(Raid.WithSlot().OnSamePlatform(b).Mask(), _activation);
    }
}

class Twinbite(BossModule module) : Components.CastCounter(module, AID.Twinbite)
{
    public readonly List<Actor> Baits = [];
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TwinbiteCast)
        {
            Baits.AddRange(RaidByEnmity(caster).Take(2));
            _activation = Module.CastFinishAt(spell, 0.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction && Baits.Count > 0)
            Baits.RemoveAt(0);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in Baits)
            P2Platforms.ZonePlatform(Arena, P2Platforms.GetPlatform(b), ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baits.Contains(actor))
            hints.Add("Bait away from raid!", Raid.WithoutSlot().OnSamePlatform(actor).Count() > 1);
        else if (Baits.OnSamePlatform(actor).Any())
            hints.Add("GTFO from tanks!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddPredictedDamage(Raid.WithSlot().Where(r => Baits.Any(b => b.TargetID == r.Item2.InstanceID)).Mask(), _activation, AIHints.PredictedDamageType.Tankbuster);
    }
}
