namespace JobApplicationHelper.Models;

public class RequirementEvidence
{
    private readonly List<Evidence> _evidences = [];

    public bool NoSupportingEvidence { get; set; }

    public IReadOnlyList<Evidence> Evidences => _evidences;

    public bool IsSatisfied => NoSupportingEvidence || Evidences.Count > 0;

    public bool AddEvidence(Evidence evidence)
    {
        if (_evidences.All(e => e.Experience.Id != evidence.Experience.Id))
        {
            _evidences.Add(evidence);
            return true;
        }

        return false;
    }

    public bool RemoveEvidence(Evidence evidence)
    {
        var index = _evidences.FindIndex(ev => ev.Experience.Id == evidence.Experience.Id);

        if (index >= 0) {
            _evidences.RemoveAt(index);
            return true;
        }

        return false;
    }
}
