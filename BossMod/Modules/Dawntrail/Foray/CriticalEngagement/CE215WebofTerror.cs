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

sealed class Conformity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Conformity, new AOEShapeCone(50f, 22.5f.Degrees()));

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
                var offset = conformity.Position - Arena.Center;
                var direction = conformity.FinalRotation.ToDirection();
                var edgeDistance = Intersect.RayCircle(offset, direction, 25f);
                var edgePosition = conformity.Position + direction * edgeDistance;
                var coneDirection = Arena.Center - edgePosition;
                // going by finished rotation not exact; adds may swerver a bit towards the middle
                // adds not always cardinals; maybe card & intercards? try setting to closest 45deg spot
                var cardIntercard = Angle.AnglesCardinals.Concat(Angle.AnglesIntercardinals).ToArray();
                for (var j = 0; j < 8; j++)
                {
                    var angle = cardIntercard[j];
                    var rotation = coneDirection.ToAngle();
                    if (rotation.AlmostEqual(angle, 20f.Degrees().Rad))
                    {
                        var finalPos = Arena.Center + (angle + 180f.Degrees()).ToDirection() * 25f;
                        var finalDirection = Arena.Center - finalPos;
                        aoes.Add(new(_cone, finalPos, finalDirection.ToAngle(), _activation));
                        break;
                    }
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.QueensOrders)
        {
            _conformities.Add(new(actor, actor.Rotation));
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
                        conformity.Position = actor.Position;
                        conformity.FinalRotation = actor.Rotation;
                    }
                }
            }
        }
    }
    private class Conformity(Actor actor, Angle rotation, bool done = false, WPos position = default, Angle finalRotation = default)
    {
        public Actor Actor = actor;
        public Angle InitialRotation = rotation;
        public bool RotationDone = done;
        public WPos Position = position;
        public Angle FinalRotation = finalRotation;
    }
}

sealed class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    // donut too thicc if player standing between starting circles
    // stand on non-inside facing sides of one circle, or maybe increase AI forbidden
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sequences.Count != 0)
        {
            var seqs = CollectionsMarshal.AsSpan(Sequences);
            var count = seqs.Length;
            for (var i = 0; i < count; i++)
            {
                ref var seq = ref seqs[i];
                if (seq.NumCastsDone == 0)
                {
                    hints.AddForbiddenZone(new AOEShapeCircle(13f), seq.Origin, activation: seq.NextActivation);
                }
                else
                {
                    base.AddAIHints(slot, actor, assignment, hints);
                }
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class ArachneDaughter(BossModule module) : Components.Adds(module, (uint)OID.ArachneDaughter, 2);
sealed class VenomEruption(BossModule module) : Components.RaidwideCast(module, (uint)AID.VenomEruption, "Kill adds before they cast!");
sealed class Debug(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var adds = WorldState.Actors.Where(x => x.OID == (uint)OID.ArachneDaughter).ToList();
        for (var i = 0; i < adds.Count; i++)
        {
            Arena.ZoneCircle(adds[i].Position, 2f, Colors.SafeFromAOE);
        }

        var compass = Angle.AnglesCardinals.Concat(Angle.AnglesIntercardinals).ToArray();
        for (var i = 0; i < compass.Length; i++)
        {
            var edgeDistance = Arena.Bounds.IntersectRay(default, compass[i].ToDirection());
            var edgePosition = Arena.Center + compass[i].ToDirection() * edgeDistance;
            Arena.ZoneCircleOutline(edgePosition, 3f, default);
        }
    }
}

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
            //.ActivateOnEnter<Debug>();
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
public sealed class CE215WebofTerror(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -136f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
