namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V14LoneSwordmaster;

sealed class DebuffTracker(BossModule module) : BossComponent(module)
{
    // any times when attacks cast without debuffs? necessary to track status lost?
    // what happens when status lost and status gained on same frame? (E -> N-E)
    public uint[] Malefic = new uint[8];
    public uint[] Magnet = new uint[8];
    public BitMask Floating = default;

    public Angle[] GetAngles(int slot)
    {
        if (slot is < 0 or > 7)
            return [];

        if (Malefic[slot] == 0)
            return [];

        Angle[] angles = Malefic[slot] switch
        {
            (uint)SID.MaleficE => [Angle.AnglesCardinals[3]],
            (uint)SID.MaleficW => [Angle.AnglesCardinals[0]],
            (uint)SID.MaleficEW => [Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficS => [Angle.AnglesCardinals[1]],
            (uint)SID.MaleficSE => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficSW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficN => [Angle.AnglesCardinals[2]],
            (uint)SID.MaleficNE => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNS => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[1]],
            _ => []
        };

        return angles;
    }
    public Angle[] GetUnsafeAngles(int slot)
    {
        if (slot is < 0 or > 7)
            return [];

        if (Malefic[slot] == 0)
            return [];

        Angle[] angles = Malefic[slot] switch
        {
            (uint)SID.MaleficE => [Angle.AnglesCardinals[0]],
            (uint)SID.MaleficW => [Angle.AnglesCardinals[3]],
            (uint)SID.MaleficEW => [Angle.AnglesCardinals[0], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficS => [Angle.AnglesCardinals[2]],
            (uint)SID.MaleficSE => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficSW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficN => [Angle.AnglesCardinals[1]],
            (uint)SID.MaleficNE => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNS => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[2]],
            _ => []
        };

        return angles;
    }

    public Angle? GetUnyieldingAngle(int slot, bool sourceNorthSouth)
    {
        if (slot is < 0 or > 7)
            return null;

        if (Malefic[slot] == 0)
            return null;

        Angle? angle = Malefic[slot] switch
        {
            (uint)SID.MaleficSE => sourceNorthSouth ? Angle.AnglesCardinals[0] : Angle.AnglesCardinals[2],
            (uint)SID.MaleficNE => sourceNorthSouth ? Angle.AnglesCardinals[0] : Angle.AnglesCardinals[1],
            (uint)SID.MaleficSW => sourceNorthSouth ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[2],
            (uint)SID.MaleficNW => sourceNorthSouth ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[1],
            _ => null
        };

        return angle;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id is >= (uint)SID.MaleficE and <= (uint)SID.MaleficNS)
        {
            Malefic[Raid.FindSlot(actor.InstanceID)] = status.ID;
        }
        else if (id is (uint)SID.PositiveCharge or (uint)SID.NegativeCharge)
        {
            Magnet[Raid.FindSlot(actor.InstanceID)] = status.ID;
        }
        else if (id == (uint)SID.MagneticLevitation)
        {
            Floating.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id is (uint)SID.MaleficE or <= (uint)SID.MaleficNS)
        {
            Malefic[Raid.FindSlot(actor.InstanceID)] = 0;
        }
        else if (id is (uint)SID.PositiveCharge or (uint)SID.NegativeCharge)
        {
            Magnet[Raid.FindSlot(actor.InstanceID)] = 0;
        }
        else if (id == (uint)SID.MagneticLevitation)
        {
            Floating.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }
}
sealed class LashOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LashOfLight, new AOEShapeCone(40f, 45f.Degrees()), 2);
sealed class EarthRendingEight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthRendingEight, 8f);
// could display early since crosses happen at circle location but could sacrifice precious AI uptime
sealed class EarthRendingEightCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthRendingEightCross, new AOEShapeCross(40f, 4f));
sealed class WaitingWounds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaitingWounds, 10f, 6);
sealed class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, (uint)AID.SteelsbreathRelease);
sealed class SteelsbreathRelease1(BossModule module) : Components.RaidwideCast(module, (uint)AID.SteelsbreathRelease1);
sealed class MawOfTheWolf(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MawOfTheWolf, new AOEShapeRect(80f, 40f));
sealed class StingOfTheScorpion(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StingOfTheScorpion);
sealed class PlummetSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PlummetSmall, 3f);
sealed class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, 5f, 4);
sealed class PlummetBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PlummetBig, 10f, 4);
sealed class PlummetProximity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PlummetProximity, 25f);
sealed class Concentrativity(BossModule module) : Components.RaidwideCast(module, (uint)AID.Concentrativity);
sealed class ConcentrativityKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.Concentrativity)
{
    // modify for per-player danger hint
    // ~5f if grounded, ~36f if floating
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private readonly List<Knockback> Casters = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var casters = CollectionsMarshal.AsSpan(Casters);
        var len = casters.Length;

        if (len == 0)
            return [];

        var kb = new Knockback[1];
        var caster = casters[0];
        kb[0] = new(caster.Origin, _debuffs.Floating[slot] ? 36f : 5f, caster.Activation);
        return kb;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(new(spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Clear();
        }
    }
}
sealed class ConcentravityMagnetFloor(BossModule module) : Components.GenericAOEs(module, warningText: "Move to safe square!")
{
    // floor sign based on EObjAnim MagnetFloor (0x00010002 = SW NE negative) constant?
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private readonly AOEShapeRect _shape = new(10f, 10f, 10f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Concentrativity)
        {
            var slot = Raid.FindSlot(Raid.Player()!.InstanceID);
            var isNegative = _debuffs.Magnet[slot] == (uint)SID.NegativeCharge;
            _aoes.Add(new(_shape, Arena.Center + new WDir(-10f, isNegative ? 10f : -10f)));
            _aoes.Add(new(_shape, Arena.Center + new WDir(10f, isNegative ? -10f : 10f)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Concentrativity)
        {
            _aoes.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count == 0)
            return;

        // make outside of arena unsafe so there's leeway for knockback
        hints.AddForbiddenZone(new AOEShapeRect(16f, 16f, 16f, invertForbiddenZone: true), Arena.Center);
    }
}
sealed class ConcentrativityRocks(BossModule module) : Components.GenericKnockback(module)
{
    // helper casts attract/repel on player, no need to check debuffs, 20f push/pull
    // followed by boss with 17f knockback from boss
    // stunned during knockbacks so clear after 1st
    private readonly List<Knockback> Casters = [with(2)];
    private DateTime _finishAt = default;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(Casters);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Repel or (uint)AID.Attract)
        {
            if (spell.TargetID == Raid.Player()!.InstanceID)
            {
                _finishAt = Module.CastFinishAt(spell);
                Casters.Add(new(caster.Position, 20f, _finishAt, kind: spell.Action.ID == (uint)AID.Repel ? Kind.AwayFromOrigin : Kind.TowardsOrigin));
                Casters.Add(new(Arena.Center, 17f, _finishAt.AddSeconds(4d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Repel or (uint)AID.Attract)
        {
            Casters.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        var len = kbs.Length;

        if (len == 0)
            return;

        // if attract, want to be on away from helper such that we get pulled into middle
        // if repel, want to be next to helper such that we get pushed towards middle
        // don't need to consider 2nd kb, should be safe as long as sent to center
        if (kbs[0].Kind == Kind.AwayFromOrigin)
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginIntoCircle(Arena.Center, kbs[0].Origin, 20f, 20f, Arena.Center, 2.5f), _finishAt);
        else
            hints.AddForbiddenZone(new SDKnockbackTowardsOriginIntoCircle(kbs[0].Origin, 20f, Arena.Center, 2.5f), _finishAt);
    }

    sealed class SDKnockbackTowardsOriginIntoCircle(WPos Origin, float MaxDistance, WPos CircleOrigin, float Radius) : ShapeDistance
    {
        private readonly WPos origin = Origin;
        private readonly float maxDistance = MaxDistance;
        private readonly WPos circleOrigin = CircleOrigin;
        private readonly float radius = Radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Contains(in WPos p)
        {
            var distance = (origin - p).Length();
            distance = distance > maxDistance ? maxDistance : distance;
            var projected = p + distance * (origin - p).Normalized();
            return !projected.InCircle(circleOrigin, radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
    }
}
sealed class HeavensConfluenceIcon(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.HeavensConfluence)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.HeavensConfluenceTarget1 or (uint)AID.HeavensConfluenceTarget2)
        {
            CurrentBaits.RemoveAt(0);
        }
    }
}
sealed class HeavensConfluence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle _inner = new(8f);
    private readonly AOEShapeDonut _outer = new(8f, 60f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        if (len == 0)
            return [];

        return aoes[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HeavensConfluenceIn1 or (uint)AID.HeavensConfluenceOut1)
        {
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(spell.Action.ID == (uint)AID.HeavensConfluenceIn1 ? _inner : _outer, caster.Position, activation: activation));
            _aoes.Add(new(spell.Action.ID == (uint)AID.HeavensConfluenceIn1 ? _outer : _inner, caster.Position, activation: activation.AddSeconds(2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HeavensConfluenceIn1 or (uint)AID.HeavensConfluenceIn2 or (uint)AID.HeavensConfluenceOut1 or (uint)AID.HeavensConfluenceOut2)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
sealed class WillOfTheUnderworld(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WillOfTheUnderworld, new AOEShapeRect(40f, 10f))
{
    // safe spot depends on player Malefic debuff
    // 1,2, or 4 at a time
    // in path 12, stand behind fallen rock with malefic in safe direction
    // slightly wider than 10f? ate a vuln stack sitting on the edge of safe spot
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        List<AOEInstance> unsafeAoes = [];
        var angles = _debuffs.GetUnsafeAngles(slot);
        var angleLen = angles.Length;

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            var aoe = aoes[i];

            for (var j = 0; j < angleLen; j++)
            {
                // roughly 1deg, unnecessary? actor angle seems equal to Angle.Cardinal
                if (aoe.Rotation.AlmostEqual(angles[j], 0.02f))
                {
                    unsafeAoes.Add(aoe);
                }
            }
        }

        return CollectionsMarshal.AsSpan(unsafeAoes);
    }
}
sealed class WillOfTheUnderworldRocks(BossModule module) : Components.GenericAOEs(module, (uint)AID.WillOfTheUnderworldRock, "Move behind safe rock!")
{
    // rocks only get E/W debuffs, player has E-W debuff, hide behind rocks based on direction
    private readonly List<Actor> _rocks = [];
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WillOfTheUnderworldRock)
        {
            var rocks = CollectionsMarshal.AsSpan(_rocks);
            var len = rocks.Length;
            var position = caster.Position;
            var rotation = caster.Rotation;
            var finishAt = Module.CastFinishAt(spell);
            if (len == 0)
            {
                _aoes.Add(new(new AOEShapeRect(40f, 5f), position, rotation, finishAt));
                return;
            }

            for (var i = 0; i < len; i++)
            {
                var rock = rocks[i];

                // only rocks in same lane
                if (Math.Abs(caster.Position.Z - rock.Position.Z) >= 1d)
                    continue;

                var status = rock.FindStatus((uint)SID.RockMaleficE) ?? rock.FindStatus((uint)SID.RockMaleficW);
                if (status == null)
                {
                    _aoes.Add(new(new AOEShapeRect(40f, 5f), position, rotation, finishAt));
                    return;
                }

                var debuffAngle = status!.Value.ID == (uint)SID.RockMaleficE ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0];
                if (caster.Rotation.AlmostEqual(debuffAngle, 0.02f))
                {
                    var dist = caster.DistanceToPoint(rock.Position);
                    _aoes.Add(new(new AOEShapeRect(dist, 5f), position, rotation, finishAt));
                }
                else
                {
                    _aoes.Add(new(new AOEShapeRect(40f, 5f), position, rotation, finishAt));
                }
            }
        }
    }
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.FallenRock && (status.ID is (uint)SID.RockMaleficE or (uint)SID.RockMaleficW))
        {
            _rocks.Add(actor);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WillOfTheUnderworldRock)
        {
            _aoes.Clear();
            _rocks.Clear();
        }
    }
}
sealed class CrusherOfLions(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrusherOfLions, new AOEShapeCone(40f, 45f.Degrees()))
{
    // safe spot depends on player Malefic debuff
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        List<AOEInstance> unsafeAoes = [];
        var angles = _debuffs.GetUnsafeAngles(slot);
        var angleLen = angles.Length;

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            var aoe = aoes[i];

            for (var j = 0; j < angleLen; j++)
            {
                if (aoe.Rotation.AlmostEqual(angles[j], 0.02f))
                {
                    unsafeAoes.Add(aoe);
                }
            }
        }

        return CollectionsMarshal.AsSpan(unsafeAoes);
    }
}

sealed class UnyieldingWill(BossModule module) : Components.GenericBaitAway(module)
{
    // safe spot depends on player Malefic debuff
    // ForceOfWillTether follows player along axis that ForceOfWillSmall is facing
    // each player gets targetted? if so, display AOE for non-player rect
    // 2nd tether breaks if short enough; possible to stand on it if starting helper is in a safe direction
    // anywhere from 1-3 directions safe
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private readonly Dictionary<Actor, (Actor Source, Actor Intermediate)> _playerTethers = [];
    private readonly List<(Actor Source, Actor Target)> _tethers = [];
    public override void Update()
    {
        base.Update();
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, 2f);
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // only fires 1 if player standing on top of tether helper; fires 2 if away from tether helper
        // player bound until AOEs finish so safe to ignore 2
        if (spell.Action.ID == (uint)AID.UnyieldingWill1)
        {
            CurrentBaits.Clear();
            _playerTethers.Clear();
            _tethers.Clear();
        }
    }
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        // how to AI handle case when tether helper spawns at player position so tether to player never happens?
        if (tether.ID == (uint)TetherID.UnyieldingWill)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null)
                return;

            _tethers.Add((source, target));
            CurrentBaits.Add(new(source, target, new AOEShapeRect(default, 2f)));

            var isTargetPlayer = Raid.WithoutSlot().Contains(target);

            if (!isTargetPlayer)
                return;

            var tethers = CollectionsMarshal.AsSpan(_tethers);
            var len = tethers.Length;
            for (var i = 0; i < len; i++)
            {
                if (tethers[i].Target == source)
                {
                    _playerTethers[target] = (tethers[i].Source, tethers[i].Target);
                    break;
                }
            }
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // add hint if player will be hit on unsafe side
        // add hint if player is in another player's baited path
        // can't control start position; only add bait away hint if another player between tether helper and target player
        // 2nd tether drops if player <= 2f from tether helper
        // does initial will have own rotation?
        if (ActiveBaits.Count == 0)
            return;

        var hasTether = _playerTethers.TryGetValue(actor, out var tethers);
        if (hasTether)
        {
            var inter = tethers.Intermediate;
            var angle = inter.AngleTo(actor);
            var angles = _debuffs.GetAngles(slot);
            var len = angles.Length;

            if (len > 0)
            {
                var safe = false;
                for (var i = 0; i < len; i++)
                {
                    if (angle.AlmostEqual(angles[i], 45f.Degrees().Rad))
                    {
                        safe = true;
                    }
                }

                if (!safe)
                {
                    hints.Add("Face safe side to AOE!");
                }
            }
        }

        // base avoid other player baited AOEs
        base.AddHints(slot, actor, hints);
        var count = CurrentBaits.Count;
        var id = actor.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            var b = CurrentBaits[i];
            if (b.Target.InstanceID != id)
            {
                continue;
            }
            if (PlayersClippedBy(ref b).Count != 0)
            {
                hints.Add(BaitAwayHint);
                return;
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
            return;

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var hasPlayer = _playerTethers.TryGetValue(pc, out var playerTether);
            //if (!b.Source.IsDead && b.Target.InstanceID != pcID && (AlwaysDrawOtherBaits || IsClippedBy(pc, ref b)))
            if (!b.Source.IsDead && (AlwaysDrawOtherBaits || IsClippedBy(pc, ref b)) && (!hasPlayer || b.Target.InstanceID != pcID && b.Source != playerTether.Source))
            {
                b.Shape.Draw(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var hasPlayer = _playerTethers.TryGetValue(pc, out var playerTether);
            //if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && b.Target.InstanceID == pcID))
            if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && (b.Target.InstanceID == pcID || hasPlayer && b.Source == playerTether.Source)))
            {
                b.Shape.Outline(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Count == 0)
            return;

        var angles = _debuffs.GetAngles(slot);
        var unsafeAngles = _debuffs.GetUnsafeAngles(slot);
        var len = angles.Length;
        if (len == 0)
            return;

        var hasTether = _playerTethers.TryGetValue(actor, out var tethers);
        if (!hasTether)
            return;

        var source = tethers.Source;
        var inter = tethers.Intermediate;

        // if source is same direction as safe side, stand in that path; try standing close to avoid clipping other players
        var sourceSafe = true;
        for (var i = 0; i < len; i++)
        {
            if (source.Rotation.AlmostEqual(unsafeAngles[i], 0.02f))
            {
                sourceSafe = false;
                break;
            }
        }

        if (sourceSafe)
        {
            hints.AddForbiddenZone(new AOEShapeRect(40f, 2f, invertForbiddenZone: true), source.Position, source.Rotation);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(source, 2f));
            return;
        }

        // add AOE from 1st bait as forbidden only if side is unsafe
        //base.AddAIHints(slot, actor, assignment, hints);

        // determine if source is N/S or E/W, need to move perpendicular
        // source sits directly on edge (20f from center), anywhere along that edge
        hints.AddForbiddenZone(new AOEShapeRect(40f, 2f), source.Position, source.Rotation);
        var isNorthSouth = source.Position.Z is -795f or -835f;
        var interAngle = _debuffs.GetUnyieldingAngle(slot, isNorthSouth);
        if (interAngle != null)
        {
            hints.AddForbiddenZone(new AOEShapeRect(40f, 2f), inter.Position, interAngle.Value);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(V14LoneSwordmasterStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.LoneSwordmaster,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14323u,
SortOrder = 5,
PlanLevel = 0)]
public class V14LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -815f), new ArenaBoundsSquare(20f));
