namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class PrincelyBlow(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 5), (uint)IconID.TankbusterKnockback, AID.PrincelyBlow, 8.3f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public bool BridgePhase;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BridgePhase)
        {
            // during bridge phase, we only try to avoid other tanks, and ignore allies (it's their job to make room for us)
            var predicted = new BitMask();
            foreach (var b in ActiveBaits)
            {
                predicted.Set(Raid.FindSlot(b.Target.InstanceID));
                if (b.Target != actor)
                    hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            }

            if (predicted.Any())
                hints.AddPredictedDamage(predicted, CurrentBaits[0].Activation, DamageType);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class PrincelyBlowKnockback(BossModule module) : Components.Knockback(module, AID.PrincelyBlow)
{
    private DateTime _activation;
    private readonly List<Actor> _targets = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var t in _targets)
            yield return new(Module.PrimaryActor.Position, 30, _activation, new AOEShapeRect(60, 5), Module.PrimaryActor.AngleTo(t), Kind: Kind.DirForward);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targets.Contains(actor) && !StopAtWall && !IsImmune(slot, _activation))
            hints.AddForbiddenZone(ShieldBash.SafetyShape(Module.PrimaryActor.Position), _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.TankbusterKnockback && WorldState.Actors.Find(targetID) is { } tar)
        {
            _activation = WorldState.FutureTime(8.3f);
            _targets.Add(tar);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _targets.Clear();
        }
    }
}
