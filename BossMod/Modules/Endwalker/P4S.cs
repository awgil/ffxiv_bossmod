using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class P4S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x35FD,
            Pinax = 0x35FE, // '', 4x exist at start at [90/110, 90/110]
            Unk2 = 0x3600, // ?? 'hesperos', 1 exists at start
            Akantha = 0x3601, // ?? 'akantha', 12 exist at start
            Helper = 0x233C, // 38 exist at start
        };

        public enum AID : uint
        {
            ElegantEviscerationSecond = 26649, // Boss->target, no cast, second hit
            SettingTheScene = 27083, // Boss->Boss
            ShiftStart = 27086, // Boss->Boss, no cast, tp to center, sword glowing (?)
            Pinax = 27087, // Boss->Boss
            MekhaneAcid = 27088, // Helper->target, no cast
            MekhaneLava = 27089, // Helper->target, no cast
            MekhaneWell = 27090, // Helper->Helper, no cast, affects arena
            MekhaneLevinstrike = 27091, // Helper->Helper, no cast, affects arena
            PinaxAcid = 27092, // Helper->Helper, affects corner
            PinaxLava = 27093, // Helper->Helper, affects corner
            PinaxWell = 27094, // Helper->Helper, affects corner
            PinaxLevinstrike = 27095, // Helper->Helper, affects corner
            Bloodrake = 27096, // Boss->Boss
            DirectorsBelone = 27110, // Boss->Boss
            DirectorsBeloneDebuffs = 27111, // Helper->target, no cast, just applies Role Call debuffs
            CursedCasting = 27113, // Helper->target, no cast, does something bad if no role call?..
            AethericChlamys = 27116, // Boss->Boss
            InversiveChlamys = 27117, // Boss->Boss
            InversiveChlamysAOE = 27119, // Helper->target, no cast, damage to tethered targets
            EasterlyShift = 27140, // Boss->Boss
            ShiftingStrike = 27142, // Helper->Helper, sword attack
            ElegantEvisceration = 27144, // Boss->target
            Decollation = 27145, // Boss->Boss
        };

        public enum SID : uint
        {
            RoleCall = 2802,
            Miscast = 2803,
        }

        public enum TetherID : uint
        {
            Chlamys = 89,
            Bloodrake = 165,
        }

        // state related to debuffs & tethers mechanic
        private class DebuffsAndTethers : Component
        {
            public enum State { Inactive, TethersAssigned, DebuffsAssigned, Chlamys, Resolve }

            public State CurState = State.Inactive;

            private P4S _module;
            private ulong _tetherForbidden = 0;
            private ulong _tetherTargets = 0;
            private ulong _tetherInAOE = 0;
            private ulong _debuffForbidden = 0;
            private ulong _debuffTargets = 0;
            private ulong _debuffImmune = 0;

            private static float _debuffPassRange = 3; // not sure about this...
            private static float _tetherStackRange = 3; // this is arbitrary
            private static float _aoeRange = 5;

            public DebuffsAndTethers(P4S module)
            {
                _module = module;
            }

            public void AssignTethersFromBloodrake()
            {
                _tetherForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
                CurState = State.TethersAssigned;
            }

            public void AssignDebuffsFromBloodrake()
            {
                _debuffForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
                CurState = State.DebuffsAssigned;
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                _tetherForbidden = _debuffForbidden = _debuffTargets = _debuffImmune = 0;
            }

            public override void Update()
            {
                _tetherTargets = _tetherInAOE = 0;
                switch (CurState)
                {
                    case State.Chlamys:
                    case State.Resolve:
                        foreach ((int i, var player) in _module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Chlamys))
                        {
                            BitVector.SetVector64Bit(ref _tetherTargets, i);
                            _tetherInAOE |= _module.FindRaidMembersInRange(i, _aoeRange);
                        }
                        if (_tetherTargets == 0 && CurState == State.Resolve)
                            Reset();
                        break;
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState == State.Inactive)
                    return;

                if (_debuffForbidden != 0)
                {
                    if (!BitVector.IsVector64BitSet(_debuffForbidden, slot))
                    {
                        // we should be grabbing debuff
                        if (_debuffTargets == 0)
                        {
                            // debuffs not assigned yet => spread and prepare to grab
                            bool stacked = _module.IterateRaidMembersInRange(slot, _debuffPassRange).Any();
                            hints.Add("Debuffs: spread and prepare to handle!", stacked);
                        }
                        else if (BitVector.IsVector64BitSet(_debuffImmune, slot))
                        {
                            hints.Add("Debuffs: failed to handle");
                        }
                        else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                        {
                            hints.Add("Debuffs: OK", false);
                        }
                        else
                        {
                            hints.Add("Debuffs: grab!");
                        }
                    }
                    else
                    {
                        // we should be passing debuff
                        if (_debuffTargets == 0)
                        {
                            bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && BitVector.IsVector64BitSet(_debuffForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _debuffPassRange)).Any();
                            hints.Add("Debuffs: stack and prepare to pass!", badStack);
                        }
                        else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                        {
                            hints.Add("Debuffs: pass!");
                        }
                        else
                        {
                            hints.Add("Debuffs: avoid", false);
                        }
                    }
                }

                if (_tetherForbidden != 0)
                {
                    if (!BitVector.IsVector64BitSet(_tetherForbidden, slot))
                    {
                        // we should be grabbing tethers
                        if (_tetherTargets == 0)
                        {
                            // no tethers yet => stack and prepare to intercept
                            if (_debuffTargets == 0 || (_debuffTargets & _debuffForbidden) != 0)
                            {
                                // we're not done with debuffs yet, ignore tethers for now...
                                hints.Add("Tethers: prepare to intercept", false);
                            }
                            else
                            {
                                bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && !BitVector.IsVector64BitSet(_tetherForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _tetherStackRange)).Any();
                                hints.Add("Tethers: stack and prepare to intercept!", badStack);
                            }
                        }
                        else if (!BitVector.IsVector64BitSet(_tetherTargets, slot))
                        {
                            hints.Add("Tethers: intercept!");
                        }
                        else if (_module.IterateRaidMembersInRange(slot, _aoeRange).Any())
                        {
                            hints.Add("Tethers: GTFO from others!");
                        }
                        else
                        {
                            hints.Add("Tethers: OK", false);
                        }
                    }
                    else
                    {
                        // we should be passing tethers
                        if (_tetherTargets == 0)
                        {
                            // no tethers yet => stack and prepare to pass
                            if (_debuffTargets == 0 || (_debuffTargets & _debuffForbidden) != 0)
                            {
                                // we're not done with debuffs yet, ignore tethers for now...
                                hints.Add("Tethers: prepare to pass", false);
                            }
                            else
                            {
                                bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && BitVector.IsVector64BitSet(_tetherForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _tetherStackRange)).Any();
                                hints.Add("Tethers: stack and prepare to pass!", badStack);
                            }
                        }
                        else if (BitVector.IsVector64BitSet(_tetherTargets, slot))
                        {
                            hints.Add("Tethers: pass!");
                        }
                        else if (BitVector.IsVector64BitSet(_tetherInAOE, slot))
                        {
                            hints.Add("Tethers: GTFO from aoe!");
                        }
                        else
                        {
                            hints.Add("Tethers: avoid", false);
                        }
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                var boss = _module.Boss();
                ulong failingPlayers = (_debuffForbidden & _debuffTargets) | (_tetherForbidden & _tetherTargets);
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                    bool inAOE = BitVector.IsVector64BitSet(_tetherInAOE, i);
                    arena.Actor(player, failing ? arena.ColorDanger : (inAOE ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));

                    if (boss != null && player.Tether.ID == (uint)TetherID.Chlamys)
                    {
                        arena.AddLine(player.Position, boss.Position, failing ? arena.ColorDanger : arena.ColorSafe);
                        arena.AddCircle(player.Position, _aoeRange, arena.ColorDanger);
                    }
                }
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.RoleCall:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffTargets, true);
                        break;
                    case SID.Miscast:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffImmune, true);
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.RoleCall:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffTargets, false);
                        break;
                    case SID.Miscast:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffImmune, false);
                        break;
                }
            }

            private void ModifyDebuff(int slot, ref ulong vector, bool active)
            {
                if (slot >= 0)
                    BitVector.ModifyVector64Bit(ref vector, slot, active);
            }
        }

        // state related to pinax mechanics
        private class Pinax : Component
        {
            public int NumFinished { get; private set; } = 0;

            private P4S _module;
            private WorldState.Actor? _acid;
            private WorldState.Actor? _fire;
            private WorldState.Actor? _water;
            private WorldState.Actor? _lighting;
            private ulong _acidInAOE = 0;

            private static float _acidAOERadius = 5;
            private static float _fireAOERadius = 6;
            private static float _knockbackRadius = 13;
            private static float _lightingSafeDistance = 15; // not sure about this, what is real safe distance?

            public Pinax(P4S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                NumFinished = 0;
                _acid = _fire = _water = _lighting = null;
            }

            public override void Update()
            {
                _acidInAOE = 0;
                if (_acid != null)
                {
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                        _acidInAOE |= _module.FindRaidMembersInRange(i, _acidAOERadius);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_acid != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _acid.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from acid square!");
                    }
                    if (BitVector.IsVector64BitSet(_acidInAOE, slot))
                    {
                        hints.Add("Spread!");
                    }
                }
                if (_fire != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _fire.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from fire square!");
                    }
                    // TODO: verify targeting (currently assuming 2 healers)
                    if (_module.IterateRaidMembersWhere(other => other.Role == WorldState.ActorRole.Healer && GeometryUtils.PointInCircle(actor.Position - other.Position, _fireAOERadius)).Count() != 1)
                    {
                        hints.Add("Stack in fours!");
                    }
                }
                if (_water != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _water.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from water square!");
                    }
                    if (!_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _water, _knockbackRadius)))
                    {
                        hints.Add("About to be knocked into wall!");
                    }
                }
                if (_lighting != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _lighting.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from lighting square!");
                    }
                    if (GeometryUtils.PointInRect(actor.Position - _module.Arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance))
                    {
                        hints.Add("GTFO from center!");
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_acid != null)
                {
                    arena.ZoneQuad(_acid.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_fire != null)
                {
                    arena.ZoneQuad(_fire.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_water != null)
                {
                    arena.ZoneQuad(_water.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_lighting != null)
                {
                    arena.ZoneQuad(_lighting.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                    arena.ZoneQuad(arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc == null)
                    return;

                if (_acid != null)
                {
                    arena.AddCircle(pc.Position, _acidAOERadius, arena.ColorDanger);
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                        arena.Actor(player, BitVector.IsVector64BitSet(_acidInAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
                if (_fire != null)
                {
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                    {
                        if (player.Role == WorldState.ActorRole.Healer)
                        {
                            arena.Actor(player, arena.ColorDanger);
                            arena.AddCircle(pc.Position, _fireAOERadius, arena.ColorDanger);
                        }
                        else
                        {
                            arena.Actor(player, arena.ColorPlayerGeneric);
                        }
                    }
                }
                if (_water != null)
                {
                    var adjPos = AdjustPositionForKnockback(pc.Position, _module.Boss(), _knockbackRadius);
                    if (adjPos != pc.Position)
                    {
                        arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                        arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                    }
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.PinaxAcid:
                        _acid = actor;
                        break;
                    case AID.PinaxLava:
                        _fire = actor;
                        break;
                    case AID.PinaxWell:
                        _water = actor;
                        break;
                    case AID.PinaxLevinstrike:
                        _lighting = actor;
                        break;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.PinaxAcid:
                        _acid = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxLava:
                        _fire = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxWell:
                        _water = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxLevinstrike:
                        _lighting = null;
                        ++NumFinished;
                        break;
                }
            }
        }

        // state related to shift mechanics
        private class Shift : Component
        {
            private P4S _module;
            private WorldState.Actor? _caster;
            private bool _isSword;

            private static float _coneHalfAngle = MathF.PI / 3; // not sure about this...

            public Shift(P4S module)
            {
                _module = module;
            }

            public override void Reset() => _caster = null;

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_caster == null)
                    return;

                if (_isSword)
                {
                    if (GeometryUtils.PointInCone(actor.Position - _caster.Position, _caster.Rotation, _coneHalfAngle))
                    {
                        hints.Add("GTFO from sword!");
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_caster == null)
                    return;

                if (_isSword)
                {
                    arena.ZoneCone(_caster.Position, 0, 50, _caster.Rotation - _coneHalfAngle, _caster.Rotation + _coneHalfAngle, arena.ColorAOE);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.ShiftingStrike:
                        _caster = actor;
                        _isSword = true;
                        break;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.ShiftingStrike:
                        _caster = null;
                        break;
                }
            }
        }

        private List<WorldState.Actor> _boss;
        private WorldState.Actor? Boss() => _boss.FirstOrDefault();

        public P4S(WorldState ws)
            : base(ws, 8)
        {
            _boss = RegisterEnemies(OID.Boss, true);

            RegisterComponent(new DebuffsAndTethers(this));
            RegisterComponent(new Pinax(this));
            RegisterComponent(new Shift(this));

            StateMachine.State? s;
            s = BuildDecollationState(ref InitialState, 9.3f);
            s = BuildBloodrakeBeloneStates(ref s.Next, 4.2f);
            s = BuildDecollationState(ref s.Next, 4.2f);
            s = BuildElegantEviscerationState(ref s.Next, 4.2f);
            s = BuildPinaxStates(ref s.Next, 11.4f);
            s = BuildElegantEviscerationState(ref s.Next, 3.7f);

            s = CommonStates.Cast(ref s.Next, Boss, AID.Bloodrake, 4.3f, 4, "Bloodrake 3");
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State BuildDecollationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildElegantEviscerationState(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref cast.Next, 3, "Tankbuster");
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
            return resolve;
        }

        private StateMachine.State BuildBloodrakeBeloneStates(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<DebuffsAndTethers>()!;

            var bloodrake1 = CommonStates.Cast(ref link, Boss, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter = comp.AssignTethersFromBloodrake;
            bloodrake1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var chlamys1 = CommonStates.Cast(ref bloodrake1.Next, Boss, AID.AethericChlamys, 3.2f, 4);
            var bloodrake2 = CommonStates.Cast(ref chlamys1.Next, Boss, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter = comp.AssignDebuffsFromBloodrake;
            bloodrake2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var belone = CommonStates.Cast(ref bloodrake2.Next, Boss, AID.DirectorsBelone, 4.2f, 5);
            var chlamys2 = CommonStates.Cast(ref belone.Next, Boss, AID.InversiveChlamys, 9.2f, 7);
            chlamys2.Enter = () => comp.CurState = DebuffsAndTethers.State.Chlamys;
            chlamys2.Exit = () => comp.CurState = DebuffsAndTethers.State.Resolve;

            var resolve = CommonStates.Condition(ref chlamys2.Next, 0.8f, () => comp.CurState == DebuffsAndTethers.State.Inactive, "Chlamys");
            return resolve;
        }

        private StateMachine.State BuildPinaxStates(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Pinax>()!;

            var setting = CommonStates.Cast(ref link, Boss, AID.SettingTheScene, delay, 4, "Scene");
            setting.EndHint |= StateMachine.StateHint.GroupWithNext;
            // ~1s after cast end, we get a bunch of env controls 8003759C, state=00020001
            // what I've seen so far:
            // 1. WF arrangement: indices 1, 2, 3, 4, 5, 10, 15, 20
            //    AL
            // index 5 corresponds to NE fire, index 10 corresponds to SE lighting, index 15 corresponds to SW acid, index 20 corresponds to NW water

            var pinax = CommonStates.Cast(ref setting.Next, Boss, AID.Pinax, 8.2f, 5, "Pinax");
            pinax.EndHint |= StateMachine.StateHint.GroupWithNext;
            // timeline:
            //  0.0s pinax cast end
            //  1.0s square 1 activation: env control (.10 = 00800040), helper starts casting 27095
            //  4.0s square 2 activation: env control (.15 = 00800040), helper starts casting 27092
            //  7.0s square 1 env control (.10 = 02000001)
            // 10.0s square 2 env control (.15 = 02000001)
            //       square 1 cast finish (+ instant 27091)
            // 13.0s square 2 cast finish (+ instant 27088)
            // 14.0s square 3 activation: env control (.20 = 00800040), helper starts casting 27094
            // 20.0s square 3 env control (.20 = 02000001)
            // 23.0s square 3 cast finish (+ instant 27090)
            // 25.0s square 4 activation: env control (.05 = 00800040), helper starts casting 27093
            // 31.0s square 4 env control (.05 = 02000001)
            // 34.0s square 4 cast finish (+ instant 27089)

            var p1 = CommonStates.Condition(ref pinax.Next, 10, () => comp.NumFinished == 1, "Corner1");
            p1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p2 = CommonStates.Condition(ref p1.Next, 3, () => comp.NumFinished == 2, "Corner2");
            p2.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.EasterlyShift] = new(null, () => { });
            var shiftStart = CommonStates.CastStart(ref p2.Next, Boss, dispatch, 3.6f);
            // together with this, one of the helpers starts casting 27142 or ???

            var p3 = CommonStates.Condition(ref shiftStart.Next, 6.4f, () => comp.NumFinished == 3, "Corner3");
            p3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var shiftEnd = CommonStates.CastEnd(ref p3.Next, Boss, 1.6f, "Shift");
            shiftEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p4 = CommonStates.Condition(ref shiftEnd.Next, 9.4f, () => comp.NumFinished == 4, "Pinax resolve");
            p4.Exit = comp.Reset;
            return p4;
        }
    }
}
