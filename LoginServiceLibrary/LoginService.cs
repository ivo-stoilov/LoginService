using System.Collections.Generic;

namespace MyClassLibrary
{
    class LoginService
    {
        readonly IAccountLocker accountLocker;
        readonly ICredentialManager cretentialManager;
        readonly Dictionary<string, int> accoutFailureAttempts = new Dictionary<string, int>();

        public LoginService(ICredentialManager cretentialManager, IAccountLocker accountLocker)
        {
            this.cretentialManager = cretentialManager;
            this.accountLocker = accountLocker;
            this.accountLocker.LockTimeoutElapsedEvent += AccountLocker_LockTimeoutElapsedEvent;
        }

        private void AccountLocker_LockTimeoutElapsedEvent(object sender, AccountElapsedEventArgs e)
        {
            this.accoutFailureAttempts.Remove(e.UserName);
        }

        public bool Login(string userName, string password)
        {
            if (this.accountLocker.IsLocked(userName))
            {
                return false;
            }

            if (!this.cretentialManager.CheckCredentials(userName, password))
            {
                int accoutFailureCount = 0;
                this.accoutFailureAttempts.TryGetValue(userName, out accoutFailureCount);
                this.accoutFailureAttempts[userName] = ++accoutFailureCount;
                if (accoutFailureCount == 3)
                {
                    this.accountLocker.Lock(userName);
                }
                return false;
            }

            if (this.accoutFailureAttempts.ContainsKey(userName))
            {
                this.accoutFailureAttempts.Remove(userName);
            }

            return true;
        }
    }
}
