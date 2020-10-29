using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Phonebook.Api.Tests.Unit.TestFramework
{
    internal static class HttpResponseExtensions
    {
        public static void EnsureCorsAllowOriginHeader(this HttpResponseMessage response, Uri expectedHeaderValue)
        {
            response.EnsureCorsAllowOriginHeader(expectedHeaderValue.ToString());
        }

        public static void EnsureCorsAllowOriginHeader(this HttpResponseMessage response, string expectedHeaderValue)
        {
            if (expectedHeaderValue is null)
            {
                response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out IEnumerable<string> headerValues);

                headerValues.Should().BeNull();
            }
            else
            {
                response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).FirstOrDefault()
                    .Should().BeEquivalentTo(expectedHeaderValue.TrimEnd('/'));
            }
        }
    }
}
