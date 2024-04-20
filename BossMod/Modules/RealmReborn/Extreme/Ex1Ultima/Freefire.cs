namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class Freefire(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Freefire))
{
    private readonly List<Actor> _casters = [];
    private DateTime _resolve;
    public bool Active => _casters.Count > 0;

    private static readonly AOEShapeCircle _shape = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(_shape, c.Position, new(), _resolve);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active && Module.PrimaryActor.TargetID == actor.InstanceID && NumCasts > 0)
        {
            // for second set, let current MT stay in place and use invuln instead of risking cleaving the raid
            var invuln = actor.Class switch
            {
                Class.WAR => ActionID.MakeSpell(WAR.AID.Holmgang),
                Class.PLD => ActionID.MakeSpell(PLD.AID.HallowedGround),
                _ => new()
            };
            if (invuln)
            {
                hints.PlannedActions.Add((invuln, actor, (float)(_resolve - WorldState.CurrentTime).TotalSeconds, false));
                return;
            }
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Helper && id == 0x0449)
        {
            _casters.Add(actor);
            _resolve = WorldState.FutureTime(6);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}
