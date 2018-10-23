using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MHWSaveUtils
{
    public class ItemsBoxReader : SaveDataReaderBase
    {
        public ItemsBoxReader(Stream saveData)
            : base(saveData)
        {
        }
    }
}
