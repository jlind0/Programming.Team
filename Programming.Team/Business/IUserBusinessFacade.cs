using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business.Core
{
    public interface IUserBusinessFacade : IBusinessRepositoryFacade<User, Guid>
    {
        Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default);
        Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface IRoleBusinessFacade : IBusinessRepositoryFacade<Role, Guid>
    {
        Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default);
        Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface ISectionTemplateBusinessFacade : IBusinessRepositoryFacade<SectionTemplate, Guid>
    {
        Task<SectionTemplate[]> GetBySection(ResumePart sectionId, IUnitOfWork? work = null, CancellationToken token = default);
        Task<SectionTemplate?> GetDefaultSection(ResumePart sectionId, IUnitOfWork? work = null, string defaultName = "Default", CancellationToken token = default);
    }
    public interface ISkillsBusinessFacade : IBusinessRepositoryFacade<Skill, Guid>
    {
        Task<Skill[]> GetSkillsExcludingPosition(Guid positionId, IUnitOfWork? work = null,
            CancellationToken token = default);
    }
}
