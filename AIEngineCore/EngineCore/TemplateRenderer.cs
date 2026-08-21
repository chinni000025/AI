using AIEngineConnectivity.EngineCore;
using System.Collections.Generic;

namespace AIEngineCore.EngineCore
{
    public class TemplateRenderer : ITemplateRenderer
    {
        public string Render(string htmlDocument, IReadOnlyDictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                htmlDocument = htmlDocument.Replace($"{{{{{parameter.Key}}}}}", parameter.Value);
            }
            return htmlDocument;
        }
    }
}
