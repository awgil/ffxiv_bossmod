namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class QuadrupedalImpact(BossModule module) : Components.GenericKnockback(module, (uint)AID.QuadrupedalImpactAOE)
{
    private WPos? _source;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_source != null)
            return new Knockback[1] { new(_source.Value, 30f, ignoreImmunes: true) }; // TODO: activation
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.QuadrupedalImpact)
            _source = spell.LocXZ;
    }
}

class QuadrupedalCrush(BossModule module) : Components.GenericAOEs(module, (uint)AID.QuadrupedalCrushAOE)
{
    private WPos? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            return new AOEInstance[1] { new(_shape, _source.Value, default, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.QuadrupedalCrush)
        {
            _source = spell.LocXZ;
            _activation = Module.CastFinishAt(spell, 0.9f);
        }
    }
}

class CentaurTetraflare(BossModule module) : TetraOctaFlareCommon(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ConceptualTetraflareCentaur)
            SetupMasks(Concept.Tetra);
    }
}

class CentaurDiflare(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ConceptualDiflare)
            AddStacks(Raid.WithoutSlot(false, true, true).Where(a => a.Role == Role.Healer));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EmergentDiflare)
            Stacks.Clear();
    }
}

// TODO: hints
class BlazingFootfalls(BossModule module) : BossComponent(module)
{
    public int NumMechanicsDone;
    private int _seenVisuals;
    private bool _firstCrush;
    private bool _firstSafeLeft;
    private bool _secondSafeTop;

    private const float _trailblazeHalfWidth = 7;
    private const float _trailblazeKnockbackDistance = 10;
    private const float _crushRadius = 30;
    private const float _impactKnockbackRadius = 30;
    private const float _safespotOffset = 15;
    private const float _safespotRadius = 3;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumMechanicsDone == 0)
        {
            // draw first trailblaze
            Arena.ZoneRect(Arena.Center, new WDir(0, 1), Arena.Bounds.Radius, Arena.Bounds.Radius, _trailblazeHalfWidth, Colors.AOE);
        }
        if (NumMechanicsDone == 2)
        {
            // draw second trailblaze
            Arena.ZoneRect(Arena.Center, new WDir(1, 0), Arena.Bounds.Radius, Arena.Bounds.Radius, _trailblazeHalfWidth, Colors.AOE);
        }

        if (_firstCrush && NumMechanicsDone < 2)
        {
            // draw first crush
            Arena.ZoneCircle(Arena.Center + Arena.Bounds.Radius * new WDir(_firstSafeLeft ? 1 : -1, 0), _crushRadius, Colors.AOE);
        }
        if (!_firstCrush && NumMechanicsDone is >= 2 and < 4)
        {
            // draw second crush
            Arena.ZoneCircle(Arena.Center + Arena.Bounds.Radius * new WDir(0, _secondSafeTop ? 1 : -1), _crushRadius, Colors.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumMechanicsDone < 2 && _seenVisuals > 0)
        {
            // draw first safespot
            Arena.ZoneCircleOutline(Arena.Center + _safespotOffset * new WDir(_firstSafeLeft ? -1 : 1, 0), _safespotRadius, Colors.Safe, 2);
        }
        if (NumMechanicsDone < 4 && _seenVisuals > 1)
        {
            // draw second safespot
            Arena.ZoneCircleOutline(Arena.Center + _safespotOffset * new WDir(0, _secondSafeTop ? -1 : 1), _safespotRadius, Colors.Safe, 2);
        }

        if (NumMechanicsDone == 0)
        {
            // draw knockback from first trailblaze
            var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(pc.PosRot.X < Arena.Center.X ? -1 : 1, 0);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }
        if (NumMechanicsDone == 2)
        {
            // draw knockback from second trailblaze
            var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(0, pc.PosRot.Z < Arena.Center.Z ? -1 : 1);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }

        if (!_firstCrush && NumMechanicsDone == 1)
        {
            // draw knockback from first impact
            var adjPos = Components.GenericKnockback.AwayFromSource(pc.Position, Arena.Center + Arena.Bounds.Radius * new WDir(_firstSafeLeft ? -1 : 1, 0), _impactKnockbackRadius);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }
        if (_firstCrush && NumMechanicsDone == 3)
        {
            // draw knockback from second impact
            var adjPos = Components.GenericKnockback.AwayFromSource(pc.Position, Arena.Center + Arena.Bounds.Radius * new WDir(0, _secondSafeTop ? -1 : 1), _impactKnockbackRadius);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BlazingFootfallsImpactVisual:
                if (_seenVisuals > 0)
                {
                    _secondSafeTop = spell.LocXZ.Z < Arena.Center.Z;
                }
                else
                {
                    _firstSafeLeft = spell.LocXZ.X < Arena.Center.X;
                }
                ++_seenVisuals;
                break;
            case (uint)AID.BlazingFootfallsCrushVisual:
                if (_seenVisuals > 0)
                {
                    _secondSafeTop = spell.LocXZ.Z > Arena.Center.Z;
                }
                else
                {
                    _firstCrush = true;
                    _firstSafeLeft = spell.LocXZ.X > Arena.Center.X;
                }
                ++_seenVisuals;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BlazingFootfallsTrailblaze or (uint)AID.BlazingFootfallsImpact or (uint)AID.BlazingFootfallsCrush)
            ++NumMechanicsDone;
    }
}
