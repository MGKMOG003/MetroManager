using MetroManager.Application.Services;

namespace MetroManager.Application.Factories
{
    public static class UserFactory
    {
        public static UserLite CreateAnonymousHolder()
        {
            return new UserLite
            {
                Id = null, // unknown in Application layer
                UserName = "anonymous@metro.local",
                Email = "anonymous@metro.local"
            };
        }
    }
}
