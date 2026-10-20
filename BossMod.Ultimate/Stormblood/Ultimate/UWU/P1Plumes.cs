namespace BossMod.Stormblood.Ultimate.UWU;

// satin plumes apply raidwide sleep after about 21 seconds
class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Plume, DateTime Spawn)> _plumes = [];

    public bool Active => _plumes.Any(p => p.Plume.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_plumes.Select(p => p.Plume), ArenaColor.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
            return;

        var dest = Module.PrimaryActor.Position + new WDir(0, 3);

        foreach (var (plume, spawn) in _plumes)
        {
            if (hints.FindEnemy(plume) is not { } enemy)
                continue;

            enemy.AllowDOTs = false;
            enemy.DesiredPosition = dest;

            if (plume.OID == (uint)OID.SpinyPlume)
            {
                var stacks = WorldState.Actors.Find(plume.TargetID)?.FindStatus(SID.ThermalLow, DateTime.MaxValue)?.Extra ?? 0;

                if (plume.Tether.ID == 0)
                {
                    if (assignment != PartyRolesConfig.Assignment.OT)
                        enemy.Priority = AIHints.Enemy.PriorityForbidden;
                }
                else
                    enemy.Priority = 0;

                if (plume.PendingHPRatio < 0.15f)
                    enemy.Priority = stacks == 2 ? 1 : AIHints.Enemy.PriorityForbidden;
            }

            if (plume.OID == (uint)OID.SatinPlume)
            {
                enemy.Priority = 0;

                // roughly when the plume casts sleep
                var deadline = spawn.AddSeconds(20.7f);
                if (deadline < WorldState.FutureTime(5))
                    enemy.Priority = 1;
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SpinyPlume or OID.SatinPlume or OID.RazorPlume)
            _plumes.Add((actor, WorldState.CurrentTime));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _plumes.RemoveAll(p => p.Plume == actor);
    }
}

class P1Friction(BossModule module) : Components.StackWithCastTargets(module, AID.Friction, 5, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.SpinyShield).FirstOrDefault() is not { } bubble)
            return;

        var toCenter = (Arena.Center - bubble.Position).Normalized();
        var target = bubble.Position + toCenter * 6;

        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target, 5), DateTime.MaxValue);

        if (IsStackTarget(actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target, 2), Stacks[0].Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

// in at least 1 pull, bubble lasts 23 seconds 
class P1SpinyShield(BossModule module) : BossComponent(module)
{
    public BitMask Forbidden;

    int _numFriction;
    WPos? _pending;
    Actor? _bubble;

    public DateTime Deadline = DateTime.MaxValue;
    DateTime _bubbleExpire;

    WPos? Bubble => _bubble?.Position ?? _pending;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[pcSlot])
            return;

        if (Arena.Config.ShowOutlinesAndShadows)
            Arena.AddCircle(e, 6, 0xFF000000, 2);
        Arena.AddCircle(e, 6, ArenaColor.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[pcSlot])
            Arena.ZoneCircle(e, 6, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Bubble is not { } e)
            return;

        var safe = Forbidden[slot] != actor.Position.InCircle(e, 6);

        if (!safe)
            hints.Add(Forbidden[slot] ? "GTFO from bubble!" : "Go to bubble!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[slot])
            hints.AddForbiddenZone(ShapeDistance.Circle(e, 6), Deadline);
        else
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(e, 6), Deadline);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gigastorm)
        {
            _pending = caster.Position;
            Forbidden = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.ThermalLow, DateTime.MaxValue) != null).Mask();
        }

        if ((AID)spell.Action.ID == AID.MistralShriek)
            Forbidden.Reset();

        if ((AID)spell.Action.ID == AID.Friction)
        {
            _numFriction++;
            if (_numFriction == 2)
            {
                var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
                if (assignments.Length > 0)
                    Forbidden.Clear(assignments[(int)PartyRolesConfig.Assignment.M1]);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Friction)
        {
            Forbidden.Reset();
            Deadline = Module.CastFinishAt(spell);

            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
            if (assignments.Length == 0)
            {
                ReportError($"Don't know who should leave for friction, doing nothing");
                return;
            }

            if (_numFriction == 0)
            {
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.MT]);
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M1]);
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M2]);
            }

            if (_numFriction == 1)
                Forbidden = new(0xff);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SpinyShield)
        {
            _bubble = actor;
            _pending = null;
            _bubbleExpire = WorldState.FutureTime(22.9f);
        }
    }

    public override void Update()
    {
        if (_bubble?.EventState == 7)
            _bubble = null;

        if (_bubbleExpire > WorldState.CurrentTime)
        {
            var ass = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
            if (ass.Length == 0)
                return;

            if (_bubbleExpire < WorldState.FutureTime(3))
                Forbidden.Clear(ass[(int)PartyRolesConfig.Assignment.M2]);
        }
    }
}
