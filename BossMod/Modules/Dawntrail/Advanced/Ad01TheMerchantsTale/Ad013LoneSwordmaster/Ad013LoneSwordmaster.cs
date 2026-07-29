namespace BossMod.Dawntrail.Advanced.Ad01MerchantsTale.Ad013LoneSwordmaster;

sealed class DebuffTracker(BossModule module) : BossComponent(module)
{
    // any times when attacks cast without debuffs? necessary to track status lost?
    // what happens when status lost and status gained on same frame? (E -> N-E)
    // coinflip on whether new status sticks or lost; check remaining time before reseting status
    // think of a cleaner way to get unsafe debuffs/angles instead of hardcoding everything
    public enum Direction
    {
        North,
        East,
        West,
        South
    }
    public uint[] Malefic = new uint[4];

    public uint[] GetDebuffsInDirections(Direction direction)
    {
        return direction switch
        {
            Direction.North => [(uint)SID.MaleficN, (uint)SID.MaleficNE, (uint)SID.MaleficNW, (uint)SID.MaleficNEW, (uint)SID.MaleficNS, (uint)SID.MaleficNSE, (uint)SID.MaleficNSW, (uint)SID.MaleficNSEW],
            Direction.East => [(uint)SID.MaleficE, (uint)SID.MaleficEW, (uint)SID.MaleficSE, (uint)SID.MaleficSEW, (uint)SID.MaleficNE, (uint)SID.MaleficNEW, (uint)SID.MaleficNSE, (uint)SID.MaleficNSEW],
            Direction.West => [(uint)SID.MaleficW, (uint)SID.MaleficEW, (uint)SID.MaleficSW, (uint)SID.MaleficSEW, (uint)SID.MaleficNW, (uint)SID.MaleficNEW, (uint)SID.MaleficNSW, (uint)SID.MaleficNSEW],
            Direction.South => [(uint)SID.MaleficS, (uint)SID.MaleficSE, (uint)SID.MaleficSW, (uint)SID.MaleficSEW, (uint)SID.MaleficNS, (uint)SID.MaleficNSE, (uint)SID.MaleficNSW, (uint)SID.MaleficNSEW],
            _ => []
        };
    }

    public Angle[] GetAngles(int slot)
    {
        if (slot < 0 || slot > 3)
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
            (uint)SID.MaleficSEW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficN => [Angle.AnglesCardinals[2]],
            (uint)SID.MaleficNE => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNEW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNS => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[1]],
            (uint)SID.MaleficNSE => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[1], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNSW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[1], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNSEW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[1], Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]],
            _ => []
        };

        return angles;
    }
    public Angle[] GetUnsafeAngles(int slot)
    {
        if (slot < 0 || slot > 3)
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
            (uint)SID.MaleficSEW => [Angle.AnglesCardinals[2], Angle.AnglesCardinals[0], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficN => [Angle.AnglesCardinals[1]],
            (uint)SID.MaleficNE => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNEW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[0], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNS => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[2]],
            (uint)SID.MaleficNSE => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[2], Angle.AnglesCardinals[0]],
            (uint)SID.MaleficNSW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[2], Angle.AnglesCardinals[3]],
            (uint)SID.MaleficNSEW => [Angle.AnglesCardinals[1], Angle.AnglesCardinals[2], Angle.AnglesCardinals[0], Angle.AnglesCardinals[3]],
            _ => []
        };

        return angles;
    }

    public Angle? GetUnyieldingAngle(int slot, bool sourceNorthSouth)
    {
        if (slot < 0 || slot > 3)
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
        if (status.ID >= (uint)SID.MaleficE && status.ID <= (uint)SID.MaleficNSEW)
        {
            Malefic[Raid.FindSlot(actor.InstanceID)] = status.ID;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID >= (uint)SID.MaleficE && status.ID <= (uint)SID.MaleficNSEW)
        {
            var remaining = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
            if (remaining < 2d)
            {
                Malefic[Raid.FindSlot(actor.InstanceID)] = 0;
            }
        }
    }
}
sealed class SteelsbreathRelease(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SteelsbreathRelease, (uint)AID.SteelsbreathReleaseArena]);
sealed class LashOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LashOfLight, new AOEShapeCone(40f, 45f.Degrees()), 2);
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

sealed class NearFarFromHeaven(BossModule module) : Components.UniformStackSpread(module, 5f, 5f)
{
    // icon before cast starts
    // target random if icon player dies? worth checking on update?
    private Actor? _target = null;
    private DateTime _activation = default;
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Lockon)
        {
            _target = actor;
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_target == null)
            return;

        if (spell.Action.ID is (uint)AID.NearToHeavenSoloCast or (uint)AID.FarFromHeavenSoloCast)
        {
            _activation = Module.CastFinishAt(spell);
            AddSpread(_target, _activation);
        }
        else if (spell.Action.ID is (uint)AID.NearToHeavenMultiCast or (uint)AID.FarFromHeavenMultiCast)
        {
            _activation = Module.CastFinishAt(spell);
            AddStack(_target, _activation);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NearToHeavenSolo or (uint)AID.NearToHeavenMulti or (uint)AID.FarFromHeavenSolo or (uint)AID.FarFromHeavenMulti)
        {
            _target = null;
            _activation = default;
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}
sealed class WolfsCrossing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolfsCrossing, new AOEShapeCross(40f, 4f));
// 4 at a time, leaves puddles, happens 4 times, echoing eight happen at position of 1st 3 sets (up to 12 spots)
sealed class EchoingHush(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.EchoingHush, (uint)AID.EchoingHush1], 8f);
sealed class EchoingHushPuddle(BossModule module) : Components.Voidzone(module, 8f, GetPuddles)
{
    public static Actor[] GetPuddles(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.EchoingHushPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.Renderflags == 0)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class EchoingEight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoingEight, new AOEShapeCross(40f, 4f));
sealed class StingOfTheScorpion(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StingOfTheScorpion);
sealed class WaitingWounds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaitingWounds, 10f, 6);
sealed class SteelsbreathReleaseArena(BossModule module) : Components.GenericAOEs(module)
{
    // spawns 4 10x5 puddles long edge
    // always spawn in same locations?
    private readonly AOEShapeRect _shape = new(20f, 5f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.SteelsbreathPuddles)
        {
            if (state == 0x00010002)
            {
                _aoes.Add(new(_shape, Arena.Center + new WDir(0f, -15f), 90f.Degrees()));
                _aoes.Add(new(_shape, Arena.Center + new WDir(15f, 0f), 0f.Degrees()));
                _aoes.Add(new(_shape, Arena.Center + new WDir(0f, 15f), -90f.Degrees()));
                _aoes.Add(new(_shape, Arena.Center + new WDir(-15f, 0f), 180f.Degrees()));
            }
            else if (state == 0x00040008)
            {
                _aoes.Clear();
            }
        }
    }
}
sealed class ChainTether(BossModule module) : Components.StretchTetherDuo(module, 20f, 30d, (uint)TetherID.Chains)
{
    // tether break distance dependent on when tether first gets attached?
    // can't tell who gets chained to who early since everyone gets icon
    // stack together until tethers go off then move to safe spots
}
sealed class MaleficPortent(BossModule module) : Components.CastCounter(module, (uint)AID.MaleficPortent)
{
    // during 1st time, untethered players can grab either tether
    // during 1st time as 4-man, can 1 player take both tethers if both sides safe? ever the case?
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private readonly Dictionary<Actor, uint> _tethers = [];
    private BitMask _tethered = default;
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (uint)TetherID.MaleficN and not (uint)TetherID.MaleficE and not (uint)TetherID.MaleficW and not (uint)TetherID.MaleficS)
            return;

        var slot = Raid.FindSlot(source.InstanceID);
        if (slot == -1)
            return;

        _tethers[source] = tether.ID;
        _tethered.Set(slot);
    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (uint)TetherID.MaleficN and not (uint)TetherID.MaleficE and not (uint)TetherID.MaleficW and not (uint)TetherID.MaleficS)
            return;

        if (!_tethers.ContainsKey(source))
            return;

        var slot = Raid.FindSlot(source.InstanceID);
        if (slot == -1)
            return;

        _tethered.Clear(slot);
        _tethers.Remove(source);
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MaleficPortent)
        {
            _tethers.Clear();
            _tethered.Reset();
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var keys = _tethers.Keys.ToArray();
        var count = keys.Length;
        if (count == 0)
            return;

        if (_tethered[slot])
        {
            var tether = _tethers[actor];
            if (!IsPlayerSafe(actor, tether))
            {
                hints.Add("Pass tether to another player!");
            }
        }
        else
        {
            bool canIntercept = false;
            for (var i = 0; i < count; i++)
            {
                var player = keys[i];
                var tether = _tethers[player];
                if (!IsPlayerSafe(player, tether))
                {
                    if (IsPlayerSafe(actor, tether))
                    {
                        canIntercept = true;
                        break;
                    }
                }
            }

            if (canIntercept && !_tethered[slot])
            {
                hints.Add("Grab the tether!");
            }
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var keys = _tethers.Keys.ToArray();
        var count = keys.Length;
        if (count == 0)
            return;
        /*
        for (var i = 0; i < count; i++)
        {
            var player = keys[i];
            var tether = _tethers[player];
            var isSafe = IsPlayerSafe(player, tether);
            Arena.AddLine(Module.PrimaryActor.Position, player.Position, isSafe ? Colors.Safe : default);

            if (!isSafe)
            {
                Arena.ZoneCircleOutline(player.Position, 1.5f);
            }
        }
        */
        if (_tethered[pcSlot])
        {
            var tether = _tethers[pc];
            var isTetherSafe = IsPlayerSafe(pc, tether);
            Arena.AddLine(Module.PrimaryActor.Position, pc.Position, isTetherSafe ? Colors.Safe : default);

            if (!isTetherSafe)
            {
                // add circle around player to pass to
                for (var i = 0; i < Raid.Members.Length; i++)
                {
                    if (i == pcSlot)
                        continue;

                    var player = Raid[i];
                    if (player == null)
                        continue;

                    // with 4man 1st time 2 players are safe to pass
                    if (IsPlayerSafe(player, tether))
                    {
                        Arena.ZoneCircleOutline(player.Position, 1.5f);
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                var player = keys[i];
                var tether = _tethers[player];
                var isTetherSafe = IsPlayerSafe(player, tether);

                if (!isTetherSafe && IsPlayerSafe(pc, tether))
                {
                    Arena.AddLine(Module.PrimaryActor.Position, player.Position);
                    Arena.ZoneCircleOutline(player.Position, 1.5f);
                }
            }
        }
    }
    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!_tethered[playerSlot])
            return PlayerPriority.Irrelevant;

        if (!_tethers.ContainsKey(player))
            return PlayerPriority.Irrelevant;

        var tetherID = _tethers[player];
        return IsPlayerSafe(player, tetherID) ? PlayerPriority.Normal : IsPlayerSafe(pc, tetherID) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }
    private bool IsPlayerSafe(Actor pc, uint tetherID)
    {
        // each tether (N/E/W/S) can have up to 8 bad statuses
        var slot = Raid.FindSlot(pc.InstanceID);
        if (slot == -1)
            return true;

        var malefic = _debuffs.Malefic[slot];
        var unsafeIDs = tetherID switch
        {
            (uint)TetherID.MaleficN => _debuffs.GetDebuffsInDirections(DebuffTracker.Direction.North),
            (uint)TetherID.MaleficE => _debuffs.GetDebuffsInDirections(DebuffTracker.Direction.East),
            (uint)TetherID.MaleficW => _debuffs.GetDebuffsInDirections(DebuffTracker.Direction.West),
            (uint)TetherID.MaleficS => _debuffs.GetDebuffsInDirections(DebuffTracker.Direction.South),
            _ => []
        };

        var count = unsafeIDs.Length;
        for (var i = 0; i < count; i++)
        {
            if (malefic == unsafeIDs[i])
                return false;
        }

        return true;
    }
}
sealed class MaleficAlignment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MaleficAlignment, new AOEShapeCone(40f, 45f.Degrees()))
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
/*
sealed class WillOfTheUnderworld(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WillOfTheUnderworld, new AOEShapeRect(40f, 10f))
{
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
*/
class GenericWillUnderworld(BossModule module, uint aid, AOEShape shape) : Components.SimpleAOEs(module, aid, shape)
{
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
sealed class WillOfTheUnderworld(BossModule module) : GenericWillUnderworld(module, (uint)AID.WillOfTheUnderworld, new AOEShapeRect(40f, 10f));
sealed class WillOfTheUnderworld1(BossModule module) : GenericWillUnderworld(module, (uint)AID.WillOfTheUnderworld1, new AOEShapeRect(40f, 5f));
sealed class SilentEight(BossModule module) : Components.GenericBaitAway(module)
{
    // 8f circle AOE followed up echoing eight at player locations
    // general strat each player takes a corner
    // draw outlines for both circle and followup crosses? if drawing crosses need to ignore hints and AI for clipped players on crosses only
    // make AI run towards nearest corner if there are no other players closer
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SilentEight)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(8f), WorldState.FutureTime(5d)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ResoundingSilence)
        {
            CurrentBaits.Clear();
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (ActiveBaits.Count == 0)
            return;

        Arena.ZoneCircleOutline(Arena.Center - new WDir(-18f, -18f), 2f);
        Arena.ZoneCircleOutline(Arena.Center - new WDir(-18f, 18f), 2f);
        Arena.ZoneCircleOutline(Arena.Center - new WDir(18f, -18f), 2f);
        Arena.ZoneCircleOutline(Arena.Center - new WDir(18f, 18f), 2f);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(Ad013LoneSwordmasterStates),
ConfigType = null, // replace null with typeof(LoneSwordmasterConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.LoneSwordmaster,
Contributors = "",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1084u,
NameID = 14323u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -815f), new ArenaBoundsSquare(20f));
