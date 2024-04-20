namespace BossMod.Endwalker.Unreal.Un1Ultima;

// common mechanics that are used for entire fight
// TODO: consider splitting into multiple components, at least for mechanics that start in later phases...
class Mechanics(BossModule module) : BossComponent(module)
{
    private readonly int[] _tankStacks = new int[PartyState.MaxPartySize];

    private readonly HashSet<ulong> _orbsSharedExploded = [];
    // TODO: think how to associate kiters with orbs
    private readonly HashSet<ulong> _orbsKitedExploded = [];
    private readonly List<ulong> _orbKiters = [];

    private Angle? _magitekOffset;

    private static readonly AOEShapeCircle _aoeCleave = new(2);
    private static readonly AOEShapeCone _aoeDiffractive = new(12, 60.Degrees());
    private static readonly AOEShapeRect _aoeAssaultCannon = new(45, 1);
    private static readonly AOEShapeRect _aoeMagitekRay = new(40, 3);
    //private static readonly float _homingLasersRange = 4;
    //private static readonly float _ceruleumVentRange = 8;
    private const float _orbSharedRange = 8;
    private const float _orbFixateRange = 6;

    public override void Update()
    {
        // TODO: this is bad, we need to find a way to associate orb to kiter...
        if (_orbKiters.Count > 0 && Module.Enemies(OID.Aetheroplasm).Count == 0)
            _orbKiters.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var mtSlot = WorldState.Party.FindSlot(Module.PrimaryActor.TargetID);
        if (actor.Role == Role.Tank)
        {
            if (Module.PrimaryActor.TargetID == actor.InstanceID)
            {
                if (_tankStacks[slot] >= 4)
                    hints.Add("Pass aggro to co-tank!");
            }
            else
            {
                if (mtSlot >= 0 && _tankStacks[mtSlot] >= 4)
                    hints.Add("Taunt boss!");
            }
        }

        var mt = WorldState.Party[mtSlot];
        if (slot != mtSlot && mt != null && (_aoeCleave.Check(actor.Position, mt) || _aoeDiffractive.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(mt.Position - Module.PrimaryActor.Position))))
        {
            hints.Add("GTFO from tank!");
        }

        // TODO: reconsider whether we really care about spread for vents/lasers...
        //if (actor.Role is Role.Healer or Role.Ranged && GeometryUtils.InCircle(actor.Position - Module.PrimaryActor.Position, _ceruleumVentRange))
        //{
        //    hints.Add("Move from boss");
        //}

        //if (Raid.WithoutSlot().InRadiusExcluding(actor, _homingLasersRange).Any())
        //{
        //    hints.Add("Spread");
        //}

        if (_magitekOffset != null && _aoeMagitekRay.Check(actor.Position, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + _magitekOffset.Value))
        {
            hints.Add("GTFO from ray aoe!");
        }

        if (_orbKiters.Contains(actor.InstanceID))
        {
            hints.Add("Kite the orb!");
        }

        if (Module.Enemies(OID.MagitekBit).Any(bit => bit.CastInfo != null && _aoeAssaultCannon.Check(actor.Position, bit)))
        {
            hints.Add("GTFO from bit aoe!");
        }

        // TODO: large detonations
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_magitekOffset != null)
            _aoeMagitekRay.Draw(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + _magitekOffset.Value);

        foreach (var bit in Module.Enemies(OID.MagitekBit).Where(bit => bit.CastInfo != null))
            _aoeAssaultCannon.Draw(Arena, bit);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var mt = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        foreach (var player in Raid.WithoutSlot().Exclude(pc))
            Arena.Actor(player, _orbKiters.Contains(player.InstanceID) ? ArenaColor.Danger : player == mt ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        if (mt != null)
            Arena.AddCircle(mt.Position, _aoeCleave.Radius, ArenaColor.Danger);

        //if (pc.Role is Role.Healer or Role.Ranged)
        //    Arena.AddCircle(Module.PrimaryActor.Position, _ceruleumVentRange, ArenaColor.Danger);

        foreach (var orb in Module.Enemies(OID.Ultimaplasm).Where(orb => !_orbsSharedExploded.Contains(orb.InstanceID)))
        {
            // TODO: line between paired orbs
            Arena.Actor(orb, ArenaColor.Danger, true);
            Arena.AddCircle(orb.Position, _orbSharedRange, ArenaColor.Safe);
        }

        foreach (var orb in Module.Enemies(OID.Aetheroplasm).Where(orb => !_orbsKitedExploded.Contains(orb.InstanceID)))
        {
            // TODO: line from corresponding target
            Arena.Actor(orb, ArenaColor.Danger, true);
            Arena.AddCircle(orb.Position, _orbFixateRange, ArenaColor.Danger);
        }

        foreach (var bit in Module.Enemies(OID.MagitekBit))
        {
            Arena.Actor(bit, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            SetTankStacks(actor, status.Extra);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            SetTankStacks(actor, 0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? ray = (AID)spell.Action.ID switch
        {
            AID.MagitekRayCenter => 0.Degrees(),
            AID.MagitekRayLeft => 45.Degrees(),
            AID.MagitekRayRight => -45.Degrees(),
            _ => null
        };
        if (ray == null)
            return;
        if (_magitekOffset != null)
            ReportError("Several concurrent magitek rays");
        _magitekOffset = ray.Value;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MagitekRayCenter or AID.MagitekRayLeft or AID.MagitekRayRight)
            _magitekOffset = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AetheroplasmBoom:
                _orbsSharedExploded.Add(caster.InstanceID);
                break;
            case AID.AetheroplasmFixated:
                _orbsKitedExploded.Add(caster.InstanceID);
                break;
            case AID.OrbFixate:
                _orbKiters.Add(spell.MainTargetID);
                break;
        }
    }

    private void SetTankStacks(Actor actor, int stacks)
    {
        int slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _tankStacks[slot] = stacks;
    }
}
