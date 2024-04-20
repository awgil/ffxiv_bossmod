namespace BossMod.Endwalker.Savage.P11SThemis;

class DivisiveOverruling(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shapeNarrow = new(46, 8, 23); // note: boss variants are 23+23, clone variants are 46+0, doesn't matter too much
    private static readonly AOEShapeRect _shapeWide = new(46, 13, 23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = AOEs.FirstOrDefault().Activation.AddSeconds(1);
        return AOEs.TakeWhile(a => a.Activation < deadline);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DivisiveOverrulingSoloAOE: // first aoe
            case AID.DivisiveRulingAOE:
            case AID.DivisiveOverrulingBossAOE:
                AddAOE(new(_shapeNarrow, caster.Position, spell.Rotation, spell.NPCFinishAt));
                break;
            case AID.DivineRuinationSolo:
            case AID.DivineRuinationClone:
            case AID.DivineRuinationBoss:
                AddAOE(new(_shapeWide, caster.Position, spell.Rotation, spell.NPCFinishAt));
                break;
            case AID.RipplesOfGloomSoloR:
            case AID.RipplesOfGloomCloneR:
            case AID.RipplesOfGloomBossR:
                AddAOE(new(_shapeNarrow, caster.Position + 2 * _shapeNarrow.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, spell.NPCFinishAt));
                break;
            case AID.RipplesOfGloomSoloL:
            case AID.RipplesOfGloomCloneL:
            case AID.RipplesOfGloomBossL:
                AddAOE(new(_shapeNarrow, caster.Position + 2 * _shapeNarrow.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.NPCFinishAt));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DivisiveOverrulingSoloAOE:
            case AID.DivisiveRulingAOE:
            case AID.DivisiveOverrulingBossAOE:
            case AID.DivineRuinationSolo:
            case AID.DivineRuinationClone:
            case AID.DivineRuinationBoss:
            case AID.RipplesOfGloomSoloR:
            case AID.RipplesOfGloomCloneR:
            case AID.RipplesOfGloomBossR:
            case AID.RipplesOfGloomSoloL:
            case AID.RipplesOfGloomCloneL:
            case AID.RipplesOfGloomBossL:
                if (AOEs.Count > 0)
                    AOEs.RemoveAt(0);
                ++NumCasts;
                break;
        }
    }

    private void AddAOE(AOEInstance aoe)
    {
        AOEs.Add(aoe);
        AOEs.SortBy(aoe => aoe.Activation);
    }
}
