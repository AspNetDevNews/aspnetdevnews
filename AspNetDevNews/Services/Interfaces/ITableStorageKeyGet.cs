﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface ITableStorageKeyGet
    {
        string GetPartitionKey();
        string GetRowKey();
    }
}
