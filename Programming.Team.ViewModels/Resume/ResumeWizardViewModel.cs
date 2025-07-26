using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class ResumeWizardViewModel : ReactiveObject
    {
        public PositionsViewModel Positions { get;  }
        public EducationsViewModel Educations { get; }
        public RecommendationsViewModel Recommendations { get; }
        public ProjectsViewModel Projects { get; }
        public CertificatesViewModel Certificates { get; }
        public PublicationsViewModel Publications { get; }
        public UserProfileLoaderViewModel UserProfile { get; }
        protected ILogger Logger { get; }
        public ResumeWizardViewModel(ILogger<ResumeWizardViewModel> logger, PositionsViewModel positions, EducationsViewModel educations, RecommendationsViewModel recommendations, ProjectsViewModel projects, CertificatesViewModel certificates, PublicationsViewModel publications, UserProfileLoaderViewModel userProfile)
        {
            Logger = logger;
            Positions = positions;
            Educations = educations;
            Recommendations = recommendations;
            Projects = projects;
            Certificates = certificates;
            Publications = publications;
            UserProfile = userProfile;
        }
    }
}
