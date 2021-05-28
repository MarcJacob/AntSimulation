namespace PerformAnts
{
    public interface ISimulationRenderer
    {
        void UpdateRenderer(Ant_Data[] ants, Map map);
        void Initialize(Map map);
    }
}



