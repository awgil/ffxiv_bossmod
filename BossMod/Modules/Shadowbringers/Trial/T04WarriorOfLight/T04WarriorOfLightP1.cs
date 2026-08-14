namespace BossMod.Shadowbringers.Trial.T04WarriorOfLightP1;

public enum OID : uint
{
    WarriorOfLight = 0x2DDD,
    WarriorOfLightP2 = 0x2DDE,
    Helper = 0x233C,
    SpectralEgi = 0x2DEB, // R1.800, x4 : Bahamut
    SpectralWarrior = 0x2DE8, // R0.945-1.350, x2
    WyrmOfLight = 0x2DEA, // R4.200, x1
    SpectralBlackMage = 0x2DE5, // R1.013-1.350, x2
    SpectralBard = 0x2DE7, // R0.945-1.350, x2
    SpectralSummoner = 0x2DE9, // R0.945-1.350, x2
    SpectralDarkKnight = 0x2DE6, // R0.945-1.350, x2
    ShadowOfTheAncients = 0x2F45, // R1.250, x1
    SpectralWhiteMage = 0x2DE4, // R1.013-1.350, x2
    SpectralNinja = 0x2DE3, // R1.013-1.350, x2
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    BrimstoneActor = 0x1EAFFE, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // WarriorOfLight/WarriorOfLightP2->player, no cast, single-target
    TerrorUnleashed = 20263, // WarriorOfLight->self, 3.0s cast, range 70 circle
    SolemnConfiteor = 20291, // WarriorOfLight/WarriorOfLightP2->self, 3.0s cast, single-target
    SolemnConfiteor1 = 20266, // 233C->location, 3.0s cast, range 6 circle
    CoruscantSaberDonut = 20241, // WarriorOfLight->self, 7.0s cast, range 5-60 donut
    CoruscantSaberCircle = 20240, // WarriorOfLight->self, 7.0s cast, range 10 circle
    AbsoluteFireIII = 20270, // WarriorOfLight/WarriorOfLightP2->self, 5.0s cast, single-target
    AbsoluteFireIII1 = 20239, // 233C->self, no cast, range 70 circle : Imbued weapon : fires off right after Imbued Coruscance
    ImbuedAbsoluteFireIII = 20242, // WarriorOfLightP2->self, 3.0s cast, single-target
    AbsoluteBlizzardIII = 20269, // WarriorOfLight->self, 5.0s cast, single-target
    AbsoluteBlizzardIII1 = 20238, // 233C->self, no cast, range 70 circle : Imbued weapon : fires off right after Imbued Coruscance
    ImbuedAbsoluteBlizzardIII = 20243, // WarriorOfLight/WarriorOfLightP2->self, 3.0s cast, single-target
    ImbuedCoruscance = 20299, // WarriorOfLight->self, 7.0s cast, range 10 circle
    ImbuedCoruscance1 = 20300, // WarriorOfLightP2->self, 7.0s cast, range 5-60 donut
    SwordOfLight = 20290, // WarriorOfLight/WarriorOfLightP2->self, 3.0s cast, single-target
    _Weaponskill_ = 20293, // WarriorOfLight/WarriorOfLightP2->location, no cast, ???
    Ascendance = 21297, // WarriorOfLight->self, 6.0s cast, range 60 circle
    _Weaponskill_1 = 20593, // WarriorOfLight->self, no cast, single-target
    AbsoluteTeleport = 21298, // WarriorOfLight->self, 5.0s cast, single-target
    _Ability_ = 20611, // 233C->player, no cast, single-target
    _Weaponskill_2 = 21379, // WarriorOfLightP2->self, no cast, single-target
    UltimateCrossoverVisual = 21627, // WarriorOfLightP2->self, 7.0s cast, single-target
    UltimateCrossover = 21628, // 233C->self, 6.0s cast, range 60 circle
    SpecterOfLight = 20279, // WarriorOfLightP2->self, 3.0s cast, single-target
    Twincast = 20285, // SpectralBlackMage->self, 3.0s cast, single-target
    Twincast1 = 20284, // SpectralWhiteMage->self, 3.0s cast, single-target
    _Weaponskill_3 = 21278, // SpectralBlackMage/SpectralWhiteMage/SpectralDarkKnight/SpectralBard/SpectralNinja/SpectralSummoner/SpectralWarrior->WarriorOfLightP2, no cast, single-target
    MeteoricBurst = 20258, // 233C->self, no cast, range 60 circle
    MeteorImpact = 20257, // 233C->self, no cast, range 4 circle
    ElddragonDive = 20265, // WarriorOfLightP2->self, 5.0s cast, range 70 circle
    SummonWyrm = 20289, // WarriorOfLightP2->self, 3.0s cast, single-target
    BrimstoneEarth = 20282, // SpectralDarkKnight->self, 8.0s cast, single-target
    BrimstoneEarth1 = 20254, // 233C->location, 8.0s cast, range 6 circle
    BrimstoneEarth2 = 20255, // 233C->self, no cast, range 0 circle
    DelugeOfDeath = 20283, // SpectralBard->self, 3.0s cast, single-target
    Cauterize = 20261, // WyrmOfLight->self, 4.0s cast, range 40 width 20 rect
    AbsoluteHoly = 20267, // WarriorOfLightP2->self, 5.0s cast, single-target
    DelugeOfDeath1 = 20256, // 233C->players, 5.0s cast, range 100 circle
    AbsoluteHoly1 = 20237, // 233C->players, no cast, range 6 circle
    TheBitterEnd = 20264, // WarriorOfLightP2->player, 5.0s cast, single-target
    ToTheLimit = 20276, // WarriorOfLightP2->self, 3.0s cast, single-target
    RadiantBraver = 21076, // WarriorOfLightP2->self, 6.0s cast, single-target
    RadiantBraver1 = 20246, // WarriorOfLightP2->self, no cast, range 60 90.000-degree cone
    RadiantBraver2 = 20247, // 233C->self, no cast, range 60 90.000-degree cone
    ToTheLimit1 = 20277, // WarriorOfLightP2->self, 3.0s cast, single-target
    RadDespMarker = 20294, // 233C->player, no cast, single-target
    RadiantDesperado = 20829, // WarriorOfLightP2->self, 6.0s cast, single-target
    RadiantDesperado1 = 20248, // WarriorOfLightP2->self, no cast, range 60 width 8 rect
    RadiantDesperado2 = 20249, // WarriorOfLightP2->self, no cast, range 60 width 8 rect
    ToTheLimit2 = 20278, // WarriorOfLightP2->self, 3.0s cast, single-target
    RadiantMeteor = 20250, // WarriorOfLightP2->self, 6.0s cast, single-target
    RadiantMeteor1 = 20251, // 233C->players, 6.0s cast, range 20 circle : IconID 233
    ShiningWave = 20262, // 233C->self, no cast, range 50 ?-degree cone
    SuitonSanVisual = 20280, // SpectralNinja->self, 3.0s cast, single-target
    KatonSan = 20281, // SpectralNinja->self, 3.0s cast, single-target
    _Ability_2 = 20296, // 233C->self, 6.0s cast, range 60 width 40 rect
    SuitonSan = 20252, // 233C->self, 6.0s cast, range 60 width 60 rect
    KatonSan1 = 20253, // 233C->players, 5.0s cast, range 6 circle
    Summon = 20287, // SpectralSummoner->self, 3.0s cast, single-target
    FlareBreath = 21073, // SpectralSummoner->self, 9.0s cast, single-target
    FlareBreath1 = 20260, // 2DEB->self, 4.0s cast, range 60 90.000-degree cone : Bahamut?
    PerfectDecimation = 20286, // SpectralWarrior->self, 5.0s cast, single-target
    PerfectDecimation1 = 20259, // 233C->self, 6.8s cast, range 60 45.000-degree cone
    PerfectDecimation2 = 21527, // SpectralWarrior->self, no cast, single-target
}

public enum SID : uint
{
    TrueWalkingDead = 2303, // WarriorOfLight->player, extra=0x0
    Pyretic = 960, // Helper->player, extra=0x0
    ImbuedSaber = 2377, // WarriorOfLight->WarriorOfLight, extra=0x1
    _Gen_1 = 2056, // none->WarriorOfLight, extra=0xC9
    DeepFreeze = 3480, // WarriorOfLight->player, extra=0x0
    Fetters = 2407, // WarriorOfLight->player, extra=0xC8
    _Gen1_ = 2414, // WarriorOfLight->player, extra=0x181E
    _Gen_VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    _Gen_ = 2193, // WarriorOfLightP2->WarriorOfLightP2, extra=0xBA/0xBB/0xBC
    _Gen_PhysicalVulnerabilityUp = 2090, // WarriorOfLightP2->player, extra=0x0
    _Gen_Bleeding = 2088, // none->player, extra=0x0
}

public enum IconID : uint
{
    NoMove = 227, // player->self
    Move = 225, // player->self
    ProximityBait = 87, // player->self
    Stack = 161, // player->self
    TankBuster = 218, // player->self
    RadiantBraverIcon = 234, // player->self
    RadiantMeteorIcon = 233, // player->self
}

public enum TetherID : uint
{
    FlareBreathTether = 17, // SpectralEgi->player
}

// Reduces the entire party's HP to 1 and inflicts True Walking Dead, which will kill affected
// players when the debuff's timer runs out. The debuff is removed by healing the player to full HP.
sealed class TerrorUnleashed(BossModule module) : Components.RaidwideCast(module, (uint)AID.TerrorUnleashed);

// range 6 circle
sealed class SolemnConfiteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SolemnConfiteor1, 6f);

// range 5-60 donut
sealed class CoruscantSaberDonut(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.CoruscantSaberDonut, new AOEShapeDonut(5f, 60f));

// range 10 circle
sealed class CoruscantSaberCircle(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.CoruscantSaberCircle, 10f);

/*
 * Marks each player with a "play" marker above their head. After the marker goes away,
 * each player will take minor damage and be inflicted with Deep Freeze unless they are
 * moving when the attack goes off.
 * Marks each player with a "pause" marker. Pyretic status means make no moves or actions.
 */
class ElementStayMove(BossModule module) : Components.StayMove(module)
{
    // create a move state when weapon is imbued.
    private bool _imbuedBlizzard;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.Move)
        {
            Requirement r = Requirement.Move;

            var p = Raid.FindSlot(actor.InstanceID);
            if (p >= 0)
            {
                // We want to only stop or force movement briefly instead of entire cast.
                var state = new PlayerState(r, activation: WorldState.FutureTime(4d), finish: WorldState.FutureTime(6));
                SetState(p, in state);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.ImbuedAbsoluteBlizzardIII)
        {
            _imbuedBlizzard = true;
        }
        else if (id is (uint)AID.ImbuedCoruscance or (uint)AID.ImbuedCoruscance1 && _imbuedBlizzard)
        {
            // Cast time for imbued coruscance is approx 7 seconds.
            var _activation = WorldState.CurrentTime.AddSeconds(5.5d);
            Array.Fill(PlayerStates, new(Requirement.Move, _activation, default, _activation.AddSeconds(2)));
        }
    }

    // Let OnStatusGain/Lose handle pyretic effects.
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Pyretic && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Stay, activation: WorldState.CurrentTime, finish: status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);

        if (status.ID is (uint)SID.Pyretic && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
        // clear Imbued Blizzard when Imbued Saber status drops off of the boss.
        if (status.ID is (uint)SID.ImbuedSaber && slot >= 0 && _imbuedBlizzard)
        {
            PlayerStates[slot] = default;
            _imbuedBlizzard = false;
        }
    }

    // Default bounds are from the time a StayMove state is entered and activation.
    // That can be 14 or 15 seconds in this fight. Narrow the scope from a projected time to a finish time.
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref readonly var state = ref PlayerStates[slot];
        switch (state.Requirement)
        {
            case Requirement.Stay:
                if (state.Activation <= WorldState.CurrentTime && WorldState.CurrentTime <= state.Finish)
                {
                    hints.Add("Stop everything!", actor.PrevPosition != actor.PrevPosition || actor.CastInfo != null || actor.TargetID != default); // note: assume if target is selected, we might autoattack...
                }
                break;
            case Requirement.Stay2:
                if (state.Activation <= WorldState.CurrentTime && WorldState.CurrentTime <= state.Finish)
                {
                    hints.Add("Don't move!", actor.PrevPosition != actor.PrevPosition); // you are allowed to attack here, only moving is forbidden
                }
                break;
            case Requirement.Move:
                if (state.Activation <= WorldState.CurrentTime && WorldState.CurrentTime <= state.Finish)
                {
                    hints.Add("Move!", actor.PrevPosition == actor.PrevPosition && actor.PosRot.Y == actor.PrevPosRot.Y);
                }
                break;

        }
        if (actor.IsDead && state.Requirement != Requirement.None)
        {
            PlayerStates[slot] = default;
        }
    }

    public override void Update()
    {
        base.Update();
        /*
         * Absolute Blizzard needs a clear condition that isn't right when the spell finishes.
         * Update checks to see if the StayMove.Finish time has passed and clears move state if so.
         */
        var slot = Raid.FindSlot(Raid.Player()!.InstanceID);

        if (PlayerStates[slot].Finish != default && PlayerStates[slot].Finish <= WorldState.CurrentTime)
        {
            Array.Fill(PlayerStates, default);
        }
    }
}

// circle aoe
sealed class ImbuedCoruscance(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.ImbuedCoruscance, new AOEShapeCircle(10f));

// Donut aoe, leaves only a small safe circle at boss location.
sealed class ImbuedCoruscanceDonut(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.ImbuedCoruscance1, new AOEShapeDonut(5f, 60f));

// Boss LB4. Tanks should cast LB3 when it says 'Transcend your limits' or party will probably wipe.
sealed class UltimateCrossover(BossModule module) : Components.RaidwideCast(module, (uint)AID.UltimateCrossover, hint: "TankLB3 when 'Transcend your limits...'");

sealed class Ascendance(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ascendance);

// Twincast happens and then there is towers to stand in. Towers only need two soakers. Max soakers is higher
// because ai can sometimes run out of a tower when it shouldn't if you set min, max soakers to 2,2.
sealed class Twincast(BossModule module) : Components.CastTowers(module, (uint)AID.MeteoricBurst, 4f, 2, 5)
{
    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        if (len <= 1)
            return towers;
        // Only expose the first towers (earliest activation) so the AI soaks in sequence rather than rushing to the closest one.
        var nextIdx = 0;
        for (var i = 1; i < len; ++i)
        {
            if (towers[i].Activation < towers[nextIdx].Activation)
                nextIdx = i;
        }
        return towers.Slice(nextIdx, 1);
    }

    /*
     * The outer circle towers follow an index placement pattern of 0 (North)
     * clockwise through 7 to place a tower at each cardinal and intercardinal direction.
     * We can use the position of 0 (100f, 89.5f) and Arena.Center (100, 100) to
     * rotate the position through all of the directions and calculate the location
     * of a tower. Each of the eight directions is 45 degrees apart. Multiply 45 degrees
     * by the index of the direction we want to determine how far to rotate around center.
     * We use a method to run the calculation : WPos.RotateAroundOrigin(rotAngFloat, Arena.Center, startPos)
     * The result is the position of our tower if index is 0-7.
     */
    public override void OnMapEffect(byte index, uint state)
    {
        // activation time is an estimate. We use map effects to remove towers.
        DateTime _activation = WorldState.FutureTime(10f);
        WPos pos;
        WPos startPos = new(100f, 89.5f);
        // We derive angle by 45 degrees times index position where 0 is North. 1 is NE, etc
        var rotAngFloat = 45f * index; // positive rotation is clockwise. negative is counterclockwise
        // Calculate the position of the tower based on the index.
        if (index <= 07)
            pos = WPos.RotateAroundOrigin(rotAngFloat, Arena.Center, startPos);
        else if (index == 08) // This tower would be between above and the arena center east side
            pos = new(104, 100);
        else if (index == 09) // // This tower would be between above and the arena center west side
            pos = new(96, 100);
        // If there are inner north or south towers I haven't seen them yet to derive a formula.
        else
            pos = default;
        // state 00020001 is when tower template is placed
        if (state == 0x00020001u && pos != default)
            Towers.Add(new(pos.Quantized(), 3f, 2, 2, default, _activation));
        // state 00080004 is when tower template is removed.
        if (state == 0x00080004u && pos != default)
            Towers.RemoveAll(tower => tower.Position == pos.Quantized());
    }
}

sealed class ElddragonDive(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElddragonDive);

// Dragon swoops across arena.
sealed class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(40f, 10f));

// Large proximity bait. Target should run it away from party.
sealed class DelugeOfDeath(BossModule module)
    : Components.BaitAwayIcon(module, 20f, (uint)IconID.ProximityBait, (uint)AID.DelugeOfDeath1)
{
    // This sets safe zones in the corners.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var _activation = WorldState.FutureTime(5.0f);
        // We want to path to another corner if we have a deluge of death bait.
        WPos[] corners = [new(80f, 80f), new(120f, 80f), new(120f, 120f), new(80f, 120f)];
        var count = corners.Length;
        if (count != 0 && ActiveBaits.Count > 0)
        {
            var baitZonez = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var c = corners[i];
                // purposefully smaller than the bait.
                baitZonez[i] = new SDInvertedCircle(c, 3f);
            }
            // Only targets of spreads run to corners.
            foreach (var bait in ActiveBaits)
            {
                if (bait.Target.InstanceID == Raid.Player()!.InstanceID)
                {
                    hints.AddForbiddenZone(new SDIntersection(baitZonez), _activation);
                }
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

// Circle cast stack.
sealed class AbsoluteHoly(BossModule module)
    : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.AbsoluteHoly1, 6f, 2, 6)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        // If pc has Deluge of Death proximity bait they should not stack.
        if (iconID == (uint)IconID.ProximityBait && targetID == Raid.Player()!.InstanceID)
        {
            // Proximity bait target -> forbidden soakers
            BitMask forbid = default;

            forbid.Set(Raid.FindSlot(targetID));
            if (Stacks.Count != 0)
            {
                Stacks.Ref(0).ForbiddenPlayers = forbid;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        // Account for KatonSan because the same SpreadIcon is used for both.
        // this prevents the bait circle from staying on radar all fight long.
        if (spell.Action.ID == (uint)AID.KatonSan1)
        {
            Stacks.Clear();
        }
    }
}

// single-target tankbuster
sealed class TheBitterEnd(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TheBitterEnd, 5f, true, true, true);

// bait away cast 90 degree cones with red arrow markers : these originate at arena.center not front of boss.
// Cones purposefully made slightly larger.
sealed class RadiantBraver(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 46f.Degrees()),
    (uint)IconID.RadiantBraverIcon, (uint)AID.RadiantBraver, 6D)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
        {
            CurrentBaits.Add(new(Arena.Center, WorldState.Actors.Find(targetID) ?? actor, Shape, WorldState.FutureTime(ActivationDelay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.RadiantBraver1 or (uint)AID.RadiantBraver2)
        {
            CurrentBaits.RemoveAt(0);
        }
    }
}

// Radiant Braver is 2 cone baits and a cone that shoots directly from front of boss model. This part is the frontal cleave cone.
sealed class RadiantBraverCleave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadiantBraver, new AOEShapeCone(60F, 46F.Degrees()));

/*
 * Looks like phys ranged limit break. Has two line stack markers. Fires one then the other.
 * Gives vulnerability up, probably kills a player that jumps into second line stack with vuln up status.
 * TODO if status vuln up do not try to stack. If pc is main target do not try to stack with other main target.
 */
sealed class RadiantDesperado(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.RadDespMarker, aidResolve: (uint)AID.RadiantDesperado1, activationDelay: 6.0f, maxStackSize: 4, markerIsFinalTarget: false)
{
    private readonly AOEShape rect = new AOEShapeRect(60f, 4f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AidMarker == default && IconId == default)
        {
            return;
        }

        var id = spell.Action.ID;
        if (id == AidMarker && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, rect, WorldState.FutureTime(ActionDelay), maxCasts: MaxCasts));
        }
        else if (id is (uint)AID.RadiantDesperado1 or (uint)AID.RadiantDesperado2)
        {
            if (CurrentBaits.Count != 0)
            {
                if (CurrentBaits.Count == 1)
                {
                    CurrentBaits.Clear();
                }
                CurrentBaits.RemoveAll(bait => bait.Target.TargetID == spell.MainTargetID);
                ++NumCasts;
            }
        }
    }
}

// Level 3 Limit Break. Extremely large circle AoEs targeted on four random players.
// added some spots in the corners to get the baits to run to.
sealed class RadiantMeteor(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RadiantMeteor1, 20f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var _activation = WorldState.FutureTime(5.0f);
        // We want to path to the corner if we have a radiant meteor bait.
        WPos[] corners = [new(80f, 80f), new(120f, 80f), new(120f, 120f), new(80f, 120f)];
        var count = corners.Length;
        if (count != 0 && ActiveSpreads.Count > 0)
        {
            var baitZonez = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var c = corners[i];
                // purposefully smaller than the bait.
                baitZonez[i] = new SDInvertedCircle(c, 3f);
            }
            // Only targets of spreads run to corners.
            foreach (var spread in ActiveSpreads)
            {
                if (spread.Target.InstanceID == Raid.Player()!.InstanceID)
                {
                    hints.AddForbiddenZone(new SDIntersection(baitZonez), _activation);
                }
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

// First circle aoe part of Brimstone cast.
sealed class BrimstoneEarth(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrimstoneEarth1, new AOEShapeCircle(6f));

// Brimstone AOE circle that grows with each instant cast.
sealed class BrimstoneEarthGrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrimstoneEarth1, new AOEShapeCircle(0.5f), maxCasts: 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BrimstoneEarth1)
            BespokeAOE(caster, spell);
    }

    public void BespokeAOE(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BrimstoneEarth1 or (uint)AID.BrimstoneEarth2)
        {
            /*
             * spin up aoe's for the growing aoe.  _genAOE is a magic number because we do not have a good way to predict
             * how many will be cast. The casts are counted from logs, we just do not know for sure if it is always the same.
             * Instead we make a few extra and then clear the Casters queue when the actors
             * that are in spell.TargetXZ position dies.
             */
            var _genAOE = 16;

            for (var i = 0; i < _genAOE; i++)
            {
                // The first long cast takes approx 6 seconds. The instants that follow every 1 seconds or so.
                DateTime _activation = WorldState.FutureTime(5f + (1.0f * i));

                Casters.Add(new(new AOEShapeCircle(i), spell.LocXZ.Quantized(), spell.Rotation, _activation,
                    actorID: caster.InstanceID));
            }
            SortHelpers.SortAOEByActivation(Casters);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BrimstoneEarth1 or (uint)AID.BrimstoneEarth2 && Casters.Count != 0)
            Casters.RemoveAll(cast => cast.Activation <= WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BrimstoneEarth2 && Casters.Count != 0)
        {
            Casters.RemoveAll(cast => cast.Activation <= WorldState.CurrentTime);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BrimstoneActor)
        {
            Casters.Clear();
        }
    }
}

// This is the triangle aoe for after sword of light draws triangles on arena
sealed class SwordOfLight(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.SwordOfLight, new AOEShapeCone(50f, 26.5f.Degrees()))
{
    // Do not show the aoe marker on SwordOfLight cast.
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    // Generate the aoe marker on map effect.
    public override void OnMapEffect(byte index, uint state)
    {
        //* ENVC|14|0002|0001 : where 14 is index (cardinal direction) and 00020001 is state that places sword at starting point of draw.
        if (state == 0x00020001u)
        {
            (var pos, var rot) = index switch
            {
                //14 is north facing south.
                0x14 => (new WPos(100f, 80f), default),
                //15 is east facing west.
                0x15 => (new WPos(120f, 100f), 270f.Degrees()),
                //index 16 south facing north.
                0x16 => (new WPos(100f, 120f), 180f.Degrees()),
                //index 17 is west facing east.
                0x17 => (new WPos(80f, 100f), 90f.Degrees()),
                _ => default
            };
            if (pos != default)
            {
                // sword of light cast time is approx 2.7 seconds, might need to extend this because aoe clears later.
                var _activation = WorldState.FutureTime(2.7d);
                Casters.Add(new(Shape, pos.Quantized(), rot, _activation));
            }
        }
    }

    // Clear the casts on ShiningWave instant cast event.
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShiningWave && Casters.Count != 0)
        {
            Casters.RemoveAll(cast => cast.Activation <= WorldState.CurrentTime && cast.Shape is AOEShapeCone);
        }
    }
}

sealed class SuitonSan(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SuitonSan, 30f, kind: Kind.DirForward)
{
    // Leave a free space where ai should run to during knockback
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDInvertedRect(kb.Origin, kb.Direction, 8f, 8f, 20f));
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

// stack cast
sealed class KatonSan(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.KatonSan1, 6f, 6, 8);

// Bait away tethers
sealed class FlareBreath(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60f, 45f.Degrees()),
    (uint)TetherID.FlareBreathTether, (uint)AID.FlareBreath1);

// Flare breath aoe to  show where the aoe markers land after tethers disappear.
sealed class FlareBreathAOE(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.FlareBreath1, new AOEShapeCone(60f, 45f.Degrees()));

// 4 cones followed by 4 cones radiating out from center.
sealed class PerfectDecimation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PerfectDecimation1, new AOEShapeCone(60f, 22.5f.Degrees()), maxCasts: 4);

[SkipLocalsInit]
sealed class T04WarriorOfLightP1States : StateMachineBuilder
{
    public T04WarriorOfLightP1States(T04WarriorOfLightP1 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerrorUnleashed>()
            .ActivateOnEnter<SolemnConfiteor>()
            .ActivateOnEnter<ElementStayMove>()
            .ActivateOnEnter<CoruscantSaberDonut>()
            .ActivateOnEnter<CoruscantSaberCircle>()
            .ActivateOnEnter<Twincast>()
            .ActivateOnEnter<AbsoluteHoly>()
            .ActivateOnEnter<TheBitterEnd>()
            .ActivateOnEnter<RadiantBraver>()
            .ActivateOnEnter<RadiantBraverCleave>()
            .ActivateOnEnter<RadiantDesperado>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<ImbuedCoruscance>()
            .ActivateOnEnter<ImbuedCoruscanceDonut>()
            .ActivateOnEnter<ElddragonDive>()
            .ActivateOnEnter<BrimstoneEarth>()
            .ActivateOnEnter<BrimstoneEarthGrow>()
            .ActivateOnEnter<SwordOfLight>()
            .ActivateOnEnter<SuitonSan>()
            .ActivateOnEnter<RadiantMeteor>()
            .ActivateOnEnter<UltimateCrossover>()
            .ActivateOnEnter<Ascendance>()
            .ActivateOnEnter<KatonSan>()
            .ActivateOnEnter<PerfectDecimation>()
            .ActivateOnEnter<FlareBreath>()
            .ActivateOnEnter<FlareBreathAOE>()
            .ActivateOnEnter<DelugeOfDeath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(T04WarriorOfLightP1States),
    ConfigType = null, // replace null with typeof(WarriorOfLightConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.WarriorOfLight,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Shadowbringers,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 738u,
    NameID = 9462u,
    SortOrder = 1,
    PlanLevel = 0)]

[SkipLocalsInit]
public sealed class T04WarriorOfLightP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
