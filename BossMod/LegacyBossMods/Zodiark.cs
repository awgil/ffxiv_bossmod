using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    class Zodiark : BossModule
    {
        public enum OID
        {
            Boss = 0x324D,
            Helper = 0x233C,
            Behemoth = 0x3832,
            Python = 0x3833,
            Bird = 0x3834,
            ExoGreen = 0x358F,
            ExoSquare = 0x3590,
            ExoTri = 0x3591,
            RoilingDarkness = 0x3595,
        };

        private enum AID
        {
            FlowCW = 26210, // src=target=Boss
            FlowCCW = 26211, // src=target=Boss
            Para = 26559, // src=target=Boss
            ExoFront = 26560, // src=target=Boss (front tri/sq - only exo2)
            ExoGeneric = 26561, // src=target=Boss (side/back tri/sq)
            ExoTripleStaggered = 26562,
            AstralEclipse = 26563, // src=target=Boss
            ExoTripleRay = 26564, // src=targt=Boss
            ExplosionBehemoth = 26594, // src=target=Helper
            ExplosionExoGreen = 26596, // src=target=ExoGreen
            ExplosionExoSquare = 26597, // src=target=ExoSquare
            ExplosionExoTri = 26598, // src=target=ExoTri
            ExplosionStars = 26599, // src=target=Helper (7 concurrent casts)
            AddsEndFail = 26600, // src=target=Boss
            AddsEndSuccess = 26601, // src=target=Boss
            PhlegetonAOE = 26602, // src=Helper, target=null (3x6 casts)
            Phlegeton = 26603, // src=target=Boss
            DiagonalTL = 26604, // src=target=Boss
            DiagonalTR = 26605, // src=target=Boss
            DiagonalAOE = 26606, // src=target=Helper
            Ania = 26607, // src=target=Boss
            AniaAOE = 27491, // src=Helper, target=MT, starts with Ania, 1 sec longer
            Phobos = 26608, // src=target=Boss
            Adikia = 26609, // src=target=Boss
            AdikiaL = 25513, // src=target=Helper, first hit
            AdikiaR = 26610, // src=target=Helper, second hit
            Styx = 26611, // src=target=Boss
            Enrage = 26613,
            Kokytos = 27744, // src=target=Boss
            PostAddAOELethal = 28026, // src=target=Helper
            PostAddAOENormal = 28027, // src=target=Helper
        };

        private ZodiarkStages _stages = new();

        public Zodiark(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            WorldState.Actors.CastStarted += ActorCastStarted;
            WorldState.Actors.CastFinished += ActorCastFinished;
            WorldState.Actors.Moved += ActorTeleported;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.Actors.CastStarted -= ActorCastStarted;
                WorldState.Actors.CastFinished -= ActorCastFinished;
                WorldState.Actors.Moved -= ActorTeleported;
            }
            base.Dispose(disposing);
        }

        public override void Draw(float cameraAzimuth, int pcSlot, MovementHints? pcMovementHints)
        {
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), ManualActionHint());
            _stages.Draw();
            _stages.DrawDebugButtons();
        }

        protected override void ResetModule()
        {
            _stages.Reset();
        }

        private void ActorCastStarted(object? sender, Actor actor)
        {
            // TODO: when {Diagonal1, Exo3, SideSmash1, } starts casting, dump render pos+flags of animals...
            if ((OID)actor.OID == OID.Boss)
            {
                if (_stages.NextEvent == ZodiarkStages.BossEvent.Diagonal1 || _stages.NextEvent == ZodiarkStages.BossEvent.Exo3 || _stages.NextEvent == ZodiarkStages.BossEvent.SideSmash1 ||
                    _stages.NextEvent == ZodiarkStages.BossEvent.Rotate2 || _stages.NextEvent == ZodiarkStages.BossEvent.Rotate3)
                {
                    DebugObjects.DumpObjectTable();
                    DebugGraphics.DumpScene();
                }
            }

            // TODO: consider updating undetected positions when Exo mobs start their casts...

            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.FlowCW:
                    SetRotationHint(ZodiarkSolver.RotDir.CW);
                    break;
                case AID.FlowCCW:
                    SetRotationHint(ZodiarkSolver.RotDir.CCW);
                    break;
                case AID.DiagonalTL:
                    SetDiagonalHint(ZodiarkSolver.LinePos.TL);
                    break;
                case AID.DiagonalTR:
                    SetDiagonalHint(ZodiarkSolver.LinePos.TR);
                    break;
                case AID.ExoGeneric:
                case AID.ExoTripleStaggered:
                    ApplyExoHints();
                    break;
            }
        }

        private void ActorCastFinished(object? sender, Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
            {
                _stages.NextTrigger();
            }
        }

        private void ActorTeleported(object? sender, (Actor actor, Vector4 prevPos) args)
        {
            // ignore teleports after wipe
            if (_stages.NextEvent == ZodiarkStages.BossEvent.Kokytos || _stages.NextEvent == ZodiarkStages.BossEvent.Intermission1)
                return; // TODO: show aoe hints during intermissions...

            switch ((OID)args.actor.OID)
            {
                case OID.ExoSquare:
                    SetSideHint(ZodiarkSolver.ExoType.Sq, args.actor.Position);
                    break;
                case OID.ExoTri:
                    SetSideHint(ZodiarkSolver.ExoType.Tri, args.actor.Position);
                    break;
            }
        }

        private void SetRotationHint(ZodiarkSolver.RotDir dir)
        {
            Service.Log($"Setting rotation hint: {dir}");
            var ev = _stages.NextEvent;
            if (ev != ZodiarkStages.BossEvent.Rotate1 && ev != ZodiarkStages.BossEvent.Rotate2 && ev != ZodiarkStages.BossEvent.Rotate3 && ev != ZodiarkStages.BossEvent.Rotate4 && ev != ZodiarkStages.BossEvent.Rotate5 && ev != ZodiarkStages.BossEvent.Rotate6)
            {
                Service.Log($"Unexpected stage for rotation hint - current next event is {ev}");
            }

            _stages.Solver.ActiveRot = dir;
        }

        private void SetDiagonalHint(ZodiarkSolver.LinePos pos)
        {
            Service.Log($"Setting diagonal hint: {pos}");
            var ev = _stages.NextEvent;
            if (ev != ZodiarkStages.BossEvent.Diagonal1 && ev != ZodiarkStages.BossEvent.Diagonal2 && ev != ZodiarkStages.BossEvent.Diagonal3 && ev != ZodiarkStages.BossEvent.Diagonal4)
            {
                Service.Log($"Unexpected stage - current next event is {ev}");
            }

            _stages.Solver.ActiveLine = pos;
        }

        private ZodiarkSolver.ExoSide DetermineSideFromPos(Vector3 pos)
        {
            if (pos.X < 80)
                return ZodiarkSolver.ExoSide.Left;
            else if (pos.X > 120)
                return ZodiarkSolver.ExoSide.Right;
            else if (pos.Z < 80)
                return ZodiarkSolver.ExoSide.Top;
            else if (pos.Z > 120)
                return ZodiarkSolver.ExoSide.Bottom;

            Service.Log($"Can't determine exo-side from {Utils.Vec3String(pos)}");
            return (ZodiarkSolver.ExoSide)(-1);
        }

        private void SetSideHint(ZodiarkSolver.ExoType type, Vector3 pos)
        {
            var side = DetermineSideFromPos(pos);
            if (side < 0)
                return;

            Service.Log($"Setting side hint: {side} to {type}");
            _stages.Solver.ActiveExo(side) = type;
        }

        private ZodiarkSolver.ExoType CountExoSqAndTriOnSide(ZodiarkSolver.ExoSide side)
        {
            int numSq = 0, numTri = 0;
            foreach (var elem in WorldState.Actors)
            {
                if (elem.OID != (uint)OID.ExoSquare && elem.OID != (uint)OID.ExoTri)
                    continue; // not interesting
                if (DetermineSideFromPos(elem.Position) != side)
                    continue; // not interesting (wrong side or not teleported yet)
                if (elem.OID == (uint)OID.ExoSquare)
                    ++numSq;
                else
                    ++numTri;
            }

            if (numSq > 0 && numTri > 0)
            {
                Service.Log($"Found {numSq} sq and {numTri} tri on single side.");
                return (ZodiarkSolver.ExoType)(-1);
            }
            else if (numSq > 0)
            {
                return ZodiarkSolver.ExoType.Sq;
            }
            else if (numTri > 0)
            {
                return ZodiarkSolver.ExoType.Tri;
            }
            else
            {
                return ZodiarkSolver.ExoType.None;
            }
        }

        private void DetectPreTeleportedSideExos()
        {
            var l = CountExoSqAndTriOnSide(ZodiarkSolver.ExoSide.Left);
            var r = CountExoSqAndTriOnSide(ZodiarkSolver.ExoSide.Right);
            if (l < 0)
            {
                Service.Log($"Left side has both squares and triangles");
            }
            else if (r < 0)
            {
                Service.Log($"Right side has both squares and triangles");
            }
            else if (l == ZodiarkSolver.ExoType.None && r == ZodiarkSolver.ExoType.None)
            {
                Service.Log($"Sides have no exos");
            }
            else if (l != ZodiarkSolver.ExoType.None && r != ZodiarkSolver.ExoType.None)
            {
                Service.Log($"Exos from both sides: L={l}, R={r}");
            }
            else
            {
                _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) = l;
                _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) = r;
            }
        }

        private void DetectPreTeleportedExo(ZodiarkSolver.ExoSide side)
        {
            var t = CountExoSqAndTriOnSide(side);
            if (t <= ZodiarkSolver.ExoType.None)
            {
                Service.Log($"Failed to determine exo for {side}");
            }
            else
            {
                _stages.Solver.ActiveExo(side) = t;
            }
        }

        private void ApplyExoHints()
        {
            Service.Log($"Applying generic exo hints...");
            switch (_stages.NextEvent)
            {
                case ZodiarkStages.BossEvent.Exo1:
                    // we should have exactly 1 side teleport before, do nothing
                    break;
                case ZodiarkStages.BossEvent.Exo3:
                case ZodiarkStages.BossEvent.Exo4:
                case ZodiarkStages.BossEvent.Exo5:
                case ZodiarkStages.BossEvent.Exo6:
                case ZodiarkStages.BossEvent.Exo8:
                    // exo3: we should have 1 or 0 hint on the sides (latter if boss reused tri from exo1)
                    // exo4: we should have 1 or 0 hint on the sides (latter if boss reused sq from exo3)
                    // exo5: we should have 1 or 0 hint on the sides (some exo could be reused, we might even be confused by multiple stale exos)
                    // exo6: we should have 1 or 0 hint on the sides (some exo could be reused, we might even be confused by multiple stale exos)
                    // exo8: not sure whether it could be 'back' exo, never seen it...
                    if (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) == ZodiarkSolver.ExoType.None && _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) == ZodiarkSolver.ExoType.None)
                        DetectPreTeleportedSideExos();
                    break;
                case ZodiarkStages.BossEvent.TriExo1:
                case ZodiarkStages.BossEvent.TriExo2:
                    // triexo1: we should have exactly 1 teleport to the back (first exo there ever) and 0 to 2 teleports to the sides
                    // triexo2: all exos could be reused
                    if (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) == ZodiarkSolver.ExoType.None)
                        DetectPreTeleportedExo(ZodiarkSolver.ExoSide.Left);
                    if (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) == ZodiarkSolver.ExoType.None)
                        DetectPreTeleportedExo(ZodiarkSolver.ExoSide.Right);
                    if (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Bottom) == ZodiarkSolver.ExoType.None)
                        DetectPreTeleportedExo(ZodiarkSolver.ExoSide.Bottom);
                    break;
                case ZodiarkStages.BossEvent.Exo7:
                    // always back square, could be reused - but we have a different hack to apply it anyway
                    break;
                default:
                    Service.Log($"Unexpected stage - current next event is {_stages.NextEvent}");
                    break;
            }
        }

        private string ManualActionHint()
        {
            switch (_stages.NextEvent)
            {
                case ZodiarkStages.BossEvent.Diagonal1:
                    return "Stand in safe bird";
                case ZodiarkStages.BossEvent.Exo3:
                case ZodiarkStages.BossEvent.Rotate1: // should never fail to detect exo here...
                case ZodiarkStages.BossEvent.Exo5:
                case ZodiarkStages.BossEvent.Exo8:
                    return _stages.Solver.ActiveSnakes == ZodiarkSolver.SnakePos.None ? "Select snakes!" : "";
                case ZodiarkStages.BossEvent.Rotate2:
                    return _stages.Solver.ActiveLine == ZodiarkSolver.LinePos.None ? "Select fire line! And stand in rotated bird" : "Stand in rotated half bird";
                case ZodiarkStages.BossEvent.Diagonal2:
                    return (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) == ZodiarkSolver.ExoType.None && _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) == ZodiarkSolver.ExoType.None) ? "Manual exo needed" : "";
                case ZodiarkStages.BossEvent.Rotate3:
                case ZodiarkStages.BossEvent.Rotate4:
                    return _stages.Solver.ActiveLine == ZodiarkSolver.LinePos.None
                        ? (_stages.Solver.ActiveSnakes == ZodiarkSolver.SnakePos.None ? "Select snakes and fire line!" : "Select fire line!")
                        : (_stages.Solver.ActiveSnakes == ZodiarkSolver.SnakePos.None ? "Select snakes!" : "");
                case ZodiarkStages.BossEvent.TriExo1:
                    return (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) == ZodiarkSolver.ExoType.None
                        || _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) == ZodiarkSolver.ExoType.None
                        || _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Bottom) == ZodiarkSolver.ExoType.None) ? "Manual exo needed" : "";
                case ZodiarkStages.BossEvent.TripleRay1:
                    return (_stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Left) == ZodiarkSolver.ExoType.None && _stages.Solver.ActiveExo(ZodiarkSolver.ExoSide.Right) == ZodiarkSolver.ExoType.None) ? "Manual exo needed. Prepare for triple rays" : "Prepare for triple rays";
                case ZodiarkStages.BossEvent.Rotate5:
                    return "Stand in half behemoth";
                default:
                    return "";
            }
        }
    }
}
