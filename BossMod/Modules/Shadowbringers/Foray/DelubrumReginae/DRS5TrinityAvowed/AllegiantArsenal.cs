using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    class AllegiantArsenal : Components.GenericAOEs
    {
        public enum Order { Unknown, SwordSecond, BowSecond, StaffSecond, StaffSwordBow, BowSwordStaff, SwordBowStaff, StaffBowSword, SwordStaffBow, BowStaffSword }

        public Order Mechanics { get; private set; }
        private AOEShape? _pendingAOE;
        private DateTime _activation;

        public bool Active => _pendingAOE != null;

        private static AOEShapeCone _shapeSword = new(70, 135.Degrees(), 180.Degrees());
        private static AOEShapeCone _shapeBow = new(70, 135.Degrees());
        private static AOEShapeCircle _shapeStaff = new(10);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_pendingAOE != null)
                yield return new(_pendingAOE, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AllegiantArsenalSword:
                    Activate(_shapeSword, spell.NPCFinishAt, Mechanics switch
                    {
                        Order.Unknown => Order.SwordSecond,
                        Order.BowSecond => Order.StaffBowSword,
                        Order.StaffSecond => Order.BowStaffSword,
                        _ => Order.Unknown
                    });
                    break;
                case AID.AllegiantArsenalBow:
                    Activate(_shapeBow, spell.NPCFinishAt, Mechanics switch
                    {
                        Order.Unknown => Order.BowSecond,
                        Order.SwordSecond => Order.StaffSwordBow,
                        Order.StaffSecond => Order.SwordStaffBow,
                        _ => Order.Unknown
                    });
                    break;
                case AID.AllegiantArsenalStaff:
                    Activate(_shapeStaff, spell.NPCFinishAt, Mechanics switch
                    {
                        Order.Unknown => Order.StaffSecond,
                        Order.SwordSecond => Order.BowSwordStaff,
                        Order.BowSecond => Order.SwordBowStaff,
                        _ => Order.Unknown
                    });
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.InfernalSlash or AID.Flashvane or AID.FuryOfBozja)
                _pendingAOE = null;
        }

        private void Activate(AOEShape shape, DateTime finishAt, Order newOrder)
        {
            _pendingAOE = shape;
            _activation = finishAt.AddSeconds(5.2f);
            if (newOrder != Order.Unknown)
                Mechanics = newOrder;
        }
    }
}
