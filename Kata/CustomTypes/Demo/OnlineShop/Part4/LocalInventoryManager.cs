﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kata.CustomTypes.OnlineShop.Part4
{
    public class LocalInventoryManager : IInventoryManager
    {
        public void LoadInventory()
        {
            Console.WriteLine("Loading the inventory");
        }
    }
}
