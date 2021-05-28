namespace PerformAnts
{
    // TODO : Add support for multiple simulations in parallel ?
    public interface ISimulationUpdater
    {
        void PlayOneFrame(Ant_Data[] ants, Map map);
        void InitializeAnts(Ant_Data[] ants);
        void InitializeMap(Map map);
    }
}



