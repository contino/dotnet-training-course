﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjectLayer.Progressive.OnlineShop.V2.Part4
{
    // resp: product selection
    public interface IShopAssistant
    {
        OnlineBasket Browse();
    }
}
