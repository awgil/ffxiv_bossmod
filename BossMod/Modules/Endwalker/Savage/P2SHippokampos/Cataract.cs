namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to cataract mechanic
class Cataract(BossModule module) : BossComponent(module)
{
    private readonly AOEShapeRect _aoeBoss = new(50, 7.5f, 50);
    private readonly AOEShapeRect _aoeHead = new(50, 50, 0, module.PrimaryActor.CastInfo?.IsSpell(AID.WingedCataract) ?? false ? 180.Degrees() : default);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoeBoss.Check(actor.Position, Module.PrimaryActor) || _aoeHead.Check(actor.Position, Module.Enemies(OID.CataractHead).FirstOrDefault()))
            hints.Add("GTFO from cataract!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoeBoss.Draw(Arena, Module.PrimaryActor);
        _aoeHead.Draw(Arena, Module.Enemies(OID.CataractHead).FirstOrDefault());
    }
}
