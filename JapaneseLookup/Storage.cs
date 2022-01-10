﻿using JapaneseLookup.CustomDict;
using JapaneseLookup.Dicts;
using JapaneseLookup.EDICT;
using JapaneseLookup.EDICT.JMdict;
using JapaneseLookup.EDICT.JMnedict;
using JapaneseLookup.EDICT.KANJIDIC;
using JapaneseLookup.EPWING;
using JapaneseLookup.Frequency;
using JapaneseLookup.PoS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace JapaneseLookup
{
    public class Storage
    {
        public static Dictionary<string, List<JmdictWc>> WcDict { get; set; } = new();
        public static Dictionary<string, Dictionary<string, List<FrequencyEntry>>> FreqDicts { get; set; } = new();

        public static readonly Dictionary<DictType, Dict> Dicts = new();

        public static async Task LoadDictionaries()
        {
            ConfigManager.Ready = false;

            List<Task> tasks = new();
            bool dictRemoved = false;

            foreach ((DictType _, Dict dict) in Dicts.ToList())
            {
                switch (dict.Type)
                {
                    case DictType.JMdict:
                        // initial jmdict load
                        if (dict.Active && !Dicts[DictType.JMdict].Contents.Any())
                        {
                            Task jMDictTask = Task.Run(async () =>
                                await JMdictLoader.Load(dict.Path).ConfigureAwait(false));

                            tasks.Add(jMDictTask);
                        }

                        else if (!dict.Active && Dicts[DictType.JMdict].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.JMnedict:
                        // JMnedict
                        if (dict.Active && !Dicts[DictType.JMnedict].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () => await JMnedictLoader.Load(dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.JMnedict].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Kanjidic:
                        // KANJIDIC
                        if (dict.Active && !Dicts[DictType.Kanjidic].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await KanjiInfoLoader.Load(dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Kanjidic].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Kenkyuusha:
                        if (dict.Active && !Dicts[DictType.Kenkyuusha].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Kenkyuusha].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Daijirin:
                        if (dict.Active && !Dicts[DictType.Daijirin].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Daijirin].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Daijisen:
                        if (dict.Active && !Dicts[DictType.Daijisen].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Daijisen].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Koujien:
                        if (dict.Active && !Dicts[DictType.Koujien].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Koujien].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.Meikyou:
                        if (dict.Active && !Dicts[DictType.Meikyou].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Meikyou].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;

                    case DictType.Gakken:
                        if (dict.Active && !Dicts[DictType.Gakken].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Gakken].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;

                    case DictType.Kotowaza:
                        if (dict.Active && !Dicts[DictType.Kotowaza].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await EpwingJsonLoader.Load(dict.Type, dict.Path).ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.Kotowaza].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;

                    case DictType.CustomWordDictionary:
                        if (dict.Active && !Dicts[DictType.CustomWordDictionary].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () => await CustomWordLoader
                                .Load(Dicts[DictType.CustomWordDictionary].Path)
                                .ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.CustomWordDictionary].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;
                    case DictType.CustomNameDictionary:
                        if (dict.Active && !Dicts[DictType.CustomNameDictionary].Contents.Any())
                        {
                            tasks.Add(Task.Run(async () =>
                                await CustomNameLoader.Load(Dicts[DictType.CustomNameDictionary].Path)
                                    .ConfigureAwait(false)));
                        }

                        else if (!dict.Active && Dicts[DictType.CustomNameDictionary].Contents.Any())
                        {
                            dict.Contents.Clear();
                            dictRemoved = true;
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (tasks.Any() || dictRemoved)
            {
                if (tasks.Any())
                {
                    await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                }

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
            }

            ConfigManager.Ready = true;
        }

        public static async Task InitializePoS()
        {
            if (!File.Exists(Path.Join(ConfigManager.ApplicationPath, "Resources/PoS.json")))
            {
                if (Dicts[DictType.JMdict].Active)
                {
                    await JmdictWcLoader.JmdictWordClassSerializer().ConfigureAwait(false);
                }

                else
                {
                    bool deleteJmdictFile = false;
                    if (!File.Exists(Path.Join(ConfigManager.ApplicationPath, Dicts[DictType.JMdict].Path)))
                    {
                        deleteJmdictFile = true;
                        await ResourceUpdater.UpdateResource(Dicts[DictType.JMdict].Path,
                            new Uri("http://ftp.edrdg.org/pub/Nihongo/JMdict_e.gz"),
                            DictType.JMdict.ToString(), false, true).ConfigureAwait(false);
                    }

                    await Task.Run(async () =>
                        await JMdictLoader.Load(Dicts[DictType.JMdict].Path).ConfigureAwait(false));
                    await JmdictWcLoader.JmdictWordClassSerializer().ConfigureAwait(false);
                    Dicts[DictType.JMdict].Contents.Clear();

                    if (deleteJmdictFile)
                        File.Delete(Path.Join(ConfigManager.ApplicationPath, Dicts[DictType.JMdict].Path));
                }
            }

            await JmdictWcLoader.Load().ConfigureAwait(false);
        }

        public static async Task LoadFrequency()
        {
            if (!FreqDicts.ContainsKey(ConfigManager.FrequencyListName))
            {
                bool callGc = false;
                Task taskNewFreqlist = null;
                if (ConfigManager.FrequencyListName != "None")
                {
                    callGc = true;
                    FreqDicts.Clear();
                    FreqDicts.Add(ConfigManager.FrequencyListName, new Dictionary<string, List<FrequencyEntry>>());

                    taskNewFreqlist = Task.Run(async () =>
                    {
                        FrequencyLoader.BuildFreqDict(await FrequencyLoader
                            .LoadJson(Path.Join(ConfigManager.ApplicationPath,
                                ConfigManager.FrequencyLists[ConfigManager.FrequencyListName]))
                            .ConfigureAwait(false));
                    });
                }

                else if (FreqDicts.Any())
                {
                    callGc = true;
                    FreqDicts.Clear();
                }

                if (callGc)
                {
                    if (taskNewFreqlist != null)
                        await taskNewFreqlist.ConfigureAwait(false);

                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
                }
            }
        }
    }
}
