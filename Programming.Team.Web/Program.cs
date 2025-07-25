using Azure.Communication.Email;
using Azure.Storage.Blobs;
using Invio.Extensions.Authentication.JwtBearer;
using Markdig;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ModelContextProtocol.Client;
using MudBlazor.Services;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Programming.Team.AI;
using Programming.Team.AI.Core;
using Programming.Team.Business;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data;
using Programming.Team.Data.Core;
using Programming.Team.Messaging;
using Programming.Team.Messaging.Core;
using Programming.Team.PurchaseManager;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.Templating;
using Programming.Team.Templating.Core;
using Programming.Team.ViewModels;
using Programming.Team.ViewModels.Admin;
using Programming.Team.ViewModels.Purchase;
using Programming.Team.ViewModels.Recruiter;
using Programming.Team.ViewModels.Resume;
using Programming.Team.Web.Authorization;
using Programming.Team.Web.Shared;
using Stripe;
using Stripe.Checkout;
using System.ClientModel;
using System.Collections.ObjectModel;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddSource("*")
    .AddOtlpExporter(config =>
    {
        config.Endpoint = new Uri(builder.Configuration["MCP:Host"] ?? throw new InvalidDataException());
    })
    .Build();
using var metricsProvider = Sdk.CreateMeterProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddMeter("*")
    .AddOtlpExporter(config =>
    {
        config.Endpoint = new Uri(builder.Configuration["MCP:Host"] ?? throw new InvalidDataException());
    })
    .Build();


// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearerQueryStringAuthentication()
    .AddMicrosoftIdentityWebApi(builder.Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddSessionTokenCaches();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
   .AddMicrosoftIdentityWebApp(options =>
   {
       builder.Configuration.Bind("AzureAd", options);
       options.Events.OnSignedOutCallbackRedirect = context =>
       {
           context.HttpContext.Session.Clear();
           context.Response.Redirect("/");
           context.HandleResponse(); // Suppress the default behavior of redirecting to the original URL
           return Task.CompletedTask;
       };
       options.Events.OnRemoteFailure = context =>
       {
           if (context.Failure.Message.Contains("AADB2C90118"))
           {
               // Redirect to Password Reset policy
               var resetPasswordUrl = "https://progteamgroundbreaker.b2clogin.com/tfp/progteamgroundbreaker.onmicrosoft.com/B2C_1_pswreset/oauth2/v2.0/authorize"
                                    + $"?client_id={Uri.EscapeDataString(options.ClientId)}"
                                    + $"&redirect_uri=https://{context.Request.Host}{Uri.EscapeDataString(options.CallbackPath)}"
                                    + "&response_mode=query"
                                    + "&response_type=code"
                                    + "&scope=openid";
               context.Response.Redirect(resetPasswordUrl);
               context.HandleResponse();
           }
           else if (context.Failure.Message.Contains("State"))
           {
               context.Response.Redirect("/");
               context.HandleResponse();
           }
           return Task.CompletedTask;
       };
   }).EnableTokenAcquisitionToCallDownstreamApi();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddTokenAcquisition();
builder.Services.AddReverseProxy()
    .LoadFromMemory(
        routes: new[]
        {
            new RouteConfig
            {
                RouteId = "blog-route",
                ClusterId = "blog-cluster",
                Match = new RouteMatch(){
                    Path = "/blog/{**catchAll}" },
                Transforms = new[]
                {
                    // 1) Remove the `/blog` prefix so WP sees the path as `/whatever`
                    new Dictionary<string, string>
                    {
                        { "PathRemovePrefix", "/blog" }
                    },
                    // 2) Rewrite the request Host header
                    new Dictionary<string, string>
                    {
                        { "RequestHeader", "Host" },
                        { "Set", "blog.programming.team" }
                    }
                }

            },
            new RouteConfig
            {
                RouteId = "blog-media-route",
                ClusterId = "blog-media-cluster",
                Match = new RouteMatch(){
                    Path = "/wp-content/{**catchAll}" },
                Transforms = new[]
                {
                    // 2) Rewrite the request Host header
                    new Dictionary<string, string>
                    {
                        { "RequestHeader", "Host" },
                        { "Set", "blog.programming.team" }
                    }
                }

            }
        },
        clusters: new[]
        {
            new ClusterConfig
            {
                ClusterId = "blog-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    // The destination is the WordPress subdomain:
                    {
                        "blog-dest", new DestinationConfig
                        {
                            Address = "https://blog.programming.team/"
                        }
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = "blog-media-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    // The destination is the WordPress subdomain:
                    {
                        "blog-dest", new DestinationConfig
                        {
                            Address = "https://blog.programming.team/"
                        }
                    }
                }
            }
        });
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(10); // Adjust as needed
})
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // 10 MB
    })
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddMudServices();
builder.Services.AddBlazorBootstrap();
builder.Services.AddKeyedSingleton("sselogging", LoggerFactory.Create(builder => builder.AddOpenTelemetry(opt => opt.AddOtlpExporter())));
builder.Services.AddTransient<OpenAIClient>(provider => new OpenAIClient(new ApiKeyCredential(builder.Configuration["ChatGPT:ApiKey"] ?? throw new InvalidDataException()), new OpenAIClientOptions()
{
    Endpoint = new Uri(builder.Configuration["ChatGPT:EndPoint"] ?? throw new InvalidDataException())
}));
builder.Services.AddTransient(provider =>
{
    return provider.GetRequiredService<OpenAIClient>().GetChatClient("gpt-4.1-mini").AsIChatClient()
        .AsBuilder()
        .UseOpenTelemetry(loggerFactory: provider.GetRequiredKeyedService<ILoggerFactory>("sselogging"), configure: o => o.EnableSensitiveData = true)
        
        .Build() as IChatClient;
});
builder.Services.AddTransient(provider =>
{
    var client = provider.GetRequiredService<IChatClient>();
    var task = McpClientFactory.CreateAsync(
        new SseClientTransport(new SseClientTransportOptions
        {
            Name = builder.Configuration["MCP:Name"] ?? throw new InvalidDataException(),
            Endpoint = new Uri(builder.Configuration["MCP:Host"] ?? throw new InvalidDataException())
        }),
    clientOptions: new()
    {
        Capabilities = new()
        {
            Sampling = new()
            {
                SamplingHandler =
            client.CreateSamplingHandler()
            }
        },
    }, provider.GetRequiredKeyedService<ILoggerFactory>("sselogging"), cancellationToken: CancellationToken.None);
    task.Wait();
   var s = task.Result;
   return s;
});
StripeConfiguration.ApiKey = builder.Configuration["Stripe:APIKey"];
builder.Services.AddTransient<PriceService>();
builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<PaymentLinkService>();
builder.Services.AddTransient<SessionService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<PayoutService>();
builder.Services.AddTransient<AccountLinkService>();
builder.Services.AddScoped(provider =>
{
    return new EmailClient(builder.Configuration["ACS:EmailConnectionString"] ??
        throw new InvalidDataException("ACS Email Connection String is not configured."));
});
builder.Services.AddScoped<IEmailMessaging, EmailMessaging>();
builder.Services.AddScoped(provider =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("ResumesBlob")));
var connectionString = builder.Configuration.GetConnectionString("Resumes");
builder.Services.AddDbContext<ResumesContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddScoped<IContextFactory, ContextFactory>();
builder.Services.AddScoped<IResumeBlob, ResumeBlob>();
builder.Services.AddScoped<INLP, NLP>();
builder.Services.AddScoped<IRepository<Role, Guid>, Repository<Role, Guid>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRepository<Company, Guid>, Repository<Company, Guid>>();
builder.Services.AddScoped<IRepository<Recommendation, Guid>, Repository<Recommendation, Guid>>();
builder.Services.AddScoped<IRepository<Position, Guid>, Repository<Position, Guid>>();
builder.Services.AddScoped<IRepository<PositionSkill, Guid>, Repository<PositionSkill, Guid>>();
builder.Services.AddScoped<IRepository<Skill, Guid>, SkillsRepository>();
builder.Services.AddScoped<IRepository<FAQ, Guid>, Repository<FAQ, Guid>>();
builder.Services.AddScoped<IRepository<DocumentType, DocumentTypes>, Repository<DocumentType, DocumentTypes>>();
builder.Services.AddScoped<IRepository<Education, Guid>, Repository<Education, Guid>>();
builder.Services.AddScoped<IRepository<DocumentTemplate, Guid>, DocumentTemplateRepository>();
builder.Services.AddScoped<IRepository<DocumentTemplatePurchase, Guid>, Repository<DocumentTemplatePurchase, Guid>>();
builder.Services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IRepository<DocumentSectionTemplate, Guid>, Repository<DocumentSectionTemplate, Guid>>();
builder.Services.AddScoped<IRepository<Institution, Guid>, Repository<Institution, Guid>>();
builder.Services.AddScoped<IRepository<Certificate, Guid>, Repository<Certificate, Guid>>();
builder.Services.AddScoped<IRepository<CertificateIssuer, Guid>, Repository<CertificateIssuer, Guid>>();
builder.Services.AddScoped<IRepository<Publication, Guid>, Repository<Publication, Guid>>();
builder.Services.AddScoped<IRepository<Package, Guid>, Repository<Package, Guid>>();
builder.Services.AddScoped<IRepository<Purchase, Guid>, Repository<Purchase, Guid>>();
builder.Services.AddScoped<IRepository<SectionTemplate, Guid>, SectionTemplateRepository>();
builder.Services.AddScoped<IRepository<Project, Guid>, Repository<Project, Guid>>();
builder.Services.AddScoped<IRepository<ProjectSkill, Guid>, Repository<ProjectSkill, Guid>>();
builder.Services.AddScoped<ISkillsRespository, SkillsRepository>();
builder.Services.AddScoped<ISectionTemplateRepository, SectionTemplateRepository>();
builder.Services.AddScoped<IRepository<EmailMessageTemplate, Guid>, Repository<EmailMessageTemplate, Guid>>();
builder.Services.AddScoped<ISkillsBusinessFacade, SkillsBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Role, Guid>, BusinessRepositoryFacade<Role, Guid, IRepository<Role, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Company, Guid>, BusinessRepositoryFacade<Company, Guid, IRepository<Company, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Position, Guid>, BusinessRepositoryFacade<Position, Guid, IRepository<Position, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Recommendation, Guid>, BusinessRepositoryFacade<Recommendation, Guid, IRepository<Recommendation, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<FAQ, Guid>, BusinessRepositoryFacade<FAQ, Guid, IRepository<FAQ, Guid>>>();
builder.Services.AddScoped<IUserBusinessFacade, UserBusinessFacade>();
builder.Services.AddScoped<IRoleBusinessFacade, RoleBusinessFacade>();
builder.Services.AddScoped<IPageBusinessFacade, PageBusinessFacade>();
builder.Services.AddScoped<IRepository<Posting, Guid>, PostingRepository>();
builder.Services.AddScoped<IBusinessRepositoryFacade<PositionSkill, Guid>, BusinessRepositoryFacade<PositionSkill, Guid, IRepository<PositionSkill, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Skill, Guid>, SkillsBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Education, Guid>, BusinessRepositoryFacade<Education, Guid, IRepository<Education, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Institution, Guid>, BusinessRepositoryFacade<Institution, Guid, IRepository<Institution, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Certificate, Guid>, BusinessRepositoryFacade<Certificate, Guid, IRepository<Certificate, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<CertificateIssuer, Guid>, BusinessRepositoryFacade<CertificateIssuer, Guid, IRepository<CertificateIssuer, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Publication, Guid>, BusinessRepositoryFacade<Publication, Guid, IRepository<Publication, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentTemplate, Guid>, DocumentTemplateBusinessFacade>();
builder.Services.AddScoped<IDocumentTemplateBusinessFacade, DocumentTemplateBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentType, DocumentTypes>, BusinessRepositoryFacade<DocumentType, DocumentTypes, IRepository<DocumentType, DocumentTypes>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Posting, Guid>, PostingBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Purchase, Guid>, BusinessRepositoryFacade<Purchase, Guid, IRepository<Purchase, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Package, Guid>, PackageBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<SectionTemplate, Guid>, SectionTemplateBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentSectionTemplate, Guid>, BusinessRepositoryFacade<DocumentSectionTemplate, Guid, IRepository<DocumentSectionTemplate, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentTemplatePurchase, Guid>, BusinessRepositoryFacade<DocumentTemplatePurchase, Guid, IRepository<DocumentTemplatePurchase, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Project, Guid>, BusinessRepositoryFacade<Project, Guid, IRepository<Project, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<ProjectSkill, Guid>, BusinessRepositoryFacade<ProjectSkill, Guid, IRepository<ProjectSkill, Guid>>>();
builder.Services.AddScoped<ISectionTemplateBusinessFacade, SectionTemplateBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>, BusinessRepositoryFacade<EmailMessageTemplate, Guid, IRepository<EmailMessageTemplate, Guid>>>();
builder.Services.AddScoped<IPurchaseManager<Package, Purchase>, PackagePurchaseManager>();
builder.Services.AddScoped<IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase>, DocumentTemplatePurchaseManager>();
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IResumeEnricher, ResumeEnricher>();
builder.Services.AddScoped<IDocumentTemplator, DocumentTemplator>();
builder.Services.AddScoped<IResumeBuilder, ResumeBuilder>();
builder.Services.AddTransient<AlertView.AlertViewModel>();
builder.Services.AddTransient<ResumeConfigurationViewModel>();
builder.Services.AddTransient<UserBarLoaderViewModel>();
builder.Services.AddTransient<AddRoleViewModel>();
builder.Services.AddTransient<ManageRolesViewModel>();
builder.Services.AddTransient<UsersViewModel>();
builder.Services.AddTransient<SelectUsersViewModel>();
builder.Services.AddTransient<RoleLoaderViewModel>();
builder.Services.AddTransient<AddCompanyViewModel>();
builder.Services.AddTransient<SearchSelectCompanyViewModel>();
builder.Services.AddTransient<AddPositionViewModel>();
builder.Services.AddTransient<PositionsViewModel>();
builder.Services.AddTransient<AddSkillViewModel>();
builder.Services.AddTransient<SearchSelectSkillViewModel>();
builder.Services.AddTransient<AddPositionSkillViewModel>();
builder.Services.AddTransient<PositionSkillsViewModel>();
builder.Services.AddTransient<AddInstitutionViewModel>();
builder.Services.AddTransient<SearchSelectInstiutionViewModel>();
builder.Services.AddTransient<AddEducationViewModel>();
builder.Services.AddTransient<EducationsViewModel>();
builder.Services.AddTransient<SearchSelectPositionViewModel>();
builder.Services.AddTransient<AddRecommendationViewModel>();
builder.Services.AddTransient<RecommendationsViewModel>();
builder.Services.AddTransient<SearchSelectCertificateIssuerViewModel>();
builder.Services.AddTransient<AddCertificateIssuerViewModel>();
builder.Services.AddTransient<AddCertificateViewModel>();
builder.Services.AddTransient<CertificatesViewModel>();
builder.Services.AddTransient<AddPublicationViewModel>();
builder.Services.AddTransient<PublicationsViewModel>();
builder.Services.AddTransient<AddDocumentTypeViewModel>();
builder.Services.AddTransient<DocumentTypesViewModel>();
builder.Services.AddTransient<AddDocumentTemplateViewModel>();
builder.Services.AddTransient<DocumentTemplatesViewModel>();
builder.Services.AddTransient<UserProfileLoaderViewModel>();
builder.Services.AddTransient<PostingsViewModel>();
builder.Services.AddTransient<PostingLoaderViewModel>();
builder.Services.AddTransient<ResumeBuilderViewModel>();
builder.Services.AddTransient<ImpersonatorViewModel>();
builder.Services.AddTransient<AcceptRecruiterViewModel>();
builder.Services.AddTransient<RecruitersViewModel>();
builder.Services.AddTransient<RecruitsViewModel>();
builder.Services.AddTransient<PurchaseHistoryViewModel>();
builder.Services.AddTransient<GlobalPurchaseHistoryViewModel>();
builder.Services.AddTransient<AddPackageViewModel>();
builder.Services.AddTransient<PackagesViewModel>();
builder.Services.AddTransient<AddSectionTemplateViewModel>();
builder.Services.AddTransient<SectionTemplatesViewModel>();
builder.Services.AddTransient<IndexViewModel>();
builder.Services.AddTransient<SuggestAddSkillsForPositionViewModel>();
builder.Services.AddTransient<CoverLetterConfigurationViewModel>();
builder.Services.AddTransient<Programming.Team.ViewModels.Admin.TrueUserLoaderViewModel>();
builder.Services.AddTransient<Programming.Team.ViewModels.Resume.TrueUserLoaderViewModel>();
builder.Services.AddTransient<SelectSectionTemplatesViewModel>();
builder.Services.AddTransient<UserStripeAccountViewModel>();
builder.Services.AddTransient<AddEmailMessageTemplateViewModel>();
builder.Services.AddTransient<EmailMessageTemplatesViewModel>();
builder.Services.AddTransient<AddFAQViewModel>();
builder.Services.AddTransient<FAQsViewModel>();
builder.Services.AddTransient<AddProjectViewModel>();
builder.Services.AddTransient<ProjectsViewModel>();
builder.Services.AddTransient<AddProjectSkillViewModel>();
builder.Services.AddTransient<SuggestAddSkillsForProjectViewModel>();
builder.Services.AddTransient<ProjectSkillsViewModel>();
builder.Services.AddTransient<LayoutViewModel>();
builder.Services.AddTransient<AddPageViewModel>();
builder.Services.AddTransient<PagesViewModel>();

builder.Services.AddTransient<ResumeWizardViewModel>();
builder.Services.AddSingleton(provider =>
{
    return new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();
});
builder.Services.AddSingleton(provider =>
{
    var config = new ReverseMarkdown.Config
    {
        GithubFlavored = true,
        RemoveComments = true,
        CleanupUnnecessarySpaces = true,
    };

    return new ReverseMarkdown.Converter(config);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // For GDPR compliance
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use HTTPS
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<LinkRewritingMiddleware>();
app.UseMiddleware<RolePopulationMiddleware>();
app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
