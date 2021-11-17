﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Recipe.Part8
{
    public interface IPantry
    {
        Ingredient Measure(string name, string amount);
        TIngredient Measure<TIngredient>(Func<TIngredient> instantiate);
    }
}
