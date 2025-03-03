using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Invio.Extensions.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Programming.Team.Data.Core;
using Programming.Team.Data;
using Programming.Team.Core;
using Programming.Team.Business.Core;
using Programming.Team.Business;
using Microsoft.EntityFrameworkCore;
using Programming.Team.Web.Authorization;
using MudBlazor.Services;
using Programming.Team.Web.Shared;
using Programming.Team.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Programming.Team.ViewModels.Resume;
using Azure.Storage.Blobs;
using Programming.Team.AI.Core;
using Programming.Team.AI;
using Programming.Team.Templating.Core;
using Programming.Team.Templating;
using Programming.Team.ViewModels.Recruiter;
using Stripe;
using Stripe.Checkout;
using Programming.Team.ViewModels.Purchase;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.PurchaseManager;
using Programming.Team.ViewModels;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSincere();
StripeConfiguration.ApiKey = builder.Configuration["Stripe:APIKey"];
builder.Services.AddTransient<PriceService>();
builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<PaymentLinkService>();
builder.Services.AddTransient<SessionService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<PayoutService>();
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
builder.Services.AddScoped<IRepository<Reccomendation, Guid>, Repository<Reccomendation, Guid>>();
builder.Services.AddScoped<IRepository<Position, Guid>, Repository<Position, Guid>>();
builder.Services.AddScoped<IRepository<PositionSkill, Guid>, Repository<PositionSkill, Guid>>();
builder.Services.AddScoped<IRepository<Skill, Guid>, Repository<Skill, Guid>>();
builder.Services.AddScoped<IRepository<DocumentType, int>, Repository<DocumentType, int>>();
builder.Services.AddScoped<IRepository<Education, Guid>, Repository<Education, Guid>>();
builder.Services.AddScoped<IRepository<DocumentTemplate, Guid>, Repository<DocumentTemplate, Guid>>();
builder.Services.AddScoped<IRepository<Institution, Guid>, Repository<Institution, Guid>>();
builder.Services.AddScoped<IRepository<Certificate, Guid>, Repository<Certificate, Guid>>();
builder.Services.AddScoped<IRepository<CertificateIssuer, Guid>, Repository<CertificateIssuer, Guid>>();
builder.Services.AddScoped<IRepository<Publication, Guid>, Repository<Publication, Guid>>();
builder.Services.AddScoped<IRepository<Package, Guid>, Repository<Package, Guid>>();
builder.Services.AddScoped<IRepository<Purchase, Guid>, Repository<Purchase, Guid>>();
builder.Services.AddScoped<IRepository<SectionTemplate, Guid>, SectionTemplateRepository>();
builder.Services.AddScoped<ISectionTemplateRepository, SectionTemplateRepository>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Role, Guid>, BusinessRepositoryFacade<Role, Guid, IRepository<Role, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Company, Guid>, BusinessRepositoryFacade<Company, Guid, IRepository<Company, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Position, Guid>, BusinessRepositoryFacade<Position, Guid, IRepository<Position, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Reccomendation, Guid>, BusinessRepositoryFacade<Reccomendation, Guid, IRepository<Reccomendation, Guid>>>();
builder.Services.AddScoped<IUserBusinessFacade, UserBusinessFacade>();
builder.Services.AddScoped<IRoleBusinessFacade, RoleBusinessFacade>();
builder.Services.AddScoped<IRepository<Posting, Guid>, Repository<Posting, Guid>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<PositionSkill, Guid>, BusinessRepositoryFacade<PositionSkill, Guid, IRepository<PositionSkill, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Skill, Guid>, BusinessRepositoryFacade<Skill, Guid, IRepository<Skill, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Education, Guid>, BusinessRepositoryFacade<Education, Guid, IRepository<Education, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Institution, Guid>, BusinessRepositoryFacade<Institution, Guid, IRepository<Institution, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Certificate, Guid>, BusinessRepositoryFacade<Certificate, Guid, IRepository<Certificate, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<CertificateIssuer, Guid>, BusinessRepositoryFacade<CertificateIssuer, Guid, IRepository<CertificateIssuer, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Publication, Guid>, BusinessRepositoryFacade<Publication, Guid, IRepository<Publication, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentTemplate, Guid>, BusinessRepositoryFacade<DocumentTemplate, Guid, IRepository<DocumentTemplate, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentType, int>, BusinessRepositoryFacade<DocumentType, int, IRepository<DocumentType, int>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Posting, Guid>, PostingBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Purchase, Guid>, BusinessRepositoryFacade<Purchase, Guid, IRepository<Purchase, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Package, Guid>, PackageBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<SectionTemplate, Guid>, SectionTemplateBusinessFacade>();
builder.Services.AddScoped<ISectionTemplateBusinessFacade, SectionTemplateBusinessFacade>();
builder.Services.AddScoped<IPurchaseManager, PurhcaseManager>();
builder.Services.AddScoped<IChatGPT, ChatGPT>();
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
builder.Services.AddTransient<AddReccomendationViewModel>();
builder.Services.AddTransient<ReccomendationsViewModel>();
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
app.UseMiddleware<RolePopulationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
