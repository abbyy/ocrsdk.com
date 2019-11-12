// Copyright © 2019 ABBYY Production LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abbyy.CloudSdk.V2.Client.Models;
using Abbyy.CloudSdk.V2.Client.Models.Enums;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Abbyy.CloudSdk.V2.Client.Sample
{
	public class Program
	{
		private const string ApplicationId = @"PASTE_Application_ID";
		private const string Password = @"PAST_Application_Password";

		/// <summary>
		/// Processing Location URL https://www.ocrsdk.com/documentation/specifications/data-processing-location/
		/// </summary>
		private const string ServiceUrl = "https://cloud-eu.ocrsdk.com";

		private static int _retryCount = 3;
		private static int _delayBetweenRetriesInSeconds = 3;
		private static string _httpClientName = "OCR_HTTP_CLIENT";

		private static readonly AuthInfo AuthInfo = new AuthInfo
		{
			Host = ServiceUrl,
			ApplicationId = ApplicationId,
			Password = Password
		};

		private static ServiceProvider _serviceProvider;
		private static HttpClient _httpClient;

		public static async Task Main()
		{
			// Init client
			// You could also call GetOcrClient to use client without retry policy
			using (var ocrClient = GetOcrClientWithRetryPolicy())
			{
				// Process image
				// You could also call ProcessDocumentAsync or any other processing method declared below
				var resultUrls = await ProcessImageAsync(ocrClient);

				// Get results
				foreach (var resultUrl in resultUrls)
					Console.WriteLine(resultUrl);

				// Get list of finished tasks
				var finishedTasks = await GetFinishedTasksAsync(ocrClient);
				foreach (var finishedTask in finishedTasks.Tasks)
					Console.WriteLine(finishedTask.TaskId);

				DisposeServices();
			}
		}

		private static IOcrClient GetOcrClient()
		{
			return new OcrClient(AuthInfo);
		}

		private static IOcrClient GetOcrClientWithRetryPolicy()
		{
			// Create service collection and configure our services
			var services = ConfigureServices();
			// Generate a provider
			_serviceProvider = services.BuildServiceProvider();

			var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
			_httpClient = httpClientFactory.CreateClient(_httpClientName);

			return new OcrClient(_httpClient);
		}

		private static ServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection();

			//Configure HttpClientFactory with retry handler
			services.AddHttpClient(_httpClientName, conf =>
				{
					conf.BaseAddress = new Uri(AuthInfo.Host);
					//increase the default value of timeout for the duration of retries
					conf.Timeout = conf.Timeout + TimeSpan.FromSeconds(_retryCount * _delayBetweenRetriesInSeconds);
				})
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
				{
					PreAuthenticate = true,
					Credentials = new NetworkCredential(AuthInfo.ApplicationId, AuthInfo.Password)
				})
				//Add  custom HttpClientRetryPolicyHandler with polly
				.AddHttpMessageHandler(() => new HttpClientRetryPolicyHandler(GetRetryPolicy()));

			//or you can use Microsoft.Extensions.DependencyInjection Polly extension
			//.AddPolicyHandler(GetRetryPolicy());
			return services;
		}

		private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions.HandleTransientHttpError()
				//Condition - what kind of request errors should we repeat
				.OrResult(r => r.StatusCode == HttpStatusCode.GatewayTimeout)
				.WaitAndRetryAsync(
					_retryCount,
					sleepDurationProvider => TimeSpan.FromSeconds(_delayBetweenRetriesInSeconds),
					(exception, calculatedWaitDuration, retries, context) =>
					{
						Console.WriteLine($"Retry {retries} for policy with key {context.PolicyKey}");
					}
				)
				.WithPolicyKey("WaitAndRetryAsync_For_GatewayTimeout_504__StatusCode");
		}

		private static async Task<List<string>> ProcessImageAsync(IOcrClient ocrClient)
		{
			var imageParams = new ImageProcessingParams
			{
				ExportFormats = new[] {ExportFormat.Docx, ExportFormat.Txt,},
				Language = "English,French",
			};
			const string filePath = "processImage.jpg";

			using (var fileStream = new FileStream(filePath, FileMode.Open))
			{
				var taskInfo = await ocrClient.ProcessImageAsync(
					imageParams,
					fileStream,
					Path.GetFileName(filePath),
					waitTaskFinished: true);

				return taskInfo.ResultUrls;
			}
		}

		private static async Task<List<string>> ProcessDocumentAsync(IOcrClient ocrClient)
		{
			var taskId = await UploadFilesAsync(ocrClient);

			var processingParams = new DocumentProcessingParams
			{
				ExportFormats = new[] {ExportFormat.Docx, ExportFormat.Txt,},
				Language = "English,French",
				TaskId = taskId,
			};

			var taskInfo = await ocrClient.ProcessDocumentAsync(
				processingParams,
				waitTaskFinished: true);

			return taskInfo.ResultUrls;
		}

		private static async Task<Guid> UploadFilesAsync(IOcrClient ocrClient)
		{
			ImageSubmittingParams submitParams;
			var firstFilePath = "processImage.jpg";
			var secondFilePath = "processDocument.jpg";

			// First file
			using (var fileStream = new FileStream(firstFilePath, FileMode.Open))
			{
				var submitImageResult = await ocrClient.SubmitImageAsync(
					null,
					fileStream,
					Path.GetFileName(firstFilePath));

				// Save TaskId for next files and ProcessDocument method
				submitParams = new ImageSubmittingParams {TaskId = submitImageResult.TaskId};
			}

			// Second file
			using (var fileStream = new FileStream(secondFilePath, FileMode.Open))
			{
				await ocrClient.SubmitImageAsync(
					submitParams,
					fileStream,
					Path.GetFileName(secondFilePath));
			}

			return submitParams.TaskId.Value;
		}

		private static async Task<TaskList> GetFinishedTasksAsync(IOcrClient ocrClient)
		{
			var finishedTasks = await ocrClient.ListFinishedTasksAsync();
			return finishedTasks;
		}

		private static void DisposeServices()
		{
			(_serviceProvider as IDisposable)?.Dispose();
		}
	}
}
