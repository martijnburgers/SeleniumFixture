﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumFixture.xUnit
{
    public interface IWebDriverInitializationAttribute
    {
        void Initialize(MethodInfo method, IWebDriver driver);
    }
}
