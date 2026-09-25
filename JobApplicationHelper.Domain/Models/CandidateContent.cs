namespace JobApplicationHelper.Domain.Models;

public class CandidateContent
{
    // This class should eventually be more fleshed out with the intention of creating new CVs
    // CandidateContent
    // ├── Personal summary
    // ├── Experience
    // ├── Skills
    // ├── Education
    // ├── Country-specific content
    // └── other candidate information

    public string CvText { get; set; } = string.Empty;
}
