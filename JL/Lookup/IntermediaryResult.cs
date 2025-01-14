﻿using System.Collections.Generic;
using JL.Dicts;

namespace JL.Lookup
{
    public class IntermediaryResult
    {
        public List<IResult> ResultsList { get; }
        public List<List<string>> ProcessList { get; }
        public string FoundForm { get; }
        public DictType DictType { get; }

        public IntermediaryResult(List<IResult> resultsList, List<List<string>> processList, string foundForm,
            DictType dictType)
        {
            ResultsList = resultsList;
            ProcessList = processList;
            FoundForm = foundForm;
            DictType = dictType;
        }
    }
}
