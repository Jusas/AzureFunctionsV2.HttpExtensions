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
using Microsoft.AspNetCore.Http.Internal;
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
            string formParamValue = formParam.Value;
            Assert.Equal("John", formParamValue);

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

        [Fact]
        public async Task Should_assign_FormAttributed_param_value_from_Request_Form_Files()
        {
            // Arrange
            // Two cases: IFormFile and Stream
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var formParam1 = mockedFunctionRequestContext.AddFormHttpParam<IFormFile>("file1");
            var formParam2 = mockedFunctionRequestContext.AddFormHttpParam<Stream>("file2");

            MemoryStream mockFileStream1 = new MemoryStream();
            var mockDataBytes1 = new byte[] {1, 2, 3};
            mockFileStream1.Write(mockDataBytes1);
            mockFileStream1.Seek(0, SeekOrigin.Begin);

            MemoryStream mockFileStream2 = new MemoryStream();
            var mockDataBytes2 = new byte[] { 4, 5, 6 };
            mockFileStream2.Write(mockDataBytes2);
            mockFileStream2.Seek(0, SeekOrigin.Begin);

            FormFileCollection formFileCollection = new FormFileCollection()
            {
                new FormFile(mockFileStream1, 0, mockFileStream1.Length, "file1", "test1.txt"),
                new FormFile(mockFileStream2, 0, mockFileStream2.Length, "file2", "test2.txt"),
            };            
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>(), formFileCollection);

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.NotNull(formParam1.Value);
            var resultBuffer1 = new byte[mockDataBytes1.Length];
            formParam1.Value.OpenReadStream().Read(resultBuffer1, 0, resultBuffer1.Length);
            mockDataBytes1.Should().BeEquivalentTo(resultBuffer1);
            Assert.Equal("test1.txt", formParam1.Value.FileName);

            Assert.NotNull(formParam2.Value);
            var resultBuffer2 = new byte[mockDataBytes2.Length];
            formParam2.Value.Read(resultBuffer2, 0, resultBuffer2.Length);
            mockDataBytes2.Should().BeEquivalentTo(resultBuffer2);

        }

    }
}
