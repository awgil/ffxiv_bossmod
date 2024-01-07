namespace BossMod {
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class SAMConfig : ConfigNode {
        [PropertyDisplay("Execute optimal rotations on Hakaze (ST) or Fuko/Fuga (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Reserve 10 Kenki for mobility skills (Gyoten/Yaten)")]
        public bool ReserveKenki = true;

        [PropertyDisplay("Automatically spend Kenki gauge")]
        public bool UseKenki = true;
    }
}
