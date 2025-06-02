using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Data.Core
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default);
        Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface IRoleRepository : IRepository<Role, Guid>
    {
        Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default);
        Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface ISectionTemplateRepository : IRepository<SectionTemplate, Guid>
    {
        Task<SectionTemplate[]> GetBySection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default);
        Task<SectionTemplate?> GetDefaultSection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface ISkillsRespository : IRepository<Skill, Guid>
    {
        Task<Skill[]> GetSkillsExcludingPosition(Guid positionId, IUnitOfWork? work = null,
            CancellationToken token = default);
    }
    public interface IDocumentTemplateRepository : IRepository<DocumentTemplate, Guid>
    {
        Task<DocumentTemplate[]> GetForUser(Guid userId, IUnitOfWork? work = null, CancellationToken token = default);
    }
}