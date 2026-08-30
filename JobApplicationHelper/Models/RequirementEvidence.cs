namespace JobApplicationHelper.Models;

public class RequirementEvidence
{
    private readonly List<Evidence> _evidences = [];

    public bool NoSupportingEvidence { get; set; }

    public IReadOnlyList<Evidence> Evidences => _evidences;

    public bool IsSatisfied => NoSupportingEvidence || Evidences.Count > 0;

    public void AddEvidence(Evidence evidence)
    {
        if (_evidences.All(e => e != evidence))
        {
            _evidences.Add(evidence);
        }
    }
}
