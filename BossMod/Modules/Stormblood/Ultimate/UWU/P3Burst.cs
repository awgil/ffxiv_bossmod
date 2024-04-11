namespace BossMod.Stormblood.Ultimate.UWU;

class P3Burst(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Burst))
{
    private IReadOnlyList<Actor> _bombs = module.Enemies(OID.BombBoulder);
    private Dictionary<ulong, DateTime?> _bombActivation = new();

    private static readonly AOEShape _shape = new AOEShapeCircle(6.3f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _bombs)
        {
            var activation = _bombActivation.GetValueOrDefault(b.InstanceID);
            if (activation != null)
                yield return new(_shape, b.Position, b.Rotation, b.CastInfo?.NPCFinishAt ?? activation.Value);
        }
    }

    public override void Update()
    {
        foreach (var b in _bombs.Where(b => !_bombActivation.ContainsKey(b.InstanceID)))
            _bombActivation[b.InstanceID] = WorldState.FutureTime(6.5f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _bombActivation[caster.InstanceID] = null;
    }
}
