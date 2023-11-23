using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    // note: apparently there's a slight overlap between aoes in the center, which looks ugly, but at least that's the truth...
    class VisceralWhirl : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shapeNormal = new(29, 14);
        private static AOEShapeRect _shapeOffset = new(60, 14);

        public bool Active => _aoes.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.VisceralWhirlRAOE1:
                case AID.VisceralWhirlLAOE1:
                    _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, spell.FinishAt));
                    break;
                case AID.VisceralWhirlRAOE2:
                    _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.FinishAt));
                    break;
                case AID.VisceralWhirlLAOE2:
                    _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, spell.FinishAt));
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.VisceralWhirlRAOE1 or AID.VisceralWhirlRAOE2 or AID.VisceralWhirlLAOE1 or AID.VisceralWhirlLAOE2)
                _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
        }
    }

    class MiasmicBlast : Components.SelfTargetedAOEs
    {
        public MiasmicBlast() : base(ActionID.MakeSpell(AID.MiasmicBlast), new AOEShapeCross(60, 5)) { }
    }

    class VoidBio : Components.GenericAOEs
    {
        private List<Actor> _bubbles = new();

        private static AOEShapeCircle _shape = new(2); // TODO: verify explosion radius

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _bubbles.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));

        public override void Init(BossModule module)
        {
            _bubbles = module.Enemies(OID.ToxicBubble);
        }
    }

    class BondsOfDarkness : BossComponent
    {
        public int NumTethers { get; private set; }
        private int[] _partners = Utils.MakeArray(PartyState.MaxPartySize, -1);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_partners[slot] >= 0)
                hints.Add("Break tether!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _partners[pcSlot] == playerSlot ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var partner = module.Raid[_partners[pcSlot]];
            if (partner != null)
                arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.BondsOfDarkness)
            {
                var slot1 = module.Raid.FindSlot(source.InstanceID);
                var slot2 = module.Raid.FindSlot(tether.Target);
                if (slot1 >= 0 && slot2 >= 0)
                {
                    ++NumTethers;
                    _partners[slot1] = slot2;
                    _partners[slot2] = slot1;
                }
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.BondsOfDarkness)
            {
                var slot1 = module.Raid.FindSlot(source.InstanceID);
                var slot2 = module.Raid.FindSlot(tether.Target);
                if (slot1 >= 0 && slot2 >= 0)
                {
                    --NumTethers;
                    _partners[slot1] = -1;
                    _partners[slot2] = -1;
                }
            }
        }
    }

    class DarkDivides : Components.UniformStackSpread
    {
        public DarkDivides() : base(0, 5) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.DivisiveDark)
                AddSpread(actor, status.ExpireAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DarkDivides)
                Spreads.Clear();
        }
    }
}
