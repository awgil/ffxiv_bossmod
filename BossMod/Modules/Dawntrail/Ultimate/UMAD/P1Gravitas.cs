namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1GravitasVitrophyre : Components.UniformStackSpread
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();
    readonly List<Spread> _predicted = [];

    Angle _facingBoss;

    public void SetNegativeOffset(float value)
    {
        for (var i = 0; i < Stacks.Count; i++)
            Stacks.Ref(i).Activation -= TimeSpan.FromSeconds(value);
        for (var i = 0; i < Spreads.Count; i++)
            Spreads.Ref(i).Activation -= TimeSpan.FromSeconds(value);
    }

    public P1GravitasVitrophyre(BossModule module) : base(module, 5, 5)
    {
        PermitOverlap = true;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage && WorldState.Actors.Find(tether.Target) is { } target)
        {
            if (source.Position.AlmostEqual(new(102.5f, 27), 5))
                AddStack(target, WorldState.FutureTime(6.5f));
            else
                _predicted.Add(new(target, 5, WorldState.FutureTime(10.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gravitas && Stacks.Count > 0)
        {
            _facingBoss = (Arena.Center - Stacks[0].Target.Position).ToAngle();
            Stacks.RemoveAt(0);
            Spreads.AddRange(_predicted);
            _predicted.Clear();
        }

        if ((AID)spell.Action.ID == AID.Vitrophyre && Spreads.Count > 0)
            Spreads.RemoveAt(0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_predicted.Any(t => t.Target == actor))
            hints.Add("Prepare to spread!", false);

        if (IsSpreadTarget(actor) && Module.Enemies(OID.Gravitas).Any(g => actor.Position.InCircle(g.Position, 10)))
            hints.Add("Bait away from puddles!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (_config.P1GravityPuddleStrategy)
        {
            case UMADConfig.P1GravityPuddlePlacement.None:
                base.AddAIHints(slot, actor, assignment, hints);
                break;

            case UMADConfig.P1GravityPuddlePlacement.StackAll:
                if (Stacks.FirstOrNull() is { Activation: var a })
                {
                    WPos dest;
                    if (!Module.Enemies(OID.Gravitas).Any())
                    {
                        dest = new(99.5f, 88);
                        if (Module.FindComponent<P1BlizzardIIIBlowout>()?.Check(slot, actor, dest) == true)
                            dest.X += 1;
                    }
                    else
                        dest = new(100, 112);
                    if (IsStackTarget(actor))
                        hints.AddForbiddenZone(ShapeContains.PrecisePosition(dest, new(0, 1), 0.5f, actor.Position, 0.5f), a);
                    else
                        hints.AddForbiddenZone(ShapeContains.InvertedCircle(new(100, dest.Z), 4.5f), a);
                }
                else
                    base.AddAIHints(slot, actor, assignment, hints);
                break;
        }

        if (IsSpreadTarget(actor))
        {
            var gravity = Module.Enemies(OID.Gravitas).Select(g => ShapeContains.Circle(g.Position, 5 + SpreadRadius + ExtraAISpreadThreshold)).ToList();
            // away from gravity
            hints.AddForbiddenZone(ShapeContains.Union(gravity), Spreads[0].Activation);

            var spreadParty = _config.P1GravityPuddleSpread[assignment];
            // spread on predetermined side, otherwise we confuse our teammates
            if (spreadParty >= 0)
            {
                var safeSide = spreadParty == 0 ? _facingBoss + 90.Degrees() : _facingBoss - 90.Degrees();
                var ctr = Arena.Center + safeSide.ToDirection() * 5;
                hints.GoalZones.Add(p => p.InCone(ctr, safeSide, 90.Degrees()) ? 1 : 0);
            }
        }
        else if (Spreads.Count > 0 && Stacks.Count == 0)
            // stay under boss for max safety
            hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor.Position, 1));
    }
}

class P1GravitasPuddle : Components.PersistentVoidzoneAtCastTarget
{
    readonly List<Actor> _puddles = [];

    public P1GravitasPuddle(BossModule module) : base(module, 5, AID.Gravitas, _ => [], 0.7f)
    {
        Sources = _ => _puddles;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Gravitas)
            _puddles.Add(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.Gravitas && state == 0x00100020)
            _puddles.Remove(actor);
    }
}

// EAnim 00100020 for puddles "priming" (can be soaked)
// EState 4 for puddles vanishing, whether soaked or not; unsoaked ones vanish about 0.7s later
class P1GravitasPuddleSoak(BossModule module) : Components.CastCounter(module, AID.GravityIII)
{
    public readonly List<Actor> Puddles = [];

    public bool EnableHints;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in Puddles)
            Arena.ZoneCircle(p.Position, 5, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            hints.Add("Soak puddles!", !Puddles.Any(p => actor.Position.InCircle(p.Position, 5)));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.Gravitas && state == 0x00100020)
            Puddles.Add(actor);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if ((OID)actor.OID == OID.Gravitas && state == 4)
        {
            Puddles.Remove(actor);
            if (Puddles.Count == 0)
                EnableHints = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            NumCasts++;
    }
}

class P1GravitationalWaveIntemperateWill(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted).Select(w => w with { Risky = Risky });

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Check, aoe.Activation);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBFBD && state == 0x00400080)
            _predicted = new(new AOEShapeCone(100, 90.Degrees()), Arena.Center, 90.Degrees(), WorldState.FutureTime(5.2f));

        if (actor.OID == 0x1EBFBC && state == 0x00400080)
            _predicted = new(new AOEShapeCone(100, 90.Degrees()), Arena.Center, -90.Degrees(), WorldState.FutureTime(5.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GravitationalWave or AID.IntemperateWill)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}
