﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib
{
    public interface ICommand
    {
        T execute<T>();
    }
}