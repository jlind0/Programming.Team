using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class AddDocumentTemplateViewModel : AddEntityViewModel<Guid, DocumentTemplate>, IDocumentTemplate
    {
        public ObservableCollection<DocumentType> DocumentTypes { get; } = new ObservableCollection<DocumentType>();
        protected IBusinessRepositoryFacade<DocumentType, int> DocumentTypesFacade { get; }
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        protected IContextFactory ContextFactory { get; }
        ~AddDocumentTemplateViewModel()
        {
            disposables.Dispose();
        }
        public AddDocumentTemplateViewModel(IContextFactory contextFactory, IBusinessRepositoryFacade<DocumentType, int> documentTypesFacade, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, ILogger<AddEntityViewModel<Guid, DocumentTemplate, IBusinessRepositoryFacade<DocumentTemplate, Guid>>> logger) : base(facade, logger)
        {
            DocumentTypesFacade = documentTypesFacade;
            ContextFactory = contextFactory;
            this.WhenPropertyChanged(p => p.DocumentType).Subscribe(p =>
            {
                if (DocumentTypes.Count == 0)
                    return;
                if (p.Sender.DocumentType != null)
                    p.Sender.DocumentTypeId = p.Sender.DocumentType.Id;
                else
                {
                    p.Sender.DocumentType = DocumentTypes.First();
                }
            }).DisposeWith(disposables);
        }
        protected override async Task DoInit(CancellationToken token)
        {
            try
            {
                DocumentTypes.Clear();
                var rs = await DocumentTypesFacade.Get(orderBy: q => q.OrderBy(c => c.Name), token: token);
                foreach (var type in rs.Entities)
                {
                    DocumentTypes.Add(type);
                }
                DocumentTypeId = DocumentTypes.First().Id;
                if (await ContextFactory.IsInRole("Admin"))
                {
                    ApprovalStatus = ApprovalStatus.Approved;
                    OwnerId = null;
                }
                else
                {
                    OwnerId = await DocumentTypesFacade.GetCurrentUserId(fetchTrueUserId: true, token: token);
                    ApprovalStatus = ApprovalStatus.Pending;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            await base.DoInit(token);
        }
        private DocumentType? documentType;
        public DocumentType? DocumentType
        {
            get => documentType;
            set
            {
                this.RaiseAndSetIfChanged(ref documentType, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
        private int documentTypeId;
        public int DocumentTypeId
        {
            get => documentTypeId;
            set => this.RaiseAndSetIfChanged(ref documentTypeId, value);
        }
        
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set
            {
                this.RaiseAndSetIfChanged(ref name, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }

        private string template = string.Empty;
        public string Template
        {
            get => template;
            set
            {
                this.RaiseAndSetIfChanged(ref template, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }

        public override bool CanAdd => base.CanAdd && !string.IsNullOrWhiteSpace(Template) && !string.IsNullOrWhiteSpace(Name) && DocumentType != null;

        public Guid? OwnerId { get; set; }
        private decimal? price;
        public decimal? Price 
        {
            get => price;
            set => this.RaiseAndSetIfChanged(ref price, value);
        }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;

        protected override Task Clear()
        {
            DocumentType = DocumentTypes.First();
            Name = string.Empty;
            Template = string.Empty;
            Price = null;
            ApprovalStatus = OwnerId == null ? ApprovalStatus.Approved : ApprovalStatus.Pending;
            return Task.CompletedTask;
        }
        
        protected override Task<DocumentTemplate> ConstructEntity()
        {
            return Task.FromResult(new DocumentTemplate()
            {
                Id = Id,
                Name = Name,
                Template = Template,
                DocumentTypeId = DocumentTypeId,
                OwnerId = OwnerId,
                Price = Price,
                ApprovalStatus = ApprovalStatus
            });
        }
    }
    
    public class DocumentTemplateViewModel : EntityViewModel<Guid, DocumentTemplate>, IDocumentTemplate
    {
        public SelectSectionTemplatesViewModel SelectSectionTemplates { get; }
        private int documentTypeId;
        public int DocumentTypeId
        {
            get => documentTypeId;
            set => this.RaiseAndSetIfChanged(ref documentTypeId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string template = string.Empty;
        private DocumentType? documentType;
        public DocumentType? DocumentType
        {
            get => documentType;
            set => this.RaiseAndSetIfChanged(ref documentType, value);
        }
        protected IContextFactory ContextFactory { get; }
        protected IBusinessRepositoryFacade<DocumentSectionTemplate, Guid> DocumentSectionTemplateFacade { get; }
        public DocumentTemplateViewModel(ILogger logger, SelectSectionTemplatesViewModel selectSectionTemplates, IBusinessRepositoryFacade<DocumentSectionTemplate, Guid> documentSectionTemplateFacade, IContextFactory contextFactory, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, Guid id) : base(logger, facade, id)
        {
            ContextFactory = contextFactory;
            SelectSectionTemplates = selectSectionTemplates;
            DocumentSectionTemplateFacade = documentSectionTemplateFacade;
        }

        public DocumentTemplateViewModel(ILogger logger, SelectSectionTemplatesViewModel selectSectionTemplates, IBusinessRepositoryFacade<DocumentSectionTemplate, Guid> documentSectionTemplateFacade, IContextFactory contextFactory, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, DocumentTemplate entity) : base(logger, facade, entity)
        {
            ContextFactory = contextFactory;
            SelectSectionTemplates = selectSectionTemplates;
            DocumentSectionTemplateFacade = documentSectionTemplateFacade;
        }

        public string Template
        {
            get => template;
            set => this.RaiseAndSetIfChanged(ref template, value);
        }
        private Guid? ownerId;
        public Guid? OwnerId
        {
            get => ownerId;
            set => this.RaiseAndSetIfChanged(ref ownerId, value);
        }
        private User? owner;
        public User? Owner
        {
            get => owner;
            set => this.RaiseAndSetIfChanged(ref owner, value);
        }
        private decimal? price;
        public decimal? Price
        {
            get => price;
            set => this.RaiseAndSetIfChanged(ref price, value);
        }
        private ApprovalStatus approvalStatus = ApprovalStatus.Approved;
        public ApprovalStatus ApprovalStatus{
            get => approvalStatus;
            set => this.RaiseAndSetIfChanged(ref approvalStatus, value);
        }
        protected override Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.DocumentType).Include(e => e.Owner).Include(e => e.DocumentSectionTemplates).ThenInclude(e => e.SectionTemplate);
        }
        protected override async Task<DocumentTemplate> Populate()
        {
            bool isAdmin = await ContextFactory.IsInRole("Admin");
            return new DocumentTemplate()
            {
                Id = Id,
                Name = Name,
                Template = template,
                DocumentTypeId = DocumentTypeId,
                OwnerId = OwnerId,
                Price = Price,
                ApprovalStatus = isAdmin ? ApprovalStatus : ApprovalStatus.Pending
            };
        }
        protected override async Task<DocumentTemplate> DoUpdate(CancellationToken token)
        {
            await using (var uow = Facade.CreateUnitOfWork())
            {
                var defaultTemplates = SelectSectionTemplates.Selected.Where(s => s.IsDefault).Select(s => s.Id).ToList();
                var oldDefaultTemplates = DocumentSectionTemplates.Where(s => s.IsDefault).ToList();
                foreach(var defaultTemplate in oldDefaultTemplates.Where(odt => !defaultTemplates.Any(dt => odt.SectionTemplateId == dt)))
                {
                    defaultTemplate.IsDefault = false;
                    await DocumentSectionTemplateFacade.Update(defaultTemplate, uow, token:token);
                }
                var oldStandardTemplates = DocumentSectionTemplates.Where(s => !s.IsDefault).ToList();
                foreach (var defaultTemplate in oldStandardTemplates.Where(odt => defaultTemplates.Any(dt => odt.SectionTemplateId == dt)))
                {
                    defaultTemplate.IsDefault = true;
                    await DocumentSectionTemplateFacade.Update(defaultTemplate, uow, token: token);
                }
                var selectedTemplates = SelectSectionTemplates.Selected.Select(d => d.Id).ToList();
                var toRemove = DocumentSectionTemplates
                    .Where(d => !selectedTemplates.Contains(d.SectionTemplateId))
                    .ToList();
                foreach (var orphan in toRemove)
                {
                    await DocumentSectionTemplateFacade.Delete(orphan.Id, uow, token);
                }
                var currentTemplates = DocumentSectionTemplates.Select(d => d.SectionTemplateId).ToList();
                selectedTemplates.RemoveAll(currentTemplates.Contains);
                foreach (var template in selectedTemplates)
                {
                    await DocumentSectionTemplateFacade.Add(new DocumentSectionTemplate()
                    {
                        DocumentTemplateId = Id,
                        SectionTemplateId = template,
                        IsDefault = defaultTemplates.Contains(template)
                    }, uow, token);
                }
                await uow.Commit(token);
            }
            return await base.DoUpdate(token);
        }
        protected ICollection<DocumentSectionTemplate> DocumentSectionTemplates { get; set; } = [];
        protected override async Task Read(DocumentTemplate entity)
        {
            Id = entity.Id;
            Template = entity.Template;
            DocumentType = entity.DocumentType;
            DocumentTypeId = entity.DocumentTypeId;
            Name = entity.Name;
            OwnerId = entity.OwnerId;
            Price = entity.Price;
            ApprovalStatus = entity.ApprovalStatus;
            Owner = entity.Owner;
            DocumentSectionTemplates = entity.DocumentSectionTemplates;
            SelectSectionTemplates.DocumentTemplateId = Id;
            SelectSectionTemplates.OwnerId = OwnerId;
            await SelectSectionTemplates.SetSelected(entity.DocumentSectionTemplates.Select(dst => dst.SectionTemplateId).ToArray());
        }
    }
    public class DocumentTemplatesViewModel : EntitiesDefaultViewModel<Guid, DocumentTemplate, DocumentTemplateViewModel, AddDocumentTemplateViewModel>
    {
        protected IContextFactory ContextFactory { get; }
        protected IBusinessRepositoryFacade<DocumentSectionTemplate, Guid> DocumentSectionTemplateFacade { get; }
        protected IServiceProvider Services { get; }
        public DocumentTemplatesViewModel(IContextFactory contextFactory, IServiceProvider services, IBusinessRepositoryFacade<DocumentSectionTemplate, Guid> documentSectionTemplateFacade, AddDocumentTemplateViewModel addViewModel, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, ILogger<EntitiesViewModel<Guid, DocumentTemplate, DocumentTemplateViewModel, IBusinessRepositoryFacade<DocumentTemplate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            ContextFactory = contextFactory;
            Services = services;
            DocumentSectionTemplateFacade = documentSectionTemplateFacade;
        }
        protected override Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.DocumentType).Include(e => e.Owner).Include(e => e.DocumentSectionTemplates).ThenInclude(e => e.SectionTemplate);
        }
        protected override async Task<Expression<Func<DocumentTemplate, bool>>?> FilterCondition()
        {
            if (await ContextFactory.IsInRole("Admin"))
            {
                return null; // Admins see all templates
            }
            else
            {
                var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
                return e => e.OwnerId == userId;
            }   
        }
        protected override Func<IQueryable<DocumentTemplate>, IOrderedQueryable<DocumentTemplate>>? OrderBy()
        {
            return e => e.OrderBy(x => x.Name);
        }
        protected override Task<DocumentTemplateViewModel> Construct(DocumentTemplate entity, CancellationToken token)
        {
            return Task.FromResult(new DocumentTemplateViewModel(Logger, 
                Services.GetRequiredService<SelectSectionTemplatesViewModel>(), DocumentSectionTemplateFacade, ContextFactory, Facade, entity));
        }
    }
}
