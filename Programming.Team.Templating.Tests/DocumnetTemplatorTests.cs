using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using System.Text.Json;

namespace Programming.Team.Templating.Tests;

[TestClass]
public class DocumnetTemplatorTests
{
    [TestMethod]
    public async Task DocumnetTemplatorTests_ApplyTemplate()
    {
        // Arrange
        var logger = new Mock<ILogger<DocumentTemplator>>();
        var template = "\\documentclass[a4paper,10pt]{article} \r\n\\usepackage[utf8]{inputenc}\r\n\\usepackage{geometry}\r\n\\geometry{margin=1in}\r\n\\usepackage{titlesec}\r\n\\usepackage{enumitem}\r\n\\usepackage{hyperref}\r\n\\usepackage{longtable}\r\n\r\n\\titleformat{\\section}{\\bfseries\\Large}{}{0em}{}[\\titlerule]\r\n\\titleformat{\\subsection}[runin]{\\normalfont\\bfseries}{\\thesubsection}{1em}{}\r\n\\titlespacing*{\\subsection}{0pt}{0pt}{0pt}\r\n\r\n\\begin{document}\r\n\r\n% Header Section\r\n\\begin{center}\r\n    {\\LARGE {{ user.FirstName }} {{ user.LastName }} } \\\\\r\n    {{ user.City }}, {{ user.State }} | {{ user.PhoneNumber }} | \\href{mailto:{{ user.EmailAddress }}}{{ user.EmailAddress }} \\\\\r\n    \\vspace{0.5cm}\r\n\\end{center}\r\n\r\n% Objective Section\r\n\\section*{Bio}\r\n{{ user.Bio }}\r\n\r\n{{ if recommendations.size > 0 }}\r\n\\filbreak\r\n\\section*{Recommendations}\r\n{{for rec in recommendations}}\r\n\\subsection*{}\r\n\\hfill \"{{rec.Body}}\" \\\\\r\n\\hfill {{rec.Name}} - {{rec.Title}}, {{rec.Position.Company.Name}} \\\\\r\n\\vspace{0.1cm}\r\n{{end}}\r\n{{end}}\r\n{{ if skills.size > 0 }}\r\n% Skills Section\r\n\\section*{Skills}\r\n\\begin{center}\r\n\\begin{longtable}{|p{4cm}|c|p{6cm}|}\r\n\\hline\r\n\\textbf{Skill} & \\textbf{Years} & \\textbf{Used At} \\\\\r\n\\hline\r\n{{ for skill in skills }}\r\n{{ skill.Skill.Name }} & {{ math.round(skill.YearsOfExperience, 1) }} & {{ for position in skill.Positions }}{{ position.Company.Name }}{{ if for.last == false }}, {{ end }}{{ end }} \\\\\r\n\\hline\r\n{{ end }}\r\n\\end{longtable}\r\n\\end{center}\r\n{{ end }}\r\n\r\n{{ if positions.size > 0 }}\r\n% Work Experience Section\r\n\\filbreak\r\n\\section*{Work Experience}\r\n\r\n{{ for position in positions }}\r\n\\filbreak\r\n\\subsection*{\\textbf {{ position.Title }} \\hfill {{ position.Company.Name }}}\r\n\\noindent\r\n\\textit {{ date.parse position.StartDateString '%F'  | date.to_string '%b-%Y' }} -- {{ if position.EndDateString == null }}Present{{ else }}{{date.parse position.EndDateString '%F'  | date.to_string '%b-%Y' }}{{ end }} \\\\\r\n\r\n\\noindent\r\n{{ position.Description }}\r\n\r\n\\noindent\r\n{{ if position.PositionSkills.size > 0 }}\r\nSkills: {{ for pskill in position.PositionSkills }}{{ pskill.Skill.Name }}{{ if for.last == false }}, {{ end }}{{ end }}{{ end }}\r\n\r\n{{ end }}\r\n{{ end }}\r\n\r\n{{if educations.size > 0}}\r\n\\section*{Education}\r\n{{for edu in educations}}\r\n\\filbreak\r\n\\subsection*{\\textbf {{ edu.Institution.Name }} {{ if edu.Major == null }} {{else}} \\hfill {{edu.Major}} {{end}} }\r\n\\noindent\r\n\\textit {{ date.parse edu.StartDateString '%F'  | date.to_string '%b-%Y' }} -- {{ if edu.EndDateString == null }}Present{{ else }}{{date.parse edu.EndDateString '%F'  | date.to_string '%b-%Y' }}{{ end }} \\\\\r\n\\noindent\r\n{{edu.Description}}\r\n{{end}}\r\n{{end}}\r\n\r\n\\end{document}\r\n";
        var strResume = await File.ReadAllTextAsync("resume.json");
        var data = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        var documentTemplator = new DocumentTemplator(logger.Object);
        // Act
        var result = await documentTemplator.ApplyTemplate(template, data);
        Assert.IsNotNull(result);
    }
}
