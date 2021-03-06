﻿using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumFixture.xUnit
{
    public abstract class ChromeOptionsAttribute : Attribute
    {
        public abstract ChromeOptions ProvideOptions(MethodInfo testMethod);
    }
}
