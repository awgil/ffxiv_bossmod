namespace BossMod.Endwalker.Alliance.A30OpeningMobs
{
    public enum OID : uint
    {
        Serpent = 0x4010, // R3.450, x6
        Triton = 0x4011, // R1.950, x2
        DivineSprite = 0x4012, // R1.600, x3
        WaterSprite = 0x4085, // R0.800, x5
        UnknownEnemy = 0x400E, // R0.500, x1
        UnknownActor1 = 0x1E8FB8, // R2.000, x2, EventObj type
        UnknownActor2 = 0x1E8F2F, // R0.500, x1, EventObj type        
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Serpent/Triton->player, no cast, single-target
        WaterIII = 35438, // Serpent->location, 4.0s cast, range 8 circle
        PelagicCleaver1 = 35439, // Triton->self, 5.0s cast, range 40 60-degree cone
        PelagicCleaver2 = 35852, // Triton->self, 5.0s cast, range 40 60-degree cone
        Water = 35469, // Water Sprite/Divine Sprite->player, no cast, single-target
        WaterFlood = 35442, // Water Sprite->self, 3.0s cast, range 6 circle
        WaterBurst = 35443, // Water Sprite->self, no cast, range 40 circle
        DivineFlood = 35440, // Divine Sprite->self, 3.0s cast, range 6 circle
        DivineBurst = 35441, // Divine Sprite->self, no cast, range 40 circle
    };
}
