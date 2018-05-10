namespace ClusterSim.ClusterLib.Calculation
{
    public interface IMassive
    {
        Vector pos { get; set; }
        double mass { get; set; }
        int id { get; set; }
        bool dead { get; set; }
    }
}
