﻿using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Phonebook.Api.Tests.TestFramework
{
    internal static class HttpResponseExtensions
    {
        public static void EnsureCorsAllowOriginHeader(this HttpResponseMessage response, Uri expectedHeaderValue)
        {
            response.EnsureCorsAllowOriginHeader(expectedHeaderValue.ToString());
        }

        public static void EnsureCorsAllowOriginHeader(this HttpResponseMessage response, string? expectedHeaderValue)
        {
            if (expectedHeaderValue is null)
            {
                response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out IEnumerable<string>? headerValues);

                headerValues.Should().BeNull();
            }
            else
            {
                response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).FirstOrDefault()
                    .Should().Be(expectedHeaderValue.TrimEnd('/'));
            }
        }

        public static async Task EnsureBadRequestContent(this HttpResponseMessage response, string expectedTitle)
        {
            var jsonContent = JsonSerializer.Deserialize<JsonElement>
                (await response.Content.ReadAsStringAsync());
            jsonContent.GetProperty("status").GetInt32().Should().Be(400);
            jsonContent.GetProperty("title").GetString().Should().Be(expectedTitle);
            jsonContent.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
            jsonContent.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();
        }

        public static async Task EnsureContentIsEquivalentTo(this HttpResponseMessage response, object expected)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var actualContent = await response.Content.ReadAsStringAsync();

            if (expected is string)
            {
                actualContent.Should().Be(expected as string);
            }
            else
            {
                actualContent.Should().Be(
                    JsonSerializer.Serialize(expected, options));
            }
        }
    }
}
