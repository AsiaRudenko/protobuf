using BenchmarkDotNet.Running;

namespace Zenserdes.Protobuf.Benchmarks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var b = new PseudoMessageBenchmarks();
			// while (true) b.Deserialize();
			b.Deserialize();
			BenchmarkRunner.Run<PseudoMessageBenchmarks>();
		}
	}
}