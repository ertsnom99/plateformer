public interface IPossessable
{
    // Returns the possession state after calling this method
    bool Possess(Possession possessingScript);
    // Returns the possession state after calling this method
    bool Unpossess();
}
