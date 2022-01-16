using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

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
            ShacklesOfCompanionship = 2742, // shackles: purple (tether to 3 closest)
            ShacklesOfLoneliness = 2743, // shackles: red (tether to 3 farthest)
            InescapableCompanionship = 2744, // replaces corresponding shackles in 13s
            InescapableLoneliness = 2745,
            DamageDown = 2911, // applied by two successive cubes of the same color
            MagicVulnerabilityUp = 2941, // applied by shackle resolve, knockbacks
        }

        private class Shackles
        {
            public bool Active = false;
            public byte DebuffsBlueImminent = 0;
            public byte DebuffsBlueFuture = 0;
            public byte DebuffsRedImminent = 0;
            public byte DebuffsRedFuture = 0;
            public ulong BlueTetherMatrix = 0;
            public ulong RedTetherMatrix = 0; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
            public ulong BlueExplosionMatrix = 0;
            public ulong RedExplosionMatrix = 0; // bit (8*i+j) is set if player j is inside explosion of player i; bit [i,i] is never set

            private static float _blueExplosionRadius = 4;
            private static float _redExplosionRadius = 8;
            private static uint _colorRed = 0xff8080ff;
            private static uint _colorBlue = 0xffff0080;
            private static uint _colorFail = 0xff00ffff;
            private static uint TetherColor(bool blue, bool red) => blue ? (red ? _colorFail : _colorBlue) : _colorRed;

            private static void SetMatrixBit(ref ulong matrix, int i, int j)
            {
                matrix |= 1ul << (8 * i + j);
            }

            private static void SetMatrixVector(ref ulong matrix, int i, byte vector)
            {
                matrix |= ((ulong)vector) << (8 * i);
            }

            private static void SetVectorBit(ref byte vector, int i, bool v)
            {
                if (v)
                    vector |= (byte)(1 << i);
                else
                    vector &= (byte)~(1 << i);
            }

            private static bool IsMatrixBitSet(ulong matrix, int i, int j)
            {
                return (matrix & (1ul << (8 * i + j))) != 0;
            }

            private static byte ExtractVectorFromMatrix(ulong matrix, int i)
            {
                return (byte)(matrix >> (i * 8));
            }

            private static bool IsVectorBitSet(byte vector, int i)
            {
                return (vector & (1 << i)) != 0;
            }

            public void ModifyDebuff(int slot, bool isRed, bool isImminent, bool active)
            {
                if (slot < 0)
                    return;

                if (isRed)
                {
                    if (isImminent)
                        SetVectorBit(ref DebuffsRedImminent, slot, active);
                    else
                        SetVectorBit(ref DebuffsRedFuture, slot, active);
                }
                else
                {
                    if (isImminent)
                        SetVectorBit(ref DebuffsBlueImminent, slot, active);
                    else
                        SetVectorBit(ref DebuffsBlueFuture, slot, active);
                }
            }

            public void ClearDebuffs(int slot)
            {
                if (slot < 0)
                    return;
                SetVectorBit(ref DebuffsBlueImminent, slot, false);
                SetVectorBit(ref DebuffsBlueFuture, slot, false);
                SetVectorBit(ref DebuffsRedImminent, slot, false);
                SetVectorBit(ref DebuffsRedFuture, slot, false);
            }

            public void Update(P1S self)
            {
                BlueTetherMatrix = RedTetherMatrix = BlueExplosionMatrix = RedExplosionMatrix = 0;
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

                    bool hasBlue = IsVectorBitSet(blueDebuffs, iSrc);
                    bool hasRed = IsVectorBitSet(redDebuffs, iSrc);
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
                        SetMatrixBit(ref BlueTetherMatrix, iSrc, iSrc);
                        for (int i = 0; i < numPotentialTethers; ++i)
                            SetMatrixBit(ref BlueTetherMatrix, playersByDistance[i].Item1, iSrc);
                    }

                    // red => 3 furthest
                    if (hasRed)
                    {
                        SetMatrixBit(ref RedTetherMatrix, iSrc, iSrc);
                        for (int i = 0; i < numPotentialTethers; ++i)
                            SetMatrixBit(ref RedTetherMatrix, playersByDistance[playersByDistance.Count - i - 1].Item1, iSrc);
                    }

                    playersByDistance.Clear();
                }

                // update explosion matrices (has to be done in a separate pass)
                for (int i = 0; i < self.RaidMembers.Length; ++i)
                {
                    if (ExtractVectorFromMatrix(BlueTetherMatrix, i) != 0)
                    {
                        SetMatrixVector(ref BlueExplosionMatrix, i, (byte)self.FindRaidMembersInRange(i, _blueExplosionRadius));
                    }
                    if (ExtractVectorFromMatrix(RedTetherMatrix, i) != 0)
                    {
                        SetMatrixVector(ref RedExplosionMatrix, i, (byte)self.FindRaidMembersInRange(i, _redExplosionRadius));
                    }
                }
            }

            public void DrawArena(P1S self)
            {
                if (!Active)
                    return;

                int pcIndex = self.FindPlayerSlot();
                if (pcIndex < 0)
                    return;
                var pc = self.RaidMembers[pcIndex]!;

                for (int i = 0; i < self.RaidMembers.Length; ++i)
                {
                    var actor = self.RaidMembers[i];
                    if (actor == null || actor.IsDead)
                        continue;

                    // draw tethers
                    var blueTetheredTo = ExtractVectorFromMatrix(BlueTetherMatrix, i);
                    var redTetheredTo = ExtractVectorFromMatrix(RedTetherMatrix, i);
                    var tetherMask = (byte)(blueTetheredTo | redTetheredTo);
                    if (tetherMask != 0)
                    {
                        self.Arena.Actor(actor.Position, actor.Rotation, TetherColor(blueTetheredTo != 0, redTetheredTo != 0));
                        for (int j = 0; j < 8; ++j)
                        {
                            var target = self.RaidMembers[j];
                            if (target != null && i != j && IsVectorBitSet(tetherMask, j))
                            {
                                self.Arena.AddLine(actor.Position, target.Position, TetherColor(IsVectorBitSet(blueTetheredTo, j), IsVectorBitSet(redTetheredTo, j)));
                            }
                        }
                    }

                    // draw explosion circles that hit me
                    if (IsMatrixBitSet(BlueExplosionMatrix, i, pcIndex))
                        self.Arena.AddCircle(actor.Position, _blueExplosionRadius, _colorFail);
                    if (IsMatrixBitSet(RedExplosionMatrix, i, pcIndex))
                        self.Arena.AddCircle(actor.Position, _redExplosionRadius, _colorFail);
                }

                // draw explosion circles if I hit anyone
                if (ExtractVectorFromMatrix(BlueExplosionMatrix, pcIndex) != 0)
                    self.Arena.AddCircle(pc.Position, _blueExplosionRadius, _colorFail);
                if (ExtractVectorFromMatrix(RedExplosionMatrix, pcIndex) != 0)
                    self.Arena.AddCircle(pc.Position, _redExplosionRadius, _colorFail);
            }
        }

        private enum FlailsZone { None, Left, Right, Inner, Outer, UnknownCircle }
        private enum CellColor { None, Red, Blue }
        private enum KnockbackPhase { None, Knockback, AOE }

        private WorldState.Actor? _playerWithShacklesOfTime;
        private WorldState.Actor? _boss;
        private List<WorldState.Actor> _weaponsAnchor = new();
        private List<WorldState.Actor> _weaponsBall = new();
        private List<WorldState.Actor> _weaponsChakram = new();

        private Shackles _shackles = new();

        private bool _showShacklesOfTime;
        private bool _showFlails;
        private bool _showAetherflails;
        private FlailsZone _imminentFlails = FlailsZone.None;
        private FlailsZone _futureFlails = FlailsZone.None;
        private CellColor _aetherExplosion = CellColor.None;
        private KnockbackPhase _knockbackPhase = KnockbackPhase.None;
        private WorldState.Actor? _knockbackTarget;
        private WorldState.Actor? _knockbackAOETarget;
        private bool _knockbackAOEIsFlare; // true -> purge aka flare (stay away from MT), false -> grace aka holy (stack to MT)
        private byte _knockbackAOEPlayersInRange;
        private string _hint = "";
        private string _problems = "";

        // various constants, should probably be looked up in sheets...
        private float _knockbackDistance = 10;
        private float _knockbackFlareRange = 20;
        private float _knockbackHolyRange = 6;
        private float _flailCircleRadius = 10;
        private float _flailConeHalfAngle = MathF.PI / 4;

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

            s = BuildFlailStates(ref s.Next, 4);
            s = BuildKnockbackStates(ref s.Next, 5);
            s = BuildFlailStates(ref s.Next, 3);
            s = BuildWarderWrathState(ref s.Next, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11);
            s = BuildWarderWrathState(ref s.Next, 1, true);
            s = BuildWarderWrathState(ref s.Next, 5, true, "Cube2"); // cube2 and aoe start happen at almost same time
            s = CommonStates.Timeout(ref s.Next, 6, "Cube3");

            s = BuildKnockbackStates(ref s.Next, 5);

            s = BuildCellsState(ref s.Next, 8);
            s = BuildAetherflailStates(ref s.Next, 8);
            s = BuildKnockbackStates(ref s.Next, 7);
            s = BuildAetherflailStates(ref s.Next, 2);
            s = CommonStates.CastStart(ref s.Next, () => _boss, AID.ShacklesOfTime, 4);
            s = BuildShacklesOfTimeCastEndState(ref s.Next);
            s = BuildTankbusterState(ref s.Next, 5, true);
            s = BuildShacklesOfTimeResolveState(ref s.Next, 5);
            s = BuildSlamShutState(ref s.Next, 1);

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

            s = BuildWarderWrathState(ref s.Next, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11);
            s = BuildFlailStartState(ref s.Next, 3);
            s = CommonStates.Timeout(ref s.Next, 8, "Cube2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildFlailEndState(ref s.Next, 3, true);
            s = CommonStates.Timeout(ref s.Next, 4, "Cube3");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, 3);

            s = BuildCellsState(ref s.Next, 11);

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = BuildShacklesCastEndState(ref s1b);
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = BuildShacklesResolveState(ref s1e.Next, 0);
            s1e = BuildWarderWrathState(ref s1e.Next, 7);
            s1e = CommonStates.CastStart(ref s1e.Next, () => _boss, AID.ShacklesOfTime, 6);
            s1e = BuildShacklesOfTimeCastEndState(ref s1e.Next);
            s1e = BuildKnockbackStates(ref s1e.Next, 2, true);
            s1e = BuildShacklesOfTimeResolveState(ref s1e.Next, 3);
            s1e = BuildWarderWrathState(ref s1e.Next, 3);

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
            var fork = CommonStates.CastStart(ref s.Next, () => _boss, forkDispatch, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback");

            // forks merge
            s = BuildAetherflailStates(ref s1e.Next, 9);
            s2e.Next = s1e.Next;
            s = BuildAetherflailStates(ref s.Next, 6);
            s = BuildAetherflailStates(ref s.Next, 6);
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

            _problems = "";
            if (_showShacklesOfTime && UpdateShacklesOfTime())
            {
                _problems += "ShacklesOfTime failing! ";
            }
            if (_showFlails && UpdateFlails())
            {
                _problems += "Flails failing! ";
            }
            if (_showAetherflails && UpdateAetherflails())
            {
                _problems += "Aetherflails failing! ";
            }
            if (_knockbackPhase != KnockbackPhase.None && UpdateKnockback())
            {
                _problems += "Knockback failing! ";
            }
        }

        protected override void DrawHeader()
        {
            ImGui.Text(_hint);

            // TODO: shackles problem?..
            if (_problems.Length > 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), _problems);
            }
        }

        protected override void DrawArena()
        {
            if (_showShacklesOfTime && _playerWithShacklesOfTime != null && _playerWithShacklesOfTime.InstanceID != WorldState.PlayerActorID)
            {
                DrawAetherAOE(CellFromPosition(_playerWithShacklesOfTime.Position));
            }

            if (_showFlails)
            {
                DrawFlailAOE(_imminentFlails);
            }

            if (_showAetherflails)
            {
                DrawAetherAOE(_aetherExplosion);
                DrawFlailAOE(_imminentFlails);
                DrawFlailAOE(_futureFlails);
            }

            Arena.Border();

            if (Arena.IsCircle)
            {
                // cells mode
                float diag = Arena.WorldHalfSize / 1.414214f;
                Arena.AddCircle(Arena.WorldCenter, 10, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldE, Arena.WorldW, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldN, Arena.WorldS, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, diag), Arena.WorldCenter - new Vector3(diag, 0, diag), Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, -diag), Arena.WorldCenter - new Vector3(diag, 0, -diag), Arena.ColorBorder);
            }

            if (_boss != null)
                Arena.Actor(_boss.Position, _boss.Rotation, 0xff0000ff);

            if (_knockbackPhase == KnockbackPhase.AOE && _knockbackAOETarget != null)
            {
                if (_knockbackAOETarget.InstanceID == WorldState.PlayerActorID)
                {
                    // draw all players to help with positioning
                    for (int i = 0; i < RaidMembers.Length; ++i)
                    {
                        var p = RaidMembers[i];
                        if (p != null && !p.IsDead)
                            Arena.Actor(p.Position, p.Rotation, (_knockbackAOEPlayersInRange & (1 << i)) != 0 ? 0xffff0080 : 0xff808080);
                    }
                }

                // draw AOE source
                Arena.Actor(_knockbackAOETarget.Position, _knockbackAOETarget.Rotation, 0xff8080ff);
                Arena.AddCircle(_knockbackAOETarget.Position, _knockbackAOEIsFlare ? _knockbackFlareRange : _knockbackHolyRange, 0xff00ffff);
            }

            // draw player
            var pc = WorldState.FindPlayer();
            if (pc != null)
                Arena.Actor(pc.Position, pc.Rotation, 0xff00ff00);

            _shackles.DrawArena(this);

            if (_playerWithShacklesOfTime != null && _playerWithShacklesOfTime.InstanceID != WorldState.PlayerActorID)
            {
                Arena.Actor(_playerWithShacklesOfTime.Position, _playerWithShacklesOfTime.Rotation, 0xff8080ff);
            }
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

            if (RaidMembers[index] == _playerWithShacklesOfTime)
                _playerWithShacklesOfTime = null;
        }

        protected override void Reset()
        {
            Arena.IsCircle = false;
            _showShacklesOfTime = _showFlails = _showAetherflails = false;
            _imminentFlails = _futureFlails = FlailsZone.None;
            _aetherExplosion = CellColor.None;
            _knockbackPhase = KnockbackPhase.None;
            _knockbackTarget = _knockbackAOETarget = null;
            _hint = "";
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
            s.Exit = () => _showShacklesOfTime = true;
            return s;
        }

        // delay from cast-end is 15 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesOfTimeResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Timeout(ref link, delay, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            s.Exit = () => _showShacklesOfTime = false;
            return s;
        }

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, float delay)
        {
            Action<FlailsZone, FlailsZone> showFlails = (first, second) =>
            {
                _imminentFlails = first;
                _futureFlails = second;
                _showFlails = true;
            };
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.GaolerFlailRL] = new(null, () => showFlails(FlailsZone.Right, FlailsZone.Left));
            dispatch[AID.GaolerFlailLR] = new(null, () => showFlails(FlailsZone.Left, FlailsZone.Right));
            dispatch[AID.GaolerFlailIO1] = new(null, () => showFlails(FlailsZone.Inner, FlailsZone.Outer));
            dispatch[AID.GaolerFlailIO2] = new(null, () => showFlails(FlailsZone.Inner, FlailsZone.Outer));
            dispatch[AID.GaolerFlailOI1] = new(null, () => showFlails(FlailsZone.Outer, FlailsZone.Inner));
            dispatch[AID.GaolerFlailOI2] = new(null, () => showFlails(FlailsZone.Outer, FlailsZone.Inner));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            return start;
        }

        // if group continues, positioning flag is not cleared
        private StateMachine.State BuildFlailEndState(ref StateMachine.State? link, float castTimeLeft, bool continueGroup)
        {
            var end = CommonStates.CastEnd(ref link, () => _boss, castTimeLeft);
            end.Exit = () =>
            {
                _imminentFlails = _futureFlails;
                _futureFlails = FlailsZone.None;
            };
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Flails");
            resolve.EndHint |= continueGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () =>
            {
                _imminentFlails = _futureFlails = FlailsZone.None;
                _showFlails = false;
            };
            return resolve;
        }

        private StateMachine.State BuildFlailStates(ref StateMachine.State? link, float delay)
        {
            var start = BuildFlailStartState(ref link, delay);
            return BuildFlailEndState(ref start.Next, 12, false);
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, float delay)
        {
            Action<FlailsZone, FlailsZone> showAetherflails = (first, second) =>
            {
                _imminentFlails = first;
                _futureFlails = second;
                _showAetherflails = true;
            };
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AetherflailRX] = new(null, () => showAetherflails(FlailsZone.Right, FlailsZone.UnknownCircle));
            dispatch[AID.AetherflailLX] = new(null, () => showAetherflails(FlailsZone.Left, FlailsZone.UnknownCircle));
            dispatch[AID.AetherflailIL] = new(null, () => showAetherflails(FlailsZone.Inner, FlailsZone.Left));
            dispatch[AID.AetherflailIR] = new(null, () => showAetherflails(FlailsZone.Inner, FlailsZone.Right));
            dispatch[AID.AetherflailOL] = new(null, () => showAetherflails(FlailsZone.Outer, FlailsZone.Left));
            dispatch[AID.AetherflailOR] = new(null, () => showAetherflails(FlailsZone.Outer, FlailsZone.Right));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 12);
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () =>
            {
                _imminentFlails = _futureFlails = FlailsZone.None;
                _showAetherflails = false;
            };
            return resolve;
        }

        // part of group => group-with-next hint + no positioning hints
        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            Action<bool> startKnockback = isFlare => {
                _knockbackPhase = KnockbackPhase.Knockback;
                _knockbackTarget = _boss?.CastInfo != null ? WorldState.FindActor(_boss.CastInfo.TargetID) : null;
                _knockbackAOEIsFlare = isFlare;
            };
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.KnockbackGrace] = new(null, () => startKnockback(false));
            dispatch[AID.KnockbackPurge] = new(null, () => startKnockback(true));
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            if (!partOfGroup)
                start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;
            end.Exit = () => _knockbackPhase = KnockbackPhase.AOE;
            var resolve = CommonStates.Timeout(ref end.Next, 5, "Explode");
            resolve.EndHint |= partOfGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => { _knockbackPhase = KnockbackPhase.None; _knockbackTarget = null; };
            return resolve;
        }

        // intemperance cast start/end + explosion start/end + first resolve
        private StateMachine.State BuildIntemperanceExplosionStart(ref StateMachine.State? link, float delay)
        {
            // TODO: determine cubes...
            var intemp = CommonStates.Cast(ref link, () => _boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.IntemperateTormentUp] = new(null, () => _hint = "Explosion order: bottom->top");
            dispatch[AID.IntemperateTormentDown] = new(null, () => _hint = "Explosion order: top->bottom");
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
            var s = CommonStates.Cast(ref link, () => _boss, AID.SlamShut, delay, 6, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => Arena.IsCircle = false;
            return s;
        }

        private CellColor CellFromPosition(Vector3 pos)
        {
            if (pos == Arena.WorldCenter)
                return CellColor.None;

            var offset = pos - Arena.WorldCenter;
            var phi = MathF.Atan2(offset.Z, offset.X) + MathF.PI;
            int coneIndex = (int)(4 * phi / MathF.PI); // phi / (pi/4); range [0, 8]
            bool oddCone = (coneIndex & 1) != 0;
            bool outerCone = offset.LengthSquared() > 10 * 10;
            return (oddCone != outerCone) ? CellColor.Blue : CellColor.Red; // inner odd = blue, outer even = blue
        }

        // returns whether there any problems
        private bool UpdateShacklesOfTime()
        {
            var pc = WorldState.FindActor(WorldState.PlayerActorID);
            if (_playerWithShacklesOfTime == null || pc == null || pc == _playerWithShacklesOfTime)
                return false;

            return CellFromPosition(pc.Position) == CellFromPosition(_playerWithShacklesOfTime.Position);
        }

        private bool IsInFlailAOE(FlailsZone zone, Vector3 pos)
        {
            if (_boss == null)
                return false;

            Func<float, bool> inConeSafeZone = (float axisDir) =>
            {
                float safeZoneRot = _boss.Rotation + axisDir;
                var safeZoneDir = new Vector3(MathF.Sin(safeZoneRot), 0, MathF.Cos(safeZoneRot));
                float cosToPlayer = Vector3.Dot(Vector3.Normalize(pos - _boss.Position), safeZoneDir);
                return cosToPlayer > MathF.Cos(_flailConeHalfAngle);
            };
            switch (zone)
            {
                case FlailsZone.Left:
                    return !inConeSafeZone(-MathF.PI / 2);
                case FlailsZone.Right:
                    return !inConeSafeZone(MathF.PI / 2);
                case FlailsZone.Inner:
                    return (pos - _boss.Position).LengthSquared() <= _flailCircleRadius * _flailCircleRadius;
                case FlailsZone.Outer:
                    return (pos - _boss.Position).LengthSquared() >= _flailCircleRadius * _flailCircleRadius;
            }
            return false;
        }

        private bool UpdateFlails()
        {
            var pc = WorldState.FindActor(WorldState.PlayerActorID);
            if (pc == null)
                return false;

            return IsInFlailAOE(_imminentFlails, pc.Position);
        }

        private bool UpdateAetherflails()
        {
            if (_futureFlails == FlailsZone.UnknownCircle && _weaponsBall.Count + _weaponsChakram.Count > 0)
            {
                if (_weaponsBall.Count > 0 && _weaponsChakram.Count > 0)
                {
                    Service.Log($"[P1S] Failed to determine second aetherflail: there are {_weaponsBall.Count} balls and {_weaponsChakram.Count} chakrams");
                }
                else
                {
                    _futureFlails = _weaponsBall.Count > 0 ? FlailsZone.Inner : FlailsZone.Outer;
                }
            }

            var pc = WorldState.FindActor(WorldState.PlayerActorID);
            if (pc == null)
                return false;

            return IsInFlailAOE(_imminentFlails, pc.Position) || IsInFlailAOE(_futureFlails, pc.Position) || CellFromPosition(pc.Position) == _aetherExplosion;
        }

        private bool UpdateKnockback()
        {
            _knockbackAOEPlayersInRange = 0;
            var pc = WorldState.FindActor(WorldState.PlayerActorID);
            if (_boss == null || pc == null)
                return false;

            // TODO: remove this debug code...
            if (_knockbackTarget != null)
                Service.Log($"[P1S] Debug: {_knockbackTarget.InstanceID:X} pos {Utils.Vec3String(_knockbackTarget.Position)}");

            if (_knockbackPhase == KnockbackPhase.Knockback && _boss.CastInfo != null && _boss.CastInfo.TargetID == WorldState.PlayerActorID)
            {
                var dir = Vector3.Normalize(pc.Position - _boss.Position);
                var newPos = pc.Position + dir * _knockbackDistance;
                return !Arena.InBounds(newPos);
            }
            else if (_knockbackPhase == KnockbackPhase.AOE)
            {
                var knockbackAOETargetSlot = FindRaidMemberSlot(_boss.TargetID);
                if (knockbackAOETargetSlot < 0)
                    return false;
                _knockbackAOETarget = RaidMembers[knockbackAOETargetSlot];
                if (_knockbackAOETarget == null)
                    return false;

                float aoeRange = _knockbackAOEIsFlare ? _knockbackFlareRange : _knockbackHolyRange;
                _knockbackAOEPlayersInRange = (byte)FindRaidMembersInRange(knockbackAOETargetSlot, aoeRange);
                if (_boss.TargetID != WorldState.PlayerActorID)
                {
                    // boss won't hit me with flare/holy - I must either stack or gtfo from target
                    bool inAOE = (_knockbackAOETarget.Position - pc.Position).LengthSquared() <= aoeRange * aoeRange;
                    bool shouldBeInAOE = pc != _knockbackTarget && !_knockbackAOEIsFlare;
                    return inAOE != shouldBeInAOE;
                }
                else if (pc == _knockbackTarget || _knockbackAOEIsFlare)
                {
                    // boss will hit me with flare/holy,
                    // and he also hit me with knockback - so assume I'm taking it with invulnerability, so no one should be inside my AOE
                    // or he hit co-tank with knockback and I should now eat flare alone
                    return _knockbackAOEPlayersInRange != 0;
                }
                else
                {
                    // boss will hit me with flare/holy,
                    // and he hit co-tank with knockback and now I'm about to take a holy - co-tank should not be in range, others should stack (TODO determine min targets?..)
                    bool coTankHit = _knockbackTarget != null ? (_knockbackTarget.Position - pc.Position).LengthSquared() <= aoeRange * aoeRange : false;
                    return coTankHit;
                }
            }
            return false;
        }

        private void DrawAetherAOE(CellColor color)
        {
            if (color == CellColor.None)
                return;

            float start = color == CellColor.Blue ? 0 : MathF.PI / 4;
            for (int i = 0; i < 4; ++i)
            {
                Arena.ZoneCone(Arena.WorldCenter, 0, 10, start, start + MathF.PI / 4, Arena.ColorDanger);
                Arena.ZoneCone(Arena.WorldCenter, 10, 20, start + MathF.PI / 4, start + MathF.PI / 2, Arena.ColorDanger);
                start += MathF.PI / 2;
            }
        }

        private void DrawFlailAOE(FlailsZone zone)
        {
            if (_boss == null)
                return;

            switch (zone)
            {
                case FlailsZone.Left:
                    Arena.ZoneCone(_boss.Position, 0, 100, _boss.Rotation - MathF.PI / 2 + _flailConeHalfAngle, _boss.Rotation + 3 * MathF.PI / 2 - _flailConeHalfAngle, Arena.ColorDanger);
                    break;
                case FlailsZone.Right:
                    Arena.ZoneCone(_boss.Position, 0, 100, _boss.Rotation + MathF.PI / 2 - _flailConeHalfAngle, _boss.Rotation - 3 * MathF.PI / 2 + _flailConeHalfAngle, Arena.ColorDanger);
                    break;
                case FlailsZone.Inner:
                    Arena.ZoneCircle(_boss.Position, _flailCircleRadius, Arena.ColorDanger);
                    break;
                case FlailsZone.Outer:
                    Arena.ZoneCone(_boss.Position, _flailCircleRadius, 100, 0, 2 * MathF.PI, Arena.ColorDanger);
                    break;
            }
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
                                _aetherExplosion = CellColor.Red;
                                break;
                            case 0x4D:
                                _aetherExplosion = CellColor.Blue;
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
                    if (_playerWithShacklesOfTime != null)
                        Service.Log($"[P1S] Unexpected ShacklesOfTime on {Utils.ObjectString(arg.actor.InstanceID)} while another is up on {Utils.ObjectString(_playerWithShacklesOfTime.InstanceID)}");
                    _playerWithShacklesOfTime = arg.actor;
                    break;
                case SID.ShacklesOfCompanionship:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, false, true);
                    break;
                case SID.ShacklesOfLoneliness:
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
                        _aetherExplosion = CellColor.None;
                    break;
                case SID.ShacklesOfTime:
                    if (arg.actor == _playerWithShacklesOfTime)
                        _playerWithShacklesOfTime = null;
                    break;
                case SID.ShacklesOfCompanionship:
                    _shackles.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, false, false);
                    break;
                case SID.ShacklesOfLoneliness:
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
