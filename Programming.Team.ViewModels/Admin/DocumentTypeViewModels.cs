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
    public class AddDocumentTypeViewModel : AddEntityViewModel<DocumentTypes, DocumentType>, IDocumentType
    {
        public AddDocumentTypeViewModel(IBusinessRepositoryFacade<DocumentType, DocumentTypes> facade, ILogger<AddEntityViewModel<DocumentTypes, DocumentType, IBusinessRepositoryFacade<DocumentType, DocumentTypes>>> logger) : base(facade, logger)
        {
        }
        public override bool CanAdd => base.CanAdd && !string.IsNullOrWhiteSpace(Name);
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

        protected override Task Clear()
        {
            Name = string.Empty;
            return Task.CompletedTask;
        }

        protected override Task<DocumentType> ConstructEntity()
        {
            return Task.FromResult(new DocumentType { Name = Name });
        }
    }
    public class DocumentTypeViewModel : EntityViewModel<DocumentTypes, DocumentType>, IDocumentType
    {
        public DocumentTypeViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentType, DocumentTypes> facade, DocumentTypes id) : base(logger, facade, id)
        {
        }

        public DocumentTypeViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentType, DocumentTypes> facade, DocumentType entity) : base(logger, facade, entity)
        {
        }
        
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        internal override Task<DocumentType> Populate()
        {
            return Task.FromResult(new DocumentType()
            {
                Name = Name,
                Id = Id
            });
        }

        internal override Task Read(DocumentType entity)
        {
            Name = entity.Name;
            Id = entity.Id;
            return Task.CompletedTask;
        }
    }
    public class DocumentTypesViewModel : EntitiesDefaultViewModel<DocumentTypes, DocumentType, DocumentTypeViewModel, AddDocumentTypeViewModel>
    {
        public DocumentTypesViewModel(AddDocumentTypeViewModel addViewModel, IBusinessRepositoryFacade<DocumentType, DocumentTypes> facade, ILogger<EntitiesViewModel<DocumentTypes, DocumentType, DocumentTypeViewModel, IBusinessRepositoryFacade<DocumentType, DocumentTypes>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<DocumentType>, IOrderedQueryable<DocumentType>>? OrderBy()
        {
            return e => e.OrderBy(e => e.Name);
        }
        protected override Task<DocumentTypeViewModel> Construct(DocumentType entity, CancellationToken token)
        {
            return Task.FromResult(new DocumentTypeViewModel(Logger, Facade, entity));
        }
    }
}
