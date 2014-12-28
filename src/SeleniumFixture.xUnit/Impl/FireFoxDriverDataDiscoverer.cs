﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SeleniumFixture.xUnit.Impl
{
    public class FireFoxDriverDataDiscoverer : IDataDiscoverer
    {
        public IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            var parameters = testMethod.GetParameters().ToArray();

            if (parameters.Length != 1)
            {
                throw new Exception("Method must take IWebDriver or Fixture");
            }

            if (parameters[0].ParameterType.ToRuntimeType() == typeof(IWebDriver))
            {
                yield return new object[] { new FirefoxDriver() };
            }
            else if (parameters[0].ParameterType.ToRuntimeType() == typeof(Fixture))
            {
                var driver = new FirefoxDriver();

                yield return new object[] { FixtureCreationAttribute.GetNewFixture(driver, testMethod.ToRuntimeMethod()) };
            }
            else
            {
                throw new Exception("Method must take IWebDriver or Fixture");
            }
        }

        public bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            return false;
        }
    }
}