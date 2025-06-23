using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Data
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }
        public async Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await Use(async (w, t) =>
            {
                var user = await w.ResumesContext.Users.Include(c => c.Recruiters).SingleAsync(w => w.Id == targetUserId);
                var updateUserId = await GetCurrentUserId(w, true, t);
                user.UpdateDate = DateTime.UtcNow;
                user.UpdatedByUserId = updateUserId;
                user.Recruiters.Add(await w.ResumesContext.Users.SingleAsync(w => w.Id == recruiterId));
            }, work, token, true);
        }
        public async Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await Use(async (w, t) =>
            {
                var user = await w.ResumesContext.Users.Include(c => c.Recruiters).SingleAsync(w => w.Id == targetUserId);
                var updateUserId = await GetCurrentUserId(w, true, t);
                user.UpdateDate = DateTime.UtcNow;
                user.UpdatedByUserId = updateUserId;
                user.Recruiters.Remove(user.Recruiters.Single(w => w.Id == recruiterId));
            }, work, token, true);
        }
        public async Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default)
        {
            User? user = null;
            await Use(async (w, t) =>
            {
                var query = w.ResumesContext.Users.AsQueryable();
                if (properties != null)
                    query = query.Include(properties);
                user = await query.SingleOrDefaultAsync(x => x.ObjectId == objectId);
            }, work, token);
            return user;
        }

        public async Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            bool canGenerate = false;
            await Use(async (w, t) =>
            {
                var user = await w.ResumesContext.Users.FindAsync(userId, token);
                if (user != null)
                {
                    canGenerate = user.ResumeGenerationsLeft > 0;
                    if (canGenerate)
                    {
                        user.ResumeGenerationsLeft--;
                        w.Context.Update(user);
                    }
                }
            }, work, token, true);
            return canGenerate;
        }
    }
    public class RoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public RoleRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }

        public async Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            Guid[] userIds = [];
            await Use(async (w, t) =>
            {
                userIds = await w.ResumesContext.Users.Where(u => u.Roles.Any(r => r.Id == roleId)).Select(u => u.Id).ToArrayAsync(token);
            }, work, token);
            return userIds;
        }

        public async Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await Use(async (w, t) =>
            {
                var role = await w.ResumesContext.Roles.Include(c => c.Users).SingleOrDefaultAsync(w => w.Id == roleId);
                if (role != null)
                {

                    var userId = await GetCurrentUserId(w, true, token: t);
                    role.Users.Clear();
                    role.UpdateDate = DateTime.UtcNow;
                    role.UpdatedByUserId = userId;
                    foreach (var id in userIds)
                    {
                        role.Users.Add(await w.ResumesContext.Users.SingleAsync(w => w.Id == id));
                    }

                }
            }, work, token, true);
        }
    }
    public class SectionTemplateRepository : Repository<SectionTemplate, Guid>, ISectionTemplateRepository
    {
        public SectionTemplateRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }
        public async Task<SectionTemplate[]> GetBySection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            SectionTemplate[] templates = [];
            await Use(async (w, t) =>
            {
                templates = await w.ResumesContext.DocumentSectionTemplates.Where(d => d.DocumentTemplateId == documentTemplateId).Select(d => d.SectionTemplate)
                    .Where(s => s.SectionId == sectionId).ToArrayAsync(token);
            }, work, token);
            return templates;
        }

        public async Task<SectionTemplate?> GetDefaultSection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            SectionTemplate? template = null;
            await Use(async (w, t) =>
            {
                template = await w.ResumesContext.DocumentSectionTemplates.Where(d => d.DocumentTemplateId == documentTemplateId && d.IsDefault)
                    .Select(d => d.SectionTemplate).SingleOrDefaultAsync(s => s.SectionId == sectionId, token);
            }, work, token);
            return template;
        }
    }
    public class SkillsRepository : Repository<Skill, Guid>, ISkillsRespository
    {
        public SkillsRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }
        public virtual async Task<Skill[]> GetSkillsExcludingPosition(Guid positionId, IUnitOfWork? work = null,
            CancellationToken token = default)
        {
            Skill[] skills = [];
            await Use(async (w, t) =>
            {
                var userId = await GetCurrentUserId(w, token: t);
                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
                var query = w.ResumesContext.Skills.Where(s =>
                    s.PositionSkills.Any(ps => ps.Position.UserId == userId && ps.PositionId != positionId)).Except(
                        w.ResumesContext.Skills.Where(s => s.PositionSkills.Any(ps => ps.PositionId == positionId))).Distinct();
                query = query.OrderByDescending(s => s.PositionSkills.Sum(
                    ps => EF.Functions.DateDiffDay(ps.Position.StartDate, ps.Position.EndDate ?? today)));
                skills = await query.ToArrayAsync(token);
            }, work, token);
            return skills;
        }
    }
    public class DocumentTemplateRepository : Repository<DocumentTemplate, Guid>, IDocumentTemplateRepository
    {
        public DocumentTemplateRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }
        public async Task<DocumentTemplate[]> GetForUser(Guid userId, DocumentTypes type = DocumentTypes.Resume, IUnitOfWork? work = null, CancellationToken token = default)
        {
            DocumentTemplate[] templates = [];
            await Use(async (w, t) =>
            {
                templates = await w.ResumesContext.DocumentTemplates
                 .Where(d => d.DocumentTypeId == type && (
                     (d.OwnerId == null || d.OwnerId == userId) ||
                     w.ResumesContext.DocumentTemplatePurchases
                         .Any(p => p.UserId == userId && p.IsPaid &&
                                   p.DocumentTemplateId == d.Id &&
                                   p.DocumentTemplate.ApprovalStatus == ApprovalStatus.Approved)))
                 .ToArrayAsync(token);
            }, work, token);
            return templates;
        }
    }
    public class PostingRepository : Repository<Posting, Guid>
    {
        public PostingRepository(IContextFactory contextFactory, IMemoryCache cache) : base(contextFactory, cache)
        {
        }
        public override async Task<RepositoryResultSet<Guid, Posting>> Get(IUnitOfWork? work = null, Pager? page = null, Expression<Func<Posting, bool>>? filter = null, Func<IQueryable<Posting>, IOrderedQueryable<Posting>>? orderBy = null, Func<IQueryable<Posting>, IQueryable<Posting>>? properites = null, CancellationToken token = default)
        {
            RepositoryResultSet<Guid, Posting> res = new RepositoryResultSet<Guid, Posting>();
            await Use(async (w, t) =>
            {
                var query = w.ResumesContext.Postings.AsQueryable();
                if (filter != null)
                    query = query.Where(filter);
                if (properites != null)
                    query = properites(query);
                if (orderBy != null)
                    query = orderBy(query);
                if (page != null)
                {
                    res.Count = await query.CountAsync(t);
                    int skip = page.Value.Size * (page.Value.Page - 1);
                    int take = page.Value.Size;
                    query = query.Skip(skip).Take(take);
                }
                
                query = query.Select(x => new Posting
                {
                    Id = x.Id,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    Name = x.Name,
                    DocumentTemplateId = x.DocumentTemplateId,
                    DocumentTemplate = x.DocumentTemplate,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedByUserId = x.UpdatedByUserId,
                    IsDeleted = x.IsDeleted,
                    UserId = x.UserId,
                    Details = x.Details
                });
                var data = await query.ToArrayAsync(t);
                res.Entities = data;
                if (page == null)
                    res.Count = data.Length;
            });
            return res;
        }
    }
}