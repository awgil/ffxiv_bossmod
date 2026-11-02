namespace BossMod.Stormblood.Ultimate.UWU;

// TODO :implement hints...
class Mesohigh(BossModule module) : Components.CastCounter(module, AID.Mesohigh)
{
    protected readonly List<Actor> _sisters = [.. module.Enemies(OID.GarudaSister)];
    private const float _radius = 3;
    protected DateTime Activation;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Mesohigh && Activation == default)
            Activation = WorldState.FutureTime(5.1f);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in EnumerateTetherSources())
        {
            var tetherTarget = WorldState.Actors.Find(s.Tether.Target);
            if (tetherTarget != null)
            {
                Arena.AddLine(s.Position, tetherTarget.Position, ArenaColor.Danger);
                Arena.AddCircle(tetherTarget.Position, _radius, ArenaColor.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _sisters.Any(s => s.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    protected IEnumerable<Actor> EnumerateTetherSources()
    {
        foreach (var s in _sisters.Tethered(TetherID.Mesohigh))
            yield return s;
        if (Module.PrimaryActor.Tether.ID == (uint)TetherID.Mesohigh)
            yield return Module.PrimaryActor;
    }
}

class P1Mesohigh(BossModule module) : Mesohigh(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sisters.Count != 2)
            return;

        var tankTarget = Raid.WithoutSlot().FirstOrDefault(a => a.Role == Role.Tank && a.FindStatus(SID.ThermalLow)?.Extra == 2);
        var dpsTarget = Raid.WithoutSlot().OrderBy(a => a.InstanceID).FirstOrDefault(a => a.Class.IsDD() && a.FindStatus(SID.ThermalLow)?.Extra == 1);

        Actor? desiredSource = null;

        if (actor.Role == Role.Tank && actor.FindStatus(SID.ThermalLow)?.Extra == 2)
        {
            desiredSource = _sisters.MinBy(s => s.Position.X);
        }
        else if (actor == Raid.WithoutSlot().Where(a => a.Class.IsDD() && a.FindStatus(SID.ThermalLow)?.Extra == 1).MinBy(a => a.InstanceID))
        {
            desiredSource = _sisters.MaxBy(s => s.Position.X);
        }

        if (desiredSource != null)
        {
            var curTarget = WorldState.Actors.Find(desiredSource.Tether.Target);
            if (curTarget == actor)
            {
                var dest = new WPos(desiredSource.Position.X > 100 ? 106 : 94, 100);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 1), Activation);
            }
            else if (curTarget != null)
            {
                var intercept = WPos.Lerp(desiredSource.Position, curTarget.Position, 0.5f);
                hints.AddForbiddenZone(ShapeDistance.PrecisePosition(intercept, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }
        }
        else if (Module.PrimaryActor.TargetID != actor.InstanceID)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(100, 103), 1), Activation);
        }
    }
}

class P4Mesohigh(BossModule module) : Mesohigh(module)
{
    WPos partyDodgeBasis;
    bool stackDone;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.WeightOfTheLandAOE && stackDone)
            partyDodgeBasis = spell.LocXZ;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.FlamingCrush)
            stackDone = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (assignment)
        {
            // R1 can move early since they won't get it anyone's way
            case PartyRolesConfig.Assignment.R1:
                if (stackDone)
                {
                    if (WorldState.Actors.Find(Module.PrimaryActor.Tether.Target) is { } tetherTarget && tetherTarget != actor)
                    {
                        var toGaruda = tetherTarget.DirectionTo(Module.PrimaryActor);
                        hints.AddForbiddenZone(ShapeDistance.PrecisePosition(tetherTarget.Position + toGaruda * 3, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
                    }
                    else
                        // take tether around C (rect so we can dodge feather rain)
                        hints.AddForbiddenZone(ShapeDistance.InvertedRect(new WPos(100, 105), new WPos(100, 110), 1), DateTime.MaxValue);
                }
                break;

            // dps have no particular job, dodge north
            case PartyRolesConfig.Assignment.M1:
            case PartyRolesConfig.Assignment.M2:
            case PartyRolesConfig.Assignment.R2:
                if (partyDodgeBasis != default)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(96, 84), 1), DateTime.MaxValue);
                break;

            // tanks walk into orb
            case PartyRolesConfig.Assignment.MT:
            case PartyRolesConfig.Assignment.OT:
                if (partyDodgeBasis != default)
                    hints.AddForbiddenZone(ShapeDistance.InvertedRect(partyDodgeBasis, new WDir(1, 0), 100, 0, 1));
                break;

            // healers spread W
            case PartyRolesConfig.Assignment.H1:
            case PartyRolesConfig.Assignment.H2:
                if (partyDodgeBasis != default)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(84, 100), 1), DateTime.MaxValue);
                break;
        }
    }
}
