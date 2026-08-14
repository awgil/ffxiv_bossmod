namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE215WebofTerror;

public enum OID : uint
{
    HiddenTrap = 0x4D28, // R1.000, x16
    Helper = 0x233C, // R0.500, x19 (spawn during fight), Helper type
    CrescentArachneHelper = 0x4DFC, // R1.000, x1
    CrescentBombadeel = 0x4E42, // R2.850, x10
    CrescentHellhound = 0x4E30, // R4.500, x10
    CrescentBlackguard = 0x4E2F, // R2.500, x2 (spawn during fight)
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    CrescentOpken = 0x4E13, // R1.690, x1 (spawn during fight)
    CrescentJester = 0x4E3F, // R3.060, x6
    Actor1ec09d = 0x1EC09D, // R0.500, x1, EventObj type
    CrescentArachne = 0x4DFA, // R6.500, x1
    ArachneDaughter = 0x4DFB, // R2.400, x0 (spawn during fight)
    CrescentSoblyn = 0x4E1A, // R2.200, x0 (spawn during fight)
}
public enum AID : uint
{
    UnknownAbility = 50365, // CrescentArachneHelper->self, no cast, range ?-30 donut
    AutoAttack = 50853, // CrescentArachne->player, no cast, single-target
    Implosion = 50366, // CrescentArachne->self, 5.0s cast, single-target
    Implosion1 = 50367, // Helper->self, no cast, ???
    Summon = 50368, // CrescentArachne->self, 3.0s cast, single-target
    ArachnidWeb = 50369, // CrescentArachne->ArachneDaughter, 3.0s cast, single-target
    ArachnidWeb1 = 50370, // ArachneDaughter->ArachneDaughter, no cast, single-target
    ArachnidFunnel = 50371, // CrescentArachne->ArachneDaughter, 5.0s cast, width 20 rect charge
    ArachnidFunnel1 = 50372, // CrescentArachne->location, no cast, width 20 rect charge
    ArachnidFunnel2 = 50680, // Helper->location, no cast, width 20 rect charge
    AutoAttack1 = 50635, // ArachneDaughter->player, no cast, single-target
    Conformity = 50376, // CrescentArachne->self, 3.0s cast, range 50 45.000-degree cone
    QueensOrders = 50647, // CrescentArachne->self, 3.0s cast, single-target
    Conformity1 = 50377, // ArachneDaughter->self, 3.0s cast, range 50 45.000-degree cone
    BedrockUplift = 50378, // CrescentArachne->self, 4.7s cast, single-target
    BedrockUplift1 = 50379, // Helper->self, 5.0s cast, range 10 circle
    BedrockUplift2 = 50380, // Helper->self, 7.0s cast, range 10-20 donut
    BedrockUplift3 = 50381, // Helper->self, 9.0s cast, range 20-30 donut
    VenomEruption = 50375, // ArachneDaughter->self, 12.0s cast, single-target
}

public enum SID : uint
{
    QueensOrders = 2056, // none->ArachneDaughter, extra=0x291

}
public enum TetherID : uint
{
    ArachnidWebBoss = 420, // ArachneDaughter->CrescentArachne
    ArachnidWebAdd = 408, // ArachneDaughter->ArachneDaughter
}

sealed class Implosion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Implosion);
sealed class ArachnidFunnel(BossModule module) : Components.GenericAOEs(module)
{
    // boss tethers to next add after each jump
    // casts web to next add / aoe order
    // any pattern to positions? able to predict based on add rotation towards location?
    private readonly List<Actor> _actors = [];
    private DateTime _activation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_actors.Count == 0)
        {
            return [];
        }

        List<AOEInstance> aoes = [];
        var actors = CollectionsMarshal.AsSpan(_actors);
        var count = actors.Length;

        for (var i = 0; i < count - 1; i++)
        {
            ref var source = ref actors[i];
            ref var target = ref actors[i + 1];
            var dir = target.Position - source.Position;
            var shape = new AOEShapeRect(dir.Length(), 10f);
            var origin = source.Position.Quantized();
            var rotation = Angle.FromDirection(dir);
            aoes.Add(new(shape, origin, rotation, _activation.AddSeconds(i * 1.4d), i == 0 ? Colors.Danger : default, i <= 1, source.InstanceID, shape.Distance(origin, rotation)));
        }

        var aoespan = CollectionsMarshal.AsSpan(aoes);
        var aoecount = aoespan.Length;
        var max = aoecount > 2 ? 2 : aoecount;
        return aoespan[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ArachnidWeb)
        {
            var target = WorldState.Actors.Find(spell.TargetID);
            if (target != null)
            {
                _actors.Add(caster);
                _actors.Add(target);
            }
        }
        else if (spell.Action.ID == (uint)AID.ArachnidFunnel)
        {
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArachnidWeb1)
        {
            var target = WorldState.Actors.Find(spell.MainTargetID);
            if (target != null)
            {
                _actors.Add(target);
            }
        }
        else if (_actors.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.ArachnidFunnel:
                case (uint)AID.ArachnidFunnel1:
                    _actors.RemoveAt(0);
                    if (_actors.Count == 1)
                    {
                        _actors.Clear();
                    }
                    break;
            }
        }
    }
}

sealed class Conformity(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Conformity, (uint)AID.Conformity1], new AOEShapeCone(50f, 22.5f.Degrees()));

sealed class ConformityAdds(BossModule module) : Components.GenericAOEs(module)
{
    // any pattern to how adds move after being assigned status?
    // loses status little before reaching final spot
    // get angle on status gain, get final angle after rotation to predict final position?
    private readonly List<Conformity> _conformities = [];
    private readonly AOEShapeCone _cone = new(50f, 22.5f.Degrees());
    private DateTime _activation = default;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_conformities.Count == 0)
        {
            return [];
        }

        List<AOEInstance> aoes = [];
        var conformities = CollectionsMarshal.AsSpan(_conformities);
        var count = conformities.Length;
        for (var i = 0; i < count; i++)
        {
            ref var conformity = ref conformities[i];

            if (conformity.RotationDone)
            {
                var offset = conformity.InitialPosition - Arena.Center;
                var direction = conformity.FinalRotation.ToDirection();
                var edgeDistance = Arena.Bounds.IntersectRay(offset, direction);
                var edgePosition = conformity.InitialPosition + direction * edgeDistance;
                /*
                var coneDirection = Arena.Center - edgePosition;
                aoes.Add(new(_cone, edgePosition, coneDirection.ToAngle(), _activation));
                */
                // going by finished rotation not exact; 1st matches, 2nd off by a several degrees
                // adds always go to cardinal? clamp to closest cardinal position?
                var edgeOffset = edgePosition - Arena.Center;
                var cardinal = Math.Abs(edgeOffset.X) < Math.Abs(edgeOffset.Z) ? edgeOffset.Z < 0 ? 180f.Degrees() : 0f.Degrees() : edgeOffset.X < 0 ? -90f.Degrees() : 90f.Degrees();
                var finalfinal = Arena.Center + cardinal.ToDirection() * 25f;
                var coneDirection = Arena.Center - finalfinal;
                aoes.Add(new(_cone, finalfinal, coneDirection.ToAngle(), _activation));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.QueensOrders)
        {
            _conformities.Add(new(actor, actor.Position, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_conformities.Count != 0 && spell.Action.ID == (uint)AID.Conformity1)
        {
            _activation = default;
            _conformities.RemoveAt(0);
        }
    }

    public override void Update()
    {
        if (_conformities.Count != 0)
        {
            var conformities = CollectionsMarshal.AsSpan(_conformities);
            var count = conformities.Length;
            for (var i = 0; i < count; i++)
            {
                ref var conformity = ref conformities[i];

                if (!conformity.RotationDone)
                {
                    if (_activation == default)
                    {
                        _activation = WorldState.FutureTime(5.5d);
                    }

                    var actor = conformity.Actor;
                    if (!actor.Rotation.AlmostEqual(conformity.InitialRotation, 0.1f) && actor.PosRot.W - actor.PrevPosRot.W == default)
                    {
                        conformity.RotationDone = true;
                        conformity.FinalRotation = actor.Rotation;
                    }
                }
            }
        }
    }

    private class Conformity(Actor actor, WPos position, Angle rotation, bool done = false, Angle finalRotation = default)
    {
        public Actor Actor = actor;
        public WPos InitialPosition = position;
        public Angle InitialRotation = rotation;
        public bool RotationDone = done;
        public Angle FinalRotation = finalRotation;
    }
}

sealed class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BedrockUplift1)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (uint)(AID)spell.Action.ID switch
        {
            (uint)AID.BedrockUplift1 => 0,
            (uint)AID.BedrockUplift2 => 1,
            (uint)AID.BedrockUplift3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2d));
    }
}

sealed class ArachneDaughter(BossModule module) : Components.Adds(module, (uint)OID.ArachneDaughter, 2);
sealed class VenomEruption(BossModule module) : Components.RaidwideCast(module, (uint)AID.VenomEruption, "Kill adds before they cast!");

[SkipLocalsInit]
sealed class CE215WebofTerrorStates : StateMachineBuilder
{
    public CE215WebofTerrorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<ArachnidFunnel>()
            .ActivateOnEnter<ArachneDaughter>()
            .ActivateOnEnter<VenomEruption>()
            .ActivateOnEnter<Conformity>()
            .ActivateOnEnter<ConformityAdds>()
            .ActivateOnEnter<BedrockUplift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE215WebofTerrorStates),
    ConfigType = null, // replace null with typeof(WebofTerrorConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.CrescentArachne,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 55u,
    SortOrder = 7,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE215WebofTerror(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -136f), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
