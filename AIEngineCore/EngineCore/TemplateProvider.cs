namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class TemplateProvider : ITemplateProvider
    {
        public Task<string> GetTemplate(EngineEvents @event, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
