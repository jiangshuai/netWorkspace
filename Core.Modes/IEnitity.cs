﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modes
{
    public interface IEnitity<TKey>
    {
        TKey ID { get; set; }
    }
}
