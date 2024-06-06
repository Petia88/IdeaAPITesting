using IdeaAPITesting.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IdeaAPITesting
{
    public class IdeaAPITests
    {
        private RestClient client;
        private const string BASE_URL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "petiatest@test.bg";
        private const string PASSWORD = "123456";

        private static string lastIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);
            var options = new RestClientOptions(BASE_URL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient(BASE_URL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            { 
                email,
              password 
            });
            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK) 
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token)) 
                {
                    throw new InvalidOperationException("Assess Token is null or white space.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected responce type {response.StatusCode}, with data {response.Content}");
            }
        }

        [Test, Order (1)]
        public void CreateNewIdea_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "Test Title",
                Description = "test Description"
            };
            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            //Act
            var responce = client.Execute(request, Method.Post);
            var responceData = JsonSerializer.Deserialize<ApiResponseDTO>(responce.Content);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responceData.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllIdeas_ShouldReturnNonEmptyArray()
        {
            //Arrange
            var request = new RestRequest("/api/Idea/All");
           
            //Act
            var responce = client.Execute(request, Method.Get);
            var responceDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(responce.Content);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responceDataArray.Length, Is.GreaterThan(0));

            lastIdeaId = responceDataArray[responceDataArray.Length - 1].IdeaId;
        }

        [Test, Order(3)]
        public void EditIdea_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "Edited Test Title",
                Description = "test Description with edit"
            };
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", lastIdeaId);
            request.AddJsonBody(requestData);

            //Act
            var responce = client.Execute(request, Method.Put);
            var responceData = JsonSerializer.Deserialize<ApiResponseDTO>(responce.Content);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responceData.Msg, Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]
        public void DeleteLastIdea_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastIdeaId);

            //Act
            var responce = client.Execute(request, Method.Delete);
            
            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responce.Content, Does.Contain("The idea is deleted!"));

        }

        [Test, Order(5)]
        public void CreateNewIdea_WithUncorrectData_ShouldFail()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "Test Title"
            };
            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            //Act
            var responce = client.Execute(request, Method.Post);
            var responceData = JsonSerializer.Deserialize<ApiResponseDTO>(responce.Content);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditIdea_WithUncorrectId_ShouldFail()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "Edited Test Title",
                Description = "test Description with edit"
            };
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", "111222333");
            request.AddJsonBody(requestData);

            //Act
            var responce = client.Execute(request, Method.Put);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responce.Content, Does.Contain("There is no such idea!"));
        }

        [Test, Order(7)]
        public void DeleteIdea_WithWrongId_ShouldFail()
        {
            //Arrange
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "11122233");

            //Act
            var responce = client.Execute(request, Method.Delete);

            //Assert
            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responce.Content, Does.Contain("There is no such idea!"));

        }
    }
}