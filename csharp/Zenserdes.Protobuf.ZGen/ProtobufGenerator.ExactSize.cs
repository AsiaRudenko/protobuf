using System;
using System.CodeDom.Compiler;

using Zenserdes.Protobuf.ZGen.Models;

namespace Zenserdes.Protobuf.ZGen
{
	public static partial class ProtobufGenerator
	{
		public class ExactSize
		{
			private IndentedTextWriter _writer;

			public ExactSize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(ZMessage message, string fullyQualifiedMessageName, string parameter)
			{
				_writer.WriteMethod(typeof(ulong), typeof(IMessageOperator<>), new string[] { fullyQualifiedMessageName }, nameof(IMessageAndOperator<IWantToUseNameof>.ExactSize), () =>
				{
					_writer.WriteLine("throw new " + typeof(NotImplementedException).FullyQualified() + "();");
				}, parameter);
			}
		}
	}
}