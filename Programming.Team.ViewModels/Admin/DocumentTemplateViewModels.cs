using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
        ~AddDocumentTemplateViewModel()
        {
            disposables.Dispose();
        }
        public AddDocumentTemplateViewModel(IBusinessRepositoryFacade<DocumentType, int> documentTypesFacade, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, ILogger<AddEntityViewModel<Guid, DocumentTemplate, IBusinessRepositoryFacade<DocumentTemplate, Guid>>> logger) : base(facade, logger)
        {
            DocumentTypesFacade = documentTypesFacade;
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
        protected override Task Clear()
        {
            DocumentType = DocumentTypes.First();
            Name = string.Empty;
            Template = string.Empty;
            return Task.CompletedTask;
        }

        protected override Task<DocumentTemplate> ConstructEntity()
        {
            return Task.FromResult(new DocumentTemplate()
            {
                Id = Id,
                Name = Name,
                Template = Template,
                DocumentTypeId = DocumentTypeId
            });
        }
    }
    public class DocumentTemplateViewModel : EntityViewModel<Guid, DocumentTemplate>, IDocumentTemplate
    {
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
        public DocumentTemplateViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public DocumentTemplateViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, DocumentTemplate entity) : base(logger, facade, entity)
        {
        }

        public string Template
        {
            get => template;
            set => this.RaiseAndSetIfChanged(ref template, value);
        }
        protected override Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.DocumentType);
        }
        protected override Task<DocumentTemplate> Populate()
        {
            return Task.FromResult(new DocumentTemplate()
            {
                Id = Id,
                Name = Name,
                Template = template,
                DocumentTypeId = DocumentTypeId
            });
        }

        protected override Task Read(DocumentTemplate entity)
        {
            Id = entity.Id;
            Template = entity.Template;
            DocumentType = entity.DocumentType;
            DocumentTypeId = entity.DocumentTypeId;
            Name = entity.Name;
            return Task.CompletedTask;
        }
    }
    public class DocumentTemplatesViewModel : EntitiesDefaultViewModel<Guid, DocumentTemplate, DocumentTemplateViewModel, AddDocumentTemplateViewModel>
    {
        public DocumentTemplatesViewModel(AddDocumentTemplateViewModel addViewModel, IBusinessRepositoryFacade<DocumentTemplate, Guid> facade, ILogger<EntitiesViewModel<Guid, DocumentTemplate, DocumentTemplateViewModel, IBusinessRepositoryFacade<DocumentTemplate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>? PropertiesToLoad()
        {
            return e => e.Include(e => e.DocumentType);
        }
        protected override Func<IQueryable<DocumentTemplate>, IOrderedQueryable<DocumentTemplate>>? OrderBy()
        {
            return e => e.OrderBy(x => x.Name);
        }
        protected override Task<DocumentTemplateViewModel> Construct(DocumentTemplate entity, CancellationToken token)
        {
            return Task.FromResult(new DocumentTemplateViewModel(Logger, Facade, entity));
        }
    }
}
