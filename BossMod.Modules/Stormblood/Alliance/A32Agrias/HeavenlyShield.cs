namespace BossMod.Stormblood.Alliance.A32Agrias;

class HeavenlyShield(BossModule module) : BossComponent(module)
{
    readonly List<(Actor Actor, int Kind)> BuffPuddles = [];
    readonly List<(Actor Source, Actor? Target, float Radius, DateTime Deadline)> Danger = [];
    readonly List<Actor> SwordTargets = [];

    const float PuddleSize_TODO = 5;
    const float PauseDuration = 0.5f;
    const float HintAhead = 2f;

    BitMask ShieldBearer;
    BitMask SwordBearer;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_JudgmentBlade:
                Danger.Add((caster, null, 100, Module.CastFinishAt(spell)));
                break;
            case AID._Weaponskill_MortalBlow:
                Danger.Add((caster, null, 30, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID._Gen_Halidom or OID._Gen_EmblazonedShield)
            SwordTargets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_JudgmentBlade or AID._Weaponskill_MortalBlow)
            Danger.RemoveAll(p => p.Source == caster);

        if ((AID)spell.Action.ID == AID._Spell_DivineRuination)
            Danger.RemoveAll(d => d.Source == Module.PrimaryActor && d.Target != null);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_m0196trg_a0h)
            Danger.Add((Module.PrimaryActor, actor, 100, WorldState.FutureTime(8.2f)));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Shieldbearer && Raid.TryFindSlot(actor, out var slot))
            ShieldBearer.Set(slot);
        if ((SID)status.ID == SID._Gen_Swordbearer && Raid.TryFindSlot(actor, out var slot2))
            SwordBearer.Set(slot2);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Shieldbearer && Raid.TryFindSlot(actor, out var slot))
            ShieldBearer.Clear(slot);
        if ((SID)status.ID == SID._Gen_Swordbearer && Raid.TryFindSlot(actor, out var slot2))
            SwordBearer.Clear(slot2);
    }

    // sword layout IDs 76355E, 76355B
    // shield layout IDs 763563, 763561
    public override void OnActorEAnim(Actor actor, uint state)
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

            if (actor.FindStatus(SID.HeavenlyShield) != null)
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
                if (deadline < WorldState.FutureTime(PauseDuration + ApplicationDelay.Get(ClassShared.AID.HeavenlyShield)))
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeavenlyShield), actor, ActionQueue.Priority.VeryHigh);
            }
            else
            {
                var puddles = ShapeDistance.Union([.. BuffPuddles.Where(p => p.Kind == 0).Select(p => ShapeDistance.Circle(p.Actor.Position, PuddleSize_TODO))]).Inverted();
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
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeavenlySword), actor, ActionQueue.Priority.High, facingAngle: actor.AngleTo(h));
                }
            }
            else if (actor.Role is Role.Healer or Role.Ranged && !shieldMode)
            {
                var swords = ShapeDistance.Union([.. BuffPuddles.Where(p => p.Kind == 1).Select(p => ShapeDistance.Circle(p.Actor.Position, PuddleSize_TODO))]).Inverted();
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

