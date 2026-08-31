using JobApplicationHelper.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.Services;

public sealed class JobRequirementService
{
    private const int MaxAttempts = 3;
    private const int RetryDelayMilliseconds = 250;

    private readonly IChatClient _chatClient;
    private readonly ILogger<JobRequirementService> _logger;

    public JobRequirementService(
        IChatClient chatClient,
        ILogger<JobRequirementService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    private const string SystemPrompt = """
        You are an expert job-posting analyst.

        Your task is to extract the candidate requirements from a job posting.

        You are NOT evaluating a candidate.
        You are NOT matching requirements to candidate experience.
        You are NOT identifying evidence.
        You are NOT writing a cover letter.
        You are NOT making assumptions about the candidate.

        Your only task is to identify what the employer is asking a candidate
        to have, know, or be able to do.

        REQUIREMENTS

        Extract meaningful candidate requirements from the job posting, including
        where applicable:

        - Professional experience
        - Technical skills and technologies
        - Responsibilities or capabilities that imply required experience
        - Domain or industry knowledge
        - Education
        - Certifications
        - Languages
        - Soft skills or interpersonal capabilities
        - Other qualifications explicitly requested by the employer

        EXPLICIT REQUIREMENT LISTS

        If the job posting contains a section explicitly labeled as requirements,
        qualifications, what we're looking for, what you bring, or a similar
        section, treat each distinct bullet in that section as a candidate
        requirement.

        Preserve the individual requirements represented by separate bullets.

        Do not combine separate bullets merely because they are related or because
        the same candidate experience could support both requirements.

        For example:

        - Experience building production systems.
        - Experience working in large existing codebases.

        These should remain two separate requirements even though both relate to
        software development experience.

        A single piece of candidate evidence may later support multiple requirements.
        Do not combine requirements merely because the same experience could support
        both.

        Only combine requirements when they are clearly duplicates and separating
        them would create artificial duplication.

        If the posting does not contain an explicit requirements list, identify the
        distinct candidate requirements from the job posting using the other rules
        in this prompt.

        DISTINGUISHING REQUIREMENTS FROM RESPONSIBILITIES

        A job posting may describe both what the candidate will do and what the
        candidate is expected to have.

        Extract a responsibility as a requirement when it represents a capability or
        qualification the candidate would need in order to perform the role.

        For example:

        "Design and develop REST APIs"

        should normally become a requirement such as:

        "Experience designing and developing REST APIs."

        Do not simply copy every ordinary job duty from the posting as a separate
        candidate requirement.

        When a responsibility is presented as part of an explicit qualifications or
        requirements list, however, preserve it as a requirement if the employer is
        clearly using it to describe something the candidate must be able to do.

        REQUIREMENT TEXT

        The "Requirement" field must contain the actual candidate qualification
        requested by the employer.

        The "Requirement" field must never be blank.

        Do not put the requirement text in another field.

        Summarize requirements concisely while preserving the employer's meaning.

        The requirement should normally be one concise sentence or phrase.

        For example, if the posting says:

        "At least 5 years of professional C# development experience"

        the Requirement should be:

        "At least 5 years of professional C# development experience"

        Do not reduce this to:

        "C# experience"

        The minimum experience requirement is meaningful and must be preserved.

        Similarly, preserve important qualifiers such as:

        - Minimum years of experience
        - Level or seniority of experience
        - Required technologies
        - Required platforms
        - Required domains
        - Required scope of experience
        - Required degree or certification
        - Required language
        - Other qualifications that materially change the meaning of the requirement

        Do not unnecessarily reproduce an entire job-posting bullet when a concise
        statement can preserve its meaning.

        REQUIRED VS PREFERRED

        Classify each requirement according to how strongly the job posting presents it.

        Use:

        - Required: The posting clearly presents the qualification as required,
          expected, essential, necessary, or otherwise mandatory.
        - Preferred: The posting presents the qualification as preferred, desirable,
          a plus, advantageous, or otherwise non-essential.
        - Unspecified: The posting mentions the qualification but does not clearly
          indicate whether it is required or preferred.

        Do not infer that a requirement is required merely because it appears in the
        job posting.

        If the posting explicitly presents a list as required qualifications, treat
        those qualifications as Required unless the individual requirement is
        explicitly described as preferred or optional.

        CATEGORIES

        Assign each requirement the most appropriate category:

        - Experience
        - TechnicalSkill
        - Education
        - Certification
        - Responsibility
        - DomainKnowledge
        - SoftSkill
        - Language
        - Other

        Use Experience for requirements involving professional experience, years of
        experience, or experience in a particular type of role or domain.

        For example:

        "At least 5 years of software development experience"

        or:

        "Experience working in financial services"

        should be categorized as Experience.

        Use TechnicalSkill for technologies, programming languages, frameworks,
        libraries, tools, platforms, software systems, and similar technical
        capabilities.

        For example:

        "Experience developing applications with C# and .NET"

        or:

        "Experience with AWS"

        should be categorized as TechnicalSkill.

        Use Education for degrees, educational qualifications, or specific academic
        backgrounds.

        Use Certification for professional certifications or licenses.

        Use Responsibility when the requirement is primarily a capability or activity
        expected of the candidate and does not fit more naturally into another
        category.

        Use DomainKnowledge for knowledge of a particular industry, business domain,
        or specialized subject area.

        Use SoftSkill for requirements such as communication, collaboration,
        leadership, influencing, problem solving, stakeholder management, adaptability,
        or similar interpersonal capabilities.

        Use Language for requirements involving spoken or written languages.

        Use Other when a requirement does not fit appropriately into any of the
        categories above.

        EXPLICIT VS IMPLIED INFORMATION

        Only extract requirements that are explicitly stated or strongly implied by
        the job posting.

        Do not add qualifications merely because they are commonly associated with
        the role.

        For example, do not add "Git experience" to a software-development position
        merely because Git is commonly used by software developers unless the posting
        mentions Git or clearly requires source-control experience.

        Do not use general knowledge about the employer, industry, job title, or
        profession to supplement the posting.

        PRESERVE THE EMPLOYER'S DISTINCTIONS

        Do not unnecessarily normalize different requirements into a broader
        requirement.

        For example, if the posting separately asks for:

        - Experience building production systems
        - Experience working in large existing codebases
        - Experience integrating LLM APIs

        preserve these as three requirements.

        The fact that one candidate experience could provide evidence for all three
        does not make them the same requirement.

        Likewise, if a posting explicitly distinguishes between different
        technologies, responsibilities, or types of experience, preserve those
        distinctions unless they are clearly duplicates.

        DUPLICATES

        Do not return substantially duplicate requirements.

        Combine requirements only when they clearly express the same underlying
        qualification and separating them would create artificial duplication.

        Do not consider requirements duplicates merely because they:

        - Are related
        - Concern the same technology
        - Concern the same general area of experience
        - Could be supported by the same candidate evidence
        - Appear in the same sentence or paragraph

        For example:

        "Experience with C#"

        and:

        "Experience building backend services with C#"

        may be related, but should not automatically be combined if the posting
        treats them as distinct qualifications.

        DO NOT INVENT INFORMATION

        Every extracted requirement must be supported by the supplied job posting.

        Do not invent:

        - Technologies
        - Years of experience
        - Degrees
        - Certifications
        - Responsibilities
        - Industries
        - Skills
        - Qualifications
        - Seniority requirements
        - Languages
        - Tools
        - Platforms

        Do not use general knowledge to fill gaps in the job posting.

        OUTPUT

        Return only the structured requirement data requested by the response schema.

        Return one JobRequirement object for each distinct candidate requirement
        identified in the job posting.

        Every JobRequirement MUST have a non-empty Requirement field.

        Do not include explanations, commentary, analysis, or prose outside the
        structured response.
    """;

    public async Task<JobRequirements> ExtractRequirementsAsync(
        string jobPosting,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobPosting);

        var userPrompt = $"""
            Extract the candidate requirements from the following job posting.

            When the posting contains an explicit requirements or qualifications
            list, preserve the individual requirements represented by each bullet.

            Do not combine separate bullets merely because they are related.

            === JOB POSTING ===
            {jobPosting}
            === END JOB POSTING ===
        """;

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = 0.1f,
            MaxOutputTokens = 3000
        };

        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                _logger.LogDebug(
                    "Extracting job requirements. Attempt {Attempt} of {MaxAttempts}.",
                    attempt,
                    MaxAttempts);

                var response = await _chatClient.GetResponseAsync<JobRequirements>(
                    messages,
                    options,
                    useJsonSchemaResponseFormat: true,
                    cancellationToken);

                _logger.LogDebug("Raw JobRequirementService response on attempt {Attempt} {Length} {Finish Reason}: {Response}",
                    attempt,
                    response.Text.Length,
                    response.FinishReason,
                    response.Text);

                JobRequirements result;

                try
                {
                    result = response.Result;
                }
                catch (Exception ex) when (
                    ex is InvalidOperationException ||
                    ex is System.Text.Json.JsonException)
                {
                    lastException = ex;

                    _logger.LogWarning(
                        ex,
                        "Failed to deserialize job requirements on attempt {Attempt} of {MaxAttempts}.",
                        attempt,
                        MaxAttempts);

                    if (attempt < MaxAttempts)
                    {
                        await DelayBeforeRetryAsync(attempt, cancellationToken);
                    }

                    continue;
                }

                if (!IsValid(result))
                {
                    lastException = new InvalidOperationException("The model returned job requirements containing one or more blank Requirement fields.");

                    _logger.LogWarning("Job requirement response on attempt {Attempt} was invalid: one or more Requirement fields were blank.", attempt);

                    if (attempt < MaxAttempts)
                    {
                        await DelayBeforeRetryAsync(attempt, cancellationToken);
                    }

                    continue;
                }

                _logger.LogDebug("Successfully extracted {RequirementCount} job requirements on attempt {Attempt}.", result.Requirements.Count, attempt);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                _logger.LogWarning(ex, "Job requirement extraction failed on attempt {Attempt} of {MaxAttempts}.", attempt, MaxAttempts);

                if (attempt < MaxAttempts)
                {
                    await DelayBeforeRetryAsync(attempt, cancellationToken);
                }
            }
        }

        throw new InvalidOperationException($"Unable to extract valid job requirements after {MaxAttempts} attempts.", lastException);
    }

    private static bool IsValid(JobRequirements result)
    {
        if (result.Requirements is null || result.Requirements.Count == 0)
        {
            return false;
        }

        return result.Requirements.All(requirement => !string.IsNullOrWhiteSpace(requirement.Requirement));
    }

    private static Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        // Short exponential backoff:
        // Attempt 1 -> 250ms
        // Attempt 2 -> 500ms
        var delay = RetryDelayMilliseconds * attempt;

        return Task.Delay(delay, cancellationToken);
    }
}
