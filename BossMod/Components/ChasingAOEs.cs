namespace BossMod.Components;

// generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts
public class GenericChasingAOEs : GenericAOEs
{
    public class Chaser
    {
        public AOEShape Shape;
        public Actor Target;
        public WPos PrevPos;
        public float MoveDist;
        public int NumRemaining;
        public DateTime NextActivation;
        public float SecondsBetweenActivations;

        public Chaser(AOEShape shape, Actor target, WPos prevPos, float moveDist, int numRemaining, DateTime nextActivation, float secondsBetweenActivations)
        {
            Shape = shape;
            Target = target;
            PrevPos = prevPos;
            MoveDist = moveDist;
            NumRemaining = numRemaining;
            NextActivation = nextActivation;
            SecondsBetweenActivations = secondsBetweenActivations;
        }

        public WPos PredictedPosition()
        {
            var offset = Target.Position - PrevPos;
            var distance = offset.Length();
            return distance > MoveDist ? PrevPos + MoveDist * offset / distance : Target.Position;
        }
    }

    public List<Chaser> Chasers = new();

    public GenericChasingAOEs(ActionID aid = default, string warningText = "GTFO from chasing aoe!") : base(aid, warningText) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var c in Chasers)
        {
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            yield return new(c.Shape, pos, off.LengthSq() > 0 ? Angle.FromDirection(off) : default, c.NextActivation);
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Chasers.Any(c => c.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    // return false if chaser was not found
    public bool Advance(WPos pos, float moveDistance, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;
        var c = Chasers.MinBy(c => (c.PredictedPosition() - pos).LengthSq());
        if (c == null)
            return false;

        if (--c.NumRemaining <= 0 && removeWhenFinished)
        {
            Chasers.Remove(c);
        }
        else
        {
            c.PrevPos = pos;
            c.MoveDist = moveDistance;
            c.NextActivation = currentTime.AddSeconds(c.SecondsBetweenActivations);
        }
        return true;
    }
}

// standard chasing aoe; first cast is long - assume it is baited on the nearest allowed target; successive casts are instant
public class StandardChasingAOEs : GenericChasingAOEs
{
    public AOEShape Shape;
    public ActionID ActionFirst;
    public ActionID ActionRest;
    public float MoveDistance;
    public float SecondsBetweenActivations;
    public int MaxCasts;
    public BitMask ExcludedTargets; // any targets in this mask aren't considered to be possible targets

    public StandardChasingAOEs(AOEShape shape, ActionID actionFirst, ActionID actionRest, float moveDistance, float secondsBetweenActivations, int maxCasts)
    {
        Shape = shape;
        ActionFirst = actionFirst;
        ActionRest = actionRest;
        MoveDistance = moveDistance;
        SecondsBetweenActivations = secondsBetweenActivations;
        MaxCasts = maxCasts;
    }

    public override void Update(BossModule module)
    {
        Chasers.RemoveAll(c => (c.Target.IsDestroyed || c.Target.IsDead) && c.NumRemaining < MaxCasts);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var c in Chasers)
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddLine(c.PrevPos, c.Target.Position, 0xFF000000, 2);
            arena.AddLine(c.PrevPos, c.Target.Position, ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst)
        {
            var pos = spell.TargetID == caster.InstanceID ? caster.Position : module.WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
            var (slot, target) = module.Raid.WithSlot().ExcludedFromMask(ExcludedTargets).MinBy(ip => (ip.Item2.Position - pos).LengthSq());
            if (target != null)
            {
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, spell.NPCFinishAt, SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets.Set(slot);
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionFirst || spell.Action == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : module.WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, module.WorldState.CurrentTime);
        }
    }
}
