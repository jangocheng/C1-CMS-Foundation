﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Composite.Functions;
using System.Xml.Linq;
using System.IO;
using Composite.Core.IO;
using Composite.C1Console.Security;
using Composite.Plugins.Functions.FunctionProviders.StandardFunctionProvider.Foundation;
using Composite.Core.ResourceSystem;
using System.Xml;
using Composite.Core.Xml;

namespace Composite.Plugins.Functions.FunctionProviders.StandardFunctionProvider.Xml
{
    internal sealed class LoadXhtmlFileFunction : StandardFunctionBase
    {
        public LoadXhtmlFileFunction(EntityTokenFactory entityTokenFactory)
            : base("LoadXhtmlFile", "Composite.Core.Xml", typeof(XhtmlDocument), entityTokenFactory)
        {
        }

        public override object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            string relativePath = parameters.GetParameter<string>("RelativePath");

            string path = Path.Combine(PathUtil.Resolve("~"), relativePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found. Ensure path is relative (that it does not start with '/').", path);
            }

            using (var streamReader = new System.IO.StreamReader(path))
            {
                using (var reader = XmlReader.Create(streamReader))
                {
                    return new XhtmlDocument(XDocument.Load(reader));
                }
            }
        }

        protected override IEnumerable<StandardFunctionParameterProfile> StandardFunctionParameterProfiles
        {
            get
            {
                WidgetFunctionProvider textboxWidget = StandardWidgetFunctions.TextBoxWidget;

                yield return new StandardFunctionParameterProfile(
                    "RelativePath", typeof(string), true, new NoValueValueProvider(), textboxWidget);
            }
        }
    }
}
