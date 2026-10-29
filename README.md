
# JobApplicationHelper

**A Windows desktop application for organizing job applications, connecting job requirements to real experience, and preparing candidate-reviewed cover letter drafts with a locally hosted LLM.**

> **Project status:** Active personal project, developed and run from Visual Studio. This repository showcases an evolving application and its engineering decisions; it is not a packaged product or a service intended for general use.

## Why I built it

JobApplicationHelper began as a way to organize the documents and information involved in applying for jobs. As the workflow grew, I started automating repetitive preparation tasks and using the project to explore technologies I wanted to learn more deeply, particularly **.NET Aspire** and **locally hosted large language models**.

The application is designed around a distinction that matters to me: **the candidate is the author of their application**. An LLM can help extract requirements, organize evidence, and draft text, but it does not know the candidate better than the candidate does. Generated cover letters are **starting points for review and rewriting**, not finished documents. The application will **never automatically submit job applications**.

## What it does

The workflow brings together job opportunities, a reusable record of experience, and application-specific preparation:

1. **Capture an opportunity** with its job posting, company, role, location, and priority.
2. **Track applications** in a searchable, paginated list and open a dedicated workspace for each one.
3. **Extract job requirements** from the posting using an LLM.
4. **Review and save requirements**, associating them with relevant entries from an Experience Bank and adding notes about why each experience is evidence.
5. **Prepare cover letter drafts** using candidate-provided information and selected evidence, with a separate verification process to check claims against the source material.
6. **Manage background work** while continuing to use the desktop application.

The goal is to make preparation more organized and deliberate, not to replace the candidate's judgment.

## Implemented features

- **Job application management:** capture, browse, filter, paginate, prioritize, and open individual applications.
- **Experience Bank:** maintain structured records of professional experience, projects, education, and other relevant evidence.
- **LLM-assisted requirement extraction:** identify and categorize requirements from job postings.
- **Candidate-controlled evidence matching:** associate requirements with Experience Bank entries, add explanatory notes, and explicitly identify requirements without supporting evidence.
- **Persisted requirements:** distinguish generated extraction results from the requirements and evidence reviewed and saved for an application.
- **Background-job processing:** persist work, manage priorities and lifecycle states, and coordinate asynchronous preparation tasks.
- **Desktop job notifications:** use SignalR to reflect background-job progress and completion without silently overwriting work in progress.
- **Cover letter generation and verification services:** use candidate information, job requirements, and selected evidence to draft and assess text. These workflows are being integrated with the newer persistence model.
- **Automated testing:** unit, database integration, and API integration tests, including PostgreSQL-backed tests using Testcontainers.

## In progress

- **Cover letter workspace:** completing the separation of saved letters, generated drafts, generation settings, and verification results, with explicit review and replacement workflows.
- **Generation inputs:** refactoring generation and verification scheduling to use persisted, application-specific settings and saved requirements rather than sending the complete input model from the WPF client.
- **Settings window:** expanding the existing settings experience, which currently provides access to the Experience Bank.
- **Configuration cleanup:** moving user-maintained data such as supported locations and document-template paths out of `appsettings.json` into application-managed settings.

## Planned improvements

- **Move user-maintained data out of backend configuration.** Replace the current `appsettings.json` entries for locations, candidate content paths, and document templates with application-managed settings, separating personal data from runtime configuration.
- **Add a React frontend orchestrated by Aspire.** Explore a browser-based interface alongside the existing WPF application, with the React development environment starting as part of the Aspire application.
- **Introduce Experience Profiles.** Allow the candidate to define subsets of the full Experience Bank tailored to different types of roles, such as backend development, full-stack development, or DevOps engineering. These profiles would help focus evidence selection without duplicating the underlying experience records.
- **Support interview preparation.** Generate example interview questions informed by the job posting and the interviewer's role, helping the candidate prepare for different interview rounds. As with cover letters, these would be preparation aids rather than substitutes for the candidate's own answers.

**Out of scope by design:** automatic submission of application materials or job applications.

## Technical overview

| Area | Technologies and approach |
| --- | --- |
| Desktop UI | WPF, XAML, CommunityToolkit.Mvvm, MVVM |
| Backend | .NET, ASP.NET Core Minimal APIs |
| Orchestration | .NET Aspire AppHost |
| Data | PostgreSQL, Entity Framework Core, migrations |
| Asynchronous work | Persisted background jobs, priority scheduling, background workers |
| Live updates | SignalR |
| AI integration | Microsoft.Extensions.AI (`IChatClient`), OpenAI-compatible local LLM endpoint |
| Testing | xUnit, Testcontainers for PostgreSQL, API integration tests |
| Development environment | Windows, Visual Studio |

### Architecture

```text
┌───────────────────────────┐
│ WPF desktop application   │
│ Views / MVVM ViewModels   │
└─────────────┬─────────────┘
              │ HTTP APIs / SignalR
              ▼
┌───────────────────────────┐
│ ASP.NET Core backend      │
│ Endpoints and contracts   │
│ Application services      │
│ Domain models             │
└───────┬───────────┬───────┘
        │           │
        ▼           ▼
  PostgreSQL    Background jobs
                    │
                    ▼
              Local LLM service
              (OpenAI-compatible)

       Aspire AppHost orchestrates
       backend infrastructure
```

The application separates HTTP contracts, application services, domain models, and persistence concerns. The WPF client uses view models and API clients rather than directly accessing the database. Long-running LLM work is handled asynchronously so the UI can remain responsive.

### Engineering decisions worth exploring

**Human-reviewed evidence.** Requirements and their supporting experience are explicit, editable application data. The generation prompts are designed to avoid inventing qualifications or presenting personal learning as professional employment.

**Durable background processing.** Work is persisted and assigned priorities rather than being tied to a single HTTP request. The desktop client observes status changes through SignalR, while coordinating refreshes with unsaved edits.

**Separate generated and saved material.** Generated content is not treated as authoritative user content. The evolving workflow preserves a distinction between machine-produced drafts and candidate-reviewed, saved work.

**Local-first AI integration.** The backend communicates through `Microsoft.Extensions.AI` with an OpenAI-compatible local server. This keeps model interaction behind an abstraction while allowing experimentation with locally hosted models. Model behavior and output quality still require human review.

**Integration testing against real infrastructure.** Database and API integration tests exercise PostgreSQL using disposable Testcontainers instances, including persistence and endpoint behavior.

## Running from source

This is a development-oriented setup for **Windows and Visual Studio**, not an installer-based distribution.

### Prerequisites

- Windows with Visual Studio and the .NET SDK/workloads required by the solution.
- A container runtime compatible with .NET Aspire's PostgreSQL container (for example, Docker Desktop), running before launch.
- An **OpenAI-compatible local LLM server**, started separately and serving the model configured below.
- Candidate CV text and document templates at paths you configure for your machine.

The precise .NET SDK version and package requirements should be taken from the solution and project files. Local LLM model names, hardware needs, and startup commands depend on the server you choose.

### First launch

1. Clone the repository and open the solution in Visual Studio.
2. Configure the backend and WPF `appsettings.json` files as described below.
3. Start your local LLM server and make sure its OpenAI-compatible endpoint is reachable.
4. **Start only the Aspire AppHost project on the first run.** Aspire will provision the PostgreSQL container; the initial image download may take some time.
5. Note the **actual backend API endpoint URL** shown during the AppHost run. Use this URL in the WPF project's `Api:BaseUrl` setting.
6. Stop the initial run once the backend and database have started successfully.

### Normal development launch

In Visual Studio, configure **multiple startup projects** so that both the **AppHost** and **WPF** projects start together. The local LLM server must also be running. The WPF client communicates with the API URL configured in its own `appsettings.json`.

## Configuration

The backend configuration currently contains both runtime settings and user-maintained data. This is temporary: locations and document-template configuration are planned to move into application-managed settings.

The following example is **anonymized**. Replace paths, candidate information, model identifier, and endpoint addresses with values for your environment. The country entries are illustrative and can be extended to match the locations and templates you use.

### Backend `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aspire.Hosting.Dcp": "Warning"
    }
  },
  "Locations": [
    { "CountryCode": "US", "CountryName": "United States" },
    { "CountryCode": "DE", "CountryName": "Germany" },
    { "CountryCode": "NL", "CountryName": "Netherlands" }
  ],
  "CandidateContent": {
    "CandidateFullName": "Example Candidate",
    "CvTextPathsByCountryCode": [
      { "CountryCode": "US", "Path": "candidate_cv.txt" },
      { "CountryCode": "DE", "Path": "candidate_cv.txt" },
      { "CountryCode": "NL", "Path": "candidate_cv.txt" }
    ]
  },
  "ApplicationDocuments": {
    "TemplateBasePath": "C:\\JobApplicationHelper\\Templates",
    "ApplicationsBasePath": "C:\\JobApplicationHelper\\Applications",
    "Documents": [
      {
        "CountryCode": "US",
        "CvTemplate": "cv_template.odt",
        "CoverLetterTemplate": "cover_letter_template.odt"
      },
      {
        "CountryCode": "DE",
        "CvTemplate": "cv_template.odt",
        "CoverLetterTemplate": "cover_letter_template.odt"
      },
      {
        "CountryCode": "NL",
        "CvTemplate": "cv_template.odt",
        "CoverLetterTemplate": "cover_letter_template.odt"
      }
    ]
  },
  "BackgroundJobs": {
    "MaxConcurrency": 4
  },
  "LLM": {
    "Model": "your-local-model-id",
    "Endpoint": "http://localhost:9931/v1"
  }
}
```

**Configuration fields**

| Section | Purpose |
| --- | --- |
| `Locations` | Country codes and display names available to the application. |
| `CandidateContent:CandidateFullName` | Candidate name used in application materials. |
| `CandidateContent:CvTextPathsByCountryCode` | CV text file to use for each configured country. Files must exist and be accessible to the backend. |
| `ApplicationDocuments:TemplateBasePath` | Directory containing document templates. |
| `ApplicationDocuments:ApplicationsBasePath` | Destination directory for application documents. |
| `ApplicationDocuments:Documents` | Country-specific CV and cover letter template filenames. |
| `BackgroundJobs:MaxConcurrency` | Configured maximum background-job concurrency. |
| `LLM:Model` | Model identifier expected by the local LLM endpoint. |
| `LLM:Endpoint` | Base URL of the OpenAI-compatible local LLM API. |

**Important:** Keep country codes consistent across `Locations`, CV mappings, and document-template mappings. Create the referenced files and directories before using workflows that depend on them. The local paths and ports above are examples, not guaranteed defaults for another machine.

### WPF `appsettings.json`

The desktop application has its own API configuration:

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5262/"
  }
}
```

Replace `BaseUrl` with the backend URL observed when you first launch AppHost. Do not assume that the example port will be the one assigned on your machine.

### Personal data

The project is intended for personal use. Candidate CVs, application documents, and experience records may contain private information. Use your own local files, avoid committing real candidate materials or machine-specific configuration to a public repository, and review any configuration changes before pushing.

## Testing

The solution includes unit tests and PostgreSQL-backed integration tests, including API integration tests. Integration tests use **Testcontainers**, so a working container runtime is required. Run the test projects from Visual Studio's Test Explorer or with the .NET CLI, using the solution's configured test projects.

## Scope and limitations

- The application is actively evolving; some backend capabilities are ahead of their corresponding WPF workflows.
- Setup is currently optimized for development from Visual Studio, not one-click installation.
- A compatible local LLM server and candidate-specific local files must be configured separately.
- LLM output may be incomplete or inaccurate and **must be reviewed and revised by the candidate**.
- No automatic job application submission is implemented or planned.

## Project intent

JobApplicationHelper is a personal tool and a practical learning project. It gives me a place to refine familiar engineering practices while exploring new tools and architectural patterns. It is published primarily to demonstrate the code, design decisions, and ongoing development process, rather than to solicit users or contributors.

