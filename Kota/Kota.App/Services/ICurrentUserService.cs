using Kota.Domain.Entities;

namespace Kota.App.Services
{
    public interface ICurrentUserService
    {
        User? CurrentUser { get; }
        void SetCurrentUser(User user);
        void ClearCurrentUser();
        bool IsAdmin { get; }
        bool IsLoggedIn { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        public User? CurrentUser { get; private set; }

        public bool IsAdmin => CurrentUser?.Role == "admin";
        public bool IsLoggedIn => CurrentUser != null;

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
        }
    }
}