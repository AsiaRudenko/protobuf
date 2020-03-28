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

			var autogen2 = new AutoGeneratedCode2VarintPatchBenchmarks();
			autogen2.LongRetention = true;
			autogen2.TryMemory();
			autogen2.TrySpan();
			autogen2.TryStream();
			autogen2.Memory();
			autogen2.Span();
			autogen2.Stream();

			autogen2.LongRetention = false;
			autogen2.TryMemory();
			autogen2.TrySpan();
			autogen2.TryStream();
			autogen2.Memory();
			autogen2.Span();
			autogen2.Stream();

			BenchmarkRunner.Run<AutoGeneratedCode2VarintPatchBenchmarks>();
		}
	}
}