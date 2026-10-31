namespace BossMod.Stormblood.Ultimate.UWU;

// satin plumes apply raidwide sleep after about 21 seconds
class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Plume, DateTime Spawn)> _plumes = [];

    public bool Active => _plumes.Any(p => p.Plume.IsTargetable || p.Plume.OID == (uint)OID.RazorPlume);

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
// TODO: rewrite to use status gain/lose event, we can occasionally cleanse too early here
class P1SpinyShield : BossComponent
{
    public BitMask Forbidden;

    int NumFrictions;
    int NumStorm1s;
    int NumStorm2s;
    WPos? _pending;
    Actor? _bubble;

    public DateTime Deadline = DateTime.MaxValue;

    private readonly int[] CleanseOrder = Utils.MakeArray(3, -1);

    private DateTime _nextCleanse;
    private int _nextCleanseSlot;

    private readonly UWUConfig _config = Service.Config.Get<UWUConfig>();

    public P1SpinyShield(BossModule module) : base(module)
    {
        foreach (var (slot, group) in _config.P1Friction.Resolve(Raid))
            if (group >= 0)
                CleanseOrder[group] = slot;
    }

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
            hints.AddForbiddenZone(ShapeDistance.Circle(e, 6));
        else
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(e, 6), Deadline);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // Gigastorm = spiny plume spawn, designated target (probably OT) should stay out
        if ((AID)spell.Action.ID == AID.Gigastorm)
        {
            _pending = caster.Position;
            Forbidden = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.ThermalLow, DateTime.MaxValue) != null).Mask();
        }

        if ((AID)spell.Action.ID == AID.SuperCyclone2)
        {
            NumStorm2s++;

            // first Cyclone2 cast: predetermined soakers should move back out of the bubble
            if (NumStorm2s == 1)
            {
                Forbidden |= BitMask.Build(CleanseOrder);
            }
            else if (NumStorm2s < 3 || _config.P1FastAwaken)
            {
                _nextCleanse = WorldState.FutureTime(2);
                _nextCleanseSlot = CleanseOrder.BoundSafeAt(NumStorm2s - 1, -1);
            }
        }

        if ((AID)spell.Action.ID == AID.SuperCyclone1)
        {
            NumStorm1s++;

            // immediate party cleanse is triggered after friction 1, at this point everyone should step out to keep their next stack
            if (NumStorm1s == 5)
                Forbidden = new(0xFF);
        }

        if ((AID)spell.Action.ID == AID.Friction)
            NumFrictions++;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        // don't do anything for StatusGain triggered on OT when component activates
        if (NumFrictions == 0)
            return;

        // friction 2: first cleanse pops as soon as they gain the status; the other two pop on a fixed delay
        if ((SID)status.ID == SID.ThermalLow && status.Extra == 2 && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot == CleanseOrder[0])
            Forbidden.Clear(slot);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SpinyShield)
        {
            _bubble = actor;
            _pending = null;
        }
    }

    public override void Update()
    {
        if (_bubble?.EventState == 7)
            _bubble = null;

        // first target cleanse as soon as whole party is safe
        if (Forbidden.NumSetBits() == 1 && NumFrictions == 0 && Raid.WithSlot().ExcludedFromMask(Forbidden).All(a => a.Item2.FindStatus(SID.ThermalHigh) != null))
            Forbidden.Reset();

        if (_nextCleanse != default && WorldState.CurrentTime >= _nextCleanse)
        {
            Forbidden.Clear(_nextCleanseSlot);
            _nextCleanse = default;
            _nextCleanseSlot = -1;
        }
    }
}
