namespace BossMod.Dawntrail.Savage.M10STheXtremes;

sealed class SickestTakeOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SickestTakeOff, new AOEShapeRect(50f, 7.5f));
sealed class SickSwell1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SickSwell1, 10f, kind: Kind.DirForward);

sealed class AwesomeSplashSlab(BossModule module) : Components.GenericStackSpread(module, false)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private readonly FreakyPyrotation _freaky = module.FindComponent<FreakyPyrotation>()!;
    private readonly List<Stack> _delayFreaky = [];
    private enum Mechanic
    {
        Stack,
        Spread,
        None
    }
    private Mechanic _mech = Mechanic.None;
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_)
        {
            //var party = Raid.WithoutSlot(false, true, true);
            var party = _debuff.WaterPlayers.Any() ? _debuff.GetWaterActors() : Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var act = WorldState.FutureTime(15.8d);
            switch (status.Extra)
            {
                // delay adding/hinting mechanic until Freaky Pyrorotation resolves to avoid clutter
                // Freaky Pyrorotation icons appear .15s before light party stack during arena split
                case 0x3ED:
                    // light party stack
                    for (var i = 0; i < len; i++)
                    {
                        if (party[i].Role == Role.Healer)
                        {
                            _mech = Mechanic.Stack;
                            Stack stack = new(party[i], 6f, 4, activation: act);
                            if (_freaky.ActiveStacks.Count == 0)
                            {
                                Stacks.Add(stack);
                            }
                            else
                            {
                                _delayFreaky.Add(stack);
                            }
                        }
                    }
                    break;
                case 0x3EE:
                    // spread
                    for (var i = 0; i < len; i++)
                    {
                        _mech = Mechanic.Spread;
                        Spreads.Add(new(party[i], 5f, activation: act));
                    }
                    break;
                case 0x3EF:
                    // watersnaking stack
                    for (var i = 0; i < len; i++)
                    {
                        if (party[i].Role == Role.Healer)
                        {
                            /*
                            if (party[i].FindStatus((uint)SID.Watersnaking) != null)
                            {
                                _mech = Mechanic.Stack;
                                Stacks.Add(new(party[i], 6f, 4, activation: act));
                            }
                            */
                            _mech = Mechanic.Stack;
                            Stacks.Add(new(party[i], 6f, 4, activation: act, forbiddenPlayers: _debuff.FirePlayers.Any() ? _debuff.FirePlayers : default));
                        }
                    }
                    break;
                case 0x3F0:
                    // watersnaking spread
                    for (var i = 0; i < len; i++)
                    {
                        /*
                        if (party[i].FindStatus((uint)SID.Watersnaking) != null)
                        {
                            _mech = Mechanic.Spread;
                            Spreads.Add(new(party[i], 5f, activation: act));
                        }
                        */
                        _mech = Mechanic.Spread;
                        Spreads.Add(new(party[i], 5f, activation: act));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AwesomeSplash or (uint)AID.AwesomeSlab or (uint)AID.AwesomeSplashAerial or (uint)AID.AwesomeSlabAerial)
        {
            _mech = Mechanic.None;
            Spreads.Clear();
            Stacks.Clear();
        }
        else if (spell.Action.ID == _freaky.StackAction)
        {
            Stacks.AddRange(_delayFreaky);
            _delayFreaky.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_mech == Mechanic.None)
            return;

        // TODO: only display hint if watersnaking
    }
}

sealed class AlleyOopWater(BossModule module) : Components.GenericBaitAway(module, alwaysDrawOtherBaits: true)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private readonly M10STheXtremesConfig _config = Service.Config.Get<M10STheXtremesConfig>();
    private readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();

    public enum State { None, BlueOnly, Snaking, SplitArena, PostInsane }
    public State CurrentState = State.None;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDipCast or (uint)AID.ReverseAlleyOopCast)
        {
            var targets = GetTargets();
            var len = targets.Length;

            for (var i = 0; i < len; i++)
            {
                CurrentBaits.Add(new(caster, targets[i], new AOEShapeCone(60f, 15f.Degrees()), Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDip1 or (uint)AID.ReverseAlleyOop1)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    private Actor[] GetTargets()
    {
        var party = Raid.WithSlot(false, true, true);
        var waters = _debuff.WaterPlayers;

        if (waters.Any())
        {
            party = [.. party.IncludedInMask(waters)];
        }

        var len = party.Length;

        if (len == 0)
            return [];

        var actors = new Actor[len];
        for (var i = 0; i < len; i++)
        {
            actors[i] = party[i].Item2;
        }

        return actors;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // same mechanic, different positions depending on when it's done
        // use public var set my state machine to control which hints to display?
        if (_config.ShowWaterAlleyOopHints)
        {
            if (CurrentBaits.Count == 0)
                return;

            DrawPlayerPosition(pcSlot, pc);
        }
    }

    private void DrawPlayerPosition(int pcSlot, Actor pc)
    {
        // config assignment resolve
        // slot = player slot in raid
        // group = index of option in config (eg. N = 0, NE = 1, etc)
        // want to make it so config assignments should be valid but only require player's assignment to be set
        // don't use resolve since it returns empty unless everyone's assigned

        if (CurrentState == State.None)
            return;

        if (!_config.WaterAlleyOopAssignment.Validate())
            return;

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        if (assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        var enemies = Module.Enemies((uint)OID.DeepBlue);
        if (enemies.Count == 0)
            return;

        var deepblue = enemies[0];
        var position = deepblue.Position;
        var rotation = deepblue.Rotation;

        var configAssignment = _config.WaterAlleyOopAssignment;
        var group = configAssignment[assignment];

        var role = assignment switch
        {
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => 0,
            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => 1,
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => 2,
            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => 3,
            _ => -1
        };

        var radius = CurrentState switch
        {
            State.BlueOnly or State.Snaking or State.SplitArena => 1f,
            State.PostInsane => 2f,
            _ => 0f
        };

        if (CurrentState == State.BlueOnly)
        {
            var targetRot = rotation - group * 45f.Degrees();
            var targetPos = deepblue.Position + targetRot.ToDirection() * (deepblue.HitboxRadius + 1.5f);
            var targetSd = new SDCircle(targetPos, radius);
            Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
        }
        else if (CurrentState == State.Snaking)
        {
            // boss jumps to NW or SW (AID 46457 to target location around 250s mark); uses same jump ability multiple times throughout fight
            // boss still rotating when cast/bait goes out, use position
            // NA does TH/MR split with TM on outside, HR on inside, DD towards corner
            // JP does T/MRH split, with T on outside, MRH with M towards corner
            var waters = _debuff.WaterPlayers;
            if (waters.Any())
            {
                if (!waters[pcSlot])
                    return;
            }

            var middle = new WPos(position.X, 100f);
            var angle = (middle - position).ToAngle();
            var sign = angle.AlmostEqual(0f.Degrees(), 0.1f) ? 1f : -1f;

            WPos targetPos;

            if (_config.HintOption == Strategy.Hector)
            {
                targetPos = role switch
                {
                    0 => position + angle.ToDirection() * deepblue.HitboxRadius,
                    1 => position + (angle + 30f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    2 => position + (angle + 180f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    3 => position + (angle + 150f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    _ => default
                };
            }
            else
            {
                targetPos = role switch
                {
                    0 => position + angle.ToDirection() * deepblue.HitboxRadius,
                    1 => position + (angle + 140f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    2 => position + (angle + 180f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    3 => position + (angle + 160f.Degrees() * sign).ToDirection() * deepblue.HitboxRadius,
                    _ => default
                };
            }

            // shift a little so circle isn't right on arena bound
            targetPos += new WDir(1f, 0f);
            var targetSd = new SDCircle(targetPos, radius);
            Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger, 2f);
        }
        else if (CurrentState == State.SplitArena)
        {
            // NA and JP use same strat, LP1 West LP2 East
            // boss always jumps to 88f or 112f
            // far side hugs flame about 5-6f away from center
            // TM 45deg from boss towards wall
            // HR directly above boss near flame puddle
            var west = Angle.AnglesCardinals[0];
            var isWest = (position - Arena.Center).ToAngle().AlmostEqual(west, 0.1f);

            WPos targetPos;

            if (isWest)
            {
                targetPos = assignment switch
                {
                    PartyRolesConfig.Assignment.R1 => new(88f, 92f),
                    PartyRolesConfig.Assignment.MT => new(84f, 96f),
                    PartyRolesConfig.Assignment.M1 => new(84f, 104f),
                    PartyRolesConfig.Assignment.H1 => new(88f, 108),
                    PartyRolesConfig.Assignment.R2 => new(106f, 81f),
                    PartyRolesConfig.Assignment.OT => new(106f, 96f),
                    PartyRolesConfig.Assignment.M2 => new(106f, 104f),
                    PartyRolesConfig.Assignment.H2 => new(106f, 119f),
                    _ => default
                };
            }
            else
            {
                targetPos = assignment switch
                {
                    PartyRolesConfig.Assignment.R1 => new(94f, 81f),
                    PartyRolesConfig.Assignment.MT => new(94f, 96f),
                    PartyRolesConfig.Assignment.M1 => new(94f, 104f),
                    PartyRolesConfig.Assignment.H1 => new(94f, 119f),
                    PartyRolesConfig.Assignment.R2 => new(112f, 92f),
                    PartyRolesConfig.Assignment.OT => new(116f, 96f),
                    PartyRolesConfig.Assignment.M2 => new(116f, 104f),
                    PartyRolesConfig.Assignment.H2 => new(112f, 108f),
                    _ => default
                };
            }

            var targetSd = new SDCircle(targetPos, radius);
            Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
        }
        else
        {
            // NA and JP use same strat
            var north = 180f.Degrees();
            var targetRot = north - group * 45f.Degrees();
            var targetPos = Arena.Center + targetRot.ToDirection() * (group % 2 == 0 ? 18f : 25.5f);
            var targetSd = new SDCircle(targetPos, radius);
            Arena.ZoneCircleOutline(targetPos, radius, targetSd.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
        }
    }
}

sealed class AlleyOopWaterAfter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly double _delay = 2.8f;
    private readonly Angle _offset = 22.5f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AlleyOopDoubleDip1)
        {
            _aoes.Add(new(new AOEShapeCone(60f, 15f.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(_delay)));
        }

        // reverse does casts in both directions
        if (spell.Action.ID == (uint)AID.ReverseAlleyOop1)
        {
            _aoes.Add(new(new AOEShapeCone(60f, 7.5f.Degrees()), caster.Position, spell.Rotation + _offset, WorldState.FutureTime(_delay)));
            _aoes.Add(new(new AOEShapeCone(60f, 7.5f.Degrees()), caster.Position, spell.Rotation - _offset, WorldState.FutureTime(_delay)));
        }

        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDip2 or (uint)AID.ReverseAlleyOop2)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

sealed class DeepImpact(BossModule module) : Components.GenericBaitProximity(module)
{
    // baited by blue debuff as well as furthest
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeepImpactCast)
        {
            var activation = Module.CastFinishAt(spell).AddSeconds(0.3d);
            Bait bait = new(caster, new AOEShapeCircle(6f), activation, 1, false, centerAtTarget: true, tankbuster: true, caster: (uint)OID.DeepBlue, forbidden: _debuff.FirePlayers, damageType: AIHints.PredictedDamageType.Tankbuster);
            CurrentBaits.Add(bait);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DeepImpact)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
sealed class DeepImpactKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly DeepImpact _impact = module.FindComponent<DeepImpact>()!;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_impact.ActiveBaits.Count == 0)
            return [];

        var baits = CollectionsMarshal.AsSpan(_impact.ActiveBaits);
        var bait = baits[0];

        if (!_impact.IsBaitTarget(ref bait, actor))
            return [];

        var direction = (actor.Position - bait.Position).ToAngle();
        return new Knockback[1] { new(actor.Position, 10f, bait.Activation, null, direction, Kind.DirForward) };
    }
}

sealed class DiversDareBlue(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDareBlue);
// blue casts sick swell before deep varial, tethers wave, varial happens on opposite side
// still resolving alley oop followup so maybe show after end
// at same time cast ends is action timeline for add; always center but rotation changes, opposite side
// shared tankbuster still being resolved; maybe hide for tanks and show for everyone else? or just config
//sealed class DeepVarial(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepVarial, new AOEShapeCone(60f, 60f.Degrees()));
sealed class DeepVarial(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepVarial, new AOEShapeCone(60f, 60f.Degrees()))
{
    private readonly M10STheXtremesConfig _config = Service.Config.Get<M10STheXtremesConfig>();
    AOEInstance _aoe;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.SickSwell && id == 0x2487)
        {
            var enemies = Module.Enemies((uint)OID.DeepBlue);
            if (enemies.Count == 0)
                return;

            var deepblue = enemies[0];
            var rotation = actor.Rotation + 180f.Degrees();
            var origin = Arena.Center - rotation.ToDirection() * 20f;
            var activation = WorldState.FutureTime(14.2d);
            _aoe = new(Shape, origin, rotation, activation, actorID: deepblue.InstanceID, shapeDistance: Shape.Distance(origin, rotation));

            if (_config.ShowDeepVarialEarly)
            {
                Casters.Add(_aoe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (!_config.ShowDeepVarialEarly && ActiveCasters.Length == 0)
            {
                Casters.Add(_aoe);
            }
        }
    }
}
sealed class AwesomeSplashSlabAerial(BossModule module) : Components.GenericStackSpread(module)
{
    // activate this before watersnaking to get status on gain
    // otherwise find actors with watersnaking status on cast end, not sure if there's noticeable performance hit
    // or maybe have a separate component for only tracking water/fire snaking status to read from?
    // TODO: need to figure out what determines spread vs stack
    // wave has action timeline but 0x2487 but same for stack spread
    // map effect differents, 2(N) 4(S) 00800040 (stack) 08000400 (spread)
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is not 0x02 and not 0x04)
            return;

        if (state is not 0x00800040 and not 0x08000400)
            return;

        var party = _debuff.GetWaterActors();
        var len = party.Length;

        for (var i = 0; i < len; i++)
        {
            var player = party[i];
            switch (state)
            {
                case 0x00800040:
                    if (player.Role == Role.Healer)
                    {
                        Stacks.Add(new(player, 6f, 4, 4, WorldState.FutureTime(4.7d), _debuff.FirePlayers));
                    }
                    break;
                case 0x08000400:
                    Spreads.Add(new(player, 5f, WorldState.FutureTime(4.7d)));
                    break;
            }
            /*
            if (state == 0x00800040)
            {
                // who does it target if healer is dead?
                if (player.Role == Role.Healer)
                {
                    Stacks.Add(new(player, 6f, 4, 4, WorldState.FutureTime(4.7d), _debuff.FirePlayers));
                }
            }
            if (state == 0x08000400)
            {
                Spreads.Add(new(player, 5f, WorldState.FutureTime(4.7d)));
            }
            */
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AwesomeSplashAerial or (uint)AID.AwesomeSlabAerial)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}
