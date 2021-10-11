﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kata.CustomTypes.MenuFactoryList
{
    public class DrinksMenu : MenuBase
    {
        protected override void CreateItems()
        {
            var price = 1.5M;
            Items.Add(new DrinksMenuItem("Water", ref price));
            //Items.Add(new DrinksMenuItem("Tea", 1.25M));
           // Items.Add(new DrinksMenuItem("Coffee", 4.0M));
        }
    }
}