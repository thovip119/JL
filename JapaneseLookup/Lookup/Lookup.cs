﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JapaneseLookup.Abstract;
using JapaneseLookup.CustomDict;
using JapaneseLookup.Deconjugation;
using JapaneseLookup.Dicts;
using JapaneseLookup.EDICT.JMdict;
using JapaneseLookup.EDICT.JMnedict;
using JapaneseLookup.EPWING;
using JapaneseLookup.KANJIDIC;
using JapaneseLookup.Utilities;

namespace JapaneseLookup.Lookup
{
    public static class Lookup
    {
        private static DateTime _lastLookupTime;

        public static IEnumerable<string> UnicodeIterator(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return char.ConvertFromUtf32(char.ConvertToUtf32(s, i));
                if (char.IsHighSurrogate(s, i))
                    i++;
            }
        }

        public static List<Dictionary<LookupResult, List<string>>> LookupText(string text)
        {
            var preciseTimeNow = new DateTime(Stopwatch.GetTimestamp());
            if ((preciseTimeNow - _lastLookupTime).Milliseconds < ConfigManager.LookupRate) return null;
            _lastLookupTime = preciseTimeNow;

            Dictionary<string, IntermediaryResult> jMdictResults;
            var jMnedictResults = new Dictionary<string, IntermediaryResult>();
            var epwingWordResultsList = new List<Dictionary<string, IntermediaryResult>>();
            var kanjiResult = new Dictionary<string, IntermediaryResult>();
            var customWordResults = new Dictionary<string, IntermediaryResult>();
            var customNameResults = new Dictionary<string, IntermediaryResult>();

            if (ConfigManager.KanjiMode)
                if (ConfigManager.Dicts[DictType.Kanjidic]?.Contents.Any() ?? false)
                {
                    return KanjiResultBuilder(GetKanjidicResults(text, DictType.Kanjidic));
                }

            List<string> textInHiraganaList = new();
            List<HashSet<Form>> deconjugationResultsList = new();
            for (int i = 0; i < text.Length; i++)
            {
                string textInHiragana = Kana.KatakanaToHiraganaConverter(text[..^i]);
                textInHiraganaList.Add(textInHiragana);
                deconjugationResultsList.Add(Deconjugator.Deconjugate(textInHiragana));
            }

            jMdictResults = GetJMdictResults(text, textInHiraganaList, deconjugationResultsList, DictType.JMdict);

            Dictionary<string, List<List<string>>> jmdictWordClasses = new Dictionary<string, List<List<string>>>();
            foreach ((string key, IntermediaryResult value) in jMdictResults)
            {
                foreach (JMdictResult jmdictResult in value.ResultsList.Cast<JMdictResult>())
                {
                    if (!jmdictWordClasses.ContainsKey(key))
                    {
                        jmdictWordClasses.Add(key, jmdictResult.WordClasses);
                    }
                }
            }

            foreach ((DictType dictType, Dict dict) in ConfigManager.Dicts)
            {
                switch (dictType)
                {
                    case DictType.JMdict:
                        // handled above
                        break;
                    case DictType.JMnedict:
                        jMnedictResults = GetJMnedictResults(text, textInHiraganaList, dictType);
                        break;
                    case DictType.Kanjidic:
                        // handled above and below
                        break;
                    case DictType.UnknownEpwing:
                        epwingWordResultsList.Add(GetEpwingResults(text, textInHiraganaList,
                            deconjugationResultsList, dict.Contents, dictType, jmdictWordClasses));
                        break;
                    case DictType.Daijirin:
                        epwingWordResultsList.Add(GetDaijirinResults(text, textInHiraganaList,
                            deconjugationResultsList, dict.Contents, dictType, jmdictWordClasses));
                        break;
                    case DictType.Daijisen:
                        // TODO
                        epwingWordResultsList.Add(GetDaijirinResults(text, textInHiraganaList,
                            deconjugationResultsList, dict.Contents, dictType, jmdictWordClasses));
                        break;
                    case DictType.Koujien:
                        // TODO
                        epwingWordResultsList.Add(GetDaijirinResults(text, textInHiraganaList,
                            deconjugationResultsList, dict.Contents, dictType, jmdictWordClasses));
                        break;
                    case DictType.Meikyou:
                        // TODO
                        epwingWordResultsList.Add(GetDaijirinResults(text, textInHiraganaList,
                            deconjugationResultsList, dict.Contents, dictType, jmdictWordClasses));
                        break;
                    case DictType.CustomWordDictionary:
                        customWordResults = GetCustomWordResults(text, textInHiraganaList,
                            deconjugationResultsList, dictType);
                        break;
                    case DictType.CustomNameDictionary:
                        customNameResults = GetCustomNameResults(text, textInHiraganaList, dictType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!jMdictResults.Any() && !jMnedictResults.Any() &&
                (!epwingWordResultsList.Any() || !epwingWordResultsList.First().Any()))
            {
                if (ConfigManager.Dicts[DictType.Kanjidic]?.Contents.Any() ?? false)
                {
                    kanjiResult = GetKanjidicResults(text, DictType.Kanjidic);
                }
            }

            List<Dictionary<LookupResult, List<string>>> lookupResults = new();

            if (jMdictResults.Any())
                lookupResults.AddRange(JmdictResultBuilder(jMdictResults));

            if (epwingWordResultsList.Any())
                foreach (var epwingWordResult in epwingWordResultsList)
                {
                    lookupResults.AddRange(EpwingResultBuilder(epwingWordResult));
                }

            if (jMnedictResults.Any())
                lookupResults.AddRange(JmnedictResultBuilder(jMnedictResults));

            if (kanjiResult.Any())
                lookupResults.AddRange(KanjiResultBuilder(kanjiResult));

            if (customWordResults.Any())
                lookupResults.AddRange(CustomWordResultBuilder(customWordResults));

            if (customNameResults.Any())
                lookupResults.AddRange(CustomNameResultBuilder(customNameResults));

            lookupResults = SortLookupResults(lookupResults);
            return lookupResults;
        }

        private static List<Dictionary<LookupResult, List<string>>> SortLookupResults(
            List<Dictionary<LookupResult, List<string>>> lookupResults)
        {
            return lookupResults
                .OrderByDescending(dict => dict[LookupResult.FoundForm][0].Length)
                .ThenBy(dict =>
                {
                    Enum.TryParse(dict[LookupResult.DictType][0], out DictType dictType);
                    return ConfigManager.Dicts[dictType].Priority;
                })
                .ThenBy(dict => Convert.ToInt32(dict[LookupResult.Frequency][0]))
                .ToList();
        }

        private static Dictionary<string, IntermediaryResult> GetJMdictResults(string text,
            List<string> textInHiraganaList, List<HashSet<Form>> deconjugationResultsList, DictType dictType)
        {
            var jMdictResults =
                new Dictionary<string, IntermediaryResult>();

            int succAttempt = 0;

            for (int i = 0; i < text.Length; i++)
            {
                bool tryLongVowelConversion = true;

                if (ConfigManager.Dicts[DictType.JMdict].Contents
                    .TryGetValue(textInHiraganaList[i], out var tempResult))
                {
                    jMdictResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(tempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                if (succAttempt < 3)
                {
                    foreach (Form result in deconjugationResultsList[i])
                    {
                        if (jMdictResults.ContainsKey(result.Text))
                            continue;

                        if (ConfigManager.Dicts[DictType.JMdict].Contents
                            .TryGetValue(result.Text, out var temp))
                        {
                            List<IResult> resultsList = new();

                            foreach (IResult rslt1 in temp)
                            {
                                var rslt = (JMdictResult) rslt1;
                                if (rslt.WordClasses.SelectMany(pos => pos).Intersect(result.Tags).Any())
                                {
                                    resultsList.Add(rslt);
                                }
                            }

                            if (resultsList.Any())
                            {
                                jMdictResults.Add(result.Text,
                                    new IntermediaryResult(resultsList, result.Process,
                                        text[..result.OriginalText.Length],
                                        dictType)
                                );
                                ++succAttempt;
                                tryLongVowelConversion = false;
                            }
                        }
                    }
                }

                if (tryLongVowelConversion && textInHiraganaList[i].Contains("ー") &&
                    textInHiraganaList[i][0] != 'ー')
                {
                    string textWithoutLongVowelMark = Kana.LongVowelMarkConverter(textInHiraganaList[i]);
                    if (ConfigManager.Dicts[DictType.JMdict].Contents
                        .TryGetValue(textWithoutLongVowelMark, out var tmpResult))
                    {
                        jMdictResults.Add(textInHiraganaList[i],
                            new IntermediaryResult(tmpResult, new List<string>(), text[..^i], dictType));
                    }
                }
            }

            return jMdictResults;
        }

        private static Dictionary<string, IntermediaryResult> GetJMnedictResults(string text,
            List<string> textInHiraganaList, DictType dictType)
        {
            var jMnedictResults =
                new Dictionary<string, IntermediaryResult>();

            for (int i = 0; i < text.Length; i++)
            {
                if (ConfigManager.Dicts[DictType.JMnedict].Contents
                    .TryGetValue(textInHiraganaList[i], out var tempJmnedictResult))
                {
                    jMnedictResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(tempJmnedictResult, new List<string>(), text[..^i], dictType));
                }
            }

            return jMnedictResults;
        }

        private static Dictionary<string, IntermediaryResult> GetKanjidicResults(string text, DictType dictType)
        {
            var kanjiResult =
                new Dictionary<string, IntermediaryResult>();

            if (ConfigManager.Dicts[DictType.Kanjidic].Contents.TryGetValue(
                text.UnicodeIterator().DefaultIfEmpty(string.Empty).First(), out List<IResult> kResult))
            {
                kanjiResult.Add(text.UnicodeIterator().First(),
                    new IntermediaryResult(kResult, new List<string>(), text.UnicodeIterator().First(),
                        dictType));
            }

            return kanjiResult;
        }

        private static Dictionary<string, IntermediaryResult> GetDaijirinResults(string text,
            List<string> textInHiraganaList, List<HashSet<Form>> deconjugationResultsList,
            Dictionary<string, List<IResult>> dict, DictType dictType,
            Dictionary<string, List<List<string>>> jmdictWordClasses)
        {
            var daijirinResults =
                new Dictionary<string, IntermediaryResult>();

            int succAttempt = 0;
            for (int i = 0; i < text.Length; i++)
            {
                bool tryLongVowelConversion = true;

                if (dict.TryGetValue(textInHiraganaList[i], out var hiraganaTempResult))
                {
                    daijirinResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(hiraganaTempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                //todo
                if (dict.TryGetValue(text, out var textTempResult))
                {
                    daijirinResults.TryAdd(text,
                        new IntermediaryResult(textTempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                if (succAttempt < 3)
                {
                    foreach (Form result in deconjugationResultsList[i])
                    {
                        if (daijirinResults.ContainsKey(result.Text))
                            continue;

                        if (dict.TryGetValue(result.Text, out var temp))
                        {
                            List<IResult> resultsList = new();

                            foreach (EpwingResult rslt in temp.Cast<EpwingResult>())
                            {
                                if (jmdictWordClasses.ContainsKey(result.Text))
                                {
                                    if (rslt.WordClasses.Union(jmdictWordClasses[result.Text]).SelectMany(pos => pos)
                                        .Intersect(result.Tags).Any())
                                    {
                                        resultsList.Add(rslt);
                                    }
                                }
                                // TODO
                                else
                                {
                                    resultsList.Add(rslt);
                                }
                            }

                            if (resultsList.Any())
                            {
                                daijirinResults.Add(result.Text,
                                    new IntermediaryResult(resultsList, result.Process,
                                        text[..result.OriginalText.Length],
                                        dictType));
                                ++succAttempt;
                                tryLongVowelConversion = false;
                            }
                        }
                    }
                }

                if (tryLongVowelConversion && textInHiraganaList[i].Contains("ー") && textInHiraganaList[i][0] != 'ー')
                {
                    string textWithoutLongVowelMark = Kana.LongVowelMarkConverter(textInHiraganaList[i]);
                    if (dict.TryGetValue(textWithoutLongVowelMark, out var tmpResult))
                    {
                        daijirinResults.Add(textInHiraganaList[i],
                            new IntermediaryResult(tmpResult, new List<string>(), text[..^i], dictType));
                    }
                }
            }

            return daijirinResults;
        }

        private static Dictionary<string, IntermediaryResult> GetEpwingResults(string text,
            List<string> textInHiraganaList, List<HashSet<Form>> deconjugationResultsList,
            Dictionary<string, List<IResult>> dict, DictType dictType,
            Dictionary<string, List<List<string>>> jmdictWordClasses)
        {
            var epwingResults =
                new Dictionary<string, IntermediaryResult>();

            int succAttempt = 0;
            for (int i = 0; i < text.Length; i++)
            {
                bool tryLongVowelConversion = true;

                if (dict.TryGetValue(textInHiraganaList[i], out var hiraganaTempResult))
                {
                    epwingResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(hiraganaTempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                //todo
                if (dict.TryGetValue(text, out var textTempResult))
                {
                    epwingResults.TryAdd(text,
                        new IntermediaryResult(textTempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                if (succAttempt < 3)
                {
                    foreach (Form result in deconjugationResultsList[i])
                    {
                        if (epwingResults.ContainsKey(result.Text))
                            continue;

                        if (dict.TryGetValue(result.Text, out var temp))
                        {
                            List<IResult> resultsList = new();

                            foreach (EpwingResult rslt in temp.Cast<EpwingResult>())
                            {
                                if (jmdictWordClasses.ContainsKey(result.Text))
                                {
                                    if (rslt.WordClasses.Union(jmdictWordClasses[result.Text]).SelectMany(pos => pos)
                                        .Intersect(result.Tags).Any())
                                    {
                                        resultsList.Add(rslt);
                                    }
                                }
                                // TODO
                                else
                                {
                                    resultsList.Add(rslt);
                                }
                            }

                            if (resultsList.Any())
                            {
                                epwingResults.Add(result.Text,
                                    new IntermediaryResult(resultsList, result.Process,
                                        text[..result.OriginalText.Length],
                                        dictType));
                                ++succAttempt;
                                tryLongVowelConversion = false;
                            }
                        }
                    }
                }

                if (tryLongVowelConversion && textInHiraganaList[i].Contains("ー") &&
                    textInHiraganaList[i][0] != 'ー')
                {
                    string textWithoutLongVowelMark = Kana.LongVowelMarkConverter(textInHiraganaList[i]);
                    if (dict.TryGetValue(textWithoutLongVowelMark, out var tmpResult))
                    {
                        epwingResults.Add(textInHiraganaList[i],
                            new IntermediaryResult(tmpResult, new List<string>(), text[..^i], dictType));
                    }
                }
            }

            return epwingResults;
        }

        private static Dictionary<string, IntermediaryResult> GetCustomWordResults(string text,
            List<string> textInHiraganaList, List<HashSet<Form>> deconjugationResultsList, DictType dictType)
        {
            var customWordResults =
                new Dictionary<string, IntermediaryResult>();

            int succAttempt = 0;

            for (int i = 0; i < text.Length; i++)
            {
                bool tryLongVowelConversion = true;

                if (ConfigManager.Dicts[DictType.CustomWordDictionary].Contents
                    .TryGetValue(textInHiraganaList[i], out var tempResult))
                {
                    customWordResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(tempResult, new List<string>(), text[..^i], dictType));
                    tryLongVowelConversion = false;
                }

                if (succAttempt < 3)
                {
                    foreach (Form result in deconjugationResultsList[i])
                    {
                        if (customWordResults.ContainsKey(result.Text))
                            continue;

                        if (ConfigManager.Dicts[DictType.CustomWordDictionary].Contents
                            .TryGetValue(result.Text, out var temp))
                        {
                            List<IResult> resultsList = new();

                            foreach (IResult rslt1 in temp)
                            {
                                var rslt = (CustomWordEntry) rslt1;
                                if (rslt.WordClasses.Intersect(result.Tags).Any())
                                {
                                    resultsList.Add(rslt);
                                }
                            }

                            if (resultsList.Any())
                            {
                                customWordResults.Add(result.Text,
                                    new IntermediaryResult(resultsList, result.Process,
                                        text[..result.OriginalText.Length],
                                        dictType));
                                ++succAttempt;
                                tryLongVowelConversion = false;
                            }
                        }
                    }
                }

                if (tryLongVowelConversion && textInHiraganaList[i].Contains("ー") && textInHiraganaList[i][0] != 'ー')
                {
                    string textWithoutLongVowelMark = Kana.LongVowelMarkConverter(textInHiraganaList[i]);
                    if (ConfigManager.Dicts[DictType.CustomWordDictionary].Contents
                        .TryGetValue(textWithoutLongVowelMark, out var tmpResult))
                    {
                        customWordResults.Add(textInHiraganaList[i],
                            new IntermediaryResult(tmpResult, new List<string>(), text[..^i], dictType));
                    }
                }
            }

            return customWordResults;
        }

        private static Dictionary<string, IntermediaryResult> GetCustomNameResults(string text,
            List<string> textInHiraganaList, DictType dictType)
        {
            var customNameResults =
                new Dictionary<string, IntermediaryResult>();

            for (int i = 0; i < text.Length; i++)
            {
                if (ConfigManager.Dicts[DictType.CustomNameDictionary].Contents
                    .TryGetValue(textInHiraganaList[i], out var tempNameResult))
                {
                    customNameResults.TryAdd(textInHiraganaList[i],
                        new IntermediaryResult(tempNameResult, new List<string>(), text[..^i], dictType));
                }
            }

            return customNameResults;
        }

        private static List<Dictionary<LookupResult, List<string>>> JmdictResultBuilder(
            Dictionary<string, IntermediaryResult> jmdictResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();

            foreach (var wordResult in jmdictResults)
            {
                foreach (IResult iResult in wordResult.Value.ResultsList)
                {
                    var jMDictResult = (JMdictResult) iResult;
                    var result = new Dictionary<LookupResult, List<string>>();

                    var foundSpelling = new List<string> { jMDictResult.PrimarySpelling };

                    var kanaSpellings = jMDictResult.KanaSpellings ?? new List<string>();

                    var readings = jMDictResult.Readings.ToList();
                    var foundForm = new List<string> { wordResult.Value.FoundForm };
                    var edictID = new List<string> { jMDictResult.Id };

                    List<string> alternativeSpellings;
                    if (jMDictResult.AlternativeSpellings != null)
                        alternativeSpellings = jMDictResult.AlternativeSpellings.ToList();
                    else
                        alternativeSpellings = new List<string>();
                    var process = wordResult.Value.ProcessList;

                    List<string> frequency;
                    if (jMDictResult.FrequencyDict != null)
                    {
                        jMDictResult.FrequencyDict.TryGetValue(ConfigManager.FrequencyList, out int freq);
                        if (freq == 0)
                            frequency = new List<string> { MainWindowUtilities.FakeFrequency };
                        else
                            frequency = new List<string> { freq.ToString() };
                    }

                    else frequency = new List<string> { MainWindowUtilities.FakeFrequency };

                    var dictType = new List<string> { wordResult.Value.DictType.ToString() };

                    var definitions = new List<string> { BuildJmdictDefinition(jMDictResult) };

                    var pOrthographyInfoList = jMDictResult.POrthographyInfoList ?? new List<string>();

                    var rList = jMDictResult.ROrthographyInfoList ?? new List<List<string>>();
                    var aList = jMDictResult.AOrthographyInfoList ?? new List<List<string>>();
                    var rOrthographyInfoList = new List<string>();
                    var aOrthographyInfoList = new List<string>();

                    foreach (var list in rList)
                    {
                        var final = "";
                        foreach (string str in list)
                        {
                            final += str + ", ";
                        }

                        final = final.TrimEnd(", ".ToCharArray());

                        rOrthographyInfoList.Add(final);
                    }

                    foreach (var list in aList)
                    {
                        var final = "";
                        foreach (string str in list)
                        {
                            final += str + ", ";
                        }

                        final = final.TrimEnd(", ".ToCharArray());

                        aOrthographyInfoList.Add(final);
                    }

                    result.Add(LookupResult.FoundSpelling, foundSpelling);
                    result.Add(LookupResult.KanaSpellings, kanaSpellings);
                    result.Add(LookupResult.Readings, readings);
                    result.Add(LookupResult.Definitions, definitions);
                    result.Add(LookupResult.FoundForm, foundForm);
                    result.Add(LookupResult.EdictID, edictID);
                    result.Add(LookupResult.AlternativeSpellings, alternativeSpellings);
                    result.Add(LookupResult.Process, process);
                    result.Add(LookupResult.Frequency, frequency);
                    result.Add(LookupResult.POrthographyInfoList, pOrthographyInfoList);
                    result.Add(LookupResult.ROrthographyInfoList, rOrthographyInfoList);
                    result.Add(LookupResult.AOrthographyInfoList, aOrthographyInfoList);
                    result.Add(LookupResult.DictType, dictType);

                    results.Add(result);
                }
            }

            return results;
        }

        private static List<Dictionary<LookupResult, List<string>>> JmnedictResultBuilder(
            Dictionary<string, IntermediaryResult> jmnedictResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();

            foreach (var nameResult in jmnedictResults)
            {
                foreach (IResult iResult in nameResult.Value.ResultsList)
                {
                    var jMnedictResult = (JMnedictResult) iResult;
                    var result = new Dictionary<LookupResult, List<string>>();

                    var foundSpelling = new List<string> { jMnedictResult.PrimarySpelling };

                    var readings = jMnedictResult.Readings != null
                        ? jMnedictResult.Readings.ToList()
                        : new List<string>();

                    var foundForm = new List<string> { nameResult.Value.FoundForm };

                    var edictID = new List<string> { jMnedictResult.Id };

                    var dictType = new List<string> { nameResult.Value.DictType.ToString() };

                    var alternativeSpellings = jMnedictResult.AlternativeSpellings ?? new List<string>();

                    var definitions = new List<string> { BuildJmnedictDefinition(jMnedictResult) };

                    result.Add(LookupResult.EdictID, edictID);
                    result.Add(LookupResult.FoundSpelling, foundSpelling);
                    result.Add(LookupResult.AlternativeSpellings, alternativeSpellings);
                    result.Add(LookupResult.Readings, readings);
                    result.Add(LookupResult.Definitions, definitions);

                    result.Add(LookupResult.FoundForm, foundForm);
                    result.Add(LookupResult.Frequency, new List<string> { MainWindowUtilities.FakeFrequency });
                    result.Add(LookupResult.DictType, dictType);

                    results.Add(result);
                }
            }

            return results;
        }

        private static List<Dictionary<LookupResult, List<string>>> KanjiResultBuilder(
            Dictionary<string, IntermediaryResult> kanjiResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();
            var result = new Dictionary<LookupResult, List<string>>();

            if (!kanjiResults.Any())
                return results;

            var iResult = kanjiResults.First().Value.ResultsList;
            KanjiResult kanjiResult = (KanjiResult) iResult.First();

            var dictType = new List<string> { kanjiResults.First().Value.DictType.ToString() };

            result.Add(LookupResult.FoundSpelling, new List<string> { kanjiResults.First().Key });
            result.Add(LookupResult.Definitions, kanjiResult.Meanings);
            result.Add(LookupResult.OnReadings, kanjiResult.OnReadings);
            result.Add(LookupResult.KunReadings, kanjiResult.KunReadings);
            result.Add(LookupResult.Nanori, kanjiResult.Nanori);
            result.Add(LookupResult.StrokeCount, new List<string> { kanjiResult.StrokeCount.ToString() });
            result.Add(LookupResult.Grade, new List<string> { kanjiResult.Grade.ToString() });
            result.Add(LookupResult.Composition, new List<string> { kanjiResult.Composition });
            result.Add(LookupResult.Frequency, new List<string> { kanjiResult.Frequency.ToString() });

            var foundForm = new List<string> { kanjiResults.First().Value.FoundForm };
            result.Add(LookupResult.FoundForm, foundForm);
            result.Add(LookupResult.DictType, dictType);

            results.Add(result);
            return results;
        }

        private static List<Dictionary<LookupResult, List<string>>> EpwingResultBuilder(
            Dictionary<string, IntermediaryResult> epwingResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();

            foreach (var wordResult in epwingResults)
            {
                foreach (IResult iResult in wordResult.Value.ResultsList)
                {
                    var epwingResult = (EpwingResult) iResult;
                    var result = new Dictionary<LookupResult, List<string>>();

                    var foundSpelling = new List<string> { epwingResult.PrimarySpelling };
                    var readings = epwingResult.Readings.ToList();
                    var foundForm = new List<string> { wordResult.Value.FoundForm };
                    var process = wordResult.Value.ProcessList;
                    List<string> frequency;
                    // TODO
                    // if (jMDictResult.FrequencyDict != null)
                    // {
                    //     jMDictResult.FrequencyDict.TryGetValue(ConfigManager.FrequencyList, out var freq);
                    //     frequency = new List<string> { freq.ToString() };
                    // }
                    //
                    // else frequency = new List<string> { FakeFrequency };
                    frequency = new List<string> { MainWindowUtilities.FakeFrequency };
                    var dictType = new List<string> { wordResult.Value.DictType.ToString() };

                    var definitions = new List<string> { BuildEpwingDefinition(epwingResult) };

                    // TODO: Should be filtered while loading the dict ideally (+ it's daijirin specific)
                    if (definitions.First().Contains("→英和"))
                        continue;

                    result.Add(LookupResult.FoundSpelling, foundSpelling);
                    result.Add(LookupResult.Readings, readings);
                    result.Add(LookupResult.Definitions, definitions);
                    result.Add(LookupResult.FoundForm, foundForm);
                    result.Add(LookupResult.Process, process);
                    result.Add(LookupResult.Frequency, frequency);
                    result.Add(LookupResult.DictType, dictType);

                    results.Add(result);
                }
            }

            return results;
        }

        private static List<Dictionary<LookupResult, List<string>>> CustomWordResultBuilder(
            Dictionary<string, IntermediaryResult> customWordResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();

            foreach (var wordResult in customWordResults)
            {
                foreach (IResult iResult in wordResult.Value.ResultsList)
                {
                    var customWordDictResults = (CustomWordEntry) iResult;
                    var result = new Dictionary<LookupResult, List<string>>();

                    var foundSpelling = new List<string> { customWordDictResults.PrimarySpelling };

                    var readings = customWordDictResults.Readings.ToList();
                    var foundForm = new List<string> { wordResult.Value.FoundForm };

                    List<string> alternativeSpellings;
                    if (customWordDictResults.AlternativeSpellings != null)
                        alternativeSpellings = customWordDictResults.AlternativeSpellings.ToList();
                    else
                        alternativeSpellings = new();
                    var process = wordResult.Value.ProcessList;

                    List<string> frequency = new() { MainWindowUtilities.FakeFrequency };

                    var dictType = new List<string> { wordResult.Value.DictType.ToString() };

                    var definitions = new List<string> { BuildCustomWordDefinition(customWordDictResults) };

                    result.Add(LookupResult.FoundSpelling, foundSpelling);
                    result.Add(LookupResult.Readings, readings);
                    result.Add(LookupResult.Definitions, definitions);
                    result.Add(LookupResult.FoundForm, foundForm);
                    result.Add(LookupResult.AlternativeSpellings, alternativeSpellings);
                    result.Add(LookupResult.Process, process);
                    result.Add(LookupResult.Frequency, frequency);
                    result.Add(LookupResult.DictType, dictType);

                    results.Add(result);
                }
            }

            return results;
        }

        private static List<Dictionary<LookupResult, List<string>>> CustomNameResultBuilder(
            Dictionary<string, IntermediaryResult> customNameResults)
        {
            var results = new List<Dictionary<LookupResult, List<string>>>();

            foreach (var customNameResult in customNameResults)
            {
                foreach (IResult iResult in customNameResult.Value.ResultsList)
                {
                    var customNameDictResult = (CustomNameEntry) iResult;
                    var result = new Dictionary<LookupResult, List<string>>();

                    var foundSpelling = new List<string> { customNameDictResult.PrimarySpelling };

                    var readings = new List<string> { customNameDictResult.Reading };

                    var foundForm = new List<string> { customNameResult.Value.FoundForm };

                    var dictType = new List<string> { customNameResult.Value.DictType.ToString() };

                    var definitions = new List<string> { BuildCustomNameDefinition(customNameDictResult) };

                    result.Add(LookupResult.FoundSpelling, foundSpelling);
                    result.Add(LookupResult.Readings, readings);
                    result.Add(LookupResult.Definitions, definitions);

                    result.Add(LookupResult.FoundForm, foundForm);
                    result.Add(LookupResult.Frequency, new List<string> { MainWindowUtilities.FakeFrequency });
                    result.Add(LookupResult.DictType, dictType);

                    results.Add(result);
                }
            }

            return results;
        }

        private static string BuildJmdictDefinition(JMdictResult jMDictResult)
        {
            string separator = ConfigManager.NewlineBetweenDefinitions ? "\n" : "";
            int count = 1;
            var defResult = new StringBuilder();
            for (int i = 0; i < jMDictResult.Definitions.Count; i++)
            {
                if (jMDictResult.WordClasses.Any() && jMDictResult.WordClasses[i].Any())
                {
                    defResult.Append('(');
                    defResult.Append(string.Join(", ", jMDictResult.WordClasses[i]));
                    defResult.Append(") ");
                }

                if (jMDictResult.Definitions.Any())
                {
                    defResult.Append($"({count}) ");

                    if (jMDictResult.SpellingInfo.Any() && jMDictResult.SpellingInfo[i] != null)
                    {
                        defResult.Append('(');
                        defResult.Append(jMDictResult.SpellingInfo[i]);
                        defResult.Append(") ");
                    }

                    if (jMDictResult.MiscList.Any() && jMDictResult.MiscList[i].Any())
                    {
                        defResult.Append("(");
                        defResult.Append(string.Join(", ", jMDictResult.MiscList[i]));
                        defResult.Append(") ");
                    }

                    defResult.Append(string.Join("; ", jMDictResult.Definitions[i]) + " ");

                    if (jMDictResult.RRestrictions != null && jMDictResult.RRestrictions[i].Any()
                        || jMDictResult.KRestrictions != null && jMDictResult.KRestrictions[i].Any())
                    {
                        defResult.Append("(only applies to ");

                        if (jMDictResult.KRestrictions != null && jMDictResult.KRestrictions[i].Any())
                            defResult.Append(string.Join("; ", jMDictResult.KRestrictions[i]));

                        if (jMDictResult.RRestrictions != null && jMDictResult.RRestrictions[i].Any())
                            defResult.Append(string.Join("; ", jMDictResult.RRestrictions[i]));

                        defResult.Append(") ");
                    }

                    defResult.Append(separator);

                    ++count;
                }
            }

            return defResult.ToString().Trim('\n');
        }

        private static string BuildJmnedictDefinition(JMnedictResult jMDictResult)
        {
            int count = 1;
            var defResult = new StringBuilder();

            if (jMDictResult.NameTypes != null &&
                (jMDictResult.NameTypes.Count > 1 || !jMDictResult.NameTypes.Contains("unclass")))
            {
                foreach (string nameType in jMDictResult.NameTypes)
                {
                    defResult.Append('(');
                    defResult.Append(nameType);
                    defResult.Append(") ");
                }
            }

            for (int i = 0; i < jMDictResult.Definitions.Count; i++)
            {
                if (jMDictResult.Definitions.Any())
                {
                    if (jMDictResult.Definitions.Count > 0)
                        defResult.Append($"({count}) ");

                    defResult.Append($"{string.Join("; ", jMDictResult.Definitions[i])} ");
                    ++count;
                }
            }

            return defResult.ToString();
        }

        private static string BuildEpwingDefinition(EpwingResult jMDictResult)
        {
            var defResult = new StringBuilder();
            for (int i = 0; i < jMDictResult.Definitions.Count; i++)
            {
                if (jMDictResult.Definitions.Any())
                {
                    // todo
                    // var separator = ConfigManager.NewlineBetweenDefinitions ? "\n" : "; ";
                    const string separator = "\n";
                    defResult.Append(string.Join(separator, jMDictResult.Definitions[i]));
                }
            }

            return defResult.ToString().Trim('\n');
        }

        private static string BuildCustomWordDefinition(CustomWordEntry customWordResult)
        {
            string separator = ConfigManager.NewlineBetweenDefinitions ? "\n" : "";
            int count = 1;
            var defResult = new StringBuilder();

            if (customWordResult.WordClasses.Any())
            {
                string tempWordClass;
                if (customWordResult.WordClasses.Contains("adj-i"))
                    tempWordClass = "adjective";
                else if (customWordResult.WordClasses.Contains("v1"))
                    tempWordClass = "verb";
                else if (customWordResult.WordClasses.Contains("noun"))
                    tempWordClass = "noun";
                else
                    tempWordClass = "other";

                defResult.Append($"({tempWordClass}) ");
            }

            for (int i = 0; i < customWordResult.Definitions.Count; i++)
            {
                if (customWordResult.Definitions.Any())
                {
                    defResult.Append($"({count}) ");
                    defResult.Append(string.Join("; ", customWordResult.Definitions[i]) + " ");
                    defResult.Append(separator);

                    ++count;
                }
            }

            return defResult.ToString().Trim('\n');
        }

        private static string BuildCustomNameDefinition(CustomNameEntry customNameDictResult)
        {
            string defResult = $"({customNameDictResult.NameType}) {customNameDictResult.Reading}";

            return defResult;
        }
    }
}