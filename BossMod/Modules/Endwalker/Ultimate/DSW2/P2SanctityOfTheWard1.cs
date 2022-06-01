using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // eyes mechanics are quite standalone...
    class P2SanctityOfTheWard1Gaze : CommonComponents.CastCounter
    {
        private Vector3? _eyePosition;

        private static float _eyeOuterH = 10;
        private static float _eyeOuterV = 6;
        private static float _eyeInnerR = 4;
        private static float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
        private static float _eyeOffsetV = _eyeOuterR - _eyeOuterV;
        private static float _eyeHalfAngle = MathF.Asin(_eyeOuterH / _eyeOuterR);

        public P2SanctityOfTheWard1Gaze() : base(ActionID.MakeSpell(AID.DragonsGazeAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            var actorForward = GeometryUtils.DirectionToVec3(actor.Rotation);
            if (EyePositions(module).Any(eye => Vector3.Dot(eye - actor.Position, actorForward) >= 0))
                hints.Add("Turn away from gaze!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var eye in EyePositions(module))
            {
                var dir = Vector3.Normalize(eye - arena.WorldCenter);
                var eyeCenter = arena.ScreenCenter + arena.RotatedCoords(dir.XZ()) * (arena.ScreenHalfSize + arena.ScreenMarginSize / 2);
                var dl = ImGui.GetWindowDrawList();
                dl.PathArcTo(eyeCenter - new Vector2(0, _eyeOffsetV), _eyeOuterR,  MathF.PI / 2 + _eyeHalfAngle,  MathF.PI / 2 - _eyeHalfAngle);
                dl.PathArcTo(eyeCenter + new Vector2(0, _eyeOffsetV), _eyeOuterR, -MathF.PI / 2 + _eyeHalfAngle, -MathF.PI / 2 - _eyeHalfAngle);
                dl.PathFillConvex(arena.ColorEnemy);
                dl.AddCircleFilled(eyeCenter, _eyeInnerR, arena.ColorBorder);
            }
        }

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
            if (featureID == 0x8003759A && state == 0x00020001 && index <= 7)
            {
                _eyePosition = module.Arena.WorldCenter + 40 * GeometryUtils.DirectionToVec3(MathF.PI - index * MathF.PI / 4);
            }
        }

        private IEnumerable<Vector3> EyePositions(BossModule module)
        {
            if (_eyePosition != null && NumCasts == 0)
            {
                yield return _eyePosition.Value;
                yield return module.PrimaryActor.Position;
            }
        }
    }

    // 'sever' (between 1/2 markers with shared damage), 'charge' (aka shining blade) + 'flare' (cw/ccw leaving exploding orbs)
    class P2SanctityOfTheWard1 : BossModule.Component
    {
        class ChargeInfo
        {
            public Actor Source;
            public List<Vector3> Positions = new();
            public List<Vector3> Spheres = new();

            public ChargeInfo(Actor source)
            {
                Source = source;
            }
        }

        public int NumSeverCasts { get; private set; }
        public int NumFlareCasts { get; private set; }
        private DSW2Config _config;
        private float _severStartDir;
        private int[] _severTargetSlots = { -1, -1 };
        private ChargeInfo?[] _charges = { null, null };
        private bool _chargeCW;
        private bool _chargeEarly;
        private ulong _groupEast; // 0 until initialized
        private string _groupSwapHints = "";

        private static float _severRadius = 6;
        private static float _chargeHalfWidth = 3;
        private static float _brightflareRadius = 9;

        public P2SanctityOfTheWard1()
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (NumSeverCasts < 4)
            {
                if (_severTargetSlots.Contains(slot))
                {
                    // TODO: check 'far enough'?
                    if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _severRadius).Count() < 3)
                        hints.Add("Stack in fours!");
                }
                else
                {
                    if (!_severTargetSlots.Select(slot => module.Raid[slot]).Any(s => s == null || GeometryUtils.PointInCircle(s.Position - actor.Position, _severRadius)))
                        hints.Add("Stack in fours!");
                }
            }

            if (ImminentCharges().Any(fromTo => GeometryUtils.PointInRect(actor.Position - fromTo.Item1, fromTo.Item2 - fromTo.Item1, _chargeHalfWidth)))
                hints.Add("GTFO from charge!");
            if (ImminentSpheres().Any(s => GeometryUtils.PointInCircle(actor.Position - s, _brightflareRadius)))
                hints.Add("GTFO from sphere!");

            if (movementHints != null && _groupEast != 0)
            {
                var from = actor.Position;
                var color = module.Arena.ColorSafe;
                foreach (var safespot in MovementHintOffsets(slot))
                {
                    var to = module.Arena.WorldCenter + safespot;
                    movementHints.Add(from, to, color);
                    from = to;
                    color = module.Arena.ColorDanger;
                }
            }
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (_groupSwapHints.Length > 0)
                hints.Add($"Swap: {_groupSwapHints}");
            if (_charges[0] != null && _charges[1] != null)
                hints.Add($"Move: {(_chargeCW ? "clockwise" : "counterclockwise")} {(_chargeEarly ? "early" : "late")}");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (from, to) in ImminentCharges())
                arena.ZoneQuad(from, to, _chargeHalfWidth, arena.ColorAOE);
            foreach (var sphere in ImminentSpheres())
                arena.ZoneCircle(sphere, _brightflareRadius, arena.ColorAOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumSeverCasts < 4)
            {
                var source = module.Enemies(OID.SerZephirin).FirstOrDefault();
                arena.Actor(source, arena.ColorEnemy);

                var target = module.Raid[_severTargetSlots[NumSeverCasts % 2]];
                if (source != null && target != null)
                    arena.AddLine(source.Position, target.Position, arena.ColorDanger);

                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    if (_severTargetSlots.Contains(slot))
                    {
                        arena.Actor(player, arena.ColorPlayerInteresting);
                        arena.AddCircle(player.Position, _severRadius, arena.ColorDanger);
                    }
                    else
                    {
                        arena.Actor(player, arena.ColorPlayerGeneric);
                    }
                }
            }

            foreach (var c in _charges)
                if (c?.Positions.Count > 1)
                    arena.Actor(c.Source, arena.ColorEnemy);

            foreach (var safespot in MovementHintOffsets(pcSlot))
            {
                arena.AddCircle(arena.WorldCenter + safespot, 2, arena.ColorSafe);
                if (_groupEast == 0)
                    arena.AddCircle(arena.WorldCenter - safespot, 2, arena.ColorSafe); // if there are no valid assignments, draw spots for both groups
                break; // only draw immediate safespot here
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (!info.IsSpell())
                return;
            switch ((AID)info.Action.ID)
            {
                case AID.SacredSever:
                    ++NumSeverCasts;
                    break;
                case AID.ShiningBlade:
                    var charge = Array.Find(_charges, c => c?.Source.InstanceID == info.CasterID);
                    if (charge?.Positions.Count > 0)
                        charge.Positions.RemoveAt(0);
                    break;
                case AID.BrightFlare:
                    var sphere = module.WorldState.Actors.Find(info.CasterID);
                    if (sphere != null)
                        foreach (var c in _charges)
                            c?.Spheres.RemoveAll(s => Utils.AlmostEqual(s, sphere.Position, 3));
                    ++NumFlareCasts;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.SacredSever1:
                    _severTargetSlots[0] = module.Raid.FindSlot(actorID);
                    InitChargesAndSafeSpots(module);
                    break;
                case IconID.SacredSever2:
                    _severTargetSlots[1] = module.Raid.FindSlot(actorID);
                    InitChargesAndSafeSpots(module);
                    break;
            }
        }

        private void InitChargesAndSafeSpots(BossModule module)
        {
            if (_severTargetSlots[0] < 0 || _severTargetSlots[1] < 0)
                return; // wait for other event...

            var severSource = module.Enemies(OID.SerZephirin).FirstOrDefault();
            _severStartDir = severSource != null ? GeometryUtils.DirectionFromVec3(severSource.Position - module.Arena.WorldCenter) : 0;
            if (_severStartDir != 0)
            {
                _groupEast = _config.P2SanctityGroups.BuildGroupMask(1, module.Raid);
                if (_groupEast == 0)
                {
                    _groupSwapHints = "unconfigured";
                }
                else
                {
                    if (_config.P2SanctityRelative && _severStartDir < 0)
                    {
                        // swap groups for relative assignment if needed
                        _groupEast ^= 0xff;
                    }

                    if (_config.P2SanctitySwapRole == Role.None)
                    {
                        AssignmentSwapWithRolePartner(module, 0, _severStartDir < 0);
                        AssignmentSwapWithRolePartner(module, 1, _severStartDir > 0);
                    }
                    else
                    {
                        AssignmentReassignIfNeeded(0, _severStartDir < 0);
                        AssignmentReassignIfNeeded(1, _severStartDir > 0);
                        if (BitOperations.PopCount(_groupEast) != 4)
                        {
                            // to balance, unmarked player of designated role should swap
                            var (swapSlot, swapper) = module.Raid.WithSlot(true).FirstOrDefault(sa => sa.Item1 != _severTargetSlots[0] && sa.Item1 != _severTargetSlots[1] && sa.Item2.Role == _config.P2SanctitySwapRole);
                            if (swapper != null)
                            {
                                BitVector.ToggleVector64Bit(ref _groupEast, swapSlot);
                                _groupSwapHints = swapper.Name;
                            }
                        }
                    }

                    if (_groupSwapHints.Length == 0)
                        _groupSwapHints = "none";
                }
            }

            bool cw1, cw2;
            (_charges[0], cw1) = BuildChargeInfo(module, OID.SerAdelphel);
            (_charges[1], cw2) = BuildChargeInfo(module, OID.SerJanlenoux);
            if (_charges[0] == null || _charges[1] == null || cw1 != cw2)
            {
                module.ReportError(this, "Failed to initialize charges");
            }
            else
            {
                _chargeCW = cw1;
                // second safe spot could be either 3rd or 5th explosion
                float severDirEast = _severStartDir;
                if (severDirEast < 0)
                    severDirEast += MathF.PI;
                bool severDiagonalSE = severDirEast < MathF.PI / 2;
                _chargeEarly = severDiagonalSE == cw1;
            }
        }

        private void AssignmentReassignIfNeeded(int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == BitVector.IsVector64BitSet(_groupEast, slot))
                return; // target is already assigned to correct position, no need to swap
            BitVector.ToggleVector64Bit(ref _groupEast, slot);
        }

        private void AssignmentSwapWithRolePartner(BossModule module, int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == BitVector.IsVector64BitSet(_groupEast, slot))
                return; // target is already assigned to correct position, no need to swap
            var role = module.Raid[slot]?.Role ?? Role.None;
            var (partnerSlot, partner) = module.Raid.WithSlot(true).Exclude(slot).FirstOrDefault(sa => sa.Item2.Role == role);
            if (partner == null)
                return;

            BitVector.ToggleVector64Bit(ref _groupEast, slot);
            BitVector.ToggleVector64Bit(ref _groupEast, partnerSlot);

            if (_groupSwapHints.Length > 0)
                _groupSwapHints += ", ";
            _groupSwapHints += role.ToString();
        }

        private (ChargeInfo?, bool) BuildChargeInfo(BossModule module, OID oid)
        {
            var actor = module.Enemies(oid).FirstOrDefault();
            if (actor == null)
                return (null, false);

            // so far I've only seen both enemies starting at (+-5, 0)
            if (!Utils.AlmostEqual(actor.Position.Z, module.Arena.WorldCenter.Z, 1))
                return (null, false);
            if (!Utils.AlmostEqual(MathF.Abs(actor.Position.X - module.Arena.WorldCenter.X), 5, 1))
                return (null, false);

            bool right = actor.Position.X > module.Arena.WorldCenter.X;
            bool facingSouth = Utils.AlmostEqual(actor.Rotation, 0, 0.1f);
            bool cw = right == facingSouth;
            var res = new ChargeInfo(actor);
            float firstPointDir = actor.Rotation;
            float angleBetweenPoints = (cw ? -1 : 1) * 5 * MathF.PI / 8;

            res.Positions.Add(actor.Position);
            Action<float> addPosition = dir => res.Positions.Add(module.Arena.WorldCenter + 21 * GeometryUtils.DirectionToVec3(dir));
            addPosition(firstPointDir);
            addPosition(firstPointDir + angleBetweenPoints);
            addPosition(firstPointDir + angleBetweenPoints * 2);

            res.Spheres.Add(res.Positions[0]);
            res.Spheres.Add((res.Positions[0] + res.Positions[1]) / 2);
            res.Spheres.Add(res.Positions[1]);
            res.Spheres.Add((res.Positions[1] * 2 + res.Positions[2]) / 3);
            res.Spheres.Add((res.Positions[1] + res.Positions[2] * 2) / 3);
            res.Spheres.Add(res.Positions[2]);
            res.Spheres.Add((res.Positions[2] * 2 + res.Positions[3]) / 3);
            res.Spheres.Add((res.Positions[2] + res.Positions[3] * 2) / 3);
            res.Spheres.Add(res.Positions[3]);
            return (res, cw);
        }

        private IEnumerable<(Vector3, Vector3)> ImminentCharges()
        {
            foreach (var c in _charges)
            {
                if (c == null)
                    continue;
                for (int i = 1; i < c.Positions.Count; ++i)
                    yield return (c.Positions[i - 1], c.Positions[i]);
            }
        }

        private IEnumerable<Vector3> ImminentSpheres()
        {
            foreach (var c in _charges)
            {
                if (c == null)
                    continue;
                foreach (var s in c.Spheres.Take(6))
                    yield return s;
            }
        }

        private Vector3 SafeSpotOffset(int slot, float dirOffset)
        {
            float dir = _severStartDir + (_chargeCW ? -1 : 1) * dirOffset;
            if (dir < 0 == BitVector.IsVector64BitSet(_groupEast, slot))
                dir += MathF.PI;
            return 20 * GeometryUtils.DirectionToVec3(dir);
        }

        private IEnumerable<Vector3> MovementHintOffsets(int slot)
        {
            if (_severStartDir != 0 && _charges[0] != null && _charges[1] != null)
            {
                // second safe spot could be either 3rd or 5th explosion
                if (_charges[0]!.Spheres.Count > (_chargeEarly ? 6 : 4))
                    yield return SafeSpotOffset(slot, _chargeEarly ? MathF.PI / 12 : MathF.PI / 15.4f);
                if (_charges[0]!.Spheres.Count > 0)
                    yield return SafeSpotOffset(slot, MathF.PI / 5.4f);
            }
        }
    }
}
