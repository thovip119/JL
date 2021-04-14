﻿using System.Collections.Generic;

namespace JapaneseLookup.EDICT
{
    class Trans
    {
        public List<string> NameTypeList { get; set; }
        public List<string> XRefList { get; set; }
        public List<string> TransDetList { get; set; }

        public Trans()
        {
            NameTypeList = new List<string>();
            XRefList = new List<string>();
            TransDetList = new List<string>();
        }
    }
}
