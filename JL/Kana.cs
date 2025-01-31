﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using JL.Utilities;

namespace JL
{
    public static class Kana
    {
        private static readonly Dictionary<string, string> s_hiraganaToKatakanaDict = new()
        {
            #pragma warning disable format
            { "あ", "ア" }, { "い", "イ" }, { "う", "ウ" }, { "え", "エ" }, { "お", "オ" },
            { "か", "カ" }, { "き", "キ" }, { "く", "ク" }, { "け", "ケ" }, { "こ", "コ" },
            { "さ", "サ" }, { "し", "シ" }, { "す", "ス" }, { "せ", "セ" }, { "そ", "ソ" },
            { "た", "タ" }, { "ち", "チ" }, { "つ", "ツ" }, { "て", "テ" }, { "と", "ト" },
            { "な", "ナ" }, { "に", "ニ" }, { "ぬ", "ヌ" }, { "ね", "ネ" }, { "の", "ノ" },
            { "は", "ハ" }, { "ひ", "ヒ" }, { "ふ", "フ" }, { "へ", "ヘ" }, { "ほ", "ホ" },
            { "ま", "マ" }, { "み", "ミ" }, { "む", "ム" }, { "め", "メ" }, { "も", "モ" },
            { "ら", "ラ" }, { "り", "リ" }, { "る", "ル" }, { "れ", "レ" }, { "ろ", "ロ" },

            { "が", "ガ" }, { "ぎ", "ギ" }, { "ぐ", "グ" }, { "げ", "ゲ" }, { "ご", "ゴ" },
            { "ざ", "ザ" }, { "じ", "ジ" }, { "ず", "ズ" }, { "ぜ", "ゼ" }, { "ぞ", "ゾ" },
            { "だ", "ダ" }, { "ぢ", "ヂ" }, { "づ", "ヅ" }, { "で", "デ" }, { "ど", "ド" },
            { "ば", "バ" }, { "び", "ビ" }, { "ぶ", "ブ" }, { "べ", "ベ" }, { "ぼ", "ボ" },
            { "ぱ", "パ" }, { "ぴ", "ピ" }, { "ぷ", "プ" }, { "ぺ", "ペ" }, { "ぽ", "ポ" },

            { "わ", "ワ" }, { "を", "ヲ" },
            { "や", "ヤ" }, { "ゆ", "ユ" }, { "よ", "ヨ" },
            { "ん", "ン" },

            { "ぁ", "ァ" }, { "ぃ", "ィ" }, { "ぅ", "ゥ" }, { "ぇ", "ェ" }, { "ぉ", "ォ" },
            { "ゃ", "ャ" }, { "ゅ", "ュ" }, { "ょ", "ョ" },

            { "ゕ", "ヵ" }, { "ゖ", "ヶ" }, { "ゔ", "ヴ" },
            { "ゝ", "ヽ" }, { "ゞ", "ヾ" }, { "っ", "ッ" },
            { "ゐ゙", "ヸ" }, { "ゑ゙", "ヹ" }, { "を゙", "ヺ" }
            #pragma warning restore format
        };

        private static readonly Dictionary<string, string> s_katakanaToHiraganaDict = new()
        {
            #pragma warning disable format
            { "ア", "あ" }, { "イ", "い" }, { "ウ", "う" }, { "エ", "え" }, { "オ", "お" },
            { "カ", "か" }, { "キ", "き" }, { "ク", "く" }, { "ケ", "け" }, { "コ", "こ" },
            { "サ", "さ" }, { "シ", "し" }, { "ス", "す" }, { "セ", "せ" }, { "ソ", "そ" },
            { "タ", "た" }, { "チ", "ち" }, { "ツ", "つ" }, { "テ", "て" }, { "ト", "と" },
            { "ナ", "な" }, { "ニ", "に" }, { "ヌ", "ぬ" }, { "ネ", "ね" }, { "ノ", "の" },
            { "ハ", "は" }, { "ヒ", "ひ" }, { "フ", "ふ" }, { "ヘ", "へ" }, { "ホ", "ほ" },
            { "マ", "ま" }, { "ミ", "み" }, { "ム", "む" }, { "メ", "め" }, { "モ", "も" },
            { "ラ", "ら" }, { "リ", "り" }, { "ル", "る" }, { "レ", "れ" }, { "ロ", "ろ" },

            { "ガ", "が" }, { "ギ", "ぎ" }, { "グ", "ぐ" }, { "ゲ", "げ" }, { "ゴ", "ご" },
            { "ザ", "ざ" }, { "ジ", "じ" }, { "ズ", "ず" }, { "ゼ", "ぜ" }, { "ゾ", "ぞ" },
            { "ダ", "だ" }, { "ヂ", "ぢ" }, { "ヅ", "づ" }, { "デ", "で" }, { "ド", "ど" },
            { "バ", "ば" }, { "ビ", "び" }, { "ブ", "ぶ" }, { "ベ", "べ" }, { "ボ", "ぼ" },
            { "パ", "ぱ" }, { "ピ", "ぴ" }, { "プ", "ぷ" }, { "ペ", "ぺ" }, { "ポ", "ぽ" },

            { "ワ", "わ" }, { "ヲ", "を" },
            { "ヤ", "や" }, { "ユ", "ゆ" }, { "ヨ", "よ" },
            { "ン", "ん" },

            { "ァ", "ぁ" }, { "ィ", "ぃ" }, { "ゥ", "ぅ" }, { "ェ", "ぇ" }, { "ォ", "ぉ" },
            { "ャ", "ゃ" }, { "ュ", "ゅ" }, { "ョ", "ょ" },

            { "ヴ", "ゔ" }, { "ヽ", "ゝ" }, { "ヾ", "ゞ" }, { "ッ", "っ" },

            { "ヸ", "ゐ゙" }, { "ヹ", "ゑ゙" }, { "ヺ", "を゙" }
            #pragma warning restore format
        };

        private static readonly Dictionary<string, string> s_kanaFinalVowelDict = new()
        {
            #pragma warning disable format
            // Katakana
            { "ア", "ア" }, { "カ", "ア" }, { "サ", "ア" }, { "タ", "ア" }, { "ナ", "ア" }, { "ハ", "ア" },
            { "マ", "ア" }, { "ラ", "ア" }, { "ガ", "ア" }, { "ザ", "ア" }, { "ダ", "ア" }, { "バ", "ア" },
            { "パ", "ア" }, { "ワ", "ア" }, { "ヤ", "ア" }, { "ァ", "ア" }, { "ャ", "ア" }, { "ヵ", "ア" },

            { "イ", "イ" }, { "キ", "イ" }, { "シ", "イ" }, { "チ", "イ" }, { "ニ", "イ" }, { "ヰ", "イ" },
            { "ヒ", "イ" }, { "ミ", "イ" }, { "リ", "イ" }, { "ギ", "イ" }, { "ジ", "イ" }, { "ヂ", "イ" },
            { "ビ", "イ" }, { "ピ", "イ" }, { "ィ", "イ" }, { "ヸ", "イ" },

            { "ウ", "ウ" }, { "ク", "ウ" }, { "ス", "ウ" }, { "ツ", "ウ" }, { "ヌ", "ウ" }, { "フ", "ウ" },
            { "ム", "ウ" }, { "ル", "ウ" }, { "グ", "ウ" }, { "ズ", "ウ" }, { "ヅ", "ウ" }, { "ブ", "ウ" },
            { "プ", "ウ" }, { "ユ", "ウ" }, { "ゥ", "ウ" }, { "ュ", "ウ" }, { "ヴ", "ウ" },

            { "エ", "エ" }, { "ケ", "エ" }, { "セ", "エ" }, { "テ", "エ" }, { "ネ", "エ" }, { "ヘ", "エ" },
            { "メ", "エ" }, { "レ", "エ" }, { "ゲ", "エ" }, { "ゼ", "エ" }, { "デ", "エ" }, { "ベ", "エ" },
            { "ペ", "エ" }, { "ヱ", "エ" }, { "ェ", "エ" }, { "ヶ", "エ" }, { "ヹ", "エ" },

            { "オ", "オ" }, { "コ", "オ" }, { "ソ", "オ" }, { "ト", "オ" }, { "ノ", "オ" }, { "ホ", "オ" },
            { "モ", "オ" }, { "ロ", "オ" }, { "ゴ", "オ" }, { "ゾ", "オ" }, { "ド", "オ" }, { "ボ", "オ" },
            { "ポ", "オ" }, { "ヲ", "オ" }, { "ヨ", "オ" }, { "ォ", "オ" }, { "ョ", "オ" }, { "ヺ", "オ" },

            //Hiragana
            { "あ", "あ" }, { "か", "あ" }, { "さ", "あ" }, { "た", "あ" }, { "な", "あ" }, { "は", "あ" },
            { "ま", "あ" }, { "ら", "あ" }, { "が", "あ" }, { "ざ", "あ" }, { "だ", "あ" }, { "ば", "あ" },
            { "ぱ", "あ" }, { "わ", "あ" }, { "や", "あ" }, { "ぁ", "あ" }, { "ゃ", "あ" }, { "ゕ", "あ" },

            { "い", "い" }, { "き", "い" }, { "し", "い" }, { "ち", "い" }, { "に", "い" }, { "ひ", "い" },
            { "み", "い" }, { "り", "い" }, { "ぎ", "い" }, { "じ", "い" }, { "ぢ", "い" }, { "び", "い" },
            { "ぴ", "い" }, { "ぃ", "い" }, { "ゐ", "い" }, { "ゐ゙", "イ" },

            { "う", "う" }, { "く", "う" }, { "す", "う" }, { "つ", "う" }, { "ぬ", "う" }, { "ふ", "う" },
            { "む", "う" }, { "る", "う" }, { "ぐ", "う" }, { "ず", "う" }, { "づ", "う" }, { "ぶ", "う" },
            { "ぷ", "う" }, { "ゆ", "う" }, { "ぅ", "う" }, { "ゅ", "う" }, { "ゔ", "う" },

            { "え", "え" }, { "け", "え" }, { "せ", "え" }, { "て", "え" }, { "ね", "え" }, { "へ", "え" },
            { "め", "え" }, { "れ", "え" }, { "げ", "え" }, { "ぜ", "え" }, { "で", "え" }, { "べ", "え" },
            { "ぺ", "え" }, { "ぇ", "え" }, { "ゖ", "え" }, { "ゑ", "え" }, { "ゑ゙", "エ" },

            { "お", "お" }, { "こ", "お" }, { "そ", "お" }, { "と", "お" }, { "の", "お" }, { "ほ", "お" },
            { "も", "お" }, { "ろ", "お" }, { "ご", "お" }, { "ぞ", "お" }, { "ど", "お" }, { "ぼ", "お" },
            { "ぽ", "お" }, { "を", "お" }, { "よ", "お" }, { "ぉ", "お" }, { "ょ", "お" }, { "を゙", "オ" }
            #pragma warning restore format
        };

        private static readonly Dictionary<string, string> s_halfWidthToFullWidthDict = new()
        {
            #pragma warning disable format
            // Half-width katakana
            { "ｱ", "あ" }, { "ｲ", "い" }, { "ｳ", "う" }, { "ｴ", "え" }, { "ｵ", "お" },
            { "ｶ", "か" }, { "ｷ", "き" }, { "ｸ", "く" }, { "ｹ", "け" }, { "ｺ", "こ" },
            { "ｻ", "さ" }, { "ｼ", "し" }, { "ｽ", "す" }, { "ｾ", "せ" }, { "ｿ", "そ" },
            { "ﾀ", "た" }, { "ﾁ", "ち" }, { "ﾂ", "つ" }, { "ﾃ", "て" }, { "ﾄ", "と" },
            { "ﾅ", "な" }, { "ﾆ", "に" }, { "ﾇ", "ぬ" }, { "ﾈ", "ね" }, { "ﾉ", "の" },
            { "ﾊ", "は" }, { "ﾋ", "ひ" }, { "ﾌ", "ふ" }, { "ﾍ", "へ" }, { "ﾎ", "ほ" },
            { "ﾏ", "ま" }, { "ﾐ", "み" }, { "ﾑ", "む" }, { "ﾒ", "め" }, { "ﾓ", "も" },
            { "ﾗ", "ら" }, { "ﾘ", "り" }, { "ﾙ", "る" }, { "ﾚ", "れ" }, { "ﾛ", "ろ" },

            { "ﾜ", "わ" }, { "ｦ", "を" },
            { "ﾔ", "や" }, { "ﾕ", "ゆ" }, { "ﾖ", "よ" },
            { "ﾝ", "ん" },

            { "ｧ", "ぁ" }, { "ｨ", "ぃ" }, { "ｩ", "ぅ" }, { "ｪ", "ぇ" }, { "ｫ", "ぉ" },
            { "ｬ", "ゃ" }, { "ｭ", "ゅ" }, { "ｮ", "ょ" },

            { "ヵ", "ゕ" }, { "ヶ", "ゖ" }, { "ｯ", "っ" },

            // Uppercase letters
            { "A", "Ａ" }, { "B", "Ｂ" }, { "C", "Ｃ" }, { "D", "Ｄ" }, { "E", "Ｅ" }, { "F", "Ｆ" },
            { "G", "Ｇ" }, { "H", "Ｈ" }, { "I", "Ｉ" }, { "J", "Ｊ" }, { "K", "Ｋ" }, { "L", "Ｌ" },
            { "M", "Ｍ" }, { "N", "Ｎ" }, { "O", "Ｏ" }, { "P", "Ｐ" }, { "Q", "Ｑ" }, { "R", "Ｒ" },
            { "S", "Ｓ" }, { "T", "Ｔ" }, { "U", "Ｕ" }, { "V", "Ｖ" }, { "W", "Ｗ" }, { "X", "Ｘ" },
            { "Y", "Ｙ" }, { "Z", "Ｚ" },

            // Lowercase letters
            { "a", "ａ" }, { "b", "ｂ" }, { "c", "ｃ" }, { "d", "ｄ" }, { "e", "ｅ" }, { "f", "ｆ" },
            { "g", "ｇ" }, { "h", "ｈ" }, { "i", "ｉ" }, { "j", "ｊ" }, { "k", "ｋ" }, { "l", "ｌ" },
            { "m", "ｍ" }, { "n", "ｎ" }, { "o", "ｏ" }, { "p", "ｐ" }, { "q", "ｑ" }, { "r", "ｒ" },
            { "s", "ｓ" }, { "t", "ｔ" }, { "u", "ｕ" }, { "v", "ｖ" }, { "w", "ｗ" }, { "x", "ｘ" },
            { "y", "ｙ" }, { "z", "ｚ" },

            // Numbers
            { "0", "０" }, { "1", "１" }, { "2", "２" }, { "3", "３" }, { "4", "４" },
            { "5", "５" }, { "6", "６" }, { "7", "７" }, { "8", "８" }, { "9", "９" },

            // Typographical symbols and punctuation marks
            { "!", "！" }, { "\"", "＂" }, { "#", "＃" }, { "$", "＄" }, { "%", "％" }, { "&", "＆" },
            { "'", "＇" }, { "(", "（" }, { ")", "）" }, { "*", "＊" }, { "+", "＋" }, { "/", "／" },
            { ":", "：" }, { ";", "；" }, { "<", "＜" }, { "=", "＝" }, { ">", "＞" }, { "?", "？" },
            { "@", "＠" }, { "[", "［" }, { "\\", "＼" }, { "]", "］" }, { "^", "＾" }, { "{", "｛" },
            { "|", "｜" }, { "}", "｝" }, { "~", "～" }, { "ｰ", "ー" }
            // ，－ ．＿｀｟｡｢｣､･￠￡
            #pragma warning restore format
        };

        private static readonly Dictionary<string, string> s_compositeHalfWidthKatakanaToFullWidthHiraganaDict = new()
        {
            #pragma warning disable format
            { "ｶﾞ", "が" }, { "ｷﾞ", "ぎ" }, { "ｸﾞ", "ぐ" }, { "ｹﾞ", "げ" }, { "ｺﾞ", "ご" },
            { "ｻﾞ", "ざ" }, { "ｼﾞ", "じ" }, { "ｽﾞ", "ず" }, { "ｾﾞ", "ぜ" }, { "ｿﾞ", "ぞ" },
            { "ﾀﾞ", "だ" }, { "ﾁﾞ", "ぢ" }, { "ﾂﾞ", "づ" }, { "ﾃﾞ", "で" }, { "ﾄﾞ", "ど" },
            { "ﾊﾞ", "ば" }, { "ﾋﾞ", "び" }, { "ﾌﾞ", "ぶ" }, { "ﾍﾞ", "べ" }, { "ﾎﾞ", "ぼ" },
            { "ﾊﾟ", "ぱ" }, { "ﾋﾟ", "ぴ" }, { "ﾌﾟ", "ぷ" }, { "ﾍﾟ", "ぺ" }, { "ﾎﾟ", "ぽ" },
            { "ｳﾞ", "ゔ" }
            #pragma warning restore format
        };

        public static string KatakanaToHiraganaConverter(string text)
        {
            StringBuilder textInHiragana = new();
            List<string> unicodeCharacters = text.UnicodeIterator().ToList();
            int listLength = unicodeCharacters.Count;
            for (int i = 0; i < listLength; i++)
            {
                if (listLength > i + 1
                    && s_compositeHalfWidthKatakanaToFullWidthHiraganaDict.TryGetValue(
                        unicodeCharacters[i] + unicodeCharacters[i + 1], out string compositeStr))
                {
                    textInHiragana.Append(compositeStr);
                    ++i;
                }
                else if (s_katakanaToHiraganaDict.TryGetValue(unicodeCharacters[i], out string hiraganaStr))
                    textInHiragana.Append(hiraganaStr);
                else if (s_halfWidthToFullWidthDict.TryGetValue(unicodeCharacters[i], out string fullWidthStr))
                    textInHiragana.Append(fullWidthStr);
                else
                    textInHiragana.Append(unicodeCharacters[i]);
            }

            return textInHiragana.ToString();
        }

        public static string HiraganaToKatakanaConverter(string text)
        {
            StringBuilder textInKatakana = new();
            foreach (string str in text.UnicodeIterator().ToList())
            {
                if (s_hiraganaToKatakanaDict.TryGetValue(str, out string hiraganaStr))
                    textInKatakana.Append(hiraganaStr);
                else
                    textInKatakana.Append(str);
            }

            return textInKatakana.ToString();
        }

        public static List<string> LongVowelMarkConverter(string text)
        {
            List<StringBuilder> stringBuilders = new();
            List<string> unicodeTextList = text.UnicodeIterator().ToList();
            stringBuilders.Add(new StringBuilder(text.Length));
            stringBuilders[0].Append(unicodeTextList[0]);
            for (int i = 1; i < unicodeTextList.Count; i++)
            {
                if (text[i] == 'ー' && s_kanaFinalVowelDict.TryGetValue(unicodeTextList[i - 1], out string vowel))
                {
                    if (vowel != "お" && vowel != "え" && vowel != "オ" && vowel != "エ")
                    {
                        for (int j = 0; j < stringBuilders.Count; j++)
                        {
                            stringBuilders[j].Append(vowel);
                        }
                    }

                    else
                    {
                        string alternativeVowel = "";

                        switch (vowel)
                        {
                            case "お":
                                alternativeVowel = "う";
                                break;
                            case "え":
                                alternativeVowel = "い";
                                break;
                            case "オ":
                                alternativeVowel = "ウ";
                                break;
                            case "エ":
                                alternativeVowel = "イ";
                                break;
                        }

                        int listSize = stringBuilders.Count;
                        for (int j = 0; j < listSize; j++)
                        {
                            stringBuilders.Add(new StringBuilder(stringBuilders[j].ToString(), text.Length));
                        }

                        listSize = stringBuilders.Count;
                        for (int j = 0; j < listSize; j++)
                        {
                            if (j < listSize / 2)
                                stringBuilders[j].Append(vowel);
                            else
                                stringBuilders[j].Append(alternativeVowel);
                        }
                    }
                }

                else
                {
                    for (int j = 0; j < stringBuilders.Count; j++)
                    {
                        stringBuilders[j].Append(text[i]);
                    }
                }
            }

            return stringBuilders.ConvertAll(sb => sb.ToString());
        }

        public static bool IsHiragana(string text)
        {
            return s_hiraganaToKatakanaDict.ContainsKey(text.UnicodeIterator().First());
        }

        public static bool IsKatakana(string text)
        {
            return s_katakanaToHiraganaDict.ContainsKey(text.UnicodeIterator().First());
        }
    }
}
