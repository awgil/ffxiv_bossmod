namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class GrandCrossRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.GrandCross);

// GrandCross will be casted three times in a row with a fake or real
sealed class GrandCrossOrder(BossModule module) : BossComponent(module)
{
    private readonly List<(bool? tellingTruth, int set, List<(uint buff, DateTime expireAt)>[] playerBuffs)> grandCross = [];
    private bool tellingTruthCaught = false; // Two orbs spawn per cast, but we only want one
    private int NumCasts = 0;
    public int currentCast = 0; // Used for state machine so it easier to detect when the cast has finished

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GrandCross)
        {
            ++currentCast;

            if (currentCast < 3)
            {
                tellingTruthCaught = false;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.NeoExdeath && status.ID == (uint)SID.TellingTruthBuff)
        {
            if ((status.Extra == 0x461 || status.Extra == 0x462) && !tellingTruthCaught)
            {
                var buffs = new List<(uint buff, DateTime expireAt)>[PartyState.MaxPartySize];

                for (var i = 0; i < PartyState.MaxPartySize; i++)
                {
                    buffs[i] = [];
                }
                grandCross.Add((status.Extra == 0x462, NumCasts, buffs)); // 0x461 is fake, 0x462 is real
                NumCasts++;
                tellingTruthCaught = true;
            }
        }

        if (status.ID is ((uint)SID.BeyondDeath) or ((uint)SID.BeyondDeath1) or
            ((uint)SID.AllaganField) or
            ((uint)SID.WhiteWound) or ((uint)SID.WhiteWound1) or
            ((uint)SID.BlackWound) or ((uint)SID.BlackWound1))
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                var id = grandCross.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0)
                {
                    grandCross[id].playerBuffs[slot].Add((status.ID, status.ExpireAt));
                }
            }
        }

        if (status.ID is ((uint)SID.ForkedLightning) or ((uint)SID.CompressedWater) or
            ((uint)SID.AccelerationBomb) or ((uint)SID.CursedShriek))
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                var id = grandCross.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0)
                {
                    grandCross[id].playerBuffs[slot].Add((status.ID, status.ExpireAt));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is ((uint)SID.BeyondDeath) or ((uint)SID.BeyondDeath1) or
            ((uint)SID.AllaganField) or
            ((uint)SID.WhiteWound) or ((uint)SID.WhiteWound1) or
            ((uint)SID.BlackWound) or ((uint)SID.BlackWound1) or
            ((uint)SID.ForkedLightning) or ((uint)SID.CompressedWater) or
            ((uint)SID.AccelerationBomb) or ((uint)SID.CursedShriek))
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                var sid = status.ID;
                var expireAt = status.ExpireAt;
                foreach (var entry in grandCross)
                {
                    entry.playerBuffs[slot].RemoveAll(b => b.buff == sid);
                    Service.Log("Slot:" + slot + "Removed status effect: " + status.ID + " at world time: " + status.ExpireAt);
                }
            }
        }
    }

    public List<(uint buff, DateTime expireAt)>? GetCurrentPlayerBuffs(int slot, int set)
    {
        var id = grandCross.FindIndex(e => e.set == set);
        if (id < 0 || slot < 0)
        {
            return null;
        }

        return grandCross[id].playerBuffs[slot];
    }

    public List<(int slot, DateTime expireAt, bool? tellingTruth)> GetNextBuffPlayers(uint status, int count)
    {
        var result = new List<(int slot, DateTime expireAt, bool? tellingTruth)>();
        foreach (var index in grandCross)
        {
            for (int slot = 0; slot < index.playerBuffs.Length; slot++)
            {
                foreach (var buff in index.playerBuffs[slot])
                {
                    if (buff.buff == status)
                    {
                        result.Add((slot, buff.expireAt, index.tellingTruth));
                    }
                }
            }
        }
        return [.. result.OrderBy(buff => buff.expireAt).Take(count)];
    }
}

// TODO merge both raid wides together
sealed class TsunamiRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.P4Tsunami);
sealed class InfernoRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.P4Inferno);

// Elements will be casted two times in a row with a fake or real
sealed class TsunamiInfernoOrder(BossModule module) : BossComponent(module)
{
    public List<(bool? tellingTruth, int set, List<(uint buff, DateTime expireAt)>[] playerBuffs)> tsunamiInferno = [];
    private bool tellingTruthCaught = false; // Two orbs spawn per cast, but we only want one
    private int NumCasts = 0;
    public int currentCast = 0; // Used for state machine so it easier to detect when the cast has finished

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.P4Inferno) or ((uint)AID.P4Tsunami))
        {
            tellingTruthCaught = false;
            ++currentCast;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.ChaosP4 && status.ID == (uint)SID.TellingTruthBuff)
        {
            if ((status.Extra == 0x45F || status.Extra == 0x460) && !tellingTruthCaught)
            {
                var buffs = new List<(uint buff, DateTime expireAt)>[PartyState.MaxPartySize];
                for (var i = 0; i < buffs.Length; i++)
                {
                    buffs[i] = [];
                }
                tsunamiInferno.Add((status.Extra == 0x460, NumCasts, buffs)); // 0x45F is fake, 0x460 is real
                NumCasts++;
                tellingTruthCaught = true;
            }
        }

        if (status.ID is ((uint)SID.DynamicFluidP4) or ((uint)SID.EntropyP4))
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                var id = tsunamiInferno.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0)
                {
                    tsunamiInferno[id].playerBuffs[slot].Add((status.ID, status.ExpireAt));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is ((uint)SID.DynamicFluidP4) or ((uint)SID.EntropyP4))
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                var sid = status.ID;
                var expireAt = status.ExpireAt;
                foreach (var entry in tsunamiInferno)
                {
                    entry.playerBuffs[slot].RemoveAll(b => b.buff == sid && b.expireAt == expireAt);
                }
            }
        }
    }

    public List<(uint buff, DateTime expireAt)>? GetCurrentPlayerBuffs(int slot, int set)
    {
        var id = tsunamiInferno.FindIndex(e => e.set == set);
        if (id < 0 || slot < 0)
        {
            return null;
        }

        return tsunamiInferno[id].playerBuffs[slot];
    }

    public List<(int slot, DateTime expireAt, bool? tellingTruth)> GetNextBuffPlayers(uint status, int count)
    {
        var result = new List<(int slot, DateTime expireAt, bool? tellingTruth)>();
        foreach (var index in tsunamiInferno)
        {
            for (int slot = 0; slot < index.playerBuffs.Length; ++slot)
            {
                foreach (var buff in index.playerBuffs[slot])
                {
                    if (buff.buff == status)
                    {
                        result.Add((slot, buff.expireAt, index.tellingTruth));
                    }
                }
            }
        }
        return [.. result.OrderBy(buff => buff.expireAt).Take(count)];
    }
}

// Kefka will cast truth and lies throughout the encounter which need to be tracked
sealed class KefkaOrder(BossModule module) : BossComponent(module)
{
    public List<(bool tellingTruth, Element element)> tellingTruthOrder = [];
    public enum Element { Thunder, Blizzard }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID == (uint)OID.KefkaP4)
        {
            if (iconID == (uint)IconID.PurpleRingBlueOrb)
            {
                tellingTruthOrder.Add((true, Element.Thunder));
            }

            if (iconID == (uint)IconID.PurpleRingQuestionMark)
            {
                tellingTruthOrder.Add((false, Element.Thunder));
            }

            if (iconID == (uint)IconID.BlueRingBlueOrb)
            {
                tellingTruthOrder.Add((true, Element.Blizzard));
            }

            if (iconID == (uint)IconID.BlueRingQuestionMark)
            {
                tellingTruthOrder.Add((false, Element.Blizzard));
            }
        }
    }
}

// TODO fix AOE middle line - small aoe line where you cant stand at all, but can be done later there is a lot of free space
sealed class EdgeOfDeath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EdgeOfDeath, new AOEShapeRect(47.0f, 1.0f));

// TODO fix AOE middle line - small aoe line where you cant stand at all, but can be done later there is a lot of free space
// TODO change to simple AOEs at some point maybe - so it has built-in NumCasts
// You can most likely actually figure out which debuffs go with which cast and make it simpler, but unsure
sealed class AntiLight(BossModule module) : BossComponent(module)
{
    private Actor? whiteAntiLight = null;
    private Actor? blackAntiLight = null;
    private bool? tellingTruth = null;
    private uint? spellID = null;
    private bool swapped = false;
    private readonly GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    public int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WhiteAntilight)
        {
            whiteAntiLight = caster;
        }

        if (spell.Action.ID == (uint)AID.BlackAntilight)
        {
            blackAntiLight = caster;
        }

        if (spell.Action.ID is ((uint)AID.FloodOfNaught) or ((uint)AID.FloodOfNaught1) or
            ((uint)AID.FloodOfNaught2) or ((uint)AID.FloodOfNaught3))
        {
            spellID = spell.Action.ID;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WhiteAntilight)
        {
            whiteAntiLight = null;
            NumCasts++;
        }

        if (spell.Action.ID == (uint)AID.BlackAntilight)
        {
            blackAntiLight = null;
            NumCasts++;
        }

        if (spell.Action.ID is ((uint)AID.FloodOfNaught) or ((uint)AID.FloodOfNaught1) or
            ((uint)AID.FloodOfNaught2) or ((uint)AID.FloodOfNaught3) or
            ((uint)AID.EdgeOfDeath))
        {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        // Real
        if (status.ID == (uint)SID.TellingTruthBuff && status.Extra == 0x462)
        {
            tellingTruth = true;
        }

        // Fake
        if (status.ID == (uint)SID.TellingTruthBuff && status.Extra == 0x461)
        {
            tellingTruth = false;
        }
    }

    public override void Update()
    {
        if (whiteAntiLight == null || blackAntiLight == null || spellID == null)
        {
            return;
        }

        // Depending on the flood ID cast, the orbs will cast the opposite of what they actually are
        if ((spellID == (uint)AID.FloodOfNaught || spellID == (uint)AID.FloodOfNaught2) && !swapped)
        {
            (whiteAntiLight, blackAntiLight) = (blackAntiLight, whiteAntiLight);
            swapped = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (grandCrossOrder == null || whiteAntiLight == null || blackAntiLight == null || tellingTruth == null || spellID == null)
        {
            return;
        }

        var buffs = grandCrossOrder.GetCurrentPlayerBuffs(pcSlot, 2); // Sets are 0, 1, 2, and AntiLight is always the final set
        if (buffs == null)
        {
            return;
        }

        // TODO This should never happen - most likely can be removed, as buffs will always be there by the time casters are set
        if (!HasBuff(buffs, (uint)SID.BeyondDeath) && !HasBuff(buffs, (uint)SID.BeyondDeath1) &&
            !HasBuff(buffs, (uint)SID.AllaganField))
        {
            return;
        }

        Actor? correctSide = null;
        Actor? wrongSide = null;

        if (HasBuff(buffs, (uint)SID.BeyondDeath) || HasBuff(buffs, (uint)SID.BeyondDeath1))
        {
            var requireWhite = HasBuff(buffs, (uint)SID.BlackWound) || HasBuff(buffs, (uint)SID.BlackWound1);
            correctSide = requireWhite ? whiteAntiLight : blackAntiLight;
            wrongSide = requireWhite ? blackAntiLight : whiteAntiLight;
        }

        if (HasBuff(buffs, (uint)SID.AllaganField))
        {
            var requireWhite = HasBuff(buffs, (uint)SID.BlackWound) || HasBuff(buffs, (uint)SID.BlackWound1);
            correctSide = requireWhite ? blackAntiLight : whiteAntiLight;
            wrongSide = requireWhite ? whiteAntiLight : blackAntiLight;
        }

        if (correctSide == null || wrongSide == null)
        {
            return;
        }

        // Fake = swap
        if (tellingTruth == false)
        {
            (correctSide, wrongSide) = (wrongSide, correctSide);
        }

        var shape = new AOEShapeRect(47.0f, 10.5f);
        shape.Draw(Arena, correctSide.Position, correctSide.Rotation, Colors.SafeFromAOE);
        shape.Draw(Arena, wrongSide.Position, wrongSide.Rotation, Colors.AOE);
    }

    private bool HasBuff(List<(uint buff, DateTime expireAt)> buffs, uint status)
    {
        return buffs.Any(b => b.buff == status);
    }
}

// 2x water & 2x Forked Lightning will go at the same time, these all share the same set
sealed class ForkedWater(BossModule module) : Components.UniformStackSpread(module, 8.0f, 8.0f, 3, 3)
{
    private readonly GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    public int NumCasts = 0; // 4 casts will always go off, better to watch this to know if the aoes are active or not
    public bool active = true; // Used to hide everything until it need to be shown if overlap with other mechancis

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.ForkedLightningFake) or ((uint)AID.ForkedLightningReal) or
            ((uint)AID.CompressedWaterFake) or ((uint)AID.CompressedWaterReal))
        {
            ++NumCasts;
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        if (grandCrossOrder == null || NumCasts >= 4)
        {
            return;
        }

        var compressedPlayers = grandCrossOrder.GetNextBuffPlayers((uint)SID.CompressedWater, 2);
        foreach (var (slot, expireAt, tellingTruth) in compressedPlayers)
        {
            // Real
            if (tellingTruth == true)
            {
                var player = Raid[slot];
                if (player != null)
                {
                    AddStack(player, expireAt);
                }
            }

            // Fake
            if (tellingTruth == false)
            {
                var player = Raid[slot];
                if (player != null)
                {
                    AddSpread(player, expireAt);
                }
            }
        }

        var forkedPlayers = grandCrossOrder.GetNextBuffPlayers((uint)SID.ForkedLightning, 2);
        foreach (var (slot, expireAt, tellingTruth) in forkedPlayers)
        {
            // Real
            if (tellingTruth == true)
            {
                var player = Raid[slot];
                if (player != null)
                {
                    AddSpread(player, expireAt);
                }
            }

            // Fake
            if (tellingTruth == false)
            {
                var player = Raid[slot];
                if (player != null)
                {
                    AddStack(player, expireAt);
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }

        // Stacks + nothing players
        if (Stacks.Any(player => player.Target == pc) || !Spreads.Any(player => player.Target == pc))
        {
            if (pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(new WPos(100.0f, 88.0f), 1.0f, active ? Colors.Safe : Colors.Danger, 2.0f);
            }

            if (pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(new WPos(100.0f, 112.0f), 1.0f, active ? Colors.Safe : Colors.Danger, 2.0f);
            }
        }

        // Spread players
        if (Spreads.Any(player => player.Target == pc))
        {
            if (pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(new WPos(88.0f, 100.0f), 1.0f, active ? Colors.Safe : Colors.Danger, 2.0f);
            }

            if (pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(new WPos(112.0f, 100.0f), 1.0f, active ? Colors.Safe : Colors.Danger, 2.0f);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

// 4x accleration bombs where two are from 1st set and the other 2 are from the 2nd set, so it can a mix of truth and lie
// TODO move the StatusLose here for acceleration bomb to reset the playerStates since it follows the standard way to do it - wont change anything tho
// TODO improve timeline for the mechanic make the component deactivate itself, can most likely do it base on the number of buffs gone by increasing it StatusLose
sealed class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    private readonly GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    public int NumCasts = 0;

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                PlayerStates[slot] = default;
                ++NumCasts;

                Service.Log($"Slot {slot} resetting");
                Service.Log($"NumCasts {NumCasts}");
            }
        }
    }

    public override void Update()
    {
        foreach (var (slot, _) in Raid.WithSlot())
        {
            PlayerStates[slot] = default;
            //Service.Logger.Info($"Slot {slot} resetting"); // todo debug - remove later
        }

        if (grandCrossOrder == null)
        {
            return;
        }

        foreach (var (slot, expireAt, tellingTruth) in grandCrossOrder.GetNextBuffPlayers((uint)SID.AccelerationBomb, 4))
        {
            if ((expireAt - WorldState.CurrentTime).TotalSeconds > 7.0f)
            {
                continue;
            }

            if (tellingTruth == true)
            {
                PlayerStates[slot] = new(Requirement.Stay, expireAt);
                Service.Log($"Slot {slot} real bomb - stay still, which expires at {expireAt}"); // todo debug - remove later
            }

            if (tellingTruth == false)
            {
                PlayerStates[slot] = new(Requirement.Move, expireAt);
                Service.Log($"Slot {slot} fake bomb - move, which expires at {expireAt}"); // todo debug - remove later
            }
        }
    }
}

// 2x CursedShriek from the same set always
sealed class CursedShriek(BossModule module) : Components.GenericGaze(module)
{
    private readonly GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    private readonly List<(int slot, DateTime expireAt, bool inverted, Actor player)> pendingGazes = [];
    readonly LightningSafeSpots? lightningSafeSpots = module.FindComponent<LightningSafeSpots>();
    private readonly List<Eye> eyes = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.CursedShriekReal) or ((uint)AID.CursedShriekFake))
        {
            ++NumCasts;
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int pcSlot, Actor pcActor)
    {
        eyes.Clear();

        foreach (var (slot, expireAt, inverted, _) in pendingGazes)
        {
            if (slot == pcSlot)
            {
                continue;
            }

            var actor = Raid[slot];
            if (actor == null)
            {
                continue;
            }

            eyes.Add(new Eye(actor.Position, expireAt, inverted: inverted));
        }

        return CollectionsMarshal.AsSpan(eyes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (grandCrossOrder == null || NumCasts >= 2)
        {
            return;
        }

        var players = grandCrossOrder.GetNextBuffPlayers((uint)SID.CursedShriek, 2).ToList();
        if (players.Count != 2)
        {
            return;
        }

        foreach (var player in players)
        {
            if (player.slot == slot)
            {
                hints.Add("You're Gaze");
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (eyes.Count == 0)
        {
            return;
        }

        if (!eyes[0].Inverted)
        {
            hints.Add("Look away from gaze players");
        }

        if (eyes[0].Inverted)
        {
            hints.Add("Look at gaze players");
        }
    }

    public override void Update()
    {
        pendingGazes.Clear();

        if (grandCrossOrder == null || NumCasts >= 2)
        {
            return;
        }

        var players = grandCrossOrder.GetNextBuffPlayers((uint)SID.CursedShriek, 2).ToList();
        if (players.Count != 2)
        {
            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            if ((players[i].expireAt - WorldState.CurrentTime).TotalSeconds > 8.0)
            {
                continue;
            }

            var player = Raid[players[i].slot];
            if (player == null)
            {
                continue;
            }

            pendingGazes.Add((players[i].slot, players[i].expireAt, players[i].tellingTruth == false, player));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // Special case: If lightning safe spots is null it means we are on the 2nd gaze which are just north and south spots instead
        if (lightningSafeSpots == null)
        {

            // Support gaze player
            if (pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(Module.Center - new WDir(0, 2.0f), 1f, Colors.Safe, 2f);
            }

            // DD gaze player
            if (pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(Module.Center - new WDir(0, -2.0f), 1f, Colors.Safe, 2f);
            }

            // Support players
            if (!pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(Module.Center - new WDir(0, 6.0f), 1f, Colors.Safe, 2f);
            }

            // DD players
            if (!pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(Module.Center - new WDir(0, -6.0f), 1f, Colors.Safe, 2f);
            }

            return;
        }

        var aoes = lightningSafeSpots.aoes.ToList();
        if (aoes.Count == 0)
        {
            return;
        }

        // For easier logic, we will just use where the actor casters are to handle the safe spots
        foreach (var aoe in aoes)
        {
            var rect = (AOEShapeRect)aoe.Shape;
            var forward = (aoe.Rotation + rect.DirectionOffset).ToDirection();
            var safeSpot = new WPos(0.0f, 0.0f);

            // Case 1: Either of these coordinates means we figure out the bad spot and go left of it
            //         so the lightning aoe will be around [117.662, 89.372] or [89.372, 82.292]
            if (aoe.Origin.AlmostEqual(new WPos(117.662f, 89.372f), 1.0f) ||
                aoe.Origin.AlmostEqual(new WPos(89.372f, 82.292f), 1.0f))
            {
                var edgeCenter = aoe.Origin - forward.OrthoL() * rect.HalfWidth;
                safeSpot = edgeCenter + forward * (Module.Center - edgeCenter).Dot(forward) - forward.OrthoL();
            }

            // Case 2: The safe spots are in the question mark areas, this means we figure out the bad spot and go right of it
            //         so the lightning aoe will be around [110.582, 82.292] or [82.292, 89.372]
            if (aoe.Origin.AlmostEqual(new WPos(110.582f, 82.292f), 1.0f) ||
                aoe.Origin.AlmostEqual(new WPos(82.292f, 89.372f), 1.0f))
            {
                var edgeCenter = aoe.Origin + forward.OrthoL() * rect.HalfWidth;
                safeSpot = edgeCenter + forward * (Module.Center - edgeCenter).Dot(forward) + forward.OrthoL();
            }

            // Support gaze player
            if (pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(safeSpot - forward * 2f, 1f, Colors.Safe, 2f);
            }

            // DD gaze player
            if (pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(safeSpot + forward * 2f, 1f, Colors.Safe, 2f);
            }

            // Support players
            if (!pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsSupport())
            {
                Arena.ZoneCircleOutline(safeSpot - forward * 6f, 1f, Colors.Safe, 2f);
            }

            // DD players
            if (!pendingGazes.Any(eye => eye.player == pc) && pc.Class.IsDD())
            {
                Arena.ZoneCircleOutline(safeSpot + forward * 6f, 1f, Colors.Safe, 2f);
            }
        }
    }
}

sealed class InfernoBaits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StrayFlamesP4Puddle)
        {
            aoes.Add(new(new AOEShapeCircle(6.0f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
        }

        if (spell.Action.ID == (uint)AID.StrayFlamesP4Donut)
        {
            aoes.Add(new(new AOEShapeDonut(6.0f, 40.0f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.StrayFlamesP4Puddle) or ((uint)AID.StrayFlamesP4Donut))
        {
            aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Inferno(BossModule module) : Components.GenericBaitProximity(module, onlyShowOutlines: true)
{
    private readonly TsunamiInfernoOrder? tsunamiInfernoOrder = module.FindComponent<TsunamiInfernoOrder>();
    public bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.StrayFlamesP4Puddle) or ((uint)AID.StrayFlamesP4Donut))
        {
            CurrentBaits.Clear();
            active = true;
        }
    }

    public override void Update()
    {
        if (tsunamiInfernoOrder == null || active)
        {
            return;
        }

        var players = tsunamiInfernoOrder.GetNextBuffPlayers((uint)SID.EntropyP4, 8);
        if (players.Count == 0)
        {
            return;
        }

        CurrentBaits.Clear();

        foreach (var (slot, expireAt, tellingTruth) in players)
        {
            var player = Raid[slot];
            if (player == null)
            {
                continue;
            }

            // Real
            if (tellingTruth == true)
            {
                CurrentBaits.Add(new(player.Position, new AOEShapeCircle(6.0f), expireAt));
            }

            // Fake
            if (tellingTruth == false)
            {
                CurrentBaits.Add(new(player.Position, new AOEShapeDonut(6.0f, 40.0f), expireAt));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!active)
        {
            Arena.ZoneCircleOutline(new WPos(100.0f, 100.0f), 1.0f, Colors.Safe, 2.0f);
        }
    }
}

sealed class TsunamiBaits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StraySprayP4Puddle)
        {
            aoes.Add(new(new AOEShapeCircle(6.0f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
        }

        if (spell.Action.ID == (uint)AID.StraySprayP4Donut)
        {
            aoes.Add(new(new AOEShapeDonut(6.0f, 40.0f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.StraySprayP4Puddle) or ((uint)AID.StraySprayP4Donut))
        {
            aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            NumCasts++;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Tsunami(BossModule module) : Components.GenericBaitProximity(module, onlyShowOutlines: true)
{
    private readonly TsunamiInfernoOrder? tsunamiInfernoOrder = module.FindComponent<TsunamiInfernoOrder>();
    public bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.StraySprayP4Puddle) or ((uint)AID.StraySprayP4Donut))
        {
            CurrentBaits.Clear();
            active = true;
        }
    }

    public override void Update()
    {
        if (tsunamiInfernoOrder == null || active)
        {
            return;
        }

        var players = tsunamiInfernoOrder.GetNextBuffPlayers((uint)SID.DynamicFluidP4, 8);
        if (players.Count == 0)
        {
            return;
        }

        CurrentBaits.Clear();

        foreach (var (slot, expireAt, tellingTruth) in players)
        {
            var player = Raid[slot];
            if (player == null)
            {
                continue;
            }

            // Real
            if (tellingTruth == true)
            {
                CurrentBaits.Add(new(player.Position, new AOEShapeDonut(6.0f, 40.0f), expireAt));
            }

            // Fake
            if (tellingTruth == false)
            {
                CurrentBaits.Add(new(player.Position, new AOEShapeCircle(6.0f), expireAt));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!active)
        {
            Arena.ZoneCircleOutline(new WPos(100.0f, 100.0f), 1.0f, Colors.Safe, 2.0f);
        }
    }
}

sealed class UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, (uint)AID.UltimaUpsurge);

// Customize version of P1 lightning safe spots function that uses the truth and lies stored throughout the phase instead
sealed class P4LightningSafeSpots(BossModule module) : LightningSafeSpots(module)
{
    private readonly KefkaOrder? kefkaOrder = module.FindComponent<KefkaOrder>();

    public override void Update()
    {
        if (kefkaOrder == null)
        {
            return;
        }

        var lightning = kefkaOrder.tellingTruthOrder.Where(e => e.element == KefkaOrder.Element.Thunder).ToList();
        if (lightning.Count != 2)
        {
            return;
        }

        questionMark = lightning[0].tellingTruth != lightning[1].tellingTruth;
    }
}
