using System;
using System.Configuration;

namespace WebApplication1.Utilities
{
    //Ordet static på raden under denna samt innehållet i klassen är tillagt av mig
    public static class ConnectionStringProvider
    {
        // The public property returns the pre-calculated value
        public static string ConnectionString => _connectionString.Value;



        /*
        Understanding Lazy<T>
        Lazy<T> delays the creation of an object until the exact moment your code actually asks for it.

        How Lazy works: It wraps the initialization logic inside a blueprint (a lambda function () => { ... }). The logic inside that blueprint remains asleep.
        The Activation: The moment you call .Value for the first time, Lazy<T> executes the blueprint, saves the result internally, and hands it to you.
        Subsequent Calls: Every time you call .Value after that, it completely skips the blueprint and instantly hands you the saved result from memory.
         
         */
        private static readonly Lazy<string> _connectionString = new Lazy<string>(() =>
        {
            // This logic only runs ONCE, the very first time .Value is accessed
            string name = Default.isLiveDatabase == 1 ? "ConnectionStringProduction" : "ConnectionStringDevelopment";
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString ?? "";


        });
    }
}