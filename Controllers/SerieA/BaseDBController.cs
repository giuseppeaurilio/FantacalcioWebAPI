﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DBControllers.SerieA
{
    public abstract class BaseDBController
    {
        protected string connection = string.Empty;
        public BaseDBController(string cs)
        {
            connection = cs;
        }
    }
}
