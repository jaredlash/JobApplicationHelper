using JobApplicationHelper.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace JobApplicationHelper.Serialization;

public sealed class ExperienceTypeYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(ExperienceType);
    }

    public object ReadYaml(
        IParser parser,
        Type type,
        ObjectDeserializer rootDeserializer)
    {
        if (!parser.TryConsume<Scalar>(out var scalar))
        {
            throw new YamlException(
                "Expected a scalar value for an experience type.");
        }

        return scalar.Value switch
        {
            "professional" => ExperienceType.Professional,
            "personal_project" => ExperienceType.PersonalProject,
            "open_source" => ExperienceType.OpenSource,
            "education" => ExperienceType.Education,
            "internship" => ExperienceType.Internship,
            "certification" => ExperienceType.Certification,
            "volunteer" => ExperienceType.Volunteer,
            "freelance" => ExperienceType.Freelance,
            "other" => ExperienceType.Other,

            _ => throw new YamlException(
                $"Unknown experience type '{scalar.Value}'. " +
                "Expected one of: professional, personal_project, " +
                "open_source, education, internship, certification, " +
                "volunteer, freelance, other.")
        };
    }

    public void WriteYaml(
        IEmitter emitter,
        object? value,
        Type type,
        ObjectSerializer serializer)
    {
        if (value is not ExperienceType experienceType)
        {
            throw new YamlException(
                $"Expected a value of type {nameof(ExperienceType)}, " +
                $"but received {value?.GetType().Name ?? "null"}.");
        }

        var yamlValue = experienceType switch
        {
            ExperienceType.Professional => "professional",
            ExperienceType.PersonalProject => "personal_project",
            ExperienceType.OpenSource => "open_source",
            ExperienceType.Education => "education",
            ExperienceType.Internship => "internship",
            ExperienceType.Certification => "certification",
            ExperienceType.Volunteer => "volunteer",
            ExperienceType.Freelance => "freelance",
            ExperienceType.Other => "other",

            _ => throw new YamlException(
                $"Unsupported experience type '{experienceType}'.")
        };

        emitter.Emit(new Scalar(yamlValue));
    }
}