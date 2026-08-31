using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Models;

public enum ExperienceType
{
    Professional,
    [Display(Name = "Personal Project")]
    PersonalProject,
    [Display(Name = "Open Source")]
    OpenSource,
    Education,
    Internship,
    Certification,
    Volunteer,
    Freelance,
    Other
}