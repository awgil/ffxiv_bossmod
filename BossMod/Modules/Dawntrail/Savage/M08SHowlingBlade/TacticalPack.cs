namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class Adds(BossModule module) : BossComponent(module)
{
    private BitMask windtether;
    private BitMask stonetether;
    public BitMask Windpack;
    private BitMask stonepack;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Windpack)
        {
            Windpack.Set(Raid.FindSlot(actor.InstanceID));
            windtether = default;
        }
        else if (status.ID == (uint)SID.Stonepack)
        {
            stonepack.Set(Raid.FindSlot(actor.InstanceID));
            stonetether = default;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Windpack)
        {
            Windpack.Clear(Raid.FindSlot(actor.InstanceID));
        }
        else if (status.ID == (uint)SID.Stonepack)
        {
            stonepack.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Windtether)
        {
            windtether.Set(Raid.FindSlot(source.InstanceID));
        }
        else if (tether.ID == (uint)TetherID.Stonetether)
        {
            stonetether.Set(Raid.FindSlot(source.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (stonepack != default)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                if (Windpack[slot])
                {
                    e.Priority = e.Actor.OID switch
                    {
                        (uint)OID.WolfOfStone2 => 1,
                        _ => AIHints.Enemy.PriorityInvincible
                    };
                }
                else if (stonepack[slot])
                {
                    e.Priority = e.Actor.OID switch
                    {
                        (uint)OID.WolfOfWind2 => 1,
                        _ => AIHints.Enemy.PriorityInvincible
                    };
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Windpack[pcSlot] ? Module.Enemies((uint)OID.WolfOfStone2) : Module.Enemies((uint)OID.WolfOfWind2));
        if (pc.Role == Role.Tank && stonepack != default)
        {
            Arena.ZoneCircleOutline(Windpack[pcSlot] ? Module.Enemies((uint)OID.FontOfWindAether)[0].Position : Module.Enemies((uint)OID.FontOfEarthAether)[0].Position, 1.5f, Colors.Vulnerable, 2f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (stonepack == default)
        {
            if (windtether == default)
                return;
            var tether = windtether[slot] ? "Earth" : "Wind";
            hints.Add($"Target: {tether} wolf", false);
        }
        else if (actor.Role == Role.Tank)
        {
            var badOrb = Windpack[slot] ? "wind" : "earth";
            var goodWolf = Windpack[slot] ? "earth" : "wind";
            hints.Add($"Don't let the {badOrb} orb catch the {goodWolf} wolf", false);
        }
    }
}

sealed class EarthWindborneEnd(BossModule module) : BossComponent(module)
{
    private readonly (int Order, Actor Actor, DateTime Expiration, bool wind)[] expirationBySlot = new (int, Actor, DateTime, bool)[8];
    private BitMask vulnerability;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref readonly var player = ref expirationBySlot[slot];
        if (player != default)
        {
            hints.Add($"Order: {player.Order} - {(player.wind ? "wind" : "earth")}", Math.Max(0d, (player.Expiration - WorldState.CurrentTime).TotalSeconds) < 9d);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        ref readonly var player = ref expirationBySlot[pcSlot];
        if (player != default)
        {
            var remaining = Math.Max(0d, (player.Expiration - WorldState.CurrentTime).TotalSeconds);
            Arena.ZoneCircleOutline(player.wind ? Module.Enemies((uint)OID.FontOfWindAether)[0].Position : Module.Enemies((uint)OID.FontOfEarthAether)[0].Position, 1.5f, remaining < 9d && vulnerability == default ? Colors.Safe : default, 2f);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.WindborneEnd or (uint)SID.EarthborneEnd)
        {
            var slot = WorldState.Party.FindSlot(actor.InstanceID);
            if (slot < 0)
                return;
            var order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
            {
                < 22d => 1,
                < 38d => 2,
                _ => 3
            };
            expirationBySlot[slot] = (order, actor, status.ExpireAt, status.ID == (uint)SID.WindborneEnd);
        }
        else if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.WindborneEnd or (uint)SID.EarthborneEnd)
        {
            var slot = WorldState.Party.FindSlot(actor.InstanceID);
            if (slot < 0)
                return;
            expirationBySlot[slot] = default;
        }
        else if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }
}

sealed class StalkingStoneWind(BossModule module) : Components.GenericBaitStack(module)
{
    private readonly AOEShapeRect rect = new(40f, 3f);
    private BitMask tanks;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind)
        {
            var act = WorldState.FutureTime(5.2d);

            if (tanks == default)
            {
                var party = Raid.WithSlot(true, true, true);
                var len = party.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref var p = ref party[i];
                    if (p.Item2.Role == Role.Tank)
                    {
                        tanks.Set(p.Item1);
                    }
                }
            }
            var stonewolves = Module.Enemies((uint)OID.WolfOfStone2);
            var windwolves = Module.Enemies((uint)OID.WolfOfWind2);
            var stonewolf = stonewolves.Count != 0 ? stonewolves[0] : null;
            var windwolf = windwolves.Count != 0 ? windwolves[0] : null;
            var earth = actor.FindStatus((uint)SID.Windpack) != null;
            var source = earth ? stonewolf : windwolf;
            if (source is Actor wolf)
            {
                CurrentBaits.Add(new(wolf, actor, rect, act, tanks));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.StalkingWind or (uint)AID.StalkingStone)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class AlphaWindStone(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly AOEShapeCone cone = new(40f, 45f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (CurrentBaits.Count == 0 && iconID == (uint)IconID.StalkingStoneWind)
        {
            var act = WorldState.FutureTime(5.2d);
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            var stonewolves = Module.Enemies((uint)OID.WolfOfStone2);
            var windwolves = Module.Enemies((uint)OID.WolfOfWind2);
            var stonewolf = stonewolves.Count != 0 ? stonewolves[0] : null;
            var windwolf = windwolves.Count != 0 ? windwolves[0] : null;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Role == Role.Tank)
                {
                    var earth = p.FindStatus((uint)SID.Windpack) != null;
                    var source = earth ? stonewolf : windwolf;
                    if (source is Actor wolf)
                    {
                        CurrentBaits.Add(new(wolf, p, cone, act));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlphaStone or (uint)AID.AlphaWind)
        {
            CurrentBaits.Clear();
        }
    }
}
