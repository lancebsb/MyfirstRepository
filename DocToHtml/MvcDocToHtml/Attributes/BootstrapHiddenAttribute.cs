﻿using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace MvcDocToHtml.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class BootstrapHiddenAttribute : BootstrapInputAttribute
    {
        public override MvcHtmlString Generate(HtmlHelper htmlHelper, string name, object value)
        {
            return htmlHelper.Hidden(name, value, new { @class = CssClass, style = CssStyle });
        }
    }
}