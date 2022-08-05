using System;

namespace MyClassLibrary
{
    public class AccountElapsedEventArgs : EventArgs
    {
        public AccountElapsedEventArgs(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; private set; }
    }
}