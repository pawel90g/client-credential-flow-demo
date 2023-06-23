using Shared;
using Shared.IoC;

var builder = WebApplication.CreateBuilder(args);

const string ServiceC_AuthZ_Policy = "service-c-authz";

builder.Services.AddB2CAuthenticationWithJwt(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
  .AddPolicy(ServiceC_AuthZ_Policy, policy =>
        policy.RequireRole(Roles.ServiceC_Read));

builder.Services.AddAzureKeyVault(builder.Configuration);
builder.Services.AddOAuthHttpClient(builder.Configuration);
builder.Services.RegisterAllServiceClients(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Hello from ServiceC!");
app.MapGet("/secret", () => "Hello from secret ServiceC!")
    .RequireAuthorization(ServiceC_AuthZ_Policy);

app.Run();
