using BenchmarkDotNet.Running;
using System.Diagnostics;

namespace Zenserdes.Protobuf.Benchmarks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var b = new PseudoMessageBenchmarks();
			// while(true)b.Deserialize();

			var b2 = new VarIntBenchmarks();

			BenchmarkRunner.Run<VarIntBenchmarks>();
		}
	}
}