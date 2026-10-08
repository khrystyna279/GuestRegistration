using Xunit;

// Sequential execution keeps database creation simple on a student's computer.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
