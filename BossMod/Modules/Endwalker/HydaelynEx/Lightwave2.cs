using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component for second lightwave (3 waves, 5 crystals) + hero's glory mechanics
    class Lightwave2 : LightwaveCommon
    {
        private Vector3 _safeCrystal;
        private Vector4? _safeCrystalOrigin;

        private static Vector3 _crystalCenter = new(100, 0, 101);
        private static Vector3 _crystalTL = new( 90, 0,  92);
        private static Vector3 _crystalTR = new(110, 0,  92);
        private static Vector3 _crystalBL = new( 90, 0, 110);
        private static Vector3 _crystalBR = new(110, 0, 110);
        private static AOEShapeCone _gloryAOE = new(40, Angle.Radians(MathF.PI / 2));

        public override void Update(BossModule module)
        {
            if (NumCasts == 4 && (module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && module.PrimaryActor.PosRot != _safeCrystalOrigin)
            {
                _safeCrystalOrigin = module.PrimaryActor.PosRot;
                _safeCrystal = new Vector3[] { _crystalTL, _crystalTR, _crystalBL, _crystalBR }.FirstOrDefault(c => !_gloryAOE.Check(c, module.PrimaryActor));
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if ((module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && _gloryAOE.Check(actor.Position, module.PrimaryActor))
                hints.Add("GTFO from glory aoe!");

            (bool inWave, bool inSafeCone) = NumCasts < 4
                ? (WaveAOE.Check(actor.Position, Wave1Pos(), new()) || WaveAOE.Check(actor.Position, Wave2Pos(), new()), InSafeCone(NextSideCrystal(), _crystalCenter, actor.Position))
                : (WaveAOE.Check(actor.Position, Wave3Pos(), new()), _safeCrystal != Vector3.Zero ? InSafeCone(_crystalCenter, _safeCrystal, actor.Position) : true);

            if (inWave)
                hints.Add("GTFO from wave!");
            if (!inSafeCone)
                hints.Add("Hide behind crystal!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false)
                _gloryAOE.Draw(arena, module.PrimaryActor);

            if (NumCasts < 4)
            {
                WaveAOE.Draw(arena, Wave1Pos(), new());
                WaveAOE.Draw(arena, Wave2Pos(), new());
                DrawSafeCone(arena, NextSideCrystal(), _crystalCenter);
            }
            else
            {
                WaveAOE.Draw(arena, Wave3Pos(), new());
                if (_safeCrystal != Vector3.Zero)
                {
                    DrawSafeCone(arena, _crystalCenter, _safeCrystal);
                }
            }
        }

        private Vector3 Wave1Pos() => Waves.Count > 0 ? Waves[0].Position : new(86, 0, 70);
        private Vector3 Wave2Pos() => Waves.Count switch
        {
            0 => new Vector3(114, 0, 70),
            1 => new Vector3(Waves[0].Position.X < 100 ? 114 : 86, 0, 70),
            _ => Waves[1].Position
        };
        private Vector3 Wave3Pos() => Waves.Count > 2 ? Waves[2].Position : new(100, 0, 70);

        private Vector3 NextSideCrystal()
        {
            bool w1Next = (NumCasts & 1) == 0;
            bool w1Left = Wave1Pos().X < 100;
            float nextX = w1Next == w1Left ? _crystalTL.X : _crystalBR.X;
            float nextZ = (NumCasts & 2) == 0 ? _crystalTL.Z : _crystalBR.Z;
            return new(nextX, 0, nextZ);
        }
    }
}
