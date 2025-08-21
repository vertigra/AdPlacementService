using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace AdPlacementService.Tests
{
    public class AdPlacementIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task SearchEndpoint_ShouldReturnMessage_WhenNoDataLoaded()
        {
            //здесь необходим новый экземпляр фабрики для сброса состояния сервера 
            using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/search/ru/msk");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Before searching, you should upload the data to the server", responseContent);
        }

        [Fact]
        public async Task LoadEndpoint_ShouldAcceptTextData_And_ReturnSuccess()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var content = new StringContent(testData, Encoding.UTF8, "text/plain");

            var response = await client.PostAsync("/api/load", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Data loaded successfully", responseContent);
            Assert.Contains("4 platforms", responseContent);
        }

        [Fact]
        public async Task SearchEndpoint_ShouldReturnCorrectResults_ForRuLocation()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            await client.PostAsync("/api/load", loadContent);

            var response = await client.GetAsync("/api/search/ru");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(result);
            Assert.Contains("Яндекс.Директ", result);
            Assert.Single(result);
        }

        [Fact]
        public async Task SearchEndpoint_ShouldReturnCorrectResults_ForRuMskLocation()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            await client.PostAsync("/api/load", loadContent);

            var response = await client.GetAsync("/api/search/ru/msk");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(result);
            Assert.Contains("Яндекс.Директ", result);
            Assert.Contains("Газета уральских москвичей", result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SearchEndpoint_ShouldReturnCorrectResults_ForRuSvrdLocation()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            await client.PostAsync("/api/load", loadContent);

            var response = await client.GetAsync("/api/search/ru/svrd");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(result);
            Assert.Contains("Яндекс.Директ", result);
            Assert.Contains("Крутая реклама", result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SearchEndpoint_ShouldReturnCorrectResults_ForRuSvrdRevdaLocation()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            await client.PostAsync("/api/load", loadContent);

            var response = await client.GetAsync("/api/search/ru/svrd/revda");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(result);
            Assert.Contains("Яндекс.Директ", result);
            Assert.Contains("Ревдинский рабочий", result);
            Assert.Contains("Крутая реклама", result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task SearchEndpoint_ShouldReturnMessage_ForNonExistentLocation()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            await client.PostAsync("/api/load", loadContent);

            var response = await client.GetAsync("/api/search/non/existent/location");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("The specified location '/non/existent/location' was not found", responseContent);
        }

        [Fact]
        public async Task LoadEndpoint_ShouldReturnBadRequest_WhenContentIsEmpty()
        {
            using var client = _factory.CreateClient();

            var content = new StringContent("", Encoding.UTF8, "text/plain");

            var response = await client.PostAsync("/api/load", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Content is empty", responseContent);
        }

        [Fact]
        public async Task ComplexSearchScenarios_ShouldWorkCorrectly()
        {
            using var client = _factory.CreateClient();

            var testData = @"Яндекс.Директ:/ru
                             Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
                             Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
                             Крутая реклама:/ru/svrd";

            var loadContent = new StringContent(testData, Encoding.UTF8, "text/plain");
            var loadResponse = await client.PostAsync("/api/load", loadContent);
            Assert.Equal(HttpStatusCode.OK, loadResponse.StatusCode);

            var testCases = new[]
            {
                new { Location = "ru", ExpectedCount = 1, ExpectedNames = new[] { "Яндекс.Директ" } },
                new { Location = "ru/msk", ExpectedCount = 2, ExpectedNames = new[] { "Яндекс.Директ", "Газета уральских москвичей" } },
                new { Location = "ru/svrd", ExpectedCount = 2, ExpectedNames = new[] { "Яндекс.Директ", "Крутая реклама" } },
                new { Location = "ru/svrd/revda", ExpectedCount = 3, ExpectedNames = new[] { "Яндекс.Директ", "Ревдинский рабочий", "Крутая реклама" } }
            };

            foreach (var testCase in testCases)
            {
                var response = await client.GetAsync($"/api/search/{testCase.Location}");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = await response.Content.ReadFromJsonAsync<List<string>>();
                Assert.NotNull(result);
                Assert.Equal(testCase.ExpectedCount, result.Count);

                foreach (var expectedName in testCase.ExpectedNames)
                {
                    Assert.Contains(expectedName, result);
                }
            }
        }
    }
}