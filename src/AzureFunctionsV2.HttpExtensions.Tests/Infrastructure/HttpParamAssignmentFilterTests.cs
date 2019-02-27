using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using AzureFunctionsV2.HttpExtensions.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Infrastructure
{
    /// <summary>
    /// Tests for HttpParamAssignmentFilter.
    /// </summary>
    public class HttpParamAssignmentFilterTests
    {
        /// <summary>
        /// Should assign HttpParam with HttpFormAttribute with the corresponding value in HttpRequest.Form.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpHeaderAttribute with the corresponding value in HttpRequest.Headers.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpQueryAttribute with the corresponding value in HttpRequest.Query.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_assign_QueryAttributed_param_value_from_Request_Query()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var queryParam = mockedFunctionRequestContext.AddQueryHttpParam<string>("queryParam", "qp");
            var queryParamList = mockedFunctionRequestContext.AddQueryHttpParam<List<string>>("queryParamList");
            mockedFunctionRequestContext.HttpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                {"qp", "hello"},
                {"queryParamList", new StringValues(new []{"one", "two"})}
            });

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("hello", queryParam.Value);
            Assert.Equal("one", queryParamList.Value.First());
            Assert.Equal("two", queryParamList.Value.Last());
        }

        public class ComplexObject
        {
            public string Stringvalue { get; set; }
            public int IntValue { get; set; }
        }

        /// <summary>
        /// Should assign HttpParam with HttpBodyAttribute with the corresponding value in HttpRequest.Body.
        /// Specifically should deserialize the body content to ComplexObject.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpBodyAttribute with the corresponding value in HttpRequest.Body.
        /// Specifically should deserialize the the body content to a List&lt;string&gt;
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpBodyAttribute with the corresponding value in HttpRequest.Body.
        /// Specifically should deserialize the the body content which is XML to ComplexObject.
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Should assign HttpParam with HttpBodyAttribute with the corresponding value in HttpRequest.Body.
        /// Specifically should deserialize the the body content which is a JSON serialized
        /// string array to a List&lt;string&gt;
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpFormAttribute with the corresponding value in HttpRequest.Form.Files.
        /// HttpParams of type IFormFile and Stream should be assigned from the corresponding entries in
        /// HttpRequest.Form.Files.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Should assign HttpParam with HttpFormAttribute with HttpRequest.Form.Files.
        /// More specifically when the HttpParam is of type IFormFileCollection the HttpRequest.Form.Files
        /// should be assigned to its value.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_assign_FormAttributed_IFormFileCollection_param_value_from_Request_Form_Files()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var formParam = mockedFunctionRequestContext.AddFormHttpParam<IFormFileCollection>("files");
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>(), 
                new FormFileCollection()
                {
                    new FormFile(new MemoryStream(), 0, 0, "file1", "file1.txt"),
                    new FormFile(new MemoryStream(), 0, 0, "file2", "file2.txt"),
                });

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal(2, formParam.Value.Count);
            Assert.Equal("file1", formParam.Value[0].Name);
            Assert.Equal("file2", formParam.Value[1].Name);

        }

        /// <summary>
        /// Produces mocked function contexts with HttpParams that are assigned from different
        /// request sources (form, body, query, header).
        /// </summary>
        /// <returns></returns>
        public List<MockedFunctionRequestContext> FunctionContextsWithRequiredHttpParamsButWithNoData()
        {
            List<MockedFunctionRequestContext> contexts = new List<MockedFunctionRequestContext>();

            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<List<string>>("body", true);
            mockedFunctionRequestContext.HttpRequest.Body = null;
            contexts.Add(mockedFunctionRequestContext);

            mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var queryParam = mockedFunctionRequestContext.AddQueryHttpParam<string>("query", required: true);
            mockedFunctionRequestContext.HttpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>());
            contexts.Add(mockedFunctionRequestContext);

            mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var formParam = mockedFunctionRequestContext.AddFormHttpParam<int>("form", required: true);
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection());
            contexts.Add(mockedFunctionRequestContext);

            mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var headerParam = mockedFunctionRequestContext.AddHeaderHttpParam<string>("header", required: true);
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(new Dictionary<string, StringValues>());
            contexts.Add(mockedFunctionRequestContext);

            mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var formFileCollectionParam = mockedFunctionRequestContext.AddFormHttpParam<IFormFileCollection>("files");
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>(),
                new FormFileCollection());

            return contexts;
        }

        /// <summary>
        /// Checks that when a parameter is missing or null the ParameterRequiredException gets thrown, but also
        /// caught and stored into the HttpContext.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_store_exception_when_required_parameters_missing_or_null()
        {
            // Arrange
            var testContexts = FunctionContextsWithRequiredHttpParamsButWithNoData();

            // Act
            foreach (var testContext in testContexts)
            {
                var httpParamAssignmentFilter = new HttpParamAssignmentFilter(testContext.RequestStoreMock.Object, null);
                await httpParamAssignmentFilter.OnExecutingAsync(testContext.FunctionExecutingContext,
                    CancellationToken.None);
                testContext.HttpContext.GetStoredExceptions().Count.Should().BeGreaterThan(0);
                //await Assert.ThrowsAnyAsync<ParameterRequiredException>(async () =>
                //{
                //    await httpParamAssignmentFilter.OnExecutingAsync(testContext.FunctionExecutingContext,
                //        new CancellationToken());
                //});
            }

        }

    }
}
