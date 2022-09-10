using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    // our grid looks like this:
    // 0  1  2  3
    // 4  5  6  7
    // 8  9  A  B
    // C  D  E  F
    class Polyominoid : Components.GenericAOEs
    {
        public enum State { None, Plus, Cross }

        private State[] _states = new State[16];
        private List<(Actor, Actor)> _tethers = new();
        private BitMask _dangerCells;
        private bool _dangerDirty;

        private static AOEShape _shape = new AOEShapeRect(5, 5, 5);

        public Polyominoid() : base(ActionID.MakeSpell(AID.PolyominousDark)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: timing...
            return _dangerCells.SetBits().Select(index => (_shape, IndexToPosition(index), 0.Degrees(), module.WorldState.CurrentTime));
        }

        public override void Update(BossModule module)
        {
            if (!_dangerDirty)
                return;

            var effStates = _states.ToArray();
            foreach (var (from, to) in _tethers)
            {
                var i1 = PositionToIndex(from.Position);
                var i2 = PositionToIndex(to.Position);
                var t = effStates[i1];
                effStates[i1] = effStates[i2];
                effStates[i2] = t;
            }

            _dangerDirty = false;
            _dangerCells = new();
            for (int i = 0; i < effStates.Length; i++)
            {
                switch (effStates[i])
                {
                    case State.Plus:
                        AddPlus(i);
                        break;
                    case State.Cross:
                        AddCross(i);
                        break;
                }
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.PolyExchange)
            {
                var target = module.WorldState.Actors.Find(tether.Target);
                if (target != null)
                {
                    _tethers.Add((source, target));
                    _dangerDirty = true;
                }
            }
        }

        //public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        //{
        //    if (tether.ID == (uint)TetherID.PolyExchange)
        //    {
        //        _tethers.RemoveAll(st => st.Item1 == source);
        //        _dangerDirty = true;
        //    }
        //}

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x800375A9)
                return;

            int square = index switch
            {
                1 => 0x0,
                2 => 0x1,
                3 => 0x2,
                4 => 0x3,
                5 => 0x5,
                6 => 0x6,
                7 => 0x9,
                8 => 0xA,
                9 => 0xC,
                10 => 0xD,
                11 => 0xE,
                12 => 0xF,
                13 => 0x8,
                14 => 0xB,
                15 => 0x4,
                16 => 0x7, // not sure
                _ => -1
            };

            if (square < 0)
                return;

            switch (state)
            {
                case 0x00020001: // +
                //case 0x00100001: // x to +
                    _states[square] = State.Plus;
                    break;
                case 0x00400020: // x
                //case 0x00800020: // + to x
                    _states[square] = State.Cross;
                    break;
                //case 0x00080004:
                //    _states[square] = State.None;
                //    break;
            }
            _dangerDirty = true;
        }

        private int CoordinateToIndex(float c) => c switch
        {
            < 90 => 0,
            < 100 => 1,
            < 110 => 2,
            _ => 3
        };
        private int PositionToIndex(WPos pos) => CoordinateToIndex(pos.Z) * 4 + CoordinateToIndex(pos.X);
        private float IndexToCoordinate(int i) => 85 + 10 * i;
        private WPos IndexToPosition(int i) => new(IndexToCoordinate(i & 3), IndexToCoordinate(i >> 2));

        private void AddPlus(int sq)
        {
            int x = sq & 0x3;
            int z = sq & 0xC;
            for (int i = 0; i < 4; i++)
                _dangerCells.Set(z + i);
            for (int i = 0; i < 4; ++i)
                _dangerCells.Set(x + i * 4);
        }

        private void AddCross(int sq)
        {
            int x = sq & 0x3;
            int z = sq & 0xC;
            _dangerCells.Set(sq);
            for (int i = 1; i < 4; ++i)
            {
                var x1 = x + i;
                var x2 = x - i;
                var z1 = z + i * 4;
                var z2 = z - i * 4;
                if (x1 < 4 && z1 < 16)
                    _dangerCells.Set(x1 + z1);
                if (x1 < 4 && z2 >= 0)
                    _dangerCells.Set(x1 + z2);
                if (x2 >= 0 && z1 < 16)
                    _dangerCells.Set(x2 + z1);
                if (x2 >= 0 && z2 >= 0)
                    _dangerCells.Set(x2 + z2);
            }
        }
    }
}
