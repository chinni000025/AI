using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public interface ITemplateRenderer
    {
        string Render(string htmlDocument, IReadOnlyDictionary<string, string> parameters);
    }
}
