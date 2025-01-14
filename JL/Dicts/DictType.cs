﻿using System.ComponentModel;

namespace JL.Dicts
{
    public enum DictType
    {
        // built-in
        JMdict,
        JMnedict,
        Kanjidic,
        [Description("Custom Word Dictionary")]
        CustomWordDictionary,
        [Description("Custom Name Dictionary")]
        CustomNameDictionary,

        // Yomichan Import
        [Description("Kenkyuusha (Yomichan)")]
        Kenkyuusha,
        [Description("Daijirin (Yomichan)")]
        Daijirin,
        [Description("Daijisen (Yomichan)")]
        Daijisen,
        [Description("Koujien (Yomichan)")]
        Koujien,
        [Description("Meikyou (Yomichan)")]
        Meikyou,
        [Description("Gakken (Yomichan)")]
        Gakken,
        [Description("Kotowaza (Yomichan)")]
        Kotowaza,

        // Nazeka Epwing Converter
        [Description("Kenkyuusha (Nazeka)")]
        KenkyuushaNazeka,
        [Description("Daijirin (Nazeka)")]
        DaijirinNazeka,
        [Description("Shinmeikai (Nazeka)")]
        ShinmeikaiNazeka,
    }
}
