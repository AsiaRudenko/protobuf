using System;
using System.CodeDom.Compiler;

using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.ZenGen
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