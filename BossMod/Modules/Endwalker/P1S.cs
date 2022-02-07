using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BossMod
{
    public class P1S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x3522,
            Helper = 0x233C,
            FlailLR = 0x3523, // "anchor" weapon, purely visual
            FlailI = 0x3524, // "ball" weapon, also used for knockbacks
            FlailO = 0x3525, // "chakram" weapon
        };

        public enum AID : uint
        {
            GaolerFlailRL = 26102, // Boss->Boss
            GaolerFlailLR = 26103, // Boss->Boss
            GaolerFlailIO1 = 26104, // Boss->Boss
            GaolerFlailIO2 = 26105, // Boss->Boss
            GaolerFlailOI1 = 26106, // Boss->Boss
            GaolerFlailOI2 = 26107, // Boss->Boss
            AetherflailRX = 26114, // Boss->Boss -- seen BlueRI & RedRO
            AetherflailLX = 26115, // Boss->Boss -- seen BlueLO, RedLI, RedLO - maybe it's *L*?
            AetherflailIL = 26116, // never seen one, inferred
            AetherflailIR = 26117, // Boss->Boss -- RedIR
            AetherflailOL = 26118, // Boss->Boss -- seen BlueOL, RedOL
            AetherflailOR = 26119, // Boss->Boss -- seen RedOR
            KnockbackGrace = 26126, // Boss->MT
            KnockbackPurge = 26127, // Boss->MT
            TrueHoly1 = 26128, // Boss->Boss, no cast, ???
            TrueFlare1 = 26129, // Boss->Boss, no cast, ???
            TrueHoly2 = 26130, // Helper->tank shared, no cast, damage after KnockbackGrace (range=6)
            TrueFlare2 = 26131, // Helper->tank and nearby, no cast, damage after KnockbackPurge (range=50??)
            ShiningCells = 26134, // Boss->Boss, raidwide aoe
            SlamShut = 26135, // Boss->Boss, raidwide aoe
            Aetherchain = 26137, // Boss->Boss
            PowerfulFire = 26138, // Helper->???, no cast, damage during aetherflails for incorrect segments?..
            ShacklesOfTime = 26140, // Boss->Boss
            OutOfTime = 26141, // Helper->???, no cast, after SoT resolve
            Intemperance = 26142, // Boss->Boss
            IntemperateTormentUp = 26143, // Boss->Boss (bottom->top)
            IntemperateTormentDown = 26144, // Boss->Boss (bottom->top)
            HotSpell = 26145, // Helper->player, no cast, red cube explosion
            ColdSpell = 26146, // Helper->player, no cast, blue cube explosion
            DisastrousSpell = 26147, // Helper->player, no cast, purple cube explosion
            PainfulFlux = 26148, // Helper->player, no cast, separator cube explosion
            AetherialShackles = 26149, // Boss->Boss
            FourShackles = 26150, // Boss->Boss
            ChainPainBlue = 26151, // Helper->chain target, no cast, damage during chain resolve
            ChainPainRed = 26152, // Helper->chain target
            HeavyHand = 26153, // Boss->MT, generic tankbuster
            WarderWrath = 26154, // Boss->Boss, generic raidwide
            GaolerFlailR1 = 28070, // Helper->Helper, first hit, right-hand cone
            GaolerFlailL1 = 28071, // Helper->Helper, first hit, left-hand cone
            GaolerFlailI1 = 28072, // Helper->Helper, first hit, point-blank
            GaolerFlailO1 = 28073, // Helper->Helper, first hit, donut
            GaolerFlailR2 = 28074, // Helper->Helper, second hit, right-hand cone
            GaolerFlailL2 = 28075, // Helper->Helper, second hit, left-hand cone
            GaolerFlailI2 = 28076, // Helper->Helper, second hit, point-blank
            GaolerFlailO2 = 28077, // Helper->Helper, second hit, donut
            InevitableFlame = 28353, // Helper->Helper no cast, after SoT resolve to red - hit others standing in fire?
        };

        public enum SID : uint
        {
            AetherExplosion = 2195, // hidden and unnamed, 'stacks' parameter determines red/blue segments explosion (0x4D for blue, 0x4C for red)
            ColdSpell = 2739, // intemperance: after blue cube explosion
            HotSpell = 2740, // intemperance: after red cube explosion
            ShacklesOfTime = 2741, // shackles of time: hits segments matching color on expiration
            ShacklesOfCompanionship0 = 2742, // shackles: purple (tether to 3 closest) - normal 13s duration
            ShacklesOfLoneliness0 = 2743, // shackles: red (tether to 3 farthest) - normal 13s duration
            InescapableCompanionship = 2744, // replaces corresponding shackles in 13s, 5s duration
            InescapableLoneliness = 2745,
            ShacklesOfCompanionship1 = 2885, // fourfold 3s duration
            ShacklesOfCompanionship2 = 2886, // fourfold 8s duration
            ShacklesOfCompanionship3 = 2887, // fourfold 13s duration
            ShacklesOfLoneliness1 = 2888, // fourfold 3s duration
            ShacklesOfLoneliness2 = 2889, // fourfold 8s duration
            ShacklesOfLoneliness3 = 2890, // fourfold 13s duration
            ShacklesOfCompanionship4 = 2923, // fourfold 18s duration
            ShacklesOfLoneliness4 = 2924, // fourfold 18s duration
            DamageDown = 2911, // applied by two successive cubes of the same color
            MagicVulnerabilityUp = 2941, // applied by shackle resolve, knockbacks
        }

        private static float _innerCircleRadius = 12; // this determines in/out flails and cells boundary

        // state related to normal and fourfold shackles
        private class Shackles : Component
        {
            private P1S _module;
            private bool _active = false;
            private byte _debuffsBlueImminent = 0;
            private byte _debuffsBlueFuture = 0;
            private byte _debuffsRedImminent = 0;
            private byte _debuffsRedFuture = 0;
            private ulong _blueTetherMatrix = 0;
            private ulong _redTetherMatrix = 0; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
            private ulong _blueExplosionMatrix = 0;
            private ulong _redExplosionMatrix = 0; // bit (8*i+j) is set if player i is inside explosion of player j; bit [i,i] is never set

            private static float _blueExplosionRadius = 4;
            private static float _redExplosionRadius = 8;
            private static uint TetherColor(bool blue, bool red) => blue ? (red ? 0xff00ffff : 0xffff0080) : 0xff8080ff;

            public Shackles(P1S module)
            {
                _module = module;
            }

            public int NumDebuffs() => BitOperations.PopCount((uint)_debuffsBlueFuture | _debuffsBlueImminent | _debuffsRedFuture | _debuffsRedImminent);

            public override void Reset() => _debuffsBlueFuture = _debuffsBlueImminent = _debuffsRedFuture = _debuffsRedImminent = 0;

            public override void Update()
            {
                _blueTetherMatrix = _redTetherMatrix = _blueExplosionMatrix = _redExplosionMatrix = 0;
                byte blueDebuffs = (byte)(_debuffsBlueImminent | _debuffsBlueFuture);
                byte redDebuffs = (byte)(_debuffsRedImminent | _debuffsRedFuture);
                _active = (blueDebuffs | redDebuffs) != 0;
                if (!_active)
                    return; // nothing to do...

                // update tether matrices
                foreach ((int iSrc, var src) in _module.IterateRaidMembers())
                {
                    // blue => 3 closest
                    if (BitVector.IsVector8BitSet(blueDebuffs, iSrc))
                    {
                        BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iSrc, iSrc, true);
                        foreach ((int iTgt, _) in _module.IterateOtherRaidMembers(iSrc).SortedByRange(src.Position).Take(3))
                            BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iTgt, iSrc, true);
                    }

                    // red => 3 furthest
                    if (BitVector.IsVector8BitSet(redDebuffs, iSrc))
                    {
                        BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iSrc, iSrc, true);
                        foreach ((int iTgt, _) in _module.IterateOtherRaidMembers(iSrc).SortedByRange(src.Position).TakeLast(3))
                            BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iTgt, iSrc, true);
                    }
                }

                // update explosion matrices and detect problems (has to be done in a separate pass)
                for (int i = 0; i < _module.RaidMembers.Length; ++i)
                {
                    if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i) != 0)
                        foreach ((int j, _) in _module.IterateRaidMembersInRange(i, _blueExplosionRadius))
                            BitVector.SetMatrix8x8Bit(ref _blueExplosionMatrix, j, i, true);

                    if (BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i) != 0)
                        foreach ((int j, _) in _module.IterateRaidMembersInRange(i, _redExplosionRadius))
                            BitVector.SetMatrix8x8Bit(ref _redExplosionMatrix, j, i, true);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, slot) != 0 && BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, slot) != 0)
                {
                    hints.Add("Target of two tethers!");
                }
                if (BitVector.ExtractVectorFromMatrix8x8(_blueExplosionMatrix, slot) != 0 || BitVector.ExtractVectorFromMatrix8x8(_redExplosionMatrix, slot) != 0)
                {
                    hints.Add("GTFO from explosion!");
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!_active || pc == null)
                    return;

                bool drawBlueAroundMe = false;
                bool drawRedAroundMe = false;
                foreach ((int i, var actor) in _module.IterateRaidMembers())
                {
                    // draw tethers
                    var blueTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i);
                    var redTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i);
                    var tetherMask = (byte)(blueTetheredTo | redTetheredTo);
                    if (tetherMask != 0)
                    {
                        arena.Actor(actor, TetherColor(blueTetheredTo != 0, redTetheredTo != 0));
                        foreach ((int j, var target) in _module.IterateRaidMembers(true))
                        {
                            if (i != j && BitVector.IsVector8BitSet(tetherMask, j))
                            {
                                arena.AddLine(actor.Position, target.Position, TetherColor(BitVector.IsVector8BitSet(blueTetheredTo, j), BitVector.IsVector8BitSet(redTetheredTo, j)));
                            }
                        }
                    }

                    // draw explosion circles that hit me
                    if (BitVector.IsMatrix8x8BitSet(_blueExplosionMatrix, _module.PlayerSlot, i))
                        arena.AddCircle(actor.Position, _blueExplosionRadius, arena.ColorDanger);
                    if (BitVector.IsMatrix8x8BitSet(_redExplosionMatrix, _module.PlayerSlot, i))
                        arena.AddCircle(actor.Position, _redExplosionRadius, arena.ColorDanger);

                    drawBlueAroundMe |= BitVector.IsMatrix8x8BitSet(_blueExplosionMatrix, i, _module.PlayerSlot);
                    drawRedAroundMe |= BitVector.IsMatrix8x8BitSet(_redExplosionMatrix, i, _module.PlayerSlot);
                }

                // draw explosion circles if I hit anyone
                if (drawBlueAroundMe)
                    arena.AddCircle(pc.Position, _blueExplosionRadius, arena.ColorDanger);
                if (drawRedAroundMe)
                    arena.AddCircle(pc.Position, _redExplosionRadius, arena.ColorDanger);
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.ShacklesOfCompanionship0:
                    case SID.ShacklesOfCompanionship1:
                    case SID.ShacklesOfCompanionship2:
                    case SID.ShacklesOfCompanionship3:
                    case SID.ShacklesOfCompanionship4:
                        ModifyDebuff(actor, ref _debuffsBlueFuture, true);
                        break;
                    case SID.ShacklesOfLoneliness0:
                    case SID.ShacklesOfLoneliness1:
                    case SID.ShacklesOfLoneliness2:
                    case SID.ShacklesOfLoneliness3:
                    case SID.ShacklesOfLoneliness4:
                        ModifyDebuff(actor, ref _debuffsRedFuture, true);
                        break;
                    case SID.InescapableCompanionship:
                        ModifyDebuff(actor, ref _debuffsBlueImminent, true);
                        break;
                    case SID.InescapableLoneliness:
                        ModifyDebuff(actor, ref _debuffsRedImminent, true);
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.ShacklesOfCompanionship0:
                    case SID.ShacklesOfCompanionship1:
                    case SID.ShacklesOfCompanionship2:
                    case SID.ShacklesOfCompanionship3:
                    case SID.ShacklesOfCompanionship4:
                        ModifyDebuff(actor, ref _debuffsBlueFuture, false);
                        break;
                    case SID.ShacklesOfLoneliness0:
                    case SID.ShacklesOfLoneliness1:
                    case SID.ShacklesOfLoneliness2:
                    case SID.ShacklesOfLoneliness3:
                    case SID.ShacklesOfLoneliness4:
                        ModifyDebuff(actor, ref _debuffsRedFuture, false);
                        break;
                    case SID.InescapableCompanionship:
                        ModifyDebuff(actor, ref _debuffsBlueImminent, false);
                        break;
                    case SID.InescapableLoneliness:
                        ModifyDebuff(actor, ref _debuffsRedImminent, false);
                        break;
                }
            }

            private void ModifyDebuff(WorldState.Actor actor, ref byte vector, bool active)
            {
                int slot = _module.FindRaidMemberSlot(actor.InstanceID);
                if (slot >= 0)
                    BitVector.ModifyVector8Bit(ref vector, slot, active);
            }
        }

        // state related to aether explosion mechanics, done as part of aetherflails, aetherchain and shackles of time abilities
        private class AetherExplosion : Component
        {
            private enum Cell { None, Red, Blue }

            private P1S _module;
            private int _memberWithSOT = -1; // if not -1, then every update exploding cells are recalculated based on this raid member's position
            private Cell _explodingCells = Cell.None;

            private static uint _colorSOTActor = 0xff8080ff;

            public AetherExplosion(P1S module)
            {
                _module = module;
            }

            public bool SOTActive => _memberWithSOT >= 0;

            public override void Reset()
            {
                _memberWithSOT = -1;
                _explodingCells = Cell.None;
            }

            public override void Update()
            {
                var sotActor = _module.RaidMember(_memberWithSOT);
                if (sotActor != null)
                    _explodingCells = CellFromOffset(sotActor.Position - _module.Arena.WorldCenter);
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (slot != _memberWithSOT && _explodingCells != Cell.None && _explodingCells == CellFromOffset(actor.Position - _module.Arena.WorldCenter))
                {
                    hints.Add("Hit by aether explosion!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_explodingCells == Cell.None || _module.PlayerSlot == _memberWithSOT)
                    return; // nothing to draw

                if (!_module.Arena.IsCircle)
                {
                    Service.Log("[P1S] Trying to draw aether AOE when cells mode is not active...");
                    return;
                }

                float start = _explodingCells == Cell.Blue ? 0 : MathF.PI / 4;
                for (int i = 0; i < 4; ++i)
                {
                    arena.ZoneCone(arena.WorldCenter, 0, _innerCircleRadius, start, start + MathF.PI / 4, arena.ColorAOE);
                    arena.ZoneCone(arena.WorldCenter, _innerCircleRadius, arena.WorldHalfSize, start + MathF.PI / 4, start + MathF.PI / 2, arena.ColorAOE);
                    start += MathF.PI / 2;
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (_memberWithSOT != _module.PlayerSlot)
                    arena.Actor(_module.RaidMember(_memberWithSOT), _colorSOTActor);
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.AetherExplosion:
                        if (_memberWithSOT != -1)
                        {
                            Service.Log($"[P1S] Unexpected forced explosion while SOT is active");
                            _memberWithSOT = -1;
                        }
                        if (actor != _module.Boss())
                            Service.Log($"[P1S] Unexpected aether explosion status on {Utils.ObjectString(actor.InstanceID)}");
                        // we rely on parameter of an invisible status on boss to detect red/blue
                        switch (actor.Statuses[index].StackCount)
                        {
                            case 0x4C:
                                _explodingCells = Cell.Red;
                                break;
                            case 0x4D:
                                _explodingCells = Cell.Blue;
                                break;
                            default:
                                Service.Log($"[P1S] Unexpected aether explosion param {actor.Statuses[index].StackCount:X2}");
                                break;
                        }
                        break;

                    case SID.ShacklesOfTime:
                        if (_memberWithSOT >= 0)
                            Service.Log($"[P1S] Unexpected ShacklesOfTime: another is already active!");
                        _memberWithSOT = _module.FindRaidMemberSlot(actor.InstanceID);
                        _explodingCells = Cell.None;
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.AetherExplosion:
                        Reset();
                        break;

                    case SID.ShacklesOfTime:
                        if (_module.RaidMember(_memberWithSOT) == actor)
                            _memberWithSOT = -1;
                        _explodingCells = Cell.None;
                        break;
                }
            }

            private static Cell CellFromOffset(Vector3 offsetFromCenter)
            {
                if (offsetFromCenter.X == 0 && offsetFromCenter.Z == 0)
                    return Cell.None;

                var phi = MathF.Atan2(offsetFromCenter.Z, offsetFromCenter.X) + MathF.PI;
                int coneIndex = (int)(4 * phi / MathF.PI); // phi / (pi/4); range [0, 8]
                bool oddCone = (coneIndex & 1) != 0;
                bool outerCone = !GeometryUtils.PointInCircle(offsetFromCenter, _innerCircleRadius);
                return (oddCone != outerCone) ? Cell.Blue : Cell.Red; // inner odd = blue, outer even = blue
            }
        }

        // state related to [aether]flails mechanics
        private class Flails : Component
        {
            public enum Zone { None, Left, Right, Inner, Outer, UnknownCircle }

            public int NumCasts { get; private set; } = 0;

            private P1S _module;
            private List<WorldState.Actor> _weaponsBall;
            private List<WorldState.Actor> _weaponsChakram;
            private Zone _first = Zone.None;
            private Zone _second = Zone.None;
            private bool _showFirst = false;
            private bool _showSecond = false;

            private static float _coneHalfAngle = MathF.PI / 4;

            public Flails(P1S module)
            {
                _module = module;
                _weaponsBall = module.Enemies(OID.FlailI);
                _weaponsChakram = module.Enemies(OID.FlailO);
            }

            public void Start(Zone first, Zone second)
            {
                NumCasts = 0;
                _first = first;
                _second = second;
                _showFirst = true;
                _showSecond = (first == Zone.Left || first == Zone.Right) != (second == Zone.Left || second == Zone.Right);
            }

            public override void Reset()
            {
                NumCasts = 0;
                _first = _second = Zone.None;
                _showFirst = _showSecond = false;
            }

            public override void Update()
            {
                // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
                // these can appear few frames later...
                if (_second == Zone.UnknownCircle && _weaponsBall.Count + _weaponsChakram.Count > 0)
                {
                    if (_weaponsBall.Count > 0 && _weaponsChakram.Count > 0)
                    {
                        Service.Log($"[P1S] Failed to determine second aetherflail: there are {_weaponsBall.Count} balls and {_weaponsChakram.Count} chakrams");
                    }
                    else
                    {
                        _second = _weaponsBall.Count > 0 ? Zone.Inner : Zone.Outer;
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                var boss = _module.Boss();
                if (boss == null)
                    return;

                if (_showFirst && IsInAOE(actor.Position, boss, _first))
                    hints.Add("Hit by first flail!");
                if (_showSecond && IsInAOE(actor.Position, boss,_second))
                    hints.Add("Hit by second flail!");
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (boss == null)
                    return;

                if (_showFirst)
                    DrawZone(arena, boss, _first);
                if (_showSecond)
                    DrawZone(arena, boss, _second);
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.GaolerFlailR1:
                    case AID.GaolerFlailL1:
                    case AID.GaolerFlailI1:
                    case AID.GaolerFlailO1:
                        ++NumCasts;
                        _showFirst = false;
                        _showSecond = true;
                        break;
                    case AID.GaolerFlailR2:
                    case AID.GaolerFlailL2:
                    case AID.GaolerFlailI2:
                    case AID.GaolerFlailO2:
                        ++NumCasts;
                        _showSecond = false;
                        break;
                }
            }

            private static bool IsInAOE(Vector3 pos, WorldState.Actor boss, Zone zone)
            {
                switch (zone)
                {
                    case Zone.Left:
                        return !GeometryUtils.PointInCone(pos - boss.Position, boss.Rotation - MathF.PI / 2, _coneHalfAngle);
                    case Zone.Right:
                        return !GeometryUtils.PointInCone(pos - boss.Position, boss.Rotation + MathF.PI / 2, _coneHalfAngle);
                    case Zone.Inner:
                        return GeometryUtils.PointInCircle(pos - boss.Position, _innerCircleRadius);
                    case Zone.Outer:
                        return !GeometryUtils.PointInCircle(pos - boss.Position, _innerCircleRadius);
                }
                return false;
            }

            private static void DrawZone(MiniArena arena, WorldState.Actor boss, Zone zone)
            {
                switch (zone)
                {
                    case Zone.Left:
                        arena.ZoneCone(boss.Position, 0, 100, boss.Rotation - MathF.PI / 2 + _coneHalfAngle, boss.Rotation + 3 * MathF.PI / 2 - _coneHalfAngle, arena.ColorAOE);
                        break;
                    case Zone.Right:
                        arena.ZoneCone(boss.Position, 0, 100, boss.Rotation + MathF.PI / 2 - _coneHalfAngle, boss.Rotation - 3 * MathF.PI / 2 + _coneHalfAngle, arena.ColorAOE);
                        break;
                    case Zone.Inner:
                        arena.ZoneCircle(boss.Position, _innerCircleRadius, arena.ColorAOE);
                        break;
                    case Zone.Outer:
                        arena.ZoneCone(boss.Position, _innerCircleRadius, 100, 0, 2 * MathF.PI, arena.ColorAOE);
                        break;
                }
            }
        }

        // state related to knockback + aoe mechanic
        // TODO: i'm not quite happy with implementation, consider revising...
        private class Knockback : Component
        {
            public bool AOEDone { get; private set; } = false;

            private P1S _module;
            private bool _active = false;
            private bool _isFlare = false; // true -> purge aka flare (stay away from MT), false -> grace aka holy (stack to MT)
            private WorldState.Actor? _knockbackTarget = null;
            private Vector3 _knockbackPos = new();

            private static float _kbDistance = 15;
            private static float _flareRange = 20; // this is tricky - range is actually 50, but it has some sort of falloff - not sure what is 'far enough'
            private static float _holyRange = 6;
            private static uint _colorAOETarget = 0xff8080ff;
            private static uint _colorVulnerable = 0xffff00ff;

            public Knockback(P1S module)
            {
                _module = module;
            }

            public void Start(uint knockbackTargetID, bool isFlare)
            {
                AOEDone = false;
                _active = true;
                _isFlare = isFlare;
                _knockbackTarget = _module.WorldState.FindActor(knockbackTargetID);
                if (_knockbackTarget == null)
                    Service.Log("[P1S] Failed to determine knockback target");
            }

            public override void Reset()
            {
                AOEDone = false;
                _active = false;
                _knockbackTarget = null;
            }

            public override void Update()
            {
                if (_knockbackTarget != null)
                {
                    _knockbackPos = _knockbackTarget.Position;
                    var boss = _module.Boss();
                    if (boss?.CastInfo != null)
                    {
                        _knockbackPos = AdjustPositionForKnockback(_knockbackPos, boss, _kbDistance);
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                var boss = _module.Boss();
                if (!_active || boss == null)
                    return;

                if (boss.CastInfo != null && actor == _knockbackTarget && !_module.Arena.InBounds(_knockbackPos))
                {
                    hints.Add("About to be knocked into wall!");
                }

                float aoeRange = _isFlare ? _flareRange : _holyRange;
                if (boss.TargetID == actor.InstanceID)
                {
                    // i'm the current tank - i should gtfo from raid if i'll get the flare -or- if i'm vulnerable (assuming i'll pop invul not to die)
                    if (RaidShouldStack(actor))
                    {
                        // check that raid is stacked on actor, except for vulnerable target (note that raid never stacks if knockback target is actor, since he is current aoe target)
                        if (_knockbackTarget != null && GeometryUtils.PointInCircle(actor.Position - _knockbackPos, aoeRange))
                        {
                            hints.Add("GTFO from co-tank!");
                        }
                        if (_module.IterateRaidMembersInRange(slot, aoeRange).Count() < 7)
                        {
                            hints.Add("Stack with raid!");
                        }
                    }
                    else
                    {
                        // check that raid is spread from actor
                        if (actor == _knockbackTarget)
                        {
                            hints.Add("Press invul!");
                        }
                        if (_module.IterateRaidMembersInRange(slot, aoeRange).Any())
                        {
                            hints.Add("GTFO from raid!");
                        }
                    }
                }
                else
                {
                    // i'm not the current tank - I should gtfo if tank is invul soaking, from flare or from holy if i'm vulnerable, otherwise stack to current tank
                    var target = _module.WorldState.FindActor(boss.TargetID);
                    if (target == null)
                        return;

                    if (RaidShouldStack(target) && actor != _knockbackTarget)
                    {
                        // check that actor is stacked with tank
                        if (!GeometryUtils.PointInCircle(actor.Position - target.Position, aoeRange))
                        {
                            hints.Add("Stack with target!");
                        }
                    }
                    else
                    {
                        // check that actor stays away from tank
                        var pos = actor == _knockbackTarget ? _knockbackPos : actor.Position;
                        if (GeometryUtils.PointInCircle(pos - target.Position, aoeRange))
                        {
                            hints.Add("GTFO from target!");
                        }
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                var boss = _module.Boss();
                if (!_active || pc == null || boss == null)
                    return;

                if (boss.CastInfo != null && pc == _knockbackTarget && pc.Position != _knockbackPos)
                {
                    arena.AddLine(pc.Position, _knockbackPos, arena.ColorDanger);
                    arena.Actor(_knockbackPos, pc.Rotation, arena.ColorDanger);
                }

                var target = _module.WorldState.FindActor(boss.TargetID);
                if (target == null)
                    return;

                var targetPos = target == _knockbackTarget ? _knockbackPos : target.Position;
                float aoeRange = _isFlare ? _flareRange : _holyRange;
                if (target == pc)
                {
                    // there will be AOE around me, draw all players to help with positioning - note that we use position adjusted for knockback
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                        arena.Actor(player, GeometryUtils.PointInCircle(player.Position - targetPos, aoeRange) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
                else
                {
                    // draw AOE source
                    arena.Actor(targetPos, target.Rotation, _colorAOETarget);
                }
                arena.AddCircle(targetPos, aoeRange, arena.ColorDanger);

                // draw vulnerable target
                if (_knockbackTarget != pc && _knockbackTarget != target)
                    arena.Actor(_knockbackTarget, _colorVulnerable);
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.TrueHoly2) || info.IsSpell(AID.TrueFlare2))
                    AOEDone = true;
            }

            // we assume that if boss target is the same as knockback target, it's a tank using invul, and so raid shouldn't stack
            private bool RaidShouldStack(WorldState.Actor bossTarget) =>  !_isFlare && _knockbackTarget != bossTarget;
        }

        // state related to intemperance mechanic
        // TODO: improve, it's now not really providing any useful hints...
        private class Intemperance : Component
        {
            public enum State { Inactive, Unknown, TopToBottom, BottomToTop }
            public enum Cube { None, R, B, P }

            public State CurState = State.Inactive;
            public int NumExplosions { get; private set; } = 0;

            private P1S _module;
            private Cube[] _cubes = new Cube[24]; // [3*i+j] corresponds to cell i [NW N NE E SE S SW W], cube j [bottom center top]

            private static float _cellHalfSize = 6;

            private static Cube[] _patternSymm = {
                Cube.R, Cube.P, Cube.R,
                Cube.B, Cube.R, Cube.B,
                Cube.R, Cube.P, Cube.R,
                Cube.R, Cube.P, Cube.B,
                Cube.R, Cube.P, Cube.R,
                Cube.B, Cube.B, Cube.B,
                Cube.R, Cube.P, Cube.R,
                Cube.R, Cube.P, Cube.B,
            };
            private static Cube[] _patternAsymm = {
                Cube.B, Cube.P, Cube.R,
                Cube.R, Cube.R, Cube.B,
                Cube.B, Cube.P, Cube.R,
                Cube.R, Cube.P, Cube.R,
                Cube.B, Cube.P, Cube.R,
                Cube.R, Cube.B, Cube.B,
                Cube.B, Cube.P, Cube.R,
                Cube.R, Cube.P, Cube.R,
            };
            private static Vector2[] _offsets = { new(-1, -1), new(0, -1), new(1, -1), new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) };

            public Intemperance(P1S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                NumExplosions = 0;
                Array.Fill(_cubes, Cube.None);
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState != State.Inactive)
                {
                    var pat = _cubes.SequenceEqual(_patternSymm) ? "symmetrical" : (_cubes.SequenceEqual(_patternAsymm) ? "asymmetrical" : "unknown");
                    hints.Add($"Order: {CurState}, pattern: {pat}.", false);
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                // draw cell delimiters
                Vector3 v1 = new(_cellHalfSize + 1, 0, arena.WorldHalfSize);
                Vector3 v2 = new(-_cellHalfSize, 0, arena.WorldHalfSize);
                arena.ZoneRect(arena.WorldCenter - v1, arena.WorldCenter + v2, arena.ColorAOE);
                arena.ZoneRect(arena.WorldCenter - v2, arena.WorldCenter + v1, arena.ColorAOE);

                v1 = new(v1.Z, 0, v1.X);
                v2 = new(v2.Z, 0, v2.X);
                arena.ZoneRect(arena.WorldCenter - v1, arena.WorldCenter + v2, arena.ColorAOE);
                arena.ZoneRect(arena.WorldCenter - v2, arena.WorldCenter + v1, arena.ColorAOE);

                // draw cubes on margins
                var drawlist = ImGui.GetWindowDrawList();
                var marginOffset = arena.ScreenHalfSize + arena.ScreenMarginSize / 2;
                for (int i = 0; i < 8; ++i)
                {
                    var center = arena.ScreenCenter + arena.RotatedCoords(_offsets[i] * marginOffset);
                    DrawTinyCube(drawlist, center + new Vector2(-3,  5), _cubes[3 * i + 0]);
                    DrawTinyCube(drawlist, center + new Vector2(-3,  0), _cubes[3 * i + 1]);
                    DrawTinyCube(drawlist, center + new Vector2(-3, -5), _cubes[3 * i + 2]);

                    drawlist.AddLine(center + new Vector2(4, -7), center + new Vector2(4, 7), 0xffffffff);
                    if (CurState != State.Unknown)
                    {
                        float lineDir = CurState == State.BottomToTop ? -1 : 1;
                        drawlist.AddLine(center + new Vector2(4, 7 * lineDir), center + new Vector2(2, 5 * lineDir), 0xffffffff);
                        drawlist.AddLine(center + new Vector2(4, 7 * lineDir), center + new Vector2(6, 5 * lineDir), 0xffffffff);
                    }
                }
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.PainfulFlux)) // this is convenient to rely on, since exactly 1 cast happens right after every explosion
                    ++NumExplosions;
            }

            public override void OnEventEnvControl(uint featureID, byte index, uint state)
            {
                // we get the following env-control messages:
                // 1. ~2.8s after 26142 cast, we get 25 EnvControl messages with featureID 800375A0
                // 2. first 24 correspond to cubes, in groups of three (bottom->top), in order: NW N NE E SE S SW W
                //    the last one (index 26) can be ignored, probably corresponds to oneshot border
                //    state corresponds to cube type (00020001 for red, 00800040 for blue, 20001000 for purple)
                //    so asymmetrical pattern is: BPR RRB BPR RPR BPR RBB BPR RPR
                //    and symmetrical pattern is: RPR BRB RPR RPB RPR BBB RPR RPB
                // 3. on each explosion, we get 8 191s, with type 00080004 for exploded red, 04000004 for exploded blue, 08000004 for exploded purple
                // 4. 3 sec before second & third explosion, we get 8 191s, with type 00200020 for preparing red, 02000200 for preparing blue, 80008000 for preparing purple
                if (featureID == 0x800375A0 && index < 24)
                {
                    switch (state)
                    {
                        case 0x00020001:
                            _cubes[index] = Cube.R;
                            break;
                        case 0x00800040:
                            _cubes[index] = Cube.B;
                            break;
                        case 0x20001000:
                            _cubes[index] = Cube.P;
                            break;
                    }
                }
            }

            private void DrawTinyCube(ImDrawListPtr drawlist, Vector2 center, Cube type)
            {
                Vector2 off = new(3);
                if (type != Cube.None)
                {
                    uint col = type == Cube.R ? 0xff0000ff : (type == Cube.B ? 0xffff0000 : 0xffff00ff);
                    drawlist.AddRectFilled(center - off, center + off, col);
                }
                drawlist.AddRect(center - off, center + off, 0xffffffff);
            }
        }

        private List<WorldState.Actor> _boss;
        private WorldState.Actor? Boss() => _boss.FirstOrDefault();

        public P1S(WorldState ws)
            : base(ws, 8)
        {
            _boss = RegisterEnemies(OID.Boss, true);
            RegisterEnemies(OID.FlailI);
            RegisterEnemies(OID.FlailO);

            RegisterComponent(new Shackles(this));
            RegisterComponent(new AetherExplosion(this));
            RegisterComponent(new Flails(this));
            RegisterComponent(new Knockback(this));
            RegisterComponent(new Intemperance(this));

            StateMachine.State? s;
            s = BuildTankbusterState(ref InitialState, 8);

            s = CommonStates.CastStart(ref s.Next, Boss, AID.AetherialShackles, 6);
            s = BuildShacklesCastEndState(ref s.Next);
            s = BuildWarderWrathState(ref s.Next, 4.1f);
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildShacklesResolveState(ref s.Next, 9.8f);

            s = BuildFlailStates(ref s.Next, 4.5f);
            s = BuildKnockbackStates(ref s.Next, 5.7f);
            s = BuildFlailStates(ref s.Next, 3.3f);
            s = BuildWarderWrathState(ref s.Next, 5.6f);
            s = BuildIntemperanceState(ref s.Next, 11.2f, true);
            s = BuildKnockbackStates(ref s.Next, 5.3f);

            s = BuildCellsState(ref s.Next, 9.1f);
            s = BuildAetherflailStates(ref s.Next, 8.1f);
            s = BuildKnockbackStates(ref s.Next, 7.6f);
            s = BuildAetherflailStates(ref s.Next, 3);
            s = CommonStates.CastStart(ref s.Next, Boss, AID.ShacklesOfTime, 4.6f);
            s = BuildShacklesOfTimeCastEndState(ref s.Next);
            s = BuildTankbusterState(ref s.Next, 5.2f);
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildShacklesOfTimeResolveState(ref s.Next, 4.7f);
            s = BuildSlamShutState(ref s.Next, 1.6f);

            s = BuildFourfoldShacklesState(ref s.Next, 13);

            s = BuildWarderWrathState(ref s.Next, 5.4f);
            s = BuildIntemperanceState(ref s.Next, 11.2f, false);
            s = BuildWarderWrathState(ref s.Next, 3.7f);

            s = BuildCellsState(ref s.Next, 11.2f);

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = BuildShacklesWithAetherchainState(ref s1b);
            s1e = BuildWarderWrathState(ref s1e.Next, 5.2f);
            s1e = CommonStates.CastStart(ref s1e.Next, Boss, AID.ShacklesOfTime, 7.2f);
            s1e = BuildShacklesOfTimeWithKnockbackState(ref s1e.Next);
            s1e = BuildWarderWrathState(ref s1e.Next, 5.9f);

            StateMachine.State? s2b = null, s2e = null;
            s2e = BuildShacklesOfTimeWithKnockbackState(ref s2b);
            s2e = BuildWarderWrathState(ref s2e.Next, 3);
            s2e = CommonStates.CastStart(ref s2e.Next, Boss, AID.AetherialShackles, 9);
            s2e = BuildShacklesWithAetherchainState(ref s2e.Next);
            s2e = BuildWarderWrathState(ref s2e.Next, 7);

            Dictionary<AID, (StateMachine.State?, Action)> forkDispatch = new();
            forkDispatch[AID.AetherialShackles] = new(s1b, () => { });
            forkDispatch[AID.ShacklesOfTime] = new(s2b, () => { });
            var fork = CommonStates.CastStart(ref s.Next, Boss, forkDispatch, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch delay = 7.8

            // forks merge
            s = BuildAetherflailStates(ref s1e.Next, 9);
            s2e.Next = s1e.Next;
            s = BuildAetherflailStates(ref s.Next, 2.7f);
            s = BuildAetherflailStates(ref s.Next, 2.7f);
            s = BuildWarderWrathState(ref s.Next, 13); // not sure about timings below...
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        protected override void ResetModule()
        {
            Arena.IsCircle = false; // reset could happen during cells
        }

        protected override void DrawArenaForegroundPost()
        {
            if (Arena.IsCircle)
            {
                // cells mode
                float diag = Arena.WorldHalfSize / 1.414214f;
                Arena.AddCircle(Arena.WorldCenter, _innerCircleRadius, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldE, Arena.WorldW, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldN, Arena.WorldS, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, diag), Arena.WorldCenter - new Vector3(diag, 0, diag), Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, -diag), Arena.WorldCenter - new Vector3(diag, 0, -diag), Arena.ColorBorder);
            }

            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State BuildTankbusterState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildWarderWrathState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.WarderWrath, delay, 5, "Wrath");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        // note: shackles are always combined with some other following mechanic, or at very least with resolve
        private StateMachine.State BuildShacklesCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, Boss, 3, "Shackles");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // delay from cast-end is 19 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesResolveState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Shackles>()!;
            var s = CommonStates.Condition(ref link, delay, () => comp.NumDebuffs() == 0, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            return s;
        }

        private StateMachine.State BuildFourfoldShacklesState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Shackles>()!;
            var cast = CommonStates.Cast(ref link, Boss, AID.FourShackles, 13, 3, "FourShackles");
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            // note that it takes almost a second for debuffs to be applied
            var hit1 = CommonStates.Condition(ref cast.Next, 9, () => comp.NumDebuffs() <= 6, "Hit1", 1, 3);
            hit1.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit2 = CommonStates.Condition(ref hit1.Next, 5, () => comp.NumDebuffs() <= 4, "Hit2");
            hit2.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit3 = CommonStates.Condition(ref hit2.Next, 5, () => comp.NumDebuffs() <= 2, "Hit3");
            hit3.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit4 = CommonStates.Condition(ref hit3.Next, 5, () => comp.NumDebuffs() == 0, "Hit4");
            hit4.EndHint |= StateMachine.StateHint.PositioningEnd;
            return hit4;
        }

        private StateMachine.State BuildShacklesOfTimeCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, Boss, 4, "ShacklesOfTime");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // delay from cast-end is 15 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesOfTimeResolveState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<AetherExplosion>()!;
            var s = CommonStates.Condition(ref link, delay, () => !comp.SOTActive, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            return s;
        }

        // in the end of the fight, there are two possible permutations of sequences
        private StateMachine.State BuildShacklesWithAetherchainState(ref StateMachine.State? link)
        {
            var s = BuildShacklesCastEndState(ref link);
            s = CommonStates.Cast(ref s.Next, Boss, AID.Aetherchain, 6.1f, 5, "Aetherchain");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Cast(ref s.Next, Boss, AID.Aetherchain, 3.2f, 5, "Aetherchain");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildShacklesResolveState(ref s.Next, 0); // technically resolve happens ~0.4sec before aetherchain cast end, but that's irrelevant
            return s;
        }

        private StateMachine.State BuildShacklesOfTimeWithKnockbackState(ref StateMachine.State? link)
        {
            var s = BuildShacklesOfTimeCastEndState(ref link);
            s = BuildKnockbackStates(ref s.Next, 2.2f, true);
            s = BuildShacklesOfTimeResolveState(ref s.Next, 3.4f);
            return s;
        }

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.GaolerFlailRL] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Right, Flails.Zone.Left));
            dispatch[AID.GaolerFlailLR] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Left, Flails.Zone.Right));
            dispatch[AID.GaolerFlailIO1] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Inner, Flails.Zone.Outer));
            dispatch[AID.GaolerFlailIO2] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Inner, Flails.Zone.Outer));
            dispatch[AID.GaolerFlailOI1] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Outer, Flails.Zone.Inner));
            dispatch[AID.GaolerFlailOI2] = new(null, () => FindComponent<Flails>()!.Start(Flails.Zone.Outer, Flails.Zone.Inner));
            return CommonStates.CastStart(ref link, Boss, dispatch, delay);
        }

        // if group continues, positioning flag is not cleared
        private StateMachine.State BuildFlailEndState(ref StateMachine.State? link, float castTimeLeft)
        {
            var end = CommonStates.CastEnd(ref link, Boss, castTimeLeft);

            var comp = FindComponent<Flails>()!;
            var resolve = CommonStates.Condition(ref end.Next, 3.6f, () => comp.NumCasts == 2, "Flails");
            resolve.Exit = () => comp.Reset();
            return resolve;
        }

        private StateMachine.State BuildFlailStates(ref StateMachine.State? link, float delay)
        {
            var start = BuildFlailStartState(ref link, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            var resolve = BuildFlailEndState(ref start.Next, 11.5f);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Flails>()!;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AetherflailRX] = new(null, () => comp.Start(Flails.Zone.Right, Flails.Zone.UnknownCircle));
            dispatch[AID.AetherflailLX] = new(null, () => comp.Start(Flails.Zone.Left, Flails.Zone.UnknownCircle));
            dispatch[AID.AetherflailIL] = new(null, () => comp.Start(Flails.Zone.Inner, Flails.Zone.Left));
            dispatch[AID.AetherflailIR] = new(null, () => comp.Start(Flails.Zone.Inner, Flails.Zone.Right));
            dispatch[AID.AetherflailOL] = new(null, () => comp.Start(Flails.Zone.Outer, Flails.Zone.Left));
            dispatch[AID.AetherflailOR] = new(null, () => comp.Start(Flails.Zone.Outer, Flails.Zone.Right));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 11.5f);

            var resolve = CommonStates.Condition(ref end.Next, 3.6f, () => comp.NumCasts == 2, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => comp.Reset();
            return resolve;
        }

        // part of group => group-with-next hint + no positioning hints
        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            var comp = FindComponent<Knockback>()!;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.KnockbackGrace] = new(null, () => comp.Start(Boss()?.CastInfo?.TargetID ?? 0, false));
            dispatch[AID.KnockbackPurge] = new(null, () => comp.Start(Boss()?.CastInfo?.TargetID ?? 0, true));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            if (!partOfGroup)
                start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;

            var resolve = CommonStates.Condition(ref end.Next, 4.4f, () => comp.AOEDone, "Explode");
            resolve.EndHint |= partOfGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => comp.Reset();
            return resolve;
        }

        // full intemperance phases (overlap either with 2 wraths or with flails)
        private StateMachine.State BuildIntemperanceState(ref StateMachine.State? link, float delay, bool withWraths)
        {
            var comp = FindComponent<Intemperance>()!;

            var intemp = CommonStates.Cast(ref link, Boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.Exit = () => comp.CurState = Intemperance.State.Unknown;
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.IntemperateTormentUp] = new(null, () => comp.CurState = Intemperance.State.BottomToTop);
            dispatch[AID.IntemperateTormentDown] = new(null, () => comp.CurState = Intemperance.State.TopToBottom);
            var explosionStart = CommonStates.CastStart(ref intemp.Next, Boss, dispatch, 5.8f);
            var explosionEnd = CommonStates.CastEnd(ref explosionStart.Next, Boss, 10);

            var cube1 = CommonStates.Condition(ref explosionEnd.Next, 1.2f, () => comp.NumExplosions > 0, "Cube1", 0.2f);
            cube1.EndHint |= StateMachine.StateHint.GroupWithNext;

            if (withWraths)
            {
                var wrath1 = BuildWarderWrathState(ref cube1.Next, 1);
                wrath1.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube2 = CommonStates.Condition(ref wrath1.Next, 5, () => comp.NumExplosions > 1, "Cube2", 0.2f);
                cube2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var wrath2 = BuildWarderWrathState(ref cube2.Next, 0.2f);
                wrath2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube3 = CommonStates.Condition(ref wrath2.Next, 5.8f, () => comp.NumExplosions > 2, "Cube3");
                cube3.Exit = () => comp.Reset();
                return cube3;
            }
            else
            {
                var flailStart = BuildFlailStartState(ref cube1.Next, 3);
                flailStart.EndHint |= StateMachine.StateHint.PositioningStart;

                var cube2 = CommonStates.Condition(ref flailStart.Next, 8, () => comp.NumExplosions > 1, "Cube2");
                cube2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var flailResolve = BuildFlailEndState(ref cube2.Next, 3.5f);
                flailResolve.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube3 = CommonStates.Condition(ref flailResolve.Next, 3.9f, () => comp.NumExplosions > 2, "Cube3");
                cube3.EndHint |= StateMachine.StateHint.PositioningEnd;
                cube3.Exit = () => comp.Reset();
                return cube3;
            }
        }

        private StateMachine.State BuildCellsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => Arena.IsCircle = true;
            return s;
        }

        private StateMachine.State BuildSlamShutState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.SlamShut, delay, 7, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => Arena.IsCircle = false;
            return s;
        }
    }
}
