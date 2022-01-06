using ImGuiNET;
using System;
using System.Text;

namespace BossMod
{
    public class ZodiarkStages
    {
        // timings: cast signifying end-of-phase --> [time from phase start to cast start]+[cast real duration]
        public enum BossEvent
        {
            Kokytos, // from pull: [6+4]
            Para1, // [7+3]; 4 birds
            Styx1, // [11+5]
            Ania1, // [9+4] (actual ania hit happens 1 sec later)
            Exo1, Exo2, // [5+5], [2+7]; exo side tri + exo front any
            Para2, Diagonal1, // [9+3], [5+7]; birds/behemoths + diagonal
            Phobos1, // [8+4]
            Para3, Exo3, Rotate1, // [7+3], [2+5], [2+10]; snakes + exo side + rotate
            Ania2, // [9+4]
            Para4, SideSmash1, // [4+3], [4+6]; snakes sides + smash

            Intermission1, // [11+X] adds with three aoe patterns appear; wipe progress 97/100 @ 38sec - so have ~40 secs?..
            Intermission2, // [28+5]; boss reappears after adds are dead and starts casting stars; becomes targetable at ~22sec and becomes untargetable at cast end
            TripleRay1, // [20+7]; just after first stars; becomes targetable at ~12sec

            Para5, Rotate2, // [13+3], [5+10]; birds/behemoths + line + rotate
            Ania3, // [15+4]
            Exo4, Diagonal2, // [7+5], [2+7]; exo side square
            Para6, Rotate3, // [11+3], [5+10]; 4 birds + snakes + line + rotate
            Styx2, // [6+5]
            TriExo1, // [8+13]; side + side + back
            SideSmash2, // [6+6]

            Intermission3, // [10+5], becomes untargetable at cast end
            Diagonal3, // [23+5]; becomes targetable at ~12sec

            Ania4, // [8+4]
            Para7, Exo5, Rotate4, // [7+3], [2+5], [2+10]; snakes + exo side + line + rotate
            Phlegethon, // [6+3]
            Styx3, // [2+5]
            Exo6, TripleRay2, // [11+5], [2+7]; exo side
            Para8, Exo7, Rotate5, // [8+3], [2+5], [2+10]; birds/behemoths + exo back sq + line + rotate
            Phobos2, // [11+4]
            TriExo2, // [10+13]; back + side + side
            Diagonal4, // [5+7]
            Styx4, // [4+5]
            Para9, Exo8, Rotate6, // [10+3], [2+5], [2+10]; 4 birds + snakes + exo side (and back too?) any + line + rotate
            Styx5, // [6+5]

            Enrage,
            Wipe
        }

        public ZodiarkSolver Solver = new ZodiarkSolver();
        public BossEvent NextEvent { get; private set; } = BossEvent.Kokytos;
        private DateTime _timeSinceTrigger = DateTime.Now;

        public void Draw()
        {
            (var nextHint, var nextLen) = BuildNextEventsHint(NextEvent, 1);
            ImGui.Text($"Next: {nextHint}");
            ImGui.Text($"Then: {BuildNextEventsHint(NextEvent + nextLen, 5).Item1}");
            DrawHint();
        }

        public void DrawDebugButtons()
        {
            ImGui.Text($"Time in phase: {DateTime.Now - _timeSinceTrigger}");
            if (ImGui.Button("Reset"))
                Reset();
            ImGui.SameLine();
            if (ImGui.Button("Clear"))
                Solver.Clear();
            ImGui.SameLine();
            if (ImGui.Button("Prev trigger"))
                PrevTrigger();
            ImGui.SameLine();
            if (ImGui.Button("Next trigger"))
                NextTrigger();
            ImGui.SameLine();
            if (ImGui.Button("Next AOE"))
                NextBigAOE();
        }

        public void Reset()
        {
            Solver.Clear();
            NextEvent = BossEvent.Kokytos;
            _timeSinceTrigger = DateTime.Now;
        }

        public void PrevTrigger()
        {
            --NextEvent;
            _timeSinceTrigger = DateTime.Now;
        }

        public void NextTrigger()
        {
            if (IsAOEImminent(NextEvent))
                Solver.Clear();
            if (NextEvent == BossEvent.TripleRay2)
                Solver.ActiveExo(ZodiarkSolver.ExoSide.Bottom) = ZodiarkSolver.ExoType.Sq;
            ++NextEvent;
            _timeSinceTrigger = DateTime.Now;
        }

        public void NextBigAOE()
        {
            while (!IsAOEImminent(NextEvent))
                NextTrigger();
            NextTrigger();
        }

        private void DrawHint()
        {
            switch (NextEvent)
            {
                case BossEvent.Kokytos:
                case BossEvent.Para1:
                case BossEvent.Styx1:
                    Solver.Draw(ZodiarkSolver.Control.Birds);
                    break;

                case BossEvent.Ania1:
                case BossEvent.Exo1:
                case BossEvent.Exo2:
                    // note: sides can only be tri?..
                    Solver.Draw(ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.ExoFront);
                    break;

                case BossEvent.Para2: // TODO: after exiting this state, determine behemoth positions
                case BossEvent.Diagonal1:
                    Solver.Draw(ZodiarkSolver.Control.Birds | ZodiarkSolver.Control.Behemoths | ZodiarkSolver.Control.Diagonal);
                    break;

                case BossEvent.Phobos1:
                case BossEvent.Para3: // TODO: after exiting this state, determine snake positions
                case BossEvent.Exo3:
                case BossEvent.Rotate1:
                    Solver.Draw(ZodiarkSolver.Control.SnakesH | ZodiarkSolver.Control.SnakesV | ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Ania2:
                case BossEvent.Para4: // TODO: after exiting this state, determine snake positions
                case BossEvent.SideSmash1:
                    Solver.Draw(ZodiarkSolver.Control.SnakesH | ZodiarkSolver.Control.SideSmash);
                    break;

                case BossEvent.Intermission1: // TODO: show exo markers
                case BossEvent.Intermission2: // TODO: stars helper
                case BossEvent.TripleRay1: // TODO: show exo markers
                    // TODO: show triple rays
                    Solver.Draw(ZodiarkSolver.Control.None);
                    break; // don't have footage for events below...

                case BossEvent.Para5: // TODO: after exiting this state, determine behemoth positions
                case BossEvent.Rotate2: // TODO: determine lines!
                    Solver.Draw(ZodiarkSolver.Control.Birds | ZodiarkSolver.Control.Behemoths | ZodiarkSolver.Control.Lines | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Ania3:
                case BossEvent.Exo4:
                case BossEvent.Diagonal2:
                    // TODO: sides can only be sq?..
                    Solver.Draw(ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.Diagonal);
                    break;

                case BossEvent.Para6: // TODO: after exiting this state, determine snake positions
                case BossEvent.Rotate3: // TODO: determine lines!
                    Solver.Draw(ZodiarkSolver.Control.Birds | ZodiarkSolver.Control.SnakesH | ZodiarkSolver.Control.SnakesV | ZodiarkSolver.Control.Lines | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Styx2:
                case BossEvent.TriExo1:
                    Solver.Draw(ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.ExoBack);
                    break;

                case BossEvent.SideSmash2:
                    Solver.Draw(ZodiarkSolver.Control.SideSmash);
                    break;

                case BossEvent.Intermission3: // TODO: stars helper
                case BossEvent.Diagonal3:
                    Solver.Draw(ZodiarkSolver.Control.Diagonal);
                    break;

                case BossEvent.Ania4:
                case BossEvent.Para7: // TODO: after exiting this state, determine snake positions
                case BossEvent.Exo5:
                case BossEvent.Rotate4: // TODO: determine lines!
                    // TODO: confirm SnakesH (seen V)
                    Solver.Draw(ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.SnakesH | ZodiarkSolver.Control.SnakesV | ZodiarkSolver.Control.Lines | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Phlegethon:
                case BossEvent.Styx3:
                case BossEvent.Exo6:
                case BossEvent.TripleRay2: // TODO: show triple rays?..
                    Solver.Draw(ZodiarkSolver.Control.ExoSides);
                    break;

                case BossEvent.Para8: // TODO: after exiting this state, determine behemoth positions
                case BossEvent.Exo7: // TODO: here exo marker is always bottom-square, currently we handle that via hack
                case BossEvent.Rotate5: // TODO: determine lines!
                    Solver.Draw(ZodiarkSolver.Control.Birds | ZodiarkSolver.Control.Behemoths | ZodiarkSolver.Control.Lines | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Phobos2:
                case BossEvent.TriExo2:
                case BossEvent.Diagonal4:
                    Solver.Draw(ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.ExoBack | ZodiarkSolver.Control.Diagonal);
                    break;

                case BossEvent.Styx4:
                case BossEvent.Para9: // TODO: after exiting this state, determine snake positions
                case BossEvent.Exo8:
                case BossEvent.Rotate6: // TODO: determine lines!
                    Solver.Draw(ZodiarkSolver.Control.Birds | ZodiarkSolver.Control.SnakesH | ZodiarkSolver.Control.SnakesV | ZodiarkSolver.Control.ExoSides | ZodiarkSolver.Control.ExoBack | ZodiarkSolver.Control.Lines | ZodiarkSolver.Control.Rot);
                    break;

                case BossEvent.Styx5:
                case BossEvent.Enrage:
                    Solver.Draw(ZodiarkSolver.Control.None);
                    break;
            }
        }

        private static bool IsAOEImminent(BossEvent e)
        {
            return e == BossEvent.Styx1 || e == BossEvent.Exo2 || e == BossEvent.Diagonal1 || e == BossEvent.Rotate1 || e == BossEvent.SideSmash1 || e == BossEvent.Intermission1 || e == BossEvent.TripleRay1 || e == BossEvent.Rotate2 || e == BossEvent.Diagonal2
                || e == BossEvent.Rotate3 || e == BossEvent.TriExo1 || e == BossEvent.SideSmash2 || e == BossEvent.Diagonal3 || e == BossEvent.Rotate4 || e == BossEvent.TripleRay2 || e == BossEvent.Rotate5 || e == BossEvent.Diagonal4 || e == BossEvent.Rotate6;
        }

        // return name + whether it is resolved together with next event
        private static (string, bool) BossEventHint(BossEvent e)
        {
            switch (e)
            {
                case BossEvent.Para1: return ("Para1 (4 birds)", false);
                case BossEvent.Exo1: return ("Exo1 (side tri)", true);
                case BossEvent.Exo2: return ("Exo2 (front)", false);
                case BossEvent.Para2: return ("Para2 (birds/behemoths)", true);
                case BossEvent.Para3: return ("Para3 (snakes)", true);
                case BossEvent.Exo3: return ("Exo3 (side)", true);
                case BossEvent.Para4: return ("Para4 (snakes side)", true);
                case BossEvent.Para5: return ("Para5 (birds/behemoths)", true);
                case BossEvent.Exo4: return ("Exo4 (side sq)", true);
                case BossEvent.Para6: return ("Para6 (4 birds + snakes)", true);
                case BossEvent.Rotate3: return (e.ToString(), true);
                case BossEvent.Para7: return ("Para7 (snakes)", true);
                case BossEvent.Exo5: return ("Exo5 (side)", true);
                case BossEvent.Rotate4: return (e.ToString(), true);
                case BossEvent.Exo6: return ("Exo6 (side)", true);
                case BossEvent.Para8: return ("Para8 (birds/behemoths)", true);
                case BossEvent.Exo7: return ("Exo7 (back sq)", true);
                case BossEvent.Para9: return ("Para9 (4 birds + snakes)", true);
                case BossEvent.Exo8: return ("Exo8 (side/back?)", true);
                default: return (e.ToString(), false);
            }
        }

        private static (string, int) BuildNextEventsHint(BossEvent startEvent, int maxLen)
        {
            var res = new StringBuilder();
            bool prevCombo = false;
            int length = 0;
            while (startEvent <= BossEvent.Enrage && (prevCombo || length < maxLen))
            {
                if (length > 0)
                    res.Append(prevCombo ? " + " : " ---> ");

                string n;
                (n, prevCombo) = BossEventHint(startEvent++);
                res.Append(n);
                length++;
            }
            return (res.ToString(), length);
        }
    }
}
