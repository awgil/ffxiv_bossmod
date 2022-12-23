using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // eyes mechanics are quite standalone...
    class P2SanctityOfTheWard1Gaze : Components.GenericGaze
    {
        private WPos? _eyePosition;

        public P2SanctityOfTheWard1Gaze() : base(ActionID.MakeSpell(AID.DragonsGazeAOE)) { }

        public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
        {
            // TODO: activation time
            if (_eyePosition != null && NumCasts == 0)
            {
                yield return new(_eyePosition.Value);
                yield return new(module.PrimaryActor.Position);
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
            if (directorID == 0x8003759A && state == 0x00020001 && index <= 7)
            {
                _eyePosition = module.Bounds.Center + 40 * (180 - index * 45).Degrees().ToDirection();
            }
        }
    }

    // 'sever' (between 1/2 markers with shared damage), 'charge' (aka shining blade) + 'flare' (cw/ccw leaving exploding orbs)
    class P2SanctityOfTheWard1 : BossComponent
    {
        class ChargeInfo
        {
            public Actor Source;
            public List<WPos> Positions = new();
            public List<WPos> Spheres = new();

            public ChargeInfo(Actor source)
            {
                Source = source;
            }
        }

        public int NumSeverCasts { get; private set; }
        public int NumFlareCasts { get; private set; }
        private DSW2Config _config;
        private Angle _severStartDir;
        private int[] _severTargetSlots = { -1, -1 };
        private ChargeInfo?[] _charges = { null, null };
        private bool _chargeCW;
        private bool _chargeEarly;
        private BitMask _groupEast; // 0 until initialized
        private string _groupSwapHints = "";

        private static float _severRadius = 6;
        private static float _chargeHalfWidth = 3;
        private static float _brightflareRadius = 9;

        public P2SanctityOfTheWard1()
        {
            _config = Service.Config.Get<DSW2Config>();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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
                    if (!_severTargetSlots.Select(slot => module.Raid[slot]).Any(s => s == null || s.Position.InCircle(actor.Position, _severRadius)))
                        hints.Add("Stack in fours!");
                }
            }

            if (ImminentCharges().Any(fromTo => actor.Position.InRect(fromTo.Item1, fromTo.Item2 - fromTo.Item1, _chargeHalfWidth)))
                hints.Add("GTFO from charge!");
            if (ImminentSpheres().Any(s => actor.Position.InCircle(s, _brightflareRadius)))
                hints.Add("GTFO from sphere!");

            if (movementHints != null && _groupEast.Any())
            {
                var from = actor.Position;
                var color = ArenaColor.Safe;
                foreach (var safespot in MovementHintOffsets(slot))
                {
                    var to = module.Bounds.Center + safespot;
                    movementHints.Add(from, to, color);
                    from = to;
                    color = ArenaColor.Danger;
                }
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_groupSwapHints.Length > 0)
                hints.Add($"Swap: {_groupSwapHints}");
            if (_charges[0] != null && _charges[1] != null)
                hints.Add($"Move: {(_chargeCW ? "clockwise" : "counterclockwise")} {(_chargeEarly ? "early" : "late")}");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (from, to) in ImminentCharges())
                arena.ZoneRect(from, to, _chargeHalfWidth, ArenaColor.AOE);
            foreach (var sphere in ImminentSpheres())
                arena.ZoneCircle(sphere, _brightflareRadius, ArenaColor.AOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumSeverCasts < 4)
            {
                var source = module.Enemies(OID.SerZephirin).FirstOrDefault();
                arena.Actor(source, ArenaColor.Enemy, true);

                var target = module.Raid[_severTargetSlots[NumSeverCasts % 2]];
                if (source != null && target != null)
                    arena.AddLine(source.Position, target.Position, ArenaColor.Danger);

                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    if (_severTargetSlots.Contains(slot))
                    {
                        arena.Actor(player, ArenaColor.PlayerInteresting);
                        arena.AddCircle(player.Position, _severRadius, ArenaColor.Danger);
                    }
                    else
                    {
                        arena.Actor(player, ArenaColor.PlayerGeneric);
                    }
                }
            }

            foreach (var c in _charges)
                if (c?.Positions.Count > 1)
                    arena.Actor(c.Source, ArenaColor.Enemy, true);

            foreach (var safespot in MovementHintOffsets(pcSlot))
            {
                arena.AddCircle(module.Bounds.Center + safespot, 2, ArenaColor.Safe);
                if (_groupEast.None())
                    arena.AddCircle(module.Bounds.Center - safespot, 2, ArenaColor.Safe); // if there are no valid assignments, draw spots for both groups
                break; // only draw immediate safespot here
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.SacredSever:
                    ++NumSeverCasts;
                    break;
                case AID.ShiningBlade:
                    var charge = Array.Find(_charges, c => c?.Source == caster);
                    if (charge?.Positions.Count > 0)
                        charge.Positions.RemoveAt(0);
                    break;
                case AID.BrightFlare:
                    foreach (var c in _charges)
                        c?.Spheres.RemoveAll(s => s.AlmostEqual(caster.Position, 2));
                    ++NumFlareCasts;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.SacredSever1:
                    _severTargetSlots[0] = module.Raid.FindSlot(actor.InstanceID);
                    InitChargesAndSafeSpots(module);
                    break;
                case IconID.SacredSever2:
                    _severTargetSlots[1] = module.Raid.FindSlot(actor.InstanceID);
                    InitChargesAndSafeSpots(module);
                    break;
            }
        }

        private void InitChargesAndSafeSpots(BossModule module)
        {
            if (_severTargetSlots[0] < 0 || _severTargetSlots[1] < 0)
                return; // wait for other event...

            var severSource = module.Enemies(OID.SerZephirin).FirstOrDefault();
            _severStartDir = severSource != null ? Angle.FromDirection(severSource.Position - module.Bounds.Center) : 0.Degrees();
            if (_severStartDir.Rad != 0)
            {
                _groupEast = _config.P2SanctityGroups.BuildGroupMask(1, module.Raid);
                if (_groupEast.None())
                {
                    _groupSwapHints = "unconfigured";
                }
                else
                {
                    if (_config.P2SanctityRelative && _severStartDir.Rad < 0)
                    {
                        // swap groups for relative assignment if needed
                        _groupEast.Raw ^= 0xff;
                    }

                    var effRoles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(module.Raid);
                    if (_config.P2SanctitySwapRole == Role.None)
                    {
                        AssignmentSwapWithRolePartner(module, effRoles, 0, _severStartDir.Rad < 0);
                        AssignmentSwapWithRolePartner(module, effRoles, 1, _severStartDir.Rad > 0);
                    }
                    else
                    {
                        AssignmentReassignIfNeeded(0, _severStartDir.Rad < 0);
                        AssignmentReassignIfNeeded(1, _severStartDir.Rad > 0);
                        if (_groupEast.NumSetBits() != 4)
                        {
                            // to balance, unmarked player of designated role should swap
                            var (swapSlot, swapper) = module.Raid.WithSlot(true).FirstOrDefault(sa => sa.Item1 != _severTargetSlots[0] && sa.Item1 != _severTargetSlots[1] && effRoles[sa.Item1] == _config.P2SanctitySwapRole);
                            if (swapper != null)
                            {
                                _groupEast.Toggle(swapSlot);
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
                var severDirEast = _severStartDir;
                if (severDirEast.Rad < 0)
                    severDirEast += 180.Degrees();
                bool severDiagonalSE = severDirEast.Rad < MathF.PI / 2;
                _chargeEarly = severDiagonalSE == cw1;
            }
        }

        private void AssignmentReassignIfNeeded(int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == _groupEast[slot])
                return; // target is already assigned to correct position, no need to swap
            _groupEast.Toggle(slot);
        }

        private void AssignmentSwapWithRolePartner(BossModule module, Role[] effRoles, int order, bool shouldGoEast)
        {
            int slot = _severTargetSlots[order];
            if (shouldGoEast == _groupEast[slot])
                return; // target is already assigned to correct position, no need to swap
            var role = effRoles[slot];
            var (partnerSlot, partner) = module.Raid.WithSlot(true).Exclude(slot).FirstOrDefault(sa => effRoles[sa.Item1] == role);
            if (partner == null)
                return;

            _groupEast.Toggle(slot);
            _groupEast.Toggle(partnerSlot);

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
            if (!Utils.AlmostEqual(actor.Position.Z, module.Bounds.Center.Z, 1))
                return (null, false);
            if (!Utils.AlmostEqual(MathF.Abs(actor.Position.X - module.Bounds.Center.X), 5, 1))
                return (null, false);

            bool right = actor.Position.X > module.Bounds.Center.X;
            bool facingSouth = Utils.AlmostEqual(actor.Rotation.Rad, 0, 0.1f);
            bool cw = right == facingSouth;
            var res = new ChargeInfo(actor);
            var firstPointDir = actor.Rotation;
            var angleBetweenPoints = (cw ? -1 : 1) * 112.5f.Degrees();

            res.Positions.Add(actor.Position);
            Action<Angle> addPosition = dir => res.Positions.Add(module.Bounds.Center + 21 * dir.ToDirection());
            addPosition(firstPointDir);
            addPosition(firstPointDir + angleBetweenPoints);
            addPosition(firstPointDir + angleBetweenPoints * 2);

            res.Spheres.Add(res.Positions[0]);
            res.Spheres.Add(WPos.Lerp(res.Positions[0], res.Positions[1], 0.5f));
            res.Spheres.Add(res.Positions[1]);
            res.Spheres.Add(WPos.Lerp(res.Positions[1], res.Positions[2], 1.0f / 3));
            res.Spheres.Add(WPos.Lerp(res.Positions[1], res.Positions[2], 2.0f / 3));
            res.Spheres.Add(res.Positions[2]);
            res.Spheres.Add(WPos.Lerp(res.Positions[2], res.Positions[3], 1.0f / 3));
            res.Spheres.Add(WPos.Lerp(res.Positions[2], res.Positions[3], 2.0f / 3));
            res.Spheres.Add(res.Positions[3]);
            return (res, cw);
        }

        private IEnumerable<(WPos, WPos)> ImminentCharges()
        {
            foreach (var c in _charges)
            {
                if (c == null)
                    continue;
                for (int i = 1; i < c.Positions.Count; ++i)
                    yield return (c.Positions[i - 1], c.Positions[i]);
            }
        }

        private IEnumerable<WPos> ImminentSpheres()
        {
            foreach (var c in _charges)
            {
                if (c == null)
                    continue;
                foreach (var s in c.Spheres.Take(6))
                    yield return s;
            }
        }

        private WDir SafeSpotOffset(int slot, Angle dirOffset)
        {
            var dir = _severStartDir + (_chargeCW ? -1 : 1) * dirOffset;
            if (dir.Rad < 0 == _groupEast[slot])
                dir += 180.Degrees();
            return 20 * dir.ToDirection();
        }

        private IEnumerable<WDir> MovementHintOffsets(int slot)
        {
            if (_severStartDir.Rad != 0 && _charges[0] != null && _charges[1] != null)
            {
                // second safe spot could be either 3rd or 5th explosion
                if (_charges[0]!.Spheres.Count > (_chargeEarly ? 6 : 4))
                    yield return SafeSpotOffset(slot, _chargeEarly ? 15.Degrees() : 11.7f.Degrees());
                if (_charges[0]!.Spheres.Count > 0)
                    yield return SafeSpotOffset(slot, 33.3f.Degrees());
            }
        }
    }
}
