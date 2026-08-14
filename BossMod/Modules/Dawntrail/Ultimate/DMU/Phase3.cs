using Lumina.Extensions;

namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class AeroIIIAssault(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroIIIAssault, 15f);

sealed class TheDecisiveBattle(BossModule module) : BossComponent(module)
{
    private Actor? chaosBoss;
    private Actor? exDeathBoss;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (chaosBoss == null || exDeathBoss == null)
        {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(chaosBoss.Position).Take(4).ToList();
        foreach (var player in players)
        {
            Arena.AddLine(chaosBoss.Position, player.Position, Colors.Danger);
        }

        players = [.. Raid.WithoutSlot().SortedByRange(exDeathBoss.Position).Take(4)];
        foreach (var player in players)
        {
            Arena.AddLine(exDeathBoss.Position, player.Position, Colors.Danger);
        }

        foreach (var (_, player) in Raid.WithSlot(true, true, true))
        {
            Arena.Actor(player, player == pc ? Colors.PC : Colors.PlayerGeneric, true);
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
        {
            Arena.ZoneCircleOutline(chaosBoss.Position, 1.25f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2 or
            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
        {
            Arena.ZoneCircleOutline(exDeathBoss.Position, 1.25f, Colors.Safe, 2.0f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheDecisiveBattle)
        {
            chaosBoss = caster;
        }

        if (spell.Action.ID == (uint)AID.TheDecisiveBattle1)
        {
            exDeathBoss = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.TheDecisiveBattle) or ((uint)AID.TheDecisiveBattle1))
        {
            chaosBoss = null;
            exDeathBoss = null;
        }
    }
}

sealed class BowelsOfAgony(BossModule module) : Components.RaidwideCast(module, (uint)AID.BowelsOfAgony);

sealed class Crystals(BossModule module) : BossComponent(module)
{
    public List<(Actor actor, uint colour)> crystals = [];
    public List<(Actor actor, uint colour)> crystalsStored = []; // Used as a reference point for where the crystals were for easier hint logic
    public List<(Actor actor, ActorStatus debuff)> debuffPlayers = [];

    public Element nextElement = Element.None;
    public enum Element { None, Water, Fire, Wind }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DynamicFluid)
        {
            debuffPlayers.Add((actor, status));
        }

        if (status.ID == (uint)SID.Entropy)
        {
            debuffPlayers.Add((actor, status));
        }

        if (debuffPlayers.Count == 4)
        {
            debuffPlayers.Sort((a, b) => a.debuff.ExpireAt.CompareTo(b.debuff.ExpireAt));
            if (debuffPlayers[0].debuff.ID == (uint)SID.DynamicFluid)
            {
                nextElement = Element.Water;
            }
            else
            {
                nextElement = Element.Fire;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is ((uint)SID.DynamicFluid) or ((uint)SID.Entropy))
        {
            debuffPlayers.RemoveAll(d => d.actor == actor);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.FireP3)
        {
            crystals.Add((actor, Colors.Enemy));
        }

        if (actor.OID == (uint)OID.WaterP3)
        {
            crystals.Add((actor, Color.FromRGBA(0x268BD280).ABGR));
        }

        if (actor.OID == (uint)OID.WindP3)
        {
            crystals.Add((actor, Colors.Safe));
        }

        if (crystals.Count == 3)
        {
            crystalsStored = [.. crystals];
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.FireP3)
        {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.Water;
        }

        if (actor.OID == (uint)OID.WaterP3)
        {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.Fire;
        }

        if (actor.OID == (uint)OID.WindP3)
        {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.None;
        }

        if (crystals.Count == 1)
        {
            nextElement = Element.Wind;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (actor, colour) in crystals)
        {
            Arena.ZoneCircle(actor.Position, 1, colour);
        }
    }
}

sealed class ThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderIII, new AOEShapeCircle(15.0f));

sealed class WaterCrystal(BossModule module) : Components.GenericBaitProximity(module)
{
    private readonly Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Tsunami)
        {
            if (crystals == null)
            {
                return;
            }

            NumCasts++;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (crystals == null || crystals.crystals.Count == 0)
        {
            return;
        }

        if (crystals.nextElement != Crystals.Element.Water)
        {
            return;
        }

        var waterCrystal = crystals.crystals.FirstOrNull(c => c.actor.OID == (uint)OID.WaterP3);
        if (waterCrystal == null)
        {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(waterCrystal.Value.actor.Position).ToList();
        for (var i = 0; i < 2; i++)
        {
            CurrentBaits.Add(new(players[i], new AOEShapeCircle(5.0f)));
        }

        var debuffPlayers = crystals.debuffPlayers.Where(d => d.debuff.ID == (uint)SID.DynamicFluid).Take(2).ToList();
        if (debuffPlayers.Count < 2)
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            CurrentBaits.Add(new(debuffPlayers[i].actor, new AOEShapeDonut(4.0f, 10.0f)));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Element: {crystals?.nextElement}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (crystals == null || crystals.crystalsStored.Count != 3)
        {
            return;
        }

        var waterCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WaterP3);
        var fireCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.FireP3);
        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.H1)
        {
            Arena.ZoneCircleOutline(waterCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.H2)
        {
            Arena.ZoneCircleOutline(Module.Center + (waterCrystal.actor.Position - Module.Center).Normalized() * 3.5f, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
        {
            Arena.ZoneCircleOutline(fireCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(windCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }
    }
}

sealed class FireCrystal(BossModule module) : Components.GenericBaitProximity(module)
{
    private readonly Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Inferno)
        {
            if (crystals == null)
            {
                return;
            }

            NumCasts++;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (crystals == null || crystals.crystals.Count == 0)
        {
            return;
        }

        if (crystals.nextElement != Crystals.Element.Fire)
        {
            return;
        }

        var fireCrystal = crystals.crystals.FirstOrNull(c => c.actor.OID == (uint)OID.FireP3);
        if (fireCrystal == null)
        {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(fireCrystal.Value.actor.Position).ToList();
        for (var i = 0; i < 2; i++)
        {
            CurrentBaits.Add(new(players[i], new AOEShapeDonut(4.0f, 10.0f)));
        }

        var debuffPlayers = crystals.debuffPlayers.Where(d => d.debuff.ID == (uint)SID.Entropy).Take(2).ToList();
        if (debuffPlayers.Count < 2)
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            CurrentBaits.Add(new(debuffPlayers[i].actor, new AOEShapeCircle(5.0f)));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Element: {crystals?.nextElement}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (crystals == null || crystals.crystalsStored.Count != 3)
        {
            return;
        }

        var waterCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WaterP3);
        var fireCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.FireP3);
        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.H1)
        {
            Arena.ZoneCircleOutline(waterCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.H2)
        {
            Arena.ZoneCircleOutline(Module.Center + (waterCrystal.actor.Position - Module.Center).Normalized() * 3.5f, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
        {
            Arena.ZoneCircleOutline(fireCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
        {
            Arena.ZoneCircleOutline(windCrystal.actor.Position + (windCrystal.actor.Position - Module.Center).Normalized().OrthoL() * 2.5f, 1.0f, Colors.Safe, 2.0f);
        }

        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(windCrystal.actor.Position + (windCrystal.actor.Position - Module.Center).Normalized().OrthoR() * 2.5f, 1.0f, Colors.Safe, 2.0f);
        }
    }
}

sealed class LongitudinalLatitudinalImplosion(BossModule module) : Components.GenericAOEs(module, (uint)AID.Shockwave)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LongitudinalImplosion)
        {
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.LatitudinalImplosion)
        {
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            ++NumCasts;
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var nextAOEs = aoes.Take(2).ToList();
        return CollectionsMarshal.AsSpan(nextAOEs);
    }
}

sealed class ThunderIIITB(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ThunderIIITBCast, new AOEShapeCircle(5.0f), true)
{
    private Actor? boss = null;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThunderIIITBCast)
        {
            boss = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThunderIIITB)
        {
            NumCasts++;
            if (NumCasts == 2)
            {
                boss = null;
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (boss == null)
        {
            return;
        }

        var player = Raid.WithoutSlot().SortedByRange(boss.Position).FirstOrDefault();
        if (player == null)
        {
            return;
        }

        CurrentBaits.Add(new(boss, player, new AOEShapeCircle(5.0f)));
    }
}

sealed class UmbraSmash(BossModule module) : Components.GenericBaitProximity(module)
{
    private readonly Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private bool castStarted = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UmbraSmash)
        {
            castStarted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UmbraSmash)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }

    public override void Update()
    {
        if (crystals == null || crystals.crystals.Count != 1)
        {
            return;
        }

        if (castStarted)
        {
            return;
        }

        CurrentBaits.Clear();

        var chaosBoss = WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.Chaos);
        if (chaosBoss == null)
        {
            return;
        }

        var player = Raid.WithoutSlot().SortedByRange(chaosBoss.Position).LastOrDefault();
        if (player == null)
        {
            return;
        }

        CurrentBaits.Add(new(player, new AOEShapeCircle(15.0f)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (crystals == null || crystals.crystals.Count != 1)
        {
            return;
        }

        if (castStarted)
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.R1)
        {
            Arena.ZoneCircleOutline((Arena.Center - (windCrystal.actor.Position - Arena.Center).Normalized() * 20f) + new WDir(0, 1.0f), 1.0f, Colors.Safe, 2.0f);
        }
    }
}

sealed class UltimaBlaster(BossModule module) : Components.RaidwideInstant(module, (uint)AID.UltimaBlaster);

sealed class UltimaBlasterLimitCut(BossModule module) : Components.GenericBaitAway(module)
{
    private WPos startPosition;
    private Angle angleRotate;
    private readonly int[] orbNumbers = Utils.MakeArray(8, -1);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UltimaBlaster)
        {
            if (startPosition == default)
            {
                startPosition = caster.Position;
            }
            else if (angleRotate == default)
            {
                angleRotate = (startPosition - Arena.Center).OrthoL().Dot(caster.Position - Arena.Center) > 0 ? -45.Degrees() : 45.Degrees();
            }
        }

        if (spell.Action.ID == (uint)AID.UltimaBlasterBait)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var orbNumber = iconID switch
        {
            (uint)IconID.OrbNumber1 => 0,
            (uint)IconID.OrbNumber2 => 1,
            (uint)IconID.OrbNumber3 => 2,
            (uint)IconID.OrbNumber4 => 3,
            (uint)IconID.OrbNumber5 => 4,
            (uint)IconID.OrbNumber6 => 5,
            (uint)IconID.OrbNumber7 => 6,
            (uint)IconID.OrbNumber8 => 7,
            _ => -1
        };

        if (orbNumber >= 0)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                orbNumbers[slot] = orbNumber;

                if (startPosition == default || angleRotate == default)
                {
                    return;
                }

                var player = WorldState.Actors.Find(targetID);
                if (player == null)
                {
                    return;
                }

                CurrentBaits.Add(new(Arena.Center + (startPosition - Arena.Center).Rotate(angleRotate * orbNumber), player, new AOEShapeRect(100.0f, 3.0f), WorldState.FutureTime(12.1f + 0.2f * orbNumber)));
                CurrentBaits.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bait in ActiveBaitsOn(pc))
        {
            Arena.ZoneCircleOutline(Arena.Center + (Arena.Center - bait.Source.Position).Normalized().Rotate(angleRotate * 0.5f) * 19.0f, 0.75f, Colors.Safe);
        }
    }
}

sealed class HeadTailWind(BossModule module) : Components.GenericKnockback(module)
{
    public uint[] Direction = new uint[8];
    private (WPos Origin, DateTime Activation, bool EventHappened) wave;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            wave = new(caster.Position, DateTime.Now, false);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            NumCasts++;
            wave.EventHappened = true;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var direction = status.ID switch
        {
            (uint)SID.Headwind => (uint)SID.Headwind,
            (uint)SID.Tailwind => (uint)SID.Tailwind,
            _ => default
        };

        if (direction != default)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                Direction[slot] = direction;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Headwind or (uint)SID.Tailwind)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                Direction[slot] = 0;
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (wave.Origin != default)
        {
            return new[] { new Knockback(wave.Origin, KnockDistance(slot, actor, wave.Origin), wave.Activation) };
        }

        return [];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var direction = Direction[pcSlot];
        if (direction is not (uint)SID.Headwind and not (uint)SID.Tailwind)
        {
            return;
        }

        foreach (var knockback in ActiveKnockbacks(pcSlot, pc))
        {
            var toSource = (knockback.Origin - pc.Position).Normalized();
            var safeFacing = (Direction[pcSlot] == (uint)SID.Headwind ? -toSource : toSource).ToAngle();
            Arena.PathArcTo(pc.Position, 1, (safeFacing + 45f.Degrees()).Rad, (safeFacing - 45f.Degrees()).Rad);
            MiniArena.PathStroke(false, Colors.Safe);
            Arena.PathArcTo(pc.Position, 1, (safeFacing + 225f.Degrees()).Rad, (safeFacing + 135f.Degrees()).Rad);
            MiniArena.PathStroke(false, Colors.Danger);
        }
    }

    public override void Update()
    {
        if (wave.EventHappened)
        {
            foreach (var player in Raid.WithoutSlot())
            {
                if (player.LastFrameMovement.Length() / WorldState.Frame.Duration > 15)
                {
                    wave = default;
                    break;
                }
            }
        }
    }

    float KnockDistance(int pcSlot, Actor pc, WPos source)
    {
        var direction = Direction[pcSlot];
        if (direction is not (uint)SID.Headwind and not (uint)SID.Tailwind)
        {
            return 20f;
        }

        var toSource = (source - pc.Position).Normalized();
        var safeFacing = direction == (uint)SID.Headwind ? -toSource : toSource;
        var rel = safeFacing.Normalized().Dot(pc.Rotation.ToDirection());

        if (rel > 0.7071067f)
        {
            return 10f;
        }

        if (rel < -0.7071068f)
        {
            return 40f;
        }

        return 20f;
    }
}

sealed class Cyclone(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly HeadTailWind? windDebuffs = module.FindComponent<HeadTailWind>();
    public int NumCasts = 0;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Cyclone)
        {
            NumCasts++;
        }
    }

    public override void Update()
    {
        Stacks.Clear();

        if (windDebuffs == null)
        {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot(true, true, true).WhereSlot(p => windDebuffs.Direction[p] is (uint)SID.Headwind or (uint)SID.Tailwind))
        {
            Stacks.Add(new(player, 6.0f));
        }
    }
}

sealed class KefkaMax(BossModule module) : BossComponent(module)
{
    public Actor? boss = null;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.KefkaMax)
        {
            boss = actor;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (boss == null)
        {
            return;
        }

        // Taken from actor source file, but with the option of adding a scale
        var actorPos = boss.Position - boss.Rotation.ToDirection() * 20.0f;
        var scale = 2.5f;
        var dir = boss.Rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale035 = actorPos - scale035;
        Arena.AddTriangleFilled(actorPos + scale07, positionscale035 + scale0433, positionscale035 - scale0433, Colors.Object);
    }
}

sealed class SlapHappy(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    // Big Hands AOEs are 10y apart
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SlapHappyRightHand)
        {
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoR() * 10.0f + (spell.Rotation.ToDirection().OrthoR() * 10.0f).OrthoR()));
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoR() * 10.0f));
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoR() * 10.0f + (spell.Rotation.ToDirection().OrthoR() * 10.0f).OrthoL()));
            aoes.Add(new(new AOEShapeCircle(6.0f), Arena.Center));
        }

        if (spell.Action.ID == (uint)AID.SlapHappyLeftHand)
        {
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoL() * 10.0f + (spell.Rotation.ToDirection().OrthoL() * 10.0f).OrthoL()));
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoL() * 10.0f));
            aoes.Add(new(new AOEShapeCircle(13.0f), Arena.Center + spell.Rotation.ToDirection().OrthoL() * 10.0f + (spell.Rotation.ToDirection().OrthoL() * 10.0f).OrthoR()));
            aoes.Add(new(new AOEShapeCircle(6.0f), Arena.Center));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.SlapHappyBigAOE) or ((uint)AID.SlapHappySmallAOE))
        {
            NumCasts++;
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class SlapHappyBaits(BossModule module) : Components.GenericBaitStack(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SlapHappyLeftHand)
        {
            var party = Raid.WithSlot(true, true, true);
            BitMask allowedTanks = default;
            BitMask allowedHealers = default;
            BitMask allowedDDs = default;

            for (var i = 0; i < party.Length; i++)
            {
                ref var p = ref party[i];

                if (p.Item2.Role == Role.Tank)
                {
                    allowedTanks.Set(p.Item1);
                }

                if (p.Item2.Role == Role.Healer)
                {
                    allowedHealers.Set(p.Item1);
                }

                if (p.Item2.Role is Role.Melee or Role.Ranged)
                {
                    allowedDDs.Set(p.Item1);
                }
            }

            var addedTank = false;
            var addedHealer = false;
            var addedDD = false;

            for (var i = 0; i < party.Length; i++)
            {
                ref var player = ref party[i];
                var p = player.Item2;

                if (p.IsDead)
                {
                    continue;
                }

                if (p.Role == Role.Tank)
                {
                    if (!addedTank)
                    {
                        CurrentBaits.Add(new(caster, p, new AOEShapeCone(100, 22.5f.Degrees()), forbidden: ~allowedTanks));
                        addedTank = true;
                    }
                }

                if (p.Role == Role.Healer)
                {
                    if (!addedHealer)
                    {
                        CurrentBaits.Add(new(caster, p, new AOEShapeCone(100, 22.5f.Degrees()), forbidden: ~allowedHealers));
                        addedHealer = true;
                    }
                }

                if (p.Role is Role.Melee or Role.Ranged)
                {
                    if (!addedDD)
                    {
                        CurrentBaits.Add(new(caster, p, new AOEShapeCone(100, 22.5f.Degrees()), forbidden: ~allowedDDs));
                        addedDD = true;
                    }
                }
            }
        }

        if (spell.Action.ID == (uint)AID.SlapHappyRightHand)
        {
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target == null)
            {
                return;
            }

            CurrentBaits.Add(new(caster, target, new AOEShapeCone(100, 30.0f.Degrees())));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.SlapHappyShockingImpactStack) or ((uint)AID.SlapHappyShockwaveRole))
        {
            if (CurrentBaits.Count > 0)
            {
                NumCasts++;
                CurrentBaits.RemoveAt(0);
            }
        }
    }
}

sealed class DamningEdict(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DamningEdict, new AOEShapeRect(60.0f, 40.0f));

sealed class LookUponMeAndDespairAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LookUponMeAndDespairAOE, new AOEShapeRect(100.0f, 8.0f));

sealed class WhiteHole(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhiteHole);

sealed class EarthquakeRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.EarthquakeRaidwide);

sealed class BlackHoleActors(BossModule module) : Components.Voidzone(module, 2.0f, enemies => enemies.Enemies((uint)OID.BlackHole));

sealed class BlackHole(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(125.0f, 3.0f), (uint)TetherID.BlackHoleTether)
{
    private readonly List<(Actor blackHole, ulong target)> Tethers = [];
    private readonly KefkaMax? kefkaMax = module.FindComponent<KefkaMax>();

    private enum Roles { NONE, DPS, SUPPORT, ACCRETION }
    private readonly (Roles role, int order)[] orderedRoles = Utils.MakeArray(8, (Roles.NONE, 0));
    private (Roles role, int order)[] currentSet = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlackHole)
        {
            CurrentSetSolver();
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if (tether.ID == (uint)TetherID.BlackHoleTether)
        {
            Tethers.Add((source, tether.Target));
            SortTethersCW();
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnUntethered(source, tether);

        if (tether.ID == (uint)TetherID.BlackHoleTether)
        {
            Tethers.RemoveAll(t => t.blackHole.InstanceID == source.InstanceID);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var order = status.ID switch
        {
            (uint)SID.FirstInLine => 1,
            (uint)SID.SecondInLine => 2,
            (uint)SID.ThirdInLine => 3,
            _ => 0
        };

        if (order != 0)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                orderedRoles[slot].order = order;
                if (orderedRoles[slot].role == Roles.NONE)
                {
                    orderedRoles[slot].role = actor.Class.IsSupport() ? Roles.SUPPORT : Roles.DPS;
                }
            }
        }

        if (status.ID == (uint)SID.Accretion)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                orderedRoles[slot].role = Roles.ACCRETION;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Nothingness)
        {
            NumCasts++;
            CurrentSetSolver();
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var baits = ActiveBaitsOn(pc);
        foreach (var bait in baits)
        {
            var currentBait = bait;
            if (IsClippedBy(player, ref currentBait))
            {
                customColor = Colors.Danger;
                return PlayerPriority.Danger;
            }
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    private void CurrentSetSolver()
    {
        currentSet = (NumCasts) switch
        {
            0 => [new(Roles.DPS, 1)], // Set 1-1
            1 => [new(Roles.DPS, 1), new(Roles.SUPPORT, 1)], // Set 1-2
            3 => [new(Roles.DPS, 1), new(Roles.SUPPORT, 1), new(Roles.ACCRETION, 1)], // Set 2-1
            6 => [new(Roles.DPS, 2), new(Roles.SUPPORT, 1), new(Roles.ACCRETION, 1)], // Set 2-2
            9 => [new(Roles.DPS, 2), new(Roles.SUPPORT, 2), new(Roles.ACCRETION, 1)], // Set 2-3
            12 => [new(Roles.DPS, 2), new(Roles.SUPPORT, 2), new(Roles.ACCRETION, 2)], // Set 3-1
            15 => [new(Roles.DPS, 3), new(Roles.SUPPORT, 2), new(Roles.ACCRETION, 2)], // Set 3-2
            18 => [new(Roles.DPS, 3), new(Roles.SUPPORT, 3), new(Roles.ACCRETION, 2)], // Set 3-3
            21 => [new(Roles.DPS, 3), new(Roles.SUPPORT, 3)], // Set 4-1
            23 => [new(Roles.SUPPORT, 3)], // Set 4-2
            _ => []
        };

    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        for (var i = 0; i < Tethers.Count; i++)
        {
            var (blackHoleActor, targetID) = Tethers[i];
            var target = WorldState.Actors.Find(targetID);
            if (target == null)
            {
                continue;
            }

            var assignedToMe = i < currentSet.Length && orderedRoles[pcSlot].role == currentSet[i].role && orderedRoles[pcSlot].order == currentSet[i].order;
            Arena.AddLine(blackHoleActor.Position, target.Position, assignedToMe ? Colors.Safe : Colors.Danger, 3.0f);
        }
    }

    // TODO move into data actor at some point called CWWith
    private void SortTethersCW()
    {
        if (kefkaMax == null || kefkaMax.boss == null)
        {
            return;
        }

        var startingPos = kefkaMax.boss.Position - kefkaMax.boss.Rotation.ToDirection() * 20.0f;
        var startingAngle = (startingPos - Module.Center).ToAngle().Rad + 5 * MathF.PI / 180;

        var list = new List<((Actor BlackHoleActor, ulong PlayerID) item, float angle)>();
        foreach (var tether in Tethers)
        {
            var thisAngle = (tether.blackHole.Position - Module.Center).ToAngle().Rad;
            if (thisAngle > startingAngle)
            {
                thisAngle -= Angle.DoublePI;
            }
            list.Add((tether, thisAngle));
        }
        list.Sort(static (a, b) => b.angle.CompareTo(a.angle));
        Tethers.Clear();
        Tethers.AddRange(list.Select(x => x.item));
    }
}

sealed class P3BlizzardBaits(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlizzardIIIBaitCast, new AOEShapeCircle(6.0f))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlizzardIIIBaitCast)
        {
            NumCasts++;
        }
    }
}

sealed class P3Blizzard(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, onlyShowOutlines: true)
{
    private Actor? boss = null;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlizzardIIICast)
        {
            boss = caster;
        }

        if (spell.Action.ID == (uint)AID.BlizzardIIIBaitCast)
        {
            NumCasts++;

            if (NumCasts == 16)
            {
                boss = null;
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (boss == null)
        {
            return;
        }

        foreach (var player in Raid.WithoutSlot())
        {
            CurrentBaits.Add(new(boss, player, new AOEShapeCircle(6.0f)));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (NumCasts >= 16)
        { // TODO remove this when adding hints array
            return;
        }

        var kefkaBoss = ((DMU)Module).BossP3();
        if (kefkaBoss == null)
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (pc.Class.IsDD())
        {
            if (NumCasts < 8)
            {
                Arena.ZoneCircleOutline(kefkaBoss.Position + 10.0f * kefkaBoss.Rotation.ToDirection(), 1.0f, Colors.Safe);
            }

            if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1)
            {
                if (NumCasts < 8)
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position + 10.0f * kefkaBoss.Rotation.ToDirection()) - 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Danger);
                }
                else
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position + 10.0f * kefkaBoss.Rotation.ToDirection()) - 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Safe);
                }
            }

            if (assignment is PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2)
            {
                if (NumCasts < 8)
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position + 10.0f * kefkaBoss.Rotation.ToDirection()) + 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Danger);
                }
                else
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position + 10.0f * kefkaBoss.Rotation.ToDirection()) + 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Safe);
                }
            }
        }

        if (pc.Class.IsSupport())
        {
            if (NumCasts < 8)
            {
                Arena.ZoneCircleOutline(kefkaBoss.Position - 10.0f * kefkaBoss.Rotation.ToDirection(), 1.0f, Colors.Safe);
            }

            if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1)
            {
                if (NumCasts < 8)
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position - 10.0f * kefkaBoss.Rotation.ToDirection()) - 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Danger);
                }
                else
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position - 10.0f * kefkaBoss.Rotation.ToDirection()) - 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Safe);
                }
            }

            if (assignment is PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2)
            {
                if (NumCasts < 8)
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position - 10.0f * kefkaBoss.Rotation.ToDirection()) + 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Danger);
                }
                else
                {
                    Arena.ZoneCircleOutline((kefkaBoss.Position - 10.0f * kefkaBoss.Rotation.ToDirection()) + 8.0f * kefkaBoss.Rotation.ToDirection().OrthoL(), 1.0f, Colors.Safe);
                }
            }
        }
    }
}

sealed class P3BlizzardMove(BossModule module) : Components.StayMove(module, 5d)
{
    public int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlizzardIIIRaidwide)
        {
            foreach (var (slot, _) in Raid.WithSlot(true, true, true))
            {
                PlayerStates[slot] = new(Requirement.Move, WorldState.FutureTime(spell.RemainingTime));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlizzardIIIRaidwide)
        {
            Array.Clear(PlayerStates);
            ++NumCasts;
        }
    }
}

sealed class KnockDown(BossModule module) : Components.GenericStackSpread(module)
{
    public List<WPos> stackLocations = [];
    public Class stackClass = Class.None;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StackShare)
        {
            var target = WorldState.Actors.Find(targetID);
            if (target == null)
            {
                return;
            }

            stackClass = target.Class;

            BitMask allowedPlayers = default;
            if (target.Class.IsSupport())
            {
                foreach (var (slot, player) in Raid.WithSlot(true, true, true))
                {
                    if (player.Class.IsSupport())
                    {
                        allowedPlayers.Set(slot);
                    }
                }
            }

            if (target.Class.IsDD())
            {
                foreach (var (slot, player) in Raid.WithSlot(true, true, true))
                {
                    if (player.Class.IsDD())
                    {
                        allowedPlayers.Set(slot);
                    }
                }
            }

            Stacks.Add(new(target, 6.0f, 4, 4, forbiddenPlayers: ~allowedPlayers));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.KnockDown)
        {
            if (Stacks.Count > 0)
            {
                stackLocations.Add(Stacks[0].Target.Position);
                Stacks.RemoveAt(0);
            }
        }
    }
}

sealed class BigBang(BossModule module) : Components.GenericAOEs(module)
{
    private readonly KnockDown? stacks = module.FindComponent<KnockDown>();
    private readonly List<AOEInstance> aoes = [];
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BigBangCast)
        {
            active = true;
        }
    }

    public override void Update()
    {
        aoes.Clear();

        if (!active)
        {
            return;
        }

        if (stacks != null && stacks.stackLocations.Count > 0)
        {
            foreach (var stack in stacks.stackLocations)
            {
                aoes.Add(new(new AOEShapeCircle(6.0f), stack));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BigBang)
        {
            NumCasts++;
            if (stacks != null)
            {
                if (stacks.stackLocations.Count > 0)
                {
                    stacks.stackLocations.RemoveAt(0);
                }
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class StompAMole(BossModule module) : Components.GenericTowers(module)
{
    private Actor? boss = null;
    private readonly KnockDown? stacks = module.FindComponent<KnockDown>();
    private enum TowerSide { LEFT, RIGHT }
    private readonly List<(Tower tower, TowerSide side, int wave)> towers = [];
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    private IEnumerable<(Tower tower, TowerSide side, int wave)> CurrentTowers => towers.Where(t => t.wave == (NumCasts < 2 ? 0 : 1));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StompAMoleCast)
        {
            boss = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StompAMoleTower)
        {
            var wave = NumCasts < 2 ? 0 : 1;
            towers.RemoveAll(t => t.wave == wave && t.tower.Position.AlmostEqual(caster.Position, 1.0f));
            NumCasts++;
        }
    }

    public override void Update()
    {
        if (boss == null)
        {
            return;
        }

        if (stacks == null || stacks.stackClass == Class.None)
        {
            return;
        }

        BitMask towerSoakers = default;

        if (stacks.stackClass.IsSupport())
        {
            foreach (var (slot, player) in Raid.WithSlot(true, true, true))
            {
                if (player.Class.IsDD())
                {
                    towerSoakers.Set(slot);
                }
            }
        }

        if (stacks.stackClass.IsDD())
        {
            foreach (var (slot, player) in Raid.WithSlot(true, true, true))
            {
                if (player.Class.IsSupport())
                {
                    towerSoakers.Set(slot);
                }
            }
        }

        towers.Add(new(new Tower(boss.Position + boss.Rotation.ToDirection().OrthoL() * 10.0f, 5.0f, 2, 2, forbiddenSoakers: ~towerSoakers), TowerSide.RIGHT, 0));
        towers.Add(new(new Tower(boss.Position + boss.Rotation.ToDirection().OrthoR() * 10.0f, 5.0f, 2, 2, forbiddenSoakers: ~towerSoakers), TowerSide.LEFT, 0));
        towers.Add(new(new Tower(boss.Position + boss.Rotation.ToDirection().OrthoL() * 10.0f, 5.0f, 2, 2, forbiddenSoakers: towerSoakers), TowerSide.RIGHT, 1));
        towers.Add(new(new Tower(boss.Position + boss.Rotation.ToDirection().OrthoR() * 10.0f, 5.0f, 2, 2, forbiddenSoakers: towerSoakers), TowerSide.LEFT, 1));
        boss = null; // Prevents towers constantly getting added
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (tower, _, _) in CurrentTowers)
        {
            if (tower.ForbiddenSoakers[pcSlot] || !tower.IsInside(pc) && tower.NumInside(Module) >= tower.MaxSoakers)
            {
                tower.Shape.Draw(Arena, tower.Position, tower.Rotation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        TowerSide? assignmentTower = null;
        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length > 0)
        {
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];
            if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1)
            {
                assignmentTower = TowerSide.LEFT;
            }
            else
            {
                assignmentTower = TowerSide.RIGHT;
            }
        }

        foreach (var (tower, side, _) in CurrentTowers)
        {
            if (tower.ForbiddenSoakers[pcSlot])
            {
                continue;
            }

            if (slots.Length > 0 && assignmentTower != side)
            {
                if (tower.NumInside(Module) < tower.MaxSoakers)
                {
                    tower.Shape.Outline(Arena, tower.Position, tower.Rotation, Colors.Danger, 2f);
                }
                continue;
            }

            var isInside = tower.IsInside(pc);
            var numInside = tower.NumInside(Module);
            var safe = numInside < tower.MaxSoakers || isInside && numInside <= tower.MaxSoakers;

            if (safe)
            {
                tower.Shape.Outline(Arena, tower.Position, tower.Rotation, Colors.Safe, 2f);
            }
            else if (isInside && numInside > tower.MaxSoakers)
            {
                tower.Shape.Outline(Arena, tower.Position, tower.Rotation, default, 2f);
            }
        }
    }
}

sealed class P3Enrage(BossModule module) : BossComponent(module)
{
    public bool enrage;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.BowelsOfAgonyEnrage) or ((uint)AID.MeteorEnrage))
        {
            enrage = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.BowelsOfAgonyEnrage) or ((uint)AID.MeteorEnrage))
        {
            enrage = false;
        }
    }
}
