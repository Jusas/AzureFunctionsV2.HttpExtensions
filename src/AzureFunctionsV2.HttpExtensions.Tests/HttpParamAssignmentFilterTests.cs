using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests
{
    public class HttpParamAssignmentFilterTests
    {
        [Fact]
        public async Task Should_assign_FormAttributed_param_value_from_Request_Form()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var formParam = mockedFunctionRequestContext.AddFormHttpParam<string>("firstname", "Name");
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>() {{"Name", "John"}});

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("John", formParam.Value);

        }

        [Fact]
        public async Task Should_assign_HeaderAttributed_param_value_from_Request_Headers()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var headerParam = mockedFunctionRequestContext.AddHeaderHttpParam<string>("myHeader", "x-my-header");
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(new Dictionary<string, StringValues>() {{"x-my-header", "value"}});

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("value", headerParam.Value);

        }

        public class ComplexObject
        {
            public string Stringvalue { get; set; }
            public int IntValue { get; set; }
        }

        [Fact]
        public async Task Should_assign_BodyAttributed_complex_object_param_value_from_Request_Body()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<ComplexObject>("body");
            var complexInputObject = new ComplexObject() {IntValue = 1, Stringvalue = "hello"};

            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(complexInputObject));
            mockedFunctionRequestContext.HttpRequest.Body = new MemoryStream(buffer);

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            bodyParam.Value.Should().BeEquivalentTo(complexInputObject);

        }

        [Fact]
        public async Task Should_assign_BodyAttributed_list_param_value_from_Request_Body()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<List<string>>("body");
            var listInputObject = new List<string>() {"hello", "world"};

            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(listInputObject));
            mockedFunctionRequestContext.HttpRequest.Body = new MemoryStream(buffer);

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            bodyParam.Value.Should().BeEquivalentTo(listInputObject);

        }

        [Fact]
        public async Task Should_assign_BodyAttributed_complex_object_from_body_xml_string_to_complex_object()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.ContentType = "application/xml";
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<ComplexObject>("body");
            var complexInputObject = new ComplexObject() {Stringvalue = "hello", IntValue = 1};

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ComplexObject));
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, complexInputObject);

            // var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arrayInputObject));
            stream.Seek(0, SeekOrigin.Begin);
            mockedFunctionRequestContext.HttpRequest.Body = stream;

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            bodyParam.Value.Should().BeEquivalentTo(complexInputObject);

        }

        [Fact]
        public async Task Should_assign_BodyAttributed_array_param_value_from_Request_Body()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<List<string>>("body");
            var arrayInputObject = new string[] { "hello", "world" };

            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arrayInputObject));
            mockedFunctionRequestContext.HttpRequest.Body = new MemoryStream(buffer);

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            bodyParam.Value.Should().BeEquivalentTo(arrayInputObject);

        }
    }
}
