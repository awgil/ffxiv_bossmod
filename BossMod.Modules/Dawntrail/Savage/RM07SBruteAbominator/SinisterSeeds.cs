namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class SporeSac(BossModule module) : Components.StandardAOEs(module, AID.SporeSac, 8);
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, 8);
class SinisterSeedsSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.SinisterSeedsSpread, 6);
class SinisterSeedsChase(BossModule module) : Components.StandardAOEs(module, AID.SinisterSeedsChase, 7);

class SinisterSeedsStored(BossModule module) : Components.GenericAOEs(module, default, "GTFO from puddle!")
{
    private readonly List<WPos> Casts = [];

    private bool Active;
    private DateTime Activation;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SinisterSeedsChase)
            Casts.Add(spell.TargetXZ);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RootsOfEvil)
            Active = false;
    }

    public void Activate()
    {
        Active = true;
        Activation = WorldState.FutureTime(5.4f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Active ? Casts.Select(s => new AOEInstance(new AOEShapeCircle(12), s, default, Activation)) : [];
}

class RootsOfEvil(BossModule module) : Components.StandardAOEs(module, AID.RootsOfEvil, 12);

class Impact : Components.UniformStackSpread
{
    public int NumCasts;

    public Impact(BossModule module) : base(module, 6, 0, 2, 4)
    {
        AddStacks(Raid.WithoutSlot().Where(r => r.Role == Role.Healer).Take(2), WorldState.FutureTime(4.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Impact)
        {
            NumCasts++;
            if (Stacks.Count > 0)
                Stacks.RemoveAt(0);
        }
    }
}
