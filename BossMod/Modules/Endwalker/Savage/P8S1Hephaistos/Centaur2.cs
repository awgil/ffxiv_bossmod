namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class QuadrupedalImpact(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.QuadrupedalImpactAOE), true)
{
    private WPos? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Value, 30); // TODO: activation
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QuadrupedalImpact)
            _source = spell.LocXZ;
    }
}

class QuadrupedalCrush(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.QuadrupedalCrushAOE))
{
    private WPos? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Value, new(), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QuadrupedalCrush)
        {
            _source = spell.LocXZ;
            _activation = spell.NPCFinishAt.AddSeconds(0.9f);
        }
    }
}

class CentaurTetraflare(BossModule module) : TetraOctaFlareCommon(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ConceptualTetraflareCentaur)
            SetupMasks(Concept.Tetra);
    }
}

class CentaurDiflare(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ConceptualDiflare)
            AddStacks(Raid.WithoutSlot().Where(a => a.Role == Role.Healer));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EmergentDiflare)
            Stacks.Clear();
    }
}

// TODO: hints
class BlazingFootfalls(BossModule module) : BossComponent(module)
{
    public int NumMechanicsDone { get; private set; }
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
            Arena.ZoneRect(Module.Center, new WDir(0, 1), Module.Bounds.Radius, Module.Bounds.Radius, _trailblazeHalfWidth, ArenaColor.AOE);
        }
        if (NumMechanicsDone == 2)
        {
            // draw second trailblaze
            Arena.ZoneRect(Module.Center, new WDir(1, 0), Module.Bounds.Radius, Module.Bounds.Radius, _trailblazeHalfWidth, ArenaColor.AOE);
        }

        if (_firstCrush && NumMechanicsDone < 2)
        {
            // draw first crush
            Arena.ZoneCircle(Module.Center + Module.Bounds.Radius * new WDir(_firstSafeLeft ? 1 : -1, 0), _crushRadius, ArenaColor.AOE);
        }
        if (!_firstCrush && NumMechanicsDone is >= 2 and < 4)
        {
            // draw second crush
            Arena.ZoneCircle(Module.Center + Module.Bounds.Radius * new WDir(0, _secondSafeTop ? 1 : -1), _crushRadius, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumMechanicsDone < 2 && _seenVisuals > 0)
        {
            // draw first safespot
            Arena.AddCircle(Module.Center + _safespotOffset * new WDir(_firstSafeLeft ? -1 : 1, 0), _safespotRadius, ArenaColor.Safe, 2);
        }
        if (NumMechanicsDone < 4 && _seenVisuals > 1)
        {
            // draw second safespot
            Arena.AddCircle(Module.Center + _safespotOffset * new WDir(0, _secondSafeTop ? -1 : 1), _safespotRadius, ArenaColor.Safe, 2);
        }

        if (NumMechanicsDone == 0)
        {
            // draw knockback from first trailblaze
            var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(pc.Position.X < Module.Center.X ? -1 : 1, 0);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }
        if (NumMechanicsDone == 2)
        {
            // draw knockback from second trailblaze
            var adjPos = pc.Position + _trailblazeKnockbackDistance * new WDir(0, pc.Position.Z < Module.Center.Z ? -1 : 1);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }

        if (!_firstCrush && NumMechanicsDone == 1)
        {
            // draw knockback from first impact
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, Module.Center + Module.Bounds.Radius * new WDir(_firstSafeLeft ? -1 : 1, 0), _impactKnockbackRadius);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }
        if (_firstCrush && NumMechanicsDone == 3)
        {
            // draw knockback from second impact
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, Module.Center + Module.Bounds.Radius * new WDir(0, _secondSafeTop ? -1 : 1), _impactKnockbackRadius);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BlazingFootfallsImpactVisual:
                if (_seenVisuals > 0)
                {
                    _secondSafeTop = spell.LocXZ.Z < Module.Center.Z;
                }
                else
                {
                    _firstSafeLeft = spell.LocXZ.X < Module.Center.X;
                }
                ++_seenVisuals;
                break;
            case AID.BlazingFootfallsCrushVisual:
                if (_seenVisuals > 0)
                {
                    _secondSafeTop = spell.LocXZ.Z > Module.Center.Z;
                }
                else
                {
                    _firstCrush = true;
                    _firstSafeLeft = spell.LocXZ.X > Module.Center.X;
                }
                ++_seenVisuals;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlazingFootfallsTrailblaze or AID.BlazingFootfallsImpact or AID.BlazingFootfallsCrush)
            ++NumMechanicsDone;
    }
}
