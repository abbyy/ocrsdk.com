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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Abbyy.CloudSdk.V2.Client.Sample
{
	public class HttpClientRetryPolicyHandler : DelegatingHandler
	{
		private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

		/// <summary>Initializes a new instance of the <see cref="HttpClientRetryPolicyHandler"/> class.</summary>
		public HttpClientRetryPolicyHandler(IAsyncPolicy<HttpResponseMessage> retryPolicy)
		{
			_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		}

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation) =>
			_retryPolicy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellation);
	}
}
