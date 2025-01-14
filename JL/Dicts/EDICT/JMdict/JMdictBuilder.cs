﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JL.Dicts.EDICT.JMdict
{
    internal static class JMdictBuilder
    {
        public static void BuildDictionary(JMdictEntry entry, Dictionary<string, List<IResult>> jMdictDictionary)
        {
            // entry (k_ele*, r_ele+, sense+)
            // k_ele (keb, ke_inf*, ke_pri*)
            // r_ele (reb, re_restr*, re_inf*, re_pri*)
            // sense (stagk*, stagr*, pos*, xref*, ant*, field*, misc*, s_inf*, dial*, gloss*)

            Dictionary<string, JMdictResult> resultList = new();
            List<string> alternativeSpellings;

            int kEleListCount = entry.KEleList.Count;
            for (int i = 0; i < kEleListCount; i++)
            {
                KEle kEle = entry.KEleList[i];

                JMdictResult result = new();
                string key = kEle.Keb;

                result.PrimarySpelling = key;

                result.POrthographyInfoList = kEle.KeInfList;
                //result.PriorityList = kEle.KePriList;

                int lREleListCount = entry.REleList.Count;
                for (int j = 0; j < lREleListCount; j++)
                {
                    REle rEle = entry.REleList[j];

                    if (!rEle.ReRestrList.Any() || rEle.ReRestrList.Contains(key))
                    {
                        result.Readings.Add(rEle.Reb);
                        result.ROrthographyInfoList.Add(rEle.ReInfList);
                    }
                }

                int senseListCount = entry.SenseList.Count;
                for (int j = 0; j < senseListCount; j++)
                {
                    Sense sense = entry.SenseList[j];

                    if ((!sense.StagKList.Any() && !sense.StagRList.Any())
                        || sense.StagKList.Contains(key)
                        || sense.StagRList.Intersect(result.Readings).Any())
                    {
                        result.Definitions.Add(sense.GlossList);
                        result.RRestrictions.Add(sense.StagRList);
                        result.KRestrictions.Add(sense.StagKList);
                        result.WordClasses.Add(sense.PosList);
                        result.TypeList.Add(sense.FieldList);
                        result.MiscList.Add(sense.MiscList);
                        result.Dialects.AddRange(sense.DialList);
                        result.SpellingInfo.Add(sense.SInf);
                        // result.RelatedTerms.AddRange(sense.XRefList);
                        // result.Antonyms.AddRange(sense.AntList);
                    }
                }

                resultList.Add(key, result);
            }

            alternativeSpellings = resultList.Keys.ToList();

            foreach ((string key, JMdictResult result) in resultList)
            {
                int alternativeSpellingsCount = alternativeSpellings.Count;
                for (int i = 0; i < alternativeSpellingsCount; i++)
                {
                    string spelling = alternativeSpellings[i];

                    if (key != spelling)
                    {
                        result.AlternativeSpellings.Add(spelling);

                        resultList.TryGetValue(spelling, out JMdictResult tempResult);
                        Debug.Assert(tempResult != null, nameof(tempResult) + " != null");

                        result.AOrthographyInfoList.Add(tempResult.POrthographyInfoList);
                    }
                }
            }
            int rEleListCount = entry.REleList.Count;
            for (int i = 0; i < rEleListCount; i++)
            {
                REle rEle = entry.REleList[i];

                string key = Kana.KatakanaToHiraganaConverter(rEle.Reb);

                if (resultList.ContainsKey(key))
                {
                    continue;
                }

                JMdictResult result = new();

                //result.KanaSpellings.Add(rEle.Reb);

                if (rEle.ReRestrList.Any())
                    result.AlternativeSpellings = rEle.ReRestrList;
                else
                    result.AlternativeSpellings = new List<string>(alternativeSpellings);

                if (result.AlternativeSpellings.Any())
                {
                    result.PrimarySpelling = result.AlternativeSpellings[0];

                    result.AlternativeSpellings.RemoveAt(0);

                    if (resultList.TryGetValue(result.PrimarySpelling, out JMdictResult mainEntry))
                    {
                        result.Readings = mainEntry.Readings;
                        result.AOrthographyInfoList = mainEntry.AOrthographyInfoList;
                        result.ROrthographyInfoList = mainEntry.ROrthographyInfoList;
                    }
                }

                else
                    result.PrimarySpelling = rEle.Reb;

                int senseListCount = entry.SenseList.Count;
                for (int j = 0; j < senseListCount; j++)
                {
                    Sense sense = entry.SenseList[j];

                    if ((!sense.StagKList.Any() && !sense.StagRList.Any())
                        || sense.StagRList.Contains(rEle.Reb)
                        || sense.StagKList.Contains(result.PrimarySpelling)
                        || sense.StagKList.Intersect(result.AlternativeSpellings).Any())
                    {
                        result.Definitions.Add(sense.GlossList);
                        result.RRestrictions.Add(sense.StagRList);
                        result.KRestrictions.Add(sense.StagKList);
                        result.WordClasses.Add(sense.PosList);
                        result.TypeList.Add(sense.FieldList);
                        result.MiscList.Add(sense.MiscList);
                        result.Dialects.AddRange(sense.DialList);
                        result.SpellingInfo.Add(sense.SInf);
                        // result.RelatedTerms.AddRange(sense.XRefList);
                        // result.Antonyms.AddRange(sense.AntList);
                    }
                }

                resultList.Add(key, result);
            }

            //if(!alternativeSpellings.Any())
            //{
            //    foreach (KeyValuePair<string, EdictResult> item in resultList)
            //    {
            //        foreach (string spelling in resultList.Keys)
            //        {
            //            if (item.Key != spelling)
            //            {
            //                item.Value.AlternativeSpellings.Add(spelling);
            //            }
            //        }
            //    }
            //}

            foreach (KeyValuePair<string, JMdictResult> rl in resultList)
            {
                if (!rl.Value.Readings.Any())
                    rl.Value.Readings = null;

                if (!rl.Value.AlternativeSpellings.Any())
                    rl.Value.AlternativeSpellings = null;

                if (!rl.Value.Definitions.Any())
                    rl.Value.Definitions = null;

                if (!rl.Value.RRestrictions.Any())
                    rl.Value.RRestrictions = null;

                if (!rl.Value.KRestrictions.Any())
                    rl.Value.KRestrictions = null;

                if (!rl.Value.Dialects.Any())
                    rl.Value.Dialects = null;

                //if (!rl.Value.KanaSpellings.Any())
                //    rl.Value.KanaSpellings = null;

                if (!rl.Value.MiscList.Any())
                    rl.Value.MiscList = null;

                if (!rl.Value.POrthographyInfoList.Any())
                    rl.Value.POrthographyInfoList = null;

                if (!rl.Value.AOrthographyInfoList.Any())
                    rl.Value.AOrthographyInfoList = null;

                if (!rl.Value.ROrthographyInfoList.Any())
                    rl.Value.ROrthographyInfoList = null;

                if (!rl.Value.SpellingInfo.Any())
                    rl.Value.SpellingInfo = null;

                if (!rl.Value.TypeList.Any())
                    rl.Value.TypeList = null;

                if (!rl.Value.WordClasses.Any())
                    rl.Value.WordClasses = null;

                rl.Value.Id = entry.Id;
                string key = Kana.KatakanaToHiraganaConverter(rl.Key);

                if (jMdictDictionary.TryGetValue(key, out List<IResult> tempResultList))
                    tempResultList.Add(rl.Value);
                else
                    tempResultList = new() { rl.Value };

                jMdictDictionary[key] = tempResultList;
            }
        }
    }
}
