// <copyright file="TestZipkinExporter.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Globalization;

namespace ConsoleApp6
{
    internal class InstrumentationWithActivitySource : IDisposable
    {
        private const string RequestPath = "/api/request";
        private SampleServer server = new SampleServer();
        private SampleClient client = new SampleClient();

        public void Start(ushort port = 19999)
        {
            var url = $"http://localhost:{port.ToString(CultureInfo.InvariantCulture)}{RequestPath}/";
            this.server.Start(url);
            this.client.Start(url);
        }

        public void Dispose()
        {
            this.client.Dispose();
            this.server.Dispose();
        }
    }
}