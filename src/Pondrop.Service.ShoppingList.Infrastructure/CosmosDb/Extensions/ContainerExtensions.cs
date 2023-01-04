using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Pondrop.Service.Store.Infrastructure.CosmosDb;

public static class ContainerExtensions
{
    public static async Task EnsureStoreProcedures(this Container container, DirectoryInfo storedProceduresDirectory)
    {
        if (!storedProceduresDirectory.Exists)
            return;

        foreach (var fi in storedProceduresDirectory.EnumerateFiles().Where(i => i.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase)))
        {
            var name = fi.Name[..^3];
            var spBody = await File.ReadAllTextAsync(fi.FullName);
                
            try
            {
                var spResp = await container.Scripts.ReadStoredProcedureAsync(name);

                using var sha1 = SHA1.Create();
                    
                var currentBytes = Encoding.UTF8.GetBytes(spBody);
                var newBytes = Encoding.UTF8.GetBytes(spResp.Resource.Body);
                    
                if (!sha1.ComputeHash(currentBytes).SequenceEqual(sha1.ComputeHash(newBytes)))
                {
                    spResp.Resource.Body = spBody;
                    await container.Scripts.ReplaceStoredProcedureAsync(spResp.Resource);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create stored procedure
                var storedProcedureProperties = new StoredProcedureProperties
                {
                    Id = name,
                    Body = spBody
                };

                await container.Scripts.CreateStoredProcedureAsync(storedProcedureProperties);
            }
        }
    }
}
