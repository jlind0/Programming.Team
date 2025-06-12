using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Programming.Team.Business
{
    public class UserBusinessFacade : BusinessRepositoryFacade<User, Guid, IUserRepository>, IUserBusinessFacade
    {
        public UserBusinessFacade(IUserRepository repository, ILogger<User> logger) : base(repository, logger)
        {
        }

        public Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default)
        {
            return Repository.GetByObjectIdAsync(objectId, work, properties, token);
        }
        public Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.AddRecruiter(targetUserId, recruiterId, work, token);
        }

        public Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.RemoveRecruiter(targetUserId, recruiterId, work, token);
        }
        public override async Task<User> Update(User entity, IUnitOfWork? work = null, Func<IQueryable<User>, IQueryable<User>>? properites = null, CancellationToken token = default)
        {
            var user = await GetByID(entity.Id, work, token: token);
            entity.ResumeGenerationsLeft = user?.ResumeGenerationsLeft ?? throw new InvalidDataException();
            return await base.Update(entity, work, properites, token);
        }

        public async Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            var user = await GetByID(userId, properites: q => q.Include(e => e.Roles), work: work, token: token);
            if(!user!.Roles.Any(e => e.Name == "Admin"))
                return await Repository.UtilizeResumeGeneration(userId, work, token);
            return true;
        }
    }
    public class RoleBusinessFacade : BusinessRepositoryFacade<Role, Guid, IRoleRepository>, IRoleBusinessFacade
    {
        public RoleBusinessFacade(IRoleRepository repository, ILogger<Role> logger) : base(repository, logger)
        {
        }

        public Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetUserIds(roleId, work, token);
        }

        public Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.SetSelectedUsersToRole(roleId, userIds, work, token);
        }
    }
    public class PackageBusinessFacade : BusinessRepositoryFacade<Package, Guid>
    {
        protected IPurchaseManager<Package, Purchase> PurchaseManager { get; }
        public PackageBusinessFacade(IPurchaseManager<Package, Purchase> purchaseManager, IRepository<Package, Guid> repository, ILogger<Package> logger) : base(repository, logger)
        {
            PurchaseManager = purchaseManager;
        }
        public override async Task Add(Package entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await base.Add(entity, work, token);
            await PurchaseManager.ConfigurePackage(entity, token);
            await RepositoryDefault.Update(entity, work, token: token);
        }
        public override async Task<Package> Update(Package entity, IUnitOfWork? work = null, Func<IQueryable<Package>, IQueryable<Package>>? properites = null, CancellationToken token = default)
        {
            
            await PurchaseManager.ConfigurePackage(entity, token);
            return await base.Update(entity, work, properites, token);
        }
    }
    public class SectionTemplateBusinessFacade : BusinessRepositoryFacade<SectionTemplate, Guid, ISectionTemplateRepository>, ISectionTemplateBusinessFacade
    {
        public SectionTemplateBusinessFacade(ISectionTemplateRepository repository, ILogger<SectionTemplate> logger) : base(repository, logger)
        {
        }

        public Task<SectionTemplate[]> GetBySection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetBySection(sectionId, documentTemplateId, work, token);
        }


        public Task<SectionTemplate?> GetDefaultSection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetDefaultSection(sectionId, documentTemplateId, work, token);
        }
    }
    public class PostingBusinessFacade : BusinessRepositoryFacade<Posting, Guid>
    {
        public PostingBusinessFacade(IRepository<Posting, Guid> repository, ILogger<Posting> logger) : base(repository, logger)
        {
        }
        public override Task Add(Posting entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            entity.Details = Regex.Replace(entity.Details, "<.*?>", String.Empty);
            return base.Add(entity, work, token);
        }
        public override Task<Posting> Update(Posting entity, IUnitOfWork? work = null, Func<IQueryable<Posting>, IQueryable<Posting>>? properites = null, CancellationToken token = default)
        {
            entity.Details = Regex.Replace(entity.Details, "<.*?>", String.Empty);
            return base.Update(entity, work, properites, token);
        }
    }
    public class SkillsBusinessFacade : BusinessRepositoryFacade<Skill, Guid, ISkillsRespository>, ISkillsBusinessFacade
    {
        public SkillsBusinessFacade(ISkillsRespository repository, ILogger<Skill> logger) : base(repository, logger)
        {
        }
        public Task<Skill[]> GetSkillsExcludingPosition(Guid positionId, IUnitOfWork? work = null,
            CancellationToken token = default)
        {
            return Repository.GetSkillsExcludingPosition(positionId, work, token);
        }
    }
    public class DocumentTemplateBusinessFacade : BusinessRepositoryFacade<DocumentTemplate, Guid, IDocumentTemplateRepository>, IDocumentTemplateBusinessFacade
    {
        protected IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> PurchaseManager { get; }
        public DocumentTemplateBusinessFacade(IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> purchaseManager, IDocumentTemplateRepository repository, ILogger<DocumentTemplate> logger) : base(repository, logger)
        {
            PurchaseManager = purchaseManager;
        }
        public override async Task Add(DocumentTemplate entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await base.Add(entity, work, token);
            await ApplyPurchase(entity, work, token);
        }
        protected virtual async Task ApplyPurchase(DocumentTemplate entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            if (entity.OwnerId != null && entity.Price > 0)
                await PurchaseManager.ConfigurePackage(entity, token);
            else
            {
                entity.StripeProductId = null;
                entity.StripePriceId = null;
                entity.StripeUrl = null;
            }
        }
        public override async Task<DocumentTemplate> Update(DocumentTemplate entity, IUnitOfWork? work = null, Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>? properites = null, CancellationToken token = default)
        {
            await ApplyPurchase(entity, work, token);
            return await base.Update(entity, work, properites, token);
        }
        public Task<DocumentTemplate[]> GetForUser(Guid userId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetForUser(userId, work, token);
        }
    }
}
