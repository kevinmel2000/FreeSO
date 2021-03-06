﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Content.Interfaces
{
    public interface IObjectCatalog
    {
        List<ObjectCatalogItem> All();
        List<ObjectCatalogItem> GetItemsByCategory(sbyte category);
        ObjectCatalogItem? GetItemByGUID(uint guid);
    }

    public struct ObjectCatalogItem
    {
        public uint GUID;
        public sbyte Category;
        public uint Price;
        public string Name;
        public byte DisableLevel; //1 = only shopping, 2 = rare (unsellable?)
    }
}
