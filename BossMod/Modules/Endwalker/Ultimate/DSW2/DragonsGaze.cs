using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // used by two trio mechanics, in p2 and in p5
    class DragonsGaze : Components.GenericGaze
    {
        public bool EnableHints;
        private OID _bossOID;
        private Actor? _boss;
        private WPos _eyePosition;

        public bool Active => _boss != null;

        public DragonsGaze(OID bossOID) : base(ActionID.MakeSpell(AID.DragonsGazeAOE))
        {
            _bossOID = bossOID;
        }

        public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
        {
            // TODO: activation time
            if (_boss != null && NumCasts == 0)
            {
                yield return new(_eyePosition, risky: EnableHints);
                yield return new(_boss.Position, risky: EnableHints);
            }
        }

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
            if (state == 0x00020001 && index <= 7)
            {
                _boss = module.Enemies(_bossOID).FirstOrDefault();
                _eyePosition = module.Bounds.Center + 40 * (180 - index * 45).Degrees().ToDirection();
            }
        }
    }
}
