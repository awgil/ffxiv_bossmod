namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class SunderedRemains(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(10)); // TODO: max-casts...
class NSunderedRemains(BossModule module) : SunderedRemains(module, AID.NSunderedRemains);
class SSunderedRemains(BossModule module) : SunderedRemains(module, AID.SSunderedRemains);

class ScreamOfTheFallen(BossModule module) : Components.UniformStackSpread(module, 0, 15, alwaysShowSpreads: true)
{
    public int NumCasts { get; private set; }
    private BitMask _second;
    private readonly List<Actor> _towers = [];

    private const float _towerRadius = 3;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!IsSpreadTarget(actor))
            hints.Add("Soak the tower!", !ActiveTowers(_second[slot]).Any(t => t.Position.InCircle(actor.Position, _towerRadius)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!IsSpreadTarget(pc))
            foreach (var t in ActiveTowers(_second[pcSlot]))
                Arena.AddCircle(t.Position, _towerRadius, ArenaColor.Safe, 2);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FirstInLine:
                AddSpread(actor);
                break;
            case SID.SecondInLine:
                _second.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NExplosion or AID.SExplosion)
            _towers.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NExplosion or AID.SExplosion)
        {
            switch (++NumCasts)
            {
                case 2:
                    Spreads.Clear();
                    AddSpreads(Raid.WithSlot().IncludedInMask(_second).Actors());
                    break;
                case 4:
                    Spreads.Clear();
                    break;
            }
        }
    }

    private IEnumerable<Actor> ActiveTowers(bool second) => second ? _towers.Take(2) : _towers.Skip(2);
}
