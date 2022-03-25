using System;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component tracking boss weapon/stance switches; also draws imminent aoe after each switch
    // note: we could rely on invisible buff 2273 to select weapon (n/a for sword, 1B4 for staff, 1B5 for chakram), it appears slightly earlier than 'official' buff
    class WeaponTracker : Component
    {
        public enum Stance { None, Sword, Staff, Chakram }
        public Stance CurStance { get; private set; }
        public bool AOEImminent { get; private set; }

        private static AOEShapeRect _aoeSword = new(20, 5, 20);
        private static AOEShapeCircle _aoeStaff = new(10);
        private static AOEShapeDonut _aoeChakram = new(5, 40);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!AOEImminent)
                return;

            bool inAOE = CurStance switch
            {
                Stance.Sword => _aoeSword.Check(actor.Position, module.PrimaryActor.Position, 0) || _aoeSword.Check(actor.Position, module.PrimaryActor.Position, MathF.PI / 2),
                Stance.Staff => _aoeStaff.Check(actor.Position, module.PrimaryActor.Position, 0),
                Stance.Chakram => _aoeChakram.Check(actor.Position, module.PrimaryActor.Position, 0),
                _ => false
            };
            if (inAOE)
                hints.Add("GTFO from weapon aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!AOEImminent)
                return;

            switch (CurStance)
            {
                case Stance.Sword:
                    _aoeSword.Draw(arena, module.PrimaryActor.Position, 0);
                    _aoeSword.Draw(arena, module.PrimaryActor.Position, MathF.PI / 2);
                    break;
                case Stance.Staff:
                    _aoeStaff.Draw(arena, module.PrimaryActor.Position, 0);
                    break;
                case Stance.Chakram:
                    _aoeChakram.Draw(arena, module.PrimaryActor.Position, 0);
                    break;
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            if (actor != module.PrimaryActor)
                return;

            var newStance = (SID)actor.Statuses[index].ID switch
            {
                SID.HerosMantle => Stance.Sword,
                SID.MagosMantle => Stance.Staff,
                SID.MousaMantle => Stance.Chakram,
                _ => Stance.None
            };
            if (newStance == Stance.None)
                return;

            AOEImminent = CurStance != Stance.None;
            CurStance = newStance;
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell() && (AID)info.Action.ID is AID.WeaponChangeAOEChakram or AID.WeaponChangeAOEStaff or AID.WeaponChangeAOESword)
                AOEImminent = false;
        }
    }
}
