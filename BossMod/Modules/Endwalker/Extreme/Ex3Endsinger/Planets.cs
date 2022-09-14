using System.Collections.Generic;

namespace BossMod.Endwalker.Extreme.Ex3Endsigner
{
    // used both for single planets (elegeia) and successions (fatalism)
    class Planets : BossComponent
    {
        private Actor? _head;
        private List<WPos> _planetsFiery = new();
        private List<WPos> _planetsAzure = new();

        private static AOEShapeCone _aoeHead = new(20, 90.Degrees());
        private static AOEShapeCircle _aoePlanet = new(30);
        private static float _knockbackDistance = 25;
        private static float _planetOffset = 19.8f; // == 14 * sqrt(2)

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoeHead.Check(actor.Position, _head))
            {
                hints.Add("GTFO from head aoe!");
            }
            if (_planetsFiery.Count > 0 && _aoePlanet.Check(actor.Position, _planetsFiery[0]))
            {
                hints.Add("GTFO from planet aoe!");
            }
            if (_planetsAzure.Count > 0)
            {
                var offsetLocation = Components.Knockback.AwayFromSource(actor.Position, _planetsAzure[0], _knockbackDistance);
                if (!module.Bounds.Contains(offsetLocation))
                {
                    hints.Add("About to be knocked into wall!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoeHead.Draw(arena, _head);
            if (_planetsFiery.Count > 0)
            {
                _aoePlanet.Draw(arena, _planetsFiery[0]);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_planetsAzure.Count > 0)
            {
                var offsetLocation = Components.Knockback.AwayFromSource(pc.Position, _planetsAzure[0], _knockbackDistance);
                arena.AddLine(pc.Position, offsetLocation, ArenaColor.Danger);
                arena.Actor(offsetLocation, pc.Rotation, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.DiairesisElegeia)
                _head = caster;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (_head == caster)
                _head = null;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FatalismFieryStar1:
                    AddPlanet(module, caster, false, true);
                    break;
                case AID.FatalismFieryStar2:
                case AID.FieryStarVisual:
                    AddPlanet(module, caster, false, false);
                    break;
                case AID.FatalismAzureStar1:
                    AddPlanet(module, caster, true, true);
                    break;
                case AID.FatalismAzureStar2:
                case AID.AzureStarVisual:
                    AddPlanet(module, caster, true, false);
                    break;
                case AID.RubistellarCollision:
                case AID.FatalismRubistallarCollisionAOE:
                    if (_planetsFiery.Count > 0)
                        _planetsFiery.RemoveAt(0);
                    else
                        module.ReportError(this, "Unexpected fiery cast, no casters available");
                    break;
                case AID.CaerustellarCollision:
                case AID.FatalismCaerustallarCollisionAOE:
                    if (_planetsAzure.Count > 0)
                        _planetsAzure.RemoveAt(0);
                    else
                        module.ReportError(this, "Unexpected azure cast, no casters available");
                    break;
            }
        }

        private void AddPlanet(BossModule module, Actor caster, bool azure, bool firstOfPair)
        {
            var origin = module.Bounds.Center + _planetOffset * caster.Rotation.ToDirection();
            var planets = azure ? _planetsAzure : _planetsFiery;
            int index = firstOfPair ? 0 : planets.Count;
            planets.Insert(index, origin);
        }
    }
}
