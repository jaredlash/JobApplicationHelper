using JobApplicationHelper.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobApplicationHelper.Services;

public sealed class CoverLetterService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<CoverLetterService> _logger;

    public CoverLetterService(IChatClient chatClient, ILogger<CoverLetterService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    private const string AnalysisSystemPrompt = """
        You are an expert recruitment and career-application analyst.

        Analyze the relationship between a candidate's information and a job
        posting.

        Your analysis will be provided to another AI model which will use it
        to write a tailored cover letter.

        Do not write a cover letter.

        Do not invent information.

        The candidate's information comes from three sources:

        1. CV
           The candidate's concise professional representation.

        2. EXPERIENCE BANK
           A broader collection of the candidate's professional experience,
           personal projects, open-source work, education, certifications,
           internships, self-guided learning, professional development, and
           other relevant experience that may not appear on the CV.

        3. CANDIDATE NOTES
           Additional information supplied by the candidate for this
           application.

        All three sources are legitimate sources of evidence.

        A qualification may be considered supported only when the supplied
        candidate information provides reasonable evidence for it.

        Be conservative when determining whether a candidate meets a
        requirement.

        EXPERIENCE BANK:

        Experience Bank entries may provide relevant evidence that is not
        present on the CV.

        Preserve the nature of each experience when evaluating it.

        In particular:

        - A professional role may support claims about professional experience.
        - A personal project may support claims about technologies used,
          software built, or skills demonstrated through that project.
        - Self-guided learning may support claims about learning or developing
          familiarity with a technology.
        - Education may support claims about academic knowledge or study.
        - Open-source work may support claims about open-source contributions.
        - Internships may support claims about experience gained during the
          internship.

        Do not treat personal projects, self-guided learning, education, or
        other non-professional experience as professional employment experience.

        Do not reject relevant Experience Bank evidence merely because it does
        not appear on the CV.

        EVIDENCE:

        For each important requirement, identify only the most relevant
        supporting evidence.

        Evidence should be concise and specific. Do not reproduce large
        portions of the CV or Experience Bank.

        For each evidence item:

        - Identify the source as Cv, ExperienceBank, or CandidateNotes.
        - If the source is ExperienceBank, include the relevant Experience ID.
        - Do not invent Experience IDs.
        - Use an empty ExperienceIds list when the evidence does not come from
          the Experience Bank.
        - Include only evidence that materially supports the requirement.

        Do not duplicate substantially identical evidence.

        MATCH CLASSIFICATION:

        For each important job requirement, classify the candidate's match as:

        - Strong: The supplied information provides clear and relevant evidence
          that the candidate meets the requirement.
        - Partial: The supplied information provides some relevant evidence,
          but the candidate does not clearly meet the entire requirement.
        - None: The supplied information does not provide meaningful evidence
          that the candidate meets the requirement.

        Do not classify a requirement as Strong merely because the candidate
        has a related technology or skill.

        Do not turn a Partial match into a Strong match.

        ANALYSIS:

        Identify the most important job requirements. Focus on requirements
        that are significant to the position rather than attempting to list
        every minor qualification in the posting.

        For each important requirement:
        - State the requirement concisely.
        - Identify the most relevant supporting evidence.
        - Identify the source of each piece of evidence.
        - Include Experience IDs when applicable.
        - Classify the match as Strong, Partial, or None.

        Identify the candidate's strongest relevant qualifications.

        For each strength, identify only the most relevant supporting evidence.

        Identify 2-4 themes that should be emphasized in the cover letter.

        Themes should help guide the writing of the cover letter rather than
        simply repeat individual skills or technologies.

        Identify significant gaps where the candidate does not appear to meet
        an important requirement.

        Do not list minor gaps that are unlikely to affect the application.

        Finally, provide a brief recommendation for how the application should
        be positioned.

        Keep the analysis concise. Prefer specific evidence over lengthy
        explanations.

        Do not invent qualifications, experience, motivations, accomplishments,
        technologies, employers, projects, or other candidate information.
    """;

    private const string GenerationSystemPrompt = """
        You are an expert career-writing assistant.

        Write tailored cover letters based only on the supplied information.

        FACTUAL ACCURACY:

        - Never invent qualifications, skills, experience, accomplishments,
          metrics, employers, projects, technologies, certifications,
          education, responsibilities, or other facts.
        - Do not claim that the candidate meets a requirement unless the
          supplied information supports that claim.
        - Do not turn a partial match into a full claim of experience.
        - Do not invent reasons why the candidate wants to work for a company.

        WRITING:

        - Write naturally and professionally.
        - Do not simply repeat the CV.
        - Select the experience most relevant to the position.
        - Avoid generic or clichéd language.
        - Avoid excessive enthusiasm or self-promotion.
        - Do not mention that you are an AI.
        - Do not imitate the stylistic pattern normally associated with em dashes
          by replacing them with spaced hyphens.

          Avoid constructions such as:

          "X - particularly Y - Z"

          Instead, rewrite the sentence naturally as separate clauses or sentences.
        - I am not effusively enthusiastic about this job, so do not use language 
          that implies that I am. Avoid phrases like "I am thrilled to apply" or
          "I am excited about the opportunity." Instead, use a professional and
          neutral tone that conveys my interest in the position without exaggeration.
        - It is not necessary to point out that I am a good fit for the position, as
          this is implied by my application. Focus on highlighting my relevant skills
          and experience in a clear and concise manner.

        EVIDENCE SOURCES:

        Application analysis may identify evidence from the CV, Experience Bank,
        or Candidate Notes.

        Use the evidence source and nature of the experience accurately.

        In particular:
        - Do not represent Experience Bank personal projects as professional
          employment experience.
        - Do not represent self-guided learning as professional experience.
        - Do not imply that an Experience Bank skill was used in a professional
          role unless the supplied evidence supports that claim.
        - Experience Bank evidence may be used when it provides relevant
          supporting evidence that is not present on the CV.

        LANGUAGE:

        - The letter must be written entirely in English.
        - Adapt the professional communication style to the specified
          target country.
        - Do not translate the letter into another language.
        
        STYLE:
        
        When the target audience is Dutch:
        - Write in clear, direct, understated professional English.
        - Avoid excessive self-promotion and exaggerated enthusiasm.
        - Prefer specific evidence over superlatives.
        - Avoid overly formal or ceremonial language.
        - Be confident but relatively matter-of-fact.
        - Do not assume that American-style enthusiasm is appropriate.

        When the target audience is German:
        - Write in precise, professional, somewhat formal English.
        - Favor clarity, structure, and substantive information.
        - Avoid excessive informality, slang, and exaggerated enthusiasm.
        - Avoid overly casual conversational language.
        - Be respectful and professional without becoming unnecessarily
          elaborate or bureaucratic.
        - Do not imitate German grammar or translate German expressions
          literally into English.

        When the target audience is international/neutral:
        - Use clear, professional international English.
        - Avoid strongly culture-specific idioms.
        - Use a moderately formal but natural tone.

        The job posting and candidate-provided information take precedence
        over general cultural conventions. Use the target audience setting
        to adjust the communication style, not to override the character
        or expectations expressed by the job posting.

        TYPOGRAPHY:

        - Use ASCII apostrophes (').
        - Use ASCII quotation marks (").
        - Never use curly or smart quotation marks.
        - Never use an em dash.
        - Never use an en dash.
        - Do not substitute spaced hyphens for an em dash.
        - Prefer natural sentence restructuring instead.

        OUTPUT:

        Return only the complete cover letter. Do not include any commentary or explanations.
    """;

    private const string VerificationSystemPrompt = """
        You are a meticulous fact checker for job applications.

        Your task is to verify a proposed cover letter against the
        candidate's CV, Experience Bank, candidate notes, and job posting.

        The candidate's information comes from three distinct sources:

        1. CV
           The candidate's concise professional representation. It contains
           the experience and qualifications the candidate has chosen to
           present on their CV.

        2. EXPERIENCE BANK
           A broader collection of the candidate's experience and evidence.
           It may contain professional experience, personal projects,
           open-source work, education, certifications, internships,
           self-guided learning, professional development, and other
           relevant experience that is not included on the CV.

        3. CANDIDATE NOTES
           Additional information supplied by the candidate for the
           application.

        All three sources are legitimate sources of factual evidence.

        A claim does not need to appear on the CV to be considered supported.
        A claim may be supported by the Experience Bank or Candidate Notes.

        Check every substantive factual claim.

        Be conservative. Do not assume that a claim is true merely because
        it would be plausible.

        A claim is supported when the supplied candidate information provides
        reasonable evidence for it.

        EXPERIENCE BANK:

        Treat Experience Bank entries as factual candidate-provided
        information, subject to the same requirement for supporting evidence
        as information from the CV.

        However, preserve the nature of the experience.

        For example:

        - A personal project may support a claim that the candidate built or
          worked with a particular technology.
        - A personal project must not be treated as professional employment
          experience unless the Experience Bank explicitly identifies it as
          such.
        - Self-guided learning may support a claim that the candidate has
          learned or developed familiarity with a technology.
        - Self-guided learning must not be treated as professional experience
          using that technology.
        - Open-source contributions may support claims about contributing to
          open-source software when the supplied evidence supports that claim.
        - Education may support claims about academic knowledge or study, but
          should not automatically be treated as professional experience.
        - A personal project may support claims about what the candidate built,
          designed, implemented, or learned, provided the Experience Bank
          provides evidence for the claim.
        - An internship may support claims about experience gained during the
          internship, but should not be represented as a later or more senior
          professional role.
        - Professional experience in the Experience Bank may be used as
          professional experience when the entry provides evidence supporting
          the claim.

        Do not reject a claim merely because it is absent from the CV when the
        claim is supported by the Experience Bank.

        Do not accept a claim merely because a related keyword appears in the
        Experience Bank. The evidence must reasonably support the actual claim
        made in the cover letter.

        FACTUAL VERIFICATION:

        Identify:

        - Unsupported skills or qualifications.
        - Unsupported years of experience.
        - Unsupported accomplishments or metrics.
        - Invented technologies, employers, projects, responsibilities,
          certifications, or education.
        - Claims that the candidate satisfies requirements without sufficient
          evidence.
        - Incorrect company names or job titles.
        - Claims that contradict the CV, Experience Bank, or Candidate Notes.
        - Claims that incorrectly characterize the nature of an experience.
        - Claims that present personal, educational, open-source, internship,
          or self-guided experience as professional employment experience
          without supporting evidence.
        - Claims that imply professional experience with a technology when the
          supplied evidence only supports personal, educational, or self-guided
          experience.
        - Unsupported claims about the company.
        - Unsupported claims about the candidate's motivations.
        - Major tone or style violations.
        - Em dashes, en dashes, or smart quotation marks.

        When evaluating a claim, consider the complete candidate information.
        Evidence may come from the CV, Experience Bank, or Candidate Notes.

        Do not require every claim in the cover letter to also appear on the
        CV. The purpose of the Experience Bank is specifically to preserve
        relevant candidate experience that may not be included in the concise
        CV.

        A cover letter may appropriately mention relevant personal projects,
        professional development, self-guided learning, open-source work, or
        other Experience Bank entries when doing so accurately represents the
        candidate's experience.

        However, the wording must accurately reflect the nature of that
        experience.

        For example, if the Experience Bank states that the candidate built a
        personal C# project, the following type of claim may be supported:

        "I have been developing my C# and .NET skills through a personal
        software project."

        But the following type of claim would not be supported unless the
        candidate information provides professional experience:

        "I have several years of professional experience developing C# and
        .NET applications."

        Do not penalize a cover letter simply because it mentions experience
        that does not appear on the CV.

        Do not rewrite the cover letter.

        Return only the verification result.
    """;


    public async Task<ApplicationAnalysis> AnalyzeApplicationAsync(
        CoverLetterRequest request,
        CancellationToken cancellationToken = default)
    {
        var userPrompt = $"""
            Analyze this job application.
        
            === CANDIDATE CV ===
            {request.Cv}
            === END CANDIDATE CV ===
        
            === EXPERIENCE BANK ===
            {FormatExperienceBank(request.ExperienceBank)}
            === END EXPERIENCE BANK ===
        
            === JOB POSTING ===
            {request.JobPosting}
            === END JOB POSTING ===
        
            === CANDIDATE NOTES ===
            {request.CandidateNotes}
            === END CANDIDATE NOTES ===
        
            Identify the most important job requirements and evaluate the
            candidate's evidence for each.
        
            For each requirement, identify concise supporting evidence, its source,
            and classify the match as Strong, Partial, or None.
        
            When evidence comes from the Experience Bank, include the relevant
            Experience ID.
        
            Identify the candidate's strongest relevant qualifications and their
            supporting evidence.
        
            Identify 2-4 themes that should be emphasized in the cover letter.
        
            Identify significant gaps.
        
            Finally, provide a brief recommendation for positioning the application.
        
            Keep the analysis concise and avoid repeating large portions of the
            supplied materials.
        """;

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, AnalysisSystemPrompt),
            new ChatMessage(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = 0.2f,
            MaxOutputTokens = 4000
        };

        ChatResponse<ApplicationAnalysis>? response = null;
        try
        {
            response = await _chatClient.GetResponseAsync<ApplicationAnalysis>(
                messages,
                options,
                useJsonSchemaResponseFormat: true,
                cancellationToken);

            return response.Result;
        }
        catch (JsonException)
        {
            _logger.LogInformation("Raw analysis response: {Response}", response?.Text);
            throw;
        }
    }

    public async Task<string> GenerateCoverLetterAsync(
        CoverLetterRequest request,
        ApplicationAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        var requirements = string.Join(Environment.NewLine, analysis.JobRequirements.Select(r =>
            $"- Requirement: {r.Requirement}\n" +
            $"  Match: {r.Match}\n" +
            $"  Evidence:\n{FormatEvidence(r.Evidence)}"));

        var strengths = string.Join(Environment.NewLine, analysis.CandidateStrengths.Select(s =>
            $"- Strength: {s.Strength}\n" +
            $"  Evidence:\n{FormatEvidence(s.Evidence)}"));

        var themes = string.Join(Environment.NewLine, analysis.RecommendedThemes.Select(t => $"- {t}"));

        var gaps = string.Join(Environment.NewLine, analysis.PotentialGaps.Select(g => $"- {g}"));

        var experienceBank = FormatExperienceBank(request.ExperienceBank);

        var userPrompt = $"""
            /no_think

            Write a complete cover letter for this application.

            === TARGET PROFESSIONAL CULTURE ===
            {request.TargetAudience}
            === END TARGET PROFESSIONAL CULTURE ===

            === TONE ===
            {request.Tone}
            === END TONE ===

            === STYLE ===
            {request.Style}
            === END STYLE ===

            === TARGET LENGTH ===
            Approximately {request.DesiredWordCount} words.
            === END TARGET LENGTH ===

            === APPLICATION ANALYSIS ===

            JOB REQUIREMENTS:
            {requirements}

            CANDIDATE STRENGTHS:
            {strengths}

            RECOMMENDED THEMES:
            {themes}

            POTENTIAL GAPS:
            {gaps}

            SUGGESTED APPROACH:
            {analysis.SuggestedApproach}

            === END APPLICATION ANALYSIS ===

            === CANDIDATE CV ===
            {request.Cv}
            === END CANDIDATE CV ===

            === EXPERIENCE BANK ===
            {experienceBank}
            === END EXPERIENCE BANK ===

            === JOB POSTING ===
            {request.JobPosting}
            === END JOB POSTING ===

            === CANDIDATE NOTES ===
            {request.CandidateNotes}
            === END CANDIDATE NOTES ===

            === ADDITIONAL INSTRUCTIONS ===
            Do not include any addressee / recipient top-matter.  Begin the letter with the salutation.
            === END ADDITIONAL INSTRUCTIONS ===

            Write the complete cover letter now.
        """;

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, GenerationSystemPrompt),
            new ChatMessage(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = 0.7f,
            MaxOutputTokens = 2000
        };

        var response = await _chatClient.GetResponseAsync(
            messages,
            options,
            cancellationToken);

        return NormalizeTypography(response.Text);
    }

    private static string NormalizeTypography(string text) => text
        // Single quotation marks
        .Replace('\u2018', '\'')
        .Replace('\u2019', '\'')
        .Replace('\u201A', '\'')
        .Replace('\u201B', '\'')
        // Double quotation marks
        .Replace('\u201C', '"')
        .Replace('\u201D', '"')
        .Replace('\u201E', '"')
        .Replace('\u201F', '"')
        // Dashes
        .Replace('\u2013', '-')  // en dash
        .Replace('\u2014', '-')  // em dash
        .Replace('\u2012', '-')  // figure dash
        .Replace('\u2015', '-')  // horizontal bar
        // Ellipsis
        .Replace("\u2026", "...")
        // Non-breaking space
        .Replace('\u00A0', ' ');

    public async Task<VerificationResult> VerifyDraftAsync(
        CoverLetterRequest request,
        string draft,
        CancellationToken cancellationToken = default)
    {
        var experienceBank = FormatExperienceBank(request.ExperienceBank);

        var userPrompt = $"""
            Verify this cover letter.

            === CV ===
            {request.Cv}
            === END CV ===

            === JOB POSTING ===
            {request.JobPosting}
            === END JOB POSTING ===
        
            === EXPERIENCE BANK ===
            {experienceBank}
            === END EXPERIENCE BANK ===

            === CANDIDATE NOTES ===
            {request.CandidateNotes}
            === END CANDIDATE NOTES ===

            === COVER LETTER ===
            {draft}
            === END COVER LETTER ===

            Determine whether the letter is factually supported,
            appropriate for the job, and compliant with the requested
            writing style.
        """;

        var messages = new[]
        {
        new ChatMessage(ChatRole.System, VerificationSystemPrompt),
        new ChatMessage(ChatRole.User, userPrompt)
    };

        var options = new ChatOptions
        {
            Temperature = 0.1f,
            MaxOutputTokens = 1000
        };
        try
        {
            var response = await _chatClient.GetResponseAsync<VerificationResult>(
                messages,
                options,
                useJsonSchemaResponseFormat: true,
                cancellationToken);

            _logger.LogInformation("Cover letter verification response: {response}", JsonSerializer.Serialize(response));

            return response.Result;
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Error verifying cover letter draft.");
            throw;
        }

    }

    private static string FormatExperienceBank(ExperienceBank? experienceBank)
    {
        if (experienceBank is null || experienceBank.Experiences.Count == 0)
        {
            return "No Experience Bank was provided.";
        }

        var sections = experienceBank.Experiences.Select(experience =>
        {
            var skills = experience.Skills.Count > 0
                ? string.Join(", ", experience.Skills)
                : "None specified";

            var evidence = experience.Evidence.Count > 0
                ? string.Join(
                    Environment.NewLine,
                    experience.Evidence.Select(e => $"- {e}"))
                : "- None specified";

            return $"""
                EXPERIENCE ID: {experience.Id}
                TITLE: {experience.Title}
                TYPE: {experience.Type}
                ORGANIZATION: {experience.Organization ?? "Not specified"}

                SUMMARY:
                {experience.Summary ?? "None specified"}

                SKILLS:
                {skills}

                EVIDENCE:
                {evidence}

                CONTEXTS:
                {string.Join(", ", experience.Contexts)}

            """;
        });

        return string.Join(
            Environment.NewLine + "---" + Environment.NewLine,
            sections);
    }

    private static string FormatEvidence(IEnumerable<EvidenceReference> evidenceReferences)
    {
        var evidence = evidenceReferences.ToList();

        if (evidence.Count == 0)
        {
            return "    - No supporting evidence identified.";
        }

        return string.Join(
            Environment.NewLine,
            evidence.Select(e =>
            {
                var experienceIds = e.ExperienceIds.Count > 0
                    ? $" (Experience IDs: {string.Join(", ", e.ExperienceIds)})"
                    : "";

                return $"    - Source: {e.Source}{experienceIds}\n" +
                       $"      Evidence: {e.Evidence}";
            }));
    }
}