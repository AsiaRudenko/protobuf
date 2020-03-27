﻿using BenchmarkDotNet.Running;

namespace Zenserdes.Protobuf.Benchmarks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var pseudo = new PseudoMessageBenchmarks();
			pseudo.Deserialize();

			var autogen = new AutoGeneratedCodeBenchmark();
			autogen.LongRetention = true;
			autogen.TryMemory();
			autogen.TrySpan();
			autogen.TryStream();
			autogen.Memory();
			autogen.Span();
			autogen.Stream();

			autogen.LongRetention = false;
			autogen.TryMemory();
			autogen.TrySpan();
			autogen.TryStream();
			autogen.Memory();
			autogen.Span();
			autogen.Stream();

			BenchmarkRunner.Run<AutoGeneratedCodeBenchmark>();
		}
	}
}