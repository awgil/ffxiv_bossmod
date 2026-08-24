namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2EarthMissileBaited(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.EarthMissileBaited, m => m.Enemies(OID.VoidzoneEarthMissileBaited).Where(z => z.EventState != 7), 0.9f);

class P2EarthMissileIce(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 10, AID.EarthMissileIce, Voidzones, 0.8f) // TODO: verify larger radius...
{
    private static IEnumerable<Actor> Voidzones(BossModule m)
    {
        foreach (var z in m.Enemies(OID.VoidzoneEarthMissileIceSmall).Where(z => z.EventState != 7))
        {
            yield return z;
            yield break;
        }
        foreach (var z in m.Enemies(OID.VoidzoneEarthMissileIceLarge).Where(z => z.EventState != 7))
        {
            yield return z;
            yield break;
        }
    }
}

// note: we use a single spread/stack component for both enumerations and ice missile spreads, since they happen at the same time
// TODO: add hint for spread target to stay close to tornado...
class P2Enumeration(BossModule module) : Components.UniformStackSpread(module, 5, 6, 3, 3, true, false) // TODO: verify enumeration radius
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.Enumeration:
                // note: we assume tanks never share enumeration
                AddStack(actor, WorldState.FutureTime(5.1f), Raid.WithSlot(true).WhereActor(p => p.Role == Role.Tank).Mask());
                break;
            case IconID.EarthMissileIce:
                AddSpread(actor, WorldState.FutureTime(5.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Enumeration:
                Stacks.Clear();
                break;
            case AID.EarthMissileIce:
                Spreads.Clear();
                break;
        }
    }
}

class P2HiddenMinefield(BossModule module) : Components.StandardAOEs(module, AID.HiddenMinefield, new AOEShapeCircle(5))
{
    private readonly List<WPos> _mines = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in _mines)
            Arena.Actor(m, default, ArenaColor.Object);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            _mines.Add(caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID is AID.HiddenMine or AID.HiddenMineShrapnel)
            _mines.RemoveAll(m => m.AlmostEqual(caster.Position, 1));
    }
}
