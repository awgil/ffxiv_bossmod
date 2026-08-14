using Dalamud.Interface;

namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class IdyllicDreamArena(BossModule module) : Components.GenericAOEs(module, warningText: "Go to correct plattform!")
{
    private readonly Polygon[] splitArena = [new(new(86f, 100f), 10f, 60), new(new(114f, 100f), 10f, 60)];
    private AOEInstance[] _aoe = [];

    public int State;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x21)
            return;

        switch (state)
        {
            case 0x00800040u:
            case 0x02000040u:
                _aoe = [];
                Arena.Bounds = new ArenaBoundsCustom(splitArena);
                State = 1;
                break;
            case 0x01000001u:
                Arena.Bounds = M12S2TheLindwurm.BuildArena().arena;
                State = 0;
                break;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public void Predict(double seconds)
    {
        var center = Arena.Center;
        var shape = new AOEShapeCustom(center, [new Square(center, 20f)], splitArena);
        _aoe = [new(shape, center, default, WorldState.FutureTime(seconds), shapeDistance: shape.Distance(center, default))];
    }
}

// Shared state for Idyllic Dream to persist tower assignments across component lifecycles
sealed class IdyllicDreamSharedState : BossComponent
{
    public readonly int[] TowerAssignments = new int[PartyState.MaxPartySize];
    public readonly Element[] TowerElements = new Element[PartyState.MaxPartySize];
    public readonly WPos[] TowerPositions = new WPos[8]; // Store positions of all 8 towers

    public IdyllicDreamSharedState(BossModule module) : base(module)
    {
        Array.Fill(TowerAssignments, -1);
    }

    public void RecordTowerSoak(int slot, int towerIndex, Element element, WPos position)
    {
        if (slot >= 0 && slot < TowerAssignments.Length)
        {
            TowerAssignments[slot] = towerIndex;
            TowerElements[slot] = element;
        }
        if (towerIndex >= 0 && towerIndex < TowerPositions.Length)
        {
            TowerPositions[towerIndex] = position;
        }
    }

    public (int towerIndex, Element element, WPos position) GetTowerSoak(int slot)
    {
        if (slot >= 0 && slot < TowerAssignments.Length)
        {
            var towerIndex = TowerAssignments[slot];
            var element = TowerElements[slot];
            var position = towerIndex >= 0 && towerIndex < TowerPositions.Length
                ? TowerPositions[towerIndex]
                : default;
            return (towerIndex, element, position);
        }
        return (-1, Element.None, default);
    }
}

sealed class ArcadianArcanum(BossModule module) : Components.UniformStackSpread(module, 0, 6)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArcadianArcanumCast)
        {
            foreach (var p in Raid.WithoutSlot())
                AddSpread(p, Module.CastFinishAt(spell, 1.4d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArcadianArcanum)
        {
            ++NumCasts;
            Spreads.Clear();
        }
    }
}

sealed class IdyllicDreamElementalMeteor(BossModule module) : Components.GenericTowers(module)
{
    internal readonly struct Meteor(Actor actor, Element element)
    {
        internal readonly Actor Actor = actor;
        internal readonly Element Element = element;
    }
    private IdyllicDreamSharedState? sharedState;
    internal readonly List<Meteor> Meteors = [];

    BitMask _lightVuln;
    public bool DrawIcons;

    private readonly int[] _assignedTowers = Utils.MakeArray(PartyState.MaxPartySize, -1);

    private readonly Tower[] _modifiedTowers = new Tower[8];

    public const uint ColorWind = 0xFF9ABB81;
    public const uint ColorDark = 0xFFE67AD2;
    public const uint ColorEarth = 0xFF81A1AD;
    public const uint ColorFire = 0xFF2A2DD5;

    // --------------------------------------------------------
    // Meteor Detection
    // --------------------------------------------------------

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002u)
            return;

        var element = actor.OID switch
        {
            (uint)OID.MeteorWind => Element.Wind,
            (uint)OID.MeteorDark => Element.Dark,
            (uint)OID.MeteorEarth => Element.Earth,
            (uint)OID.MeteorFire => Element.Fire,
            _ => default
        };

        if (element != default)
            Meteors.Add(new(actor, element));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LightResistanceDownII)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _lightVuln.Set(slot);
        }
    }

    public override void Update()
    {
        if (DrawIcons && Towers.Count > 0)
        {
            sharedState ??= Module.FindComponent<IdyllicDreamSharedState>();
            if (sharedState != null)
            {
                foreach (var (slot, actor) in Raid.WithSlot(true))
                {
                    var assignedTower = _assignedTowers[slot];
                    if (assignedTower >= 0 && assignedTower < Towers.Count && assignedTower < Meteors.Count)
                    {
                        var tower = Towers[assignedTower];
                        if (tower.IsInside(actor))
                        {
                            var meteor = Meteors[assignedTower];
                            sharedState.RecordTowerSoak(slot, assignedTower, meteor.Element, meteor.Actor.Position);
                        }
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.CosmicKiss)
            return;

        ++NumCasts;
        Towers.Clear();
        Meteors.Clear();
        _lightVuln = default;
        DrawIcons = false;
        Array.Fill(_assignedTowers, -1);
    }

    // --------------------------------------------------------
    // Tower Creation
    // --------------------------------------------------------

    public void CreateTowers()
    {
        DrawIcons = true;

        var activation = WorldState.FutureTime(8.4d);
        var meteors = CollectionsMarshal.AsSpan(Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];

            var forbidden = m.Element is Element.Wind or Element.Dark
                ? _lightVuln
                : ~_lightVuln;

            Towers.Add(new(m.Actor.Position, 3, forbiddenSoakers: forbidden, activation: activation));
        }

        AssignTowers();
    }

    private void AssignTowers()
    {
        var roleAssignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
        if (roleAssignments.Length == 0)
        {
            Module.ReportError(this, "Tower assignments require party roles to be configured");
            return;
        }

        var config = Service.Config.Get<M12S2LindwurmConfig>();

        var mtSlot = roleAssignments[(int)PartyRolesConfig.Assignment.MT];
        var otSlot = roleAssignments[(int)PartyRolesConfig.Assignment.OT];
        var h1Slot = roleAssignments[(int)PartyRolesConfig.Assignment.H1];
        var h2Slot = roleAssignments[(int)PartyRolesConfig.Assignment.H2];
        var m1Slot = roleAssignments[(int)PartyRolesConfig.Assignment.M1];
        var m2Slot = roleAssignments[(int)PartyRolesConfig.Assignment.M2];
        var r1Slot = roleAssignments[(int)PartyRolesConfig.Assignment.R1];
        var r2Slot = roleAssignments[(int)PartyRolesConfig.Assignment.R2];

        var actualWestSlots = new List<int>();
        var actualEastSlots = new List<int>();

        if (config.IdyllicDreamAdjustToMistakes)
        {
            var slots = new[] { mtSlot, otSlot, h1Slot, h2Slot, m1Slot, m2Slot, r1Slot, r2Slot };
            for (var i = 0; i < slots.Length; ++i)
            {
                var slot = slots[i];
                if (slot < 0)
                    continue;

                var player = Raid[slot];
                if (player == null)
                    continue;

                var pos = player.Position;
                if (pos.InCircle(new WPos(86f, 100f), 10f))
                    actualWestSlots.Add(slot);
                else if (pos.InCircle(new WPos(114f, 100f), 10f))
                    actualEastSlots.Add(slot);
            }

            // Only adjust if we have exactly 4 players on each platform
            if (actualWestSlots.Count == 4 && actualEastSlots.Count == 4)
            {
                AssignTowersByActualPositions(actualWestSlots, actualEastSlots,
                    mtSlot, h1Slot, m1Slot, r1Slot,
                    otSlot, h2Slot, m2Slot, r2Slot);
                return;
            }
        }

        // Standard assignment: West = MT/H1/M1/R1, East = OT/H2/M2/R2
        // Categorize meteors by position
        // West/Left platform: x < 100 (centered at 86, 100)
        // East/Right platform: x > 100 (centered at 114, 100)
        var westTowers = new List<(int index, Meteor meteor, bool isNorth)>();
        var eastTowers = new List<(int index, Meteor meteor, bool isNorth)>();

        var meteors = CollectionsMarshal.AsSpan(Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];
            var pos = m.Actor.PosRot;
            var isWest = pos.X < 100f;
            var isNorth = pos.Z < 100f;

            if (isWest)
                westTowers.Add((i, m, isNorth));
            else
                eastTowers.Add((i, m, isNorth));
        }

        // Sort towers: closer to boss (closer to center X) first, then by element type
        westTowers.Sort(static (a, b) =>
        {
            var distA = Math.Abs(a.meteor.Actor.PosRot.X - 100);
            var distB = Math.Abs(a.meteor.Actor.PosRot.X - 100);
            return distA.CompareTo(distB);
        });

        eastTowers.Sort(static (a, b) =>
        {
            var distA = Math.Abs(a.meteor.Actor.PosRot.X - 100);
            var distB = Math.Abs(a.meteor.Actor.PosRot.X - 100);
            return distA.CompareTo(distB);
        });

        if (westTowers.Count != 4 || eastTowers.Count != 4)
        {
            Module.ReportError(this, $"Unexpected tower distribution: West={westTowers.Count}, East={eastTowers.Count}");
            return;
        }

        // West platform (Light Party 2): MT, H1, M1, R1
        // Initial assignment: MT & H1 (supports) take north towers, M1 & R1 take south towers
        // Closer towers: MT & M1, Farther towers: H1 & R1
        AssignPlatformTowers(westTowers,
            mtSlot, h1Slot, m1Slot, r1Slot,
            true); // West platform: supports north

        // East platform (Light Party 1): OT, H2, M2, R2
        // Initial assignment: M2 & R2 take north towers, OT & H2 take south towers
        // Closer towers: OT & M2, Farther towers: H2 & R2
        AssignPlatformTowers(eastTowers,
            otSlot, h2Slot, m2Slot, r2Slot,
            false); // East platform: DPS north
    }

    private void AssignTowersByActualPositions(
        List<int> westSlots, List<int> eastSlots,
        int mtSlot, int h1Slot, int m1Slot, int r1Slot,
        int otSlot, int h2Slot, int m2Slot, int r2Slot)
    {
        var westRoles = DetermineRoleComposition(westSlots, mtSlot, h1Slot, m1Slot, r1Slot, otSlot, h2Slot, m2Slot, r2Slot);
        var eastRoles = DetermineRoleComposition(eastSlots, mtSlot, h1Slot, m1Slot, r1Slot, otSlot, h2Slot, m2Slot, r2Slot);

        // Validate role composition: must have exactly 2 supports and 2 DPS per platform
        if (!westRoles.Valid || !eastRoles.Valid)
        {
            // Invalid composition detected - mechanic will fail regardless, don't adjust logic
            Module.ReportError(this, "Invalid platform composition: mechanic requires 2 supports (tank/healer) and 2 DPS (melee/ranged) per platform");
            return;
        }

        // Categorize meteors by position
        var westTowers = new List<(int index, Meteor meteor, bool isNorth)>();
        var eastTowers = new List<(int index, Meteor meteor, bool isNorth)>();

        var meteors = CollectionsMarshal.AsSpan(Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];
            var pos = m.Actor.PosRot;
            var isWest = pos.X < 100f;
            var isNorth = pos.Z < 100f;

            if (isWest)
                westTowers.Add((i, m, isNorth));
            else
                eastTowers.Add((i, m, isNorth));
        }

        // Sort towers by distance to center
        westTowers.Sort(static (a, b) =>
        {
            var distA = Math.Abs(a.meteor.Actor.PosRot.X - 100f);
            var distB = Math.Abs(b.meteor.Actor.PosRot.X - 100f);
            return distA.CompareTo(distB);
        });

        eastTowers.Sort(static (a, b) =>
        {
            var distA = Math.Abs(a.meteor.Actor.PosRot.X - 100f);
            var distB = Math.Abs(b.meteor.Actor.PosRot.X - 100f);
            return distA.CompareTo(distB);
        });

        if (westTowers.Count != 4 || eastTowers.Count != 4)
        {
            Module.ReportError(this, $"Unexpected tower distribution: West={westTowers.Count}, East={eastTowers.Count}");
            return;
        }

        // Assign west platform
        AssignPlatformTowersFlexible(westTowers, westSlots, westRoles);

        // Assign east platform
        AssignPlatformTowersFlexible(eastTowers, eastSlots, eastRoles);
    }

    private PlatformRoles DetermineRoleComposition(
        List<int> platformSlots,
        int mtSlot, int h1Slot, int m1Slot, int r1Slot,
        int otSlot, int h2Slot, int m2Slot, int r2Slot)
    {
        var roles = new PlatformRoles();

        for (var i = 0; i < platformSlots.Count; ++i)
        {
            var slot = platformSlots[i];

            // Categorize as support (tank/healer) or DPS (melee/ranged)
            if (slot == mtSlot || slot == otSlot || slot == h1Slot || slot == h2Slot)
            {
                // Support role
                roles.SupportCount++;
                if (roles.SupportSlots[0] < 0)
                    roles.SupportSlots[0] = slot;
                else if (roles.SupportSlots[1] < 0)
                    roles.SupportSlots[1] = slot;
                else
                    roles.Valid = false; // More than 2 supports
            }
            else if (slot == m1Slot || slot == m2Slot || slot == r1Slot || slot == r2Slot)
            {
                // DPS role
                roles.DPSCount++;
                if (roles.DPSSlots[0] < 0)
                    roles.DPSSlots[0] = slot;
                else if (roles.DPSSlots[1] < 0)
                    roles.DPSSlots[1] = slot;
                else
                    roles.Valid = false; // More than 2 DPS
            }
        }

        // Validate we have exactly 2 supports and 2 DPS
        if (roles.SupportCount != 2 || roles.DPSCount != 2)
            roles.Valid = false;

        return roles;
    }

    private struct PlatformRoles
    {
        public int SupportCount;
        public int DPSCount;
        public int[] SupportSlots; // [0] and [1]
        public int[] DPSSlots; // [0] and [1]
        public bool Valid;

        public PlatformRoles()
        {
            SupportCount = 0;
            DPSCount = 0;
            SupportSlots = [-1, -1];
            DPSSlots = [-1, -1];
            Valid = true;
        }
    }

    private void AssignPlatformTowersFlexible(
        List<(int index, Meteor meteor, bool isNorth)> towers,
        List<int> platformSlots,
        PlatformRoles roles)
    {
        if (towers.Count != 4 || platformSlots.Count != 4)
        {
            Module.ReportError(this, $"AssignPlatformTowersFlexible: tower/slot mismatch {towers.Count}/{platformSlots.Count}");
            return;
        }

        // Get the 2 support and 2 DPS players
        var support1 = Raid[roles.SupportSlots[0]];
        var support2 = Raid[roles.SupportSlots[1]];
        var dps1 = Raid[roles.DPSSlots[0]];
        var dps2 = Raid[roles.DPSSlots[1]];

        if (support1 == null || support2 == null || dps1 == null || dps2 == null)
        {
            Module.ReportError(this, "Cannot find all players for flexible tower assignment");
            return;
        }

        // Determine which players are close (<10 yalms from center X=100) vs far
        var support1Close = Math.Abs(support1.PosRot.X - 100f) < 10f;
        var support2Close = Math.Abs(support2.PosRot.X - 100f) < 10f;
        var dps1Close = Math.Abs(dps1.PosRot.X - 100f) < 10f;
        var dps2Close = Math.Abs(dps2.PosRot.X - 100f) < 10f;

        // Count how many of each category are close
        var supportsCloseCount = (support1Close ? 1 : 0) + (support2Close ? 1 : 0);
        var dpsCloseCount = (dps1Close ? 1 : 0) + (dps2Close ? 1 : 0);

        // Separate towers into close and far
        var closeTowers = new List<(int index, Meteor meteor, bool isNorth)>();
        var farTowers = new List<(int index, Meteor meteor, bool isNorth)>();

        for (var i = 0; i < towers.Count; ++i)
        {
            var tower = towers[i];
            if (Math.Abs(tower.meteor.Actor.PosRot.X - 100f) < 10f)
                closeTowers.Add(tower);
            else
                farTowers.Add(tower);
        }

        if (closeTowers.Count != 2 || farTowers.Count != 2)
        {
            closeTowers.Clear();
            farTowers.Clear();
            for (var i = 0; i < 2 && i < towers.Count; ++i)
                closeTowers.Add(towers[i]);
            for (var i = 2; i < towers.Count; ++i)
                farTowers.Add(towers[i]);
        }

        // Find north/south in each group
        var closeNorthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var closeSouthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var farNorthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var farSouthList = new List<(int index, Meteor meteor, bool isNorth)>();

        for (var i = 0; i < closeTowers.Count; ++i)
        {
            var tower = closeTowers[i];
            if (tower.isNorth)
                closeNorthList.Add(tower);
            else
                closeSouthList.Add(tower);
        }

        for (var i = 0; i < farTowers.Count; ++i)
        {
            var tower = farTowers[i];
            if (tower.isNorth)
                farNorthList.Add(tower);
            else
                farSouthList.Add(tower);
        }

        if (closeNorthList.Count == 0 || closeSouthList.Count == 0 ||
            farNorthList.Count == 0 || farSouthList.Count == 0)
        {
            Module.ReportError(this, "Platform tower layout invalid");
            return;
        }

        var closeNorth = closeNorthList[0];
        var closeSouth = closeSouthList[0];
        var farNorth = farNorthList[0];
        var farSouth = farSouthList[0];

        // Determine assignment based on actual positions
        // Standard strategy: supports north on west, DPS north on east
        // But we adapt based on where people actually are

        var closePair = new int[2];
        var farPair = new int[2];

        // Assign close/far based on actual positions (prioritize what players are doing)
        if (supportsCloseCount == 2 && dpsCloseCount == 0)
        {
            // Both supports close, both DPS far
            closePair[0] = roles.SupportSlots[0];
            closePair[1] = roles.SupportSlots[1];
            farPair[0] = roles.DPSSlots[0];
            farPair[1] = roles.DPSSlots[1];
        }
        else if (supportsCloseCount == 0 && dpsCloseCount == 2)
        {
            // Both DPS close, both supports far
            closePair[0] = roles.DPSSlots[0];
            closePair[1] = roles.DPSSlots[1];
            farPair[0] = roles.SupportSlots[0];
            farPair[1] = roles.SupportSlots[1];
        }
        else if (supportsCloseCount == 1 && dpsCloseCount == 1)
        {
            // Mixed: 1 support + 1 DPS close, 1 support + 1 DPS far
            if (support1Close)
            {
                closePair[0] = roles.SupportSlots[0];
                farPair[0] = roles.SupportSlots[1];
            }
            else
            {
                closePair[0] = roles.SupportSlots[1];
                farPair[0] = roles.SupportSlots[0];
            }

            if (dps1Close)
            {
                closePair[1] = roles.DPSSlots[0];
                farPair[1] = roles.DPSSlots[1];
            }
            else
            {
                closePair[1] = roles.DPSSlots[1];
                farPair[1] = roles.DPSSlots[0];
            }
        }
        else
        {
            // Fallback: assume standard strategy (supports north on west)
            closePair[0] = roles.SupportSlots[0];
            closePair[1] = roles.DPSSlots[0];
            farPair[0] = roles.SupportSlots[1];
            farPair[1] = roles.DPSSlots[1];
        }

        // Assign north/south towers with swap logic
        // Use simple heuristic: supports prefer north on west platform, DPS prefer north on east
        _assignedTowers[closePair[0]] = ShouldSwap(closePair[0], closeNorth.index, closeSouth.index) ? closeSouth.index : closeNorth.index;
        _assignedTowers[closePair[1]] = ShouldSwap(closePair[1], closeSouth.index, closeNorth.index) ? closeNorth.index : closeSouth.index;
        _assignedTowers[farPair[0]] = ShouldSwap(farPair[0], farNorth.index, farSouth.index) ? farSouth.index : farNorth.index;
        _assignedTowers[farPair[1]] = ShouldSwap(farPair[1], farSouth.index, farNorth.index) ? farNorth.index : farSouth.index;
    }

    private void AssignPlatformTowers(
        List<(int index, Meteor meteor, bool isNorth)> towers,
        int tankSlot, int healerSlot, int meleeSlot, int rangedSlot,
        bool supportsNorth)
    {
        if (towers.Count != 4)
        {
            Module.ReportError(this, $"AssignPlatformTowers expected 4 towers, got {towers.Count}");
            return;
        }

        // Separate into close (tank/melee) and far (healer/ranged) towers
        var closeTowers = new List<(int index, Meteor meteor, bool isNorth)>();
        var farTowers = new List<(int index, Meteor meteor, bool isNorth)>();

        for (var i = 0; i < towers.Count; ++i)
        {
            var tower = towers[i];
            if (Math.Abs(tower.meteor.Actor.PosRot.X - 100f) < 10f)
                closeTowers.Add(tower);
            else
                farTowers.Add(tower);
        }

        if (closeTowers.Count != 2 || farTowers.Count != 2)
        {
            // Fallback: first 2 are close, last 2 are far
            closeTowers.Clear();
            farTowers.Clear();
            for (var i = 0; i < 2 && i < towers.Count; ++i)
                closeTowers.Add(towers[i]);
            for (var i = 2; i < towers.Count; ++i)
                farTowers.Add(towers[i]);
        }

        // Find north and south towers in each group
        var closeNorthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var closeSouthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var farNorthList = new List<(int index, Meteor meteor, bool isNorth)>();
        var farSouthList = new List<(int index, Meteor meteor, bool isNorth)>();

        for (var i = 0; i < closeTowers.Count; ++i)
        {
            var tower = closeTowers[i];
            if (tower.isNorth)
                closeNorthList.Add(tower);
            else
                closeSouthList.Add(tower);
        }

        for (var i = 0; i < farTowers.Count; ++i)
        {
            var tower = farTowers[i];
            if (tower.isNorth)
                farNorthList.Add(tower);
            else
                farSouthList.Add(tower);
        }

        if (closeNorthList.Count == 0 || closeSouthList.Count == 0 ||
            farNorthList.Count == 0 || farSouthList.Count == 0)
        {
            Module.ReportError(this, $"Platform tower layout invalid");
            return;
        }

        var closeNorth = closeNorthList[0];
        var closeSouth = closeSouthList[0];
        var farNorth = farNorthList[0];
        var farSouth = farSouthList[0];

        // Assign based on roles and strategy
        int tankInitial, meleeInitial, healerInitial, rangedInitial;

        if (supportsNorth)
        {
            tankInitial = closeNorth.index;
            meleeInitial = closeSouth.index;
            healerInitial = farNorth.index;
            rangedInitial = farSouth.index;
        }
        else
        {
            tankInitial = closeSouth.index;
            meleeInitial = closeNorth.index;
            healerInitial = farSouth.index;
            rangedInitial = farNorth.index;
        }

        // Apply assignments with swap logic
        _assignedTowers[tankSlot] = ShouldSwap(tankSlot, tankInitial, meleeInitial) ? meleeInitial : tankInitial;
        _assignedTowers[meleeSlot] = ShouldSwap(meleeSlot, meleeInitial, tankInitial) ? tankInitial : meleeInitial;
        _assignedTowers[healerSlot] = ShouldSwap(healerSlot, healerInitial, rangedInitial) ? rangedInitial : healerInitial;
        _assignedTowers[rangedSlot] = ShouldSwap(rangedSlot, rangedInitial, healerInitial) ? healerInitial : rangedInitial;
    }

    private bool ShouldSwap(int slot, int initialTower, int partnerTower)
    {
        if (initialTower < 0 || initialTower >= Towers.Count)
            return false;

        var tower = Towers[initialTower];

        // Check if player is forbidden from their initial tower
        if (tower.ForbiddenSoakers[slot])
        {
            // Check if partner tower is available
            if (partnerTower >= 0 && partnerTower < Towers.Count)
            {
                var partnerTowerData = Towers[partnerTower];
                return !partnerTowerData.ForbiddenSoakers[slot];
            }
        }

        return false;
    }

    // --------------------------------------------------------
    // Drawing
    // --------------------------------------------------------

    void DrawElement(Element el, WPos p)
    {
        var (icon, color) = el switch
        {
            Element.Wind => (FontAwesomeIcon.Tornado, ColorWind),
            Element.Dark => (FontAwesomeIcon.StarOfLife, ColorDark),
            Element.Earth => (FontAwesomeIcon.Gem, ColorEarth),
            Element.Fire => (FontAwesomeIcon.Fire, ColorFire),
            _ => default
        };

        Arena.ZoneCircle(p, 1.5f, Colors.Background);
        Arena.IconWorld(p, icon, color);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (!DrawIcons)
            return;

        var meteors = CollectionsMarshal.AsSpan(Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];
            var pos = m.Actor.Position;
            var center = pos.X > 100f
                ? new WPos(114f, 100f)
                : new WPos(86f, 100f);

            DrawElement(m.Element, pos + (pos - center));
        }

        // Highlight assigned tower
        var assignedTower = _assignedTowers[pcSlot];
        if (assignedTower >= 0 && assignedTower < Towers.Count)
        {
            var tower = Towers[assignedTower];
            if (tower.Shape is AOEShapeCircle circle)
                Arena.ZoneCircleOutline(tower.Position, circle.Radius, Colors.Safe, 2f);
        }
    }

    // --------------------------------------------------------
    // Tower Restrictions
    // --------------------------------------------------------

    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        if (!DrawIcons || Towers.Count == 0 || _assignedTowers[slot] < 0)
            return base.ActiveTowers(slot, actor);

        var assignedTower = _assignedTowers[slot];

        // Create modified towers where only the assigned tower is not forbidden
        for (var i = 0; i < Towers.Count; ++i)
        {
            _modifiedTowers[i] = Towers[i];

            if (i == assignedTower)
            {
                // Assigned tower: never forbidden, ignore max soakers
                _modifiedTowers[i].ForbiddenSoakers = default;
                _modifiedTowers[i].MaxSoakers = int.MaxValue;
            }
            else
            {
                // Non-assigned tower: always forbidden
                _modifiedTowers[i].ForbiddenSoakers = default(BitMask).WithBit(slot);
            }
        }

        return _modifiedTowers.AsSpan(0, Towers.Count);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // Provide tower assignment hints
        var assignedTower = _assignedTowers[slot];
        if (assignedTower >= 0 && assignedTower < Towers.Count && DrawIcons)
        {
            var tower = Towers[assignedTower];
            var meteor = Meteors[assignedTower];

            // Check if player is in their assigned tower
            var inAssignedTower = tower.IsInside(actor);

            // Record tower soak in shared state
            if (inAssignedTower)
            {
                var sharedState = Module.FindComponent<IdyllicDreamSharedState>();
                sharedState?.RecordTowerSoak(slot, assignedTower, meteor.Element, meteor.Actor.Position);
            }

            if (!inAssignedTower && tower.Activation > WorldState.CurrentTime)
            {
                hints.Add($"Go to {meteor.Element} tower!");
            }

            // Warn if in wrong tower
            for (var i = 0; i < Towers.Count; ++i)
            {
                if (i != assignedTower && Towers[i].IsInside(actor))
                {
                    hints.Add("Wrong tower!");
                    break;
                }
            }
        }
        else if (DrawIcons && Towers.Count > 0)
        {
            // Fallback: assignment failed, use base tower hints
            base.AddHints(slot, actor, hints);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        // Highlight players who should be in the same tower as pc
        var pcTower = _assignedTowers[pcSlot];
        var playerTower = _assignedTowers[playerSlot];

        if (pcTower < 0 || playerTower < 0)
            return PlayerPriority.Normal;

        if (pcTower == playerTower)
        {
            customColor = Colors.Safe;
            return PlayerPriority.Interesting; // Same tower assignment
        }

        return PlayerPriority.Normal;
    }

    // Public method to get assigned tower for LindwurmsPortent
    public int GetAssignedTower(int slot)
    {
        if (slot < 0 || slot >= _assignedTowers.Length)
            return -1;
        return _assignedTowers[slot];
    }
}

sealed class IdyllicDreamWindTower(BossModule module) : Components.GenericKnockback(module)
{
    readonly List<Knockback> _knockbacks = [];
    IdyllicDreamElementalMeteor? _meteor;
    bool _initialized;

    public override void Update()
    {
        if (_initialized)
            return;

        _meteor ??= Module.FindComponent<IdyllicDreamElementalMeteor>();
        if (_meteor == null)
            return;

        if (_meteor.Towers.Count == 0 || _meteor.Meteors.Count == 0)
            return;

        var activation = _meteor.Towers[0].Activation;
        var meteors = CollectionsMarshal.AsSpan(_meteor.Meteors);

        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];
            if (m.Element != Element.Wind)
                continue;

            _knockbacks.Add(new Knockback(
                origin: m.Actor.Position,
                distance: 23.5f,
                activation: activation,
                shape: new AOEShapeCircle(3f),
                kind: Kind.AwayFromOrigin,
                ignoreImmunes: true
            ));
        }

        _initialized = true;
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_knockbacks);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.LindwurmsDarkII)
            return;

        _knockbacks.Clear();
        _initialized = false;
    }
}

sealed class IdyllicDreamLindwurmsDarkII(BossModule module) : Components.GenericBaitAway(module)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];
    IdyllicDreamElementalMeteor? _meteor;
    bool _initialized;

    static readonly AOEShapeRect DarkRect = new(50, 5);

    public override void Update()
    {
        if (!_initialized)
        {
            _meteor ??= Module.FindComponent<IdyllicDreamElementalMeteor>();
            if (_meteor == null)
                return;

            if (_meteor.Towers.Count == 0 || _meteor.Meteors.Count == 0)
                return;

            var activation = _meteor.Towers[0].Activation;
            var meteors = CollectionsMarshal.AsSpan(_meteor.Meteors);

            for (var i = 0; i < meteors.Length; ++i)
            {
                ref var m = ref meteors[i];
                if (m.Element != Element.Dark)
                    continue;

                _sources.Add((m.Actor, activation));
            }

            _initialized = true;
        }

        CurrentBaits.Clear();

        // Stop showing baits after activation time has passed
        if (_sources.Count > 0 && WorldState.CurrentTime >= _sources[0].Activation)
            return;

        var count = _sources.Count;
        for (var i = 0; i < count; ++i)
        {
            var (src, activation) = _sources[i];

            // closest player in 3y circle
            var target = Raid.WithoutSlot(true, true, true).Closest(src.Position);
            if (target == null)
                continue;

            if (!target.Position.InCircle(src.Position, 3))
                continue;

            CurrentBaits.Add(new(src, target, DarkRect, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LindwurmsDarkII or (uint)AID.CosmicKiss)
        {
            _sources.Clear();
            _initialized = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // intentionally suppress default bait hints
    }
}

sealed class IdyllicDreamDoom(BossModule module) : BossComponent(module)
{
    readonly BitMask _doomSlots;

    // --------------------------------------------------------
    // Status Tracking
    // --------------------------------------------------------

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Doom)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _doomSlots.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Doom)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _doomSlots.Clear(slot);
    }

    // --------------------------------------------------------
    // Player Hints
    // --------------------------------------------------------

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;

        var mask = _doomSlots;
        if (mask.None())
            return;

        for (var i = 0; i < PartyState.MaxAllies; ++i)
        {
            if (!mask[i])
                continue;

            var target = Raid[i];
            if (target == null || target.PendingDispels.Count != 0)
                continue;

            hints.Add($"Cleanse {target.Name}!");
            break; // only need one warning
        }
    }

    // --------------------------------------------------------
    // AI Hints
    // --------------------------------------------------------

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var mask = _doomSlots;
        if (mask.None())
            return;

        for (var i = 0; i < PartyState.MaxAllies; ++i)
        {
            if (!mask[i])
                continue;

            var target = Raid[i];
            if (target == null || target.PendingDispels.Count != 0)
                continue;

            hints.ShouldCleanse.Set(i);
        }
    }

    // --------------------------------------------------------
    // Priority Highlighting
    // --------------------------------------------------------

    public override PlayerPriority CalcPriority(
        int pcSlot,
        Actor pc,
        int playerSlot,
        Actor player,
        ref uint customColor)
    {
        if (!pc.Class.CanEsuna())
            return PlayerPriority.Normal;

        if (!_doomSlots[playerSlot])
            return PlayerPriority.Normal;

        if (player.PendingDispels.Count != 0)
            return PlayerPriority.Normal;

        return PlayerPriority.Critical;
    }
}

sealed class IdyllicDreamHotBlooded : Components.StayMove
{
    readonly List<Actor> _sources = [];
    readonly DateTime _activation;
    bool _resolved;

    const int PriorityHotBlooded = 1;

    public IdyllicDreamHotBlooded(BossModule module) : base(module)
    {
        var meteorComp = module.FindComponent<IdyllicDreamElementalMeteor>();
        if (meteorComp == null)
            return;

        if (meteorComp.Towers.Count > 0)
            _activation = meteorComp.Towers[0].Activation;

        var meteors = CollectionsMarshal.AsSpan(meteorComp.Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            if (meteors[i].Element == Element.Fire)
                _sources.Add(meteors[i].Actor);
        }
    }

    public override void Update()
    {
        if (_resolved || _activation == default)
            return;

        var sources = CollectionsMarshal.AsSpan(_sources);
        var sourceCount = sources.Length;

        foreach (var (slot, player) in Raid.WithSlot(true, true, true))
        {
            var pos = player.Position;

            var inside = false;
            for (var i = 0; i < sourceCount; ++i)
            {
                var delta = pos - sources[i].Position;
                if (delta.LengthSq() <= 9f) // radius 3 squared
                {
                    inside = true;
                    break;
                }
            }

            if (inside)
            {
                var state = new PlayerState(
                    Requirement.Stay2,
                    _activation,
                    PriorityHotBlooded
                );

                SetState(slot, ref state);
            }
            else
            {
                ClearState(slot, PriorityHotBlooded);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.HotBlooded)
            return;

        _resolved = true;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            ClearState(slot, PriorityHotBlooded);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.HotBlooded)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            ClearState(slot, PriorityHotBlooded);
    }
}

sealed class LindwurmsStoneIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LindwurmsStoneIII, 4);

sealed class LindwurmsPortent(BossModule module) : Components.GenericBaitAway(module)
{
    struct Portent
    {
        public Actor Source;
        public bool Far;
        public DateTime Expire;
    }

    // "Giving Far" = Wind tower position, FarPortent (player has far debuff, stands here)
    static readonly WPos WestGivingFar = new(91f, 100f);
    static readonly WPos EastGivingFar = new(109f, 100f);

    // "Giving Near" = Doom tower position, NearbyPortent (player has near debuff, stands here)
    static readonly WPos WestGivingNear = new(93f, 94f);
    static readonly WPos EastGivingNear = new(107f, 94f);

    // "Taking Far" = Earth/Fire tower position for far baiting (player baiting far cone)
    static readonly WPos WestTakingFar = new(86f, 109f);
    static readonly WPos EastTakingFar = new(114f, 109f);

    // "Taking Near" = Earth/Fire tower position for near baiting (player baiting near cone)
    static readonly WPos WestTakingNear = new(89f, 95f);
    static readonly WPos EastTakingNear = new(110f, 97f);

    static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    readonly List<Portent> _sources = [];
    readonly Dictionary<int, WPos> _assignedPositions = [];
    readonly BitMask _nearbyPortentSlots = new();
    readonly BitMask _farawayPortentSlots = new();

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        switch (status.ID)
        {
            case (uint)SID.FarawayPortent:
                _sources.Add(new Portent
                {
                    Source = actor,
                    Far = true,
                    Expire = status.ExpireAt
                });
                _farawayPortentSlots.Set(slot);
                break;

            case (uint)SID.NearbyPortent:
                _sources.Add(new Portent
                {
                    Source = actor,
                    Far = false,
                    Expire = status.ExpireAt
                });
                _nearbyPortentSlots.Set(slot);
                break;
        }
    }

    public override void Update()
    {
        // Calculate positions when portents are active
        if (_sources.Count > 0)
        {
            _assignedPositions.Clear();
            CalculatePositions();
        }

        CurrentBaits.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var len = sources.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var s = ref sources[i];

            var sourcePos = s.Source.Position;
            Actor? target = null;

            if (s.Far)
            {
                var maxDist = -1f;
                foreach (var player in Raid.WithoutSlot(true, true, true))
                {
                    var dist = (player.Position - sourcePos).LengthSq();
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        target = player;
                    }
                }
            }
            else
            {
                var minDist = float.MaxValue;
                foreach (var player in Raid.WithoutSlot(true, true, true))
                {
                    if (player == s.Source)
                        continue;

                    var dist = (player.Position - sourcePos).LengthSq();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        target = player;
                    }
                }
            }

            if (target != null)
            {
                CurrentBaits.Add(new(
                    s.Source,
                    target,
                    _shape,
                    s.Expire
                ));
            }
        }
    }

    void CalculatePositions()
    {
        // Get shared state to retrieve tower assignments
        var sharedState = Module.FindComponent<IdyllicDreamSharedState>();
        if (sharedState == null)
            return;

        foreach (var (slot, actor) in Raid.WithSlot(true, true, true))
        {
            var (towerIndex, element, position) = sharedState.GetTowerSoak(slot);
            if (towerIndex < 0)
                continue; // No tower assignment found

            // Determine platform based on player's CURRENT position (after knockback)
            var isWest = actor.Position.X < 100;

            // Position is determined by the tower element, not the portent status
            // Wind towers -> GivingFar position (after being knocked to opposite platform)
            // Dark towers -> GivingNear position  
            // Earth/Fire close towers -> TakingNear position
            // Earth/Fire far towers -> TakingFar position

            if (element == Element.Wind)
            {
                _assignedPositions[slot] = isWest ? WestGivingFar : EastGivingFar;
            }
            else if (element == Element.Dark)
            {
                _assignedPositions[slot] = isWest ? WestGivingNear : EastGivingNear;
            }
            else if (element is Element.Earth or Element.Fire)
            {
                // Determine if this Earth/Fire tower is close or far from center
                // Close towers (X closer to 100) -> TakingNear
                // Far towers (X further from 100) -> TakingFar
                var distFromCenter = Math.Abs(position.X - 100);
                var isCloseTower = distFromCenter < 10; // Adjust threshold as needed

                _assignedPositions[slot] = isWest
                    ? (isCloseTower ? WestTakingNear : WestTakingFar)
                    : (isCloseTower ? EastTakingNear : EastTakingFar);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        // Add positional hint if config enabled
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        if (config.ShowLindwurmsPortentHints && _assignedPositions.TryGetValue(slot, out var assignedPos))
        {
            var dist = (actor.Position - assignedPos).Length();
            if (dist > 1.5f)
                hints.Add($"Go to assigned position!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // Draw only the assigned position for this player if config enabled
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        if (config.ShowLindwurmsPortentHints && _assignedPositions.TryGetValue(pcSlot, out var assignedPos))
        {
            Arena.ZoneCircleOutline(assignedPos, 1, Colors.Safe);
            Arena.AddLine(pc.Position, assignedPos, Colors.Safe);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.LindwurmsThunderII)
            return;

        NumCasts++;
        _sources.Clear();
        _assignedPositions.Clear();
        _nearbyPortentSlots.Reset();
        _farawayPortentSlots.Reset();
    }
}
