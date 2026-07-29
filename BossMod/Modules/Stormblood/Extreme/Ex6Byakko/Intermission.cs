namespace BossMod.Stormblood.Extreme.Ex6Byakko;

class VoiceOfThunder(BossModule module) : BossComponent(module)
{
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.AramitamaSoul);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
        {
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var o = orbs[i];
                forbidden[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircleOutline(orbs[i].Position, 2f, Colors.Safe);
    }
}

class Intermission(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures && state is 0x00040008 or 0x00100020)
            Active = state == 0x00040008;
    }
}

class IntermissionOrbAratama(BossModule module) : Components.GenericAOEs(module, (uint)AID.IntermissionOrbAratama)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IntermissionOrbSpawn:
                AOEs.Add(new(_shape, spell.TargetXZ, default, WorldState.FutureTime(5.1d)));
                break;
            case (uint)AID.IntermissionOrbAratama:
                ++NumCasts;
                var count = AOEs.Count;
                var pos = spell.TargetXZ;
                for (var i = 0; i < count; ++i)
                {
                    if (AOEs[i].Origin.AlmostEqual(pos, 1f))
                    {
                        AOEs.RemoveAt(i);
                        return;
                    }
                }
                break;
        }
    }
}

class IntermissionSweepTheLeg(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IntermissionSweepTheLeg, new AOEShapeDonut(5f, 25f));
class ImperialGuard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImperialGuard, new AOEShapeRect(44.75f, 2.5f));
class FellSwoop(BossModule module) : Components.CastCounter(module, (uint)AID.FellSwoop);
