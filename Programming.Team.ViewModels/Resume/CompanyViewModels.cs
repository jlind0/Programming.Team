using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class SearchSelectCompanyViewModel : EntitySelectSearchViewModel<Guid, Company, AddCompanyViewModel>
    {
        public SearchSelectCompanyViewModel(IBusinessRepositoryFacade<Company, Guid> facade, AddCompanyViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Company, IBusinessRepositoryFacade<Company, Guid>, AddCompanyViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<Company>> DoSearch(string? text, CancellationToken token = default)
        {

            if (string.IsNullOrWhiteSpace(text))
                return [];
            SearchString = text;
            var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
                filter: q => q.Name.StartsWith(text), token: token);
            if (result != null)
                return result.Entities;
            return [];
        }
    }
    public class AddCompanyViewModel : AddEntityViewModel<Guid, Company>, ICompany, ITextual
    {
        public SmartTextEditorViewModel<AddCompanyViewModel> SmartText { get; }
        public AddCompanyViewModel(IBusinessRepositoryFacade<Company, Guid> facade, IDocumentTemplator templator, IConfiguration config, ILogger<AddEntityViewModel<Guid, Company, IBusinessRepositoryFacade<Company, Guid>>> logger) : base(facade, logger)
        {
            SmartText = new SmartTextEditorViewModel<AddCompanyViewModel>(this, logger, templator, config);
        }
        private string name = null!;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? city;
        public string? City
        {
            get => city;
            set => this.RaiseAndSetIfChanged(ref city, value);
        }

        private string? state;
        public string? State
        {
            get => state;
            set => this.RaiseAndSetIfChanged(ref state, value);
        }

        private string? country;
        public string? Country
        {
            get => country;
            set => this.RaiseAndSetIfChanged(ref country, value);
        }

        private string? url;

        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }
        private TextType textTypeId = TextType.Text;
        public TextType TextTypeId
        {
            get => textTypeId;
            set => this.RaiseAndSetIfChanged(ref textTypeId, value);
        }
        public string? Text
        {
            get => Description;
            set => Description = value;
        }

        protected override Task Clear()
        {
            Name = "";
            Description = null;
            City = null;
            State = null;
            Country = null;
            Url = null;
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }

        protected override Task<Company> ConstructEntity()
        {
            return Task.FromResult(new Company()
            {
                Name = Name,
                Description = Description,
                City = City,
                State = State,
                Country = Country,
                Url = Url,
                TextTypeId = TextTypeId
            });
        }
        public override void SetText(string text)
        {
            Name = text;
        }
    }
}
