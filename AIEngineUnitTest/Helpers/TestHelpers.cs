using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIEngineUnitTest.Helpers
{
    public class TestHelpers
    {
        public void SetupMockHttpContext(string scheme = "https", string host = "localhost:8085")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = new HostString(host);
        }
    }
}
