namespace MyClassLibrary
{
    internal interface IAccountLocker
    {
        bool IsLocked(string userName);
        void Lock(string userName);

        event LockTimeoutElapsedHandler LockTimeoutElapsedEvent;
    }
}