namespace BossMod.Stormblood.Alliance.A32Agrias;

class HeavenlyShield(BossModule module) : BossComponent(module)
{
    readonly List<(Actor Actor, int Kind)> BuffPuddles = [];
    readonly List<(Actor Source, Actor? Target, float Radius, DateTime Deadline)> Danger = [];
    readonly List<Actor> SwordTargets = [];

    const float PuddleRadius = 4.8f;
    const float PauseDuration = 0.5f;
    const float HintAhead = 2f;

    BitMask ShieldBearer;
    BitMask SwordBearer;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.JudgmentBlade:
                Danger.Add((caster, null, 100, Module.CastFinishAt(spell)));
                break;
            case AID.MortalBlow:
                Danger.Add((caster, null, 30, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Halidom or OID.EmblazonedShield or OID.TaintlessSoul)
            SwordTargets.Add(actor);

        if (actor.OID == 0x1EA1A1 && actor.Position.AlmostEqual(new(300, -65), 1))
            BuffPuddles.Add((actor, 1));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        BuffPuddles.RemoveAll(p => p.Actor == actor);
        SwordTargets.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.JudgmentBlade or AID.MortalBlow)
            Danger.RemoveAll(p => p.Source == caster);

        if ((AID)spell.Action.ID == AID.DivineRuination)
            Danger.RemoveAll(d => d.Source == Module.PrimaryActor && d.Target != null);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.DivineRuination)
            Danger.Add((Module.PrimaryActor, actor, 100, WorldState.FutureTime(8.2f)));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Shieldbearer && Raid.TryFindSlot(actor, out var slot))
            ShieldBearer.Set(slot);
        if ((SID)status.ID == SID.Swordbearer && Raid.TryFindSlot(actor, out var slot2))
            SwordBearer.Set(slot2);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Shieldbearer && Raid.TryFindSlot(actor, out var slot))
            ShieldBearer.Clear(slot);
        if ((SID)status.ID == SID.Swordbearer && Raid.TryFindSlot(actor, out var slot2))
            SwordBearer.Clear(slot2);
    }

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x00010002)
        {
            var kind = actor.LayoutID switch
            {
                0x76355E or 0x76355B => 1, // sword
                0x763563 or 0x763561 => 0, // shield
                _ => -1
            };
            if (kind >= 0)
                BuffPuddles.Add((actor, kind));
        }

        if (actor.OID == 0x1EA1A1 && state == 0x00040008)
            BuffPuddles.RemoveAll(p => p.Actor == actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (source, radius, _) in DangerFor(pc))
            if (pc.Position.InCircle(source.Position, radius))
            {
                var danger = pc.FindStatus(SID.HeavenlyShield) == null;

                Components.GenericGaze.DrawEye(Arena.WorldPositionToScreenPosition(source.Position), danger);

                Arena.PathArcTo(pc.Position, 1, (pc.Rotation + 45.Degrees()).Rad, (pc.Rotation + 315.Degrees()).Rad);
                Arena.PathStroke(false, ArenaColor.Enemy);
            }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (source, radius, _) in DangerFor(pc))
            if (!pc.Position.InCircle(source.Position, radius) || !ShieldBearer[pcSlot] && radius < 40)
                Arena.ZoneCircle(source.Position, radius, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (source, radius, deadline) in DangerFor(actor))
        {
            if (actor.Position.InCircle(source.Position, radius))
            {
                if (ShieldBearer[slot])
                {
                    hints.Add("Use shield!", actor.FindStatus(SID.HeavenlyShield) == null && deadline < WorldState.FutureTime(HintAhead));

                    var toSource = actor.AngleTo(source);
                    if (!toSource.AlmostEqual(actor.Rotation, 45.Degrees().Rad))
                        hints.Add("Face boss!");
                }
                else if (BuffPuddles.Any(p => p.Kind == 0))
                    hints.Add("Grab shield!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shieldMode = false;

        foreach (var (source, radius, deadline) in DangerFor(actor))
        {
            if (!actor.Position.InCircle(source.Position, radius))
            {
                if (!ShieldBearer[slot])
                    hints.AddForbiddenZone(ShapeDistance.Circle(source.Position, radius), deadline);

                continue;
            }

            shieldMode = true;

            if (actor.FindStatus(SID.HeavenlyShield, DateTime.MaxValue) != null)
            {
                if (deadline < WorldState.FutureTime(PauseDuration))
                {
                    hints.AddSpecialMode(AIHints.SpecialMode.Pyretic, deadline);
                    var toBoss = actor.AngleTo(Module.PrimaryActor);
                    hints.ForbiddenDirections.Add((toBoss + 180.Degrees(), 135.Degrees(), deadline));
                }
            }
            else if (ShieldBearer[slot])
            {
                var shieldDelay = PauseDuration + ApplicationDelay.Get(ClassShared.AID.HeavenlyShield);
                var leeway = Math.Max(0, (float)(deadline - WorldState.CurrentTime).TotalSeconds - shieldDelay);

                hints.MaxCastTime = leeway;

                if (leeway == 0)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeavenlyShield), actor, ActionQueue.Priority.VeryHigh);
            }
            else
            {
                var puddles = ShapeDistance.Union([.. BuffPuddles.Where(p => p.Kind == 0).Select(p => ShapeDistance.Circle(p.Actor.Position, PuddleRadius))]).Inverted();
                hints.AddForbiddenZone(puddles, deadline.AddSeconds(-1));
            }
        }

        if (SwordTargets.Count > 0)
        {
            if (SwordBearer[slot])
            {
                foreach (var h in SwordTargets)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(h.Position, h.HitboxRadius + 6, 3));

                    if (actor.DistanceToPoint(h.Position) <= h.HitboxRadius + 6)
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeavenlySword), actor, ActionQueue.Priority.VeryHigh, facingAngle: actor.AngleTo(h));
                }
            }
            else if (actor.Role is Role.Healer or Role.Ranged && !shieldMode || SwordTargets.Any(t => t.OID == (uint)OID.TaintlessSoul))
            {
                var swords = ShapeDistance.Union([.. BuffPuddles.Where(p => p.Kind == 1).Select(p => ShapeDistance.Circle(p.Actor.Position, PuddleRadius))]).Inverted();
                hints.AddForbiddenZone(swords);
            }
        }
    }

    private IEnumerable<(Actor Source, float Radius, DateTime Deadline)> DangerFor(Actor target) => Danger.Where(t => t.Target == null || t.Target == target).Select(d => (d.Source, d.Radius, d.Deadline));

    public override void Update()
    {
        SwordTargets.RemoveAll(t => t.IsDeadOrDestroyed || t.PendingDead);
    }
}
