using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BodyFat.Models;

namespace BodyFat.Controllers
{
    public class HomeController : Controller
    {
        private const string EndpointUrl = "https://automl-bodyfat.germanywestcentral.inference.ml.azure.com/score";
        private const string ApiKey = "90yAvPzEyjCcaiDoOnv1DEtPxbsEcEcqGBsRWRJ4VeUqPNBGcpcoJQQJ99CHAAAAAAAAAAAAINFRAZML1NGN";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new BodyFatInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(BodyFatInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                double weightKg = ParseDouble(model.WeightKg);
                double heightCm = ParseDouble(model.HeightCm);
                double neckCm = ParseDouble(model.NeckCm);
                double chestCm = ParseDouble(model.ChestCm);
                double abdomenCm = ParseDouble(model.AbdomenCm);
                double hipCm = ParseDouble(model.HipCm);
                double thighCm = ParseDouble(model.ThighCm);
                double kneeCm = ParseDouble(model.KneeCm);
                double ankleCm = ParseDouble(model.AnkleCm);
                double bicepsCm = ParseDouble(model.BicepsCm);
                double forearmCm = ParseDouble(model.ForearmCm);
                double wristCm = ParseDouble(model.WristCm);

                double weightLbs = weightKg * 2.20462;
                double heightInches = heightCm / 2.54;

                var payload = new
                {
                    input_data = new
                    {
                        columns = new[]
                        {
                            "Age", "Weight", "Height", "Neck", "Chest",
                            "Abdomen", "Hip", "Thigh", "Knee", "Ankle",
                            "Biceps", "Forearm", "Wrist"
                        },
                        index = new[] { 0 },
                        data = new object[][]
                        {
                            new object[]
                            {
                                model.Age,
                                Math.Round(weightLbs, 2),
                                Math.Round(heightInches, 2),
                                neckCm,
                                chestCm,
                                abdomenCm,
                                hipCm,
                                thighCm,
                                kneeCm,
                                ankleCm,
                                bicepsCm,
                                forearmCm,
                                wristCm
                            }
                        }
                    },
                    params_data = new { }
                };

                var handler = new HttpClientHandler()
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    var jsonContent = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                    HttpResponseMessage response = await client.PostAsync(EndpointUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        double? predictedFat = ParseAzureResponse(responseString);

                        if (predictedFat.HasValue)
                        {
                            double resultVal = Math.Round(predictedFat.Value, 1);
                            ViewBag.Result = resultVal;

                            if (resultVal < 10.0)
                            {
                                ViewBag.ResultCategory = "low";
                            }
                            else if (resultVal > 20.0)
                            {
                                ViewBag.ResultCategory = "high";
                            }
                            else
                            {
                                ViewBag.ResultCategory = "norm";
                            }
                        }
                        else
                        {
                            ViewBag.Error = $"Не вдалося розпізнати результат від сервера: {responseString}";
                        }
                    }
                    else
                    {
                        string responseError = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = $"Запит завершився з помилкою (Код {response.StatusCode}): {responseError}";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Виникла помилка під час виконання запиту: {ex.Message}";
            }

            return View(model);
        }

        private static double ParseDouble(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0.0;

            string normalizedInput = input.Replace(',', '.');

            if (double.TryParse(normalizedInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0.0;
        }

        private double? ParseAzureResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    return root[0].GetDouble();

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("Results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                        return results[0].GetDouble();

                    if (root.TryGetProperty("predictions", out var predictions) && predictions.ValueKind == JsonValueKind.Array && predictions.GetArrayLength() > 0)
                        return predictions[0].GetDouble();

                    if (root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array && result.GetArrayLength() > 0)
                        return result[0].GetDouble();
                }
            }
            catch
            {
                string cleanJson = jsonResponse.Trim('[', ']', ' ', '\r', '\n', '"');
                if (double.TryParse(cleanJson, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    return val;
                }
            }
            return null;
        }
    }
}