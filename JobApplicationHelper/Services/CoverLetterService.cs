using JobApplicationHelper.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text;
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

    private const string GenerationSystemPrompt = """
        You are an expert career-writing assistant.
    
        Write tailored cover letters based only on the information supplied in
        the user prompt.
    
        The candidate-provided information is the source of truth for the
        candidate's qualifications and experience. The job posting is the source
        of truth for the employer's requirements and expectations.
    
        FACTUAL ACCURACY:
    
        - Never invent qualifications, skills, experience, accomplishments,
          metrics, employers, projects, technologies, certifications,
          education, responsibilities, or other facts.
        - Do not claim that the candidate has experience that is not supported
          by the supplied candidate information.
        - Do not turn a partial match into a claim of full or extensive
          experience.
        - Do not infer specific accomplishments, responsibilities, technologies,
          or results that are not supported by the supplied information.
        - Do not invent reasons why the candidate wants to work for a company.
        - When the supplied information does not establish a qualification,
          do not manufacture or embellish evidence for it.
    
        REQUIREMENTS AND EVIDENCE:
    
        - The requirement-to-experience mappings supplied in the user prompt
          have been explicitly reviewed and selected by the candidate.
        - Treat those mappings as authoritative for the purposes of writing
          the cover letter.
        - Do not perform your own requirement-to-experience matching.
        - Do not replace a selected experience with a different experience
          merely because you consider the different experience to be a better
          match.
        - Do not introduce additional experiences as evidence for a requirement
          unless the supplied candidate information clearly supports doing so.
        - When an evidence note is provided, use it as guidance about why the
          selected experience is relevant to that particular requirement.
        - Do not treat the existence of a selected experience as proof that the
          candidate completely satisfies every aspect of the associated
          requirement.
        - If a requirement has no supporting experience, do not invent or imply
          supporting experience in order to address it.
        - It is acceptable for the letter not to address every job requirement.
          Prioritize the requirements for which credible supporting evidence is
          available.
    
        EVIDENCE SOURCES:
    
        Use the evidence source and nature of the experience accurately.
    
        In particular:
    
        - Do not represent personal projects as professional employment
          experience.
        - Do not represent self-guided learning as professional experience.
        - Do not imply that a skill was used in a professional role unless the
          supplied evidence supports that claim.
        - Personal projects, independent work, and self-guided learning may be
          mentioned when they provide relevant supporting evidence, but describe
          them accurately.
    
        WRITING:
    
        - Write naturally and professionally.
        - Write a cohesive letter rather than a point-by-point response to the
          job requirements.
        - Do not simply repeat the CV.
        - Use specific evidence to demonstrate relevant skills and experience.
        - Prioritize the strongest and most relevant selected evidence.
        - Avoid generic or clichéd language.
        - Avoid excessive enthusiasm or self-promotion.
        - Do not mention that you are an AI.
        - Do not use language that implies exaggerated enthusiasm about the
          position, such as "I am thrilled to apply" or "I am excited about
          the opportunity."
        - Convey professional interest in the position without exaggeration.
        - Do not explicitly state that the candidate is a "good fit" for the
          position. Demonstrate relevance through the candidate's experience
          instead.
        - Do not simply restate claims from the job posting as claims about the
          candidate. Connect the candidate's actual experience to the employer's
          needs.
        - Do not imitate the stylistic pattern normally associated with em dashes
          by replacing them with spaced hyphens.
    
          Avoid constructions such as:
    
          "X - particularly Y - Z"
    
          Instead, rewrite the sentence naturally using separate clauses or
          sentences.
    
        LANGUAGE:
    
        - Write the letter entirely in English.
        - Adapt the professional communication style to the specified target
          audience.
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
    
        OUTPUT:
    
        Return only the complete cover letter. Do not include commentary,
        explanations, analysis, headings describing your process, or other
        meta-text.
    """;

    private const string VerificationSystemPrompt = """
        You are a meticulous fact checker for job applications.
    
        Your task is to verify a proposed cover letter against the candidate's
        supplied information, the job posting, and the candidate-selected
        requirement-to-experience mappings.
    
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
    
        REQUIREMENTS AND SELECTED EVIDENCE:
    
        The requirement-to-experience mappings supplied with the cover letter
        have been explicitly reviewed and selected by the candidate.
    
        Treat these mappings as authoritative.
    
        Do not independently create alternative requirement-to-experience
        matches. Your task is to verify the cover letter, not to redo the
        candidate's evidence selection.
    
        A selected experience indicates that the candidate considers that
        experience relevant to the associated requirement. It does not
        necessarily establish that the candidate completely satisfies every
        aspect of the requirement.
    
        In particular:
    
        - Do not treat a selected experience as proof of a qualification that
          is not supported by the underlying candidate information.
        - Do not turn a partial match into a claim of full qualification.
        - Do not assume that the candidate satisfies a requirement simply
          because an experience has been selected for it.
        - Do not require the cover letter to address every requirement.
        - Do not consider the omission of a requirement to be a factual error
          when the supplied evidence does not credibly support that requirement.
        - Do not penalize the cover letter merely because it uses only some of
          the selected experiences or evidence. The writer is expected to
          prioritize the strongest and most relevant material.
    
        EVIDENCE NOTES:
    
        A selected evidence item may contain a "Relevance to requirement"
        note written specifically for that requirement.
    
        Use this note to understand why the candidate selected the experience,
        but do not treat the note itself as evidence of facts that are not
        supported by the underlying candidate information.
    
        Verify that claims made in the cover letter can be traced back to
        actual information supplied about the candidate.
    
        FACTUAL VERIFICATION:
    
        Check every substantive factual claim in the cover letter.
    
        Identify:
    
        - Unsupported skills or qualifications.
        - Unsupported years of experience.
        - Unsupported accomplishments or metrics.
        - Invented technologies, employers, projects, responsibilities,
          certifications, or education.
        - Claims that the candidate satisfies a requirement without sufficient
          supporting evidence.
        - Incorrect company names or job titles.
        - Claims that contradict the CV, Experience Bank, or Candidate Notes.
        - Claims that incorrectly characterize the nature of an experience.
        - Claims that present personal, educational, open-source, internship,
          or self-guided experience as professional employment experience
          without supporting evidence.
        - Claims that imply professional experience with a technology when the
          supplied evidence only supports personal, educational, or
          self-guided experience.
        - Unsupported claims about the company.
        - Unsupported claims about the candidate's motivations.
    
        When evaluating a claim, consider the complete candidate information.
        Evidence may come from the CV, Experience Bank, or Candidate Notes.
    
        Do not require every claim in the cover letter to appear on the CV.
        The Experience Bank and Candidate Notes exist specifically to preserve
        relevant candidate information that may not be included in the concise
        CV.
    
        EXPERIENCE BANK:
    
        Treat Experience Bank entries as factual candidate-provided information,
        subject to the same requirement for supporting evidence as information
        from the CV and Candidate Notes.
    
        However, preserve the nature of the experience.
    
        For example:
    
        - A personal project may support a claim that the candidate built or
          worked with a particular technology.
        - A personal project must not be represented as professional employment
          experience unless the supplied information explicitly supports that
          characterization.
        - Self-guided learning may support a claim that the candidate has
          learned or developed familiarity with a technology.
        - Self-guided learning must not be represented as professional
          experience using that technology.
        - Open-source contributions may support claims about contributing to
          open-source software when the supplied evidence supports that claim.
        - Education may support claims about academic knowledge or study, but
          should not automatically be treated as professional experience.
        - An internship may support claims about experience gained during the
          internship, but should not be represented as a later or more senior
          professional role.
        - Professional experience in the Experience Bank may be represented as
          professional experience when the entry provides evidence supporting
          the claim.
    
        Do not reject a claim merely because it is absent from the CV when the
        claim is supported by the Experience Bank or Candidate Notes.
    
        Do not accept a claim merely because a related keyword appears in the
        Experience Bank. The supplied evidence must reasonably support the
        actual claim made in the cover letter.
    
        JOB RELEVANCE:
    
        Determine whether the cover letter is appropriately tailored to the
        supplied job posting.
    
        The letter should make meaningful connections between the candidate's
        supported experience and the employer's needs.
    
        Do not require every job requirement to be addressed.
    
        Do not consider the absence of a claim to satisfy an unsupported
        requirement to be a defect.
    
        Do identify cases where the cover letter claims qualifications that
        the supplied candidate information does not support.
    
        WRITING AND INSTRUCTION COMPLIANCE:
    
        Verify that the cover letter follows the requested tone, style,
        professional culture, and other instructions supplied with the
        application.
    
        In particular, identify:
    
        - Major tone or style violations.
        - Excessive enthusiasm or self-promotion when inconsistent with the
          requested style.
        - Generic or substantially untailored writing.
        - Failure to follow explicit formatting instructions.
        - Unnecessary claims that the candidate is a "good fit" rather than
          demonstrating relevance through evidence.
        - Unnatural constructions that use spaced hyphens in place of an
          em-dash-style construction.
    
        Do not penalize a cover letter simply because you would have written
        a sentence differently.
    
        VERIFICATION STANDARD:
    
        Be conservative when evaluating factual claims.
    
        Do not assume that a claim is true merely because it is plausible,
        common for someone with the candidate's background, or consistent
        with the job requirements.
    
        At the same time, do not flag a claim as unsupported merely because
        the exact wording does not appear in the source material. Reasonable
        paraphrasing and synthesis are acceptable when the underlying claim
        is supported.
    
        Distinguish between:
    
        - A factual error or unsupported claim.
        - A misleading characterization of the candidate's experience.
        - A violation of an explicit instruction.
        - A merely subjective stylistic preference.
    
        Only report genuine problems.
    
        Do not rewrite the cover letter.
    
        Return only the verification result.
    """;


    public async Task<string> GenerateCoverLetterAsync(
        CoverLetterDraftParameters draftParameters,
        CancellationToken cancellationToken = default)
    {
        var requirementsEvidence = FormatJobRequirements(draftParameters.Requirements);

        var userPrompt = $"""
            /no_think
        
            Write a complete cover letter for this job application.
        
            The information below contains the job posting, the candidate's background,
            and requirements that have been explicitly matched to candidate experiences.
            Use these materials as the basis for the letter.
        
            === TARGET PROFESSIONAL CULTURE ===
            {draftParameters.TargetAudience}
            === END TARGET PROFESSIONAL CULTURE ===
        
            === TONE ===
            {draftParameters.Tone}
            === END TONE ===
        
            === STYLE ===
            {draftParameters.Style}
            === END STYLE ===
        
            === TARGET LENGTH ===
            Approximately {draftParameters.DesiredWordCount} words.
            === END TARGET LENGTH ===
        
            === CANDIDATE CV ===
            {draftParameters.Cv}
            === END CANDIDATE CV ===
        
            === JOB REQUIREMENTS AND SELECTED SUPPORTING EXPERIENCE ===
            The requirements below have been reviewed by the candidate. The experiences
            listed under each requirement were explicitly selected as supporting evidence.
        
            Treat these requirement-to-experience selections as authoritative.
            Do not invent additional experiences or claim that an experience supports
            a requirement unless the provided materials support that claim.
        
            Where an experience includes a "Relevance to requirement" note, use it to
            understand why the candidate considers that experience relevant to the
            specific requirement.
        
            If a requirement has no supporting experience, do not attempt to manufacture
            one. Do not make unsupported claims merely to address that requirement.
        
            {requirementsEvidence}
            === END JOB REQUIREMENTS AND SELECTED SUPPORTING EXPERIENCE ===
        
            === JOB POSTING ===
            {draftParameters.JobPosting}
            === END JOB POSTING ===
        
            === CANDIDATE NOTES ===
            {draftParameters.CandidateNotes}
            === END CANDIDATE NOTES ===
        
            === ADDITIONAL INSTRUCTIONS ===
            Do not include any addressee / recipient top-matter.
            Begin the letter with the salutation.
        
            Write a natural, persuasive cover letter rather than a point-by-point
            response to the requirements.
        
            Prioritize the strongest and most relevant evidence when deciding which
            experiences to emphasize.
        
            Do not simply repeat the wording of the job posting. Connect the candidate's
            actual experience to the employer's needs using specific evidence from the
            supplied materials.
        
            Do not invent employers, job titles, responsibilities, accomplishments,
            technologies, years of experience, metrics, projects, or other qualifications.
        
            Do not claim experience with a technology, tool, methodology, or practice
            unless that experience is supported by the supplied candidate materials.
        
            Use the candidate's selected experiences to demonstrate qualifications rather
            than merely stating that the candidate possesses them.
        
            The cover letter should read as a cohesive letter written specifically for
            this position, not as a summary of the candidate's CV.
        
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
        CoverLetterDraftParameters draftParameters,
        string draft,
        CancellationToken cancellationToken = default)
    {
        var requirementsEvidence = FormatJobRequirements(draftParameters.Requirements);

        var userPrompt = $"""
        Verify the following cover letter against the supplied job and candidate
        information.
    
        The purpose of this verification is to identify factual inaccuracies,
        unsupported claims, misleading representations, poor use of the selected
        evidence, and violations of the requested writing instructions.
    
        === CV ===
        {draftParameters.Cv}
        === END CV ===
    
        === JOB POSTING ===
        {draftParameters.JobPosting}
        === END JOB POSTING ===
    
        === JOB REQUIREMENTS AND SELECTED SUPPORTING EXPERIENCE ===
        The requirement-to-experience mappings below were explicitly reviewed and
        selected by the candidate.
    
        Treat these mappings as authoritative. Do not attempt to create alternative
        requirement-to-experience matches.
    
        For each selected experience, verify that claims made in the cover letter
        are actually supported by the supplied experience information. Pay
        particular attention to the "Relevance to requirement" notes, but do not
        treat those notes as evidence of facts that are not otherwise supported by
        the candidate information.
    
        A selected experience being associated with a requirement does not mean
        that the candidate necessarily satisfies every aspect of that requirement.
        In particular, do not treat a partial match as evidence of full
        qualification.
    
        If a requirement has no supporting experience, the cover letter must not
        invent experience to address it.
    
        {requirementsEvidence}
        === END JOB REQUIREMENTS AND SELECTED SUPPORTING EXPERIENCE ===
    
        === CANDIDATE NOTES ===
        {draftParameters.CandidateNotes}
        === END CANDIDATE NOTES ===
    
        === REQUESTED PROFESSIONAL CULTURE ===
        {draftParameters.TargetAudience}
        === END REQUESTED PROFESSIONAL CULTURE ===
    
        === REQUESTED TONE ===
        {draftParameters.Tone}
        === END REQUESTED TONE ===
    
        === REQUESTED STYLE ===
        {draftParameters.Style}
        === END REQUESTED STYLE ===
    
        === TARGET LENGTH ===
        Approximately {draftParameters.DesiredWordCount} words.
        === END TARGET LENGTH ===
    
        === COVER LETTER ===
        {draft}
        === END COVER LETTER ===
    
        Verify the cover letter according to the following criteria:
    
        1. FACTUAL ACCURACY
           Determine whether every substantive claim about the candidate is
           supported by the CV, Candidate Notes, or selected supporting
           experiences.
    
           Flag claims that:
           - invent qualifications, skills, technologies, responsibilities,
             accomplishments, metrics, employers, projects, certifications,
             education, or other facts;
           - exaggerate the candidate's experience;
           - turn a partial match into a claim of full qualification;
           - attribute professional experience to a personal project or
             self-guided learning;
           - claim experience with a technology or practice that the supplied
             information does not support.
    
        2. EVIDENCE FIDELITY
           Verify that the selected experiences are represented accurately.
    
           The letter may summarize or synthesize the selected evidence, but it
           must not introduce facts that are absent from the supplied information.
    
           Do not penalize the letter merely because it does not mention every
           selected experience or every piece of evidence. The writer is expected
           to select the strongest and most relevant material.
    
        3. REQUIREMENT COVERAGE
           Determine whether the letter makes effective use of the strongest
           supported requirements and experiences.
    
           It is not necessary for the letter to address every job requirement.
           Do not consider an omission a defect when the candidate has no credible
           supporting evidence for that requirement.
    
           Do not require the letter to claim that the candidate meets a
           requirement when the supplied evidence only establishes a partial
           match.
    
        4. JOB RELEVANCE
           Determine whether the letter is clearly tailored to the supplied job
           posting.
    
           The letter should connect relevant candidate experience to the
           employer's needs rather than merely summarizing the CV or repeating
           the job posting.
    
        5. WRITING STYLE
           Verify that the letter follows the requested professional culture,
           tone, and style.
    
           It should be professional and appropriately restrained, avoid generic
           or clichéd language, and avoid exaggerated enthusiasm or
           self-promotion.
    
           Do not require the letter to explicitly state that the candidate is
           a "good fit." Relevance should be demonstrated through the candidate's
           experience.
    
        6. FORMAT AND INSTRUCTIONS
           Verify that the letter:
           - begins with a salutation;
           - does not contain addressee or recipient top-matter;
           - is written entirely in English;
           - is approximately the requested length;
           - does not contain commentary or meta-text about the generation or
             verification process.
    
        Report only genuine problems. Do not flag stylistic choices merely because
        you would have written them differently.
    
        Determine whether the cover letter passes verification.
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

    private string FormatJobRequirements(JobRequirements jobRequirements)
    {
        var sb = new StringBuilder();

        foreach (var requirement in jobRequirements.Requirements)
        {
            sb.AppendLine($"REQUIREMENT: {requirement.Requirement}");
            sb.AppendLine($"CATEGORY: {requirement.Category}");
            sb.AppendLine($"PRIORITY: {requirement.Priority}");

            if (requirement.Evidence.NoSupportingEvidence)
            {
                sb.AppendLine("SUPPORTING EXPERIENCE: None identified.");
            }
            else if (requirement.Evidence.Evidences.Count == 0)
            {
                sb.AppendLine("SUPPORTING EXPERIENCE: None selected.");
            }
            else
            {
                sb.AppendLine("SUPPORTING EXPERIENCE:");

                foreach (var evidence in requirement.Evidence.Evidences)
                {
                    var experience = evidence.Experience;

                    FormatExperience(sb, experience);

                    if (!string.IsNullOrWhiteSpace(evidence.EvidenceNote))
                    {
                        sb.AppendLine(
                            $"    Relevance to requirement: {evidence.EvidenceNote}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void FormatExperience(StringBuilder sb, Experience experience)
    {
        sb.AppendLine($"  - {experience.Title}");

        if (!string.IsNullOrWhiteSpace(experience.Organization))
        {
            sb.AppendLine($"    Organization: {experience.Organization}");
        }

        if (!string.IsNullOrWhiteSpace(experience.Summary))
        {
            sb.AppendLine($"    Summary: {experience.Summary}");
        }

        if (experience.Skills.Count > 0)
        {
            sb.AppendLine(
                $"    Skills: {string.Join(", ", experience.Skills)}");
        }

        if (experience.Evidence.Count > 0)
        {
            sb.AppendLine("    Evidence:");

            foreach (var item in experience.Evidence)
            {
                sb.AppendLine($"      - {item}");
            }
        }

        if (experience.Contexts.Count > 0)
        {
            sb.AppendLine(
                $"    Contexts: {string.Join(", ", experience.Contexts)}");
        }

        if (!string.IsNullOrWhiteSpace(experience.Notes))
        {
            sb.AppendLine($"    Notes: {experience.Notes}");
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

    private static string FormatEvidence(IEnumerable<Evidence> evidenceReferences)
    {
        var evidence = evidenceReferences.ToList();

        if (evidence.Count == 0)
        {
            return "    - No supporting evidence identified.";
        }

        return string.Empty; /* string.Join(
            Environment.NewLine,
            evidence.Select(e =>
            {
                var experienceIds = e.ExperienceIds.Count > 0
                    ? $" (Experience IDs: {string.Join(", ", e.ExperienceIds)})"
                    : "";

                return $"    - Source: {e.Source}{experienceIds}\n" +
                       $"      Evidence: {e.Evidence}";
            }));*/
    }
}