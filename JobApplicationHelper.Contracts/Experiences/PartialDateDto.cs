namespace JobApplicationHelper.Contracts.Experiences;

public sealed record PartialDateDto(
    int Year,
    int? Month,
    int? Day);