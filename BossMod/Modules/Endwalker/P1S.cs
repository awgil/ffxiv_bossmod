using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BossMod
{
    // TODOs:
    // - determine range constants for knockback
    // - find a way to detect cubes in intemperance phase
    // - triple-check aetherflail AIDs and color SID param...
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

        private static float _innerCircleRadius = 12; // this determines in/out shackles and cells boundary

        // state related to normal and fourfold shackles
        // controlled purely by debuffs on players
        private class Shackles
        {
            private bool Active = false;
            private bool Failing = false;
            private byte DebuffsBlueImminent = 0;
            private byte DebuffsBlueFuture = 0;
            private byte DebuffsRedImminent = 0;
            private byte DebuffsRedFuture = 0;
            private ulong BlueTetherMatrix = 0;
            private ulong RedTetherMatrix = 0; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
            private ulong BlueExplosionMatrix = 0;
            private ulong RedExplosionMatrix = 0; // bit (8*i+j) is set if player j is inside explosion of player i; bit [i,i] is never set

            private static float _blueExplosionRadius = 4;
            private static float _redExplosionRadius = 8;
            private static uint _colorRed = 0xff8080ff;
            private static uint _colorBlue = 0xffff0080;
            private static uint _colorFail = 0xff00ffff;
            private static uint TetherColor(bool blue, bool red) => blue ? (red ? _colorFail : _colorBlue) : _colorRed;

            public void ModifyDebuff(int slot, bool isRed, bool isImminent, bool active)
            {
                if (slot < 0)
                    return;

                if (isRed)
                {
                    if (isImminent)
                        BitVector.SetVector8Bit(ref DebuffsRedImminent, slot, active);
                    else
                        BitVector.SetVector8Bit(ref DebuffsRedFuture, slot, active);
                }
                else
                {
                    if (isImminent)
                        BitVector.SetVector8Bit(ref DebuffsBlueImminent, slot, active);
                    else
                        BitVector.SetVector8Bit(ref DebuffsBlueFuture, slot, active);
                }
            }

            public void ClearDebuffs(int slot)
            {
                if (slot < 0)
                    return;
                BitVector.SetVector8Bit(ref DebuffsBlueImminent, slot, false);
                BitVector.SetVector8Bit(ref DebuffsBlueFuture, slot, false);
                BitVector.SetVector8Bit(ref DebuffsRedImminent, slot, false);
                BitVector.SetVector8Bit(ref DebuffsRedFuture, slot, false);
            }

            public void Update(P1S self)
            {
                BlueTetherMatrix = RedTetherMatrix = BlueExplosionMatrix = RedExplosionMatrix = 0;
                Failing = false;
                byte blueDebuffs = (byte)(DebuffsBlueImminent | DebuffsBlueFuture);
                byte redDebuffs = (byte)(DebuffsRedImminent | DebuffsRedFuture);
                Active = (blueDebuffs | redDebuffs) != 0;
                if (!Active)
                    return; // nothing to do...

                // update tether matrices
                var playersByDistance = new List<(int, float)>();
                for (int iSrc = 0; iSrc < self.RaidMembers.Length; ++iSrc)
                {
                    var src = self.RaidMembers[iSrc];
                    if (src == null || src.IsDead)
                        continue;

                    bool hasBlue = BitVector.IsVector8BitSet(blueDebuffs, iSrc);
                    bool hasRed = BitVector.IsVector8BitSet(redDebuffs, iSrc);
                    if (!hasBlue && !hasRed)
                        continue;

                    for (int iTgt = 0; iTgt < self.RaidMembers.Length; ++iTgt)
                    {
                        var tgt = self.RaidMembers[iTgt];
                        if (iTgt == iSrc || tgt == null || tgt.IsDead)
                            continue;

                        playersByDistance.Add(new(iTgt, (tgt.Position - src.Position).LengthSquared()));
                    }
                    playersByDistance.Sort((l, r) => l.Item2.CompareTo(r.Item2));
                    var numPotentialTethers = Math.Min(playersByDistance.Count, 3);

                    // blue => 3 closest
                    if (hasBlue)
                    {
                        BitVector.SetMatrix8x8Bit(ref BlueTetherMatrix, iSrc, iSrc, true);
                        for (int i = 0; i < numPotentialTethers; ++i)
                            BitVector.SetMatrix8x8Bit(ref BlueTetherMatrix, playersByDistance[i].Item1, iSrc, true);
                    }

                    // red => 3 furthest
                    if (hasRed)
                    {
                        BitVector.SetMatrix8x8Bit(ref RedTetherMatrix, iSrc, iSrc, true);
                        for (int i = 0; i < numPotentialTethers; ++i)
                            BitVector.SetMatrix8x8Bit(ref RedTetherMatrix, playersByDistance[playersByDistance.Count - i - 1].Item1, iSrc, true);
                    }

                    playersByDistance.Clear();
                }

                // update explosion matrices and detect problems (has to be done in a separate pass)
                for (int i = 0; i < self.RaidMembers.Length; ++i)
                {
                    bool blueExplosion = BitVector.ExtractVectorFromMatrix8x8(BlueTetherMatrix, i) != 0;
                    bool redExplosion = BitVector.ExtractVectorFromMatrix8x8(RedTetherMatrix, i) != 0;
                    Failing |= blueExplosion && redExplosion; // player i will have two explosions on self

                    if (blueExplosion)
                    {
                        BitVector.SetMatrix8x8Vector(ref BlueExplosionMatrix, i, (byte)self.FindRaidMembersInRange(i, _blueExplosionRadius));
                    }
                    if (redExplosion)
                    {
                        BitVector.SetMatrix8x8Vector(ref RedExplosionMatrix, i, (byte)self.FindRaidMembersInRange(i, _redExplosionRadius));
                    }
                }
                Failing |= BlueExplosionMatrix != 0; // there are some people hit by explosions
                Failing |= RedExplosionMatrix != 0;
            }

            public void DrawArenaForeground(P1S self)
            {
                if (!Active || self.PlayerSlot < 0)
                    return;
                var pc = self.RaidMembers[self.PlayerSlot]!;

                for (int i = 0; i < self.RaidMembers.Length; ++i)
                {
                    var actor = self.RaidMembers[i];
                    if (actor == null || actor.IsDead)
                        continue;

                    // draw tethers
                    var blueTetheredTo = BitVector.ExtractVectorFromMatrix8x8(BlueTetherMatrix, i);
                    var redTetheredTo = BitVector.ExtractVectorFromMatrix8x8(RedTetherMatrix, i);
                    var tetherMask = (byte)(blueTetheredTo | redTetheredTo);
                    if (tetherMask != 0)
                    {
                        self.Arena.Actor(actor.Position, actor.Rotation, TetherColor(blueTetheredTo != 0, redTetheredTo != 0));
                        for (int j = 0; j < 8; ++j)
                        {
                            var target = self.RaidMembers[j];
                            if (target != null && i != j && BitVector.IsVector8BitSet(tetherMask, j))
                            {
                                self.Arena.AddLine(actor.Position, target.Position, TetherColor(BitVector.IsVector8BitSet(blueTetheredTo, j), BitVector.IsVector8BitSet(redTetheredTo, j)));
                            }
                        }
                    }

                    // draw explosion circles that hit me
                    if (BitVector.IsMatrix8x8BitSet(BlueExplosionMatrix, i, self.PlayerSlot))
                        self.Arena.AddCircle(actor.Position, _blueExplosionRadius, _colorFail);
                    if (BitVector.IsMatrix8x8BitSet(RedExplosionMatrix, i, self.PlayerSlot))
                        self.Arena.AddCircle(actor.Position, _redExplosionRadius, _colorFail);
                }

                // draw explosion circles if I hit anyone
                if (BitVector.ExtractVectorFromMatrix8x8(BlueExplosionMatrix, self.PlayerSlot) != 0)
                    self.Arena.AddCircle(pc.Position, _blueExplosionRadius, _colorFail);
                if (BitVector.ExtractVectorFromMatrix8x8(RedExplosionMatrix, self.PlayerSlot) != 0)
                    self.Arena.AddCircle(pc.Position, _redExplosionRadius, _colorFail);
            }

            public void DescribeProblems(StringBuilder res)
            {
                if (Failing)
                {
                    res.Append("Shackles failing! ");
                }
            }
        }

        // state related to aether explosion mechanics, done as part of aetherflails, aetherchain and shackles of time abilities
        private class AetherExplosion
        {
            public enum Cell { None, Red, Blue }

            private int MemberWithSOT = -1; // if not -1, then every update exploding cells are recalculated based on this raid member's position
            private Cell ExplodingCells = Cell.None;

            private static uint _colorSOTActor = 0xff8080ff;

            public static Cell CellFromOffset(Vector3 offsetFromCenter)
            {
                if (offsetFromCenter.X == 0 && offsetFromCenter.Z == 0)
                    return Cell.None;

                var phi = MathF.Atan2(offsetFromCenter.Z, offsetFromCenter.X) + MathF.PI;
                int coneIndex = (int)(4 * phi / MathF.PI); // phi / (pi/4); range [0, 8]
                bool oddCone = (coneIndex & 1) != 0;
                bool outerCone = offsetFromCenter.LengthSquared() > _innerCircleRadius * _innerCircleRadius;
                return (oddCone != outerCone) ? Cell.Blue : Cell.Red; // inner odd = blue, outer even = blue
            }

            public void ModifyDebuff(int slot, bool active)
            {
                if (slot < 0)
                    return;

                if (active)
                {
                    if (MemberWithSOT >= 0)
                        Service.Log($"[P1S] Unexpected ShacklesOfTime: another is already active!");
                    MemberWithSOT = slot;
                }
                else if (MemberWithSOT == slot)
                {
                    MemberWithSOT = -1;
                }
                ExplodingCells = Cell.None;
            }

            public void SetForcedExplosion(Cell cell)
            {
                if (MemberWithSOT != -1)
                {
                    Service.Log($"[P1S] Unexpected forced explosion: SOT is currently active!");
                    MemberWithSOT = -1;
                }
                ExplodingCells = cell;
            }

            public void Update(P1S self)
            {
                var sotActor = MemberWithSOT >= 0 ? self.RaidMembers[MemberWithSOT] : null;
                if (sotActor != null)
                    ExplodingCells = CellFromOffset(sotActor.Position - self.Arena.WorldCenter);
            }

            public void DrawArenaBackground(P1S self)
            {
                if (ExplodingCells == Cell.None || self.PlayerSlot == MemberWithSOT)
                    return; // nothing to draw

                if (!self.Arena.IsCircle)
                {
                    Service.Log("[P1S] Trying to draw aether AOE when cells mode is not active...");
                    return;
                }

                float start = ExplodingCells == Cell.Blue ? 0 : MathF.PI / 4;
                for (int i = 0; i < 4; ++i)
                {
                    self.Arena.ZoneCone(self.Arena.WorldCenter, 0, _innerCircleRadius, start, start + MathF.PI / 4, self.Arena.ColorDanger);
                    self.Arena.ZoneCone(self.Arena.WorldCenter, _innerCircleRadius, self.Arena.WorldHalfSize, start + MathF.PI / 4, start + MathF.PI / 2, self.Arena.ColorDanger);
                    start += MathF.PI / 2;
                }
            }

            public void DrawArenaForeground(P1S self)
            {
                if (MemberWithSOT < 0 || MemberWithSOT == self.PlayerSlot)
                    return;

                var actor = self.RaidMembers[MemberWithSOT];
                if (actor == null)
                    return;

                self.Arena.Actor(actor.Position, actor.Rotation, _colorSOTActor);
            }

            public void DescribeProblems(P1S self, StringBuilder res)
            {
                if (self.PlayerSlot >= 0 && self.PlayerSlot != MemberWithSOT && ExplodingCells != Cell.None)
                {
                    if (ExplodingCells == CellFromOffset(self.RaidMembers[self.PlayerSlot]!.Position - self.Arena.WorldCenter))
                    {
                        res.Append("Hit by aether explosion! ");
                    }
                }
            }
        }

        // state related to [aether]flails mechanics
        private class Flails
        {
            public enum Zone { None, Left, Right, Inner, Outer, UnknownCircle }

            private Zone Imminent = Zone.None;
            private Zone Future = Zone.None;
            private bool ShowBoth = false;

            private static float _coneHalfAngle = MathF.PI / 4;

            public void Reset(Zone imminent = Zone.None, Zone future = Zone.None, bool showBoth = false)
            {
                Imminent = imminent;
                Future = future;
                ShowBoth = showBoth;
            }

            public void Advance()
            {
                Imminent = Future;
                Future = Zone.None;
                ShowBoth = false;
            }

            public void Update(P1S self)
            {
                // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
                // these can appear few frames later...
                if (Future == Zone.UnknownCircle && self._weaponsBall.Count + self._weaponsChakram.Count > 0)
                {
                    if (self._weaponsBall.Count > 0 && self._weaponsChakram.Count > 0)
                    {
                        Service.Log($"[P1S] Failed to determine second aetherflail: there are {self._weaponsBall.Count} balls and {self._weaponsChakram.Count} chakrams");
                    }
                    else
                    {
                        Future = self._weaponsBall.Count > 0 ? Zone.Inner : Zone.Outer;
                    }
                }
            }

            public void DrawArenaBackground(P1S self)
            {
                if (Imminent == Zone.None || self._boss == null)
                    return; // not active

                DrawZone(self, self._boss.Position, self._boss.Rotation, Imminent);

                // if it is possible to show safe spot for both zones (aetherflails case), do that
                if (ShowBoth)
                {
                    DrawZone(self, self._boss.Position, self._boss.Rotation, Future);
                }
            }

            public void DescribeProblems(P1S self, StringBuilder res)
            {
                if (self.PlayerSlot < 0 || Imminent == Zone.None || self._boss == null)
                    return;

                var pcPos = self.RaidMembers[self.PlayerSlot]!.Position;
                if (IsInAOE(pcPos, self._boss.Position, self._boss.Rotation, Imminent))
                {
                    res.Append("Hit by first flail! ");
                }
                if (ShowBoth && IsInAOE(pcPos, self._boss.Position, self._boss.Rotation, Future))
                {
                    res.Append("Hit by next flail! ");
                }
            }

            private void DrawZone(P1S self, Vector3 origin, float rot, Zone zone)
            {
                switch (zone)
                {
                    case Zone.Left:
                        self.Arena.ZoneCone(origin, 0, 100, rot - MathF.PI / 2 + _coneHalfAngle, rot + 3 * MathF.PI / 2 - _coneHalfAngle, self.Arena.ColorDanger);
                        break;
                    case Zone.Right:
                        self.Arena.ZoneCone(origin, 0, 100, rot + MathF.PI / 2 - _coneHalfAngle, rot - 3 * MathF.PI / 2 + _coneHalfAngle, self.Arena.ColorDanger);
                        break;
                    case Zone.Inner:
                        self.Arena.ZoneCircle(origin, _innerCircleRadius, self.Arena.ColorDanger);
                        break;
                    case Zone.Outer:
                        self.Arena.ZoneCone(origin, _innerCircleRadius, 100, 0, 2 * MathF.PI, self.Arena.ColorDanger);
                        break;
                }
            }

            private bool IsInAOE(Vector3 pos, Vector3 origin, float rot, Zone zone)
            {
                switch (zone)
                {
                    case Zone.Left:
                        return !IsInCone(pos - origin, rot - MathF.PI / 2);
                    case Zone.Right:
                        return !IsInCone(pos - origin, rot + MathF.PI / 2);
                    case Zone.Inner:
                        return (pos - origin).LengthSquared() <= _innerCircleRadius * _innerCircleRadius;
                    case Zone.Outer:
                        return (pos - origin).LengthSquared() >= _innerCircleRadius * _innerCircleRadius;
                }
                return false;
            }

            private bool IsInCone(Vector3 offset, float axisDir)
            {
                var axis = new Vector3(MathF.Sin(axisDir), 0, MathF.Cos(axisDir));
                float cosToPlayer = Vector3.Dot(Vector3.Normalize(offset), axis);
                return cosToPlayer > MathF.Cos(_coneHalfAngle);
            }
        }

        // state related to knockback + aoe mechanic
        private class Knockback
        {
            private enum Phase { None, Knockback, AOE }
            private enum Behaviour { Unknown, GtfoFromTarget, GtfoFromEveryone, StackWithTarget, StackWithRaid }

            private Phase CurrentPhase = Phase.None;
            public int KnockbackTarget { get; private set; } = -1;
            private int AOETarget = -1;
            private bool IsFlare; // true -> purge aka flare (stay away from MT), false -> grace aka holy (stack to MT)
            private byte AOEInRange;
            private Behaviour DesiredBehaviour = Behaviour.Unknown;

            private static float _kbDistance = 15; // quite sure about this
            private static float _flareRange = 20; // unconfirmed
            private static float _holyRange = 6; // unconfirmed, but looks correct...
            private static uint _colorInAOE = 0xffc0c0c0;
            private static uint _colorOutOfAOE = 0xff808080;
            private static uint _colorAOETarget = 0xff8080ff;
            private static uint _colorVulnerable = 0xffff00ff;

            public void Reset()
            {
                CurrentPhase = Phase.None;
                KnockbackTarget = AOETarget = -1;
            }

            public void StartKnockback(int targetSlot, bool isFlare)
            {
                if (targetSlot < 0)
                {
                    Service.Log("[P1S] Failed to determine knockback target");
                    return;
                }

                CurrentPhase = Phase.Knockback;
                KnockbackTarget = targetSlot;
                IsFlare = isFlare;
                AOETarget = -1;
            }

            public void StartAOE()
            {
                if (CurrentPhase == Phase.Knockback)
                {
                    CurrentPhase = Phase.AOE;
                }
                else
                {
                    Service.Log($"[P1S] Unexpected knockback AOE-start in phase {CurrentPhase}");
                    Reset();
                }
            }

            public void Update(P1S self)
            {
                if (CurrentPhase == Phase.AOE)
                {
                    AOEInRange = 0;
                    DesiredBehaviour = Behaviour.Unknown;
                    AOETarget = self.FindRaidMemberSlot(self._boss?.TargetID ?? 0);
                    var aoeTargetActor = AOETarget >= 0 ? self.RaidMembers[AOETarget] : null;
                    if (aoeTargetActor == null)
                        return;

                    float aoeRange = IsFlare ? _flareRange : _holyRange;
                    AOEInRange = (byte)self.FindRaidMembersInRange(AOETarget, aoeRange);

                    var pc = self.PlayerSlot >= 0 ? self.RaidMembers[self.PlayerSlot] : null;
                    if (pc == null)
                        return;

                    bool shouldStackWithTarget = self.PlayerSlot != KnockbackTarget && !IsFlare;
                    if (aoeTargetActor != pc)
                    {
                        // i'm not the current tank - I should gtfo from flare or from holy if i'm vulnerable, otherwise stack to current tank
                        DesiredBehaviour = shouldStackWithTarget ? Behaviour.StackWithTarget : Behaviour.GtfoFromTarget;
                    }
                    else
                    {
                        // i'm the current tank - I should gtfo from everyone else if i'll get flare -or- if i'm vulnerable (assuming i'll pop invuln not to die)
                        DesiredBehaviour = shouldStackWithTarget ? Behaviour.StackWithRaid : Behaviour.GtfoFromEveryone;
                    }
                }
            }

            public void DrawArenaForeground(P1S self)
            {
                if (CurrentPhase == Phase.AOE)
                {
                    var pc = self.PlayerSlot >= 0 ? self.RaidMembers[self.PlayerSlot] : null;
                    var aoeTargetActor = AOETarget >= 0 ? self.RaidMembers[AOETarget] : null;
                    var knockbackActor = KnockbackTarget >= 0 ? self.RaidMembers[KnockbackTarget] : null;
                    if (pc == null || aoeTargetActor == null || knockbackActor == null)
                        return;

                    if (aoeTargetActor == pc)
                    {
                        // there will be AOE around me, draw all players to help with positioning
                        for (int i = 0; i < self.RaidMembers.Length; ++i)
                        {
                            var p = self.RaidMembers[i];
                            if (p != null && !p.IsDead)
                                self.Arena.Actor(p.Position, p.Rotation, BitVector.IsVector8BitSet(AOEInRange, i) ? _colorInAOE : _colorOutOfAOE);
                        }
                    }

                    // draw AOE source
                    self.Arena.Actor(aoeTargetActor.Position, aoeTargetActor.Rotation, _colorAOETarget);
                    self.Arena.AddCircle(aoeTargetActor.Position, IsFlare ? _flareRange : _holyRange, self.Arena.ColorDanger);

                    // draw vulnerable target
                    self.Arena.Actor(knockbackActor.Position, knockbackActor.Rotation, _colorVulnerable);
                }
            }

            public void DescribeProblems(P1S self, StringBuilder res)
            {
                if (CurrentPhase == Phase.Knockback)
                {
                    var pc = self.PlayerSlot >= 0 ? self.RaidMembers[self.PlayerSlot] : null;
                    bool pcIsMT = self._boss?.CastInfo?.TargetID == pc?.InstanceID;
                    if (pc != null && pcIsMT)
                    {
                        // warn if about to be knocked into wall
                        var dir = Vector3.Normalize(pc.Position - self._boss!.Position);
                        var newPos = pc.Position + dir * _kbDistance;
                        if (!self.Arena.InBounds(newPos))
                        {
                            res.Append("About to be knocked into wall! ");
                        }
                    }
                }
                else if (CurrentPhase == Phase.AOE && self.PlayerSlot >= 0)
                {
                    switch (DesiredBehaviour)
                    {
                        case Behaviour.GtfoFromTarget:
                            if (BitVector.IsVector8BitSet(AOEInRange, self.PlayerSlot))
                            {
                                res.Append("GTFO from target! ");
                            }
                            break;
                        case Behaviour.GtfoFromEveryone:
                            if (AOEInRange != 0)
                            {
                                res.Append("GTFO from everyone! ");
                            }
                            break;
                        case Behaviour.StackWithTarget:
                            if (!BitVector.IsVector8BitSet(AOEInRange, self.PlayerSlot))
                            {
                                res.Append("Stack with target! ");
                            }
                            break;
                        case Behaviour.StackWithRaid:
                            if (BitVector.IsVector8BitSet(AOEInRange, KnockbackTarget))
                            {
                                res.Append("GTFO from co-tank! ");
                            }
                            if (AOEInRange == 0)
                            {
                                // TODO: consider how many people should be in aoe range...
                                res.Append("Stack with raid! ");
                            }
                            break;
                    }
                }
            }
        }

        // state related to intemperance mechanic
        private class Intemperance
        {
            private bool Active = false;
            private bool DirDown = false;

            public void Reset()
            {
                Active = false;
            }

            public void Start(bool dirDown)
            {
                Active = true;
                DirDown = dirDown;
            }

            public void DescribeProblems(P1S self, StringBuilder res)
            {
                if (Active)
                {
                    res.Append(DirDown ? "Order: top->bottom " : "Order: bottom->top ");
                }
            }
        }

        private WorldState.Actor? _boss;
        private List<WorldState.Actor> _weaponsAnchor = new();
        private List<WorldState.Actor> _weaponsBall = new();
        private List<WorldState.Actor> _weaponsChakram = new();

        private Shackles _shackles = new();
        private AetherExplosion _aetherExplosion = new();
        private Flails _flails = new();
        private Knockback _knockback = new();
        private Intemperance _intemperance = new();

        public P1S(WorldState ws)
            : base(ws, 8)
        {
            WorldState.ActorStatusGain += ActorStatusGain;
            WorldState.ActorStatusLose += ActorStatusLose;

            StateMachine.State? s;
            s = BuildTankbusterState(ref InitialState, 8);

            s = CommonStates.CastStart(ref s.Next, () => _boss, AID.AetherialShackles, 6);
            s = BuildShacklesCastEndState(ref s.Next);
            s = BuildWarderWrathState(ref s.Next, 4, true);
            s = BuildShacklesResolveState(ref s.Next, 10);

            s = BuildFlailStates(ref s.Next, 4.3f);
            s = BuildKnockbackStates(ref s.Next, 5.3f);
            s = BuildFlailStates(ref s.Next, 2.7f);
            s = BuildWarderWrathState(ref s.Next, 5.2f);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11.2f);
            s = BuildWarderWrathState(ref s.Next, 1.2f, true);
            s = BuildWarderWrathState(ref s.Next, 5.2f, true, "Cube2"); // cube2 and aoe start happen at almost same time
            s = CommonStates.Timeout(ref s.Next, 6, "Cube3");
            s.Exit = () => _intemperance.Reset();

            s = BuildKnockbackStates(ref s.Next, 5.3f);

            s = BuildCellsState(ref s.Next, 8.6f);
            s = BuildAetherflailStates(ref s.Next, 8);
            s = BuildKnockbackStates(ref s.Next, 7.2f);
            s = BuildAetherflailStates(ref s.Next, 2.2f);
            s = CommonStates.CastStart(ref s.Next, () => _boss, AID.ShacklesOfTime, 4.2f);
            s = BuildShacklesOfTimeCastEndState(ref s.Next);
            s = BuildTankbusterState(ref s.Next, 5.2f, true);
            s = BuildShacklesOfTimeResolveState(ref s.Next, 5);
            s = BuildSlamShutState(ref s.Next, 1.4f);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FourShackles, 13, 3, "FourShackles");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 10, "Hit1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit3");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit4");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, 4);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11.2f);
            s = BuildFlailStartState(ref s.Next, 3.2f);
            s = CommonStates.Timeout(ref s.Next, 8, "Cube2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildFlailEndState(ref s.Next, 3.5f, true);
            s = CommonStates.Timeout(ref s.Next, 4, "Cube3");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            s.Exit = () => _intemperance.Reset();

            s = BuildWarderWrathState(ref s.Next, 3.3f);

            s = BuildCellsState(ref s.Next, 11.2f);

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = BuildShacklesCastEndState(ref s1b);
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = BuildShacklesResolveState(ref s1e.Next, 0);
            s1e = BuildWarderWrathState(ref s1e.Next, 5.2f);
            s1e = CommonStates.CastStart(ref s1e.Next, () => _boss, AID.ShacklesOfTime, 7.2f);
            s1e = BuildShacklesOfTimeCastEndState(ref s1e.Next);
            s1e = BuildKnockbackStates(ref s1e.Next, 2.2f, true);
            s1e = BuildShacklesOfTimeResolveState(ref s1e.Next, 3);
            s1e = BuildWarderWrathState(ref s1e.Next, 5.5f);

            StateMachine.State? s2b = null, s2e = null;
            s2e = BuildShacklesOfTimeCastEndState(ref s2b);
            s2e = BuildKnockbackStates(ref s2e.Next, 2, true);
            s2e = BuildShacklesOfTimeResolveState(ref s2e.Next, 3);
            s2e = BuildWarderWrathState(ref s2e.Next, 3);
            s2e = CommonStates.CastStart(ref s2e.Next, () => _boss, AID.AetherialShackles, 9);
            s2e = BuildShacklesCastEndState(ref s2e.Next);
            s2e = CommonStates.Cast(ref s2e.Next, () => _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s2e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = CommonStates.Cast(ref s2e.Next, () => _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = BuildShacklesResolveState(ref s2e.Next, 0);
            s2e = BuildWarderWrathState(ref s2e.Next, 7);

            Dictionary<AID, (StateMachine.State?, Action)> forkDispatch = new();
            forkDispatch[AID.AetherialShackles] = new(s1b, () => { });
            forkDispatch[AID.ShacklesOfTime] = new(s2b, () => { });
            var fork = CommonStates.CastStart(ref s.Next, () => _boss, forkDispatch, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch - delay ~8?

            // forks merge
            s = BuildAetherflailStates(ref s1e.Next, 9);
            s2e.Next = s1e.Next;
            s = BuildAetherflailStates(ref s.Next, 2.3f);
            s = BuildAetherflailStates(ref s.Next, 6); // not sure about timings below...
            s = BuildWarderWrathState(ref s.Next, 13);
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.ActorStatusGain -= ActorStatusGain;
                WorldState.ActorStatusLose -= ActorStatusLose;
            }
            base.Dispose(disposing);
        }

        public override void Update()
        {
            base.Update();

            _shackles.Update(this);
            _aetherExplosion.Update(this);
            _flails.Update(this);
            _knockback.Update(this);
        }

        protected override void DrawHeader()
        {
            var problems = new StringBuilder();
            _shackles.DescribeProblems(problems);
            _aetherExplosion.DescribeProblems(this, problems);
            _flails.DescribeProblems(this, problems);
            _knockback.DescribeProblems(this, problems);
            _intemperance.DescribeProblems(this, problems);
            if (problems.Length > 0)
            {
                ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), problems.ToString());
            }
        }

        protected override void DrawArena()
        {
            _aetherExplosion.DrawArenaBackground(this);
            _flails.DrawArenaBackground(this);

            Arena.Border();

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

            if (_boss != null)
                Arena.Actor(_boss.Position, _boss.Rotation, 0xff0000ff);

            // draw player
            var pc = PlayerSlot >= 0 ? RaidMembers[PlayerSlot] : null;
            if (pc != null)
                Arena.Actor(pc.Position, pc.Rotation, 0xff00ff00);

            _knockback.DrawArenaForeground(this);
            _shackles.DrawArenaForeground(this);
            _aetherExplosion.DrawArenaForeground(this);
        }

        protected override void NonPlayerCreated(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != null)
                        Service.Log($"[P1S] Created boss {actor.InstanceID} while another boss {_boss.InstanceID} is still alive");
                    _boss = actor;
                    break;
                case OID.FlailLR:
                    _weaponsAnchor.Add(actor);
                    break;
                case OID.FlailI:
                    _weaponsBall.Add(actor);
                    break;
                case OID.FlailO:
                    _weaponsChakram.Add(actor);
                    break;
            }
        }

        protected override void NonPlayerDestroyed(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != actor)
                        Service.Log($"[P1S] Destroying boss {actor.InstanceID} while active boss is different: {_boss?.InstanceID}");
                    else
                        _boss = null;
                    break;
                case OID.FlailLR:
                    _weaponsAnchor.Remove(actor);
                    break;
                case OID.FlailI:
                    _weaponsBall.Remove(actor);
                    break;
                case OID.FlailO:
                    _weaponsChakram.Remove(actor);
                    break;
            }
        }

        protected override void RaidMemberDestroyed(int index)
        {
            _shackles.ClearDebuffs(index);
            _aetherExplosion.ModifyDebuff(index, false);
            if (_knockback.KnockbackTarget == index)
                _knockback.Reset();
        }

        protected override void Reset()
        {
            Arena.IsCircle = false; // reset could happen during cells
            // note: no need to clean up any shackles, they are controlled purely by active debuffs...
            _aetherExplosion.SetForcedExplosion(AetherExplosion.Cell.None);
            _flails.Reset();
            _knockback.Reset();
            _intemperance.Reset();
        }

        private StateMachine.State BuildTankbusterState(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            if (partOfGroup)
                s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State BuildWarderWrathState(ref StateMachine.State? link, float delay, bool partOfGroup = false, string startName = "")
        {
            var start = CommonStates.CastStart(ref link, () => _boss, AID.WarderWrath, delay, startName);
            start.EndHint |= StateMachine.StateHint.GroupWithNext;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Wrath");
            end.EndHint |= StateMachine.StateHint.Raidwide;
            if (partOfGroup)
                end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: shackles are always combined with some other following mechanic, or at very least with resolve
        private StateMachine.State BuildShacklesCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, () => _boss, 3, "Shackles");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // delay from cast-end is 19 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Timeout(ref link, delay, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            return s;
        }

        private StateMachine.State BuildShacklesOfTimeCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, () => _boss, 4, "ShacklesOfTime");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // delay from cast-end is 15 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesOfTimeResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Timeout(ref link, delay, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            return s;
        }

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.GaolerFlailRL] = new(null, () => _flails.Reset(Flails.Zone.Right, Flails.Zone.Left));
            dispatch[AID.GaolerFlailLR] = new(null, () => _flails.Reset(Flails.Zone.Left, Flails.Zone.Right));
            dispatch[AID.GaolerFlailIO1] = new(null, () => _flails.Reset(Flails.Zone.Inner, Flails.Zone.Outer));
            dispatch[AID.GaolerFlailIO2] = new(null, () => _flails.Reset(Flails.Zone.Inner, Flails.Zone.Outer));
            dispatch[AID.GaolerFlailOI1] = new(null, () => _flails.Reset(Flails.Zone.Outer, Flails.Zone.Inner));
            dispatch[AID.GaolerFlailOI2] = new(null, () => _flails.Reset(Flails.Zone.Outer, Flails.Zone.Inner));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            return start;
        }

        // if group continues, positioning flag is not cleared
        private StateMachine.State BuildFlailEndState(ref StateMachine.State? link, float castTimeLeft, bool continueGroup)
        {
            var end = CommonStates.CastEnd(ref link, () => _boss, castTimeLeft);
            end.Exit = () => _flails.Advance();

            var resolve = CommonStates.Timeout(ref end.Next, 4, "Flails");
            resolve.EndHint |= continueGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _flails.Reset();
            return resolve;
        }

        private StateMachine.State BuildFlailStates(ref StateMachine.State? link, float delay)
        {
            var start = BuildFlailStartState(ref link, delay);
            return BuildFlailEndState(ref start.Next, 11.5f, false);
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AetherflailRX] = new(null, () => _flails.Reset(Flails.Zone.Right, Flails.Zone.UnknownCircle, true));
            dispatch[AID.AetherflailLX] = new(null, () => _flails.Reset(Flails.Zone.Left, Flails.Zone.UnknownCircle, true));
            dispatch[AID.AetherflailIL] = new(null, () => _flails.Reset(Flails.Zone.Inner, Flails.Zone.Left, true));
            dispatch[AID.AetherflailIR] = new(null, () => _flails.Reset(Flails.Zone.Inner, Flails.Zone.Right, true));
            dispatch[AID.AetherflailOL] = new(null, () => _flails.Reset(Flails.Zone.Outer, Flails.Zone.Left, true));
            dispatch[AID.AetherflailOR] = new(null, () => _flails.Reset(Flails.Zone.Outer, Flails.Zone.Right, true));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 11.5f);
            end.Exit = () => _flails.Advance();

            var resolve = CommonStates.Timeout(ref end.Next, 4, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _flails.Reset();
            return resolve;
        }

        // part of group => group-with-next hint + no positioning hints
        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.KnockbackGrace] = new(null, () => _knockback.StartKnockback(FindRaidMemberSlot(_boss?.CastInfo?.TargetID ?? 0), false));
            dispatch[AID.KnockbackPurge] = new(null, () => _knockback.StartKnockback(FindRaidMemberSlot(_boss?.CastInfo?.TargetID ?? 0), true));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            if (!partOfGroup)
                start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;
            end.Exit = () => _knockback.StartAOE();

            var resolve = CommonStates.Timeout(ref end.Next, 5, "Explode");
            resolve.EndHint |= partOfGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _knockback.Reset();
            return resolve;
        }

        // intemperance cast start/end + explosion start/end + first resolve
        private StateMachine.State BuildIntemperanceExplosionStart(ref StateMachine.State? link, float delay)
        {
            // TODO: determine cubes... use the following idea:
            // 1. look for a bunch of server messages with opcode 191 (~3 sec after intemp cast end) - there should be 25 of them
            // 2. first 24 correspond to cubes, in groups of three (bottom->top), in order: NW N NE E SE S SW W
            //    the last 191 can be ignored, probably corresponds to oneshot border
            //    so asymmetrical pattern is: BPR RRB BPR RPR BPR RBB BPR RPR
            //    and symmetrical pattern is: RPR BRB RPR RPB RPR BBB RPR RPB
            // 3. each message has 4 dwords of payload; first dword is some id (800375A0), second dword is cube type, third dword is {XXXXXXsq} (where sq is sequential order, from 0 to 23), fourth dword is instance ID of boss
            // 4. cube type is 00020001 for red, 00800040 for blue, 20001000 for purple
            // 5. on each explosion, we get 8 191s, with type 00080004 for exploded red, 04000004 for exploded blue, 08000004 for exploded purple
            // 6. 3 sec before second & third explosion, we get 8 191s, with type 00200020 for preparing red, 02000200 for preparing blue, 80008000 for preparing purple
            var intemp = CommonStates.Cast(ref link, () => _boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.IntemperateTormentUp] = new(null, () => _intemperance.Start(false));
            dispatch[AID.IntemperateTormentDown] = new(null, () => _intemperance.Start(true));
            var explosion = CommonStates.CastStart(ref intemp.Next, () => _boss, dispatch, 6);
            var end = CommonStates.CastEnd(ref explosion.Next, () => _boss, 10);
            var resolve = CommonStates.Timeout(ref end.Next, 1, "Cube1");
            resolve.EndHint |= StateMachine.StateHint.GroupWithNext;
            return resolve;
        }

        private StateMachine.State BuildCellsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => Arena.IsCircle = true;
            return s;
        }

        private StateMachine.State BuildSlamShutState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.SlamShut, delay, 7, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => Arena.IsCircle = false;
            return s;
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            switch ((SID)arg.actor.Statuses[arg.index].ID)
            {
                case SID.AetherExplosion:
                    if (arg.actor == _boss)
                    {
                        switch (arg.actor.Statuses[arg.index].StackCount)
                        {
                            case 0x4C:
                                _aetherExplosion.SetForcedExplosion(AetherExplosion.Cell.Red);
                                break;
                            case 0x4D:
                                _aetherExplosion.SetForcedExplosion(AetherExplosion.Cell.Blue);
                                break;
                            default:
                                Service.Log($"[P1S] Unexpected aether explosion param {arg.actor.Statuses[arg.index].StackCount:X2}");
                                break;
                        }
                    }
                    else
                    {
                        Service.Log($"[P1S] Unexpected aether explosion status on {Utils.ObjectString(arg.actor.InstanceID)}");
                    }
                    break;
                case SID.ShacklesOfTime:
                    _aetherExplosion.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true);
                    break;
                case SID.ShacklesOfCompanionship0:
                case SID.ShacklesOfCompanionship1:
                case SID.ShacklesOfCompanionship2:
                case SID.ShacklesOfCompanionship3:
                case SID.ShacklesOfCompanionship4:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, false, true);
                    break;
                case SID.ShacklesOfLoneliness0:
                case SID.ShacklesOfLoneliness1:
                case SID.ShacklesOfLoneliness2:
                case SID.ShacklesOfLoneliness3:
                case SID.ShacklesOfLoneliness4:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, false, true);
                    break;
                case SID.InescapableCompanionship:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, true, true);
                    break;
                case SID.InescapableLoneliness:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, true, true);
                    break;
            }
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            switch ((SID)arg.actor.Statuses[arg.index].ID)
            {
                case SID.AetherExplosion:
                    if (arg.actor == _boss)
                        _aetherExplosion.SetForcedExplosion(AetherExplosion.Cell.None);
                    break;
                case SID.ShacklesOfTime:
                    _aetherExplosion.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false);
                    break;
                case SID.ShacklesOfCompanionship0:
                case SID.ShacklesOfCompanionship1:
                case SID.ShacklesOfCompanionship2:
                case SID.ShacklesOfCompanionship3:
                case SID.ShacklesOfCompanionship4:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, false, false);
                    break;
                case SID.ShacklesOfLoneliness0:
                case SID.ShacklesOfLoneliness1:
                case SID.ShacklesOfLoneliness2:
                case SID.ShacklesOfLoneliness3:
                case SID.ShacklesOfLoneliness4:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, false, false);
                    break;
                case SID.InescapableCompanionship:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, true, false);
                    break;
                case SID.InescapableLoneliness:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, true, false);
                    break;
            }
        }
    }
}
