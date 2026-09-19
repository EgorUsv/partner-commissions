namespace PartnerCommissions.Commissions.Api.Hosting;

public interface ICommissionsMetrics
{
    void EventAccepted(int accrued);
}
