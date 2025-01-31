﻿using System.Collections.Generic;

namespace JL.Deconjugation
{
    public class Form
    {
        public string Text { get; }
        public string OriginalText { get; }
        public List<string> Tags { get; }
        public HashSet<string> Seentext { get; }
        public List<string> Process { get; }
        public Form(string text, string originalText, List<string> tags, HashSet<string> seentext,
            List<string> process)
        {
            Text = text;
            OriginalText = originalText;
            Tags = tags;
            Seentext = seentext;
            Process = process;
        }
    }
}
