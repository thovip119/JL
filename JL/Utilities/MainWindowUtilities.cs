﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text.RegularExpressions;

namespace JL.Utilities
{
    public static class MainWindowUtilities
    {
        public static readonly List<string> Backlog = new();
        public static readonly string FakeFrequency = int.MaxValue.ToString();

        public static readonly Regex JapaneseRegex =
            new(
                @"[\u2e80-\u30ff\u31c0-\u4dbf\u4e00-\u9fff\uf900-\ufaff\ufe30-\ufe4f\uff00-\uffef]|\ud82c[\udc00-\udcff]|\ud83c[\ude00-\udeff]|\ud840[\udc00-\udfff]|[\ud841-\ud868][\udc00-\udfff]|\ud869[\udc00-\udedf]|\ud869[\udf00-\udfff]|[\ud86a-\ud879][\udc00-\udfff]|\ud87a[\udc00-\udfef]|\ud87e[\udc00-\ude1f]|\ud880[\udc00-\udfff]|[\ud881-\ud883][\udc00-\udfff]|\ud884[\udc00-\udf4f]");

        // Consider checking for \t, \r, "　", " ", ., !, ?, –, —, ―, ‒, ~, ‥, ♪, ～, ♡, ♥, ☆, ★
        private static readonly List<string> s_japanesePunctuation =
            new()
            {
                "。",
                "！",
                "？",
                "…",
                ".",
                "、",
                "「",
                "」",
                "『",
                "』",
                "（",
                "）",
                "\n"
            };

        public static int FindWordBoundary(string text, int position)
        {
            int endPosition = -1;

            for (int i = 0; i < s_japanesePunctuation.Count; i++)
            {
                int tempIndex = text.IndexOf(s_japanesePunctuation[i], position, StringComparison.Ordinal);

                if (tempIndex != -1 && (endPosition == -1 || tempIndex < endPosition))
                    endPosition = tempIndex;
            }

            if (endPosition == -1)
                endPosition = text.Length;

            return endPosition;
        }

        public static void InitializeMainWindow()
        {
            if (!File.Exists(Path.Join(Storage.ApplicationPath, "Config/dicts.json")))
                Utils.CreateDefaultDictsConfig();

            if (!File.Exists("Resources/custom_words.txt"))
                File.Create("Resources/custom_words.txt").Dispose();

            if (!File.Exists("Resources/custom_names.txt"))
                File.Create("Resources/custom_names.txt").Dispose();

            Utils.DeserializeDicts().ContinueWith(_ =>
            {
                Storage.LoadDictionaries().ContinueWith(_ =>
                    {
                        Storage.InitializePoS().ContinueWith(_ =>
                        {
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
                        }).ConfigureAwait(false);
                    }
                ).ConfigureAwait(false);
            }).ConfigureAwait(false);

            ConfigManager.ApplyPreferences();

            if (ConfigManager.CheckForJLUpdatesOnStartUp)
            {
                Utils.CheckForJLUpdates(true);
            }
        }
    }
}
