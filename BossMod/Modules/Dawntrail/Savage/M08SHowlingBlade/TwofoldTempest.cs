namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class TwofoldTempestVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(3)];
    private readonly AOEShapeCircle circle = new(9f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (_aoes.Count != 0 && state == 0x004)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WindVoidzone)
        {
            _aoes.Add(new(circle, actor.Position.Quantized()));
        }
    }
}

sealed class TwofoldTempestTetherAOE(BossModule module) : Components.InterceptTetherAOE(module, (uint)AID.TwofoldTempestCircle, (uint)TetherID.TwofoldTempest, 6f)
{
    private BitMask vulnerability;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }
        if (Tethers[0].Player == actor && vulnerability[slot])
        {
            hints.Add("Pass tether!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }
        if (Tethers[0].Player == actor)
        {
            if (!vulnerability[slot])
            {
                hints.AddForbiddenZone(new SDCircle(new(100f, 100f), 23.5f), Activation);
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }
        var side = Tethers[0];

        Arena.AddLine(side.Enemy.Position, side.Player.Position);
        Arena.ZoneCircleOutline(side.Player.Position, Radius, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }
}

sealed class TwofoldTempestTetherVoidzone(BossModule module) : Components.InterceptTetherAOE(module, (uint)AID.TwofoldTempestCircle, (uint)TetherID.TwofoldTempest, 9f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints) { }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }
        var side = Tethers[0];
        if (side.Player != pc)
        {
            return;
        }
        Arena.ZoneCircleOutline(side.Player.Position, Radius);
    }
}

sealed class TwofoldTempestRect(BossModule module) : Components.GenericBaitStack(module, (uint)AID.TwofoldTempestRect, onlyShowOutlines: true)
{
    private readonly AOEShapeRect rect = new(40f, 8f);
    private DateTime _activation;
    private readonly M08SHowlingBlade bossmod = (M08SHowlingBlade)module;
    private BitMask vulnerability;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TwofoldTempestVisual1)
        {
            _activation = Module.CastFinishAt(spell, 0.1d);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_activation != default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            Actor? closest = null;
            var boss = bossmod.BossP2()!;
            var closestDistSq = float.MaxValue;

            for (var i = 0; i < len; ++i)
            {
                var player = party[i];
                var distSq = (player.Position - boss.Position).LengthSq();
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = player;
                }
            }

            if (closest != null)
            {
                CurrentBaits.Add(new(boss, closest, rect, _activation));
            }
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);

            if (baits.Length == 0)
                return;
            ref var b = ref baits[0];
            for (var i = 0; i < 5; ++i)
            {
                var center = ArenaChanges.EndArenaPlatforms[i].Center;
                if (b.Target.Position.InCircle(center, 8f))
                {
                    b.CustomRotation = ArenaChanges.PlatformAngles[i];
                    break;
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            vulnerability.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        if (len == 0)
        {
            return;
        }
        var playerPlatform = 0;
        var pos = actor.Position;
        for (var i = 0; i < 5; ++i)
        {
            if (pos.InCircle(ArenaChanges.EndArenaPlatforms[i].Center, 8f))
            {
                if (ArenaChanges.PlatformAngles[i] != baits[0].Rotation)
                {
                    return;
                }
                else if (vulnerability[slot])
                {
                    hints.Add("Avoid bait!");
                    return;
                }
                playerPlatform = i;
                break;
            }
        }

        ref var b = ref baits[0];
        var countP = 0;
        var party = Raid.WithoutSlot(false, true, true);
        var pLen = party.Length;

        for (var i = 0; i < pLen; ++i)
        {
            var p = party[i];
            if (p == actor)
            {
                continue;
            }
            if (p.Position.InCircle(ArenaChanges.EndArenaPlatforms[playerPlatform].Center, 8f))
            {
                if (++countP == 2)
                {
                    hints.Add("More than 2 players on your platform!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}
