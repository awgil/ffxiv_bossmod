using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2SanctityOfTheWard1 : BossModule.Component
    {
        class ChargeInfo
        {
            public Actor Source;
            public List<Vector3> Positions = new();
            public List<Vector3> Spheres = new();
            public bool Clockwise;

            public ChargeInfo(Actor source, bool clockwise)
            {
                Source = source;
                Clockwise = clockwise;
            }
        }

        public bool GazeDone { get; private set; }
        public int NumSeverCasts { get; private set; }
        public int NumFlareCasts { get; private set; }
        private DSW2Config _config;
        private Vector3? _eyePosition;
        private float? _severStartDir;
        private int[] _severTargetSlots = { -1, -1 };
        private ChargeInfo?[] _charges = { null, null };
        private ulong _assignedGroupEast;
        private ulong _actualGroupEast;

        private static float _severRadius = 6;
        private static float _chargeHalfWidth = 3;
        private static float _brightflareRadius = 9;

        private static float _eyeOuterH = 10;
        private static float _eyeOuterV = 6;
        private static float _eyeInnerR = 4;
        private static float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
        private static float _eyeOffsetV = _eyeOuterR - _eyeOuterV;
        private static float _eyeHalfAngle = MathF.Asin(_eyeOuterH / _eyeOuterR);

        public P2SanctityOfTheWard1()
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public override void Init(BossModule module)
        {
            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(module.WorldState.Party);
            if (_config.P2SanctityGroups.Validate() && assignments.Length == _config.P2SanctityGroups.Assignments.Length)
            {
                for (int i = 0; i < assignments.Length; ++i)
                {
                    if (_config.P2SanctityGroups.Assignments[i] != 0)
                    {
                        BitVector.SetVector64Bit(ref _assignedGroupEast, assignments[i]);
                    }
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_eyePosition != null)
            {
                var actorForward = GeometryUtils.DirectionToVec3(actor.Rotation);
                if (FacingEye(actor.Position, actorForward, _eyePosition.Value) || FacingEye(actor.Position, actorForward, module.PrimaryActor.Position))
                    hints.Add("Turn away from gaze!");
            }

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

            if (movementHints != null && _actualGroupEast != 0)
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
            if (_assignedGroupEast == 0)
            {
                hints.Add("Group assignments not configured");
            }
            else if (_actualGroupEast != 0)
            {
                var swapped = _assignedGroupEast ^ _actualGroupEast;
                BitVector.ClearVector64Bit(ref swapped, _severTargetSlots[0]);
                BitVector.ClearVector64Bit(ref swapped, _severTargetSlots[1]);
                if (swapped == 0)
                {
                    hints.Add("Swap: none");
                }
                else if (_config.P2SanctitySwapRole == Role.None)
                {
                    hints.Add($"Swap: {string.Join(", ", module.Raid.WithSlot(true).IncludedInMask(swapped).Select(ip => ip.Item2.Role.ToString()))}");
                }
                else
                {
                    hints.Add($"Swap: {module.Raid.WithSlot(true).IncludedInMask(swapped).FirstOrDefault().Item2?.Name}");
                }
            }

            if (_charges[0] != null)
            {
                hints.Add($"Move: {(_charges[0]!.Clockwise ? "clockwise" : "counterclockwise")}");
            }
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
            if (_eyePosition != null)
            {
                DrawEye(arena, _eyePosition.Value);
                DrawEye(arena, module.PrimaryActor.Position);
            }

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
            {
                if (c != null && c.Positions.Count > 1)
                {
                    arena.Actor(c.Source, arena.ColorEnemy);
                }
            }

            foreach (var safespot in MovementHintOffsets(pcSlot))
            {
                arena.AddCircle(arena.WorldCenter + safespot, 2, arena.ColorSafe);
                if (_actualGroupEast == 0)
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
                case AID.DragonsGazeAOE:
                case AID.DragonsGlory:
                    _eyePosition = null;
                    GazeDone = true;
                    break;
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
                            if (c != null)
                                c.Spheres.RemoveAll(s => Utils.AlmostEqual(s, sphere.Position, 3));
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

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
            if (featureID == 0x8003759A && state == 0x00020001 && index <= 7)
            {
                _eyePosition = module.Arena.WorldCenter + 40 * GeometryUtils.DirectionToVec3(MathF.PI - index * MathF.PI / 4);
            }
        }

        private bool FacingEye(Vector3 actorPosition, Vector3 actorDirection, Vector3 eyePosition)
        {
            return Vector3.Dot(eyePosition - actorPosition, actorDirection) >= 0;
        }

        private void DrawEye(MiniArena arena, Vector3 position)
        {
            var dir = Vector3.Normalize(position - arena.WorldCenter);
            var eyeCenter = arena.ScreenCenter + arena.RotatedCoords(dir.XZ()) * (arena.ScreenHalfSize + arena.ScreenMarginSize / 2);
            var dl = ImGui.GetWindowDrawList();
            dl.PathArcTo(eyeCenter - new Vector2(0, _eyeOffsetV), _eyeOuterR,  MathF.PI / 2 + _eyeHalfAngle,  MathF.PI / 2 - _eyeHalfAngle);
            dl.PathArcTo(eyeCenter + new Vector2(0, _eyeOffsetV), _eyeOuterR, -MathF.PI / 2 + _eyeHalfAngle, -MathF.PI / 2 - _eyeHalfAngle);
            dl.PathFillConvex(arena.ColorEnemy);
            dl.AddCircleFilled(eyeCenter, _eyeInnerR, arena.ColorBorder);
        }

        private void InitChargesAndSafeSpots(BossModule module)
        {
            if (_severStartDir == null)
            {
                var source = module.Enemies(OID.SerZephirin).FirstOrDefault();
                if (source != null)
                    _severStartDir = GeometryUtils.DirectionFromVec3(source.Position - module.Arena.WorldCenter);
            }

            if (_charges[0] == null)
                _charges[0] = BuildChargeInfo(module, OID.SerAdelphel);
            if (_charges[1] == null)
                _charges[1] = BuildChargeInfo(module, OID.SerJanlenoux);

            if (_severTargetSlots[0] >= 0 && _severTargetSlots[1] >= 0 && _assignedGroupEast != 0 && _severStartDir != null)
            {
                bool firstTargetGoesEast = _severStartDir.Value < 0;
                bool secondTargetGoesEast = !firstTargetGoesEast;

                _actualGroupEast = _assignedGroupEast;
                if (_config.P2SanctitySwapRole == Role.None)
                {
                    AssignmentSwapWithRolePartner(module, 0, firstTargetGoesEast);
                    AssignmentSwapWithRolePartner(module, 1, secondTargetGoesEast);
                }
                else
                {
                    AssignmentReassignIfNeeded(0, firstTargetGoesEast);
                    AssignmentReassignIfNeeded(1, secondTargetGoesEast);
                    if (BitOperations.PopCount(_actualGroupEast) != 4)
                    {
                        // to balance, unmarked player of designated role should swap
                        var (swapSlot, swapper) = module.Raid.WithSlot(true).FirstOrDefault(sa => sa.Item1 != _severTargetSlots[0] && sa.Item1 != _severTargetSlots[1] && sa.Item2.Role == _config.P2SanctitySwapRole);
                        if (swapper != null)
                            BitVector.ToggleVector64Bit(ref _actualGroupEast, swapSlot);
                    }
                }
            }
        }

        private void AssignmentReassignIfNeeded(int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == BitVector.IsVector64BitSet(_actualGroupEast, slot))
                return; // target is already assigned to correct position, no need to swap
            BitVector.ToggleVector64Bit(ref _actualGroupEast, slot);
        }

        private void AssignmentSwapWithRolePartner(BossModule module, int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == BitVector.IsVector64BitSet(_actualGroupEast, slot))
                return; // target is already assigned to correct position, no need to swap
            var role = module.Raid[slot]?.Role ?? Role.None;
            var (partnerSlot, partner) = module.Raid.WithSlot(true).Exclude(slot).FirstOrDefault(sa => sa.Item2.Role == role);
            if (partner == null)
                return;
            BitVector.ToggleVector64Bit(ref _actualGroupEast, slot);
            BitVector.ToggleVector64Bit(ref _actualGroupEast, partnerSlot);
        }

        private ChargeInfo? BuildChargeInfo(BossModule module, OID oid)
        {
            var actor = module.Enemies(oid).FirstOrDefault();
            if (actor == null)
                return null;

            // so far I've only seen both enemies starting at (+-5, 0)
            if (!Utils.AlmostEqual(actor.Position.Z, module.Arena.WorldCenter.Z, 1))
                return null;
            if (!Utils.AlmostEqual(MathF.Abs(actor.Position.X - module.Arena.WorldCenter.X), 5, 1))
                return null;

            bool right = actor.Position.X > module.Arena.WorldCenter.X;
            bool facingSouth = Utils.AlmostEqual(actor.Rotation, 0, 0.1f);
            var res = new ChargeInfo(actor, right == facingSouth);
            float firstPointDir = actor.Rotation;
            float angleBetweenPoints = (res.Clockwise ? -1 : 1) * 5 * MathF.PI / 8;

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
            return res;
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

        private Vector3 SafeSpotOffset(int slot, float dir)
        {
            if (dir < 0 == BitVector.IsVector64BitSet(_actualGroupEast, slot))
                dir += MathF.PI;
            return 20 * GeometryUtils.DirectionToVec3(dir);
        }

        private IEnumerable<Vector3> MovementHintOffsets(int slot)
        {
            if (_severStartDir != null && _charges[0] != null && _charges[1] != null && _charges[0]!.Clockwise == _charges[1]!.Clockwise)
            {
                // second safe spot could be either 3rd or 5th explosion
                float severDirEast = _severStartDir.Value;
                if (severDirEast < 0)
                    severDirEast += MathF.PI;
                bool severDiagonalSE = severDirEast < MathF.PI / 2;
                bool moveIntoThird = severDiagonalSE == _charges[0]!.Clockwise;
                float moveOffset = _charges[0]!.Clockwise ? -1 : 1;
                if (_charges[0]!.Spheres.Count > (moveIntoThird ? 6 : 4))
                    yield return SafeSpotOffset(slot, _severStartDir.Value + moveOffset * (moveIntoThird ? MathF.PI / 12 : MathF.PI / 15.4f));
                if (_charges[0]!.Spheres.Count > 0)
                    yield return SafeSpotOffset(slot, _severStartDir.Value + moveOffset * MathF.PI / 5.4f);
            }
        }
    }
}
