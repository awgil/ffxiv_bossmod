namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class Freefire : Components.GenericAOEs
{
    private List<Actor> _casters = new();
    private DateTime _resolve;
    public bool Active => _casters.Count > 0;

    private static readonly AOEShapeCircle _shape = new(15);

    public Freefire() : base(ActionID.MakeSpell(AID.Freefire)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(_shape, c.Position, new(), _resolve);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active && module.PrimaryActor.TargetID == actor.InstanceID && NumCasts > 0)
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
                hints.PlannedActions.Add((invuln, actor, (float)(_resolve - module.WorldState.CurrentTime).TotalSeconds, false));
                return;
            }
        }

        base.AddAIHints(module, slot, actor, assignment, hints);
    }

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Helper && id == 0x0449)
        {
            _casters.Add(actor);
            _resolve = module.WorldState.CurrentTime.AddSeconds(6);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}
