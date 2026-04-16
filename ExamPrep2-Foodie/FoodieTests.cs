using ExamPrep2_Foodie.Models;
using Microsoft.VisualBasic;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Text.Json;


namespace ExamPrep2_Foodie
{
    public class Tests
    {
        private RestClient client;
        private const string BaseUrl = "http://144.91.123.158:81";
        private const string StaticTocken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJhZWFhOWMwNy00MjBjLTQyYjEtYmNlNS02NjExYTAwZjJkZmIiLCJpYXQiOiIwNC8xNi8yMDI2IDExOjE4OjE0IiwiVXNlcklkIjoiOTQ3OGE2NTUtMjk5OS00ZjYwLTc0OTQtMDhkZTc2OGU4Zjk3IiwiRW1haWwiOiJNZXJyNHlCZGVlckBkbXguY29tIiwiVXNlck5hbWUiOiJNZXI0eUJkIiwiZXhwIjoxNzc2MzU5ODk0LCJpc3MiOiJGb29keV9BcHBfU29mdFVuaSIsImF1ZCI6IkZvb2R5X1dlYkFQSV9Tb2Z0VW5pIn0.j0JlPoK7eIUQCoK8ks16rwDrQt3RwXJ2_5w-WKX3Jog";
        private const string LoginUserName = "Mer4yBd";
        private const string LoginPassword = "123456789";

        private static string foodId;  // First created Food Id


        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticTocken))
            {
                jwtToken = StaticTocken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginUserName, LoginPassword);
            }
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var tempClient = new RestRequest(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = new RestClient(BaseUrl).Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString(); //"AccessToken"

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewFood_WithRequeredField_shouldReturnSuccess()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "Pizza",
                Description = "This is the best Food.",
                Url = ""
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            RestResponse response = this.client.Execute(request);
        
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            ApiResponseDTO readyReasponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            if (readyReasponse.FoodId != null)
            {
                foodId = readyReasponse.FoodId;
            }
        }


        [Order(2)]
        [Test]
        public void EditLastCreatedFoodTitle_shouldReturnSuccess()
        {
            RestRequest request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);
            request.AddJsonBody(new[]
            {
                new
                {  path = "/name",
                   op = "replace",
                   value = "Spagheti"
                }
            });
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDTO readyReasponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyReasponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllFoods_shouldReturnListWithAllFoods()
        {
            RestRequest request = new RestRequest($"/api/Food/All", Method.Get);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            List<FoodDTO> foods = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);

            Assert.That(foods, Is.Not.Null);
            Assert.That(foods, Is.Not.Empty);
            Assert.That(foods.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void DeleteLastEditedFood_shouldReturnSuccess()
        {
            RestRequest request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyReasponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyReasponse.Msg, Is.EqualTo("Deleted successfully!"));
          
        }

        [Order(5)]
        [Test]
        public void CreateFoodWithoutRequeredFields_schouldReturnBadRequest()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "",
                Description = "",
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            RestResponse response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNotExistingFood_schouldReturnNotFound()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
            request.AddJsonBody(new[]
            {
                new
                {  path = "/name",
                   op = "replace",
                   value = "Spagheti"
                }
            });
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            ApiResponseDTO readyReasponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);    
            Assert.That(readyReasponse.Msg, Is.EqualTo("No food revues..."));
        }

        [Order(7)]
        [Test]
        public void DeleteNotExistingFood_schouldReturnBadRequest()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);           
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            ApiResponseDTO readyReasponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyReasponse.Msg, Is.EqualTo("No food revues..."));
        }
               
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}