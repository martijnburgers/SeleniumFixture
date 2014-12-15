﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SimpleFixture.Impl;

namespace SeleniumFixture.Impl
{
    public class AutoFillActionProvider
    {
        private readonly IActionProvider _actionProvider;
        private readonly IEnumerable<IWebElement> _elements;
        private readonly object _seedWith;
        private readonly bool _isSimpleSeed;
        private readonly IConstraintHelper _constraintHelper;

        public AutoFillActionProvider(IActionProvider actionProvider, IEnumerable<IWebElement> elements, object seedWith)
        {
            _actionProvider = actionProvider;
            _elements = elements;
            _seedWith = seedWith;
            _isSimpleSeed = IsSeedSimple();
            _constraintHelper = _actionProvider.UsingFixture.Data.Locate<IConstraintHelper>();
        }

        public IThenSubmitActionProvider PerformFill()
        {
            AutoFillElement();

            return new ThenSubmitActionProvider(_actionProvider.UsingFixture, _elements.First());
        }


        private void AutoFillElement()
        {
            Dictionary<string, List<IWebElement>> radioButtons = new Dictionary<string, List<IWebElement>>();

            foreach (IWebElement element in _elements)
            {
                if (!ProcessFormElement(element, radioButtons))
                {
                    foreach (IWebElement findElement in element.FindElements(By.CssSelector("input, select, textarea, datalist")))
                    {
                        ProcessFormElement(findElement, radioButtons);
                    }
                }
            }

            foreach (KeyValuePair<string, List<IWebElement>> keyValuePair in radioButtons)
            {
                AutoFillRadioButtonGroup(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private bool ProcessFormElement(IWebElement findElement, Dictionary<string, List<IWebElement>> radioButtons)
        {
            bool returnValue = false;

            if (findElement.TagName == "select")
            {
                AutoFillSelectElement(findElement);
                returnValue = true;
            }
            else if (findElement.TagName == "input")
            {
                AutoFillInputElement(findElement, radioButtons);
                returnValue = true;
            }
            else if (findElement.TagName == "datalist")
            {
                AutoFillInputElement(findElement, radioButtons);
                returnValue = true;
            }
            else if (findElement.TagName == "textarea")
            {
                AutoFillInputElement(findElement, radioButtons);
                returnValue = true;
            }

            return returnValue;
        }

        private void AutoFillRadioButtonGroup(string key, List<IWebElement> values)
        {
            string stringConstraintValue = null;

            if (_isSimpleSeed)
            {
                stringConstraintValue = GetStringFromValue(_seedWith);
            }
            else
            {
                object setValue = _constraintHelper.GetValue<object>(_seedWith, null, key);

                if (setValue != null)
                {
                    stringConstraintValue = GetStringFromValue(setValue);
                }
            }

            if (!string.IsNullOrEmpty(stringConstraintValue))
            {
                foreach (IWebElement webElement in values)
                {
                    if (webElement.GetAttribute(ElementContants.ValueAttribute) == stringConstraintValue)
                    {
                        webElement.Click();
                        return;
                    }
                }
            }

            var radioElement = _actionProvider.Generate<IRandomDataGeneratorService>().NextInSet(values);

            if (radioElement != null)
            {
                radioElement.Click();
            }
        }

        private void AutoFillInputElement(IWebElement element, Dictionary<string, List<IWebElement>> radioButtons)
        {
            var type = element.GetAttribute(ElementContants.TypeAttribute);

            if (type == ElementContants.RadioButtonType)
            {
                AddRadioButtonToGroup(element, radioButtons);

                return;
            }

            if (type == ElementContants.HiddenType || type == ElementContants.SubmitType)
            {
                return;
            }

            if (type == ElementContants.CheckBoxType)
            {
                AutoFillCheckBox(element);

                return;
            }

            if (_isSimpleSeed)
            {
                element.Clear();
                element.SendKeys(GetStringFromValue(_seedWith));

                return;
            }

            string elementId = element.GetAttribute("id") ?? element.GetAttribute("name");
            object setValue = null;

            if (!string.IsNullOrEmpty(elementId))
            {
                setValue = _constraintHelper.GetValue<object>(_seedWith, null, elementId);
            }

            if (setValue == null)
            {
                setValue = _actionProvider.Generate<string>(elementId, new { stringType = StringType.AlphaNumeric });
            }

            element.Clear();
            element.SendKeys(GetStringFromValue(setValue));
        }

        private void AutoFillCheckBox(IWebElement element)
        {
            string elementId = element.GetAttribute("id") ?? element.GetAttribute("name");
            bool? checkedValue = null;

            if (!string.IsNullOrEmpty(elementId))
            {
                object checkValue = _constraintHelper.GetValue<object>(_seedWith, null, elementId);

                if (checkValue != null)
                {
                    if (string.Compare(checkValue.ToString(), "true", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        checkedValue = true;
                    }
                    else if (string.Compare(checkValue.ToString(), "false", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        checkedValue = false;
                    }
                }
            }

            if (!checkedValue.HasValue)
            {
                checkedValue = _actionProvider.Generate<bool>();
            }

            if (element.Selected != checkedValue.Value)
            {
                element.Click();
            }
        }

        private static void AddRadioButtonToGroup(IWebElement element, Dictionary<string, List<IWebElement>> radioButtons)
        {
            var name = element.GetAttribute("name");

            if (!string.IsNullOrEmpty(name))
            {
                List<IWebElement> radioButtonGroup;

                if (!radioButtons.TryGetValue(name, out radioButtonGroup))
                {
                    radioButtonGroup = new List<IWebElement>();

                    radioButtons[name] = radioButtonGroup;
                }

                radioButtonGroup.Add(element);
            }
        }

        private string GetStringFromValue(object stringObject)
        {
            if (stringObject is string)
            {
                return stringObject as string;
            }
            if (_seedWith is DateTime)
            {
                return ((DateTime)_seedWith).ToShortDateString();
            }

            if (_seedWith is Enum)
            {
                return Convert.ChangeType(_seedWith, Enum.GetUnderlyingType(_seedWith.GetType())).ToString();
            }

            return _seedWith.ToString();
        }

        private void AutoFillSelectElement(IWebElement element)
        {
            SelectElement selectElement = new SelectElement(element);
            bool setValue = false;

            if (_isSimpleSeed)
            {
                setValue = true;
            }
            else
            {
                string elementId = element.GetAttribute("id") ?? element.GetAttribute("name");

                object value = _constraintHelper.GetValue<object>(_seedWith, null, elementId);

                if (value != null)
                {
                    string stringValue = GetStringFromValue(value);

                    var option = selectElement.Options.FirstOrDefault(e => e.GetAttribute(ElementContants.ValueAttribute) == stringValue);

                    if (option != null)
                    {
                        selectElement.SelectByValue(stringValue);
                        setValue = true;
                    }
                    else
                    {
                        option = selectElement.Options.FirstOrDefault(e => e.GetAttribute("text") == stringValue);

                        if (option != null)
                        {
                            selectElement.SelectByText(stringValue);
                            setValue = true;
                        }
                    }
                }
            }

            if (!setValue)
            {
                var selectedOption =
                    _actionProvider.Generate<IRandomDataGeneratorService>().NextInSet(
                        selectElement.Options.Where(e => !string.IsNullOrEmpty(e.GetAttribute(ElementContants.ValueAttribute))));

                if (selectedOption != null)
                {
                    var value = selectedOption.GetAttribute(ElementContants.ValueAttribute);

                    selectElement.SelectByValue(value);
                }
            }
        }

        private bool IsSeedSimple()
        {
            if (_seedWith == null)
                return false;

            return _seedWith.GetType().IsPrimitive ||
                   _seedWith is string ||
                   _seedWith is DateTime ||
                   _seedWith is Enum;
        }
    }
}
