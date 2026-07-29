namespace BossMod.Dawntrail.Savage.M12SLindwurm;

// Mortal Slayer hints using the 'Roles' solution
// Green orbs (Lindwurm6/0x4B01) and purple TB orbs (Lindwurm5/0x4B00) spawn in waves
// Purple side (TH): 2 green + 2 purple orbs; Healers bait green, Tanks bait purple in H1/H2; MT/OT order
// Green side (DPS): 4 green orbs; DPS bait all in M1/M2/R1/R2 order
sealed class MortalSlayer(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(6f);
    private static readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly List<AOEInstance> _aoes = [];
    private record struct OrbInfo(Actor Orb, bool IsPurple, bool IsLeft, int WaveIndex, DateTime SpawnTime, bool Baited);
    private readonly List<OrbInfo> _allOrbs = [];
    private bool? _purpleSideIsLeft;
    private const double WaveGroupingThresholdMs = 200.0; // I dunno, this worked
    private DateTime _lastOrbSpawnTime;
    private int _currentWaveIndex;

    // baits per group for role order display
    private int _dpsGreenBaited;
    private int _healerGreenBaited;
    private int _tankPurpleBaited;

    private DateTime _mechanicActivation;

    private static readonly PartyRolesConfig.Assignment[] DPSOrder = [
        PartyRolesConfig.Assignment.M1,
        PartyRolesConfig.Assignment.M2,
        PartyRolesConfig.Assignment.R1,
        PartyRolesConfig.Assignment.R2
    ];

    private static readonly PartyRolesConfig.Assignment[] HealerOrder = [
        PartyRolesConfig.Assignment.H1,
        PartyRolesConfig.Assignment.H2
    ];

    private static readonly PartyRolesConfig.Assignment[] TankOrder = [
        PartyRolesConfig.Assignment.MT,
        PartyRolesConfig.Assignment.OT
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // MortalSlayer1/2 are the instant damage abilities cast by orbs when baited
        if (spell.Action.ID is (uint)AID.MortalSlayer1 or (uint)AID.MortalSlayer2)
        {
            for (var i = 0; i < _allOrbs.Count; i++)
            {
                var orb = _allOrbs[i];
                if (orb.Orb.InstanceID == caster.InstanceID && !orb.Baited)
                {
                    _allOrbs[i] = orb with { Baited = true };

                    if (orb.IsPurple)
                    {
                        _tankPurpleBaited++;
                    }
                    else if (IsOnPurpleSide(orb))
                    {
                        _healerGreenBaited++;
                    }
                    else
                    {
                        _dpsGreenBaited++;
                    }
                    break;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Lindwurm5 or (uint)OID.Lindwurm6)
        {
            var bossPos = Module.PrimaryActor.Position;
            var isLeft = actor.Position.X < bossPos.X;
            var isPurple = actor.OID == (uint)OID.Lindwurm5;

            // determine purple side when first purple orb spawns
            if (isPurple && _purpleSideIsLeft == null)
                _purpleSideIsLeft = isLeft;

            // orbs within WaveGroupingThresholdMs are in the same wave
            var now = WorldState.CurrentTime;
            if (_lastOrbSpawnTime != default && (now - _lastOrbSpawnTime).TotalMilliseconds > WaveGroupingThresholdMs)
                _currentWaveIndex++;
            _lastOrbSpawnTime = now;

            _allOrbs.Add(new OrbInfo(actor, isPurple, isLeft, _currentWaveIndex, now, false));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MortalSlayer0)
        {
            _allOrbs.Clear();
            _aoes.Clear();
            _purpleSideIsLeft = null;
            _lastOrbSpawnTime = default;
            _currentWaveIndex = 0;
            _dpsGreenBaited = 0;
            _healerGreenBaited = 0;
            _tankPurpleBaited = 0;
            _mechanicActivation = Module.CastFinishAt(spell);
        }
    }

    public override void Update()
    {
        _aoes.Clear();

        if (_mechanicActivation == default)
            return;

        // build list of active non-destroyed, non-baited orbs for prediction
        var activeOrbs = new List<Actor>();
        foreach (var orb in _allOrbs)
        {
            if (IsOrbActive(orb))
                activeOrbs.Add(orb.Orb);
        }

        // reset when orbs have spawned and all are now baited/destroyed
        if (activeOrbs.Count == 0 && _allOrbs.Count > 0)
        {
            _mechanicActivation = default;
            return;
        }

        // find closest player to each orb
        var party = Raid.WithoutSlot(false, true, true);
        foreach (var orb in activeOrbs)
        {
            var closest = party.Closest(orb.Position);
            if (closest != null)
            {
                _aoes.Add(new(_shape, closest.Position, default, _mechanicActivation));
            }
        }
    }

    private static bool IsOrbActive(OrbInfo orb) => !orb.Orb.IsDestroyed && !orb.Baited;

    private int GetCurrentWaveIndex()
    {
        var minWave = int.MaxValue;
        foreach (var orb in _allOrbs)
            if (IsOrbActive(orb) && orb.WaveIndex < minWave)
                minWave = orb.WaveIndex;
        return minWave == int.MaxValue ? -1 : minWave;
    }

    private bool IsOnPurpleSide(OrbInfo orb) => _purpleSideIsLeft != null && orb.IsLeft == _purpleSideIsLeft.Value;

    private (bool isDPS, bool isHealer, bool isTank) GetRoleInfo(int slot, Actor actor)
    {
        var assignment = _partyConfig[Raid.Members[slot].ContentId];
        var isDPS = assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2
                                or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2;
        var isHealer = assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2;
        var isTank = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT;

        if (!isDPS && !isHealer && !isTank)
        {
            isDPS = actor.Role is Role.Melee or Role.Ranged;
            isHealer = actor.Role == Role.Healer;
            isTank = actor.Role == Role.Tank;
        }
        return (isDPS, isHealer, isTank);
    }

    private bool IsBaitDangerous(Actor player, OrbInfo orb, int playerSlot, List<OrbInfo> waveOrbs)
    {
        // danger if player already has the debuff
        if (player.FindStatus((uint)SID.PoisonResistanceDownII) != null)
            return true;

        var (isDPS, isHealer, isTank) = GetRoleInfo(playerSlot, player);

        // if TH side has 1 purple + 1 green, assume correct targeting
        if (IsOnPurpleSide(orb))
        {
            var purpleCount = 0;
            var greenCount = 0;
            foreach (var o in waveOrbs)
            {
                if (IsOnPurpleSide(o))
                {
                    if (o.IsPurple) purpleCount++;
                    else greenCount++;
                }
            }
            if (purpleCount == 1 && greenCount == 1)
            {
                // assume tank gets purple, healer gets green regardless of distance
                return false;
            }
        }

        // DPS should only bait green on DPS side
        if (isDPS && (orb.IsPurple || IsOnPurpleSide(orb)))
            return true;

        // healer should only bait green on purple side
        if (isHealer && orb.IsPurple)
            return true;

        // tank should only bait purple
        if (isTank && !orb.IsPurple)
            return true;

        return false;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_mechanicActivation == default)
            return;

        var currentWave = GetCurrentWaveIndex();
        if (currentWave < 0)
            return;

        var party = Raid.WithoutSlot(false, true, true).ToList();
        if (party.Count == 0)
            return;

        // collect active orbs in current wave
        var waveOrbs = new List<OrbInfo>();
        foreach (var orb in _allOrbs)
            if (IsOrbActive(orb) && orb.WaveIndex == currentWave)
                waveOrbs.Add(orb);

        // for each orb, find the closest player (who will bait it)
        // then draw circle with appropriate color
        foreach (var orb in waveOrbs)
        {
            var closest = party.Closest(orb.Orb.Position);
            if (closest == null)
                continue;

            var isSelf = closest.InstanceID == pc.InstanceID;
            var playerSlot = Raid.FindSlot(closest.InstanceID);
            var isDangerous = playerSlot >= 0 && IsBaitDangerous(closest, orb, playerSlot, waveOrbs);

            // self: safe unless dangerous, others: always danger
            var color = isSelf ? (isDangerous ? Colors.Danger : Colors.Safe) : Colors.Danger;
            Arena.ZoneCircleOutline(closest.Position, _shape.Radius, color);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_purpleSideIsLeft == null)
            return;

        var currentWave = GetCurrentWaveIndex();
        if (currentWave < 0)
            return;

        var (isDPS, isHealer, isTank) = GetRoleInfo(slot, actor);

        // check if player is on correct side
        var bossPos = Module.PrimaryActor.Position;
        var playerIsLeft = actor.Position.X < bossPos.X;
        var playerOnPurpleSide = playerIsLeft == _purpleSideIsLeft.Value;
        var shouldBeOnPurpleSide = isHealer || isTank;

        if (playerOnPurpleSide != shouldBeOnPurpleSide)
        {
            var targetIsLeft = shouldBeOnPurpleSide ? _purpleSideIsLeft.Value : !_purpleSideIsLeft.Value;
            hints.Add($"Go {(targetIsLeft ? "left" : "right")}!", true);
            return;
        }

        // show bait order
        if (isDPS && _dpsGreenBaited < DPSOrder.Length)
            hints.Add($"Order: {BuildRemainingOrder(DPSOrder, _dpsGreenBaited)}", false);
        else if (isHealer && _healerGreenBaited < HealerOrder.Length)
            hints.Add($"Order: {BuildRemainingOrder(HealerOrder, _healerGreenBaited)}", false);
        else if (isTank && _tankPurpleBaited < TankOrder.Length)
            hints.Add($"Order: {BuildRemainingOrder(TankOrder, _tankPurpleBaited)}", false);
    }

    private static string BuildRemainingOrder(PartyRolesConfig.Assignment[] order, int baited)
    {
        var parts = new List<string>();
        for (var i = baited; i < order.Length; i++)
            parts.Add(order[i].ToString());
        return string.Join(" > ", parts);
    }
}
