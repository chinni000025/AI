namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface ITemplateRenderer
    {
        string Render(string htmlDocument, IReadOnlyDictionary<string, string> parameters);
    }
}
