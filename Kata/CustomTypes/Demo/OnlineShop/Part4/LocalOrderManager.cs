﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kata.CustomTypes.OnlineShop.Part4
{
    public class LocalOrderManager : IOrderManager
    {
        public void AddToCart()
        {
            Console.WriteLine("Creating new order");
        }
    }
}