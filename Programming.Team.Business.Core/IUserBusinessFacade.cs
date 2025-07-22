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
    /// <summary>
    /// User Business Facade
    /// </summary>
    public interface IUserBusinessFacade : IBusinessRepositoryFacade<User, Guid>
    {
        /// <summary>
        /// Get user by Azure AD B2C object id
        /// </summary>
        /// <param name="objectId">Target Azure AD B2C Object Id</param>
        /// <param name="work">Optional unit of work</param>
        /// <param name="properties">Optional User Properites to load</param>
        /// <param name="token">Optional cancellation token</param>
        /// <returns>User, if found</returns>
        Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default);
        /// <summary>
        /// Add a recruiter to a user
        /// </summary>
        /// <param name="targetUserId">Target User.Id</param>
        /// <param name="recruiterId">Recruiter's User.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task representing work of adding a recruiter</returns>
        Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        /// <summary>
        /// Remove a recruiter from a user
        /// </summary>
        /// <param name="targetUserId">Target User.Id</param>
        /// <param name="recruiterId">Recruiter User.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task representing work of removing a recruiter</returns>
        Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default);
        /// <summary>
        /// Utilize a resume generation, will decrement User.ResumeGenerationsLeft if user is not an Admin
        /// </summary>
        /// <param name="userId">Target User.Id</param>
        /// <param name="work">optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Boolean indicating if the resume generation can be executed</returns>
        Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default);
    }
    /// <summary>
    /// Role Business Facade
    /// </summary>
    public interface IRoleBusinessFacade : IBusinessRepositoryFacade<Role, Guid>
    {
        /// <summary>
        /// Get User Ids for a Role
        /// </summary>
        /// <param name="roleId">Target Role.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Array of User.Id's</returns>
        Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default);
        /// <summary>
        /// Set selected users to a role
        /// </summary>
        /// <param name="roleId">Target Role.Id</param>
        /// <param name="userIds">Target User.Id's</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task represent work of assigning roles</returns>
        Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default);
    }
    /// <summary>
    /// Section Template Business Facade
    /// </summary>
    public interface ISectionTemplateBusinessFacade : IBusinessRepositoryFacade<SectionTemplate, Guid>
    {
        Task<SectionTemplate[]> GetBySection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default);
        Task<SectionTemplate?> GetDefaultSection(ResumePart sectionId, Guid documentTemplateId, IUnitOfWork? work = null, CancellationToken token = default);
    }
    /// <summary>
    /// Skills Business Facade
    /// </summary>
    public interface ISkillsBusinessFacade : IBusinessRepositoryFacade<Skill, Guid>
    {
        /// <summary>
        /// Get Skills excluding those already assigned to a given Position
        /// </summary>
        /// <param name="positionId">Target Position.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Array of Skills not assigned to the given position but exist for that user</returns>
        Task<Skill[]> GetSkillsExcludingPosition(Guid positionId, IUnitOfWork? work = null,
            CancellationToken token = default);
        Task<Skill[]> GetSkillsExcludingProject(Guid projectId, IUnitOfWork? work = null, CancellationToken token = default);
    }

    public interface IDocumentTemplateBusinessFacade : IBusinessRepositoryFacade<DocumentTemplate, Guid>
    {
        Task<DocumentTemplate[]> GetForUser(Guid userId, DocumentTypes type = DocumentTypes.Resume, IUnitOfWork? work = null, CancellationToken token = default);
    }
    public interface IPageBusinessFacade : IBusinessRepositoryFacade<Page, Guid>
    {
        Task<Page?> GetByRoute(string route, IUnitOfWork? work = null, CancellationToken token = default);
    }
}
