namespace BossMod.RealmReborn.Raid.T05Twintania;

// P3 mechanics
// TODO: preposition for divebombs? it seems that boss spawns in one of the fixed spots that is closest to target...
class P3Divebomb(BossModule module) : Components.GenericAOEs(module)
{
    public WPos? Target;
    public DateTime HitAt;

    private static readonly AOEShapeRect _shape = new(35f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Target != null)
        {
            if (Module.PrimaryActor.CastInfo == null)
                return new AOEInstance[1] { new(_shape, Module.PrimaryActor.Position, Angle.FromDirection(Target.Value - Module.PrimaryActor.Position), HitAt) };
            else
                return new AOEInstance[1] { new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo)) };
        }
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.DivebombMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID)?.Position;
                HitAt = WorldState.FutureTime(1.7d);
                break;
            case (uint)AID.DivebombAOE:
                Target = null;
                break;
        }
    }
}

class P3Adds(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _hygieia = module.Enemies((uint)OID.Hygieia);
    public readonly List<Actor> Asclepius = module.Enemies((uint)OID.Asclepius);
    public IEnumerable<Actor> ActiveHygieia => _hygieia.Where(a => !a.IsDead);

    private const float _explosionRadius = 8;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var nextHygieia = ActiveHygieia.MinBy(a => a.InstanceID); // select next add to kill by lowest hp
        var asclepiusVuln = Asclepius.FirstOrDefault()?.FindStatus((uint)SID.Disseminate);
        var killHygieia = asclepiusVuln == null || (asclepiusVuln.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10;
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            switch (e.Actor.OID)
            {
                case (uint)OID.Hygieia:
                    var predictedHP = e.Actor.PendingHPRaw;
                    e.Priority = e.Actor.HPMP.CurHP == 1 ? 0
                        : killHygieia && e.Actor == nextHygieia ? 2
                        : predictedHP < 0.3f * e.Actor.HPMP.MaxHP ? -1
                        : 1;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    var gtfo = predictedHP <= (e.ShouldBeTanked ? 1 : 0.1f * e.Actor.HPMP.MaxHP);
                    if (gtfo)
                        hints.AddForbiddenZone(new SDCircle(e.Actor.Position, 9f));
                    break;
                case (uint)OID.Asclepius:
                    e.Priority = 1;
                    e.AttackStrength = 0.15f;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.MT;
                    break;
            }
        }

        if (!Module.PrimaryActor.IsTargetable && !ActiveHygieia.Any() && !Asclepius.Any(a => !a.IsDead))
        {
            // once all adds are dead, gather where boss will return
            hints.AddForbiddenZone(new SDInvertedCircle(new(-6.67f, 5f), 5f), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in ActiveHygieia)
        {
            Arena.Actor(a);
            Arena.ZoneCircleOutline(a.Position, _explosionRadius);
        }
        Arena.Actors(Asclepius);
    }
}

class P3AethericProfusion(BossModule module) : Components.CastCounter(module, (uint)AID.AethericProfusion)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(6.7d);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // select neurolinks to stand at; let everyone except MT stay in one closer to boss
        var neurolinks = Module.Enemies((uint)OID.Neurolink);
        var closerNeurolink = neurolinks.Closest(Module.PrimaryActor.Position);
        foreach (var neurolink in neurolinks)
        {
            var isClosest = neurolink == closerNeurolink;
            var stayAtClosest = assignment != PartyRolesConfig.Assignment.MT;
            if (isClosest == stayAtClosest)
                hints.AddForbiddenZone(new SDInvertedCircle(neurolink.Position, T05Twintania.NeurolinkRadius), _activation);
        }

        // let MT taunt boss if needed
        hints.FindEnemy(Module.PrimaryActor)?.PreferProvoking = true;

        // mitigate heavy raidwide
        hints.AddPredictedDamage(Raid.WithSlot(false, true, true).Mask(), _activation);
        if (actor.Role == Role.Ranged)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Addle), Module.PrimaryActor, ActionQueue.Priority.High, (float)(_activation - WorldState.CurrentTime).TotalSeconds);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var neurolink in Module.Enemies((uint)OID.Neurolink))
            Arena.ZoneCircleOutline(neurolink.Position, T05Twintania.NeurolinkRadius, Colors.Safe);
    }
}
