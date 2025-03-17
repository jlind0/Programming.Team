using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class AddSectionTemplateViewModel : AddEntityViewModel<Guid, SectionTemplate>, ISectionTemplate
    {
        public ObservableCollection<ResumePart> ResumeParts { get; } = new ObservableCollection<ResumePart>(Enum.GetValues<ResumePart>());
        public AddSectionTemplateViewModel(IBusinessRepositoryFacade<SectionTemplate, Guid> facade, ILogger<AddEntityViewModel<Guid, SectionTemplate, IBusinessRepositoryFacade<SectionTemplate, Guid>>> logger) : base(facade, logger)
        {
        }
        private ResumePart sectionId;
        public ResumePart SectionId
        {
            get => sectionId;
            set => this.RaiseAndSetIfChanged(ref sectionId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string template = string.Empty;
        public string Template
        {
            get => template;
            set => this.RaiseAndSetIfChanged(ref template, value);
        }


        protected override Task Clear()
        {
            Name = string.Empty;
            Template = string.Empty;
            return Task.CompletedTask;
        }

        protected override Task<SectionTemplate> ConstructEntity()
        {
            return Task.FromResult(new SectionTemplate { Name = Name, Template = Template, SectionId = SectionId });
        }
    }
    public class SectionTemplateViewModel : EntityViewModel<Guid, SectionTemplate>, ISectionTemplate
    {
        private ResumePart sectionId;
        public ResumePart SectionId
        {
            get => sectionId;
            set => this.RaiseAndSetIfChanged(ref sectionId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string template = string.Empty;
        public string Template
        {
            get => template;
            set => this.RaiseAndSetIfChanged(ref template, value);
        }
        public SectionTemplateViewModel(ILogger logger, IBusinessRepositoryFacade<SectionTemplate, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public SectionTemplateViewModel(ILogger logger, IBusinessRepositoryFacade<SectionTemplate, Guid> facade, SectionTemplate entity) : base(logger, facade, entity)
        {
        }

        protected override Task<SectionTemplate> Populate()
        {
            return Task.FromResult(new SectionTemplate()
            {
                Name = Name,
                Template = Template,
                SectionId = SectionId,
                Id = Id
            });
        }

        protected override Task Read(SectionTemplate entity)
        {
            Name = entity.Name;
            Template = entity.Template;
            SectionId = entity.SectionId;
            Id = entity.Id;
            return Task.CompletedTask;
        }
    }
    public class SectionTemplatesViewModel : EntitiesDefaultViewModel<Guid, SectionTemplate, SectionTemplateViewModel, AddSectionTemplateViewModel>
    {
        public SectionTemplatesViewModel(AddSectionTemplateViewModel addViewModel, IBusinessRepositoryFacade<SectionTemplate, Guid> facade, ILogger<EntitiesViewModel<Guid, SectionTemplate, SectionTemplateViewModel, IBusinessRepositoryFacade<SectionTemplate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<SectionTemplate>, IOrderedQueryable<SectionTemplate>>? OrderBy()
        {
            return e => e.OrderBy(e => e.Name);
        }
        protected override Task<SectionTemplateViewModel> Construct(SectionTemplate entity, CancellationToken token)
        {
            return Task.FromResult(new SectionTemplateViewModel(Logger, Facade, entity));
        }
    }
}
