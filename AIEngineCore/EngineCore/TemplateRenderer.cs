namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System.Collections.Generic;
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
