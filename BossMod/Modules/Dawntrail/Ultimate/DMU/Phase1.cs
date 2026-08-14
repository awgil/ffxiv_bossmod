namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class RevoltingRuinIII(BossModule module) : Components.GenericBaitAway(module, (uint)AID.RevoltingRuinIII, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Actor? source; // If this ever becomes null, it means the mechanic has not started, or it has ended
    private Actor? target;
    private DateTime activation;
    private bool secondTB = false;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (source == null)
        {
            return;
        }

        Actor? target;
        if (secondTB)
        {
            var byEnmity = RaidByEnmity(source, true);
            target = byEnmity.Count > 1 ? byEnmity[1] : null;
        }
        else
        {
            target = this.target;
        }
        if (target != null)
        {
            CurrentBaits.Add(new(source, target, new AOEShapeCone(30f, 45f.Degrees()), activation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII)
        {
            source = caster;
            target = WorldState.Actors.Find(spell.TargetID);
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII)
        {
            NumCasts++;
            secondTB = true;
            activation = WorldState.FutureTime(0.3f);
        }

        if (spell.Action.ID == (uint)AID.RevoltingRuinIII1)
        {
            NumCasts++;
            source = null;
            CurrentBaits.Clear();
        }
    }
}

sealed class GravenImage(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave)
{
    private readonly List<(ulong SourceID, int slot)> tethers = [];
    private readonly List<Knockback> knockbacks = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0)
        {
            return;
        }

        tethers.Add((source.InstanceID, slot));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PulseWave)
        {
            NumCasts++;
            tethers.Clear();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        knockbacks.Clear();

        foreach (var target in tethers)
        {
            if (target.slot != slot)
            {
                continue;
            }

            var source = WorldState.Actors.Find(target.SourceID);
            if (source == null)
            {
                continue;
            }

            knockbacks.Add(new(source.Position, 14f, actorID: source.InstanceID));
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

sealed class StackSpreadOrbs(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4)
{
    private bool? spread = null;
    private IconID? iconID = null;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.FlagrantFireIIIStack) or ((uint)AID.FlagrantFireIIISpread))
        {
            spread = null;
            iconID = null;
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        if (spread == null || iconID == null)
        {
            return;
        }

        // We do the opposite of whatever we are told
        if (iconID == IconID.FireRingQuestionMark)
        {
            if (spread == true)
            {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null)
                {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == false)
            {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }

        // We do what we are told
        if (iconID == IconID.FireRingBlueOrb)
        {
            if (spread == false)
            {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null)
                {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == true)
            {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.spreadIcon)
        {
            spread = true;
        }

        if (iconID == (uint)IconID.stackIcon)
        {
            spread = false;
        }

        if (iconID == (uint)IconID.FireRingQuestionMark)
        {
            this.iconID = (IconID)iconID;
        }

        if (iconID == (uint)IconID.FireRingBlueOrb)
        {
            this.iconID = (IconID)iconID;
        }
    }
}

sealed class BlizzardSafeSpots(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlizzardIIIBlowout, (uint)AID.BlizzardIIIBlowout2], new AOEShapeCone(40f, 45f.Degrees()));

sealed class WaveCannon(BossModule module) : Components.BaitAwayEveryone(module, module.Enemies((uint)OID.StatueBodyOrb).FirstOrDefault(), new AOEShapeRect(100f, 3f), (uint)AID.WaveCannon)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaveCannon)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4f)
{
    private readonly DateTime[] magicVulnerability = new DateTime[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                magicVulnerability[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction)
        {
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; i++)
            {
                towers[i].ForbiddenSoakers = Raid.WithSlot(false, true, true)
                    .WhereSlot(player => magicVulnerability[player] > Towers[i].Activation).Mask();
            }
        }
    }
}

class LightningSafeSpots(BossModule module) : Components.GenericAOEs(module)
{
    protected bool questionMark = false;
    private readonly List<(uint AID, AOEInstance AOE)> aoesAvailable = [];
    public readonly List<AOEInstance> aoes = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PurpleRingQuestionMark)
        {
            questionMark = true;
        }
        else if (iconID == (uint)IconID.PurpleRingBlueOrb)
        {
            questionMark = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.ThrummingThunderIII) or ((uint)AID.ThrummingThunderIII1) or ((uint)AID.ThrummingThunderIII2))
        {
            aoesAvailable.Add((spell.Action.ID, new AOEInstance(new AOEShapeRect(40f, 5f), spell.LocXZ, spell.Rotation, actorID: caster.InstanceID)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.ThrummingThunderIII) or ((uint)AID.ThrummingThunderIII1) or ((uint)AID.ThrummingThunderIII2))
        {
            ++NumCasts;
            aoesAvailable.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        foreach (var currentAOE in aoesAvailable)
        {
            if (questionMark)
            {
                if (currentAOE.AID == (uint)AID.ThrummingThunderIII2)
                {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (!questionMark)
            {
                if (currentAOE.AID is ((uint)AID.ThrummingThunderIII) or ((uint)AID.ThrummingThunderIII1))
                {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            Stacks.RemoveAt(0);
        }
    }
}

sealed class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DoubleTroubleTrap1)
{
    private readonly List<Knockback> knockbacks = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        knockbacks.Clear();

        var stack = Module.FindComponent<DoubleTroubleTrapStacks>();
        if (stack == null)
        {
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        foreach (var stackPoint in stack.Stacks)
        {
            if (actor.Position.InCircle(stackPoint.Target.Position, stackPoint.Radius))
            {
                knockbacks.Add(new(stackPoint.Target.Position, 14f, stackPoint.Activation, actorID: stackPoint.Target.InstanceID));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

sealed class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private DateTime? activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfJudgment)
        {
            activation = Module.CastFinishAt(spell, 3.0f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;

            if (NumCasts >= 3)
            {
                activation = null;
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (activation != null)
        {
            var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
            if (target != null)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCircle(5), activation.Value));
            }
        }
    }
}

sealed class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 4)
{
    private readonly List<Spread> spreadsIncoming = [];
    private int totalStacks = 0;
    public int NumCasts = 0;
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    private enum TetherGroup { Support, DPS }
    private TetherGroup tetherGroup = TetherGroup.Support;

    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];
        var partyConfigRolesSet = true;

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            partyConfigRolesSet = false;
        }

        // First puddles bait
        if (Stacks.Count != 0 && totalStacks == 4)
        {
            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Normal)
            {
                if (partyConfigRolesSet)
                {
                    if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or
                        PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1)
                    {
                        Arena.ZoneCircleOutline(Arena.Center + new WDir(0f, -19.5f), 1.0f, Colors.Safe, 2);
                    }

                    if (assignment is PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2 or
                        PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2)
                    {
                        Arena.ZoneCircleOutline(Arena.Center + new WDir(0f, 19.5f), 1.0f, Colors.Safe, 2);
                    }
                }
                else
                {
                    Arena.ZoneCircleOutline(Arena.Center + new WDir(0f, 19.5f), 1.0f, Colors.Safe, 2);
                    Arena.ZoneCircleOutline(Arena.Center + new WDir(0f, -19.5f), 1.0f, Colors.Safe, 2);
                }
            }

            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Uptime)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 87.500f), 1.0f, Colors.Safe, 2);
            }
        }

        // Second puddles bait - Ensuring the other isn't close - Only needed for normal strat
        if (Stacks.Count != 0 && totalStacks == 8)
        {
            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Normal)
            {
                var puddles = Module.FindComponent<GravitasPuddles>();

                if (puddles == null)
                {
                    return;
                }

                var puddleSources = puddles.puddles;

                var puddle1 = puddleSources.Where(p => p.actor.Position.Z > Module.Center.Z).Select(p => p.actor).MinBy(p => (p.Position - Module.Center).Length());
                if (puddle1 == null)
                {
                    return;
                }

                var puddle2 = puddleSources.Where(p => p.actor.Position.Z < Module.Center.Z).Select(p => p.actor).MinBy(p => (p.Position - Module.Center).Length());
                if (puddle2 == null)
                {
                    return;
                }

                if (partyConfigRolesSet)
                {
                    if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or
                        PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1)
                    {
                        Arena.ZoneCircleOutline(puddle2.Position - (puddle2.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
                    }

                    if (assignment is PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2 or
                        PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2)
                    {
                        Arena.ZoneCircleOutline(puddle1.Position - (puddle1.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
                    }
                }
                else
                {
                    Arena.ZoneCircleOutline(puddle1.Position - (puddle1.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
                    Arena.ZoneCircleOutline(puddle2.Position - (puddle2.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
                }
            }

            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Uptime)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 112.500f), 1.0f, Colors.Safe, 2);
            }
        }

        // Spreads hint
        if (Spreads.Count != 0)
        {
            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Normal)
            {
                if (partyConfigRolesSet)
                {
                    if (tetherGroup == TetherGroup.Support)
                    {
                        if (assignment == PartyRolesConfig.Assignment.MT)
                        {
                            Arena.ZoneCircleOutline(new WPos(90.054f, 100.718f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.OT)
                        {
                            Arena.ZoneCircleOutline(new WPos(109.197f, 99.618f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.H1)
                        {
                            Arena.ZoneCircleOutline(new WPos(87.971f, 86.119f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.H2)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.566f, 112.415f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or
                            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                        {
                            Arena.ZoneCircleOutline(Module.Center, 1.0f, Colors.Safe, 2);
                        }
                    }

                    if (tetherGroup == TetherGroup.DPS)
                    {
                        if (assignment == PartyRolesConfig.Assignment.M1)
                        {
                            Arena.ZoneCircleOutline(new WPos(90.054f, 100.718f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.M2)
                        {
                            Arena.ZoneCircleOutline(new WPos(109.197f, 99.618f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.R1)
                        {
                            Arena.ZoneCircleOutline(new WPos(87.971f, 86.119f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.R2)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.566f, 112.415f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or
                            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                        {
                            Arena.ZoneCircleOutline(Module.Center, 1.0f, Colors.Safe, 2);
                        }
                    }
                }
                else
                { // If party roles are not configured, we just show all available spots for that ROLE group
                    if (tetherGroup == TetherGroup.Support ? pc.Class.IsSupport() : tetherGroup == TetherGroup.DPS && pc.Class.IsDD())
                    {
                        // MT/M1 - [90.054, 0.000, 100.718, -178.033]
                        Arena.ZoneCircleOutline(new WPos(90.054f, 100.718f), 1.0f, Colors.Safe, 2);

                        // H1/R1 - [87.971, 0.000, 86.119, 37.967]
                        Arena.ZoneCircleOutline(new WPos(87.971f, 86.119f), 1.0f, Colors.Safe, 2);

                        // OT/M2 - [109.197, 0.000, 99.618, -87.793]
                        Arena.ZoneCircleOutline(new WPos(109.197f, 99.618f), 1.0f, Colors.Safe, 2);

                        // H2/R2 - [113.566, 0.000, 112.415, -133.273]
                        Arena.ZoneCircleOutline(new WPos(113.566f, 112.415f), 1.0f, Colors.Safe, 2);
                    }
                    else
                    {
                        // Middle
                        Arena.ZoneCircleOutline(Module.Center, 1.0f, Colors.Safe, 2);
                    }
                }
            }

            if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2Uptime)
            {
                if (partyConfigRolesSet)
                {
                    if (tetherGroup == TetherGroup.Support)
                    {
                        if (assignment == PartyRolesConfig.Assignment.MT)
                        {
                            Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.OT)
                        {
                            Arena.ZoneCircleOutline(new WPos(107.000f, 100.000f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.H1 && totalStacks == 4)
                        {
                            Arena.ZoneCircleOutline(new WPos(86.500f, 87.500f), 1.0f, Colors.Safe, 2);
                        }
                        else if (assignment == PartyRolesConfig.Assignment.H1 && totalStacks == 8)
                        {
                            Arena.ZoneCircleOutline(new WPos(86.500f, 112.500f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.H2 && totalStacks == 4)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.500f, 87.500f), 1.0f, Colors.Safe, 2);
                        }
                        else if (assignment == PartyRolesConfig.Assignment.H2 && totalStacks == 8)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.500f, 112.500f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or
                            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                        {
                            Arena.ZoneCircleOutline(Module.Center, 1.0f, Colors.Safe, 2);
                        }
                    }

                    if (tetherGroup == TetherGroup.DPS)
                    {
                        if (assignment == PartyRolesConfig.Assignment.M1)
                        {
                            Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.M2)
                        {
                            Arena.ZoneCircleOutline(new WPos(107.000f, 100.000f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.R1 && totalStacks == 4)
                        {
                            Arena.ZoneCircleOutline(new WPos(86.500f, 87.500f), 1.0f, Colors.Safe, 2);
                        }
                        else if (assignment == PartyRolesConfig.Assignment.R1 && totalStacks == 8)
                        {
                            Arena.ZoneCircleOutline(new WPos(86.500f, 112.500f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment == PartyRolesConfig.Assignment.R2 && totalStacks == 4)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.500f, 87.500f), 1.0f, Colors.Safe, 2);
                        }
                        else if (assignment == PartyRolesConfig.Assignment.R2 && totalStacks == 8)
                        {
                            Arena.ZoneCircleOutline(new WPos(113.500f, 112.500f), 1.0f, Colors.Safe, 2);
                        }

                        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or
                            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                        {
                            Arena.ZoneCircleOutline(Module.Center, 1.0f, Colors.Safe, 2);
                        }
                    }
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0)
        {
            return;
        }

        if (source.Position.AlmostEqual(new(102.500f, 22.500f), 5))
        {
            AddStack(target, WorldState.FutureTime(6.5f));
            totalStacks++;
        }

        if (source.Position.AlmostEqual(new(126.000f, 41.500f), 5))
        {
            spreadsIncoming.Add(new(target, 5, WorldState.FutureTime(10.6f)));
            tetherGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gravitas && Stacks.Count > 0)
        {
            Stacks.RemoveAt(0);
            NumCasts++;
            Spreads.AddRange(spreadsIncoming);
            spreadsIncoming.Clear();
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre && Spreads.Count > 0)
        {
            Spreads.RemoveAt(0);
        }
    }
}

sealed class GravitasPuddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5f, _ => [], (uint)AID.Gravitas)
{
    public List<(Actor actor, uint colour)> puddles = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PurplePuddles)
        {
            puddles.Add((actor, Colors.AOE));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleSoakReady)
        {
            var puddle = puddles.FindIndex(p => p.actor.InstanceID == actor.InstanceID);
            puddles[puddle] = (actor, Colors.SafeFromAOE);
        }

        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleExplosion)
        {
            var puddle = puddles.FindIndex(p => p.actor.InstanceID == actor.InstanceID);
            if (puddle >= 0)
            {
                puddles.RemoveAt(puddle);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var puddle in puddles)
        {
            Shape.Draw(Arena, puddle.actor.Position, puddle.actor.Rotation, puddle.colour);
        }
    }
}

sealed class GravitationalWave(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeRect rect = new(40f, 20f);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)Animations.PulseOrbStart)
        {
            _aoe = [new(rect, Arena.Center.Quantized(), (actor.OID == (uint)OID.YellowOrb ? 1f : -1f) * 90.Degrees())];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.GravitationalWave) or ((uint)AID.IntemperateWill))
        {
            ++NumCasts;
            _aoe = [];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
}

sealed class TeleTrouncing(BossModule module) : BossComponent(module)
{
    public int NumCasts = 0;
    private (Direction direction, DateTime activation)? Debuff1;
    private (Direction direction, DateTime activation)? Debuff2;
    private readonly List<WPos> hints = [];
    private enum Direction { UP, DOWN, LEFT, RIGHT }
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1)
        {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var player = Raid.FindSlot(actor.InstanceID);
        if (player is not (>= 0 and PartyState.PlayerSlot))
        {
            return;
        }

        Direction? dir = status.ID switch
        {
            (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2 => Direction.UP,
            (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2 => Direction.DOWN,
            (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2 => Direction.LEFT,
            (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => null
        };

        if (dir == null)
        {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
        {
            Debuff2 = (dir.Value, status.ExpireAt);
        }
        else
        {
            Debuff1 = (dir.Value, status.ExpireAt);
        }

        if (Debuff1 == null || Debuff2 == null)
        {
            return;
        }

        // Case 1: Both debuffs are in the same direction
        if (Debuff1.Value.direction == Debuff2.Value.direction)
        {
            if (Debuff1.Value.direction == Direction.DOWN)
            { // A waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(87.750f, 88.030f));
                    hints.Add(new WPos(87.750f, 93.570f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(112.000f, 94.000f));
                    hints.Add(new WPos(112.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.LEFT)
            { // B waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(112.135f, 87.993f));
                    hints.Add(new WPos(106.579f, 87.922f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(106.000f, 112.000f));
                    hints.Add(new WPos(100.000f, 112.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.UP)
            { // C waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(111.989f, 112.003f));
                    hints.Add(new WPos(112.125f, 106.306f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(88.000f, 106.000f));
                    hints.Add(new WPos(88.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.RIGHT)
            { // D waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(88.069f, 112.037f));
                    hints.Add(new WPos(93.798f, 112.161f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(94.000f, 88.000f));
                    hints.Add(new WPos(100.000f, 88.000f));
                }
            }

            return;
        }

        // Case 2: Both debuffs are in different directions
        var debuff1First = Debuff1.Value.activation <= Debuff2.Value.activation;

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.LEFT))
        {

            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                }
                else
                {
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(88.000f, 112.000f));
                    hints.Add(new WPos(94.000f, 112.000f));
                }
                else
                {
                    hints.Add(new WPos(94.000f, 112.000f));
                    hints.Add(new WPos(88.000f, 112.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.RIGHT))
        {
            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                }
                else
                {
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(88.000f, 94.000f));
                    hints.Add(new WPos(88.000f, 88.000f));
                }
                else
                {
                    hints.Add(new WPos(88.000f, 88.000f));
                    hints.Add(new WPos(88.000f, 94.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.RIGHT))
        {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                }
                else
                {
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(112.000f, 88.000f));
                    hints.Add(new WPos(106.000f, 88.000f));
                }
                else
                {
                    hints.Add(new WPos(106.000f, 88.000f));
                    hints.Add(new WPos(112.000f, 88.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.LEFT))
        {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                }
                else
                {
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                }
            }
            else if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(112.000f, 106.000f));
                    hints.Add(new WPos(112.000f, 112.000f));
                }
                else
                {
                    hints.Add(new WPos(112.000f, 112.000f));
                    hints.Add(new WPos(112.000f, 106.000f));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2
            or (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2
            or (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2
            or (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2)
        {

            var player = Raid.FindSlot(actor.InstanceID);
            if (player != PartyState.PlayerSlot)
            {
                return;
            }

            if (hints.Count != 0)
            {
                hints.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts == 16)
        {
            return;
        }

        if (Debuff1 == null || Debuff2 == null)
        {
            return;
        }
        var count = hints.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(hints[i], 1.0f, i == 0 ? Colors.Safe : default, 2f);
        }
    }
}

sealed class GravenImage3(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 1, 1)
{
    private static readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    private enum TetherGroup { Support, DPS }
    private TetherGroup? tetherSleepGroup = null;
    private TetherGroup? tetherConfusionGroup = null;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        if (source.Position.AlmostEqual(new(95.000f, 25.000f), 5f))
        {
            tetherConfusionGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
        }
        else if (source.Position.AlmostEqual(new(107.000f, 43.000f), 5f))
        {
            tetherSleepGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
            AddSpread(target, WorldState.FutureTime(6.5f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IdyllicWill)
        {
            Spreads.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        // Static strategy - keep separate to make it easier to manage
        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo && dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo && !dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
                }
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow && dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow && !dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }
        }
    }
}

sealed class Gaze(BossModule module) : Components.GenericGaze(module)
{
    private Eye[] _eye = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)Animations.EyeStart)
        {
            _eye = [new Eye(actor.Position, WorldState.FutureTime(9.9d), inverted: actor.OID == (uint)OID.StatueYellowEye)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.IndolentWill) or ((uint)AID.AveMaria))
        {
            ++NumCasts;
            _eye = [];
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eye;
}
