namespace JobApplicationHelper.Contracts.Experiences;

public sealed record DateRangeDto(
    PartialDateDto? Start,
    PartialDateDto? End);