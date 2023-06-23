using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.IoC;

var builder = WebApplication.CreateBuilder(args);

const string ServiceB_AuthZ_Policy = "service-b-authz";

builder.Services.AddB2CAuthenticationWithJwt(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
  .AddPolicy(ServiceB_AuthZ_Policy, policy =>
        policy.RequireRole(Roles.ServiceB_Read));

builder.Services.AddAzureKeyVault(builder.Configuration);
builder.Services.AddOAuthHttpClient(builder.Configuration);
builder.Services.RegisterAllServiceClients(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Hello from ServiceB!");
app.MapGet("/secret", () => "Hello from secret ServiceB!")
    .RequireAuthorization(ServiceB_AuthZ_Policy);

app.MapGet("/chain", async (IHttpClientFactory httpClientFactory) =>
    {
        var servicebClient = httpClientFactory.CreateClient("ServiceC");
        var resp = await servicebClient.GetAsync("/secret");

        if(!resp.IsSuccessStatusCode)
            return Results.StatusCode((int)resp.StatusCode);
            
        return Results.Ok(await resp.Content.ReadAsStringAsync());
    })
    .RequireAuthorization(ServiceB_AuthZ_Policy);

app.Run();


