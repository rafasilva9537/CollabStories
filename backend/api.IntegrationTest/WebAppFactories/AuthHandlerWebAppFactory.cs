using System.Net.Http.Headers;
using api.Constants;
using api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.WebAppFactories;

/// <summary>
/// AuthHandlerWebAppFactory is a specialized implementation of the CustomWebAppFactory that provides
/// support for integration tests requiring customized authentication configuration. This factory
/// is made for scenarios where test clients or SignalR Hub connections need to access
/// endpoints with authentication headers, without the need to handle tokens.
/// </summary>
public sealed class AuthHandlerWebAppFactory : CustomWebAppFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme,
                    options => { });
        });
    }
    
    public HttpClient CreateClientWithAuth(AppUser user, params string[] roles)
    {
        HttpClient client = CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameHeader, user.UserName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameIdentifierHeader, user.UserName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, user.Email);
        foreach (string role in roles)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        }

        return client;
    }
    
    public HubConnection CreateHubConnectionWithAuth(AppUser user, params string[] roles)
    {
        TestServer server = Server;
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(server.BaseAddress + UrlConstants.StoryHub, options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.Headers.Add("Authorization", TestAuthHandler.AuthenticationScheme);;
                options.Headers.Add(TestAuthHandler.NameHeader, user.UserName);
                options.Headers.Add(TestAuthHandler.NameIdentifierHeader, user.UserName);
                options.Headers.Add(TestAuthHandler.EmailHeader, user.UserName);
                foreach (string role in roles)
                {
                    options.Headers.Add(TestAuthHandler.RoleHeader, role);
                }
            })
            .Build();
        
        return connection;
    }
}