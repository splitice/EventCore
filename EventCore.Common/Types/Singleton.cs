namespace EventCore.Common.Types
{
    /// <summary>
    /// A simple singleton class
    /// </summary>
    /// <typeparam name="T">Singleton class</typeparam>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        /// <summary>
        /// Prevent instansiation
        /// </summary>
        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                return _instance;
            }
        }
    }
}