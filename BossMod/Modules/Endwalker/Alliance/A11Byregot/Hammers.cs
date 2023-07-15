using System;

namespace BossMod.Endwalker.Alliance.A11Byregot
{
    class Hammers : BossComponent
    {
        private enum State { Inactive, SidesAboutToBeDestroyed, Active }

        private State _curState;
        private int[] _lineOffset = new int[5];
        private int[] _lineMovement = new int[5];
        private Actor? _levinforge;
        private Actor? _spire;

        private static AOEShapeRect _aoeLevinforge = new(50, 5);
        private static AOEShapeRect _aoeSpire = new(50, 15);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_curState == State.Inactive)
                return;

            var off = (actor.Position - module.Bounds.Center) / 10;
            int x = Math.Clamp((int)MathF.Round(off.X), -2, 2);
            int z = Math.Clamp((int)MathF.Round(off.Z), -2, 2);
            if (CellDangerous(x, z, true))
                hints.Add("GTFO from dangerous cell!");

            if (_aoeLevinforge.Check(actor.Position, _levinforge) || _aoeSpire.Check(actor.Position, _spire))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_curState == State.Inactive)
                return;

            float cellHalfSize = 5;
            for (int z = -2; z <= 2; ++z)
            {
                for (int x = -2; x <= 2; ++x)
                {
                    var cellCenter = module.Bounds.Center + new WDir(x, z) * 10;
                    if (CellDangerous(x, z, true))
                        arena.ZoneRect(cellCenter, new WDir(1, 0), cellHalfSize, cellHalfSize, cellHalfSize, ArenaColor.AOE);
                    else if (CellDangerous(x, z, false))
                        arena.ZoneRect(cellCenter, new WDir(1, 0), cellHalfSize, cellHalfSize, cellHalfSize, ArenaColor.SafeFromAOE);
                }
            }

            _aoeLevinforge.Draw(arena, _levinforge);
            _aoeSpire.Draw(arena, _spire);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_curState == State.Inactive)
                return;

            arena.AddLine(module.Bounds.Center + new WDir(-15, -25), module.Bounds.Center + new WDir(-15, +25), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir( -5, -25), module.Bounds.Center + new WDir( -5, +25), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir( +5, -25), module.Bounds.Center + new WDir( +5, +25), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(+15, -25), module.Bounds.Center + new WDir(+15, +25), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(-25, -15), module.Bounds.Center + new WDir(+25, -15), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(-25,  -5), module.Bounds.Center + new WDir(+25,  -5), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(-25,  +5), module.Bounds.Center + new WDir(+25,  +5), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(-25, +15), module.Bounds.Center + new WDir(+25, +15), ArenaColor.Border);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DestroySideTiles:
                    _curState = State.SidesAboutToBeDestroyed;
                    break;
                case AID.Levinforge:
                    _levinforge = caster;
                    break;
                case AID.ByregotSpire:
                    _spire = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DestroySideTiles:
                    _curState = State.Active;
                    break;
                case AID.Levinforge:
                    _levinforge = null;
                    break;
                case AID.ByregotSpire:
                    _spire = null;
                    break;
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x800375A3)
                return;
            if (index is >= 7 and <= 11)
            {
                int i = index - 7;
                (_lineOffset[i], _lineMovement[i]) = state switch
                {
                    0x00020001 => ( 0, +1),
                    0x08000400 => (-1, +1),
                    0x00800040 => ( 0, -1),
                    0x80004000 => (+1, -1),
                    _ => (_lineOffset[i], 0),
                };
                if (_lineMovement[i] == 0)
                    module.ReportError(this, $"Unexpected env-control {i}={state:X}, offset={_lineOffset[i]}");
            }
            else if (index == 26)
            {
                for (int i = 0; i < 5; ++i)
                {
                    _lineOffset[i] += _lineMovement[i];
                    _lineMovement[i] = 0;
                }
            }
            else if (index == 79 && state == 0x00080004)
            {
                _curState = State.Inactive;
                Array.Fill(_lineOffset, 0);
                Array.Fill(_lineMovement, 0);
            }
        }

        private bool CellDangerous(int x, int z, bool future)
        {
            int off = _lineOffset[z + 2];
            if (future)
                off += _lineMovement[z + 2];
            return (future || _curState == State.Active) && Math.Abs(x - off) > 1;
        }
    }
}
