namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ValorousAscensionRaidwide(BossModule module) : Components.RaidwideCast(module, AID.ValorousAscension1);

class ValorousAscensionRect(BossModule module) : Components.GenericAOEs(module, AID.ValorousAscensionRect)
{
    public readonly List<(Actor Caster, DateTime Activation)> Casters = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DB && actor.OID == (uint)OID.BriarThorn)
        {
            Casters.Add((actor, WorldState.FutureTime(10.8f)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = Casters.FindIndex(c => c.Caster == caster);
            if (ix >= 0)
                Casters.Ref(ix).Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Caster == caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Take(2).Select(c => new AOEInstance(new AOEShapeRect(40, 4), c.Caster.Position, c.Caster.Rotation, c.Activation));
}
