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
    public class AddDocumentTypeViewModel : AddEntityViewModel<int, DocumentType>, IDocumentType
    {
        public AddDocumentTypeViewModel(IBusinessRepositoryFacade<DocumentType, int> facade, ILogger<AddEntityViewModel<int, DocumentType, IBusinessRepositoryFacade<DocumentType, int>>> logger) : base(facade, logger)
        {
        }
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
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
    public class DocumentTypeViewModel : EntityViewModel<int, DocumentType>, IDocumentType
    {
        public DocumentTypeViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentType, int> facade, int id) : base(logger, facade, id)
        {
        }

        public DocumentTypeViewModel(ILogger logger, IBusinessRepositoryFacade<DocumentType, int> facade, DocumentType entity) : base(logger, facade, entity)
        {
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override Task<DocumentType> Populate()
        {
            return Task.FromResult(new DocumentType()
            {
                Name = Name,
                Id = Id
            });
        }

        protected override Task Read(DocumentType entity)
        {
            Name = entity.Name;
            Id = entity.Id;
            return Task.CompletedTask;
        }
    }
    public class DocumentTypesViewModel : EntitiesDefaultViewModel<int, DocumentType, DocumentTypeViewModel, AddDocumentTypeViewModel>
    {
        public DocumentTypesViewModel(AddDocumentTypeViewModel addViewModel, IBusinessRepositoryFacade<DocumentType, int> facade, ILogger<EntitiesViewModel<int, DocumentType, DocumentTypeViewModel, IBusinessRepositoryFacade<DocumentType, int>>> logger) : base(addViewModel, facade, logger)
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
