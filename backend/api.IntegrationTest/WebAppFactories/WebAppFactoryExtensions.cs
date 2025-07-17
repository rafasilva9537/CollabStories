using System.Net.Http.Headers;
using api.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;

namespace api.IntegrationTests.WebAppFactories;

public static class WebAppFactoryExtensions
{
    public static HttpClient CreateClientWithAuth(
        this WebApplicationFactory<Program> factory, 
        string userName, 
        string nameIdentifier, 
        string email, 
        params string[] roles)
    {
        HttpClient client = factory.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameHeader, userName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameIdentifierHeader, nameIdentifier);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        foreach (string role in roles)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        }

        return client;
    }
    
    public static HubConnection CreateHubConnectionWithAuth(
        this WebApplicationFactory<Program> factory,
        string userName, 
        string nameIdentifier, 
        string email,
        params string[] roles
    )
    {
        TestServer server = factory.Server;
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(server.BaseAddress + UrlConstants.StoryHub, options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.Headers.Add("Authorization", TestAuthHandler.AuthenticationScheme);;
                options.Headers.Add(TestAuthHandler.NameHeader, userName);
                options.Headers.Add(TestAuthHandler.NameIdentifierHeader, nameIdentifier);
                options.Headers.Add(TestAuthHandler.EmailHeader, email);
                foreach (string role in roles)
                {
                    options.Headers.Add(TestAuthHandler.RoleHeader, role);
                }
            })
            .Build();
        
        return connection;
    }
}