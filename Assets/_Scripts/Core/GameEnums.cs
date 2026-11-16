public enum GameState { MainMenu, Match, Shop, GameOver }
public enum GameScenes { MainMenuScene, StageScene, ShopScene }
public enum CardType { Ankh, Triquetra, Lotus, Crescent, Bomb } //bomb = zonk
public enum ShopItemType { PermanentBuff, PassiveBuff, Consumable }
public enum ItemRarity { Dusty, Arcane, Eldritch } //common, rare, legend/mythic
public enum GrimoireTarget { None, Triquetra, Crescent, Ankh, Lotus, Covenant }
public enum WaxSealType
{
    None,
    Fortune, // 40% Token
    GlassCannon, // Low Health tapi Mult Tinggi
    Bloodstone, // +2 Max Health
    FirstBlood, // 3x mult match pertama, 2x mult match ke dua
    Crescendo, // combo 3x dst, +50 base score
    SwiftExecution, // cluster 1, 30 token + 1 health
    EyeOfHorus, // awal cluster intip 2 kartu 1 detik
    UndyingCovenant, // revive, +1 hp +2 turn
    TwinEclipse, // match Crescent, combo tambah 2, bukan 1 spt biasanya
    AnkhBlessing, // match Ankh, +1 token untuk setiap 10 token yang dimiliki
    TrinityCadence, // match Triquetra, saat turn kelipatan 3, +1.5 mult
    SereneLotus, // match Lotus ketika hp full turn +1 (tdk berkurang)
    Ouroboros, // tiap cluster habis, +20 base score & +0.15 base mult permanen
}
public enum ConsumableType
{
    None,
    Lens, // Intip 3 kartu random selama 1.5 detik
    Hourglass, // +3 Turn
    Garlic, // Tandai/netralkan 1 kartu bom aktif
    Elixir // 2x Match Mult selama 3 turn ke depan
}
