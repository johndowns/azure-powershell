﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Azure.Common.Authentication
{
    /// <summary>
    /// Interface to the certificate store for authentication
    /// </summary>
    internal sealed class CertificateApplicationCredentialProvider : IApplicationAuthenticationProvider
    {
        private string _certificateThumbprint;
        private string _certificatePassword;
        private Func<string, byte[]> _certificateMap;

        /// <summary>
        /// Create a certificate provider
        /// </summary>
        /// <param name="certificateThumbprint"></param>
        /// <param name="certificatePassword"></param>
        /// <param name="certificateMap"></param>
        public CertificateApplicationCredentialProvider(string certificateThumbprint, string certificatePassword, Func<string, byte[]> certificateMap )
        {
            this._certificateThumbprint = certificateThumbprint;
            this._certificatePassword = certificatePassword;
            _certificateMap = certificateMap;
        }
        
        /// <summary>
        /// Authenticate using certificate thumbprint from the datastore 
        /// </summary>
        /// <param name="clientId">The active directory client id for the application.</param>
        /// <param name="audience">The intended audience for authentication</param>
        /// <param name="context">The AD AuthenticationContext to use</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> AuthenticateAsync(
            string clientId, 
            string audience, 
            AuthenticationContext context)
        {
            var task = new Task<byte[]>(() =>
            {
                return  _certificateMap(this._certificateThumbprint);
            });
            task.Start();
            var certificate = await task.ConfigureAwait(false);
            return await context.AcquireTokenAsync(
                audience,
                new ClientAssertionCertificate(clientId, certificate, this._certificatePassword));
        }
    }
}