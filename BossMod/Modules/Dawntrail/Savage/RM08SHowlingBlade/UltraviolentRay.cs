namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class GleamingBeam(BossModule module) : Components.StandardAOEs(module, AID.GleamingBeam, new AOEShapeRect(31, 4));

class UVRBait(BossModule module) : Components.CastCounter(module, AID.UltraviolentRay)
{
    public readonly List<Actor> Baits = [];
    private DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Divebomb)
        {
            Baits.Add(actor);
            _activation = WorldState.FutureTime(6.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
            Baits.RemoveAt(0);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Baits.Contains(pc))
        {
            foreach (var b in Baits.Exclude(pc))
                P2Platforms.ZonePlatform(Arena, P2Platforms.GetPlatform(b), ArenaColor.AOE);
        }
        else
        {
            for (var i = 0; i < 5; i++)
                if (Baits.OnPlatform(i).Count() > 1)
                    P2Platforms.ZonePlatform(Arena, i, ArenaColor.AOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baits.Contains(actor) && Baits.OnSamePlatform(actor).Count() > 1)
            hints.Add("GTFO from other baits!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activation);
    }
}
