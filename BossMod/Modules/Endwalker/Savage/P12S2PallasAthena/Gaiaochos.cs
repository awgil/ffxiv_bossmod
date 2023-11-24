using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    class Gaiaochos : Components.SelfTargetedAOEs
    {
        public Gaiaochos() : base(ActionID.MakeSpell(AID.GaiaochosTransition), new AOEShapeDonut(7, 30)) { }
    }

    // TODO: we could show it earlier, casters do PATE 11D2 ~4s before starting cast
    class UltimaRay : Components.SelfTargetedAOEs
    {
        public UltimaRay() : base(ActionID.MakeSpell(AID.UltimaRay), new AOEShapeRect(20, 3)) { }
    }

    class MissingLink : Components.CastCounter
    {
        public bool TethersAssigned { get; private set; }
        private int[] _partner = Utils.MakeArray(PartyState.MaxPartySize, -1);

        public MissingLink() : base(ActionID.MakeSpell(AID.MissingLink)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_partner[slot] >= 0)
                hints.Add("Break the tether!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _partner[pcSlot] == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.Raid[_partner[pcSlot]] is var partner && partner != null)
                arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.MissingLink)
            {
                TethersAssigned = true;
                var slot1 = module.Raid.FindSlot(source.InstanceID);
                var slot2 = module.Raid.FindSlot(tether.Target);
                if (slot1 >= 0 && slot2 >= 0)
                {
                    _partner[slot1] = slot2;
                    _partner[slot2] = slot1;
                }
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.MissingLink)
            {
                var slot1 = module.Raid.FindSlot(source.InstanceID);
                var slot2 = module.Raid.FindSlot(tether.Target);
                if (slot1 >= 0 && slot2 >= 0)
                {
                    _partner[slot1] = -1;
                    _partner[slot2] = -1;
                }
            }
        }
    }

    class DemiParhelion : Components.SelfTargetedAOEs
    {
        public DemiParhelion() : base(ActionID.MakeSpell(AID.DemiParhelionAOE), new AOEShapeCircle(2)) { }
    }

    class Geocentrism : Components.GenericAOEs
    {
        public int NumConcurrentAOEs { get; private set; }
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shapeLine = new(20, 2);
        private static AOEShapeCircle _shapeCircle = new(2);
        private static AOEShapeDonut _shapeDonut = new(3, 7);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.GeocentrismV:
                    _aoes.Add(new(_shapeLine, new(95, 83), default, spell.FinishAt.AddSeconds(0.6f)));
                    _aoes.Add(new(_shapeLine, new(100, 83), default, spell.FinishAt.AddSeconds(0.6f)));
                    _aoes.Add(new(_shapeLine, new(105, 83), default, spell.FinishAt.AddSeconds(0.6f)));
                    NumConcurrentAOEs = 3;
                    break;
                case AID.GeocentrismC:
                    _aoes.Add(new(_shapeCircle, new(100, 90), default, spell.FinishAt.AddSeconds(0.6f)));
                    _aoes.Add(new(_shapeDonut, new(100, 90), default, spell.FinishAt.AddSeconds(0.6f)));
                    NumConcurrentAOEs = 2;
                    break;
                case AID.GeocentrismH:
                    _aoes.Add(new(_shapeLine, new(93, 85), 90.Degrees(), spell.FinishAt.AddSeconds(0.6f)));
                    _aoes.Add(new(_shapeLine, new(93, 90), 90.Degrees(), spell.FinishAt.AddSeconds(0.6f)));
                    _aoes.Add(new(_shapeLine, new(93, 95), 90.Degrees(), spell.FinishAt.AddSeconds(0.6f)));
                    NumConcurrentAOEs = 3;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.DemiParhelionGeoLine or AID.DemiParhelionGeoDonut or AID.DemiParhelionGeoCircle)
                ++NumCasts;
        }
    }

    class DivineExcoriation : Components.UniformStackSpread
    {
        public DivineExcoriation() : base(0, 1) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DivineExcoriation)
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(3.1f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DivineExcoriation)
                Spreads.Clear();
        }
    }

    class GaiaochosEnd : BossComponent
    {
        public bool Finished { get; private set; }

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            // note: there are 3 env controls happening at the same time, not sure which is the actual trigger: .9=02000001, .11=00800001, .12=00080004
            if (index == 9 && state == 0x02000001)
                Finished = true;
        }
    }

    // TODO: assign pairs, draw wrong pairs as aoes
    class UltimaBlow : Components.CastCounter
    {
        private List<(Actor source, Actor target)> _tethers = new();
        private BitMask _vulnerable;

        private static AOEShapeRect _shape = new(20, 3);

        public UltimaBlow() : base(ActionID.MakeSpell(AID.UltimaBlow)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_vulnerable[slot])
            {
                var source = _tethers.Find(t => t.target == actor).source;
                var numHit = source != null ? module.Raid.WithoutSlot().Exclude(actor).InShape(_shape, source.Position, Angle.FromDirection(actor.Position - source.Position)).Count() : 0;
                if (numHit == 0)
                    hints.Add("Hide behind partner!");
                else if (numHit > 1)
                    hints.Add("Bait away from raid!");
            }
            else if (_tethers.Count > 0)
            {
                var numHit = _tethers.Count(t => _shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)));
                if (numHit == 0)
                    hints.Add("Intercept the charge!");
                else if (numHit > 1)
                    hints.Add("GTFO from other charges!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _tethers.Any(t => t.target == player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_vulnerable[pcSlot]) // TODO: reconsider
                foreach (var t in _tethers.Where(t => t.target != pc))
                    _shape.Draw(arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in _tethers)
            {
                arena.Actor(t.source, ArenaColor.Object, true);
                arena.AddLine(t.source.Position, t.target.Position, ArenaColor.Danger);
                if (t.target == pc || !_vulnerable[pcSlot])
                    _shape.Outline(arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position), t.target == pc ? ArenaColor.Safe : ArenaColor.Danger); // TODO: reconsider...
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.ClassicalConceptsShapes && module.WorldState.Actors.Find(tether.Target) is var target && target != null)
            {
                _tethers.Add((source, target));
                _vulnerable.Set(module.Raid.FindSlot(tether.Target));
            }
        }
    }
}
