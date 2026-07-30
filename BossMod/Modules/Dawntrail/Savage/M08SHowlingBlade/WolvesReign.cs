namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class WolvesReignConeCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private readonly AOEShapeCircle circle = new(14f);
    private AOEInstance[] _aoe = [];
    private WPos jumpLoc;
    private bool isCone;

    private static readonly M08SHowlingBladeConfig _config = Service.Config.Get<M08SHowlingBladeConfig>();
    private static readonly PartyRolesConfig _party = Service.Config.Get<PartyRolesConfig>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        AOEShape? shape = id switch
        {
            (uint)AID.WolvesReignTeleport1 => cone,
            (uint)AID.WolvesReignTeleport2 => circle,
            _ => null
        };
        if (shape != null)
        {
            var pos = spell.LocXZ;
            _aoe = [new(shape, pos, Angle.FromDirection(Arena.Center - pos), Module.CastFinishAt(spell, 3.6d), Colors.Danger)];
        }
        else if (id is (uint)AID.EminentReign or (uint)AID.RevolutionaryReign)
        {
            var pos = spell.LocXZ;
            jumpLoc = pos + 17.5f * (Arena.Center - pos).Normalized();
            isCone = spell.Action.ID == (uint)AID.EminentReign;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WolvesReignCone:
            case (uint)AID.WolvesReignCircleBig:
                ++NumCasts;
                break;
        }
    }

    // based on xan's work since dhogGPT really wanted to have this
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safespots = SafeSpots(pcSlot, pc);
        var count = safespots.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(safespots[i], 0.7f, Colors.Safe, 2f);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var safespots = SafeSpots(slot, actor);
        var count = safespots.Count;
        for (var i = 0; i < count; ++i)
        {
            movementHints.Add(actor.Position, safespots[i], Colors.Safe);
        }
    }

    private List<WPos> SafeSpots(int slot, Actor actor)
    {
        if (jumpLoc == default && _config.ReignHints == M08SHowlingBladeConfig.ReignStrategy.Disabled)
            return [];

        var assignment = _party[WorldState.Party.Members[slot].ContentId];
        var lp = GetLightparty(assignment, _config.ReignHints);

        var isTank = actor.Role == Role.Tank || assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT;
        var bossDir = (Arena.Center - jumpLoc).Normalized();

        var safespots = new List<WPos>(lp == 0 ? 2 : 1);

        void AddSafeSpot(float angleDeg, float distance) => safespots.Add(jumpLoc + bossDir.Rotate(angleDeg.Degrees()) * distance);

        if (isCone)
        {
            if (lp != 2)
                AddSafeSpot(isTank ? -145f : -75f, isTank ? 4f : 7f);
            if (lp != 1)
                AddSafeSpot(isTank ? 145f : 75f, isTank ? 4f : 7f);
        }
        else
        {
            if (lp != 2)
                AddSafeSpot(isTank ? -53f : -12f, isTank ? 14.4f : 18f);
            if (lp != 1)
                AddSafeSpot(isTank ? 53f : 12f, isTank ? 14.4f : 18f);
        }

        return safespots;

        static int GetLightparty(PartyRolesConfig.Assignment assignment, M08SHowlingBladeConfig.ReignStrategy strategy)
        {
            if (strategy == M08SHowlingBladeConfig.ReignStrategy.Any)
                return 0;

            var lp = assignment switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.H1 => 1,
                PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H2 => 2,
                _ => 0
            };

            if (strategy == M08SHowlingBladeConfig.ReignStrategy.Inverse)
                lp = lp == 1 ? 2 : 1;

            return lp;
        }
    }
}
sealed class WolvesReignRect(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(28f, 5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RevolutionaryReign or (uint)AID.EminentReign)
        {
            var rot = spell.Rotation;
            _aoe = [new(rect, (caster.Position - 4f * rot.ToDirection()).Quantized(), rot, Module.CastFinishAt(spell, 2.4d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignRect1 or (uint)AID.WolvesReignRect2)
        {
            ++NumCasts;
        }
    }
}

sealed class WolvesReignCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WolvesReignCircle1, (uint)AID.WolvesReignCircle2,
(uint)AID.WolvesReignCircle3, (uint)AID.EminentReign, (uint)AID.RevolutionaryReign], 6f);

sealed class SovereignScar(BossModule module) : Components.GenericBaitStack(module)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport1 or (uint)AID.WolvesReignTeleport2)
        {
            var act = Module.CastFinishAt(spell, 3.6d);
            var party = Raid.WithSlot(true, true, true);
            var source = spell.LocXZ;
            var len = party.Length;
            BitMask forbidden = default;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Item2.Role == Role.Tank)
                {
                    forbidden.Set(p.Item1);
                }
            }

            for (var i = 0; i < len; ++i)
            {
                ref readonly var player = ref party[i];
                var p = player.Item2;
                if (p.Role == Role.Healer)
                {
                    CurrentBaits.Add(new(source, p, cone, act, forbidden: forbidden));
                }
            }
        }
    }
}

sealed class ReignsEnd(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport1 or (uint)AID.WolvesReignTeleport2)
        {
            var act = Module.CastFinishAt(spell, 3.6d);
            var party = Raid.WithoutSlot(true, true, true);
            var source = spell.LocXZ;
            var len = party.Length;

            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Role == Role.Tank)
                {
                    CurrentBaits.Add(new(source, p, cone, act));
                }
            }
        }
    }
}

sealed class RoaringWind(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = [with(4)];
    public bool Draw;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D2 && actor.OID == (uint)OID.WolfOfWind4)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(5.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RoaringWind)
        {
            ++NumCasts;
        }
    }
}

sealed class WealOfStone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private static readonly AOEShapeRect rect = new(40f, 3f);
    public bool Draw;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D2 && actor.OID == (uint)OID.WolfOfStone3)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(5.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WealOfStone1 or (uint)AID.WealOfStone2)
        {
            ++NumCasts;
        }
    }
}
