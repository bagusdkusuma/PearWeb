﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSLNG.PEAR.Web.ViewModels.ProcessBlueprint
{
    public class ProcessBlueprintViewModel
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public string Name { get; set; }

        public bool IsFolder { get; set; }

        public byte[] Data { get; set; }

        public DateTime? LastWriteTime { get; set; }
    }
}