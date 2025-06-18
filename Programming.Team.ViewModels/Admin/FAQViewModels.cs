using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class AddFAQViewModel : AddEntityViewModel<Guid, FAQ>, IFAQ
    {
        public AddFAQViewModel(IBusinessRepositoryFacade<FAQ, Guid> facade, ILogger<AddEntityViewModel<Guid, FAQ, IBusinessRepositoryFacade<FAQ, Guid>>> logger) : base(facade, logger)
        {
        }
        private string question = string.Empty;
        private string answer = string.Empty;
        private string? sortOrder;

        // Properties with Notification
        public string Question
        {
            get => question;
            set => this.RaiseAndSetIfChanged(ref question, value);
        }

        public string Answer
        {
            get => answer;
            set => this.RaiseAndSetIfChanged(ref answer, value);
        }

        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }

        protected override Task Clear()
        {
            Question = string.Empty;
            Answer = string.Empty;
            SortOrder = null;
            return Task.CompletedTask;
        }

        protected override Task<FAQ> ConstructEntity()
        {
            return Task.FromResult(new FAQ()
            {
                Question = Question,
                Answer = Answer,
                SortOrder = SortOrder
            });
        }
    }
    public class FAQViewModel : EntityViewModel<Guid, FAQ>, IFAQ
    {
        private string question = string.Empty;
        private string answer = string.Empty;
        private string? sortOrder;

        public FAQViewModel(ILogger logger, IBusinessRepositoryFacade<FAQ, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public FAQViewModel(ILogger logger, IBusinessRepositoryFacade<FAQ, Guid> facade, FAQ entity) : base(logger, facade, entity)
        {
        }

        // Properties with Notification
        public string Question
        {
            get => question;
            set => this.RaiseAndSetIfChanged(ref question, value);
        }

        public string Answer
        {
            get => answer;
            set => this.RaiseAndSetIfChanged(ref answer, value);
        }

        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }

        internal override Task<FAQ> Populate()
        {
            return Task.FromResult(new FAQ()
            {
                Question = question,
                Answer = answer,
                SortOrder = sortOrder,
                Id = Id
            });
        }

        internal override Task Read(FAQ entity)
        {
            Id = entity.Id;
            Question = entity.Question;
            Answer = entity.Answer;
            SortOrder = entity.SortOrder;
            return Task.CompletedTask;
        }
    }
    public class FAQsViewModel : EntitiesDefaultViewModel<Guid, FAQ, FAQViewModel, AddFAQViewModel>
    {
        public FAQsViewModel(AddFAQViewModel addViewModel, IBusinessRepositoryFacade<FAQ, Guid> facade, ILogger<EntitiesViewModel<Guid, FAQ, FAQViewModel, IBusinessRepositoryFacade<FAQ, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<FAQ>, IOrderedQueryable<FAQ>>? OrderBy()
        {
            return e => e.OrderBy(x => x.SortOrder);
        }
        protected override Task<FAQViewModel> Construct(FAQ entity, CancellationToken token)
        {
            return Task.FromResult(new FAQViewModel(Logger, Facade, entity));
        }
    }
}
