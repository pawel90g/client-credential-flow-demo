using Shared;
using Shared.IoC;

var builder = WebApplication.CreateBuilder(args);

const string ServiceA_AuthZ_Policy = "service-a-authz";

builder.Services.AddTelemetry("ServiceA");

builder.Services.AddB2CAuthenticationWithJwt(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
  .AddPolicy(ServiceA_AuthZ_Policy, policy =>
        policy.RequireRole(Roles.ServiceA_Read));

builder.Services.AddAzureKeyVault(builder.Configuration);
builder.Services.AddOAuthHttpClient(builder.Configuration);
builder.Services.RegisterAllServiceClients(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Hello from ServiceA!");
app.MapGet("/secret", () => "Hello from secret ServiceA!")
    .RequireAuthorization(ServiceA_AuthZ_Policy);

app.MapGet("/chain", async (IHttpClientFactory httpClientFactory) =>
    {
        var servicebClient = httpClientFactory.CreateClient("ServiceB");
        var resp = await servicebClient.GetAsync("/chain");

        if (!resp.IsSuccessStatusCode)
            return Results.StatusCode((int)resp.StatusCode);

        return Results.Ok(await resp.Content.ReadAsStringAsync());
    });

app.Run();
